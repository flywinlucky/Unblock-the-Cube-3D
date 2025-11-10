using UnityEngine;
using UnityEngine.UI;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Manages the main UI: level text, progress slider, shop and power-up UI updates.
    /// Keeps logic identical but improves inspector documentation and code organization.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Inspector - General UI

        [Header("UI Components")]
        [Tooltip("Text that displays the current level number.")]
        public Text currentLevelText;

        [Tooltip("Slider that displays current progress.")]
        public Slider currentProgresSlider;

        [Tooltip("Panel shown when level is won.")]
        public GameObject levelWin_panel;

        [Tooltip("Shop panel GameObject.")]
        public GameObject shop_panel;

        [Tooltip("Safe area UI container (hidden when shop opens).")]
        public GameObject safeAreaUI;

        #endregion

        #region Inspector - Coins & Win UI

        [Tooltip("Text showing the global coins balance.")]
        public Text globalCoinsText;

        [Tooltip("Text shown in the win panel indicating coins gained (e.g. +5).")]
        public Text winCoinsText;

        #endregion

        #region Inspector - PowerUp UI

        [Header("PowerUp UI")]
        [Tooltip("Text that shows how many Smash power-ups the player has.")]
        public Text smashCountText;

        #endregion

        #region Inspector - Shop Buttons / References

        [Header("Shop Buttons")]
        public Button buySmashButton;

        [Header("Use Buttons")]
        public Button useSmashButton;

        [Header("References")]
        [Tooltip("Reference to the LevelManager to forward buy/use requests.")]
        public LevelManager levelManager;

        [Header("Shop Price Labels")]
        [Tooltip("Text showing the Smash price in the shop.")]
        public Text buySmashPriceText;

        [Header("Shop Buttons (Open/Close)")]
        [Tooltip("Button that opens the shop panel.")]
        public Button openShopButton;

        [Tooltip("Button that closes the shop panel.")]
        public Button closeShopButton;

        #endregion

        #region Private - Progress Animation

        private Coroutine _progressCoroutine;
        private float _progressAnimDuration = 0.25f;

        #endregion

        #region Public API - UI Updates

        /// <summary>
        /// Updates the level display text.
        /// </summary>
        public void UpdateLevelDisplay(int levelNumber)
        {
            if (currentLevelText != null)
            {
                currentLevelText.text = levelNumber.ToString();
            }
        }

        /// <summary>
        /// Updates the global coins balance text.
        /// </summary>
        public void UpdateGlobalCoinsDisplay(int totalCoins)
        {
            if (globalCoinsText != null)
            {
                globalCoinsText.text = totalCoins.ToString();
            }
        }

        /// <summary>
        /// Updates the win panel coins text (prefixed with + when positive).
        /// </summary>
        public void UpdateWinCoinsDisplay(int gainedCoins)
        {
            if (winCoinsText != null)
            {
                winCoinsText.text = (gainedCoins >= 0 ? "+" : "") + gainedCoins.ToString();
            }
        }

        #endregion

        #region MonoBehaviour - Setup

        private void Start()
        {
            if (shop_panel != null) shop_panel.SetActive(false);
            if (safeAreaUI != null) safeAreaUI.SetActive(true);

            // Bind shop buy button
            if (buySmashButton != null && levelManager != null)
            {
                buySmashButton.onClick.RemoveAllListeners();
                buySmashButton.onClick.AddListener(() =>
                {
                    if (levelManager.BuySmash())
                        UpdatePowerUpCounts(levelManager.smashCount);
                });
            }

            // Bind use button to start remove mode
            if (useSmashButton != null && levelManager != null)
            {
                useSmashButton.onClick.RemoveAllListeners();
                useSmashButton.onClick.AddListener(() =>
                {
                    levelManager.StartRemoveMode();
                    UpdatePowerUpCounts(levelManager.smashCount);
                });
            }

            // Open/Close shop buttons
            if (openShopButton != null)
            {
                openShopButton.onClick.RemoveAllListeners();
                openShopButton.onClick.AddListener(OpenShopPanel);
            }
            if (closeShopButton != null)
            {
                closeShopButton.onClick.RemoveAllListeners();
                closeShopButton.onClick.AddListener(CloseShopPanel);
            }

            // Initialize displays from LevelManager if present
            if (levelManager != null)
            {
                UpdatePowerUpCounts(levelManager.smashCount);
                if (buySmashPriceText != null) buySmashPriceText.text = levelManager.smashCost.ToString();
            }
        }

        #endregion

        #region Public - Shop / PowerUp Helpers

        public void UpdatePowerUpCounts(int smash)
        {
            if (smashCountText != null) smashCountText.text = smash.ToString();
        }

        public void OpenShopPanel()
        {
            if (shop_panel != null) shop_panel.SetActive(true);
            if (levelManager != null) levelManager.OpenShop();
            if (safeAreaUI != null) safeAreaUI.SetActive(false);
            if (levelManager != null && levelManager.levelContainer != null) levelManager.levelContainer.gameObject.SetActive(false);
        }

        public void CloseShopPanel()
        {
            if (shop_panel != null) shop_panel.SetActive(false);
            if (levelManager != null) levelManager.CloseShop();
            if (safeAreaUI != null) safeAreaUI.SetActive(true);
            if (levelManager != null && levelManager.levelContainer != null) levelManager.levelContainer.gameObject.SetActive(true);
        }

        /// <summary>
        /// Initialize the progress slider for the level.
        /// </summary>
        public void InitProgress(int maxBlocks)
        {
            if (currentProgresSlider == null) return;
            currentProgresSlider.wholeNumbers = false;
            currentProgresSlider.maxValue = Mathf.Max(1, maxBlocks);
            currentProgresSlider.value = 0;
        }

        /// <summary>
        /// Updates progress based on total and remaining block counts.
        /// </summary>
        public void UpdateProgressByCounts(int totalBlocks, int remainingBlocks)
        {
            if (currentProgresSlider == null) return;

            int safeTotal = Mathf.Max(1, totalBlocks);
            float target = Mathf.Clamp(safeTotal - remainingBlocks, 0f, safeTotal);
            currentProgresSlider.maxValue = safeTotal;
            StartProgressAnimation(target);
        }

        public void SetProgressImmediate(float value)
        {
            if (currentProgresSlider == null) return;
            if (_progressCoroutine != null) StopCoroutine(_progressCoroutine);
            currentProgresSlider.value = Mathf.Clamp(value, 0f, currentProgresSlider.maxValue);
        }

        #endregion

        #region Private - Progress Animation

        private void StartProgressAnimation(float targetValue)
        {
            if (currentProgresSlider == null) return;
            if (_progressCoroutine != null) StopCoroutine(_progressCoroutine);
            _progressCoroutine = StartCoroutine(AnimateSlider(currentProgresSlider.value, targetValue, _progressAnimDuration));
        }

        private System.Collections.IEnumerator AnimateSlider(float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float v = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / duration));
                currentProgresSlider.value = v;
                yield return null;
            }
            currentProgresSlider.value = to;
            _progressCoroutine = null;
        }

        #endregion
    }
}