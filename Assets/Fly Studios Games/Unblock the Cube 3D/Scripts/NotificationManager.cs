using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour
{
    public GameObject notificationPopUp;
    public Text notificationTextMesage;

    private Coroutine _hideCoroutine;

    private void Awake()
    {
        // Asigurăm popup-ul dezactivat la start
        if (notificationPopUp != null)
            notificationPopUp.SetActive(false);
    }

    /// <summary>
    /// Afișează o notificare pentru o durată specificată (seconds). Popup este activat automat și apoi dezactivat.
    /// </summary>
    public void ShowNotification(string message, float duration = 2f)
    {
        if (notificationPopUp == null || notificationTextMesage == null)
        {
            Debug.LogWarning("NotificationManager: popup or text not assigned.");
            return;
        }

        // Oprire coroutine anterioară dacă exista
        if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);

        notificationTextMesage.text = message;
        notificationPopUp.SetActive(true);
        _hideCoroutine = StartCoroutine(HideAfterDelay(duration));
    }

    /// <summary>
    /// Dezactivează imediat notificarea curentă.
    /// </summary>
    public void HideNotification()
    {
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
        }
        if (notificationPopUp != null) notificationPopUp.SetActive(false);
    }

    private IEnumerator HideAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (notificationPopUp != null) notificationPopUp.SetActive(false);
        _hideCoroutine = null;
    }
}
