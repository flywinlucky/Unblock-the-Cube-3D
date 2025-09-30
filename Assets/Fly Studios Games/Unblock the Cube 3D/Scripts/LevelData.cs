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

    public void Generate() { GenerateCubeShape(); }

    private void GenerateCubeShape()
    {
        blocks.Clear();
        Random.InitState(seed);

        int gridSize;
        switch (difficulty)
        {
            case Difficulty.Easy: gridSize = 3; break;
            case Difficulty.Normal: gridSize = 5; break;
            case Difficulty.Hard: gridSize = 7; break;
            case Difficulty.Custom: gridSize = Mathf.Max(2, customGridSize); break;
            default: gridSize = 5; break;
        }

        // --- FAZA 1: UMPLEREA VOLUMULUI (rămâne la fel) ---
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

        // --- FAZA 2: ALGORITM NOU BAZAT PE RAYCAST SIMULAT ---

        // Creăm un HashSet cu toate pozițiile ocupate pentru verificări rapide
        HashSet<Vector3Int> occupiedPositions = new HashSet<Vector3Int>(generatedBlocks.Select(b => b.position));

        // O listă cu toți vectorii de direcție posibili
        Vector3Int[] allDirectionVectors = {
            Vector3Int.forward, Vector3Int.back, Vector3Int.up,
            Vector3Int.down, Vector3Int.left, Vector3Int.right
        };

        foreach (var block in generatedBlocks)
        {
            List<MoveDirection> validDirections = new List<MoveDirection>();

            // Pentru fiecare bloc, verificăm toate cele 6 direcții posibile
            foreach (var directionVector in allDirectionVectors)
            {
                // Simulăm un "raycast" în direcția curentă
                Vector3Int currentPos = block.position;
                bool pathIsClear = true;
                while (IsInBounds(currentPos + directionVector, gridSize))
                {
                    currentPos += directionVector;
                    if (occupiedPositions.Contains(currentPos))
                    {
                        // Am lovit un alt bloc, calea nu este liberă
                        pathIsClear = false;
                        break;
                    }
                }

                // Dacă am ajuns la capăt fără a lovi nimic, direcția e validă
                if (pathIsClear)
                {
                    validDirections.Add(GetEnumFromVector(directionVector));
                }
            }

            // Alegem o direcție aleatorie din cele valide găsite
            if (validDirections.Count > 0)
            {
                block.direction = validDirections[Random.Range(0, validDirections.Count)];
            }
            else
            {
                // Fallback pentru blocurile din centru care nu au nicio cale liberă
                // Le dăm o direcție aleatorie
                block.direction = (MoveDirection)Random.Range(0, 6);
            }
        }

        // --- FAZA 3: CENTRAREA POZIȚIILOR ---
        Vector3Int centerOffset = new Vector3Int(gridSize / 2, gridSize / 2, gridSize / 2);
        foreach (var block in generatedBlocks)
        {
            block.position -= centerOffset;
            blocks.Add(block);
        }
    }

    // Funcții ajutătoare
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