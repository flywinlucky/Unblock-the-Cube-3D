using UnityEngine;
using UnityEngine.UI; // Ensure Text component is available
using System.Collections;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Calculates and displays FPS in a UI Text component.
    /// </summary>
    public class FpsCounter : MonoBehaviour
    {
        #region Inspector

        [Header("UI")]
        [SerializeField]
        [Tooltip("UI Text component where FPS will be displayed. Assign from Canvas.")]
        private Text fpsText;

        [Header("Settings")]
        [SerializeField]
        [Tooltip("How often to update the displayed value in seconds. e.g. 0.5 = twice per second.")]
        private float updateInterval = 0.5f;

        #endregion

        // Internal counters for FPS calculation
        private float accumulatedTime = 0f;
        private int frameCount = 0;
        private float currentFps;

        private void Start()
        {
            if (fpsText == null)
            {
                Debug.LogError("FPS Text component is not assigned in the Inspector!", this);
                enabled = false;
                return;
            }
        }

        private void Update()
        {
            accumulatedTime += Time.unscaledDeltaTime;
            frameCount++;

            if (accumulatedTime >= updateInterval)
            {
                currentFps = frameCount / accumulatedTime;
                fpsText.text = "FPS: " + currentFps.ToString("F0");
                accumulatedTime = 0f;
                frameCount = 0;
            }
        }
    }

}