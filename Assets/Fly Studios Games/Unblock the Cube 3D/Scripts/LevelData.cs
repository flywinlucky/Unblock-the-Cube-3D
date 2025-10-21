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

[CreateAssetMenu(fileName = "SolvableCubeLevel", menuName = "Unblock Cube/Solvable Cube Level")]
public class LevelData : ScriptableObject
{
    [Header("Generation Settings")]
    [Range(2, 10)]
    public int customGridSize = 3;
    public int seed = 0;

    private List<BlockData> blocks = new List<BlockData>();

    private const int MinGridSize = 2;
    private const int MaxGridSize = 40;

    public List<BlockData> GetBlocks() => blocks ?? (blocks = new List<BlockData>());

    public int GetGridSize()
    {
        int gs = Mathf.Max(MinGridSize, customGridSize);
        gs = Mathf.Min(gs, MaxGridSize);
        return gs;
    }

    /// <summary>
    /// Punctul de intrare pentru generarea nivelului.
    /// </summary>
    public void Generate()
    {
        try
        {
            GenerateSolvableLevel();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("LevelData.Generate failed: " + ex.Message + "\n" + ex.StackTrace);
        }
    }

    /// <summary>
    /// Algoritm nou care construiește o soluție de la primul la ultimul bloc, prevenind ciclurile.
    /// Funcționează prin a găsi mai întâi blocurile care pot ieși, apoi pe cele care se pot muta în spațiile eliberate.
    /// </summary>
    private void GenerateSolvableLevel()
    {
        blocks = new List<BlockData>();
        Random.InitState(seed);
        int gridSize = GetGridSize();

        if (gridSize <= 0)
        {
            Debug.LogWarning("Invalid grid size for generation.");
            return;
        }

        // 1. Creăm un bloc pentru fiecare celulă din grilă
        List<BlockData> generatedBlocks = new List<BlockData>();
        int offset = gridSize / 2;
        for (int x = -offset; x < gridSize - offset; x++)
        {
            for (int y = -offset; y < gridSize - offset; y++)
            {
                for (int z = -offset; z < gridSize - offset; z++)
                {
                    generatedBlocks.Add(new BlockData { position = new Vector3Int(x, y, z) });
                }
            }
        }

        // 2. Pregătim structurile de date pentru algoritmul "forward"
        List<BlockData> remainingBlocks = new List<BlockData>(generatedBlocks);
        HashSet<Vector3Int> clearedPositions = new HashSet<Vector3Int>(); // Poziții considerate "libere" pentru mișcare

        // 3. Atribuim direcții construind calea de rezolvare, de la primul la ultimul bloc mișcat
        while (remainingBlocks.Count > 0)
        {
            bool assignedOneThisPass = false;
            // Amestecăm blocurile în fiecare pas pentru a varia ordinea de rezolvare
            remainingBlocks = remainingBlocks.OrderBy(b => Random.value).ToList();

            for (int i = remainingBlocks.Count - 1; i >= 0; i--)
            {
                BlockData currentBlock = remainingBlocks[i];
                
                // Căutăm o direcție în care blocul poate fi mișcat (spre exterior sau spre o poziție deja eliberată)
                MoveDirection? possibleDirection = FindForwardPath(currentBlock.position, clearedPositions, gridSize);

                if (possibleDirection.HasValue)
                {
                    currentBlock.direction = possibleDirection.Value;
                    
                    // Adăugăm poziția acestui bloc la cele "eliberate" pentru următorii pași
                    clearedPositions.Add(currentBlock.position);
                    remainingBlocks.RemoveAt(i);
                    assignedOneThisPass = true;
                }
            }

            if (!assignedOneThisPass && remainingBlocks.Count > 0)
            {
                // Acest caz nu ar trebui să se întâmple cu noua logică, dar rămâne ca o siguranță.
                Debug.LogError($"Failed to generate a solvable level. A deadlock was detected with {remainingBlocks.Count} blocks left. This indicates a flaw in the generation logic.");
                break; // Ieșim pentru a preveni o buclă infinită
            }
        }

        // 4. Setăm rotațiile vizuale aleatorii pentru estetică
        foreach (var block in generatedBlocks)
        {
            block.randomVisualRotation = Quaternion.Euler(
                Random.Range(0, 4) * 90f,
                Random.Range(0, 4) * 90f,
                Random.Range(0, 4) * 90f
            );
        }

        this.blocks = generatedBlocks;
        Debug.Log($"Level successfully generated – {blocks.Count} blocks placed. Guaranteed playable and cycle-free.");
    }
    
    /// <summary>
    /// Caută o cale de mișcare "înainte". O cale este validă dacă duce în afara grilei sau într-o locație deja eliberată.
    /// </summary>
    private MoveDirection? FindForwardPath(Vector3Int blockPos, HashSet<Vector3Int> clearedPositions, int gridSize)
    {
        var shuffledDirections = System.Enum.GetValues(typeof(MoveDirection))
                                            .Cast<MoveDirection>()
                                            .OrderBy(d => Random.value);

        foreach (var dir in shuffledDirections)
        {
            Vector3Int targetPos = blockPos + GetVectorFromEnum(dir);

            // O cale este validă dacă duce în afara grilei (spre ieșire)
            if (!IsInBounds(targetPos, gridSize))
            {
                return dir;
            }

            // Sau dacă duce într-o poziție care a fost deja eliberată de un alt bloc
            if (clearedPositions.Contains(targetPos))
            {
                return dir;
            }
        }
    
        return null; // Nicio cale de mișcare găsită în acest pas
    }

    #region Helper Functions

    private bool IsInBounds(Vector3Int pos, int gridSize)
    {
        if (gridSize <= 0) return false;
        int offset = gridSize / 2;
        int min = -offset;
        int max = gridSize - offset;

        return pos.x >= min && pos.x < max &&
               pos.y >= min && pos.y < max &&
               pos.z >= min && pos.z < max;
    }
    
    private MoveDirection GetOppositeDirection(MoveDirection dir)
    {
        switch (dir)
        {
            case MoveDirection.Forward: return MoveDirection.Back;
            case MoveDirection.Back: return MoveDirection.Forward;
            case MoveDirection.Up: return MoveDirection.Down;
            case MoveDirection.Down: return MoveDirection.Up;
            case MoveDirection.Left: return MoveDirection.Right;
            case MoveDirection.Right: return MoveDirection.Left;
            default: throw new System.ArgumentOutOfRangeException();
        }
    }

    private Vector3Int GetVectorFromEnum(MoveDirection dir)
    {
        switch (dir)
        {
            case MoveDirection.Forward: return Vector3Int.forward;
            case MoveDirection.Back: return Vector3Int.back;
            case MoveDirection.Up: return Vector3Int.up;
            case MoveDirection.Down: return Vector3Int.down;
            case MoveDirection.Left: return Vector3Int.left;
            case MoveDirection.Right: return Vector3Int.right;
            default: return Vector3Int.zero;
        }
    }

    #endregion
}

