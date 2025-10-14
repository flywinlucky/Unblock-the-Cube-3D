// UIManager.cs
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("Textul care afișează numărul nivelului curent.")]
    public Text currentLevelText; // Schimbat din TextMeshProUGUI în Text

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
}