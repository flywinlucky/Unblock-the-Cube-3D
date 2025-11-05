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
    public Text playerGameStats;
    [Space]
    public Text rounds_Text;
    public GameObject twoPlayersResultPanel;
    public Text player_1_Result_Text;
    public Text player_2_Result_Text;

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

        UpdatePlayerStatsDisplay();
        if (twoPlayersResultPanel != null) twoPlayersResultPanel.SetActive(false);
        if (rounds_Text != null) rounds_Text.gameObject.SetActive(false);
    }

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

    // -------------------------
    // Statistici jucători
    // Single Player keys
    private const string SP_GAMES = "SP_Games";
    private const string SP_CORRECT = "SP_Correct";
    private const string SP_TOTAL_RT = "SP_TotalRT";

    // Local Multiplayer keys (player1/player2)
    private const string MP_P1_GAMES = "MP_P1_Games";
    private const string MP_P1_CORRECT = "MP_P1_Correct";
    private const string MP_P1_TOTAL_RT = "MP_P1_TotalRT";
    private const string MP_P2_GAMES = "MP_P2_Games";
    private const string MP_P2_CORRECT = "MP_P2_Correct";
    private const string MP_P2_TOTAL_RT = "MP_P2_TotalRT";

    // Bot Multiplayer keys
    private const string BOT_P1_GAMES = "BOT_P1_Games";
    private const string BOT_P1_CORRECT = "BOT_P1_Correct";
    private const string BOT_P1_TOTAL_RT = "BOT_P1_TotalRT";
    private const string BOT_BOT_GAMES = "BOT_BOT_Games";
    private const string BOT_BOT_CORRECT = "BOT_BOT_Correct";
    private const string BOT_BOT_TOTAL_RT = "BOT_BOT_TotalRT";

    public void RecordSinglePlayerResult(bool correct, float reactionTime)
    {
        int games = PlayerPrefs.GetInt(SP_GAMES, 0) + 1;
        int corrects = PlayerPrefs.GetInt(SP_CORRECT, 0) + (correct ? 1 : 0);
        float totalRT = PlayerPrefs.GetFloat(SP_TOTAL_RT, 0f) + reactionTime;
        PlayerPrefs.SetInt(SP_GAMES, games);
        PlayerPrefs.SetInt(SP_CORRECT, corrects);
        PlayerPrefs.SetFloat(SP_TOTAL_RT, totalRT);
        PlayerPrefs.Save();
        UpdatePlayerStatsDisplay();
    }

    public void RecordLocalMultiplayerResult(int playerIndex, bool correct, float reactionTime)
    {
        if (playerIndex == 1)
        {
            int games = PlayerPrefs.GetInt(MP_P1_GAMES, 0) + 1;
            int corrects = PlayerPrefs.GetInt(MP_P1_CORRECT, 0) + (correct ? 1 : 0);
            float totalRT = PlayerPrefs.GetFloat(MP_P1_TOTAL_RT, 0f) + reactionTime;
            PlayerPrefs.SetInt(MP_P1_GAMES, games);
            PlayerPrefs.SetInt(MP_P1_CORRECT, corrects);
            PlayerPrefs.SetFloat(MP_P1_TOTAL_RT, totalRT);
        }
        else
        {
            int games = PlayerPrefs.GetInt(MP_P2_GAMES, 0) + 1;
            int corrects = PlayerPrefs.GetInt(MP_P2_CORRECT, 0) + (correct ? 1 : 0);
            float totalRT = PlayerPrefs.GetFloat(MP_P2_TOTAL_RT, 0f) + reactionTime;
            PlayerPrefs.SetInt(MP_P2_GAMES, games);
            PlayerPrefs.SetInt(MP_P2_CORRECT, corrects);
            PlayerPrefs.SetFloat(MP_P2_TOTAL_RT, totalRT);
        }
        PlayerPrefs.Save();
        UpdatePlayerStatsDisplay();
    }

    public void RecordMultiplayerBotResult(int playerIndex, bool correct, float reactionTime)
    {
        // playerIndex == 1 -> human player, playerIndex == 2 -> bot
        if (playerIndex == 1)
        {
            int games = PlayerPrefs.GetInt(BOT_P1_GAMES, 0) + 1;
            int corrects = PlayerPrefs.GetInt(BOT_P1_CORRECT, 0) + (correct ? 1 : 0);
            float totalRT = PlayerPrefs.GetFloat(BOT_P1_TOTAL_RT, 0f) + reactionTime;
            PlayerPrefs.SetInt(BOT_P1_GAMES, games);
            PlayerPrefs.SetInt(BOT_P1_CORRECT, corrects);
            PlayerPrefs.SetFloat(BOT_P1_TOTAL_RT, totalRT);
        }
        else
        {
            int games = PlayerPrefs.GetInt(BOT_BOT_GAMES, 0) + 1;
            int corrects = PlayerPrefs.GetInt(BOT_BOT_CORRECT, 0) + (correct ? 1 : 0);
            float totalRT = PlayerPrefs.GetFloat(BOT_BOT_TOTAL_RT, 0f) + reactionTime;
            PlayerPrefs.SetInt(BOT_BOT_GAMES, games);
            PlayerPrefs.SetInt(BOT_BOT_CORRECT, corrects);
            PlayerPrefs.SetFloat(BOT_BOT_TOTAL_RT, totalRT);
        }
        PlayerPrefs.Save();
        UpdatePlayerStatsDisplay();
    }

    // Construiește textul de statistică afișat în playerGameStats
    public void UpdatePlayerStatsDisplay()
    {
        if (playerGameStats == null) return;

        // Single player
        int spGames = PlayerPrefs.GetInt(SP_GAMES, 0);
        int spCorrect = PlayerPrefs.GetInt(SP_CORRECT, 0);
        float spTotalRT = PlayerPrefs.GetFloat(SP_TOTAL_RT, 0f);
        float spAvgRT = spGames > 0 ? spTotalRT / spGames : 0f;
        float spAcc = spGames > 0 ? (100f * spCorrect / spGames) : 0f;

        // Local multiplayer (player 1)
        int mp1Games = PlayerPrefs.GetInt(MP_P1_GAMES, 0);
        int mp1Correct = PlayerPrefs.GetInt(MP_P1_CORRECT, 0);
        float mp1AvgRT = mp1Games > 0 ? PlayerPrefs.GetFloat(MP_P1_TOTAL_RT, 0f) / mp1Games : 0f;
        float mp1Acc = mp1Games > 0 ? (100f * mp1Correct / mp1Games) : 0f;

        // Local multiplayer (player 2)
        int mp2Games = PlayerPrefs.GetInt(MP_P2_GAMES, 0);
        int mp2Correct = PlayerPrefs.GetInt(MP_P2_CORRECT, 0);
        float mp2AvgRT = mp2Games > 0 ? PlayerPrefs.GetFloat(MP_P2_TOTAL_RT, 0f) / mp2Games : 0f;
        float mp2Acc = mp2Games > 0 ? (100f * mp2Correct / mp2Games) : 0f;

        // Versus Bot (only show human player 1 stats)
        int botP1Games = PlayerPrefs.GetInt(BOT_P1_GAMES, 0);
        int botP1Correct = PlayerPrefs.GetInt(BOT_P1_CORRECT, 0);
        float botP1AvgRT = botP1Games > 0 ? PlayerPrefs.GetFloat(BOT_P1_TOTAL_RT, 0f) / botP1Games : 0f;
        float botP1Acc = botP1Games > 0 ? (100f * botP1Correct / botP1Games) : 0f;

        // Formatări: procente fără zecimale, timpi cu 3 zecimale
        string spLine = $"Single Player\nGames: {spGames}   Accuracy: {spAcc:0}%   Avg RT: {spAvgRT:0.###}s";

        string localLine =
            $"Local Multiplayer\n" +
            $"P1 - Games: {mp1Games}   Accuracy: {mp1Acc:0}%   Avg RT: {mp1AvgRT:0.###}s\n" +
            $"P2 - Games: {mp2Games}   Accuracy: {mp2Acc:0}%   Avg RT: {mp2AvgRT:0.###}s";

        string vsBotLine = $"Versus Bot\nP1 - Games: {botP1Games}   Accuracy: {botP1Acc:0}%   Avg RT: {botP1AvgRT:0.###}s";

        // Construim textul final curat, separat pe blocuri
        playerGameStats.text = $"{spLine}\n\n{localLine}\n\n{vsBotLine}";
    }

    // Opțional: metodă publică pentru reset statistici (dacă vrei în UI)
    public void ResetAllStats()
    {
        PlayerPrefs.DeleteKey(SP_GAMES);
        PlayerPrefs.DeleteKey(SP_CORRECT);
        PlayerPrefs.DeleteKey(SP_TOTAL_RT);
        PlayerPrefs.DeleteKey(MP_P1_GAMES);
        PlayerPrefs.DeleteKey(MP_P1_CORRECT);
        PlayerPrefs.DeleteKey(MP_P1_TOTAL_RT);
        PlayerPrefs.DeleteKey(MP_P2_GAMES);
        PlayerPrefs.DeleteKey(MP_P2_CORRECT);
        PlayerPrefs.DeleteKey(MP_P2_TOTAL_RT);
        PlayerPrefs.DeleteKey(BOT_P1_GAMES);
        PlayerPrefs.DeleteKey(BOT_P1_CORRECT);
        PlayerPrefs.DeleteKey(BOT_P1_TOTAL_RT);
        PlayerPrefs.DeleteKey(BOT_BOT_GAMES);
        PlayerPrefs.DeleteKey(BOT_BOT_CORRECT);
        PlayerPrefs.DeleteKey(BOT_BOT_TOTAL_RT);
        PlayerPrefs.Save();
        UpdatePlayerStatsDisplay();
    }

    // Setează textul rundelor, format: "2/10"
    public void SetRoundsText(int current, int total)
    {
        if (rounds_Text == null) return;
        rounds_Text.text = $"{current}/{total}";
    }

    // Afișează panelul final pentru două jucători (apelează când runde s-au terminat)
    public void ShowTwoPlayersResultPanel(string player1Text, string player2Text)
    {
        if (twoPlayersResultPanel == null) return;
        if (player_1_Result_Text != null) player_1_Result_Text.text = player1Text;
        if (player_2_Result_Text != null) player_2_Result_Text.text = player2Text;
        twoPlayersResultPanel.SetActive(true);
    }
    // -------------------------
}