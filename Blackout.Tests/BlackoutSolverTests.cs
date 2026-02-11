using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Blackout;

namespace Blackout.Tests
{
    [TestClass]
    public class BlackoutSolverTests
    {
        [TestMethod]
        public void Solve_SimpleGrid_ReturnsValidSolution()
        {
            var game = new BlackoutGame(3);
            game.Randomize(new Random(42));

            var solution = BlackoutSolver.Solve(game);
            Assert.IsNotNull(solution, "Solution should exist for a randomized board");
            Assert.IsTrue(solution.Count > 0, "Solution should have at least one move");

            // Apply the solution and verify it actually solves the puzzle
            foreach (var (row, col) in solution)
                game.ToggleCell(row, col);

            Assert.IsTrue(game.HasWon(), "Board should be solved after applying solution");
        }

        [TestMethod]
        public void Solve_AlreadySolved_ReturnsEmpty()
        {
            var game = new BlackoutGame(3);
            // Default state: all lights off = solved

            var solution = BlackoutSolver.Solve(game);
            Assert.IsNotNull(solution);
            Assert.AreEqual(0, solution.Count, "Already solved board needs 0 moves");
        }

        [TestMethod]
        public void GetHint_ReturnsFirstMove()
        {
            var game = new BlackoutGame(3);
            game.Randomize(new Random(42));

            var hint = BlackoutSolver.GetHint(game);
            Assert.IsTrue(hint.HasValue, "Hint should be available for an unsolved board");

            // The hint should be a valid cell
            Assert.IsTrue(hint.Value.row >= 0 && hint.Value.row < 3);
            Assert.IsTrue(hint.Value.col >= 0 && hint.Value.col < 3);
        }

        [TestMethod]
        public void GetHint_SolvedBoard_ReturnsNull()
        {
            var game = new BlackoutGame(3);
            var hint = BlackoutSolver.GetHint(game);
            Assert.IsNull(hint, "No hint needed for a solved board");
        }

        [TestMethod]
        public void GetStepByStep_SolvesWhenApplied()
        {
            var game = new BlackoutGame(4);
            game.Randomize(new Random(99));

            var steps = BlackoutSolver.GetStepByStepSolution(game);
            Assert.IsTrue(steps.Count > 0);

            foreach (var (row, col) in steps)
                game.ToggleCell(row, col);

            Assert.IsTrue(game.HasWon(), "Applying step-by-step solution should solve the puzzle");
        }

        [TestMethod]
        public void GenerateWithDifficulty_ProducesValidPuzzle()
        {
            var game = new BlackoutGame(4);
            BlackoutSolver.GenerateWithDifficulty(game, new Random(42), 5);

            Assert.IsFalse(game.HasWon(), "Generated puzzle should not be solved");

            // Verify it's solvable
            var solution = BlackoutSolver.Solve(game);
            Assert.IsNotNull(solution, "Generated puzzle should be solvable");
        }

        [TestMethod]
        public void Solve_WithDiagonalPattern_Works()
        {
            var game = new BlackoutGame(3, TogglePatternType.Diagonal);
            game.Randomize(new Random(42));

            var solution = BlackoutSolver.Solve(game);
            if (solution != null)
            {
                foreach (var (row, col) in solution)
                    game.ToggleCell(row, col);
                Assert.IsTrue(game.HasWon(), "Solver should work with diagonal pattern");
            }
            // Some diagonal puzzles may be unsolvable, which is also valid
        }
    }
}
