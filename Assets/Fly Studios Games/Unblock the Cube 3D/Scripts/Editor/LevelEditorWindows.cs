using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class LevelEditorWindow : EditorWindow
{
    // --- Referințe ---
    private LevelData _currentLevel;
    private GameObject _blockPrefab;
    private Editor _levelDataEditor;

    // --- Variabile pentru Editare ---
    private GameObject _selectedBlockInstance;
    private int _selectedBlockIndex = -1;

    // --- Variabile pentru Preview 3D ---
    private PreviewRenderUtility _previewRenderUtility;
    private GameObject _previewRoot;
    private Vector2 _previewRotation = new Vector2(150, -30);
    private float _previewZoom = 10f;
    private const float MIN_ZOOM = 2f;
    private const float MAX_ZOOM = 40f;

    // --- Stocare persistentă ---
    private static string _blockPrefabPathKey = "LevelEditor_BlockPrefabPath";

    // --- Stilizare ---
    private GUIStyle _previewBoxStyle;

    [MenuItem("Tools/Unblock Cube/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    #region Ciclul de Viață al Ferstrei

    private void OnEnable()
    {
        // Inițializăm tot ce este necesar pentru preview
        _previewRenderUtility = new PreviewRenderUtility();

        _previewRenderUtility.camera.fieldOfView = 30f;
        _previewRenderUtility.camera.nearClipPlane = 0.1f;
        _previewRenderUtility.camera.farClipPlane = 100f;

        // Setup lumini
        Light mainLight = new GameObject("MainLight").AddComponent<Light>();
        mainLight.type = LightType.Directional;
        mainLight.intensity = 1.5f;
        mainLight.transform.rotation = Quaternion.Euler(50, -30, 0);
        _previewRenderUtility.AddSingleGO(mainLight.gameObject);

        Light fillLight = new GameObject("FillLight").AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.intensity = 0.7f;
        fillLight.color = Color.grey;
        fillLight.transform.rotation = Quaternion.Euler(-30, 60, 0);
        _previewRenderUtility.AddSingleGO(fillLight.gameObject);

        _previewRoot = new GameObject("PreviewRoot");
        _previewRenderUtility.AddSingleGO(_previewRoot);

        _previewBoxStyle = new GUIStyle("box");
        _previewBoxStyle.padding = new RectOffset(2, 2, 2, 2);

        // Încărcăm prefab-ul salvat
        string prefabPath = EditorPrefs.GetString(_blockPrefabPathKey, "");
        if (!string.IsNullOrEmpty(prefabPath))
        {
            _blockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }
    }

    private void OnDisable()
    {
        // Curățăm resursele pentru a evita memory leaks
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

        // Forțăm redesenarea dacă un obiect este selectat pentru a menține uneltele vizibile
        if (_selectedBlockInstance != null)
        {
            Repaint();
        }
    }

    #region Desenare UI
    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("New Level", EditorStyles.toolbarButton)) CreateNewLevelAsset();
        if (GUILayout.Button("Save Changes", EditorStyles.toolbarButton)) SaveChanges();
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
            DeselectBlock();
            RebuildPreview();
        }

        if (_currentLevel != null)
        {
            if (_levelDataEditor == null || _levelDataEditor.target != _currentLevel)
            {
                _levelDataEditor = Editor.CreateEditor(_currentLevel, typeof(CleanLevelDataEditor));
            }
            _levelDataEditor.OnInspectorGUI();

            EditorGUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(_blockPrefab == null);
            if (GUILayout.Button("Generate New Level", GUILayout.Height(30)))
            {
                _currentLevel.Generate();
                DeselectBlock();
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
        // Obținem dreptunghiul unde va fi desenat preview-ul
        Rect previewRect = GUILayoutUtility.GetRect(200, 200, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        // Randăm scena de preview
        if (Event.current.type == EventType.Repaint)
        {
            _previewRenderUtility.BeginPreview(previewRect, GUIStyle.none);

            Quaternion rotation = Quaternion.Euler(_previewRotation.y, _previewRotation.x, 0);
            Vector3 center = _previewRoot != null ? _previewRoot.transform.position : Vector3.zero;
            Vector3 camPosition = center + (rotation * Vector3.forward * -_previewZoom);

            _previewRenderUtility.camera.transform.position = camPosition;
            _previewRenderUtility.camera.transform.rotation = rotation;

            _previewRenderUtility.Render();

            Texture result = _previewRenderUtility.EndPreview();
            GUI.DrawTexture(previewRect, result, ScaleMode.StretchToFill, false);
        }

        // Desenăm uneltele deasupra preview-ului
        DrawEditingGizmos(previewRect);

        // Gestionăm input-ul de la mouse
        HandlePreviewInput(previewRect);
        EditorGUILayout.EndVertical();
    }

    #endregion

    #region Logică Tool

    private void HandlePreviewInput(Rect previewRect)
    {
        Event e = Event.current;
        if (!previewRect.Contains(e.mousePosition)) return;

        // Rotația camerei cu CLICK-DREAPTA
        if (e.type == EventType.MouseDrag && e.button == 1)
        {
            _previewRotation.x += e.delta.x * 0.5f;
            _previewRotation.y += e.delta.y * 0.5f;
            _previewRotation.y = Mathf.Clamp(_previewRotation.y, -80, 80);
            e.Use();
            Repaint();
        }

        // Zoom cu scroll
        if (e.type == EventType.ScrollWheel)
        {
            _previewZoom -= e.delta.y * 0.2f;
            _previewZoom = Mathf.Clamp(_previewZoom, MIN_ZOOM, MAX_ZOOM);
            e.Use();
            Repaint();
        }

        // Selecția cu CLICK-STÂNGA
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            HandleSelection(e.mousePosition, previewRect);
            e.Use();
        }
    }

    private void DrawEditingGizmos(Rect previewRect)
    {
        if (_selectedBlockInstance == null || _selectedBlockIndex == -1) return;

        // Setăm contextul corect pentru desenare
        Handles.SetCamera(_previewRenderUtility.camera);

        // Desenăm în interiorul chenarului de preview
        GUI.BeginClip(previewRect);
        Handles.BeginGUI();

        Renderer renderer = _selectedBlockInstance.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            // Desenăm conturul galben
            Bounds bounds = renderer.bounds;
            Handles.color = Color.yellow;
            Handles.DrawWireCube(bounds.center, bounds.size * 1.05f);

            // Calculăm poziția butoanelor pe ecran
            Vector3 screenPos = _previewRenderUtility.camera.WorldToScreenPoint(bounds.center);

            // Desenăm doar dacă obiectul nu e în spatele camerei
            if (screenPos.z > 0)
            {
                // Inversăm coordonata Y pentru a se potrivi cu sistemul GUI
                screenPos.y = previewRect.height - screenPos.y;

                float buttonSize = 25;
                float spacing = 5;

                if (GUI.Button(new Rect(screenPos.x - buttonSize / 2, screenPos.y - 45, buttonSize, buttonSize), "X"))
                {
                    DeleteSelectedBlock();
                }

                if (GUI.Button(new Rect(screenPos.x + spacing, screenPos.y - buttonSize / 2, buttonSize, buttonSize), ">"))
                {
                    RotateSelectedBlock(Vector3.up);
                }
                if (GUI.Button(new Rect(screenPos.x - buttonSize - spacing, screenPos.y - buttonSize / 2, buttonSize, buttonSize), "<"))
                {
                    RotateSelectedBlock(Vector3.down);
                }
            }
        }

        Handles.EndGUI();
        GUI.EndClip();
    }

    // ▼▼▼ FUNCȚIE CRITICĂ REPARATĂ ▼▼▼
    private void HandleSelection(Vector2 mousePosition, Rect previewRect)
    {
        // 1. Calculăm poziția mouse-ului relativ la chenarul de preview
        Vector2 localMousePos = mousePosition - previewRect.position;

        Ray ray = _previewRenderUtility.camera.ScreenPointToRay(localMousePos);

        RaycastHit hit;
        // Folosim un layer mask pentru a ne asigura că lovim doar obiectele din preview
        // Toate obiectele din PreviewRenderUtility sunt pe un layer ascuns
        if (Physics.Raycast(ray, out hit, 200f))
        {
            SelectBlock(hit.collider.gameObject);
        }
        else
        {
            DeselectBlock();
        }
        Repaint();
    }

    private void SelectBlock(GameObject instance)
    {
        _selectedBlockInstance = instance;
        _selectedBlockIndex = -1; // Resetăm înainte de a căuta

        // Găsim indexul căutând instanța în ierarhia de preview
        for (int i = 0; i < _previewRoot.transform.childCount; i++)
        {
            if (_previewRoot.transform.GetChild(i).gameObject == instance)
            {
                _selectedBlockIndex = i;
                break;
            }
        }
    }

    private void DeselectBlock()
    {
        _selectedBlockInstance = null;
        _selectedBlockIndex = -1;
    }

    private void RotateSelectedBlock(Vector3 axis)
    {
        if (_selectedBlockIndex == -1 || _currentLevel == null || _selectedBlockIndex >= _currentLevel.GetBlocks().Count) return;

        BlockData data = _currentLevel.GetBlocks()[_selectedBlockIndex];

        Quaternion currentRotation = GetStableLookRotation(data.direction);
        Quaternion ninetyDegreesTurn = Quaternion.AngleAxis(90, axis);
        Vector3 newDirectionVector = ninetyDegreesTurn * currentRotation * Vector3.forward;

        data.direction = GetEnumFromVector(newDirectionVector.normalized);

        SaveChanges();
        RebuildPreview();
    }

    private void DeleteSelectedBlock()
    {
        if (_selectedBlockIndex == -1 || _currentLevel == null || _selectedBlockIndex >= _currentLevel.GetBlocks().Count) return;

        _currentLevel.GetBlocks().RemoveAt(_selectedBlockIndex);

        DeselectBlock();
        SaveChanges();
        RebuildPreview();
    }

    private void RebuildPreview()
    {
        if (_previewRoot == null) return;

        var oldSelectionIndex = _selectedBlockIndex;
        DeselectBlock();

        while (_previewRoot.transform.childCount > 0)
        {
            DestroyImmediate(_previewRoot.transform.GetChild(0).gameObject);
        }

        if (_currentLevel == null || _blockPrefab == null)
        {
            Repaint();
            return;
        }

        for (int i = 0; i < _currentLevel.GetBlocks().Count; i++)
        {
            BlockData blockData = _currentLevel.GetBlocks()[i];
            GameObject blockInstance = (GameObject)PrefabUtility.InstantiatePrefab(_blockPrefab, _previewRoot.transform);
            blockInstance.transform.position = (Vector3)blockData.position * 0.5f;
            blockInstance.transform.rotation = GetStableLookRotation(blockData.direction);

            // ▼▼▼ CORECTAT: Ne asigurăm că există un collider și este activ ▼▼▼
            Collider col = blockInstance.GetComponentInChildren<Collider>(true); // true = include copiii inactivi
            if (col == null)
            {
                col = blockInstance.AddComponent<BoxCollider>();
            }
            col.enabled = true;
        }

        // Reselectăm blocul dacă încă există în listă
        if (oldSelectionIndex != -1 && oldSelectionIndex < _previewRoot.transform.childCount)
        {
            SelectBlock(_previewRoot.transform.GetChild(oldSelectionIndex).gameObject);
        }

        Repaint();
    }

    private void SaveChanges()
    {
        if (_currentLevel != null)
        {
            EditorUtility.SetDirty(_currentLevel);
            AssetDatabase.SaveAssets();
        }
    }

    private void CreateNewLevelAsset()
    {
        string path = EditorUtility.SaveFilePanelInProject("Create New Level Asset", "NewLevelData.asset", "asset", "Please enter a file name.");
        if (string.IsNullOrEmpty(path)) return;

        LevelData newLevel = ScriptableObject.CreateInstance<LevelData>();
        AssetDatabase.CreateAsset(newLevel, path);
        SaveChanges();

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

    private MoveDirection GetEnumFromVector(Vector3 dir)
    {
        dir = new Vector3(Mathf.Round(dir.x), Mathf.Round(dir.y), Mathf.Round(dir.z));

        if (dir == Vector3.forward) return MoveDirection.Forward;
        if (dir == Vector3.back) return MoveDirection.Back;
        if (dir == Vector3.up) return MoveDirection.Up;
        if (dir == Vector3.down) return MoveDirection.Down;
        if (dir == Vector3.left) return MoveDirection.Left;
        if (dir == Vector3.right) return MoveDirection.Right;

        return MoveDirection.Forward;
    }

    #endregion
}

// Editor custom pentru a avea un inspector curat pentru LevelData în fereastra noastră
[CustomEditor(typeof(LevelData))]
public class CleanLevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}

