using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
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
    public int totalCountInScene = 5; // Exemplu: numărul total de cuburi din scenă

    private bool player_1_Done = false;
    private bool player_2_Done = false;

    private void Start()
    {
        player_1_UI.InitializeUI(
            player_1_IncreaseScore_Button_KeyCode.ToString(),
            player_1_Done_Button_KeyCode.ToString()
        );

        player_2_UI.InitializeUI(
            player_2_IncreaseScore_Button_KeyCode.ToString(),
            player_2_Done_Button_KeyCode.ToString()
        );
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

    private void CheckBothPlayersDone()
    {
        if (player_1_Done && player_2_Done)
        {
            StartCoroutine(ShowFinalResultsAfterDelay());
        }
    }

    private IEnumerator ShowFinalResultsAfterDelay()
    {
        yield return new WaitForSeconds(0.5f); // Așteaptă 1 secundă

        player_1_UI.ActivateResultIcon(); // Activăm iconița pentru Player 1
        player_2_UI.ActivateResultIcon(); // Activăm iconița pentru Player 2

        uiManager.UpdateGameMessage("Game Over! Results are displayed.");
    }
}