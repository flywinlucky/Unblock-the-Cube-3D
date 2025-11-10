using UnityEngine;
using UnityEngine.UI;

namespace Tap_Away_Block_Puzzle_3D
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI Components")]
        [Tooltip("Textul care afișează numărul nivelului curent.")]
        public Text currentLevelText; // Schimbat din TextMeshProUGUI în Text
        public Slider currentProgresSlider;

        public GameObject levelWin_panel;
        public GameObject shop_panel;
        public GameObject safeAreaUI;

        // NOU: Text pentru soldul global de coins și pentru suma afișată în panelul de win
        [Tooltip("Text care arată soldul global de coins.")]
        public Text globalCoinsText;
        [Tooltip("Text afișat în level win panel cu suma câștigată la acel win (ex: +5).")]
        public Text winCoinsText;

        [Header("PowerUp UI")]
        [Tooltip("Text care arată câte Smash avem.")]
        public Text smashCountText;

        [Header("Shop Buttons")]
        public Button buySmashButton;

        [Header("Use Buttons")]
        public Button useSmashButton;

        [Header("References")]
        [Tooltip("Referință către LevelManager pentru a comanda cumpărări/folosiri.")]
        public LevelManager levelManager;

        [Header("Shop Price Labels")]
        [Tooltip("Text care afișează prețul pentru Smash în shop.")]
        public Text buySmashPriceText;

        [Header("Shop Buttons (Open/Close)")]
        [Tooltip("Buton care deschide shop panel-ul.")]
        public Button openShopButton;
        [Tooltip("Buton care închide shop panel-ul.")]
        public Button closeShopButton;

        // Smooth progress animation
        private Coroutine _progressCoroutine;
        private float _progressAnimDuration = 0.25f; // secunde

        /// <summary>
        /// Actualizează textele pentru nivelul curent.
        /// </summary>
        /// <param name="levelNumber">Numărul nivelului curent.</param>
        public void UpdateLevelDisplay(int levelNumber)
        {
            if (currentLevelText != null)
            {
                currentLevelText.text = levelNumber.ToString();
            }
        }

        // NOU: actualizări UI pentru coins
        public void UpdateGlobalCoinsDisplay(int totalCoins)
        {
            if (globalCoinsText != null)
            {
                globalCoinsText.text = totalCoins.ToString();
            }
        }

        public void UpdateWinCoinsDisplay(int gainedCoins)
        {
            if (winCoinsText != null)
            {
                winCoinsText.text = (gainedCoins >= 0 ? "+" : "") + gainedCoins.ToString();
            }
        }

        private void Start()
        {
            shop_panel.SetActive(false);
            safeAreaUI.SetActive(true);

            // Legăm butoanele shop la funcțiile din LevelManager (dacă sunt setate)
            if (buySmashButton != null && levelManager != null)
            {
                buySmashButton.onClick.RemoveAllListeners();
                buySmashButton.onClick.AddListener(() =>
                {
                    if (levelManager.BuySmash()) UpdatePowerUpCounts(levelManager.smashCount);
                });
            }

            // Legăm butoanele de folosire la funcțiile LevelManager
            if (useSmashButton != null && levelManager != null)
            {
                useSmashButton.onClick.RemoveAllListeners();
                // apăsarea butonului pornește modul de selecție pentru a alege block-ul de distrus
                useSmashButton.onClick.AddListener(() => { levelManager.StartRemoveMode(); UpdatePowerUpCounts(levelManager.smashCount); });
            }

            // Legăm butoanele de open/close shop (dacă sunt setate)
            if (openShopButton != null)
            {
                openShopButton.onClick.RemoveAllListeners();
                openShopButton.onClick.AddListener(() => OpenShopPanel());
            }
            if (closeShopButton != null)
            {
                closeShopButton.onClick.RemoveAllListeners();
                closeShopButton.onClick.AddListener(() => CloseShopPanel());
            }

            // inițializăm afișajul contorilor (doar smash)
            if (levelManager != null)
                UpdatePowerUpCounts(levelManager.smashCount);

            // setăm prețurile pe butoane (dacă există)
            if (levelManager != null)
            {
                if (buySmashPriceText != null) buySmashPriceText.text = levelManager.smashCost.ToString();
            }
        }

        // NOU: actualizează textele power-up (doar smash)
        public void UpdatePowerUpCounts(int smash)
        {
            if (smashCountText != null) smashCountText.text = smash.ToString();
        }

        // NOU: deschide shop panel-ul
        public void OpenShopPanel()
        {
            if (shop_panel != null) shop_panel.SetActive(true);
            // sincronizare cu LevelManager (opțional)
            if (levelManager != null) levelManager.OpenShop();
            safeAreaUI.SetActive(false);
            levelManager.levelContainer.gameObject.SetActive(false);
        }

        // NOU: închide shop panel-ul
        public void CloseShopPanel()
        {
            if (shop_panel != null) shop_panel.SetActive(false);
            // sincronizare cu LevelManager (opțional)
            if (levelManager != null) levelManager.CloseShop();
            safeAreaUI.SetActive(true);
            levelManager.levelContainer.gameObject.SetActive(true);
        }

        // NOU: initializează progresul la startul nivelului
        public void InitProgress(int maxBlocks)
        {
            if (currentProgresSlider == null) return;
            currentProgresSlider.wholeNumbers = false;
            currentProgresSlider.maxValue = Mathf.Max(1, maxBlocks); // evităm zero ca max
            currentProgresSlider.value = 0;
            // opțional: afișare inițială sau text asociat
        }

        // NOU: actualizează progresul pe baza numerelor (mai robust decât increment local)
        // totalBlocks = numărul inițial de blocuri din nivel
        // remainingBlocks = câte blocuri mai sunt active în scenă
        public void UpdateProgressByCounts(int totalBlocks, int remainingBlocks)
        {
            if (currentProgresSlider == null) return;

            int safeTotal = Mathf.Max(1, totalBlocks);
            // calculează valoarea pe slider (câte blocuri au fost eliminate)
            float target = Mathf.Clamp(safeTotal - remainingBlocks, 0f, safeTotal);

            // asigurăm maxValue corect (în caz că nu a fost apelat InitProgress)
            currentProgresSlider.maxValue = safeTotal;

            StartProgressAnimation(target);
        }

        // NOU: setează instant valoarea slider-ului (fără animație)
        public void SetProgressImmediate(float value)
        {
            if (currentProgresSlider == null) return;
            if (_progressCoroutine != null) StopCoroutine(_progressCoroutine);
            currentProgresSlider.value = Mathf.Clamp(value, 0f, currentProgresSlider.maxValue);
        }

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
    }
}