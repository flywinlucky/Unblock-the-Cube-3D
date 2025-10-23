using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YG;

public class RewardAdCoins : MonoBehaviour
{  
    public void ShowRewarded_GetCoins()
    {
        Debug.Log("Try Show Reward");

        // Afișează reclama și dă recompensa dacă a fost vizionată complet
        YG2.RewardedAdvShow("showrewardadd_coins", () =>
        {
            Debug.Log("Get Reward");
        });
    }
}
