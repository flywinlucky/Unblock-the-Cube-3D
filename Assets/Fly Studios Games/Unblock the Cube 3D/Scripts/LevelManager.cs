// LevelManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Level Configuration")]
    // Lista de nivele - adaugă în inspector nivelurile dorite
    public List<LevelData> levelList = new List<LevelData>();
    [Tooltip("Indexul nivelului curent în listă (0 = primul nivel)")]
    public int currentLevelIndex = 0;

    private int _currentLevelNumber = 1; // Începem cu nivelul 1

    [Header("Object References")]
    public GameObject singleBlockPrefab;
    public Transform levelContainer;
    public UIManager uiManager; // Referință către UIManager
    public AudioManager audioManager; // NOU: referință la AudioManager (lege în inspector)

    [Header("Grid Settings")]
    public float gridUnitSize = 0.5f;

    private List<Block> _activeBlocks = new List<Block>();

    // Flag pentru a evita multiple tranziții simultane
    private bool _isTransitioning = false;

    [Header("Coins")]
    [Tooltip("Câte coins se acordă la finalizarea unui nivel.")]
    public int coinsPerLevel = 5; // poți schimba în inspector

    private const string CoinsKey = "PlayerCoins";

    // Helper: returnează LevelData curent sau null
    private LevelData GetCurrentLevelData()
    {
        if (levelList == null || levelList.Count == 0) return null;
        if (currentLevelIndex < 0) currentLevelIndex = 0;
        if (currentLevelIndex >= levelList.Count) currentLevelIndex = levelList.Count - 1;
        return levelList[currentLevelIndex];
    }

    // Actualizează numărul afișat pentru nivel (folosim index+1)
    private void UpdateCurrentLevelNumber()
    {
        _currentLevelNumber = currentLevelIndex + 1;
        if (uiManager != null) uiManager.UpdateLevelDisplay(_currentLevelNumber);
    }

    void Start()
    {
        // Initializăm UI-ul de coins și afișajul nivelului la start
        if (uiManager != null)
        {
            uiManager.UpdateGlobalCoinsDisplay(GetCoins());
            uiManager.UpdateWinCoinsDisplay(0); // resetează textul din panelul de win la start
        }

        // Actualizăm numărul curent de nivel (va apela și UpdateLevelDisplay)
        UpdateCurrentLevelNumber();

        // Generăm nivelul curent (dacă există)
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        LevelData currentLevelData = GetCurrentLevelData();
        if (currentLevelData == null || singleBlockPrefab == null)
        {
            if (levelList == null || levelList.Count == 0)
                Debug.LogError("No levels in levelList or Single Block Prefab is not set!");
            else
                Debug.LogError("Single Block Prefab is not set!");
            return;
        }

        // Curățăm nivelul anterior
        foreach (Transform child in levelContainer) { Destroy(child.gameObject); }
        _activeBlocks.Clear();

        // Obținem lista de blocuri
        List<BlockData> blocksToGenerate = currentLevelData.GetBlocks();

        // --- Integrare UI: afișăm doar nivelul curent dacă avem UI ---
        UpdateCurrentLevelNumber();
        // --------------------

        foreach (BlockData data in blocksToGenerate)
        {
            Vector3 worldPosition = (Vector3)data.position * gridUnitSize;
            Quaternion finalRotation = GetStableLookRotation(data.direction);

            GameObject newBlockObj = Instantiate(singleBlockPrefab, worldPosition, finalRotation, levelContainer);
            Block blockScript = newBlockObj.GetComponent<Block>();
            blockScript.Initialize(data.direction, this, gridUnitSize);
            _activeBlocks.Add(blockScript);
        }
    }

    public void OnBlockRemoved(Block block)
    {
        if (_activeBlocks.Remove(block))
        {
            // Am eliminat actualizările către progress bar (componentă ștearsă).
            CheckWinCondition();
        }
    }

    private void CheckWinCondition()
    {
        if (_activeBlocks.Count == 0)
        {
            Debug.Log($"Felicitări! Ai câștigat nivelul {_currentLevelNumber}!");

            // Acordăm coins o singură dată și apoi afișăm panelul de win
            if (!_isTransitioning)
            {
                // acordăm coins
                AddCoins(coinsPerLevel);

                // actualizăm textul din win panel (dacă există)
                if (uiManager != null)
                {
                    uiManager.UpdateWinCoinsDisplay(coinsPerLevel);
                    uiManager.UpdateGlobalCoinsDisplay(GetCoins());
                }

                // Activăm panelul de level win din UI (dacă e setat)
                if (uiManager != null && uiManager.levelWin_panel != null)
                {
                    uiManager.levelWin_panel.SetActive(true);
                    _isTransitioning = true; // blocăm alte acțiuni până la alegerea jucătorului
                }
                else
                {
                    // fallback: dacă nu avem UI, trecem imediat la nivelul următor
                    StartCoroutine(ProceedToNextLevelCoroutine());
                }
            }
        }
    }

    private IEnumerator ProceedToNextLevelCoroutine()
    {
        _isTransitioning = true;
        // așteptăm 1 secundă înainte de a trece nivelul
        yield return new WaitForSeconds(1f);

        // încercăm să trecem la nivelul următor din listă
        if (levelList != null && currentLevelIndex + 1 < levelList.Count)
        {
            currentLevelIndex++;
            UpdateCurrentLevelNumber();
            GenerateLevel();
        }
        else
        {
            Debug.Log("Ai terminat toate nivelele din listă!");
            // Poți adăuga aici logică pentru restart / meniuri etc.
        }

        _isTransitioning = false;
    }

    // --- Funcțiile ajutătoare rămân neschimbate ---

    private Quaternion GetStableLookRotation(MoveDirection dir)
    {
        Vector3 directionVector = GetDirectionVector(dir);
        if (directionVector == Vector3.zero) return Quaternion.identity;
        Vector3 upReference = (dir == MoveDirection.Up || dir == MoveDirection.Down) ? Vector3.forward : Vector3.up;
        return Quaternion.LookRotation(directionVector, upReference);
    }

    private Vector3 GetDirectionVector(MoveDirection dir)
    {
        switch (dir)
        {
            case MoveDirection.Forward: return Vector3.forward;
            case MoveDirection.Back: return Vector3.back;
            case MoveDirection.Up: return Vector3.up;
            case MoveDirection.Down: return Vector3.down;
            case MoveDirection.Left: return Vector3.left;
            case MoveDirection.Right: return Vector3.right;
        }
        return Vector3.forward;
    }

    // Public API apelabil din UI (legate la butoanele din levelWin_panel)
    public void NextLevel()
    {
        // Închidem panelul de win
        if (uiManager != null && uiManager.levelWin_panel != null)
            uiManager.levelWin_panel.SetActive(false);

        _isTransitioning = true;

        // Trecem la nivelul următor din listă imediat
        if (levelList != null && currentLevelIndex + 1 < levelList.Count)
        {
            currentLevelIndex++;
            UpdateCurrentLevelNumber();
            GenerateLevel();
        }
        else
        {
            Debug.Log("Nu există nivel următor. Ai terminat toate nivelele din listă!");
            // Poți deschide shop sau afișa ecran final aici
        }

        _isTransitioning = false;
    }

    // Public API pentru butonul "Open Shop" din panelul de win
    public void OpenShop()
    {
        if (uiManager != null && uiManager.shop_panel != null)
        {
            uiManager.shop_panel.SetActive(true);
        }
    }

    // Închide panelul de win (buton cancel/close)
    public void CloseLevelWin()
    {
        if (uiManager != null && uiManager.levelWin_panel != null)
        {
            uiManager.levelWin_panel.SetActive(false);
        }
        _isTransitioning = false;
    }

    // Inchide shop panel
    public void CloseShop()
    {
        if (uiManager != null && uiManager.shop_panel != null)
        {
            uiManager.shop_panel.SetActive(false);
        }
    }

    // --- Funcții pentru coins (persistență simplă) ---
    public int GetCoins()
    {
        return PlayerPrefs.GetInt(CoinsKey, 0);
    }

    private void SetCoins(int amount)
    {
        PlayerPrefs.SetInt(CoinsKey, amount);
        PlayerPrefs.Save();
        // actualizăm UI global imediat dacă există
        if (uiManager != null) uiManager.UpdateGlobalCoinsDisplay(amount);
    }

    public void AddCoins(int amount)
    {
        int newAmount = GetCoins() + amount;
        SetCoins(newAmount);
    }

    // Returnează true dacă tranzacția a avut succes (suficiente coins)
    public bool SpendCoins(int amount)
    {
        int current = GetCoins();
        if (current >= amount)
        {
            SetCoins(current - amount);
            return true;
        }
        return false;
    }
}