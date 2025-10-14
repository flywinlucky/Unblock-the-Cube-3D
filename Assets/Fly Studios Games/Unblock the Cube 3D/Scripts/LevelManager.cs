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

    // NOU: Undo stack - păstrează ultimele mov-uri / distrugeri
    private struct MoveRecord
    {
        public bool wasDestroyed;
        public Vector3 startPos;
        public Vector3 endPos;
        public Vector3Int gridPos;
        public MoveDirection direction;
        public Quaternion rotation;
        public Vector3 scale;
    }
    private Stack<MoveRecord> _undoStack = new Stack<MoveRecord>();
    public int maxUndo = 20;

    // NOU: Power-up inventory & costs
    [Header("PowerUps")]
    [Tooltip("Numărul curent de Undo disponibile.")]
    public int undoCount = 0;
    [Tooltip("Numărul curent de Hint disponibile.")]
    public int hintCount = 0;
    [Tooltip("Numărul curent de Smash disponibile.")]
    public int smashCount = 0;

    [Tooltip("Cost coins pentru a cumpăra un Undo.")]
    public int undoCost = 5;
    [Tooltip("Cost coins pentru a cumpăra un Hint.")]
    public int hintCost = 5;
    [Tooltip("Cost coins pentru a cumpăra un Smash.")]
    public int smashCost = 10;

    private const string UndoKey = "PlayerUndo";
    private const string HintKey = "PlayerHint";
    private const string SmashKey = "PlayerSmash";

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

        // NOU: încărcăm powerup-urile persistente la start și actualizăm UI
        undoCount = PlayerPrefs.GetInt(UndoKey, undoCount);
        hintCount = PlayerPrefs.GetInt(HintKey, hintCount);
        smashCount = PlayerPrefs.GetInt(SmashKey, smashCount);
        if (uiManager != null) uiManager.UpdatePowerUpCounts(undoCount, hintCount, smashCount);

        // Generăm nivelul curent (dacă există)
        GenerateLevel();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddCoins(100);
        }
    }

    // NOU: cumpărare power-ups din shop
    public bool BuyUndo()
    {
        if (SpendCoins(undoCost))
        {
            undoCount++;
            PlayerPrefs.SetInt(UndoKey, undoCount);
            if (uiManager != null) uiManager.UpdatePowerUpCounts(undoCount, hintCount, smashCount);
            return true;
        }
        return false;
    }

    public bool BuyHint()
    {
        if (SpendCoins(hintCost))
        {
            hintCount++;
            PlayerPrefs.SetInt(HintKey, hintCount);
            if (uiManager != null) uiManager.UpdatePowerUpCounts(undoCount, hintCount, smashCount);
            return true;
        }
        return false;
    }

    public bool BuySmash()
    {
        if (SpendCoins(smashCost))
        {
            smashCount++;
            PlayerPrefs.SetInt(SmashKey, smashCount);
            if (uiManager != null) uiManager.UpdatePowerUpCounts(undoCount, hintCount, smashCount);
            return true;
        }
        return false;
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
            // calculăm poziția pe grid pe baza poziției world
            Vector3Int gridPos = Vector3Int.RoundToInt(worldPosition / gridUnitSize);
            blockScript.Initialize(data.direction, this, gridUnitSize, gridPos);
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

    // NOU: Block-uri apelează asta înainte de a porni mutarea/distrugerea
    public void RegisterMove(Block block, Vector3 startPos, Vector3 endPos, bool wasDestroyed, Vector3Int gridPos, MoveDirection dir, Quaternion rot, Vector3 scale)
    {
        MoveRecord rec = new MoveRecord
        {
            wasDestroyed = wasDestroyed,
            startPos = startPos,
            endPos = endPos,
            gridPos = gridPos,
            direction = dir,
            rotation = rot,
            scale = scale
        };

        _undoStack.Push(rec);
        if (_undoStack.Count > maxUndo) // păstrăm doar ultimele N
        {
            // pierdem cel mai vechi (pop all to temp queue approach)
            var arr = _undoStack.ToArray();
            _undoStack.Clear();
            for (int i = 0; i < Mathf.Min(arr.Length, maxUndo); i++) _undoStack.Push(arr[Mathf.Min(arr.Length - 1, i)]);
        }
    }

    // NOU: Undo - inversează ultima acțiune (mișcare sau distrugere)
    public void UseUndo()
    {
        if (undoCount <= 0)
        {
            Debug.Log("No Undos available.");
            return;
        }
        undoCount--;
        PlayerPrefs.SetInt(UndoKey, undoCount);
        if (uiManager != null) uiManager.UpdatePowerUpCounts(undoCount, hintCount, smashCount);

        if (_undoStack == null || _undoStack.Count == 0) return;
        MoveRecord rec = _undoStack.Pop();

        if (rec.wasDestroyed)
        {
            // recreăm block la poziția start
            Vector3 worldPos = rec.startPos;
            GameObject newBlockObj = Instantiate(singleBlockPrefab, worldPos, rec.rotation, levelContainer);
            Block blockScript = newBlockObj.GetComponent<Block>();
            blockScript.Initialize(rec.direction, this, gridUnitSize, rec.gridPos);
            _activeBlocks.Add(blockScript);
        }
        else
        {
            // găsim block-ul care s-a mutat la endPos (cautăm cel mai apropiat)
            Block closest = null;
            float best = float.MaxValue;
            foreach (var b in _activeBlocks)
            {
                if (b == null) continue;
                float d = Vector3.Distance(b.transform.position, rec.endPos);
                if (d < best)
                {
                    best = d;
                    closest = b;
                }
            }
            if (closest != null)
            {
                // mutăm instant blocul înapoi la start
                closest.transform.position = rec.startPos;
            }
        }

        // actualizăm UI coins etc. (dacă este cazul)
        if (uiManager != null) uiManager.UpdateGlobalCoinsDisplay(GetCoins());
    }

    // NOU: Hint - evidențiază un block care poate fi mutat (primul găsit)
    public void UseHint()
    {
        if (hintCount <= 0)
        {
            Debug.Log("No Hints available.");
            return;
        }
        hintCount--;
        PlayerPrefs.SetInt(HintKey, hintCount);
        if (uiManager != null) uiManager.UpdatePowerUpCounts(undoCount, hintCount, smashCount);

        foreach (var b in _activeBlocks)
        {
            if (b == null) continue;
            // verificăm dacă are spațiu liber imediat în față (nu e blocat)
            RaycastHit hit;
            if (!Physics.Raycast(b.transform.position, b.transform.forward, out hit, _GetGridDetectionDistance(b)))
            {
                // găsit candidate
                b.FlashHint();
                return;
            }
        }
        // dacă nu găsim nimic, ai putea arăta un mesaj
        Debug.Log("No hint available: no movable block detected.");
    }

    // NOU: găsește un block pentru smash (folosim aceeași logică ca la hint) și îl distrugem
    public void UseSmashHint()
    {
        if (smashCount <= 0)
        {
            Debug.Log("No Smash available.");
            return;
        }
        smashCount--;
        PlayerPrefs.SetInt(SmashKey, smashCount);
        if (uiManager != null) uiManager.UpdatePowerUpCounts(undoCount, hintCount, smashCount);

        foreach (var b in new List<Block>(_activeBlocks))
        {
            if (b == null) continue;
            RaycastHit hit;
            if (!Physics.Raycast(b.transform.position, b.transform.forward, out hit, _GetGridDetectionDistance(b)))
            {
                // înregistrăm și distrugem (Block.Smash se ocupă de animație și notificare)
                b.Smash();
                return;
            }
        }
        Debug.Log("No smash target found.");
    }

    // NOU: când true așteptăm ca jucătorul să dea click pe un bloc pentru a-l distruge manual
    private bool _awaitingRemoveSelection = false;

    // NOU: pornește modul în care următorul click pe un block îl va distruge (consumă un Smash la confirmare)
    public void StartRemoveMode()
    {
        if (smashCount <= 0)
        {
            Debug.Log("No Smash items available.");
            // poți afișa un mesaj în UI aici
            return;
        }

        _awaitingRemoveSelection = true;

        // Poți notifica UIManager pentru a afișa un prompt (opțional)
        if (uiManager != null)
        {
            // de ex: uiManager.ShowPrompt("Select a block to remove");
        }
    }

    public bool IsAwaitingRemove()
    {
        return _awaitingRemoveSelection;
    }

    // NOU: apelată când jucătorul a dat click pe un Block în modul Remove
    public void ConfirmRemoveAtBlock(Block block)
    {
        if (!_awaitingRemoveSelection || block == null) return;

        if (smashCount <= 0)
        {
            Debug.Log("No Smash items available.");
            _awaitingRemoveSelection = false;
            return;
        }

        // consumăm un Smash
        smashCount--;
        PlayerPrefs.SetInt(SmashKey, smashCount);
        if (uiManager != null) uiManager.UpdatePowerUpCounts(undoCount, hintCount, smashCount);

        // înregistrare pentru undo (opțional) și distrugere
        // Block.Smash se ocupă de animatie și notificare OnBlockRemoved
        block.Smash();

        _awaitingRemoveSelection = false;
    }

    // Helper mic: distanța pentru testul de blocaj (folosim gridUnitSize)
    private float _GetGridDetectionDistance(Block b)
    {
        return gridUnitSize * 0.6f; // detectăm obstacol imediat din față
    }
}