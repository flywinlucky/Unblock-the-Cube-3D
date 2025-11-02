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

    public void InitializeUI(string increaseKey, string doneKey, string countMessage, string doneMessage)
    {
        player_countButton_Keyboard_Key_Text.text = increaseKey;
        player_doneButton_Keyboard_Key_Text.text = doneKey;
        player_countButton_message_Text.text = countMessage;
        player_doneButton_message_Text.text = doneMessage;
    }

    public void ShowFinalResult(bool isCorrect)
    {
        player_resultIcon_image.sprite = isCorrect ? player_trueFlag_icon_sprite : player_falseFlag_icon_sprite;
        player_finalCountResult.SetActive(true);
    }
}
