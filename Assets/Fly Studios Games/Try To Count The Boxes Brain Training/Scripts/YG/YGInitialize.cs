using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YG;

public class YGInitialize : MonoBehaviour
{
    private string currendLang;
    public GameObject[] mobileButtons;
    
    private void Start()
    {
        YG2.GameReadyAPI();
        Debug.Log("GameReady called — game is now ready for interaction.");

        currendLang = YG2.envir.language;

        ChangeLanguage(currendLang);

        if (YG2.envir.isDesktop)
        {
            foreach (GameObject button in mobileButtons)
            {
                button.SetActive(false);
            }
        }
        else
        {
            foreach (GameObject button in mobileButtons)
            {
                button.SetActive(true);
            }
        }
    }

    public void ChangeLanguage(string prefixLanguage)
    {
        Debug.Log("YGInitialize : Language changed to " + prefixLanguage);
        YG2.SwitchLanguage(prefixLanguage);
    }


}