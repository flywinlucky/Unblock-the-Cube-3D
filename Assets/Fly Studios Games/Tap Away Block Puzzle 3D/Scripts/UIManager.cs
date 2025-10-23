﻿using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("Textul care afișează numărul nivelului curent.")]
    public Text currentLevelText; // Schimbat din TextMeshProUGUI în Text

    public GameObject levelWin_panel;
    public GameObject shop_panel;
    public GameObject safeAreaUI;

    // NOU: Text pentru soldul global de coins și pentru suma afișată în panelul de win
    [Tooltip("Text care arată soldul global de coins.")]
    public Text globalCoinsText;
    [Tooltip("Text afișat în level win panel cu suma câștigată la acel win (ex: +5).")]
    public Text winCoinsText;

    [Header("PowerUp UI")]
    [Tooltip("Text care arată câte Undo avem.")]
    public Text undoCountText;
    [Tooltip("Text care arată câte Smash avem.")]
    public Text smashCountText;

    [Header("Shop Buttons")]
    public Button buyUndoButton;
    public Button buySmashButton;

    [Header("Use Buttons")]
    public Button useUndoButton;
    public Button useSmashButton;

    [Header("References")]
    [Tooltip("Referință către LevelManager pentru a comanda cumpărări/folosiri.")]
    public LevelManager levelManager;

    [Header("Shop Price Labels")]
    [Tooltip("Text care afișează prețul pentru Undo în shop.")]
    public Text buyUndoPriceText;
    [Tooltip("Text care afișează prețul pentru Smash în shop.")]
    public Text buySmashPriceText;

    [Header("Shop Buttons (Open/Close)")]
    [Tooltip("Buton care deschide shop panel-ul.")]
    public Button openShopButton;
    [Tooltip("Buton care închide shop panel-ul.")]
    public Button closeShopButton;

    /// <summary>
    /// Actualizează textele pentru nivelul curent.
    /// </summary>
    /// <param name="levelNumber">Numărul nivelului curent.</param>
    public void UpdateLevelDisplay(int levelNumber)
    {
        if (currentLevelText != null)
        {
            currentLevelText.text = levelNumber.ToString();
        }
    }

    // NOU: actualizări UI pentru coins
    public void UpdateGlobalCoinsDisplay(int totalCoins)
    {
        if (globalCoinsText != null)
        {
            globalCoinsText.text = totalCoins.ToString();
        }
    }

    public void UpdateWinCoinsDisplay(int gainedCoins)
    {
        if (winCoinsText != null)
        {
            winCoinsText.text = (gainedCoins >= 0 ? "+" : "") + gainedCoins.ToString();
        }
    }

    private void Start()
    {
        shop_panel.SetActive(false);
        safeAreaUI.SetActive(true);

        // Legăm butoanele shop la funcțiile din LevelManager (dacă sunt setate)
        if (buyUndoButton != null && levelManager != null)
        {
            buyUndoButton.onClick.RemoveAllListeners();
            buyUndoButton.onClick.AddListener(() => {
                if (levelManager.BuyUndo()) UpdatePowerUpCounts(levelManager.undoCount, levelManager.smashCount);
            });
        }
        if (buySmashButton != null && levelManager != null)
        {
            buySmashButton.onClick.RemoveAllListeners();
            buySmashButton.onClick.AddListener(() => {
                if (levelManager.BuySmash()) UpdatePowerUpCounts(levelManager.undoCount, levelManager.smashCount);
            });
        }

        // Legăm butoanele de folosire la funcțiile LevelManager
        if (useUndoButton != null && levelManager != null)
        {
            useUndoButton.onClick.RemoveAllListeners();
            useUndoButton.onClick.AddListener(() => { levelManager.UseUndo(); UpdatePowerUpCounts(levelManager.undoCount, levelManager.smashCount); });
        }
        if (useSmashButton != null && levelManager != null)
        {
            useSmashButton.onClick.RemoveAllListeners();
            // schimbăm comportamentul: apăsarea butonului pornește modul de selecție pentru a alege block-ul de distrus
            useSmashButton.onClick.AddListener(() => { levelManager.StartRemoveMode(); UpdatePowerUpCounts(levelManager.undoCount, levelManager.smashCount); });
        }

        // Legăm butoanele de open/close shop (dacă sunt setate)
        if (openShopButton != null)
        {
            openShopButton.onClick.RemoveAllListeners();
            openShopButton.onClick.AddListener(() => OpenShopPanel());
        }
        if (closeShopButton != null)
        {
            closeShopButton.onClick.RemoveAllListeners();
            closeShopButton.onClick.AddListener(() => CloseShopPanel());
        }

        // inițializăm afișajul contorilor
        if (levelManager != null)
            UpdatePowerUpCounts(levelManager.undoCount, levelManager.smashCount);

        // setăm prețurile pe butoane (dacă există)
        if (levelManager != null)
        {
            if (buyUndoPriceText != null) buyUndoPriceText.text = levelManager.undoCost.ToString();
            if (buySmashPriceText != null) buySmashPriceText.text = levelManager.smashCost.ToString();
        }
    }

    // NOU: actualizează textele power-up
    public void UpdatePowerUpCounts(int undo, int smash)
    {
        if (undoCountText != null) undoCountText.text = undo.ToString();
        if (smashCountText != null) smashCountText.text = smash.ToString();
    }

    // NOU: deschide shop panel-ul
    public void OpenShopPanel()
    {
        if (shop_panel != null) shop_panel.SetActive(true);
        // sincronizare cu LevelManager (opțional)
        if (levelManager != null) levelManager.OpenShop();
        safeAreaUI.SetActive(false);
        levelManager.levelContainer.gameObject.SetActive(false);
    }

    // NOU: închide shop panel-ul
    public void CloseShopPanel()
    {
        if (shop_panel != null) shop_panel.SetActive(false);
        // sincronizare cu LevelManager (opțional)
        if (levelManager != null) levelManager.CloseShop();
        safeAreaUI.SetActive(true);
        levelManager.levelContainer.gameObject.SetActive(true);
    }
}