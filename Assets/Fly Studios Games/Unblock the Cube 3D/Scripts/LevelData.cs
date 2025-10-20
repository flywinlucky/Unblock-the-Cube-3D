using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum MoveDirection { Forward, Back, Up, Down, Left, Right }
public enum Difficulty { Custom }

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
    private Difficulty difficulty = Difficulty.Custom;
    public int customGridSize = 0;
    public int seed = 0;

    private List<BlockData> blocks = new List<BlockData>();

    private const int MinGridSize = 2;
    private const int MaxGridSize = 40; // limităm pentru a evita operațiuni prea mari în editor/build

    public List<BlockData> GetBlocks() => blocks ?? (blocks = new List<BlockData>());

    public int GetGridSize()
    {
        // Asigurăm o valoare validă între MinGridSize și MaxGridSize
        int gs = Mathf.Max(MinGridSize, customGridSize);
        gs = Mathf.Min(gs, MaxGridSize);
        return gs;
    }

    public void Generate() 
    { 
        try
        {
            GenerateCubeShape();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("LevelData.Generate failed: " + ex.Message);
        }
    }

    private void GenerateCubeShape()
    {
        blocks = new List<BlockData>();
        Random.InitState(seed);
        int gridSize = GetGridSize();

        if (gridSize <= 0)
        {
            Debug.LogWarning("Invalid grid size for GenerateCubeShape.");
            return;
        }

        int offset = gridSize / 2;

        List<BlockData> generatedBlocks = new List<BlockData>();

        // Defensive cap to avoid alocări uriașe
        long totalCells = (long)gridSize * gridSize * gridSize;
        if (totalCells > 2000000) // prag foarte mare
        {
            Debug.LogWarning($"Grid too large ({totalCells} cells). Generation aborted.");
            return;
        }

        for (int x = -offset; x < gridSize - offset; x++)
        {
            for (int y = -offset; y < gridSize - offset; y++)
            {
                for (int z = -offset; z < gridSize - offset; z++)
                {
                    generatedBlocks.Add(new BlockData { position = new Vector3Int(x, y, z), randomVisualRotation = Quaternion.identity });
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
                // fallback: assignăm o direcție aleatoare
                unassignedBlocks[0].direction = (MoveDirection)Random.Range(0, 6);
                allPositions.Remove(unassignedBlocks[0].position);
                unassignedBlocks.RemoveAt(0);
            }
        }

        foreach (var block in generatedBlocks)
        {
            // asigurăm rotație validă
            block.randomVisualRotation = Quaternion.Euler(
                Random.Range(0, 4) * 90f,
                Random.Range(0, 4) * 90f,
                Random.Range(0, 4) * 90f
            );
        }

        blocks = new List<BlockData>(generatedBlocks);
    }

    private MoveDirection? FindClearPath(Vector3Int blockPos, HashSet<Vector3Int> occupied, int gridSize)
    {
        if (occupied == null || gridSize <= 0) return null;
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

    // ▼▼▼ MODIFICARE CHEIE AICI ▼▼▼
    private bool IsInBounds(Vector3Int pos, int gridSize)
    {
        if (gridSize <= 0) return false;
        // Recalculăm limitele pe baza aceluiași offset
        int offset = gridSize / 2;
        int min = -offset;
        int max = gridSize - offset;

        // Verificăm dacă poziția se află în noul cub centrat
        return pos.x >= min && pos.x < max &&
               pos.y >= min && pos.y < max &&
               pos.z >= min && pos.z < max;
    }

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