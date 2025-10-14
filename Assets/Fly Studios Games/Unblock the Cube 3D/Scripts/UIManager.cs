// UIManager.cs
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("Textul care afișează numărul nivelului curent.")]
    public Text currentLevelText; // Schimbat din TextMeshProUGUI în Text

    public GameObject levelWin_panel;
    public GameObject shop_panel;

    // NOU: Text pentru soldul global de coins și pentru suma afișată în panelul de win
    [Tooltip("Text care arată soldul global de coins.")]
    public Text globalCoinsText;
    [Tooltip("Text afișat în level win panel cu suma câștigată la acel win (ex: +5).")]
    public Text winCoinsText;

    [Header("PowerUp UI")]
    [Tooltip("Text care arată câte Undo avem.")]
    public Text undoCountText;
    [Tooltip("Text care arată câte Hint avem.")]
    public Text hintCountText;
    [Tooltip("Text care arată câte Smash avem.")]
    public Text smashCountText;

    [Header("Shop Buttons")]
    public Button buyUndoButton;
    public Button buyHintButton;
    public Button buySmashButton;

    [Header("Use Buttons")]
    public Button useUndoButton;
    public Button useHintButton;
    public Button useSmashButton;

    [Header("References")]
    [Tooltip("Referință către LevelManager pentru a comanda cumpărări/folosiri.")]
    public LevelManager levelManager;

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
        // Legăm butoanele shop la funcțiile din LevelManager (dacă sunt setate)
        if (buyUndoButton != null && levelManager != null)
        {
            buyUndoButton.onClick.RemoveAllListeners();
            buyUndoButton.onClick.AddListener(() => {
                if (levelManager.BuyUndo()) UpdatePowerUpCounts(levelManager.undoCount, levelManager.hintCount, levelManager.smashCount);
            });
        }
        if (buyHintButton != null && levelManager != null)
        {
            buyHintButton.onClick.RemoveAllListeners();
            buyHintButton.onClick.AddListener(() => {
                if (levelManager.BuyHint()) UpdatePowerUpCounts(levelManager.undoCount, levelManager.hintCount, levelManager.smashCount);
            });
        }
        if (buySmashButton != null && levelManager != null)
        {
            buySmashButton.onClick.RemoveAllListeners();
            buySmashButton.onClick.AddListener(() => {
                if (levelManager.BuySmash()) UpdatePowerUpCounts(levelManager.undoCount, levelManager.hintCount, levelManager.smashCount);
            });
        }

        // Legăm butoanele de folosire la funcțiile LevelManager
        if (useUndoButton != null && levelManager != null)
        {
            useUndoButton.onClick.RemoveAllListeners();
            useUndoButton.onClick.AddListener(() => { levelManager.UseUndo(); UpdatePowerUpCounts(levelManager.undoCount, levelManager.hintCount, levelManager.smashCount); });
        }
        if (useHintButton != null && levelManager != null)
        {
            useHintButton.onClick.RemoveAllListeners();
            useHintButton.onClick.AddListener(() => { levelManager.UseHint(); UpdatePowerUpCounts(levelManager.undoCount, levelManager.hintCount, levelManager.smashCount); });
        }
        if (useSmashButton != null && levelManager != null)
        {
            useSmashButton.onClick.RemoveAllListeners();
            // schimbăm comportamentul: apăsarea butonului pornește modul de selecție pentru a alege block-ul de distrus
            useSmashButton.onClick.AddListener(() => { levelManager.StartRemoveMode(); UpdatePowerUpCounts(levelManager.undoCount, levelManager.hintCount, levelManager.smashCount); });
        }

        // inițializăm afișajul contorilor
        if (levelManager != null)
            UpdatePowerUpCounts(levelManager.undoCount, levelManager.hintCount, levelManager.smashCount);
    }

    // NOU: actualizează textele power-up
    public void UpdatePowerUpCounts(int undo, int hint, int smash)
    {
        if (undoCountText != null) undoCountText.text = undo.ToString();
        if (hintCountText != null) hintCountText.text = hint.ToString();
        if (smashCountText != null) smashCountText.text = smash.ToString();
    }
}