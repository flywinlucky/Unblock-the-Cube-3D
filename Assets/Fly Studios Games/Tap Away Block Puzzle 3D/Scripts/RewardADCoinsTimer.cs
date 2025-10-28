using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RewardADCoinsTimer : MonoBehaviour
{
    public GameObject rewardAdCoinsPopup;
    public float firstActivationTimer = 10f; // Timer pentru prima activare
    public float activateAfter = 30f;
    public float dezactivateAfter = 10f;
    public Text timerText;

    private Coroutine _timerCoroutine;
    private bool _isFirstActivation = true; // Flag pentru prima activare

    void Start()
    {
        if (rewardAdCoinsPopup == null)
        {
            Debug.LogError("RewardADCoinsTimer: rewardAdCoinsPopup is not assigned.");
            enabled = false;
            return;
        }

        if (timerText == null)
        {
            Debug.LogError("RewardADCoinsTimer: timerText is not assigned.");
            enabled = false;
            return;
        }

        // Dezactivăm popup-ul la început
        rewardAdCoinsPopup.SetActive(false);

        // Pornim ciclul de activare/dezactivare
        StartCoroutine(RewardAdLoop());
    }

    private IEnumerator RewardAdLoop()
    {
        // Așteptăm timpul pentru prima activare
        if (_isFirstActivation)
        {
            yield return new WaitForSeconds(firstActivationTimer);
        }

        while (true)
        {
            // Activăm popup-ul
            rewardAdCoinsPopup.SetActive(true);

            // Pornim countdown-ul
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
            }
            _timerCoroutine = StartCoroutine(StartCountdown(dezactivateAfter));

            // Așteptăm timpul de dezactivare
            yield return new WaitForSeconds(dezactivateAfter);

            // Dezactivăm popup-ul
            rewardAdCoinsPopup.SetActive(false);

            // După prima activare, setăm flag-ul
            _isFirstActivation = false;

            // Așteptăm timpul de activare pentru ciclurile următoare
            yield return new WaitForSeconds(activateAfter);
        }
    }

    private IEnumerator StartCountdown(float duration)
    {
        float remainingTime = duration;

        while (remainingTime > 0)
        {
            // Calculăm minutele și secundele
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);

            // Actualizăm textul timerului
            timerText.text = $"{minutes:D2}:{seconds:D2}";

            // Așteptăm un frame și scădem timpul rămas
            yield return null;
            remainingTime -= Time.deltaTime;
        }

        // Resetăm textul timerului la final
        timerText.text = "00:00";
    }
}