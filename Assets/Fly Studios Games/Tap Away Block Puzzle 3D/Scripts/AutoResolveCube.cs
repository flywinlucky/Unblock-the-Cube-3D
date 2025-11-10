using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Automatically simulates clicks on blocks under levelTarget to "resolve" the level.
    /// Useful for debugging or demo playback.
    /// </summary>
    public class AutoResolveCube : MonoBehaviour
    {
        #region Inspector

        [Tooltip("Transform whose children (blocks) will be auto-clicked.")]
        public Transform levelTarget;

        [Tooltip("Button to start auto-resolve.")]
        public Button playAutoLevelResolve_Button;

        [Tooltip("Button to stop auto-resolve.")]
        public Button stopAutoLevelResolve_Button;

        [Tooltip("Time interval (seconds) between each block move/click.")]
        public float moveInterval = 1.0f;

        #endregion

        private bool _isResolving = false;

        private void Start()
        {
            if (playAutoLevelResolve_Button != null)
            {
                playAutoLevelResolve_Button.onClick.AddListener(StartAutoResolve);
            }
            if (stopAutoLevelResolve_Button != null)
            {
                stopAutoLevelResolve_Button.onClick.AddListener(StopAutoResolve);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                StartAutoResolve();
            }
            if (Input.GetKeyDown(KeyCode.Z))
            {
                StopAutoResolve();
            }
        }

        /// <summary>
        /// Start auto-resolve coroutine (if not already running).
        /// </summary>
        public void StartAutoResolve()
        {
            if (!_isResolving)
            {
                StartCoroutine(ResolveCube());
            }
        }

        /// <summary>
        /// Stop auto-resolve immediately.
        /// </summary>
        public void StopAutoResolve()
        {
            StopAllCoroutines();
            _isResolving = false;
        }

        private IEnumerator ResolveCube()
        {
            _isResolving = true;

            while (levelTarget != null && levelTarget.childCount > 0)
            {
                for (int i = 0; i < levelTarget.childCount; i++)
                {
                    Transform child = levelTarget.GetChild(i);
                    if (child == null) continue;

                    Block block = child.GetComponent<Block>();
                    if (block != null)
                    {
                        // Simulate a click to force the block to move
                        block.SendMessage("OnMouseUpAsButton", SendMessageOptions.DontRequireReceiver);
                    }

                    yield return new WaitForSeconds(moveInterval);
                }
            }

            _isResolving = false;
            Debug.Log("Auto-resolve stopped: no more blocks.");
        }
    }
}