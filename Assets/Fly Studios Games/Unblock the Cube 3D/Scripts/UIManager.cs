// UIManager.cs
using UnityEngine;
using UnityEngine.UI; // Folosim UI standard, nu TMPro
using System.Collections; // Necesar pentru Corutine

public class UIManager : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("Slider-ul care arată progresul eliminării blocurilor.")]
    public Slider progressBar;

    [Tooltip("Textul care afișează numărul nivelului curent.")]
    public Text currentLevelText; // Schimbat din TextMeshProUGUI în Text

    [Tooltip("Textul care afișează numărul nivelului următor.")]
    public Text nextLevelText; // Schimbat din TextMeshProUGUI în Text

    [Header("Animation Settings")]
    [Tooltip("Durata animației pentru umplerea barei de progres (în secunde).")]
    public float progressAnimationDuration = 0.25f;

    // O referință la corutina care rulează, pentru a o putea opri dacă este necesar
    private Coroutine _progressBarAnimation;

    /// <summary>
    /// Actualizează textele pentru nivelul curent și cel următor.
    /// </summary>
    /// <param name="levelNumber">Numărul nivelului curent.</param>
    public void UpdateLevelDisplay(int levelNumber)
    {
        if (currentLevelText != null)
        {
            currentLevelText.text = levelNumber.ToString();
        }

        if (nextLevelText != null)
        {
            nextLevelText.text = (levelNumber + 1).ToString();
        }
    }

    /// <summary>
    /// Inițiază animația de actualizare a slider-ului de progres.
    /// </summary>
    /// <param name="remainingBlocks">Numărul de blocuri rămase.</param>
    /// <param name="totalBlocks">Numărul total de blocuri la începutul nivelului.</param>
    public void UpdateProgressBar(int remainingBlocks, int totalBlocks)
    {
        if (progressBar == null) return;

        float targetProgress = 0f;
        if (totalBlocks > 0)
        {
            // Calculăm progresul ca procentaj de blocuri eliminate
            targetProgress = 1f - ((float)remainingBlocks / totalBlocks);
        }
        else
        {
            targetProgress = 1f; // Dacă nu există blocuri, progresul e complet
        }

        // Oprim orice animație anterioară pentru a porni una nouă
        if (_progressBarAnimation != null)
        {
            StopCoroutine(_progressBarAnimation);
        }

        // Pornim noua animație de interpolare
        _progressBarAnimation = StartCoroutine(AnimateProgressBar(targetProgress));
    }

    /// <summary>
    /// O corutină care animă valoarea slider-ului de la valoarea curentă la o valoare țintă.
    /// </summary>
    /// <param name="targetValue">Valoarea finală a slider-ului (între 0 și 1).</param>
    private IEnumerator AnimateProgressBar(float targetValue)
    {
        float startValue = progressBar.value;
        float elapsedTime = 0f;

        while (elapsedTime < progressAnimationDuration)
        {
            // Interpolăm liniar între valoarea de start și cea țintă
            progressBar.value = Mathf.Lerp(startValue, targetValue, elapsedTime / progressAnimationDuration);

            // Trecem la următorul frame
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // La final, ne asigurăm că slider-ul ajunge exact la valoarea țintă
        progressBar.value = targetValue;
    }
}