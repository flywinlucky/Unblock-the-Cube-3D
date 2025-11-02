using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text gameMessage_Text;
    public Text countDown_Text;

    public void UpdateGameMessage(string message)
    {
        gameMessage_Text.text = message;
    }

    public void StartCountdown(int startValue, System.Action onComplete)
    {
        StartCoroutine(CountdownRoutine(startValue, onComplete));
    }

    private IEnumerator CountdownRoutine(int startValue, System.Action onComplete)
    {
        countDown_Text.gameObject.SetActive(true);
        for (int i = startValue; i > 0; i--)
        {
            countDown_Text.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }
        countDown_Text.gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    public void StartCountUp(int endValue, float interval, System.Action onComplete)
    {
        StartCoroutine(CountUpRoutine(endValue, interval, onComplete));
    }

    private IEnumerator CountUpRoutine(int endValue, float interval, System.Action onComplete)
    {
        countDown_Text.gameObject.SetActive(true);
        for (int i = 1; i <= endValue; i++)
        {
            countDown_Text.text = i.ToString();
            yield return new WaitForSeconds(interval);
        }
        countDown_Text.gameObject.SetActive(false);
        onComplete?.Invoke();
    }
}