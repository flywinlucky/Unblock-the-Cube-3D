using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class RewardedAdManager : MonoBehaviour
{
    [Header("Reward Settings")]
    [Tooltip("Coins awarded for watching a rewarded ad.")]
    public int rewardAmount = 20;

    [Header("Simulation / Integration")]
    [Tooltip("If true the manager considers an ad 'ready' automatically.")]
    public bool adReady = true;
    [Tooltip("If true the manager will simulate showing an ad automatically (useful for editor/testing).")]
    public bool autoSimulate = true;
    [Tooltip("If simulating, whether the simulated ad will succeed.")]
    public bool simulateSuccess = true;
    [Tooltip("Delay (seconds) used when simulating an ad show.")]
    public float simulateDelay = 2f;

    [Header("Simulator UI (optional)")]
    [Tooltip("Panel GameObject shown while simulating a rewarded ad (will be activated during simulation).")]
    public GameObject rewardSimulatorPanel;

    [Header("References (optional)")]
    public LevelManager levelManager;
    public NotificationManager notificationManager;

    [Header("Events")]
    public UnityEvent onAdStarted;
    public UnityEvent onAdCompleted;
    public UnityEvent onAdFailed;

    // internal callbacks if caller provided Actions
    private Action _pendingSuccess;
    private Action _pendingFail;
    private bool _waitingExternal = false;

    private void Start()
    {
        // nothing to initialize now; adapter removed
    }

    // Public API for UI buttons (keeps previous signature)
    public void ShowRewardedAd()
    {
        ShowRewardedAdInternal(true, null, null);
    }

    // more flexible API with callbacks
    public void ShowRewardedAd(Action onSuccess, Action onFail)
    {
        ShowRewardedAdInternal(false, onSuccess, onFail);
    }

    // internal centralized logic
    private void ShowRewardedAdInternal(bool useNotifications, Action onSuccess, Action onFail)
    {
        if (!IsReady())
        {
            if (useNotifications && notificationManager != null) notificationManager.ShowNotification("Ad not ready. Try again later.", 2f);
            onFail?.Invoke();
            onAdFailed?.Invoke();
            return;
        }

        onAdStarted?.Invoke();

        // store pending callbacks in case of external flow
        _pendingSuccess = onSuccess;
        _pendingFail = onFail;

        if (autoSimulate)
        {
            // Activate simulator UI if provided
            if (rewardSimulatorPanel != null) rewardSimulatorPanel.SetActive(true);
            StartCoroutine(SimulateAdFlow(useNotifications));
        }
        else
        {
            // Wait for external SDK to call ExternalAdSuccess/ExternalAdFail
            _waitingExternal = true;
            if (useNotifications && notificationManager != null) notificationManager.ShowNotification("Waiting for external ad SDK...", 2f);
        }
    }

    // Public helper to check readiness (UI can query)
    public bool IsReady()
    {
        return adReady;
    }

    // Simulation coroutine
    private IEnumerator SimulateAdFlow(bool useNotifications)
    {
        // optional small delay to mimic loading/showing
        yield return new WaitForSeconds(simulateDelay);

        // deactivate simulator UI before awarding/closing
        if (rewardSimulatorPanel != null) rewardSimulatorPanel.SetActive(false);

        if (simulateSuccess)
        {
            // success path
            AwardReward();
            _pendingSuccess?.Invoke();
            onAdCompleted?.Invoke();
            if (useNotifications && notificationManager != null) notificationManager.ShowNotification($"+{rewardAmount} coins", 2f);
        }
        else
        {
            // failed / skipped path
            _pendingFail?.Invoke();
            onAdFailed?.Invoke();
            if (useNotifications && notificationManager != null) notificationManager.ShowNotification("Ad not completed", 2f);
        }

        ClearPending();
    }

    // Methods to be called by real SDK wrapper when an external ad finishes
    public void ExternalAdSuccess()
    {
        if (!_waitingExternal) return;
        if (rewardSimulatorPanel != null) rewardSimulatorPanel.SetActive(false);
        AwardReward();
        _pendingSuccess?.Invoke();
        onAdCompleted?.Invoke();
        if (notificationManager != null) notificationManager.ShowNotification($"+{rewardAmount} coins", 2f);
        _waitingExternal = false;
        ClearPending();
    }

    public void ExternalAdFail()
    {
        if (!_waitingExternal) return;
        if (rewardSimulatorPanel != null) rewardSimulatorPanel.SetActive(false);
        _pendingFail?.Invoke();
        onAdFailed?.Invoke();
        if (notificationManager != null) notificationManager.ShowNotification("Ad not completed", 2f);
        _waitingExternal = false;
        ClearPending();
    }

    private void ClearPending()
    {
        _pendingSuccess = null;
        _pendingFail = null;
        _waitingExternal = false;
    }

    private void AwardReward()
    {
        if (levelManager != null)
        {
            levelManager.AddCoins(rewardAmount);
            if (levelManager.uiManager != null) levelManager.uiManager.UpdateGlobalCoinsDisplay(levelManager.GetCoins());
        }
        else
        {
            // fallback: store directly in PlayerPrefs
            int cur = PlayerPrefs.GetInt("PlayerCoins", 0);
            PlayerPrefs.SetInt("PlayerCoins", cur + rewardAmount);
            PlayerPrefs.Save();
            if (notificationManager != null) notificationManager.ShowNotification($"+{rewardAmount} coins", 2f);
        }
    }
}
