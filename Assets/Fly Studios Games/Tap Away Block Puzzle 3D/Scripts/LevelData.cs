using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Tap_Away_Block_Puzzle_3D
{
    /// <summary>
    /// Direction a block can move.
    /// </summary>
    public enum MoveDirection { Forward, Back, Up, Down, Left, Right }

    public enum Difficulty { Custom }

    [System.Serializable]
    public class BlockData
    {
        public Vector3Int position;
        public MoveDirection direction;
        public Quaternion randomVisualRotation;
    }

    [CreateAssetMenu(fileName = "LevelData", menuName = "TapAway/LevelData")]
    public class LevelData : ScriptableObject
    {
        #region Generation Settings

        [Header("Generation Settings")]
        [HideInInspector]
        [Range(2, 10)]
        public int customGridLength = 3; // Grid length

        [HideInInspector]
        [Range(2, 10)]
        public int customGridHeight = 3; // Grid height

        [HideInInspector]
        public int seed = 0;

        [SerializeField]
        [HideInInspector]
        private List<BlockData> blocks = new List<BlockData>();

        private const int MinGridSize = 2;
        private const int MaxGridSize = 10;

        #endregion

        public List<BlockData> GetBlocks() => blocks ?? (blocks = new List<BlockData>());

        public int GetGridLength() => Mathf.Clamp(customGridLength, MinGridSize, MaxGridSize);
        public int GetGridHeight() => Mathf.Clamp(customGridHeight, MinGridSize, MaxGridSize);

        /// <summary>
        /// Entry point for level generation. Wraps generation in a try/catch to surface errors.
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
        /// Builds a solvable level by constructing a forward "release" order, preventing cycles.
        /// The algorithm finds blocks that can be released based on previously cleared positions.
        /// </summary>
        private void GenerateSolvableLevel()
        {
            blocks = new List<BlockData>();
            Random.InitState(seed);
            int gridLength = GetGridLength();
            int gridHeight = GetGridHeight();

            if (gridLength <= 0 || gridHeight <= 0)
            {
                Debug.LogWarning("Invalid grid dimensions for generation.");
                return;
            }

            // 1. Create a block for every cell in the grid
            List<BlockData> generatedBlocks = new List<BlockData>();
            int offsetLength = gridLength / 2;
            int offsetHeight = gridHeight / 2;
            for (int x = -offsetLength; x < gridLength - offsetLength; x++)
            {
                for (int y = -offsetHeight; y < gridHeight - offsetHeight; y++)
                {
                    for (int z = -offsetLength; z < gridLength - offsetLength; z++)
                    {
                        generatedBlocks.Add(new BlockData { position = new Vector3Int(x, y, z) });
                    }
                }
            }

            // 2. Prepare data structures for the forward pass algorithm
            List<BlockData> remainingBlocks = new List<BlockData>(generatedBlocks);
            HashSet<Vector3Int> clearedPositions = new HashSet<Vector3Int>(); // positions considered "free"

            // 3. Assign directions in passes, building the solution path (prevents cycles)
            while (remainingBlocks.Count > 0)
            {
                List<BlockData> blocksClearedThisPass = new List<BlockData>();

                // Shuffle to avoid deterministic solutions
                remainingBlocks = remainingBlocks.OrderBy(b => Random.value).ToList();

                // Phase 1: find ALL blocks that can be released this pass based on previous state
                foreach (var currentBlock in remainingBlocks)
                {
                    MoveDirection? possibleDirection = FindForwardPath(currentBlock.position, clearedPositions, gridLength);

                    if (possibleDirection.HasValue)
                    {
                        currentBlock.direction = possibleDirection.Value;
                        blocksClearedThisPass.Add(currentBlock);
                    }
                }

                if (blocksClearedThisPass.Count == 0 && remainingBlocks.Count > 0)
                {
                    Debug.LogError($"Failed to generate a solvable level. Deadlock detected with {remainingBlocks.Count} blocks left.");
                    break;
                }

                // Phase 2: commit cleared blocks for this pass
                foreach (var block in blocksClearedThisPass)
                {
                    clearedPositions.Add(block.position);
                    remainingBlocks.Remove(block);
                }
            }

            // 4. Set random visual rotations for aesthetics
            foreach (var block in generatedBlocks)
            {
                block.randomVisualRotation = Quaternion.Euler(
                    Random.Range(0, 4) * 90f,
                    Random.Range(0, 4) * 90f,
                    Random.Range(0, 4) * 90f
                );
            }

            this.blocks = generatedBlocks;
            Debug.Log($"Level successfully generated – {blocks.Count} blocks placed. Dimensions: {gridLength}x{gridHeight}.");
        }

        /// <summary>
        /// Searches for a forward move. A valid path leads outside the grid (exit) or to an already cleared position.
        /// </summary>
        private MoveDirection? FindForwardPath(Vector3Int blockPos, HashSet<Vector3Int> clearedPositions, int gridSize)
        {
            var shuffledDirections = System.Enum.GetValues(typeof(MoveDirection))
                                                .Cast<MoveDirection>()
                                                .OrderBy(d => Random.value);

            foreach (var dir in shuffledDirections)
            {
                Vector3Int targetPos = blockPos + GetVectorFromEnum(dir);

                // Valid if it goes outside the grid (an exit)
                if (!IsInBounds(targetPos, gridSize))
                {
                    return dir;
                }

                // Or if it goes to a position already cleared
                if (clearedPositions.Contains(targetPos))
                {
                    return dir;
                }
            }

            return null;
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
}