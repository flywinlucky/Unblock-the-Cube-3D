using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YG;
using UnityEngine.UI;

public class RewardAdCoins : MonoBehaviour
{
    [Header("Reward Settings")]
    [Tooltip("Coins awarded for watching a rewarded ad.")]
    public int rewardAmount;

    [Header("References (optional)")]
    public LevelManager levelManager;
    public NotificationManager notificationManager;
    [Header("UI Elements")]
    public Text rewardAmount_Text;

    private void Start()
    {
        rewardAmount_Text.text = rewardAmount.ToString();   
    }

    public void ShowRewarded_GetCoins()
    {
        Debug.Log("Try Show Reward");

        // Afișează reclama și dă recompensa dacă a fost vizionată complet
        YG2.RewardedAdvShow("showrewardadd_coins", () =>
        {
            AwardReward();
        });
    }

    private void AwardReward()
    {
        levelManager.AddCoins(rewardAmount);
        if (levelManager.uiManager != null) levelManager.uiManager.UpdateGlobalCoinsDisplay(levelManager.GetCoins());
        if (notificationManager != null) notificationManager.ShowNotification($"+{rewardAmount} coins", 2f);

        Debug.Log("Get Reward coins: " + rewardAmount);
    }
}
