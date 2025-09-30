using UnityEngine;
using System.Collections.Generic;

// Enum-urile pentru tipul de bloc și orientare au fost eliminate.
public enum MoveDirection { Forward, Back, Up, Down, Left, Right }
public enum Difficulty { Easy, Normal, Hard, Custom }

[System.Serializable]
public class BlockData
{
    public Vector3Int position;
    public MoveDirection direction;
    // Câmpurile pentru tip și orientare au fost eliminate.
}

[CreateAssetMenu(fileName = "SimpleCubeLevel", menuName = "Unblock Cube/Simple Cube Level")]
public class LevelData : ScriptableObject
{
    [Header("Generation Settings")]
    public Difficulty difficulty;
    public int customGridSize = 4;
    public int seed = 0;
    // Am eliminat șansa pentru blocuri duble.

    [Header("Block Data")]
    [SerializeField] private List<BlockData> blocks = new List<BlockData>();

    public List<BlockData> GetBlocks() => blocks;

    public void Generate()
    {
        GenerateCubeShape();
    }

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

        // --- FAZA 1: UMPLEREA COMPLETĂ A VOLUMULUI CU BLOCURI SIMPLE ---
        // Algoritmul este acum foarte simplu: 3 bucle for care parcurg tot spațiul.
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    blocks.Add(new BlockData { position = new Vector3Int(x, y, z) });
                }
            }
        }

        // --- FAZA 2: ASIGNAREA DIRECȚIILOR (SOLVABILE) ---
        Vector3Int centerOffset = new Vector3Int(gridSize / 2, gridSize / 2, gridSize / 2);
        foreach (var block in blocks)
        {
            Vector3Int currentPos = block.position;
            Vector3Int centeredPos = currentPos - centerOffset;

            if (Mathf.Abs(centeredPos.x) >= Mathf.Abs(centeredPos.y) && Mathf.Abs(centeredPos.x) >= Mathf.Abs(centeredPos.z))
                block.direction = centeredPos.x >= 0 ? MoveDirection.Right : MoveDirection.Left;
            else if (Mathf.Abs(centeredPos.y) >= Mathf.Abs(centeredPos.x) && Mathf.Abs(centeredPos.y) >= Mathf.Abs(centeredPos.z))
                block.direction = centeredPos.y >= 0 ? MoveDirection.Up : MoveDirection.Down;
            else
                block.direction = centeredPos.z >= 0 ? MoveDirection.Forward : MoveDirection.Back;

            // Centram pozitia finală a blocului în jurul punctului (0,0,0)
            block.position -= centerOffset;
        }
    }
}