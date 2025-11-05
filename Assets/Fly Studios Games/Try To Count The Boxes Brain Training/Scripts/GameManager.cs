using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public List<GameObject> levels;
    public Transform levelTarget;

    private LevelManager currentLevelManager;

    [Header("Player UI 1")]
    public PlayerUI player_1_UI;
    public KeyCode player_1_IncreaseScore_Button_KeyCode;
    public KeyCode player_1_Done_Button_KeyCode;
    private int player_1_Score;

    [Header("Player UI 2")]
    public PlayerUI player_2_UI;
    public KeyCode player_2_IncreaseScore_Button_KeyCode;
    public KeyCode player_2_Done_Button_KeyCode;
    private int player_2_Score;

    [Header("References")]
    public UIManager uiManager;
    public AudioManager audioManager;

    [Header("Game Settings")]
    public int totalCountInScene;

    private bool player_1_Done;
    private bool player_2_Done;
    private int currentLevelIndex;
    private bool canInteract;
    private bool randomMode;
    private int lastRandomLevelIndex;

    private bool isSinglePlayerMode;
    private bool isMultiplayerBotMode;

    // Rounds support
    [Header("Rounds (multiplayer modes)")]
    public int roundsTotal = 5;        // numărul de runde (configurabil)
    private int currentRound = 0;
    private bool roundsModeActive = false;
    private int player1RoundsWins = 0;
    private int player2RoundsWins = 0;
    private int player1ScoreTotal = 0;
    private int player2ScoreTotal = 0;

    // Bot configuration
    [Header("Bot Settings (Multiplayer vs Bot)")]
    public float botMinActionDelay = 0.2f;
    public float botMaxActionDelay = 1f;
    [Range(0f, 1f)]
    public float botAccuracy = 0.6f; // chance to guess exact count
    public int botFluctuation = 2; // +/- deviation when bot is not accurate
    private Coroutine botCoroutine;

    private bool gamePaused = true; // Flag to pause the game initially

    // Reaction timing
    private float interactionStartTime;

    // Helper: înregistrează rezultatul când un jucător finalizează
    private void RecordPlayerDone(int playerIndex, int playerScore)
    {
        float reactionTime = Mathf.Max(0f, Time.time - interactionStartTime);
        bool correct = playerScore == totalCountInScene;
        if (uiManager != null)
        {
            if (isSinglePlayerMode)
            {
                uiManager.RecordSinglePlayerResult(correct, reactionTime, playerScore);
            }
            else if (isMultiplayerBotMode)
            {
                uiManager.RecordMultiplayerBotResult(playerIndex, correct, reactionTime, playerScore);
            }
            else
            {
                uiManager.RecordLocalMultiplayerResult(playerIndex, correct, reactionTime, playerScore);
            }
        }
    }

    private void Start()
    {
        if (uiManager?.players_UI_Canvas != null)
        {
            uiManager.players_UI_Canvas.SetActive(false);
        }

        player_1_UI?.InitializeUI(
            player_1_IncreaseScore_Button_KeyCode.ToString(),
            player_1_Done_Button_KeyCode.ToString()
        );

        player_2_UI?.InitializeUI(
            player_2_IncreaseScore_Button_KeyCode.ToString(),
            player_2_Done_Button_KeyCode.ToString()
        );

        uiManager?.UpdateGameMessage("Try to count the boxes");

        // Bind UI buttons to central handlers (works for both UI and keyboard paths)
        if (player_1_UI != null)
        {
            if (player_1_UI.increaseScore_Button != null)
            {
                player_1_UI.increaseScore_Button.onClick.RemoveAllListeners();
                player_1_UI.increaseScore_Button.onClick.AddListener(() => ButtonIncreaseScore(1));
            }
            if (player_1_UI.doneScore_Button != null)
            {
                player_1_UI.doneScore_Button.onClick.RemoveAllListeners();
                player_1_UI.doneScore_Button.onClick.AddListener(() => ButtonDone(1));
            }
        }

        if (player_2_UI != null)
        {
            if (player_2_UI.increaseScore_Button != null)
            {
                player_2_UI.increaseScore_Button.onClick.RemoveAllListeners();
                player_2_UI.increaseScore_Button.onClick.AddListener(() => ButtonIncreaseScore(2));
            }
            if (player_2_UI.doneScore_Button != null)
            {
                player_2_UI.doneScore_Button.onClick.RemoveAllListeners();
                player_2_UI.doneScore_Button.onClick.AddListener(() => ButtonDone(2));
            }
        }
    }

    private void InitializeLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError("Invalid level index.");
            return;
        }

        GameObject levelInstance = Instantiate(levels[levelIndex], levelTarget);
        currentLevelManager = levelInstance.GetComponent<LevelManager>();

        if (currentLevelManager == null)
        {
            Debug.LogError("LevelManager is missing in the loaded level.");
            return;
        }

        currentLevelManager.StartActiveCountdown(() =>
        {
            uiManager?.players_UI_Canvas?.SetActive(true);
            canInteract = true;
            OnCountdownComplete();
        });

        currentLevelManager.InitializeLevel();
        totalCountInScene = currentLevelManager.cubesCount;

        // Dacă suntem în modul cu bot, (re)pornește coroutine-ul bot dacă nu rulează deja.
        if (isMultiplayerBotMode && botCoroutine == null)
        {
            botCoroutine = StartCoroutine(BotPlayerRoutine());
        }
    }

    private void Update()
    {
        if (!gamePaused && canInteract)
        {
            HandlePlayerInput();
        }
    }

    public void StartGame()
    {
        gamePaused = false; // Unpause the game
        uiManager?.StartCountdown(3, () => InitializeLevel(currentLevelIndex));
    }

    public void StartMultiplayerBotMode()
    {
        isSinglePlayerMode = false;
        isMultiplayerBotMode = true;

        if (player_2_UI != null)
        {
            player_2_UI.gameObject.SetActive(true); // Enable Player 2 UI
        }

        // Dezactivează butoanele UI pentru player2 (botul lucrează intern)
        SetPlayer2ButtonsActive(false);

        // Start bot coroutine if not already running; coroutine will wait until level starts/canInteract
        if (botCoroutine == null)
            botCoroutine = StartCoroutine(BotPlayerRoutine());

        // enable rounds mode for multiplayer vs bot
        roundsModeActive = true;
        currentRound = 1;
        player1RoundsWins = player2RoundsWins = 0;
        player1ScoreTotal = player2ScoreTotal = 0;
        // update rounds text in UI
        uiManager?.rounds_Text?.gameObject?.SetActive(true);
        uiManager?.SetRoundsText(currentRound, roundsTotal);
    }

    public void StartLocalMultiplayerMode()
    {
        isSinglePlayerMode = false;
        isMultiplayerBotMode = false;

        if (player_2_UI != null)
        {
            player_2_UI.gameObject.SetActive(true); // Enable Player 2 UI
        }

        // În local multiplayer activăm butoanele pentru player2
        SetPlayer2ButtonsActive(true);

        // enable rounds mode for local multiplayer
        roundsModeActive = true;
        currentRound = 1;
        player1RoundsWins = player2RoundsWins = 0;
        player1ScoreTotal = player2ScoreTotal = 0;
        uiManager?.rounds_Text?.gameObject?.SetActive(true);
        uiManager?.SetRoundsText(currentRound, roundsTotal);
    }

    public void StartSinglePlayerMode()
    {
        // ensure single-player leaves rounds deactivated (keeps earlier behavior)
        roundsModeActive = false;
        uiManager?.rounds_Text?.gameObject?.SetActive(false);

        isSinglePlayerMode = true;
        isMultiplayerBotMode = false;

        if (player_2_UI != null)
        {
            player_2_UI.gameObject.SetActive(false); // Disable Player 2 UI
        }

        // IMPORTANT: în single-player considerăm player_2 deja "done"
        // astfel când player_1 apasă Done nivelul/rezultatul se va procesa imediat.
        player_2_Done = true;

        // Asigurăm butoanele player2 dezactivate
        SetPlayer2ButtonsActive(false);
    }

    private IEnumerator BotPlayerRoutine()
    {
        // Wait until a level is instantiated and totalCountInScene is known
        while (isMultiplayerBotMode && (currentLevelManager == null || totalCountInScene <= 0))
            yield return null;

        // Wait until player interaction is enabled (countdown finished)
        while (isMultiplayerBotMode && !canInteract)
            yield return null;

        // Decide bot's target guess
        int baseCount = Mathf.Max(0, totalCountInScene);
        bool willGuessCorrect = Random.value <= botAccuracy;
        int target = willGuessCorrect ? baseCount : baseCount + Random.Range(-botFluctuation, botFluctuation + 1);
        target = Mathf.Max(0, target);

        // Simulate pressing increase with random intervals until target (with occasional overshoot/hesitation)
        while (isMultiplayerBotMode && !player_2_Done)
        {
            if (!canInteract)
            {
                yield return null;
                continue;
            }

            if (player_2_Score < target)
            {
                float delay = Random.Range(botMinActionDelay, botMaxActionDelay);
                yield return new WaitForSeconds(delay);

                // Random behaviour: sometimes skip, sometimes increment 2, mostly increment 1
                float r = Random.value;
                if (r < 0.1f)
                {
                    // hesitate, do nothing this tick
                }
                else if (r > 0.95f)
                {
                    player_2_Score += 2;
                }
                else
                {
                    player_2_Score += 1;
                }

                player_2_UI?.UpdateScore(player_2_Score);
                audioManager?.PlayButtonClick();
            }
            else
            {
                // Reached target: small thinking delay, sometimes change mind, sometimes press done
                yield return new WaitForSeconds(Random.Range(0.4f, 1.2f));

                // small chance to adjust target (simulate rethinking)
                if (Random.value < 0.2f)
                {
                    int adjust = Random.Range(-1, 2);
                    target = Mathf.Max(0, target + adjust);
                    // continue loop to adjust score toward new target
                    continue;
                }

                // Possibly overshoot a little before pressing done (simulate human error)
                if (Random.value < 0.15f)
                {
                    player_2_Score += 1;
                    player_2_UI?.UpdateScore(player_2_Score);
                    audioManager?.PlayButtonClick();
                    yield return new WaitForSeconds(Random.Range(0.2f, 0.6f));
                }

                // Press done
                player_2_Done = true;
                player_2_UI?.ShowFinalResult(player_2_Score, totalCountInScene);
                audioManager?.PlayButtonDoneClick();
                RecordPlayerDone(2, player_2_Score);
                CheckBothPlayersDone();
            }
            yield return null;
        }

        // clear coroutine reference when finished
        botCoroutine = null;
    }

    // Central handlers used by both UI buttons and keyboard input
    public void ButtonIncreaseScore(int playerIndex)
    {
        if (gamePaused || !canInteract) return;

        if (playerIndex == 1)
        {
            if (player_1_Done) return;
            player_1_Score++;
            player_1_UI?.UpdateScore(player_1_Score);
            audioManager?.PlayButtonClick();
        }
        else if (playerIndex == 2)
        {
            // Prevent manual control of Player 2 when in single-player mode or when Player 2 is a bot.
            if (isSinglePlayerMode || isMultiplayerBotMode || player_2_Done) return;
            player_2_Score++;
            player_2_UI?.UpdateScore(player_2_Score);
            audioManager?.PlayButtonClick();
        }
    }

    public void ButtonDone(int playerIndex)
    {
        if (gamePaused || !canInteract) return;

        if (playerIndex == 1)
        {
            if (player_1_Done) return;
            player_1_Done = true;
            player_1_UI?.ShowFinalResult(player_1_Score, totalCountInScene);
            audioManager?.PlayButtonDoneClick();
            RecordPlayerDone(1, player_1_Score);
            CheckBothPlayersDone();
        }
        else if (playerIndex == 2)
        {
            // Block manual Done for Player 2 if single-player mode or bot mode is active.
            if (isSinglePlayerMode || isMultiplayerBotMode || player_2_Done) return;
            player_2_Done = true;
            player_2_UI?.ShowFinalResult(player_2_Score, totalCountInScene);
            audioManager?.PlayButtonDoneClick();
            RecordPlayerDone(2, player_2_Score);
            CheckBothPlayersDone();
        }
    }

    private void HandlePlayerInput()
    {
        if (!canInteract) return;

        // Keyboard input now delegates to same handlers as UI buttons
        if (Input.GetKeyDown(player_1_IncreaseScore_Button_KeyCode))
        {
            ButtonIncreaseScore(1);
        }
        if (Input.GetKeyDown(player_1_Done_Button_KeyCode))
        {
            ButtonDone(1);
        }
        if (Input.GetKeyDown(player_2_IncreaseScore_Button_KeyCode))
        {
            ButtonIncreaseScore(2);
        }
        if (Input.GetKeyDown(player_2_Done_Button_KeyCode))
        {
            ButtonDone(2);
        }
    }

    private void OnCountdownComplete()
    {
        uiManager?.UpdateGameMessage("How many boxes were there?");
        EnablePlayerInteraction();
        // startăm măsurarea timpului de reacție
        interactionStartTime = Time.time;
    }

    private void EnablePlayerInteraction()
    {
        // player 1 începe neterminat
        player_1_Done = false;
        // pentru single-player, player_2 rămâne marcat ca done;
        // pentru celelalte moduri îl resetăm ca fiind nefinalizat
        player_2_Done = isSinglePlayerMode ? true : false;
    }

    private void CheckBothPlayersDone()
    {
        if (player_1_Done && player_2_Done)
        {
            if (roundsModeActive)
                StartCoroutine(HandleRoundSequence());
            else
                StartCoroutine(HandleLevelCompletion());
        }
    }

    // Called when both players have finished a round-level in rounds mode
    private IEnumerator HandleRoundSequence()
    {
        // Show visual reveal similar to HandleLevelCompletion
        currentLevelManager?.ActivateLevelsCubesFlorrCell(true);
        currentLevelManager?.ActivateSelfFlorrCell(true);
        currentLevelManager?.ShowChildsMaterialFocus();

        yield return new WaitForSeconds(currentLevelManager.cubesCount * 0.25f);

        // Show count up and result icons (similar to ShowFinalResultsAfterDelay but not loading next immediately)
        float interval = Mathf.Clamp(0.25f / Mathf.Log10(totalCountInScene + 1), 0.02f, 0.25f);
        bool countUpCompleted = false;

        uiManager?.StartCountUp(totalCountInScene, interval, () => countUpCompleted = true);
        yield return new WaitUntil(() => countUpCompleted);

        player_1_UI?.ActivateResultIcon();
        player_2_UI?.ActivateResultIcon();
        uiManager.countDown_Text.text = totalCountInScene.ToString();

        audioManager?.PlayCountShowResult();

        yield return new WaitForSeconds(1.0f);

        // Record round results
        RecordRoundResults();

        // If last round -> show summary panel; else -> load next round/level
        if (currentRound >= roundsTotal)
        {
            // Show final aggregated panel
            roundsModeActive = false;
            gamePaused = true;
            uiManager?.SetRoundsText(currentRound, roundsTotal);
            uiManager?.ShowTwoPlayersResultPanel(BuildPlayerSummaryText(1), BuildPlayerSummaryText(2));
        }
        else
        {
            // proceed to next round
            currentRound++;
            uiManager?.SetRoundsText(currentRound, roundsTotal);
            // Move to next level (reusing LoadNextLevel flow)
            LoadNextLevel();
        }
    }

    private void RecordRoundResults()
    {
        bool p1Correct = player_1_Score == totalCountInScene;
        bool p2Correct = player_2_Score == totalCountInScene;
        if (p1Correct) player1RoundsWins++;
        if (p2Correct) player2RoundsWins++;
        player1ScoreTotal += player_1_Score;
        player2ScoreTotal += player_2_Score;
        // persist optional per-round stats via PlayerPrefs if desired (not required)
    }

    private string BuildPlayerSummaryText(int playerIndex)
    {
        if (playerIndex == 1)
        {
            float avgScore = (player1ScoreTotal > 0 && currentRound > 0) ? (float)player1ScoreTotal / Mathf.Max(1, currentRound) : 0f;
            return $"\nWins: {player1RoundsWins}/{currentRound}\nTotalScore: {player1ScoreTotal}\nAvgScore: {avgScore:0.##}";
        }
        else
        {
            float avgScore = (player2ScoreTotal > 0 && currentRound > 0) ? (float)player2ScoreTotal / Mathf.Max(1, currentRound) : 0f;
            return $"\nWins: {player2RoundsWins}/{currentRound}\nTotalScore: {player2ScoreTotal}\nAvgScore: {avgScore:0.##}";
        }
    }

    private IEnumerator HandleLevelCompletion()
    {
        currentLevelManager?.ActivateLevelsCubesFlorrCell(true);
        currentLevelManager?.ActivateSelfFlorrCell(true);
        currentLevelManager?.ShowChildsMaterialFocus();

        yield return new WaitForSeconds(currentLevelManager.cubesCount * 0.25f);
        StartCoroutine(ShowFinalResultsAfterDelay());
    }

    private IEnumerator ShowFinalResultsAfterDelay()
    {
        float interval = Mathf.Clamp(0.25f / Mathf.Log10(totalCountInScene + 1), 0.02f, 0.25f);
        bool countUpCompleted = false;

        uiManager?.StartCountUp(totalCountInScene, interval, () => countUpCompleted = true);
        yield return new WaitUntil(() => countUpCompleted);

        player_1_UI?.ActivateResultIcon();
        player_2_UI?.ActivateResultIcon();
        uiManager.countDown_Text.text = totalCountInScene.ToString();

        audioManager?.PlayCountShowResult(); // Play result display sound

        yield return new WaitForSeconds(1.5f);
        LoadNextLevel();
    }

    private void LoadNextLevel()
    {
        currentLevelManager?.ActivateLevelsCubesFlorrCell(false);
        currentLevelManager?.ActivateSelfFlorrCell(true);

        uiManager?.players_UI_Canvas?.SetActive(false);
        canInteract = false;

        if (currentLevelManager != null)
        {
            Destroy(currentLevelManager.gameObject);
        }

        currentLevelIndex = randomMode
            ? GetRandomLevelIndex()
            : (currentLevelIndex + 1) % levels.Count;

        ResetGameData();
        player_1_UI?.ResetUI();
        player_2_UI?.ResetUI();

        uiManager?.StartCountdown(3, () => InitializeLevel(currentLevelIndex));
    }

    private int GetRandomLevelIndex()
    {
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, levels.Count);
        } while (randomIndex == lastRandomLevelIndex);

        lastRandomLevelIndex = randomIndex;
        return randomIndex;
    }

    private void ResetGameData()
    {
        player_1_Score = player_2_Score = 0;
        player_1_Done = false;
        // păstrăm comportamentul single-player între nivele
        player_2_Done = isSinglePlayerMode ? true : false;
        canInteract = false;

        // Stop bot coroutine between levels to avoid duplicates
        if (botCoroutine != null)
        {
            StopCoroutine(botCoroutine);
            botCoroutine = null;
        }

        // Restaurăm starea butoanelor player2 conform modului curent
        // - single player: butoanele dezactivate
        // - bot mode: butoanele dezactivate (botul controlează intern)
        // - local multiplayer: butoanele activate
        bool player2ButtonsShouldBeActive = !isSinglePlayerMode && !isMultiplayerBotMode;
        SetPlayer2ButtonsActive(player2ButtonsShouldBeActive);

        player_1_UI?.UpdateScore(player_1_Score);
        player_2_UI?.UpdateScore(player_2_Score);
    }

    // Helper pentru a activa/dezactiva butoanele din PlayerUI2 (gameObject + interactable)
    private void SetPlayer2ButtonsActive(bool active)
    {
        if (player_2_UI == null) return;

        if (player_2_UI.increaseScore_Button != null)
        {
            player_2_UI.increaseScore_Button.gameObject.SetActive(active);
            player_2_UI.increaseScore_Button.interactable = active;
        }
        if (player_2_UI.doneScore_Button != null)
        {
            player_2_UI.doneScore_Button.gameObject.SetActive(active);
            player_2_UI.doneScore_Button.interactable = active;
        }
    }
}