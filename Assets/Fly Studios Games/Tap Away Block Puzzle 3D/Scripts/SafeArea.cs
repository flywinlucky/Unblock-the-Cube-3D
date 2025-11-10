using UnityEngine;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Applies the device safe area to a RectTransform so UI fits notches and cutouts.
    /// Attach to a GameObject with a RectTransform. Optionally assign a different RectTransform in the inspector.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        #region Inspector

        [Header("Safe Area Target")]
        [Tooltip("Optional RectTransform to apply the safe area to. If null, the component's RectTransform will be used.")]
        public RectTransform targetRectTransform;

        #endregion

        #region Private Fields

        private RectTransform _panel;
        private Rect _lastSafeArea = Rect.zero;

        #endregion

        #region MonoBehaviour

        private void Awake()
        {
            // Prefer an explicitly assigned RectTransform, otherwise fall back to the component's RectTransform.
            _panel = targetRectTransform != null ? targetRectTransform : GetComponent<RectTransform>();

            if (_panel == null)
            {
                Debug.LogError("SafeArea requires a RectTransform reference.", this);
                enabled = false;
                return;
            }

            // Apply safe area immediately on awake.
            ApplySafeArea();
        }

        private void Update()
        {
            // Only re-apply when safe area changes (fast check).
            if (Screen.safeArea != _lastSafeArea)
            {
                ApplySafeArea();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Calculates and applies anchors to match the device's safe area (in normalized 0..1 space).
        /// </summary>
        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;

            // Convert safe area pixels to normalized anchor coordinates.
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _panel.anchorMin = anchorMin;
            _panel.anchorMax = anchorMax;

            _lastSafeArea = safeArea;
        }

        #endregion
    }
}