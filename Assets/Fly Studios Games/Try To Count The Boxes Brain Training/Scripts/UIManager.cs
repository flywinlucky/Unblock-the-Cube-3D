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
    private const string BOT_BOT_TOTAL_RT = "BOT_BOT_TOTAL_RT";

    // Total score keys (aggregate score across games)
    private const string SP_TOTAL_SCORE = "SP_TotalScore";
    private const string MP_P1_TOTAL_SCORE = "MP_P1_TotalScore";
    private const string MP_P2_TOTAL_SCORE = "MP_P2_TotalScore";
    private const string BOT_P1_TOTAL_SCORE = "BOT_P1_TotalScore";

    public void RecordSinglePlayerResult(bool correct, float reactionTime, int score)
    {
        int games = PlayerPrefs.GetInt(SP_GAMES, 0) + 1;
        int corrects = PlayerPrefs.GetInt(SP_CORRECT, 0) + (correct ? 1 : 0);
        float totalRT = PlayerPrefs.GetFloat(SP_TOTAL_RT, 0f) + reactionTime;
        int totalScore = PlayerPrefs.GetInt(SP_TOTAL_SCORE, 0) + score;
        PlayerPrefs.SetInt(SP_GAMES, games);
        PlayerPrefs.SetInt(SP_CORRECT, corrects);
        PlayerPrefs.SetFloat(SP_TOTAL_RT, totalRT);
        PlayerPrefs.SetInt(SP_TOTAL_SCORE, totalScore);
        PlayerPrefs.Save();
        UpdatePlayerStatsDisplay();
    }

    public void RecordLocalMultiplayerResult(int playerIndex, bool correct, float reactionTime, int score)
    {
        if (playerIndex == 1)
        {
            int games = PlayerPrefs.GetInt(MP_P1_GAMES, 0) + 1;
            int corrects = PlayerPrefs.GetInt(MP_P1_CORRECT, 0) + (correct ? 1 : 0);
            float totalRT = PlayerPrefs.GetFloat(MP_P1_TOTAL_RT, 0f) + reactionTime;
            int totalScore = PlayerPrefs.GetInt(MP_P1_TOTAL_SCORE, 0) + score;
            PlayerPrefs.SetInt(MP_P1_GAMES, games);
            PlayerPrefs.SetInt(MP_P1_CORRECT, corrects);
            PlayerPrefs.SetFloat(MP_P1_TOTAL_RT, totalRT);
            PlayerPrefs.SetInt(MP_P1_TOTAL_SCORE, totalScore);
        }
        else
        {
            int games = PlayerPrefs.GetInt(MP_P2_GAMES, 0) + 1;
            int corrects = PlayerPrefs.GetInt(MP_P2_CORRECT, 0) + (correct ? 1 : 0);
            float totalRT = PlayerPrefs.GetFloat(MP_P2_TOTAL_RT, 0f) + reactionTime;
            int totalScore = PlayerPrefs.GetInt(MP_P2_TOTAL_SCORE, 0) + score;
            PlayerPrefs.SetInt(MP_P2_GAMES, games);
            PlayerPrefs.SetInt(MP_P2_CORRECT, corrects);
            PlayerPrefs.SetFloat(MP_P2_TOTAL_RT, totalRT);
            PlayerPrefs.SetInt(MP_P2_TOTAL_SCORE, totalScore);
        }
        PlayerPrefs.Save();
        UpdatePlayerStatsDisplay();
    }

    public void RecordMultiplayerBotResult(int playerIndex, bool correct, float reactionTime, int score)
    {
        // playerIndex == 1 -> human player, playerIndex == 2 -> bot
        if (playerIndex == 1)
        {
            int games = PlayerPrefs.GetInt(BOT_P1_GAMES, 0) + 1;
            int corrects = PlayerPrefs.GetInt(BOT_P1_CORRECT, 0) + (correct ? 1 : 0);
            float totalRT = PlayerPrefs.GetFloat(BOT_P1_TOTAL_RT, 0f) + reactionTime;
            int totalScore = PlayerPrefs.GetInt(BOT_P1_TOTAL_SCORE, 0) + score;
            PlayerPrefs.SetInt(BOT_P1_GAMES, games);
            PlayerPrefs.SetInt(BOT_P1_CORRECT, corrects);
            PlayerPrefs.SetFloat(BOT_P1_TOTAL_RT, totalRT);
            PlayerPrefs.SetInt(BOT_P1_TOTAL_SCORE, totalScore);
        }
        else
        {
            int games = PlayerPrefs.GetInt(BOT_BOT_GAMES, 0) + 1;
            int corrects = PlayerPrefs.GetInt(BOT_BOT_CORRECT, 0) + (correct ? 1 : 0);
            float totalRT = PlayerPrefs.GetFloat(BOT_BOT_TOTAL_RT, 0f) + reactionTime;
            // int totalScore = PlayerPrefs.GetInt(BOT_BOT_GAMES, 0) + score; // optional store for bot if wanted
            PlayerPrefs.SetInt(BOT_BOT_GAMES, games);
            PlayerPrefs.SetInt(BOT_BOT_CORRECT, corrects);
            PlayerPrefs.SetFloat(BOT_BOT_TOTAL_RT, totalRT);
            // PlayerPrefs.SetInt(BOT_BOT_TOTAL_SCORE, totalScore); // not used for display
        }
        PlayerPrefs.Save();
        UpdatePlayerStatsDisplay();
    }

    // Construiește textul de statistică afișat în playerGameStats (multiplayer-focused, human readable)
    public void UpdatePlayerStatsDisplay()
    {
        if (playerGameStats == null) return;

        // Local multiplayer (player 1)
        int mp1Games = PlayerPrefs.GetInt(MP_P1_GAMES, 0);
        int mp1Correct = PlayerPrefs.GetInt(MP_P1_CORRECT, 0);
        float mp1TotalRT = PlayerPrefs.GetFloat(MP_P1_TOTAL_RT, 0f);
        float mp1AvgRT = mp1Games > 0 ? mp1TotalRT / mp1Games : 0f;
        float mp1Acc = mp1Games > 0 ? (100f * mp1Correct / mp1Games) : 0f;
        int mp1TotalScore = PlayerPrefs.GetInt(MP_P1_TOTAL_SCORE, 0);
        float mp1AvgScore = mp1Games > 0 ? (float)mp1TotalScore / mp1Games : 0f;

        // Local multiplayer (player 2)
        int mp2Games = PlayerPrefs.GetInt(MP_P2_GAMES, 0);
        int mp2Correct = PlayerPrefs.GetInt(MP_P2_CORRECT, 0);
        float mp2TotalRT = PlayerPrefs.GetFloat(MP_P2_TOTAL_RT, 0f);
        float mp2AvgRT = mp2Games > 0 ? mp2TotalRT / mp2Games : 0f;
        float mp2Acc = mp2Games > 0 ? (100f * mp2Correct / mp2Games) : 0f;
        int mp2TotalScore = PlayerPrefs.GetInt(MP_P2_TOTAL_SCORE, 0);
        float mp2AvgScore = mp2Games > 0 ? (float)mp2TotalScore / mp2Games : 0f;

        // Versus Bot (only human player 1 stats shown)
        int botP1Games = PlayerPrefs.GetInt(BOT_P1_GAMES, 0);
        int botP1Correct = PlayerPrefs.GetInt(BOT_P1_CORRECT, 0);
        float botP1TotalRT = PlayerPrefs.GetFloat(BOT_P1_TOTAL_RT, 0f);
        float botP1AvgRT = botP1Games > 0 ? botP1TotalRT / botP1Games : 0f;
        float botP1Acc = botP1Games > 0 ? (100f * botP1Correct / botP1Games) : 0f;
        int botP1TotalScore = PlayerPrefs.GetInt(BOT_P1_TOTAL_SCORE, 0);
        float botP1AvgScore = botP1Games > 0 ? (float)botP1TotalScore / botP1Games : 0f;

        // Build human-friendly blocks (labels, newlines)
        string localP1Block =
            "Local Multiplayer - Player 1\n" +
            $"Games Played: {mp1Games}\n" +
            $"Wins / Accuracy: {mp1Correct}/{mp1Games} ({mp1Acc:0}% )\n" +
            $"Total Score: {mp1TotalScore}   Average Score: {mp1AvgScore:0.##}\n" +
            $"Average Reaction Time: {mp1AvgRT:0.###} s\n";

        string localP2Block =
            "Local Multiplayer - Player 2\n" +
            $"Games Played: {mp2Games}\n" +
            $"Wins / Accuracy: {mp2Correct}/{mp2Games} ({mp2Acc:0}% )\n" +
            $"Total Score: {mp2TotalScore}   Average Score: {mp2AvgScore:0.##}\n" +
            $"Average Reaction Time: {mp2AvgRT:0.###} s\n";

        string vsBotBlock =
            "Versus Bot - Player 1 (human)\n" +
            $"Games Played: {botP1Games}\n" +
            $"Wins / Accuracy: {botP1Correct}/{botP1Games} ({botP1Acc:0}% )\n" +
            $"Total Score: {botP1TotalScore}   Average Score: {botP1AvgScore:0.##}\n" +
            $"Average Reaction Time: {botP1AvgRT:0.###} s\n";

        // Compose final text: show local multiplayer first, then versus-bot (only human stats)
        playerGameStats.text = $"{localP1Block}\n{localP2Block}\n{vsBotBlock}";
    }

    // Opțional: metodă publică pentru reset statistici (dacă vrei în UI)
    public void ResetAllStats()
    {
        PlayerPrefs.DeleteKey(SP_GAMES);
        PlayerPrefs.DeleteKey(SP_CORRECT);
        PlayerPrefs.DeleteKey(SP_TOTAL_RT);
        PlayerPrefs.DeleteKey(SP_TOTAL_SCORE);
        PlayerPrefs.DeleteKey(MP_P1_GAMES);
        PlayerPrefs.DeleteKey(MP_P1_CORRECT);
        PlayerPrefs.DeleteKey(MP_P1_TOTAL_RT);
        PlayerPrefs.DeleteKey(MP_P1_TOTAL_SCORE);
        PlayerPrefs.DeleteKey(MP_P2_GAMES);
        PlayerPrefs.DeleteKey(MP_P2_CORRECT);
        PlayerPrefs.DeleteKey(MP_P2_TOTAL_RT);
        PlayerPrefs.DeleteKey(MP_P2_TOTAL_SCORE);
        PlayerPrefs.DeleteKey(BOT_P1_GAMES);
        PlayerPrefs.DeleteKey(BOT_P1_CORRECT);
        PlayerPrefs.DeleteKey(BOT_P1_TOTAL_RT);
        PlayerPrefs.DeleteKey(BOT_P1_TOTAL_SCORE);
        PlayerPrefs.DeleteKey(BOT_BOT_GAMES);
        PlayerPrefs.DeleteKey(BOT_BOT_CORRECT);
        PlayerPrefs.DeleteKey(BOT_BOT_TOTAL_RT);
        PlayerPrefs.Save();
        UpdatePlayerStatsDisplay();
    }
    // -------------------------
}