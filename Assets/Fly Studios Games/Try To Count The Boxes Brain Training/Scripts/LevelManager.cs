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
 
    private void Start()
    {

    }

    private void Update()
    {
        HandlePlayerInput();
    }

    private void HandlePlayerInput()
    {
        if (Input.GetKeyDown(player_1_IncreaseScore_Button_KeyCode))
        {
            player_1_Score++;
            player_1_UI.UpdateScore(player_1_Score); // Actualizează scorul folosind metoda din PlayerUI
        }

        if (Input.GetKeyDown(player_2_IncreaseScore_Button_KeyCode))
        {
            player_2_Score++;
            player_2_UI.UpdateScore(player_2_Score); // Actualizează scorul folosind metoda din PlayerUI
        }

        if (Input.GetKeyDown(player_1_Done_Button_KeyCode))
        {
            Debug.Log("Player 1 Done!");
        }

        if (Input.GetKeyDown(player_2_Done_Button_KeyCode))
        {
            Debug.Log("Player 2 Done!");
        }
    }
}