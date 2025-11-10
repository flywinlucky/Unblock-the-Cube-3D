using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Manages background images and switches them periodically based on level number.
    /// </summary>
    public class BackgroundImageManager : MonoBehaviour
    {
        #region Inspector

        [Tooltip("Array of background sprites to choose from.")]
        public Sprite[] backgrounds;

        [Tooltip("Change background every N levels (e.g. 5).")]
        public int changeEveryLovelCount = 5; // keep original public name to avoid inspector links

        [Tooltip("UI Image component used for displaying the background.")]
        public Image backgroundImage;

        #endregion

        private int _currentLevel = 0;
        private Sprite _lastBackground;

        /// <summary>
        /// Update background based on provided level number.
        /// </summary>
        public void UpdateBackground(int levelNumber)
        {
            _currentLevel = levelNumber;

            if (_currentLevel % changeEveryLovelCount == 0)
            {
                Sprite newBackground = GetRandomBackground();
                if (newBackground != null && backgroundImage != null)
                {
                    backgroundImage.sprite = newBackground;
                    _lastBackground = newBackground;
                }
            }
        }

        /// <summary>
        /// Returns a random background avoiding immediate repetition of the last used sprite.
        /// </summary>
        private Sprite GetRandomBackground()
        {
            if (backgrounds == null || backgrounds.Length == 0)
            {
                Debug.LogWarning("BackgroundImageManager: No backgrounds available.");
                return null;
            }

            List<Sprite> availableBackgrounds = new List<Sprite>(backgrounds);

            if (_lastBackground != null)
            {
                availableBackgrounds.Remove(_lastBackground);
            }

            if (availableBackgrounds.Count > 0)
            {
                int randomIndex = Random.Range(0, availableBackgrounds.Count);
                return availableBackgrounds[randomIndex];
            }

            return null;
        }
    }
}