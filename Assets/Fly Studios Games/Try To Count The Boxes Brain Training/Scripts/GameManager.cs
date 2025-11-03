using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public List<GameObject> levels;
    public Transform levelTarget;

    private LevelManager currentLevelManager;

    [Header("Game Mode")]
    // two mods of game single player amnd for two players, run on local keyboard
    [Header("Player UI 1")]
    public PlayerUI player_1_UI;
    public KeyCode player_1_IncreaseScore_Button_KeyCode;
    public KeyCode player_1_Done_Button_KeyCode;
    private int player_1_Score = 0;

    [Header("Player UI 2")]
    public PlayerUI player_2_UI;
    public KeyCode player_2_IncreaseScore_Button_KeyCode;
    public KeyCode player_2_Done_Button_KeyCode;
    private int player_2_Score = 0;

    [Header("Referinces")]
    public UIManager uiManager;

    [Header("Game Settings")]
    public int totalCountInScene; // Exemplu: numărul total de cuburi din scenă

    private bool player_1_Done = false;
    private bool player_2_Done = false;

    private int currentLevelIndex = 0;

    private void Start()
    {
        InitializeLevel(currentLevelIndex); // Începem cu primul nivel

        player_1_UI.InitializeUI(
            player_1_IncreaseScore_Button_KeyCode.ToString(),
            player_1_Done_Button_KeyCode.ToString()
        );

        player_2_UI.InitializeUI(
            player_2_IncreaseScore_Button_KeyCode.ToString(),
            player_2_Done_Button_KeyCode.ToString()
        );

        uiManager.UpdateGameMessage("Try to count the boxes.");
        uiManager.StartCountdown(3, OnCountdownComplete);
    }

    private void InitializeLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Count)
        {
            Debug.LogError("Invalid level index.");
            return;
        }

        // Instanțiem nivelul și conectăm LevelManager
        GameObject levelInstance = Instantiate(levels[levelIndex], levelTarget);
        currentLevelManager = levelInstance.GetComponent<LevelManager>();

        if (currentLevelManager == null)
        {
            Debug.LogError("LevelManager is missing in the loaded level.");
            return;
        }

        // Apelăm metoda InitializeLevel din LevelManager
        currentLevelManager.InitializeLevel();

        // Setăm totalCountInScene cu valoarea cubesCount din LevelManager
        totalCountInScene = currentLevelManager.cubesCount;
    }

    private void Update()
    {
        HandlePlayerInput();
    }

    private void HandlePlayerInput()
    {
        if (!player_1_Done && Input.GetKeyDown(player_1_IncreaseScore_Button_KeyCode))
        {
            player_1_Score++;
            player_1_UI.UpdateScore(player_1_Score); // Actualizează scorul folosind metoda din PlayerUI
        }

        if (!player_2_Done && Input.GetKeyDown(player_2_IncreaseScore_Button_KeyCode))
        {
            player_2_Score++;
            player_2_UI.UpdateScore(player_2_Score); // Actualizează scorul folosind metoda din PlayerUI
        }

        if (Input.GetKeyDown(player_1_Done_Button_KeyCode) && !player_1_Done)
        {
            player_1_Done = true;
            player_1_UI.ShowFinalResult(player_1_Score, totalCountInScene);
            CheckBothPlayersDone();
        }

        if (Input.GetKeyDown(player_2_Done_Button_KeyCode) && !player_2_Done)
        {
            player_2_Done = true;
            player_2_UI.ShowFinalResult(player_2_Score, totalCountInScene);
            CheckBothPlayersDone();
        }
    }

    private void OnCountdownComplete()
    {
        uiManager.UpdateGameMessage("How many boxes were there?");
        EnablePlayerInteraction();
    }

    private void EnablePlayerInteraction()
    {
        // Logica pentru activarea interacțiunii cu butoanele
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
        // Apelăm ShowChildsMaterialFocus din LevelManager
        currentLevelManager.ShowChildsMaterialFocus();

        // Așteptăm până când toți copiii sunt setați
        yield return new WaitForSeconds(currentLevelManager.cubesCount * 0.3f);

        StartCoroutine(ShowFinalResultsAfterDelay());
    }

    private IEnumerator ShowFinalResultsAfterDelay()
    {
        // Apelăm StartCountUp pentru a face incrementarea scorului în countDown_Text
        bool countUpCompleted = false;
        uiManager.StartCountUp(totalCountInScene, 0.3f, () =>
        {
            countUpCompleted = true;
        });

        // Așteptăm până când numerotarea este completă
        yield return new WaitUntil(() => countUpCompleted);

        player_1_UI.ActivateResultIcon(); // Activăm iconița pentru Player 1
        player_2_UI.ActivateResultIcon(); // Activăm iconița pentru Player 2
        uiManager.countDown_Text.text = totalCountInScene.ToString();

        // Așteptăm 1.5 secunde înainte de a începe numerotarea
        yield return new WaitForSeconds(2f);

        // După ce numerotarea este completă, trecem la nivelul următor
        LoadNextLevel();
    }

    private void LoadNextLevel()
    {
        // Dezinstanțiem nivelul curent
        if (currentLevelManager != null)
        {
            Destroy(currentLevelManager.gameObject);
        }

        // Incrementăm indexul nivelului
        currentLevelIndex++;
        if (currentLevelIndex >= levels.Count)
        {
            Debug.Log("All levels completed!");

            // Resetăm datele jocului
            ResetGameData();

            // Resetăm UI-ul pentru fiecare jucător
            player_1_UI.ResetUI();
            player_2_UI.ResetUI();
            return; // Ieșim dacă nu mai sunt niveluri
        }

        // Resetăm datele jocului
        ResetGameData();

        // Resetăm UI-ul pentru fiecare jucător
        player_1_UI.ResetUI();
        player_2_UI.ResetUI();

        // Incarcăm următorul nivel
        InitializeLevel(currentLevelIndex);

        // Reîncepem logica pentru noul nivel
        uiManager.UpdateGameMessage("Try to count the boxes.");
        uiManager.StartCountdown(3, OnCountdownComplete);
    }

    private void ResetGameData()
    {
        player_1_Score = 0;
        player_2_Score = 0;
        player_1_Done = false;
        player_2_Done = false;

        player_1_UI.UpdateScore(player_1_Score);
        player_2_UI.UpdateScore(player_2_Score);
    }
}