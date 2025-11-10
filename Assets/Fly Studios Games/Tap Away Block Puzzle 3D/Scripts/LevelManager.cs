using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Central manager for level flow: generation, instantiation, progress, coins and power-ups.
    /// This refactor keeps runtime logic unchanged while improving inspector UX and English docs.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        #region Inspector - Level Configuration

        [Header("Level Configuration")]
        [Tooltip("List of LevelData assets. Add levels in the inspector.")]
        public List<LevelData> levelList = new List<LevelData>();

        [Tooltip("Index of the current level in the list (0 = first level).")]
        public int currentLevelIndex = 0;

        private int _currentLevelNumber = 1;

        #endregion

        #region Inspector - References

        [Header("Object References")]
        [Tooltip("Prefab used to instantiate single blocks.")]
        public GameObject singleBlockPrefab;

        [Tooltip("Transform that will contain all level block instances.")]
        public Transform levelContainer;

        [Tooltip("Euler rotation applied to the level container after generation.")]
        public Vector3 rotationLevelContainer;

        [Tooltip("Reference to UIManager.")]
        public UIManager uiManager;

        [Tooltip("Optional AudioManager reference.")]
        public AudioManager audioManager;

        [Tooltip("NotificationManager for player messages.")]
        public NotificationManager notificationManager;

        [Tooltip("ShopManager reference (optional).")]
        public ShopManager shopManager;

        [Tooltip("Camera controller used to frame the level.")]
        public CameraControler cameraControler;

        [Tooltip("Super power UI for remove/shops.")]
        public SuperPowerUI removePowerUI;

        [Tooltip("Simple tutorial hand for the remover power.")]
        public RemoverPowerSimpleStepTutorialHand removerPowerSimpleStepTutorialHand;

        [Tooltip("Background image manager to update backgrounds by level number.")]
        public BackgroundImageManager backgroundImageManager;

        #endregion

        #region Inspector - Grid & Runtime

        [Header("Grid Settings")]
        [Tooltip("Unit size of the grid in world units.")]
        public float gridUnitSize = 0.5f;

        private List<Block> _activeBlocks = new List<Block>();

        // Remove-mode flag: the next click will remove the selected block
        private bool _awaitingRemoveSelection = false;

        // Transition guard
        private bool _isTransitioning = false;

        #endregion

        #region Inspector - Coins & Leveling

        [Header("Coins")]
        [Tooltip("How many coins awarded on level completion.")]
        public int coinsPerLevel = 5;

        private const string CoinsKey = "PlayerCoins";
        private const string LevelNumberKey = "PlayerLevelNumber";
        private int _overallLevelNumber = 1;

        #endregion

        #region Inspector - PowerUps

        [Header("PowerUps")]
        [Tooltip("Current number of Smash power-ups available.")]
        public int smashCount = 0;

        [Tooltip("Cost in coins to buy a Smash.")]
        public int smashCost = 10;

        private const string SmashKey = "PlayerSmash";

        #endregion

        #region Inspector - Random Cycle Settings

        [Header("Random Cycle Settings")]
        [Tooltip("Comma-separated list of level indices to IGNORE when doing random cycling (e.g. \"0,2,5\").")]
        public string ignoredLevelIndicesCSV = "0";

        [Tooltip("When the sequential list ends, enable infinite random cycling through allowed levels.")]
        public bool enableRandomCycleOnEnd = true;

        #endregion

        #region Inspector - Editor Mode

        [Header("Level Manager For Editor")]
        [Tooltip("When true, the manager uses runCurrentLevel only (level editor mode).")]
        public bool isLevelEditorManager;

        [Tooltip("LevelData to run in editor mode.")]
        public LevelData runCurrentLevel;

        #endregion

        // runtime caches
        private HashSet<int> _ignoredIndices = new HashSet<int>();
        private List<int> _allowedRandomIndices = new List<int>();

        // initial block count for progress UI
        private int _initialBlockCount = 0;

        // periodic progress check to correct discrepancies
        private float _progressCheckInterval = 0.5f;
        private float _progressCheckTimer = 0f;

        // Persistence key for current level index
        private const string CurrentLevelIndexKey = "CurrentLevelIndex";

        #region Helpers

        private LevelData GetCurrentLevelData()
        {
            if (levelList == null || levelList.Count == 0) return null;
            if (currentLevelIndex < 0) currentLevelIndex = 0;
            if (currentLevelIndex >= levelList.Count) currentLevelIndex = levelList.Count - 1;
            return levelList[currentLevelIndex];
        }

        private void UpdateCurrentLevelNumber()
        {
            _currentLevelNumber = _overallLevelNumber;
            if (uiManager != null) uiManager.UpdateLevelDisplay(_currentLevelNumber);
        }

        #endregion

        #region MonoBehaviour

        void Start()
        {
            // Editor mode: use only runCurrentLevel
            if (isLevelEditorManager)
            {
                levelList.Clear();
                if (runCurrentLevel != null)
                {
                    levelList.Add(runCurrentLevel);
                }
                else
                {
                    Debug.LogWarning("runCurrentLevel is not set! Assign a LevelData in the inspector.");
                }
            }

            // Initialize UI coins and level display
            if (uiManager != null)
            {
                uiManager.UpdateGlobalCoinsDisplay(GetCoins());
                uiManager.UpdateWinCoinsDisplay(0);
            }

            // Load overall level number (global) from PlayerPrefs
            _overallLevelNumber = PlayerPrefs.GetInt(LevelNumberKey, 1);

            // Load saved current level index
            currentLevelIndex = PlayerPrefs.GetInt(CurrentLevelIndexKey, 0);
            currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, levelList.Count > 0 ? levelList.Count - 1 : 0);

            UpdateCurrentLevelNumber();

            // Load persistent power-ups
            smashCount = PlayerPrefs.GetInt(SmashKey, smashCount);
            if (uiManager != null) uiManager.UpdatePowerUpCounts(smashCount);

            // Ensure camera rotation is enabled if controller exists
            if (cameraControler != null)
            {
                cameraControler.rotationEnabled = true;
            }

            // Update background for current overall level
            if (backgroundImageManager != null)
            {
                backgroundImageManager.UpdateBackground(_overallLevelNumber);
            }

            // Generate the current level
            GenerateLevel();
        }

        private void Update()
        {
            // Periodic progress correction
            _progressCheckTimer += Time.deltaTime;
            if (_progressCheckTimer >= _progressCheckInterval)
            {
                _progressCheckTimer = 0f;
                if (uiManager != null && uiManager.currentProgresSlider != null)
                {
                    float expected = Mathf.Clamp(_initialBlockCount - (_activeBlocks != null ? _activeBlocks.Count : 0), 0, Mathf.Max(1, _initialBlockCount));
                    float current = uiManager.currentProgresSlider.value;
                    if (Mathf.Abs(current - expected) > 0.01f)
                    {
                        uiManager.UpdateProgressByCounts(_initialBlockCount, _activeBlocks != null ? _activeBlocks.Count : 0);
                    }
                }
            }
        }

        #endregion

        #region Purchases & PowerUp Methods

        public bool BuySmash()
        {
            if (SpendCoins(smashCost))
            {
                smashCount++;
                PlayerPrefs.SetInt(SmashKey, smashCount);
                if (uiManager != null) uiManager.UpdatePowerUpCounts(smashCount);
                if (notificationManager != null) notificationManager.ShowNotification("Remover purchased +1", 2f);
                return true;
            }
            if (notificationManager != null) notificationManager.ShowNotification("Not enough coins", 2f);
            return false;
        }

        #endregion

        #region Level Generation & Instantiation

        public void GenerateLevel()
        {
            if (levelContainer != null)
                levelContainer.rotation = Quaternion.Euler(0f, 0f, 0f);

            LevelData currentLevelData = GetCurrentLevelData();
            if (currentLevelData == null || singleBlockPrefab == null)
            {
                if (levelList == null || levelList.Count == 0)
                    Debug.LogError("No levels in levelList or Single Block Prefab is not set!");
                else
                    Debug.LogError("Single Block Prefab is not set!");
                return;
            }

            // Clear previous level
            if (levelContainer != null)
            {
                foreach (Transform child in levelContainer) { Destroy(child.gameObject); }
            }
            _activeBlocks.Clear();

            // Populate with blocks
            List<BlockData> blocksToGenerate = currentLevelData.GetBlocks();
            if (blocksToGenerate != null)
            {
                int blockIndex = 0;
                foreach (BlockData data in blocksToGenerate)
                {
                    Vector3 worldPosition = (Vector3)data.position * gridUnitSize;
                    Quaternion finalRotation = GetStableLookRotation(data.direction);

                    GameObject newBlockObj = Instantiate(singleBlockPrefab, worldPosition, finalRotation, levelContainer);
                    newBlockObj.name = $"Block_{blockIndex}";
                    blockIndex++;

                    Block blockScript = newBlockObj.GetComponent<Block>();
                    Vector3Int gridPos = Vector3Int.RoundToInt(worldPosition / gridUnitSize);
                    blockScript.Initialize(data.direction, this, gridUnitSize, gridPos);

                    if (shopManager != null && shopManager.selectedMaterial != null)
                    {
                        blockScript.ApplySkin(shopManager.selectedMaterial);
                    }

                    _activeBlocks.Add(blockScript);
                }
            }

            // Initialize progress UI
            _initialBlockCount = _activeBlocks != null ? _activeBlocks.Count : 0;
            if (uiManager != null)
            {
                uiManager.InitProgress(_initialBlockCount);
                uiManager.UpdateProgressByCounts(_initialBlockCount, _activeBlocks.Count);
            }

            // Frame camera next frame if available
            if (cameraControler != null)
            {
                StartCoroutine(FrameCameraNextFrame());
            }
            else
            {
                Debug.LogWarning("CameraControler is not assigned; cannot frame target.", this);
            }

            if (uiManager != null && uiManager.safeAreaUI != null)
            {
                uiManager.safeAreaUI.SetActive(true);
            }

            if (levelContainer != null)
                levelContainer.rotation = Quaternion.Euler(rotationLevelContainer);
        }

        private IEnumerator FrameCameraNextFrame()
        {
            yield return new WaitForEndOfFrame();
            if (cameraControler != null)
            {
                cameraControler.FrameTarget();
            }
        }

        #endregion

        #region Active Blocks Management

        public List<Block> GetActiveBlocks()
        {
            return new List<Block>(_activeBlocks);
        }

        public void OnBlockRemoved(Block block)
        {
            if (_activeBlocks.Remove(block))
            {
                if (uiManager != null)
                {
                    uiManager.UpdateProgressByCounts(_initialBlockCount, _activeBlocks.Count);
                }

                CheckWinCondition();
            }
        }

        #endregion

        #region Win / Level Flow

        private void CheckWinCondition()
        {
            if (_activeBlocks.Count == 0)
            {
                Debug.Log($"Congratulations! You completed level {_currentLevelNumber}!");

                if (isLevelEditorManager)
                {
                    Debug.Log("Level Editor Mode: Level completed. Progress will not be saved or advanced.");
                    return;
                }

                if (!_isTransitioning)
                {
                    AddCoins(coinsPerLevel);

                    if (uiManager != null)
                    {
                        uiManager.UpdateWinCoinsDisplay(coinsPerLevel);
                        uiManager.UpdateGlobalCoinsDisplay(GetCoins());
                    }

                    if (uiManager != null && uiManager.levelWin_panel != null)
                    {
                        _isTransitioning = true;
                        StartCoroutine(levelWinDelay());

                        IEnumerator levelWinDelay()
                        {
                            yield return new WaitForSeconds(0.6f);
                            if (uiManager != null) uiManager.safeAreaUI.SetActive(false);
                            if (uiManager != null) uiManager.levelWin_panel.SetActive(true);
                        }
                    }
                    else
                    {
                        StartCoroutine(ProceedToNextLevelCoroutine());
                    }
                }
            }
        }

        private IEnumerator ProceedToNextLevelCoroutine()
        {
            if (isLevelEditorManager)
            {
                Debug.Log("Level Editor Mode: Not proceeding to the next level automatically.");
                yield break;
            }

            _isTransitioning = true;
            yield return new WaitForSeconds(1f);

            if (levelList != null && currentLevelIndex + 1 < levelList.Count)
            {
                currentLevelIndex++;
                SaveCurrentLevelIndex();
                AdvanceOverallLevelNumber();

                if (backgroundImageManager != null) backgroundImageManager.UpdateBackground(_overallLevelNumber);

                UpdateCurrentLevelNumber();
                GenerateLevel();
            }
            else
            {
                if (enableRandomCycleOnEnd && levelList != null && levelList.Count > 0)
                {
                    ParseIgnoredIndicesAndBuildAllowed();
                    AdvanceOverallLevelNumber();

                    int nextIdx = GetRandomNextLevelIndex(excludeCurrent: true);
                    currentLevelIndex = nextIdx;
                    UpdateCurrentLevelNumber();
                    GenerateLevel();
                }
                else
                {
                    Debug.Log("You've finished all levels in the list!");
                    // Optional: open shop / show final screen
                }
            }

            _isTransitioning = false;
        }

        #endregion

        #region Helpers - Directions & UI Calls

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

        public void NextLevel()
        {
            if (uiManager != null && uiManager.levelWin_panel != null)
                uiManager.levelWin_panel.SetActive(false);

            _isTransitioning = true;

            if (levelList != null && currentLevelIndex + 1 < levelList.Count)
            {
                currentLevelIndex++;
                SaveCurrentLevelIndex();
                AdvanceOverallLevelNumber();

                if (backgroundImageManager != null) backgroundImageManager.UpdateBackground(_overallLevelNumber);

                UpdateCurrentLevelNumber();
                GenerateLevel();
            }
            else
            {
                if (enableRandomCycleOnEnd && levelList != null && levelList.Count > 0)
                {
                    ParseIgnoredIndicesAndBuildAllowed();
                    AdvanceOverallLevelNumber();
                    int nextIdx = GetRandomNextLevelIndex(excludeCurrent: true);
                    currentLevelIndex = nextIdx;
                    UpdateCurrentLevelNumber();
                    GenerateLevel();
                }
                else
                {
                    Debug.Log("No next level. You've completed all levels in the list!");
                }
            }

            _isTransitioning = false;
        }

        public void OpenShop()
        {
            if (uiManager != null && uiManager.shop_panel != null)
            {
                uiManager.shop_panel.SetActive(true);
            }
        }

        public void CloseLevelWin()
        {
            if (uiManager != null && uiManager.levelWin_panel != null)
            {
                uiManager.levelWin_panel.SetActive(false);
            }
            _isTransitioning = false;
        }

        public void CloseShop()
        {
            if (uiManager != null && uiManager.shop_panel != null)
            {
                uiManager.shop_panel.SetActive(false);
            }
        }

        #endregion

        #region Coins Persistence

        public int GetCoins()
        {
            return PlayerPrefs.GetInt(CoinsKey, 0);
        }

        private void SetCoins(int amount)
        {
            PlayerPrefs.SetInt(CoinsKey, amount);
            PlayerPrefs.Save();
            if (uiManager != null) uiManager.UpdateGlobalCoinsDisplay(amount);
        }

        public void AddCoins(int amount)
        {
            int newAmount = GetCoins() + amount;
            SetCoins(newAmount);
        }

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

        #endregion

        #region Remove / Smash Logic

        public void UseSmashHint()
        {
            if (smashCount <= 0)
            {
                if (notificationManager != null) notificationManager.ShowNotification("No smash items available", 2f);
                return;
            }

            Block candidate = null;
            foreach (var b in new List<Block>(_activeBlocks))
            {
                if (b == null) continue;
                RaycastHit hit;
                if (!Physics.Raycast(b.transform.position, b.transform.forward, out hit, _GetGridDetectionDistance(b)))
                {
                    candidate = b;
                    break;
                }
            }

            if (candidate == null)
            {
                if (notificationManager != null) notificationManager.ShowNotification("No smash target found", 2f);
                return;
            }

            smashCount--;
            PlayerPrefs.SetInt(SmashKey, smashCount);
            if (uiManager != null) uiManager.UpdatePowerUpCounts(smashCount);

            candidate.Smash();
        }

        public void StartRemoveMode()
        {
            if (smashCount <= 0)
            {
                if (notificationManager != null) notificationManager.ShowNotification("No smash items available", 2f);
                if (removePowerUI != null) removePowerUI.ToggleBuyPowerPanel();
                return;
            }

            if (_activeBlocks == null || _activeBlocks.Count == 0)
            {
                if (notificationManager != null) notificationManager.ShowNotification("No blocks to remove", 2f);
                return;
            }

            _awaitingRemoveSelection = true;
            if (notificationManager != null) notificationManager.ShowNotification("Select a block to remove", 5f);
        }

        public bool IsAwaitingRemove()
        {
            return _awaitingRemoveSelection;
        }

        public void ConfirmRemoveAtBlock(Block block)
        {
            if (!_awaitingRemoveSelection || block == null) return;

            if (smashCount <= 0)
            {
                if (notificationManager != null) notificationManager.ShowNotification("No smash items available", 2f);
                _awaitingRemoveSelection = false;
                return;
            }

            smashCount--;
            PlayerPrefs.SetInt(SmashKey, smashCount);
            if (uiManager != null) uiManager.UpdatePowerUpCounts(smashCount);

            block.Smash();
            _awaitingRemoveSelection = false;
        }

        private float _GetGridDetectionDistance(Block b)
        {
            return gridUnitSize * 0.6f;
        }

        #endregion

        #region Random Level Helpers

        private void ParseIgnoredIndicesAndBuildAllowed()
        {
            _ignoredIndices.Clear();
            _allowedRandomIndices.Clear();

            if (!string.IsNullOrWhiteSpace(ignoredLevelIndicesCSV))
            {
                var parts = ignoredLevelIndicesCSV.Split(',');
                foreach (var p in parts)
                {
                    string t = p.Trim();
                    if (string.IsNullOrEmpty(t)) continue;
                    if (!int.TryParse(t, out int val))
                    {
                        Debug.LogWarning($"Ignored level index '{t}' is not a valid integer and will be ignored.");
                        continue;
                    }
                    if (val < 0)
                    {
                        Debug.LogWarning($"Ignored level index '{val}' is negative and will be ignored.");
                        continue;
                    }
                    _ignoredIndices.Add(val);
                }
            }

            if (levelList != null && levelList.Count > 0)
            {
                for (int i = 0; i < levelList.Count; i++)
                {
                    if (!_ignoredIndices.Contains(i))
                        _allowedRandomIndices.Add(i);
                }
            }

            if ((_allowedRandomIndices == null || _allowedRandomIndices.Count == 0) && levelList != null && levelList.Count > 0)
            {
                Debug.LogWarning("No allowed random indices after parsing ignoredLevelIndicesCSV. Falling back to all level indices.");
                _allowedRandomIndices = new List<int>();
                for (int i = 0; i < levelList.Count; i++) _allowedRandomIndices.Add(i);
            }
        }

        private int GetRandomNextLevelIndex(bool excludeCurrent = true)
        {
            if (_allowedRandomIndices == null || _allowedRandomIndices.Count == 0)
            {
                if (levelList == null || levelList.Count == 0) return 0;
                return Random.Range(0, levelList.Count);
            }

            List<int> candidates = _allowedRandomIndices;
            if (excludeCurrent && candidates.Count > 1)
            {
                candidates = candidates.FindAll(i => i != currentLevelIndex);
                if (candidates.Count == 0)
                {
                    candidates = new List<int>(_allowedRandomIndices);
                }
            }

            int pick = candidates[Random.Range(0, candidates.Count)];
            return Mathf.Clamp(pick, 0, Mathf.Max(0, levelList != null ? levelList.Count - 1 : 0));
        }

        #endregion

        #region Persistence Helpers

        private void AdvanceOverallLevelNumber()
        {
            _overallLevelNumber = Mathf.Max(1, _overallLevelNumber) + 1;
            PlayerPrefs.SetInt(LevelNumberKey, _overallLevelNumber);
            PlayerPrefs.Save();
            UpdateCurrentLevelNumber();
        }

        private void SaveCurrentLevelIndex()
        {
            PlayerPrefs.SetInt(CurrentLevelIndexKey, currentLevelIndex);
            PlayerPrefs.Save();
        }

        public void ResetLevel()
        {
            if (isLevelEditorManager)
            {
                Debug.Log("Level Editor Mode: Reset level not performed.");
                return;
            }

            if (levelContainer != null)
            {
                foreach (Transform child in levelContainer) { Destroy(child.gameObject); }
            }
            _activeBlocks.Clear();

            if (shopManager != null && shopManager.selectedSkin != null)
            {
                foreach (var block in _activeBlocks)
                {
                    if (block != null)
                    {
                        block.ApplySkin(shopManager.selectedSkin.material);
                        block.arrowCollor = shopManager.selectedSkin.arrowColor;
                        block.ApplyArrowColor();
                    }
                }
            }

            if (uiManager != null)
            {
                uiManager.UpdateProgressByCounts(_initialBlockCount, 0);
            }

            Debug.Log("Level reset successfully.");
        }

        #endregion

        #region Moves Check

        public void CheckForAvailableMoves()
        {
            if (_activeBlocks == null || _activeBlocks.Count < 2) return;

            foreach (var block in _activeBlocks)
            {
                if (block == null) continue;
                Vector3 direction = block.transform.forward;
                if (!Physics.Raycast(block.transform.position, direction, _GetGridDetectionDistance(block)))
                {
                    return;
                }
            }

            if (notificationManager != null)
            {
                notificationManager.ShowNotification("No Moves! Use Remover or Restart!", 4f);
                if (removerPowerSimpleStepTutorialHand != null) removerPowerSimpleStepTutorialHand.ShowHand();
            }
        }

        #endregion
    }
}