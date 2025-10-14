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
}