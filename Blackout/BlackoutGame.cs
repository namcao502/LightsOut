using System;
using System.Collections.Generic;

namespace Blackout
{
    /// <summary>
    /// Core game logic for the Blackout puzzle.
    /// Supports rectangular grids, configurable toggle patterns,
    /// move counting, undo, and board serialization.
    /// </summary>
    public class BlackoutGame
    {
        private readonly bool[,] lights;
        private readonly Stack<(int row, int col)> moveHistory = new Stack<(int, int)>();
        private readonly TogglePatternType patternType;
        private readonly int[] rowOffsets;
        private readonly int[] colOffsets;

        public int Rows { get; }
        public int Cols { get; }
        public int MoveCount { get; private set; }
        public TogglePatternType Pattern => patternType;
        public bool CanUndo => moveHistory.Count > 0;

        /// <summary>
        /// Convenience property for square grids. Throws if grid is not square.
        /// </summary>
        public int GridSize
        {
            get
            {
                if (Rows != Cols)
                    throw new InvalidOperationException("Grid is not square. Use Rows and Cols instead.");
                return Rows;
            }
        }

        /// <summary>
        /// Creates a square grid game with the specified toggle pattern.
        /// </summary>
        public BlackoutGame(int gridSize, TogglePatternType pattern = TogglePatternType.Cross)
            : this(gridSize, gridSize, pattern)
        {
        }

        /// <summary>
        /// Creates a rectangular grid game with the specified toggle pattern.
        /// </summary>
        public BlackoutGame(int rows, int cols, TogglePatternType pattern = TogglePatternType.Cross)
        {
            if (rows <= 0)
                throw new ArgumentOutOfRangeException(nameof(rows), "Rows must be positive.");
            if (cols <= 0)
                throw new ArgumentOutOfRangeException(nameof(cols), "Cols must be positive.");

            Rows = rows;
            Cols = cols;
            patternType = pattern;
            lights = new bool[rows, cols];

            var offsets = TogglePattern.GetOffsets(pattern);
            rowOffsets = offsets.rows;
            colOffsets = offsets.cols;
        }

        /// <summary>
        /// Returns whether the light at the given position is on.
        /// Throws if coordinates are out of bounds.
        /// </summary>
        public bool IsLightOn(int row, int col)
        {
            ValidateBounds(row, col);
            return lights[row, col];
        }

        /// <summary>
        /// Toggles the cell at (row, col) and its neighbors according to the pattern.
        /// Tracks the move for undo and increments MoveCount.
        /// </summary>
        public void ToggleCell(int row, int col)
        {
            ToggleCellInternal(row, col);
            moveHistory.Push((row, col));
            MoveCount++;
        }

        /// <summary>
        /// Undoes the last move. Returns the coordinates of the undone move.
        /// Throws if there are no moves to undo.
        /// </summary>
        public (int row, int col) Undo()
        {
            if (!CanUndo)
                throw new InvalidOperationException("No moves to undo.");

            var last = moveHistory.Pop();
            ToggleCellInternal(last.row, last.col);
            MoveCount--;
            return last;
        }

        /// <summary>
        /// Returns true if all lights are off (player wins).
        /// </summary>
        public bool HasWon()
        {
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Cols; j++)
                    if (lights[i, j])
                        return false;
            return true;
        }

        /// <summary>
        /// Sets all lights to the given state. Resets move count and history.
        /// </summary>
        public void SetAll(bool on)
        {
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Cols; j++)
                    lights[i, j] = on;
            ResetTracking();
        }

        /// <summary>
        /// Sets an individual light without toggling neighbors.
        /// Intended for the puzzle editor.
        /// </summary>
        public void SetLight(int row, int col, bool on)
        {
            ValidateBounds(row, col);
            lights[row, col] = on;
        }

        /// <summary>
        /// Creates a random solvable puzzle. Resets move count and history.
        /// Throws if random is null or click range is invalid.
        /// </summary>
        public void Randomize(Random random, int minClicks = 5, int maxClicks = 15)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));
            if (minClicks < 0)
                throw new ArgumentOutOfRangeException(nameof(minClicks), "Must be non-negative.");
            if (minClicks > maxClicks)
                throw new ArgumentOutOfRangeException(nameof(maxClicks), "maxClicks must be >= minClicks.");

            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Cols; j++)
                    lights[i, j] = false;

            int clicks = random.Next(minClicks, maxClicks + 1);
            for (int c = 0; c < clicks; c++)
                ToggleCellInternal(random.Next(Rows), random.Next(Cols));

            if (HasWon())
                ToggleCellInternal(random.Next(Rows), random.Next(Cols));

            ResetTracking();
        }

        /// <summary>
        /// Returns a copy of the current board state.
        /// </summary>
        public bool[,] GetBoardSnapshot()
        {
            var snapshot = new bool[Rows, Cols];
            Array.Copy(lights, snapshot, lights.Length);
            return snapshot;
        }

        /// <summary>
        /// Restores the board from a snapshot. Resets move count and history.
        /// </summary>
        public void LoadBoard(bool[,] board)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));
            if (board.GetLength(0) != Rows || board.GetLength(1) != Cols)
                throw new ArgumentException("Board dimensions must match game dimensions.");

            Array.Copy(board, lights, lights.Length);
            ResetTracking();
        }

        private void ToggleCellInternal(int row, int col)
        {
            for (int k = 0; k < rowOffsets.Length; k++)
            {
                int newRow = row + rowOffsets[k];
                int newCol = col + colOffsets[k];
                if (newRow >= 0 && newRow < Rows && newCol >= 0 && newCol < Cols)
                    lights[newRow, newCol] = !lights[newRow, newCol];
            }
        }

        private void ResetTracking()
        {
            MoveCount = 0;
            moveHistory.Clear();
        }

        private void ValidateBounds(int row, int col)
        {
            if (row < 0 || row >= Rows)
                throw new ArgumentOutOfRangeException(nameof(row),
                    $"Row must be between 0 and {Rows - 1}.");
            if (col < 0 || col >= Cols)
                throw new ArgumentOutOfRangeException(nameof(col),
                    $"Col must be between 0 and {Cols - 1}.");
        }
    }
}
