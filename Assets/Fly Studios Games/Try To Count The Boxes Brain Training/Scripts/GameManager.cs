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

        uiManager?.UpdateGameMessage("Try to count the boxes.");
        uiManager?.StartCountdown(3, () => InitializeLevel(currentLevelIndex));
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
    }

    private void Update()
    {
        if (canInteract)
        {
            HandlePlayerInput();
        }
    }

    private void HandlePlayerInput()
    {
        if (!canInteract) return;

        if (!player_1_Done && Input.GetKeyDown(player_1_IncreaseScore_Button_KeyCode))
        {
            player_1_Score++;
            player_1_UI?.UpdateScore(player_1_Score);
            audioManager?.PlayButtonClick(); // Play button click sound
        }

        if (!player_2_Done && Input.GetKeyDown(player_2_IncreaseScore_Button_KeyCode))
        {
            player_2_Score++;
            player_2_UI?.UpdateScore(player_2_Score);
            audioManager?.PlayButtonClick(); // Play button click sound
        }

        if (Input.GetKeyDown(player_1_Done_Button_KeyCode) && !player_1_Done)
        {
            player_1_Done = true;
            player_1_UI?.ShowFinalResult(player_1_Score, totalCountInScene);
            audioManager?.PlayButtonDoneClick(); // Play button done click sound
            CheckBothPlayersDone();
        }

        if (Input.GetKeyDown(player_2_Done_Button_KeyCode) && !player_2_Done)
        {
            player_2_Done = true;
            player_2_UI?.ShowFinalResult(player_2_Score, totalCountInScene);
            audioManager?.PlayButtonDoneClick(); // Play button done click sound
            CheckBothPlayersDone();
        }
    }

    private void OnCountdownComplete()
    {
        uiManager?.UpdateGameMessage("How many boxes were there?");
        EnablePlayerInteraction();
    }

    private void EnablePlayerInteraction()
    {
        player_1_Done = false;
        player_2_Done = false;
    }

    private void CheckBothPlayersDone()
    {
        if (player_1_Done && player_2_Done)
        {
            StartCoroutine(HandleLevelCompletion());
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
        player_1_Done = player_2_Done = false;
        canInteract = false;

        player_1_UI?.UpdateScore(player_1_Score);
        player_2_UI?.UpdateScore(player_2_Score);
    }
}