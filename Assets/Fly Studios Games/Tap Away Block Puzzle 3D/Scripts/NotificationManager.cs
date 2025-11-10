using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Simple notification popup with fade in/out.
    /// </summary>
    public class NotificationManager : MonoBehaviour
    {
        [Tooltip("Popup root GameObject for the notification.")]
        public GameObject notificationPopUp;

        [Tooltip("Text component used to show the notification message.")]
        public Text notificationTextMesage;

        [Header("Fade Settings")]
        [Tooltip("Time (s) for fade-in.")]
        public float fadeInTime = 0.25f;
        [Tooltip("Time (s) for fade-out.")]
        public float fadeOutTime = 0.25f;

        private Coroutine _hideCoroutine;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            if (notificationPopUp != null)
                notificationPopUp.SetActive(false);

            if (notificationPopUp != null)
            {
                _canvasGroup = notificationPopUp.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = notificationPopUp.AddComponent<CanvasGroup>();

                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Show a notification with a message for the given duration (seconds).
        /// </summary>
        public void ShowNotification(string message, float duration = 2f)
        {
            if (notificationPopUp == null || notificationTextMesage == null)
            {
                Debug.LogWarning("NotificationManager: popup or text not assigned.");
                return;
            }

            if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);

            notificationTextMesage.text = message;
            notificationPopUp.SetActive(true);

            if (_canvasGroup == null)
                _canvasGroup = notificationPopUp.GetComponent<CanvasGroup>() ?? notificationPopUp.AddComponent<CanvasGroup>();

            _hideCoroutine = StartCoroutine(FadeInThenOut(duration));
        }

        /// <summary>
        /// Immediately hide the current notification (quick fade-out).
        /// </summary>
        public void HideNotification()
        {
            if (notificationPopUp == null) return;

            if (_hideCoroutine != null)
            {
                StopCoroutine(_hideCoroutine);
                _hideCoroutine = null;
            }

            if (_canvasGroup == null) _canvasGroup = notificationPopUp.GetComponent<CanvasGroup>();
            if (_canvasGroup != null)
            {
                StartCoroutine(FadeOutAndDisable(fadeOutTime * 0.6f));
            }
            else
            {
                notificationPopUp.SetActive(false);
            }
        }

        private IEnumerator HideAfterDelay(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (notificationPopUp != null) notificationPopUp.SetActive(false);
            _hideCoroutine = null;
        }

        private IEnumerator FadeInThenOut(float totalDuration)
        {
            float t = 0f;
            if (_canvasGroup == null) _canvasGroup = notificationPopUp.GetComponent<CanvasGroup>();
            while (t < fadeInTime)
            {
                t += Time.deltaTime;
                if (_canvasGroup != null) _canvasGroup.alpha = Mathf.Clamp01(t / fadeInTime);
                yield return null;
            }
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            float stay = Mathf.Max(0f, totalDuration - fadeInTime - fadeOutTime);
            if (stay > 0f) yield return new WaitForSeconds(stay);

            yield return FadeOutAndDisableCoroutine(fadeOutTime);
            _hideCoroutine = null;
        }

        private IEnumerator FadeOutAndDisableCoroutine(float outTime)
        {
            if (_canvasGroup == null) _canvasGroup = notificationPopUp.GetComponent<CanvasGroup>();
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;

                float startAlpha = _canvasGroup.alpha;
                float t = 0f;
                while (t < outTime)
                {
                    t += Time.deltaTime;
                    _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t / outTime);
                    yield return null;
                }

                if (notificationPopUp != null) notificationPopUp.SetActive(false);
                _canvasGroup.alpha = 0f;
            }
            else
            {
                if (notificationPopUp != null) notificationPopUp.SetActive(false);
            }
        }

        private IEnumerator FadeOutAndDisable(float outTime)
        {
            yield return FadeOutAndDisableCoroutine(outTime);
        }
    }
}