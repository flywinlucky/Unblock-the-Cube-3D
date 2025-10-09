using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class LevelEditorWindow : EditorWindow
{
    // --- Referințe ---
    private LevelData _currentLevel;
    private Editor _levelDataEditor;
    private GameObject _blockPrefab;

    // --- Variabile pentru Preview 3D ---
    private PreviewRenderUtility _previewRenderUtility;
    private GameObject _previewRoot;
    private Vector2 _previewRotation = new Vector2(150, -30);
    private float _previewZoom = 5f;
    private const float MIN_ZOOM = 2f;
    private const float MAX_ZOOM = 20f;

    // --- Stocare persistentă ---
    private static string _blockPrefabPathKey = "LevelEditor_BlockPrefabPath";

    // --- Stilizare ---
    private GUIStyle _previewBoxStyle;

    [MenuItem("Tools/Unblock Cube/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    #region Ciclul de Viață al Ferestrei

    private void OnEnable()
    {
        _previewRenderUtility = new PreviewRenderUtility();

        _previewRenderUtility.camera.fieldOfView = 30f;
        // Poziția și rotația inițială vor fi calculate dinamic în DrawPreviewPanel

        // Lumină principală (Key Light)
        Light mainLight = new GameObject("MainLight").AddComponent<Light>();
        mainLight.type = LightType.Directional;
        mainLight.intensity = 1.2f;
        mainLight.transform.rotation = Quaternion.Euler(50, -30, 0);
        _previewRenderUtility.AddSingleGO(mainLight.gameObject);

        // ▼▼▼ CORECTAT: Lumină de umplere (Fill Light), nu ambientală ▼▼▼
        Light fillLight = new GameObject("FillLight").AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.intensity = 0.4f;
        fillLight.transform.rotation = Quaternion.Euler(-30, 60, 0);
        _previewRenderUtility.AddSingleGO(fillLight.gameObject);

        _previewRoot = new GameObject("PreviewRoot");
        _previewRenderUtility.AddSingleGO(_previewRoot);

        _previewBoxStyle = new GUIStyle("box");
        _previewBoxStyle.padding = new RectOffset(2, 2, 2, 2);

        // Încărcăm calea prefab-ului salvată anterior
        string prefabPath = EditorPrefs.GetString(_blockPrefabPathKey, "");
        if (!string.IsNullOrEmpty(prefabPath))
        {
            _blockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }
    }

    private void OnDisable()
    {
        _previewRenderUtility.Cleanup();
        _previewRenderUtility = null;
    }

    #endregion

    private void OnGUI()
    {
        DrawToolbar();
        EditorGUILayout.BeginHorizontal();
        DrawPropertiesPanel();
        DrawPreviewPanel();
        EditorGUILayout.EndHorizontal();
    }

    #region Desenare UI

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("New Level", EditorStyles.toolbarButton)) CreateNewLevelAsset();
        if (GUILayout.Button("Save Level", EditorStyles.toolbarButton)) SaveChanges();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Rebuild Preview", EditorStyles.toolbarButton))
        {
            if (_currentLevel != null) RebuildPreview();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawPropertiesPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(300));

        EditorGUILayout.LabelField("Editor Settings", EditorStyles.boldLabel);
        GameObject newPrefab = (GameObject)EditorGUILayout.ObjectField("Block Prefab", _blockPrefab, typeof(GameObject), false);
        if (newPrefab != _blockPrefab)
        {
            _blockPrefab = newPrefab;
            string path = _blockPrefab != null ? AssetDatabase.GetAssetPath(_blockPrefab) : "";
            EditorPrefs.SetString(_blockPrefabPathKey, path);
        }
        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Level Properties", EditorStyles.boldLabel);

        LevelData newLevel = (LevelData)EditorGUILayout.ObjectField("Current Level", _currentLevel, typeof(LevelData), false);
        if (newLevel != _currentLevel)
        {
            _currentLevel = newLevel;
            RebuildPreview();
        }

        if (_currentLevel != null)
        {
            if (_levelDataEditor == null || _levelDataEditor.target != _currentLevel)
            {
                _levelDataEditor = Editor.CreateEditor(_currentLevel);
            }
            _levelDataEditor.OnInspectorGUI();

            EditorGUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(_blockPrefab == null);
            if (GUILayout.Button("Generate Level", GUILayout.Height(30)))
            {
                _currentLevel.Generate();
                RebuildPreview();
                SaveChanges();
                Debug.Log("Level generated and preview updated!");
            }
            EditorGUI.EndDisabledGroup();

            if (_blockPrefab == null)
            {
                EditorGUILayout.HelpBox("Assign a Block Prefab to enable generation.", MessageType.Warning);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No level loaded. Create a 'New Level' or assign one.", MessageType.Info);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawPreviewPanel()
    {
        EditorGUILayout.BeginVertical(_previewBoxStyle);
        Rect previewRect = GUILayoutUtility.GetRect(200, 200, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        if (Event.current.type == EventType.Repaint)
        {
            _previewRenderUtility.BeginPreview(previewRect, GUIStyle.none);

            // ▼▼▼ MODIFICARE CHEIE: Logica de Orbit Camera ▼▼▼
            if (_previewRoot != null)
            {
                // 1. Calculăm rotația camerei pe baza input-ului
                Quaternion rotation = Quaternion.Euler(_previewRotation.y, _previewRotation.x, 0);

                // 2. Calculăm poziția camerei: un punct pe o sferă în jurul centrului, la distanța de zoom
                Vector3 center = _previewRoot.transform.position;
                Vector3 camPosition = center + (rotation * Vector3.forward * -_previewZoom);

                // 3. Aplicăm poziția și rotația DOAR camerei
                _previewRenderUtility.camera.transform.position = camPosition;
                _previewRenderUtility.camera.transform.rotation = rotation;

                // 4. Randăm scena (obiectul _previewRoot rămâne nemișcat)
                _previewRenderUtility.Render();
            }

            Texture result = _previewRenderUtility.EndPreview();
            GUI.DrawTexture(previewRect, result, ScaleMode.StretchToFill, false);
        }

        HandlePreviewInput(previewRect);
        EditorGUILayout.EndVertical();
    }

    #endregion

    #region Logică Tool

    private void HandlePreviewInput(Rect previewRect)
    {
        Event e = Event.current;
        if (previewRect.Contains(e.mousePosition))
        {
            if (e.type == EventType.MouseDrag && e.button == 0)
            {
                _previewRotation.x += e.delta.x * 0.5f;
                _previewRotation.y += e.delta.y * 0.5f;
                _previewRotation.y = Mathf.Clamp(_previewRotation.y, -80, 80); // Limităm rotația verticală
                e.Use();
                Repaint();
            }

            if (e.type == EventType.ScrollWheel)
            {
                _previewZoom -= e.delta.y * 0.1f;
                _previewZoom = Mathf.Clamp(_previewZoom, MIN_ZOOM, MAX_ZOOM);
                e.Use();
                Repaint();
            }
        }
    }

    private void RebuildPreview()
    {
        if (_previewRoot == null) return;

        while (_previewRoot.transform.childCount > 0)
        {
            DestroyImmediate(_previewRoot.transform.GetChild(0).gameObject);
        }

        if (_currentLevel == null || _blockPrefab == null)
        {
            Repaint();
            return;
        }

        foreach (BlockData blockData in _currentLevel.GetBlocks())
        {
            GameObject blockInstance = (GameObject)PrefabUtility.InstantiatePrefab(_blockPrefab, _previewRoot.transform);
            blockInstance.transform.position = (Vector3)blockData.position * 0.5f;
            blockInstance.transform.rotation = GetStableLookRotation(blockData.direction);
        }

        Repaint();
    }

    private void SaveChanges()
    {
        if (_currentLevel != null)
        {
            EditorUtility.SetDirty(_currentLevel);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private void CreateNewLevelAsset()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create New Level Asset", "NewLevelData.asset", "asset", "Please enter a file name.");
        if (string.IsNullOrEmpty(path)) return;

        LevelData newLevel = ScriptableObject.CreateInstance<LevelData>();
        AssetDatabase.CreateAsset(newLevel, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        _currentLevel = newLevel;
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newLevel;
        RebuildPreview();
    }

    #endregion

    #region Funcții Ajutătoare (Helpers)

    private Quaternion GetStableLookRotation(MoveDirection dir)
    {
        Vector3 directionVector = GetDirectionVector(dir);
        if (directionVector == Vector3.zero) return Quaternion.identity;
        Vector3 upReference = (dir == MoveDirection.Up || dir == MoveDirection.Down) ? Vector3.forward : Vector3.up;
        return Quaternion.LookRotation(directionVector, upReference);
    }

    private Vector3 GetDirectionVector(MoveDirection dir)
    {
        switch (dir)
        {
            case MoveDirection.Forward: return Vector3.forward;
            case MoveDirection.Back: return Vector3.back;
            case MoveDirection.Up: return Vector3.up;
            case MoveDirection.Down: return Vector3.down;
            case MoveDirection.Left: return Vector3.left;
            case MoveDirection.Right: return Vector3.right;
        }
        return Vector3.forward;
    }

    #endregion
}

