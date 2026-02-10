using System;

namespace WindowsFormsApp_LightsOut
{
    /// <summary>
    /// Core game logic for the Lights Out puzzle.
    /// The board is a grid of lights that can be on or off.
    /// Clicking a cell toggles it and its orthogonal neighbors.
    /// The goal is to turn all lights off.
    /// </summary>
    public class LightsOutGame
    {
        // Direction offsets: up, right, down, left, self
        private static readonly int[] RowOffset = { -1, 0, 1, 0, 0 };
        private static readonly int[] ColOffset = { 0, 1, 0, -1, 0 };

        private readonly int gridSize;
        private readonly bool[,] lights;

        public int GridSize => gridSize;

        public LightsOutGame(int gridSize)
        {
            if (gridSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(gridSize), "Grid size must be positive.");

            this.gridSize = gridSize;
            lights = new bool[gridSize, gridSize];
        }

        /// <summary>
        /// Returns whether the light at the given position is on.
        /// </summary>
        public bool IsLightOn(int row, int col) => lights[row, col];

        /// <summary>
        /// Toggles the light at (row, col) and its orthogonal neighbors.
        /// </summary>
        public void ToggleCell(int row, int col)
        {
            for (int k = 0; k < RowOffset.Length; k++)
            {
                int newRow = row + RowOffset[k];
                int newCol = col + ColOffset[k];
                if (newRow >= 0 && newRow < gridSize && newCol >= 0 && newCol < gridSize)
                    lights[newRow, newCol] = !lights[newRow, newCol];
            }
        }

        /// <summary>
        /// Returns true if all lights are off (player wins).
        /// </summary>
        public bool HasWon()
        {
            for (int i = 0; i < gridSize; i++)
                for (int j = 0; j < gridSize; j++)
                    if (lights[i, j])
                        return false;
            return true;
        }

        /// <summary>
        /// Sets all lights to the given state.
        /// </summary>
        public void SetAll(bool on)
        {
            for (int i = 0; i < gridSize; i++)
                for (int j = 0; j < gridSize; j++)
                    lights[i, j] = on;
        }

        /// <summary>
        /// Creates a random solvable puzzle by starting from a solved state
        /// and applying random clicks. This guarantees solvability.
        /// </summary>
        public void Randomize(Random random, int minClicks = 5, int maxClicks = 15)
        {
            // Start from solved state (all off)
            SetAll(false);

            int clicks = random.Next(minClicks, maxClicks + 1);
            for (int c = 0; c < clicks; c++)
                ToggleCell(random.Next(gridSize), random.Next(gridSize));

            // Ensure at least one light is on so the puzzle isn't already solved
            if (HasWon())
                ToggleCell(random.Next(gridSize), random.Next(gridSize));
        }
    }
}
