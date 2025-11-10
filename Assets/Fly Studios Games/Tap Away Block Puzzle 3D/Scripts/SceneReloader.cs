using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Utility to reload the currently active scene. Can be invoked from UI buttons or other scripts.
    /// </summary>
    public class SceneReloader : MonoBehaviour
    {
        /// <summary>
        /// Reloads the currently active scene by build index.
        /// </summary>
        public void RestartCurrentScene()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(currentSceneIndex);
        }
    }
}