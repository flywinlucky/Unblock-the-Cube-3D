using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text gameMessage_Text;
    public Text countDown_Text;
    public GameObject players_UI_Canvas;
    public AudioManager audioManager;
    [Space]
    public GameObject ChoseGameModePanel;
    public Button singlePlayerGameMode;
    public Button multiplayerGameMode;
    public Button LocalMultiplayerGameMode;
    
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
            audioManager?.PlayCountdownClick(); // Play countdown click sound
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
            audioManager?.PlayCountdownClick(); // Play countdown click sound
            yield return new WaitForSeconds(interval);
        }
        countDown_Text.gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    private void Start()
    {
        // Add listeners for game mode buttons
        singlePlayerGameMode.onClick.AddListener(() =>
        {
            ChoseGameModePanel.SetActive(false);
            FindObjectOfType<GameManager>()?.StartSinglePlayerMode();
            FindObjectOfType<GameManager>()?.StartGame();
        });

        multiplayerGameMode.onClick.AddListener(() =>
        {
            ChoseGameModePanel.SetActive(false);
            FindObjectOfType<GameManager>()?.StartMultiplayerBotMode();
            FindObjectOfType<GameManager>()?.StartGame();
        });

        LocalMultiplayerGameMode.onClick.AddListener(() =>
        {
            ChoseGameModePanel.SetActive(false);
            FindObjectOfType<GameManager>()?.StartLocalMultiplayerMode();
            FindObjectOfType<GameManager>()?.StartGame();
        });
    }
}