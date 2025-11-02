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
}
