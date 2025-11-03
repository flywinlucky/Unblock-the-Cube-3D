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

    private Dictionary<Transform, Coroutine> activeAnimations = new Dictionary<Transform, Coroutine>();

    private Vector3 initialScale_Cell1;
    private Vector3 initialScale_Cell2;

    private void Start()
    {
        initialScale_Cell1 = player_countCell_1_Text.transform.localScale;
        initialScale_Cell2 = player_countCell_2_Text.transform.localScale;
    }

    public void UpdateScore(int score)
    {
        int cell2 = score / 10; // Diviziunea pentru a obține cifra zecilor
        int cell1 = score % 10; // Restul pentru a obține cifra unităților

        player_countCell_1_Text.text = cell1.ToString();
        player_countCell_2_Text.text = cell2.ToString(); // Afișează 0 dacă nu are valoare

        AnimateBouncingScale(cell1 > 0 ? player_countCell_1_Text.transform : player_countCell_2_Text.transform);
    }

    private void AnimateBouncingScale(Transform target)
    {
        if (activeAnimations.ContainsKey(target) && activeAnimations[target] != null)
        {
            StopCoroutine(activeAnimations[target]);
        }

        activeAnimations[target] = StartCoroutine(BouncingScaleRoutine(target));
    }

    private IEnumerator BouncingScaleRoutine(Transform target)
    {
        Vector3 originalScale = target == player_countCell_1_Text.transform ? initialScale_Cell1 : initialScale_Cell2;
        Vector3 enlargedScale = originalScale * 1.2f;

        // Resetează scara la valoarea inițială înainte de animație
        target.localScale = originalScale;

        // Scale up
        float elapsedTime = 0f;
        float duration = 0.1f;
        while (elapsedTime < duration)
        {
            target.localScale = Vector3.Lerp(originalScale, enlargedScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Scale down
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            target.localScale = Vector3.Lerp(enlargedScale, originalScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        target.localScale = originalScale; // Asigură-te că scara este resetată
        activeAnimations[target] = null; // Elimină referința la animația finalizată
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

    public void ResetUI()
    {
        // Dezactivează rezultatul final și activează UI-ul principal
         player_finalCountResult.SetActive(false);
 
        
        player_UI_panel.SetActive(true);

        // Resetează textele de count la 0
        player_countCell_1_Text.text = "0";
        player_countCell_2_Text.text = "0";
    }
}
