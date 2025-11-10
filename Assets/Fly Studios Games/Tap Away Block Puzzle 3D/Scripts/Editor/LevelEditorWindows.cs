using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Tap_Away_Block_Puzzle_3D
{
	public class LevelEditorWindow : EditorWindow
	{
		#region Fields

		private LevelData _currentLevel;
		private GameObject _blockPrefab;
		private Editor _levelDataEditor;

		private GameObject _selectedBlockInstance;
		private int _selectedBlockIndex = -1;

		private static string _blockPrefabPathKey = "LevelEditor_BlockPrefabPath";

		private GUIStyle _previewBoxStyle;
		private float _gridUnitSize = 0.5f;

		[Tooltip("If true, the editor scene will be opened automatically when the window is enabled.")]
		private bool _autoOpenEditorScene = true;
		private Scene _tempEditorScene;
		private bool _editorSceneOpen = false;
		private GameObject _sceneRootGO;
		private const string TempSceneName = "LevelEditor_TempScene";

		private const string LevelEditorScenePath = "Assets/Fly Studios Games/Tap Away Block Puzzle 3D/Scenes/Level Editor.unity";
		private const string GameScenePath = "Assets/Fly Studios Games/Tap Away Block Puzzle 3D/Scenes/Game Demo Scene.unity";

		private List<BlockData> _lastSceneSnapshot = new List<BlockData>();
		private bool _suppressSceneSync = false;

		private List<LevelData> _allLevels = new List<LevelData>();
		private Vector2 _levelsScroll = Vector2.zero;

		private bool _isDirty = false;

		#endregion

		#region Compatibility Accessors

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

		private Bounds CalculateBounds()
		{
			return CalculateLevelBounds();
		}

		#endregion

		#region Initialization / Window Lifecycle

		[MenuItem("Tools/Tap Away Block Puzzle 3D/Level Editor")]
		public static void ShowWindow()
		{
			GetWindow<LevelEditorWindow>("Level Editor");
		}

		private void OnEnable()
		{
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

			EditorApplication.update -= EditorUpdate;
			EditorApplication.update += EditorUpdate;

			if (_autoOpenEditorScene)
			{
				try
				{
					OpenLevelInScene(singleMode: true);
				}
				catch (System.Exception)
				{
					// ignore
				}
			}

			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
		}

		private void OnDisable()
		{
			EditorApplication.update -= EditorUpdate;
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

			if (_isDirty && _currentLevel != null)
			{
				int choice = EditorUtility.DisplayDialogComplex("Unsaved changes", $"Save changes to {_currentLevel.name}?", "Save", "Don't Save", "Cancel");
				if (choice == 0) SaveChanges();
			}

			if (_editorSceneOpen) CloseEditorScene();
		}

		private void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.EnteredEditMode)
			{
				Debug.Log("Reinitializing Level Editor window after exiting Play Mode.");
				Close();
				EditorApplication.delayCall += ShowWindow;
			}
		}

		#endregion

		#region GUI

		private void OnGUI()
		{
			if (_previewBoxStyle == null)
			{
				_previewBoxStyle = new GUIStyle("box") { padding = new RectOffset(2, 2, 2, 2) };
			}

			bool isInPlayMode = EditorApplication.isPlayingOrWillChangePlaymode;
			GUI.enabled = !isInPlayMode;

			if (isInPlayMode)
			{
				EditorGUILayout.HelpBox("Editing is disabled in Play Mode. Exit Play Mode to edit levels.", MessageType.Warning);
			}

			DrawToolbar();
			EditorGUILayout.BeginHorizontal();
			DrawLevelListPanel();
			DrawEditorSettingsPanel();
			EditorGUILayout.EndHorizontal();

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
			EditorGUILayout.BeginVertical(GUILayout.Width(250));
			EditorGUILayout.LabelField("Levels", EditorStyles.boldLabel);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("New", GUILayout.Width(80))) CreateNewLevelAsset();
			if (_currentLevel != null && GUILayout.Button("Delete", GUILayout.Width(80)))
			{
				if (EditorUtility.DisplayDialog("Delete Level", $"Delete {_currentLevel.name}?", "Yes", "No"))
					DeleteCurrentLevelAsset();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space(5);

			_levelsScroll = EditorGUILayout.BeginScrollView(_levelsScroll, GUILayout.ExpandHeight(true));
			foreach (var lvl in _allLevels)
			{
				if (lvl == null) continue;
				bool isSelected = (lvl == _currentLevel);
				GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, padding = new RectOffset(10, 10, 5, 5) };

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
			EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
			EditorGUILayout.LabelField("Editor Settings", EditorStyles.boldLabel);

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

		#endregion

		#region Tool Logic

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
				ClearEditorScene();
			}
			else
			{
				Debug.LogWarning("Failed to delete level asset: " + path);
			}
		}

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
					return false;
				}
			}

			_currentLevel = newLevel;
			SyncLevelManager();

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

		#endregion

		#region Editor Scene Sync

		private void OpenLevelInScene(bool singleMode = true)
		{
			if (_editorSceneOpen) return;

			if (!System.IO.File.Exists(LevelEditorScenePath))
			{
				if (!EditorUtility.DisplayDialog("Scene missing", $"Cannot find scene at {LevelEditorScenePath}. Create or set the path correctly.", "OK")) return;
			}

			var openMode = OpenSceneMode.Single;
			_tempEditorScene = EditorSceneManager.OpenScene(LevelEditorScenePath, openMode);
			_editorSceneOpen = true;

			_sceneRootGO = GameObject.Find("LevelEditorSceneRoot");
			if (_sceneRootGO == null)
			{
				_sceneRootGO = new GameObject("LevelEditorSceneRoot");
				SceneManager.MoveGameObjectToScene(_sceneRootGO, _tempEditorScene);
			}

			PopulateEditorScene();
			Selection.activeGameObject = _sceneRootGO;
		}

		private void PopulateEditorScene()
		{
			if (_sceneRootGO == null)
			{
				_sceneRootGO = GameObject.Find("LevelEditorSceneRoot");
				if (_sceneRootGO == null)
				{
					_sceneRootGO = new GameObject("LevelEditorSceneRoot");
					if (EditorSceneManager.GetActiveScene().IsValid())
						SceneManager.MoveGameObjectToScene(_sceneRootGO, EditorSceneManager.GetActiveScene());
				}
			}

			_suppressSceneSync = true;

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

			if (Mathf.Approximately(_gridUnitSize, 0f)) _gridUnitSize = 0.5f;

			int blockIndex = 0;
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
					inst.name = $"Block_{blockIndex}";
					blockIndex++;

					if (inst.GetComponent<Collider>() == null) inst.AddComponent<BoxCollider>();

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

		private void CloseEditorScene()
		{
			if (!_editorSceneOpen) return;

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

			if (_sceneRootGO != null)
			{
				for (int i = _sceneRootGO.transform.childCount - 1; i >= 0; i--)
				{
					DestroyImmediate(_sceneRootGO.transform.GetChild(i).gameObject);
				}
				Debug.Log("All children of LevelEditorSceneRoot have been cleared.");
			}

			try
			{
				if (System.IO.File.Exists(GameScenePath))
				{
					EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
				}
			}
			catch (System.Exception)
			{
				// ignore
			}

			_editorSceneOpen = false;
			_sceneRootGO = null;
			_lastSceneSnapshot.Clear();
		}

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

		private void EditorUpdate()
		{
			try
			{
				if (!_editorSceneOpen || _suppressSceneSync) return;
				if (_sceneRootGO == null || _currentLevel == null) return;

				var newSnapshot = new List<BlockData>();
				for (int i = 0; i < _sceneRootGO.transform.childCount; i++)
				{
					Transform t = _sceneRootGO.transform.GetChild(i);
					if (t == null) continue;
					Vector3Int gridPos = Vector3Int.RoundToInt(t.position / Mathf.Max(0.0001f, _gridUnitSize));
					MoveDirection dir = GetDirectionFromForward(t.forward);
					newSnapshot.Add(new BlockData { position = gridPos, direction = dir, randomVisualRotation = Quaternion.identity });
				}

				if (!AreBlockListsEqual(_lastSceneSnapshot, newSnapshot))
				{
					var blocksList = _currentLevel.GetBlocks();
					if (blocksList != null)
					{
						blocksList.Clear();
						foreach (var b in newSnapshot) blocksList.Add(b);
						EditorUtility.SetDirty(_currentLevel);
						_isDirty = true;
					}
					_lastSceneSnapshot = new List<BlockData>(newSnapshot);
				}
			}
			catch (System.Exception ex)
			{
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

		#region Helpers

		private Bounds CalculateLevelBounds()
		{
			if (_sceneRootGO != null && _sceneRootGO.transform.childCount > 0)
			{
				var renderers = _sceneRootGO.GetComponentsInChildren<Renderer>();
				if (renderers != null && renderers.Length > 0)
				{
					var b = new Bounds(renderers[0].bounds.center, Vector3.zero);
					foreach (var r in renderers) if (r != null) b.Encapsulate(r.bounds);
					return b;
				}
				else
				{
					var bounds = new Bounds(_sceneRootGO.transform.GetChild(0).position, Vector3.zero);
					for (int i = 0; i < _sceneRootGO.transform.childCount; i++) bounds.Encapsulate(_sceneRootGO.transform.GetChild(i).position);
					return bounds;
				}
			}

			if (_currentLevel != null)
			{
				var blocks = _currentLevel.GetBlocks();
				if (blocks != null && blocks.Count > 0)
				{
					var firstWorld = (Vector3)blocks[0].position * _gridUnitSize;
					var b = new Bounds(firstWorld, Vector3.zero);
					foreach (var bl in blocks) b.Encapsulate((Vector3)bl.position * _gridUnitSize);
					return b;
				}
			}

			return new Bounds(Vector3.zero, Vector3.one * 5f);
		}

		private void FocusCamera()
		{
			Bounds bounds = CalculateLevelBounds();
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

#if UNITY_EDITOR
			if (UnityEditor.SceneView.lastActiveSceneView != null)
			{
				var sv = UnityEditor.SceneView.lastActiveSceneView;
				Vector3 center = bounds.center;
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

	[CustomEditor(typeof(LevelData))]
	public class CleanLevelDataEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
		}
	}
}