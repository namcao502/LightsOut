using System;
using System.Collections.Generic;

namespace Blackout
{
    /// <summary>
    /// Solves Blackout puzzles using Gaussian elimination over GF(2).
    /// </summary>
    public static class BlackoutSolver
    {
        /// <summary>
        /// Solves the current game state.
        /// Returns a list of cells to click, or null if unsolvable.
        /// </summary>
        public static List<(int row, int col)> Solve(BlackoutGame game)
        {
            if (game == null) throw new ArgumentNullException(nameof(game));

            int rows = game.Rows;
            int cols = game.Cols;
            int n = rows * cols;

            var (rowOffsets, colOffsets) = TogglePattern.GetOffsets(game.Pattern);

            var matrix = new int[n, n + 1];

            for (int clickIdx = 0; clickIdx < n; clickIdx++)
            {
                int cr = clickIdx / cols;
                int cc = clickIdx % cols;
                for (int k = 0; k < rowOffsets.Length; k++)
                {
                    int nr = cr + rowOffsets[k];
                    int nc = cc + colOffsets[k];
                    if (nr >= 0 && nr < rows && nc >= 0 && nc < cols)
                        matrix[nr * cols + nc, clickIdx] = 1;
                }
            }

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    matrix[r * cols + c, n] = game.IsLightOn(r, c) ? 1 : 0;

            var pivotCol = new int[n];
            for (int i = 0; i < n; i++) pivotCol[i] = -1;

            int pivotRow = 0;
            for (int col = 0; col < n && pivotRow < n; col++)
            {
                int found = -1;
                for (int row = pivotRow; row < n; row++)
                {
                    if (matrix[row, col] == 1) { found = row; break; }
                }
                if (found == -1) continue;

                if (found != pivotRow)
                {
                    for (int j = 0; j <= n; j++)
                    {
                        int tmp = matrix[pivotRow, j];
                        matrix[pivotRow, j] = matrix[found, j];
                        matrix[found, j] = tmp;
                    }
                }

                pivotCol[pivotRow] = col;

                for (int row = 0; row < n; row++)
                {
                    if (row != pivotRow && matrix[row, col] == 1)
                    {
                        for (int j = 0; j <= n; j++)
                            matrix[row, j] ^= matrix[pivotRow, j];
                    }
                }
                pivotRow++;
            }

            for (int row = pivotRow; row < n; row++)
            {
                if (matrix[row, n] == 1)
                    return null;
            }

            var solution = new int[n];
            for (int row = 0; row < pivotRow; row++)
            {
                if (pivotCol[row] != -1)
                    solution[pivotCol[row]] = matrix[row, n];
            }

            var result = new List<(int row, int col)>();
            for (int i = 0; i < n; i++)
            {
                if (solution[i] == 1)
                    result.Add((i / cols, i % cols));
            }
            return result;
        }

        public static (int row, int col)? GetHint(BlackoutGame game)
        {
            if (game.HasWon()) return null;
            var solution = Solve(game);
            if (solution == null || solution.Count == 0) return null;
            return solution[0];
        }

        public static List<(int row, int col)> GetStepByStepSolution(BlackoutGame game)
        {
            return Solve(game) ?? new List<(int row, int col)>();
        }

        public static void GenerateWithDifficulty(BlackoutGame game, Random random, int targetMoves)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (targetMoves <= 0) throw new ArgumentOutOfRangeException(nameof(targetMoves));

            int maxAttempts = 200;
            int bestDiff = int.MaxValue;
            bool[,] bestBoard = null;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                game.Randomize(random, Math.Max(1, targetMoves - 3), targetMoves + 5);
                var solution = Solve(game);
                if (solution != null)
                {
                    int diff = Math.Abs(solution.Count - targetMoves);
                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestBoard = game.GetBoardSnapshot();
                        if (diff == 0) break;
                    }
                }
            }

            if (bestBoard != null)
                game.LoadBoard(bestBoard);
        }
    }
}
