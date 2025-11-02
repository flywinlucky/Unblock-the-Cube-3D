using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerUI : MonoBehaviour
{
    [Header("Player UI")]
    public GameObject player_UI_panel;
    public Text player_countCell_1_Text;
    public Text player_countCell_2_Text;
        [Space]
    public Text player_countButton_Keyboard_Key_Text;
    public Text player_countButton_message_Text;
    public string player_countButton_message_string;
    public Text player_doneButton_Keyboard_Key_Text;

    public Text player_doneButton_message_Text;
    public string player_doneButton_message_string;

    [Space]
    public GameObject player_finalCountResult;
    public Text player_finalCount_Text;
    public Image player_resultIcon_image;
    public Sprite player_trueFlag_icon_sprite;
    public Sprite player_falseFlag_icon_sprite;

    private void Start()
    {
     
    }

    public void UpdateScore(int score)
    {
        int cell2 = score / 10; // Diviziunea pentru a obține cifra zecilor
        int cell1 = score % 10; // Restul pentru a obține cifra unităților

        player_countCell_1_Text.text = cell1.ToString();
        player_countCell_2_Text.text = cell2.ToString(); // Afișează 0 dacă nu are valoare
    }

    public void InitializeUI(string countButton_Keyboard, string doneButton_Keyboard)
    {
        player_countButton_Keyboard_Key_Text.text = countButton_Keyboard;
        player_doneButton_Keyboard_Key_Text.text = doneButton_Keyboard;
        player_countButton_message_Text.text =  player_countButton_message_string;
        player_doneButton_message_Text.text = player_doneButton_message_string;
    }

    public void ShowFinalResult(int playerScore, int totalCountInScene)
    {
        player_UI_panel.SetActive(false);
        player_finalCountResult.SetActive(true);

        player_finalCount_Text.text = playerScore.ToString();

        bool isCorrect = playerScore == totalCountInScene;
        player_resultIcon_image.gameObject.SetActive(false); // Dezactivăm inițial
        player_resultIcon_image.sprite = isCorrect ? player_trueFlag_icon_sprite : player_falseFlag_icon_sprite;
    }

    public void ActivateResultIcon()
    {
        player_resultIcon_image.gameObject.SetActive(true); // Activăm când este necesar
    }
}
