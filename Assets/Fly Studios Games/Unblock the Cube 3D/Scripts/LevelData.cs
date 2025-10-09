using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum MoveDirection { Forward, Back, Up, Down, Left, Right }
public enum Difficulty { Easy, Normal, Hard, Custom }

[System.Serializable]
public class BlockData
{
    public Vector3Int position;
    public MoveDirection direction;
    public Quaternion randomVisualRotation;
}

[CreateAssetMenu(fileName = "SimpleCubeLevel", menuName = "Unblock Cube/Simple Cube Level")]
public class LevelData : ScriptableObject
{
    [Header("Generation Settings")]
    public Difficulty difficulty;
    public int customGridSize = 4;
    public int seed = 0;

    [Header("Block Data")]
    [SerializeField] private List<BlockData> blocks = new List<BlockData>();

    public List<BlockData> GetBlocks() => blocks;

    // --- NOU: Funcția care lipsea ---
    // Această metodă returnează mărimea gridului pe baza dificultății selectate.
    public int GetGridSize()
    {
        switch (difficulty)
        {
            case Difficulty.Easy: return 3;
            case Difficulty.Normal: return 5;
            case Difficulty.Hard: return 7;
            case Difficulty.Custom: return Mathf.Max(2, customGridSize);
            default: return 5;
        }
    }

    public void Generate() { GenerateCubeShape(); }

    private void GenerateCubeShape()
    {
        blocks.Clear();
        Random.InitState(seed);
        int gridSize = GetGridSize();

        List<BlockData> generatedBlocks = new List<BlockData>();
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    generatedBlocks.Add(new BlockData { position = new Vector3Int(x, y, z) });
                }
            }
        }

        HashSet<Vector3Int> allPositions = new HashSet<Vector3Int>(generatedBlocks.Select(b => b.position));
        List<BlockData> unassignedBlocks = new List<BlockData>(generatedBlocks);
        unassignedBlocks = unassignedBlocks.OrderBy(b => Random.value).ToList();

        while (unassignedBlocks.Count > 0)
        {
            bool assignedOne = false;
            for (int i = unassignedBlocks.Count - 1; i >= 0; i--)
            {
                BlockData currentBlock = unassignedBlocks[i];
                MoveDirection? clearDirection = FindClearPath(currentBlock.position, allPositions, gridSize);

                if (clearDirection.HasValue)
                {
                    currentBlock.direction = clearDirection.Value;
                    allPositions.Remove(currentBlock.position);
                    unassignedBlocks.RemoveAt(i);
                    assignedOne = true;
                }
            }
            if (!assignedOne && unassignedBlocks.Count > 0)
            {
                unassignedBlocks[0].direction = (MoveDirection)Random.Range(0, 6);
                allPositions.Remove(unassignedBlocks[0].position);
                unassignedBlocks.RemoveAt(0);
            }
        }

        foreach (var block in generatedBlocks)
        {
            block.randomVisualRotation = Quaternion.Euler(
                Random.Range(0, 4) * 90,
                Random.Range(0, 4) * 90,
                Random.Range(0, 4) * 90
            );
        }

        blocks = new List<BlockData>(generatedBlocks);
    }

    private MoveDirection? FindClearPath(Vector3Int blockPos, HashSet<Vector3Int> occupied, int gridSize)
    {
        var shuffledDirections = GetDirectionVectors().OrderBy(d => Random.value);
        foreach (var dir in shuffledDirections)
        {
            Vector3Int currentPos = blockPos;
            bool pathIsClear = true;
            while (IsInBounds(currentPos + dir, gridSize))
            {
                currentPos += dir;
                if (occupied.Contains(currentPos))
                {
                    pathIsClear = false;
                    break;
                }
            }
            if (pathIsClear) { return GetEnumFromVector(dir); }
        }
        return null;
    }

    private Vector3Int[] GetDirectionVectors() => new[] {
        Vector3Int.forward, Vector3Int.back, Vector3Int.up,
        Vector3Int.down, Vector3Int.left, Vector3Int.right
    };

    private bool IsInBounds(Vector3Int pos, int gridSize) =>
        pos.x >= 0 && pos.x < gridSize && pos.y >= 0 && pos.y < gridSize && pos.z >= 0 && pos.z < gridSize;

    private MoveDirection GetEnumFromVector(Vector3Int dir)
    {
        if (dir == Vector3Int.forward) return MoveDirection.Forward;
        if (dir == Vector3Int.back) return MoveDirection.Back;
        if (dir == Vector3Int.up) return MoveDirection.Up;
        if (dir == Vector3Int.down) return MoveDirection.Down;
        if (dir == Vector3Int.left) return MoveDirection.Left;
        return MoveDirection.Right;
    }
}

