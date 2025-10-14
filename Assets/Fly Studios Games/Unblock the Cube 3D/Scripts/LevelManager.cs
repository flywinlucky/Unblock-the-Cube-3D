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

    [Header("Grid Settings")]
    public float gridUnitSize = 0.5f;

    private List<Block> _activeBlocks = new List<Block>();

    // Flag pentru a evita multiple tranziții simultane
    private bool _isTransitioning = false;

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
            if (!_isTransitioning)
            {
                StartCoroutine(ProceedToNextLevelCoroutine());
            }
            // Aici poți adăuga logica pentru a trece la nivelul următor
            // De exemplu:
            // _currentLevelNumber++;
            // currentLevelData = LoadNextLevelData(); // O funcție ipotetică
            // GenerateLevel();
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
}