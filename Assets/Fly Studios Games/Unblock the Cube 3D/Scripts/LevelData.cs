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
    // NOU: Câmp pentru a stoca rotația vizuală aleatorie
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

        // --- FAZA 1: UMPLEREA VOLUMULUI ---
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

        // --- FAZA 2: ASIGNAREA DIRECȚIILOR (Algoritm Îmbunătățit) ---
        // Acest algoritm creează lanțuri de dependențe pentru un puzzle mai complex.

        // Stocăm toate pozițiile pentru verificări rapide
        HashSet<Vector3Int> allPositions = new HashSet<Vector3Int>(generatedBlocks.Select(b => b.position));
        // O listă a blocurilor care încă nu au o direcție asignată
        List<BlockData> unassignedBlocks = new List<BlockData>(generatedBlocks);

        // Amestecăm blocurile pentru a procesa într-o ordine aleatorie
        unassignedBlocks = unassignedBlocks.OrderBy(b => Random.value).ToList();

        // Cât timp mai avem blocuri de asignat
        while (unassignedBlocks.Count > 0)
        {
            bool assignedOne = false;
            // Parcurgem lista de la coadă la cap pentru a putea șterge elemente în siguranță
            for (int i = unassignedBlocks.Count - 1; i >= 0; i--)
            {
                BlockData currentBlock = unassignedBlocks[i];

                // Găsim o direcție liberă pentru blocul curent
                MoveDirection? clearDirection = FindClearPath(currentBlock.position, allPositions, gridSize);

                if (clearDirection.HasValue)
                {
                    // Am găsit o cale liberă. Asta înseamnă că blocul curent este la exterior.
                    // Îl asignăm pe el și îl ștergem din "lumea" noastră virtuală.
                    currentBlock.direction = clearDirection.Value;
                    allPositions.Remove(currentBlock.position); // Simulăm eliminarea lui
                    unassignedBlocks.RemoveAt(i); // Îl scoatem din lista de așteptare
                    assignedOne = true;
                }
            }

            // Mecanism de siguranță: dacă într-o iterație completă nu găsim niciun bloc de eliminat
            // (ceea ce nu ar trebui să se întâmple), spargem bucla pentru a evita un loop infinit.
            if (!assignedOne && unassignedBlocks.Count > 0)
            {
                // Asignăm o direcție aleatorie la un bloc rămas pentru a debloca situația
                unassignedBlocks[0].direction = (MoveDirection)Random.Range(0, 6);
                allPositions.Remove(unassignedBlocks[0].position);
                unassignedBlocks.RemoveAt(0);
            }
        }

        // --- FAZA 2.5: Generăm rotația vizuală aleatorie ---
        foreach (var block in generatedBlocks)
        {
            block.randomVisualRotation = Quaternion.Euler(
                Random.Range(0, 4) * 90,
                Random.Range(0, 4) * 90,
                Random.Range(0, 4) * 90
            );
        }

        // --- FAZA 3: CENTRAREA POZIȚIILOR ---
        Vector3Int centerOffset = new Vector3Int(gridSize / 2, gridSize / 2, gridSize / 2);
        foreach (var block in generatedBlocks)
        {
            block.position -= centerOffset;
            blocks.Add(block);
        }
    }

    // --- Funcție nouă: Găsește o singură cale liberă pentru un bloc ---
    private MoveDirection? FindClearPath(Vector3Int blockPos, HashSet<Vector3Int> occupied, int gridSize)
    {
        Vector3Int[] allDirectionVectors = {
            Vector3Int.forward, Vector3Int.back, Vector3Int.up,
            Vector3Int.down, Vector3Int.left, Vector3Int.right
        };

        // Amestecăm direcțiile pentru a nu avea o preferință (ex: mereu 'dreapta')
        var shuffledDirections = allDirectionVectors.OrderBy(d => Random.value);

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
            if (pathIsClear)
            {
                return GetEnumFromVector(dir); // Returnăm prima direcție liberă găsită
            }
        }

        return null; // Nicio cale liberă găsită
    }

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

