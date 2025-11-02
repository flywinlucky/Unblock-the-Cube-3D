using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text gameMessage_Text;
    public Text countDown_Text;

    public void UpdateGameMessage(string message)
    {
        gameMessage_Text.text = message;
    }

    public void UpdateCountdown(string countdown)
    {
        countDown_Text.text = countdown;
    }
}