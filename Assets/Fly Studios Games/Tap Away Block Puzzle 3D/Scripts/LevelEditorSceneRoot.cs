using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Helper to hide the level editor root when entering Play Mode in the Editor,
    /// and re-enable it when returning to Edit Mode.
    /// </summary>
    public class LevelEditorSceneRoot : MonoBehaviour
    {
        [Tooltip("Root GameObject that contains level editor objects. This will be disabled in Play Mode.")]
        public GameObject levelEditorLevelRoot;

#if UNITY_EDITOR
        private void OnEnable()
        {
            // Listen to PlayMode state changes
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            // Clean up event subscription
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (levelEditorLevelRoot == null) return;

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                levelEditorLevelRoot.SetActive(false);
                Debug.Log("Level Editor Root disabled (Play Mode)");
            }

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                levelEditorLevelRoot.SetActive(true);
                Debug.Log("Level Editor Root re-enabled (Edit Mode)");
            }
        }
#endif
    }
}