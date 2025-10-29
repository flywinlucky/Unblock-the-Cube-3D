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

	// ADĂUGAT: Căi către scenele proiectului (asigură că path-urile corespund proiectului tău)
	private const string LevelEditorScenePath = "Assets/Fly Studios Games/Tap Away Block Puzzle 3D/Scenes/Level Editor.unity";
	private const string GameScenePath = "Assets/Fly Studios Games/Tap Away Block Puzzle 3D/Scenes/Game Scene.unity";

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
			Debug.Log("Level asset deleted.");

			// NOU: Curățăm toate cuburile din scena Level Editor
			ClearEditorScene();
		}
		else
		{
			Debug.LogWarning("Failed to delete level asset: " + path);
		}
	}

	// NOU: Funcție pentru a curăța toate cuburile din scena Level Editor
	private void ClearEditorScene()
	{
		if (_sceneRootGO != null)
		{
			for (int i = _sceneRootGO.transform.childCount - 1; i >= 0; i--)
			{
				DestroyImmediate(_sceneRootGO.transform.GetChild(i).gameObject);
			}
			Debug.Log("All blocks cleared from the Level Editor scene.");
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
		}

		_currentLevel = newLevel;

		// Sincronizăm LevelManager cu nivelul curent
		SyncLevelManager();

		// în loc de RebuildPreview (preview offscreen), sincronizăm scena editor reală
		if (_editorSceneOpen)
		{
			PopulateEditorScene();
		}

		_isDirty = false;
		FocusCamera();
		return true;
	}

	private void SyncLevelManager()
	{
		// Căutăm componenta LevelManager în scenă
		LevelManager levelManager = FindObjectOfType<LevelManager>();
		if (levelManager != null)
		{
			levelManager.runCurrentLevel = _currentLevel;
			Debug.Log($"LevelManager synchronized with current level: {_currentLevel?.name ?? "None"}");
		}
		else
		{
			Debug.LogWarning("LevelManager not found in the scene. Synchronization skipped.");
		}
	}

	[MenuItem("Tools/Tap Away Block Puzzle 3D/Level Editor")]
	public static void ShowWindow()
	{
		GetWindow<LevelEditorWindow>("Level Editor");
	}

	#region Ciclul de Viață al Ferstrei

	private void OnEnable()
	{
		// NOTE: removed creation of preview lights and preview root (these were only for off-screen preview)
		_previewBoxStyle = null;

		try
		{
			string prefabPath = EditorPrefs.GetString(_blockPrefabPathKey, "");
			if (!string.IsNullOrEmpty(prefabPath))
			{
				_blockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
			}
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("Failed to load block prefab from EditorPrefs: " + ex.Message);
			_blockPrefab = null;
		}

		RefreshLevelList();

		// subscribe update pentru sincronizare scena -> LevelData
		EditorApplication.update -= EditorUpdate;
		EditorApplication.update += EditorUpdate;

		// Dacă cerem auto-open, deschidem scena editor (scene reală)
		if (_autoOpenEditorScene)
		{
			try
			{
				OpenLevelInScene(singleMode: true);
			}
			catch (System.Exception ex)
			{
				//Debug.LogWarning("OpenLevelInScene failed: " + ex.Message);
			}
		}

		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
	}

	private void OnDisable()
	{
		EditorApplication.update -= EditorUpdate;
		EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

		// Prompt save la închiderea ferestrei dacă avem modificări nesalvate
		if (_isDirty && _currentLevel != null)
		{
			int choice = EditorUtility.DisplayDialogComplex("Unsaved changes", $"Save changes to {_currentLevel.name}?", "Save", "Don't Save", "Cancel");
			if (choice == 0) SaveChanges();
			else if (choice == 2)
			{
				Debug.Log("Close canceled - unsaved changes remain.");
			}
		}

		// Dacă scena editor e deschisă, închidem și revenim la scena de joc
		if (_editorSceneOpen) CloseEditorScene();
	}

	private void OnPlayModeStateChanged(PlayModeStateChange state)
	{
		if (state == PlayModeStateChange.EnteredEditMode)
		{
			// Close and reopen the Level Editor window to ensure full reinitialization
			Debug.Log("Reinitializing Level Editor window after exiting Play Mode.");
			Close();
			EditorApplication.delayCall += ShowWindow; // Reopen the window on the next editor update
		}
	}

	#endregion


	private void OnGUI()
	{
		// Asigurăm inițializarea stilurilor GUI
		if (_previewBoxStyle == null)
		{
			_previewBoxStyle = new GUIStyle("box")
			{
				padding = new RectOffset(2, 2, 2, 2)
			};
		}

		// Disable interactivity in Play Mode
		bool isInPlayMode = EditorApplication.isPlayingOrWillChangePlaymode;
		GUI.enabled = !isInPlayMode;

		if (isInPlayMode)
		{
			EditorGUILayout.HelpBox("Editing is disabled in Play Mode. Exit Play Mode to edit levels.", MessageType.Warning);
		}

		DrawToolbar();
		EditorGUILayout.BeginHorizontal();
		DrawLevelListPanel(); // Panel pentru lista nivelelor
		DrawEditorSettingsPanel(); // Panel pentru setările editorului
		EditorGUILayout.EndHorizontal();

		// Re-enable interactivity after the UI
		GUI.enabled = true;
	}


	private void DrawToolbar()
	{
		EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
		if (GUILayout.Button("New Level", EditorStyles.toolbarButton)) CreateNewLevelAsset();
		if (GUILayout.Button("Save Changes", EditorStyles.toolbarButton)) SaveChanges();
		if (GUILayout.Button("Refresh", EditorStyles.toolbarButton)) RefreshLevelList();

		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
	}

	private void DrawLevelListPanel()
	{
		EditorGUILayout.BeginVertical(GUILayout.Width(250)); // Panel pentru lista nivelelor
		EditorGUILayout.LabelField("Levels", EditorStyles.boldLabel);

		// Butoane pentru gestionarea nivelelor
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("New", GUILayout.Width(80))) CreateNewLevelAsset();
		if (_currentLevel != null && GUILayout.Button("Delete", GUILayout.Width(80)))
		{
			if (EditorUtility.DisplayDialog("Delete Level", $"Delete {_currentLevel.name}?", "Yes", "No"))
				DeleteCurrentLevelAsset();
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space(5);

		// ScrollView pentru lista nivelelor
		_levelsScroll = EditorGUILayout.BeginScrollView(_levelsScroll, GUILayout.ExpandHeight(true));
		foreach (var lvl in _allLevels)
		{
			if (lvl == null) continue;
			bool isSelected = (lvl == _currentLevel);
			GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
			buttonStyle.alignment = TextAnchor.MiddleLeft;
			buttonStyle.padding = new RectOffset(10, 10, 5, 5);

			if (GUILayout.Toggle(isSelected, lvl.name, buttonStyle))
			{
				if (_currentLevel != lvl) TryChangeSelection(lvl);
			}
		}
		EditorGUILayout.EndScrollView();

		EditorGUILayout.EndVertical();
	}

	private void DrawEditorSettingsPanel()
	{
		EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)); // Panel pentru setările editorului
		EditorGUILayout.LabelField("Editor Settings", EditorStyles.boldLabel);

		// Setări generale
		EditorGUILayout.Space(5);
		EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
		_gridUnitSize = EditorGUILayout.FloatField("Grid Unit Size", _gridUnitSize);

		GameObject newPrefab = (GameObject)EditorGUILayout.ObjectField("Block Prefab", _blockPrefab, typeof(GameObject), false);
		if (newPrefab != _blockPrefab)
		{
			_blockPrefab = newPrefab;
			EditorPrefs.SetString(_blockPrefabPathKey, _blockPrefab != null ? AssetDatabase.GetAssetPath(_blockPrefab) : "");
		}

		EditorGUILayout.Space(10);

		// Setări pentru nivelul curent
		EditorGUILayout.LabelField("Level Properties", EditorStyles.boldLabel);
		LevelData newLevel = (LevelData)EditorGUILayout.ObjectField("Current Level", _currentLevel, typeof(LevelData), false);
		if (newLevel != _currentLevel)
		{
			_currentLevel = newLevel;
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

			// Afișăm și edităm variabilele ascunse din LevelData
			EditorGUILayout.LabelField("Level Generation Settings", EditorStyles.boldLabel);
			_currentLevel.customGridLength = EditorGUILayout.IntSlider("Grid Length", _currentLevel.customGridLength, 2, 10);
			_currentLevel.customGridHeight = EditorGUILayout.IntSlider("Grid Height", _currentLevel.customGridHeight, 2, 10);
			_currentLevel.seed = EditorGUILayout.IntField("Seed", _currentLevel.seed);

			EditorGUILayout.Space(10);

			EditorGUI.BeginDisabledGroup(_blockPrefab == null);
			if (GUILayout.Button("Generate Level", GUILayout.Height(30)))
			{
				_currentLevel.Generate();
				SaveChanges();
				FocusCamera();
				Debug.Log("Level generated and preview updated!");
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

	#region Desenare UI
	#endregion

	#region Logică Tool

	private void SaveChanges()
	{
		if (_currentLevel != null)
		{
			EditorUtility.SetDirty(_currentLevel);
			AssetDatabase.SaveAssets();
			_isDirty = false;
			Debug.Log($"Saved Level {_currentLevel.name}");
		    RefreshLevelList();
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
	}

	// Deschide o scenă (single sau additive) și populează ea cu blocuri
	private void OpenLevelInScene(bool singleMode = true)
	{
		// dacă deja e deschis, nu facem nimic
		if (_editorSceneOpen) return;

		// Încercăm să deschidem scena Level Editor.unity
		if (!System.IO.File.Exists(LevelEditorScenePath))
		{
			if (!EditorUtility.DisplayDialog("Scene missing", $"Cannot find scene at {LevelEditorScenePath}. Create or set the path correctly.", "OK")) return;
		}

		var openMode = OpenSceneMode.Single;
		_tempEditorScene = EditorSceneManager.OpenScene(LevelEditorScenePath, openMode);
		_editorSceneOpen = true;

		// Găsim sau creăm root-ul pentru populare
		_sceneRootGO = GameObject.Find("LevelEditorSceneRoot");
		if (_sceneRootGO == null)
		{
			_sceneRootGO = new GameObject("LevelEditorSceneRoot");
			SceneManager.MoveGameObjectToScene(_sceneRootGO, _tempEditorScene);
		}

		// Populăm scena cu date actuale
		PopulateEditorScene();

		// focus în SceneView
		Selection.activeGameObject = _sceneRootGO;
	}

	// Populează scena editor (rămâne similar, dar asigurăm că _sceneRootGO există în scenă)
	private void PopulateEditorScene()
	{
		if (_sceneRootGO == null)
		{
			_sceneRootGO = GameObject.Find("LevelEditorSceneRoot");
			if (_sceneRootGO == null)
			{
				// Dacă nu găsim root, creăm unul în scena activă
				_sceneRootGO = new GameObject("LevelEditorSceneRoot");
				if (EditorSceneManager.GetActiveScene().IsValid())
					SceneManager.MoveGameObjectToScene(_sceneRootGO, EditorSceneManager.GetActiveScene());
			}
		}

		_suppressSceneSync = true; // prevenim sincronizarea în timp ce modificăm scena

		// Curățăm vechiul conținut
		for (int i = _sceneRootGO.transform.childCount - 1; i >= 0; i--)
		{
			DestroyImmediate(_sceneRootGO.transform.GetChild(i).gameObject);
		}

		if (_currentLevel == null || _blockPrefab == null)
		{
			Debug.LogWarning("Current level or block prefab is not set. Cannot populate the scene.");
			_suppressSceneSync = false;
			return;
		}

		var blocks = _currentLevel.GetBlocks();
		if (blocks == null || blocks.Count == 0)
		{
			Debug.LogWarning("No blocks found in the current level data.");
			_suppressSceneSync = false;
			return;
		}

		// Protecție la grid unit size (evită divide by zero etc.)
		if (Mathf.Approximately(_gridUnitSize, 0f))
			_gridUnitSize = 0.5f;

		int blockIndex = 0; // Contor pentru redenumirea cuburilor
		foreach (var data in blocks)
		{
			try
			{
				GameObject inst = PrefabUtility.InstantiatePrefab(_blockPrefab) as GameObject;
				if (inst == null)
				{
					Debug.LogError("Failed to instantiate block prefab.");
					continue;
				}

				inst.transform.SetParent(_sceneRootGO.transform);
				inst.transform.position = (Vector3)data.position * _gridUnitSize;
				inst.transform.rotation = GetStableLookRotation(data.direction) * (data.randomVisualRotation != null ? data.randomVisualRotation : Quaternion.identity);
				inst.name = $"Block_{blockIndex}"; // Redenumim cubul
                blockIndex++;

				// Asigurăm că există un collider pentru interacțiune
				if (inst.GetComponent<Collider>() == null)
				{
					inst.AddComponent<BoxCollider>();
				}

				EditorUtility.SetDirty(inst);
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"Error while creating block instance: {ex.Message}");
			}
		}

		CacheSceneSnapshot();
		_suppressSceneSync = false;
	}

	// Închide scena editor și revine la scena de joc (Game Scene.unity)
	private void CloseEditorScene()
	{
		if (!_editorSceneOpen) return;

		// Salvăm scena editor dacă e dirty
		try
		{
			if (EditorSceneManager.GetActiveScene().isDirty)
			{
				if (EditorUtility.DisplayDialog("Save Scene", "Save changes to the Level Editor scene?", "Save", "Don't Save"))
				{
					EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
				}
			}
		}
		catch (System.Exception ex)
		{
			Debug.LogWarning("Error while saving editor scene: " + ex.Message);
		}

		// Resetăm root-ul LevelEditorSceneRoot
		if (_sceneRootGO != null)
		{
			for (int i = _sceneRootGO.transform.childCount - 1; i >= 0; i--)
			{
				DestroyImmediate(_sceneRootGO.transform.GetChild(i).gameObject);
			}
			Debug.Log("All children of LevelEditorSceneRoot have been cleared.");
		}

		// Închidem scena editor curentă și deschidem scena de joc
		try
		{
			if (System.IO.File.Exists(GameScenePath))
			{
				EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
			}
			else
			{
				//Debug.LogWarning("Game scene not found at: " + GameScenePath);
				// Eliminăm fallback-ul pentru NewScene, deoarece nu este permis în timpul reîncărcării asamblării
			}
		}
		catch (System.Exception ex)
		{
			//Debug.LogWarning("Failed to open game scene: " + ex.Message);
		}

		_editorSceneOpen = false;
		_sceneRootGO = null;
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
			if (t == null) continue;
			Vector3Int gridPos = Vector3Int.RoundToInt(t.position / Mathf.Max(0.0001f, _gridUnitSize));
			MoveDirection dir = GetDirectionFromForward(t.forward);
			BlockData bd = new BlockData { position = gridPos, direction = dir, randomVisualRotation = Quaternion.identity };
			_lastSceneSnapshot.Add(bd);
		}
	}

	// Periodic update: detect changes in scene and write back to LevelData
	private void EditorUpdate()
	{
		// protecție generală pentru a nu arunca excepții în update-ul editorului
		try
		{
			if (!_editorSceneOpen || _suppressSceneSync) return;
			if (_sceneRootGO == null || _currentLevel == null) return;

			// build snapshot from scene
			var newSnapshot = new List<BlockData>();
			for (int i = 0; i < _sceneRootGO.transform.childCount; i++)
			{
				Transform t = _sceneRootGO.transform.GetChild(i);
				if (t == null) continue;
				Vector3Int gridPos = Vector3Int.RoundToInt(t.position / Mathf.Max(0.0001f, _gridUnitSize));
				MoveDirection dir = GetDirectionFromForward(t.forward);
				newSnapshot.Add(new BlockData { position = gridPos, direction = dir, randomVisualRotation = Quaternion.identity });
			}

			// compare with last snapshot (simple equality)
			if (!AreBlockListsEqual(_lastSceneSnapshot, newSnapshot))
			{
				// actualizăm LevelData (rescriem lista)
				var blocksList = _currentLevel.GetBlocks();
				if (blocksList != null)
				{
					blocksList.Clear();
					foreach (var b in newSnapshot)
					{
						blocksList.Add(b);
					}
					// NU mai salvăm automat pe disc; marcam ca dirty și actualizăm preview
					EditorUtility.SetDirty(_currentLevel);
					_isDirty = true;
				}
				// cache noul snapshot
				_lastSceneSnapshot = new List<BlockData>(newSnapshot);
			}
		}
		catch (System.Exception ex)
		{
			// pentru siguranță, supresăm sincronizarea temporar și raportăm eroarea
			_suppressSceneSync = true;
			Debug.LogWarning("EditorUpdate error: " + ex.Message);
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
		// Prioritizăm scena reală (_sceneRootGO) dacă este deschisă
		if (_sceneRootGO != null && _sceneRootGO.transform.childCount > 0)
		{
			var renderers = _sceneRootGO.GetComponentsInChildren<Renderer>();
			if (renderers != null && renderers.Length > 0)
			{
				var b = new Bounds(renderers[0].bounds.center, Vector3.zero);
				foreach (var r in renderers)
				{
					if (r != null) b.Encapsulate(r.bounds);
				}
				return b;
			}
			else
			{
				// fallback: folosește pozițiile copiilor
				var bounds = new Bounds(_sceneRootGO.transform.GetChild(0).position, Vector3.zero);
				for (int i = 0; i < _sceneRootGO.transform.childCount; i++)
					bounds.Encapsulate(_sceneRootGO.transform.GetChild(i).position);
				return bounds;
			}
		}

		// Daca scena nu e populata, încercăm să estimăm bounds din LevelData (dacă există)
		if (_currentLevel != null)
		{
			var blocks = _currentLevel.GetBlocks();
			if (blocks != null && blocks.Count > 0)
			{
				var firstWorld = (Vector3)blocks[0].position * _gridUnitSize;
				var b = new Bounds(firstWorld, Vector3.zero);
				foreach (var bl in blocks)
				{
					b.Encapsulate((Vector3)bl.position * _gridUnitSize);
				}
				return b;
			}
		}

		// fallback generic
		return new Bounds(Vector3.zero, Vector3.one * 5f);
	}

	// NOU: Funcție pentru a focaliza camera pe nivel (folosește SceneView FOV când e disponibil)
	private void FocusCamera()
	{
		Bounds bounds = CalculateLevelBounds();

		// Determinăm un FOV sigur: preferăm SceneView dacă există
		float fov = 30f;
#if UNITY_EDITOR
		if (UnityEditor.SceneView.lastActiveSceneView != null && UnityEditor.SceneView.lastActiveSceneView.camera != null)
		{
			fov = UnityEditor.SceneView.lastActiveSceneView.camera.fieldOfView;
		}
#endif
		float objectSize = bounds.size.magnitude;
		float denom = 2.0f * Mathf.Tan(0.5f * fov * Mathf.Deg2Rad);
		if (denom <= 0.0001f) denom = 0.0001f;
		float cameraDistance = objectSize / denom;

		// Dacă scena editor e deschisă, poziționăm SceneView camera pentru o focalizare rapidă
#if UNITY_EDITOR
		if (UnityEditor.SceneView.lastActiveSceneView != null)
		{
			var sv = UnityEditor.SceneView.lastActiveSceneView;
			Vector3 center = bounds.center;
			Vector3 dir = (sv.camera.transform.position - center).normalized;
			sv.pivot = center;
			sv.size = Mathf.Max(1f, objectSize * 0.5f);
			sv.Repaint();
		}
#endif
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