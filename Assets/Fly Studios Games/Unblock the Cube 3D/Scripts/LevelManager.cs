using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public LevelData currentLevelData;

    [Header("Prefabs")]
    public GameObject singleBlockPrefab;

    [Header("Grid Settings")]
    public float gridUnitSize = 0.5f;

    public Transform levelContainer;

    private List<Block> _activeBlocks = new List<Block>();

    void Start() { GenerateLevel(); }

    public void GenerateLevel()
    {
        if (currentLevelData == null || singleBlockPrefab == null)
        {
            Debug.LogError("Level Data or Single Block Prefab is not set!");
            return;
        }

        foreach (Transform child in levelContainer) { Destroy(child.gameObject); }
        _activeBlocks.Clear();

        // Bucla de generare este acum mult mai simplă
        foreach (BlockData data in currentLevelData.GetBlocks())
        {
            Vector3 worldPosition = (Vector3)data.position * gridUnitSize;
            Quaternion finalRotation = GetStableLookRotation(data.direction);

            // Instantiem direct prefab-ul singular
            GameObject newBlockObj = Instantiate(singleBlockPrefab, worldPosition, finalRotation, levelContainer);

            Block blockScript = newBlockObj.GetComponent<Block>();
            // CORECTAT: Am adăugat parametrul 'gridUnitSize' care lipsea
            blockScript.Initialize(data.direction, this, gridUnitSize);
            _activeBlocks.Add(blockScript);
        }
    }

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

    public void OnBlockRemoved(Block block) { _activeBlocks.Remove(block); CheckWinCondition(); }
    private void CheckWinCondition() { if (_activeBlocks.Count == 0) { Debug.Log("Felicitări! Ai câștigat nivelul!"); } }
}

