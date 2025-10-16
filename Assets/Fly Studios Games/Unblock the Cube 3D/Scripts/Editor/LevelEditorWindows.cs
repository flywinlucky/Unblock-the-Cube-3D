using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

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
    private Vector3 _previewPanOffset = Vector3.zero; // NOU: Pentru deplasarea camerei (panning)
    private const float MIN_ZOOM = 2f;
    private const float MAX_ZOOM = 100f;
    private readonly List<Collider> _previewColliders = new List<Collider>();
    private Ray _debugRay;
    private Vector2 _mousePosition; // NOU: Stocăm poziția mouse-ului pentru a o folosi în Repaint


    // --- Stocare persistentă ---
    private static string _blockPrefabPathKey = "LevelEditor_BlockPrefabPath";

    // --- Stilizare ---
    private GUIStyle _previewBoxStyle;

    // Unitatea grilei folosită pentru poziționarea blocurilor în preview (potrivita cu runtime)
    private float _gridUnitSize = 0.5f;

    // Editor-scene integration
    [Tooltip("If true, the editor scene will be opened automatically when the window is enabled.")]
    private bool _autoOpenEditorScene = true;
    private Scene _tempEditorScene;
    private bool _editorSceneOpen = false;
    private GameObject _sceneRootGO;
    private const string TempSceneName = "LevelEditor_TempScene";

    // snapshot pentru detectarea modificărilor din scena editor
    private List<BlockData> _lastSceneSnapshot = new List<BlockData>();
    private bool _suppressSceneSync = false; // pentru a evita bucle când populăm scena

    // --- Compatibilitate nume vechi ---
    // Unele părți ale codului foloseau nume alternative (_selectedInstance, _selectedIndex, CalculateBounds).
    // Pentru a evita erori de compilare le mapăm către variantele curente.
    private GameObject _selectedInstance
    {
        get => _selectedBlockInstance;
        set => _selectedBlockInstance = value;
    }

    private int _selectedIndex
    {
        get => _selectedBlockIndex;
        set => _selectedBlockIndex = value;
    }

    // Mapare pentru funcția CalculateBounds -> folosim CalculateLevelBounds existentă
    private Bounds CalculateBounds()
    {
        return CalculateLevelBounds();
    }

    // manager pentru lista nivelelor din proiect
    private List<LevelData> _allLevels = new List<LevelData>();
    private Vector2 _levelsScroll = Vector2.zero;

    // dirty tracking
    private bool _isDirty = false;

    // --- ADĂUGATE: helper methods lipsă ---
    // Reîncarcă lista de LevelData din proiect
    private void RefreshLevelList()
    {
        _allLevels.Clear();
        string[] guids = AssetDatabase.FindAssets("t:LevelData");
        foreach (string g in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(g);
            LevelData lvl = AssetDatabase.LoadAssetAtPath<LevelData>(path);
            if (lvl != null) _allLevels.Add(lvl);
        }
    }

    // Șterge asset-ul LevelData curent (cu confirmare) și actualizează lista
    private void DeleteCurrentLevelAsset()
    {
        if (_currentLevel == null) return;
        string path = AssetDatabase.GetAssetPath(_currentLevel);
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("Cannot delete level: invalid asset path.");
            return;
        }
        if (!EditorUtility.DisplayDialog("Delete Level", $"Are you sure you want to delete {_currentLevel.name}?", "Delete", "Cancel"))
            return;

        bool success = AssetDatabase.DeleteAsset(path);
        if (success)
        {
            AssetDatabase.SaveAssets();
            _currentLevel = null;
            RefreshLevelList();
            RebuildPreview();
            Debug.Log("Level asset deleted.");
        }
        else
        {
            Debug.LogWarning("Failed to delete level asset: " + path);
        }
    }

    // Încearcă schimbarea selecției la un alt LevelData. Dacă există modificări nesalvate,
    // întreabă utilizatorul. Returnează true dacă schimbarea a avut loc.
    private bool TryChangeSelection(LevelData newLevel)
    {
        if (newLevel == _currentLevel) return true;

        if (_isDirty && _currentLevel != null)
        {
            int choice = EditorUtility.DisplayDialogComplex("Unsaved changes", $"Save changes to {_currentLevel.name}?", "Save", "Don't Save", "Cancel");
            if (choice == 0)
            {
                SaveChanges();
            }
            else if (choice == 2)
            {
                // Cancel
                return false;
            }
            // If "Don't Save" (choice == 1) fallthrough and change selection
        }

        _currentLevel = newLevel;
        DeselectBlock();
        RebuildPreview();
        _isDirty = false;
        FocusCamera();
        return true;
    }

    [MenuItem("Tools/Unblock Cube/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    #region Ciclul de Viață al Ferstrei

    private void OnEnable()
    {
        _previewRenderUtility = new PreviewRenderUtility();
        _previewRenderUtility.camera.cameraType = CameraType.Preview;
        _previewRenderUtility.camera.clearFlags = CameraClearFlags.SolidColor;
        _previewRenderUtility.camera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0);
        _previewRenderUtility.camera.fieldOfView = 30f;
        _previewRenderUtility.camera.nearClipPlane = 0.1f;
        _previewRenderUtility.camera.farClipPlane = 1000f;

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

        string prefabPath = EditorPrefs.GetString(_blockPrefabPathKey, "");
        if (!string.IsNullOrEmpty(prefabPath))
        {
            _blockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        RefreshLevelList();

        // subscribe update pentru sincronizare scena -> LevelData
        EditorApplication.update -= EditorUpdate;
        EditorApplication.update += EditorUpdate;

        // Dacă cerem auto-open, deschidem scena editor (single mode) automat
        if (_autoOpenEditorScene)
        {
            OpenLevelInScene(singleMode: true);
        }
    }

    private void OnDisable()
    {
        _previewRenderUtility.Cleanup();
        _previewRenderUtility = null;
        // unsubscribe update și închidem scena editor dacă e deschisă
        EditorApplication.update -= EditorUpdate;

        // Prompt save la închiderea ferestrei dacă avem modificări nesalvate
        if (_isDirty && _currentLevel != null)
        {
            int choice = EditorUtility.DisplayDialogComplex("Unsaved changes", $"Save changes to {_currentLevel.name}?", "Save", "Don't Save", "Cancel");
            if (choice == 0) SaveChanges();
            else if (choice == 2)
            {
                // dacă utilizatorul a ales Cancel, păstrăm scena deschisă (nu închidem acum)
                // pentru simplitate aici nu re-opens scena, doar avertizăm
                Debug.Log("Close canceled - unsaved changes remain.");
            }
        }

        if (_editorSceneOpen) CloseEditorScene();
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
        if (GUILayout.Button("Save Changes", EditorStyles.toolbarButton)) SaveChanges();
        // Buton pentru deschiderea nivelului în Scene view (editor normal)
        if (GUILayout.Button(_editorSceneOpen ? "Close Scene" : "Open In Scene", EditorStyles.toolbarButton))
        {
            if (!_editorSceneOpen) OpenLevelInScene(singleMode: true);
            else CloseEditorScene();
        }
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

        EditorGUILayout.LabelField("Levels", EditorStyles.boldLabel);

        // Lista nivelelor din proiect
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh")) RefreshLevelList();
        if (GUILayout.Button("New")) CreateNewLevelAsset();
        if (_currentLevel != null && GUILayout.Button("Delete")) { if (EditorUtility.DisplayDialog("Delete Level", $"Delete {_currentLevel.name}?", "Yes", "No")) DeleteCurrentLevelAsset(); }
        EditorGUILayout.EndHorizontal();

        _levelsScroll = EditorGUILayout.BeginScrollView(_levelsScroll, GUILayout.Height(120));
        for (int i = 0; i < _allLevels.Count; i++)
        {
            LevelData lvl = _allLevels[i];
            if (lvl == null) continue;
            EditorGUILayout.BeginHorizontal();
            bool isSelected = (lvl == _currentLevel);
            if (GUILayout.Toggle(isSelected, lvl.name, "Button"))
            {
                if (_currentLevel != lvl) { if (!TryChangeSelection(lvl)) break; }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(8);

        EditorGUILayout.LabelField("Editor Settings", EditorStyles.boldLabel);

        // Permitem ajustarea unității grilei în editor (corelează cu gridRuntime de 0.5)
        _gridUnitSize = EditorGUILayout.FloatField("Grid Unit Size", _gridUnitSize);

        GameObject newPrefab = (GameObject)EditorGUILayout.ObjectField("Block Prefab", _blockPrefab, typeof(GameObject), false);
        if (newPrefab != _blockPrefab)
        {
            _blockPrefab = newPrefab;
            EditorPrefs.SetString(_blockPrefabPathKey, _blockPrefab != null ? AssetDatabase.GetAssetPath(_blockPrefab) : "");
            RebuildPreview();
        }
        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Level Properties", EditorStyles.boldLabel);

        LevelData newLevel = (LevelData)EditorGUILayout.ObjectField("Current Level", _currentLevel, typeof(LevelData), false);
        if (newLevel != _currentLevel)
        {
            _currentLevel = newLevel;
            DeselectBlock();
            RebuildPreview();
            FocusCamera();
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
                FocusCamera();
                Debug.Log("Level generated and preview updated!");
                // NOU: populăm scena editor dacă este deschisă
                if (_editorSceneOpen) PopulateEditorScene();
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

        HandlePreviewInput(previewRect);

        if (Event.current.type == EventType.Repaint)
        {
            _previewRenderUtility.BeginPreview(previewRect, GUIStyle.none);

            // Asigurăm pixelRect-ul camerei preview în pixeli (pentru ecrane Retina / DPI scale)
            float ppp = EditorGUIUtility.pixelsPerPoint;
            _previewRenderUtility.camera.pixelRect = new Rect(0, 0, previewRect.width * ppp, previewRect.height * ppp);

            // Convertim poziția mouse-ului (GUI space) în coordonate pixel pentru camera preview
            Vector2 localMousePos = _mousePosition - previewRect.position;
            Vector2 pixelPos = localMousePos * ppp;
            pixelPos.y = previewRect.height * ppp - pixelPos.y; // flip Y pentru ScreenPoint
            _debugRay = _previewRenderUtility.camera.ScreenPointToRay(new Vector3(pixelPos.x, pixelPos.y, 0));

            Quaternion rotation = Quaternion.Euler(_previewRotation.y, _previewRotation.x, 0);
            Bounds bounds = CalculateLevelBounds();
            Vector3 center = bounds.center + _previewPanOffset;
            Vector3 camPosition = center + (rotation * Vector3.forward * -_previewZoom);

            _previewRenderUtility.camera.transform.position = camPosition;
            _previewRenderUtility.camera.transform.rotation = rotation;

            _previewRenderUtility.Render();

            Handles.SetCamera(_previewRenderUtility.camera);
            DrawGrid(bounds.center);
            DrawRaycastGizmo_3D();
            DrawSelectionOutline_3D();

            Texture result = _previewRenderUtility.EndPreview();
            GUI.DrawTexture(previewRect, result, ScaleMode.StretchToFill, false);
        }

        GUI.BeginClip(previewRect);
        DrawEditingButtons_GUI(previewRect);
        GUI.EndClip();

        EditorGUILayout.EndVertical();
    }

    #endregion

    #region Logică Tool

    private void HandlePreviewInput(Rect previewRect)
    {
        Event e = Event.current;

        if (previewRect.Contains(e.mousePosition))
        {
            _mousePosition = e.mousePosition;
            if (e.type == EventType.MouseMove || e.type == EventType.MouseDrag || e.type == EventType.ScrollWheel)
            {
                Repaint();
            }
        }

        if (!previewRect.Contains(e.mousePosition)) return;

        // NOU: Funcția Focus pe tasta 'F'
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.F)
        {
            FocusCamera();
            e.Use();
        }

        // NOU: Panning cu CLICK-MIJLOC
        if (e.type == EventType.MouseDrag && e.button == 2)
        {
            _previewPanOffset -= (_previewRenderUtility.camera.transform.right * e.delta.x - _previewRenderUtility.camera.transform.up * e.delta.y) * 0.05f * (_previewZoom / 10f);
            e.Use();
        }

        if (e.type == EventType.MouseDrag && e.button == 1)
        {
            _previewRotation.x += e.delta.x * 0.5f;
            _previewRotation.y += e.delta.y * 0.5f;
            _previewRotation.y = Mathf.Clamp(_previewRotation.y, -80, 80);
            e.Use();
        }

        if (e.type == EventType.ScrollWheel)
        {
            _previewZoom -= e.delta.y * 0.2f;
            _previewZoom = Mathf.Clamp(_previewZoom, MIN_ZOOM, MAX_ZOOM);
            e.Use();
        }

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            HandleSelection();
            e.Use();
        }
    }

    // NOU: Funcție pentru a desena un grilaj
    private void DrawGrid(Vector3 center)
    {
        // Desenăm un grilaj centrat pe nivel, cu spacing egal cu gridUnitSize
        float spacing = Mathf.Max(0.0001f, _gridUnitSize);
        Bounds b = CalculateBounds();
        float extentX = Mathf.Max(2f, b.extents.x + 2f);
        float extentZ = Mathf.Max(2f, b.extents.z + 2f);
        int linesX = Mathf.CeilToInt((extentX * 2f) / spacing);
        int linesZ = Mathf.CeilToInt((extentZ * 2f) / spacing);

        float halfX = linesX * spacing * 0.5f;
        float halfZ = linesZ * spacing * 0.5f;
        float y = Mathf.Floor(center.y) - (_gridUnitSize * 0.5f);

        Handles.color = new Color(1f, 1f, 1f, 0.08f);
        for (int i = 0; i <= linesX; i++)
        {
            float x = -halfX + i * spacing;
            Handles.DrawLine(new Vector3(x, y, -halfZ) + center, new Vector3(x, y, halfZ) + center);
        }
        for (int j = 0; j <= linesZ; j++)
        {
            float z = -halfZ + j * spacing;
            Handles.DrawLine(new Vector3(-halfX, y, z) + center, new Vector3(halfX, y, z) + center);
        }
    }

    private void DrawRaycastGizmo_3D()
    {
        if (_debugRay.direction == Vector3.zero) return;
        Handles.color = Color.red;
        Handles.DrawLine(_debugRay.origin, _debugRay.origin + _debugRay.direction * 200f);
    }

    private void DrawSelectionOutline_3D()
    {
        if (_selectedBlockInstance == null) return;

        Renderer renderer = _selectedBlockInstance.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;
            Handles.color = Color.yellow;
            Handles.DrawWireCube(bounds.center, bounds.size * 1.05f);
        }
    }

    private void DrawEditingButtons_GUI(Rect previewRect)
    {
        if (_selectedBlockInstance == null) return;

        Renderer renderer = _selectedBlockInstance.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Vector3 screenPos = _previewRenderUtility.camera.WorldToScreenPoint(renderer.bounds.center);

            if (screenPos.z > 0)
            {
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
    }

    private void HandleSelection()
    {
        GameObject closestColliderObject = null;
        float closestDistance = float.MaxValue;

        // Găsim collider-ul cel mai apropiat lovit de rază
        foreach (Collider col in _previewColliders)
        {
            if (col == null) continue;
            if (col.Raycast(_debugRay, out RaycastHit hit, 2000f))
            {
                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    closestColliderObject = col.gameObject;
                }
            }
        }

        if (closestColliderObject != null)
        {
            // Avem collider -> urcăm la copilul direct al _previewRoot (instanța blocului)
            GameObject rootInstance = GetPreviewRootChildForObject(closestColliderObject);
            if (rootInstance != null)
            {
                SelectBlock(rootInstance);
            }
            else
            {
                // fallback: dacă nu găsim root, deselectăm
                DeselectBlock();
            }
        }
        else
        {
            DeselectBlock();
        }
        Repaint();
    }

    // Helper: primește un GameObject (de obicei collider.gameObject) și urcă până la copilul direct al _previewRoot
    private GameObject GetPreviewRootChildForObject(GameObject obj)
    {
        if (obj == null || _previewRoot == null) return null;
        Transform t = obj.transform;
        while (t != null && t.parent != null)
        {
            if (t.parent == _previewRoot.transform)
                return t.gameObject;
            t = t.parent;
        }
        return null;
    }

    private void SelectBlock(GameObject instance)
    {
        if (instance == null)
        {
            DeselectBlock();
            return;
        }

        _selectedBlockInstance = instance;
        _selectedBlockIndex = -1;

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

        _selectedInstance = null;
        _selectedIndex = -1;
        _previewColliders.Clear();

        while (_previewRoot.transform.childCount > 0)
            DestroyImmediate(_previewRoot.transform.GetChild(0).gameObject);

        if (_currentLevel == null || _blockPrefab == null) { Repaint(); return; }

        var blocks = _currentLevel.GetBlocks();
        for (int i = 0; i < blocks.Count; i++)
        {
            var data = blocks[i];
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(_blockPrefab, _previewRoot.transform);
            // folosim unitatea grilei configurabilă (potrivit cu runtime)
            inst.transform.localPosition = (Vector3)data.position * _gridUnitSize;
            inst.transform.localRotation = GetStableLookRotation(data.direction);

            Collider c = inst.GetComponentInChildren<Collider>(true);
            if (c == null) c = inst.AddComponent<BoxCollider>();
            // Ajustăm dimensiunea colider-ului pentru a se potrivi cu unitatea grilei (dacă e BoxCollider)
            if (c is BoxCollider bc)
            {
                bc.size = Vector3.one * _gridUnitSize;
                bc.center = Vector3.zero;
            }
            c.enabled = true;
            _previewColliders.Add(c);
        }
        Repaint();
    }

    private void SaveChanges()
    {
        if (_currentLevel != null)
        {
            EditorUtility.SetDirty(_currentLevel);
            AssetDatabase.SaveAssets();
            _isDirty = false;
            Debug.Log($"Saved Level {_currentLevel.name}");
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

    // Deschide o scenă (single sau additive) și populează ea cu blocuri
    private void OpenLevelInScene(bool singleMode = true)
    {
        if (_currentLevel == null)
        {
            // nu avem nivel încă selectat, dar tot putem crea o scenă goală
            if (!EditorUtility.DisplayDialog("Open Editor Scene", "No Level selected. Open empty editor scene anyway?", "Yes", "No")) return;
        }

        // dacă deja e deschis, facem nothing
        if (_editorSceneOpen) return;

        // Creeăm/Deschidem scena în modul single (pentru a rula ca scena full editor)
        _tempEditorScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        _tempEditorScene.name = TempSceneName;

        // Root container
        _sceneRootGO = new GameObject("LevelEditorSceneRoot");
        SceneManager.MoveGameObjectToScene(_sceneRootGO, _tempEditorScene);

        // Populam scena cu date actuale
        PopulateEditorScene();
        _editorSceneOpen = true;

        // focus în SceneView
        Selection.activeGameObject = _sceneRootGO;
     
    }

    // Populează scena editor cu prefab-urile reale, reflectând LevelData
    private void PopulateEditorScene()
    {
        if (_sceneRootGO == null) return;
        _suppressSceneSync = true; // prevenim sincronizarea în timp ce modificăm scena

        // curățăm vechiul conținut
        for (int i = _sceneRootGO.transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(_sceneRootGO.transform.GetChild(i).gameObject);

        if (_currentLevel == null || _blockPrefab == null)
        {
            _suppressSceneSync = false;
            return;
        }

        var blocks = _currentLevel.GetBlocks();
        for (int i = 0; i < blocks.Count; i++)
        {
            var data = blocks[i];
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(_blockPrefab);
            inst.transform.SetParent(_sceneRootGO.transform);
            inst.transform.position = (Vector3)data.position * _gridUnitSize;
            inst.transform.rotation = GetStableLookRotation(data.direction) * data.randomVisualRotation;
            // asigurăm collider pentru interacțiune
            if (inst.GetComponentInChildren<Collider>(true) == null) inst.AddComponent<BoxCollider>();
            // mark prefab instance saved to scene
            EditorUtility.SetDirty(inst);
        }

        // cache snapshot
        CacheSceneSnapshot();
        _suppressSceneSync = false;
    }

    // Închide scena temporară
    private void CloseEditorScene()
    {
        if (!_editorSceneOpen) return;
        if (_sceneRootGO != null) DestroyImmediate(_sceneRootGO);
        if (_tempEditorScene.IsValid())
        {
            EditorSceneManager.CloseScene(_tempEditorScene, true);
        }
        _editorSceneOpen = false;
        _sceneRootGO = null;
        _tempEditorScene = default;
        _lastSceneSnapshot.Clear();
    }

    // Cache current scene content from _sceneRootGO into _lastSceneSnapshot
    private void CacheSceneSnapshot()
    {
        _lastSceneSnapshot.Clear();
        if (_sceneRootGO == null) return;
        for (int i = 0; i < _sceneRootGO.transform.childCount; i++)
        {
            Transform t = _sceneRootGO.transform.GetChild(i);
            Vector3Int gridPos = Vector3Int.RoundToInt(t.position / _gridUnitSize);
            MoveDirection dir = GetDirectionFromForward(t.forward);
            BlockData bd = new BlockData { position = gridPos, direction = dir, randomVisualRotation = Quaternion.identity };
            _lastSceneSnapshot.Add(bd);
        }
    }

    // Periodic update: detect changes in scene and write back to LevelData
    private void EditorUpdate()
    {
        if (!_editorSceneOpen || _suppressSceneSync) return;
        if (_sceneRootGO == null || _currentLevel == null) return;

        // build snapshot from scene
        var newSnapshot = new List<BlockData>();
        for (int i = 0; i < _sceneRootGO.transform.childCount; i++)
        {
            Transform t = _sceneRootGO.transform.GetChild(i);
            Vector3Int gridPos = Vector3Int.RoundToInt(t.position / _gridUnitSize);
            MoveDirection dir = GetDirectionFromForward(t.forward);
            newSnapshot.Add(new BlockData { position = gridPos, direction = dir, randomVisualRotation = Quaternion.identity });
        }

        // compare with last snapshot (simple equality)
        if (!AreBlockListsEqual(_lastSceneSnapshot, newSnapshot))
        {
            // actualizăm LevelData (rescriem lista)
            var blocksList = _currentLevel.GetBlocks();
            blocksList.Clear();
            foreach (var b in newSnapshot)
            {
                blocksList.Add(b);
            }
            // NU mai salvăm automat pe disc; marcam ca dirty și actualizăm preview
            EditorUtility.SetDirty(_currentLevel);
            _isDirty = true;

            // actualizăm preview pentru a reflecta schimbările
            RebuildPreview();

            // cache noul snapshot
            _lastSceneSnapshot = new List<BlockData>(newSnapshot);
        }
    }

    private bool AreBlockListsEqual(List<BlockData> a, List<BlockData> b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i].position != b[i].position) return false;
            if (a[i].direction != b[i].direction) return false;
        }
        return true;
    }

    private MoveDirection GetDirectionFromForward(Vector3 fwd)
    {
        Vector3 abs = new Vector3(Mathf.Abs(fwd.x), Mathf.Abs(fwd.y), Mathf.Abs(fwd.z));
        if (abs.x >= abs.y && abs.x >= abs.z) return fwd.x >= 0 ? MoveDirection.Right : MoveDirection.Left;
        if (abs.y >= abs.x && abs.y >= abs.z) return fwd.y >= 0 ? MoveDirection.Up : MoveDirection.Down;
        return fwd.z >= 0 ? MoveDirection.Forward : MoveDirection.Back;
    }

    #endregion

    #region Funcții Ajutătoare (Helpers)

    // NOU: Calculează limitele tuturor blocurilor pentru centrare și focalizare
    private Bounds CalculateLevelBounds()
    {
        if (_previewRoot == null || _previewRoot.transform.childCount == 0)
        {
            return new Bounds(Vector3.zero, Vector3.one * 5);
        }

        var bounds = new Bounds();
        var renderers = _previewRoot.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            bounds = new Bounds(renderers[0].bounds.center, Vector3.zero);
            foreach (var r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }
        }
        return bounds;
    }

    // NOU: Funcție pentru a focaliza camera pe nivel
    private void FocusCamera()
    {
        Bounds bounds = CalculateLevelBounds();
        _previewPanOffset = Vector3.zero;

        float objectSize = bounds.size.magnitude;
        float cameraDistance = objectSize / (2.0f * Mathf.Tan(0.5f * _previewRenderUtility.camera.fieldOfView * Mathf.Deg2Rad));

        _previewZoom = Mathf.Clamp(cameraDistance, MIN_ZOOM, MAX_ZOOM);
        Repaint();
    }

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

