// LevelManager.cs
using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    [Header("Level Configuration")]
    public LevelData currentLevelData;
    private int _currentLevelNumber = 1; // Începem cu nivelul 1

    [Header("Object References")]
    public GameObject singleBlockPrefab;
    public Transform levelContainer;
    public UIManager uiManager; // Referință către UIManager

    [Header("Grid Settings")]
    public float gridUnitSize = 0.5f;

    private List<Block> _activeBlocks = new List<Block>();
    private int _totalBlocksInLevel; // NOU: Stocăm numărul total de blocuri

    void Start()
    {
        if (uiManager == null)
        {
            Debug.LogError("UIManager nu este asignat în LevelManager!");
            return;
        }
        GenerateLevel();
    }

    public void GenerateLevel()
    {
        if (currentLevelData == null || singleBlockPrefab == null)
        {
            Debug.LogError("Level Data or Single Block Prefab is not set!");
            return;
        }

        // Curățăm nivelul anterior
        foreach (Transform child in levelContainer) { Destroy(child.gameObject); }
        _activeBlocks.Clear();

        // Obținem lista de blocuri și stocăm numărul total
        List<BlockData> blocksToGenerate = currentLevelData.GetBlocks();
        _totalBlocksInLevel = blocksToGenerate.Count;

        // --- Integrare UI ---
        // Actualizăm afișajul nivelului și resetăm bara de progres
        uiManager.UpdateLevelDisplay(_currentLevelNumber);
        uiManager.UpdateProgressBar(_totalBlocksInLevel, _totalBlocksInLevel);
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
            // --- Integrare UI ---
            // Actualizăm bara de progres de fiecare dată când un bloc este eliminat
            uiManager.UpdateProgressBar(_activeBlocks.Count, _totalBlocksInLevel);
            // --------------------
            CheckWinCondition();
        }
    }

    private void CheckWinCondition()
    {
        if (_activeBlocks.Count == 0)
        {
            Debug.Log($"Felicitări! Ai câștigat nivelul {_currentLevelNumber}!");
            // Aici poți adăuga logica pentru a trece la nivelul următor
            // De exemplu:
            // _currentLevelNumber++;
            // currentLevelData = LoadNextLevelData(); // O funcție ipotetică
            // GenerateLevel();
        }
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