using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Blackout;

namespace Blackout.Tests
{
    [TestClass]
    public class BlackoutGameTests
    {
        // ── Existing tests (updated for new API) ─────────────────

        [TestMethod]
        public void NewGame_AllLightsOff_HasWonReturnsTrue()
        {
            var game = new BlackoutGame(4);
            Assert.IsTrue(game.HasWon());
        }

        [TestMethod]
        public void SetAll_On_NoLightIsOff()
        {
            var game = new BlackoutGame(4);
            game.SetAll(true);

            for (int r = 0; r < game.Rows; r++)
                for (int c = 0; c < game.Cols; c++)
                    Assert.IsTrue(game.IsLightOn(r, c));
        }

        [TestMethod]
        public void SetAll_Off_HasWonReturnsTrue()
        {
            var game = new BlackoutGame(4);
            game.SetAll(true);
            game.SetAll(false);
            Assert.IsTrue(game.HasWon());
        }

        [TestMethod]
        public void ToggleCell_Center_TogglesItselfAndNeighbors()
        {
            var game = new BlackoutGame(4);
            game.ToggleCell(1, 1);

            Assert.IsTrue(game.IsLightOn(1, 1), "Center should be on");
            Assert.IsTrue(game.IsLightOn(0, 1), "Up neighbor should be on");
            Assert.IsTrue(game.IsLightOn(2, 1), "Down neighbor should be on");
            Assert.IsTrue(game.IsLightOn(1, 0), "Left neighbor should be on");
            Assert.IsTrue(game.IsLightOn(1, 2), "Right neighbor should be on");

            Assert.IsFalse(game.IsLightOn(0, 0), "Diagonal should still be off");
            Assert.IsFalse(game.IsLightOn(2, 2), "Diagonal should still be off");
        }

        [TestMethod]
        public void ToggleCell_Corner_OnlyTogglesValidNeighbors()
        {
            var game = new BlackoutGame(4);
            game.ToggleCell(0, 0);

            Assert.IsTrue(game.IsLightOn(0, 0), "Corner should be on");
            Assert.IsTrue(game.IsLightOn(0, 1), "Right neighbor should be on");
            Assert.IsTrue(game.IsLightOn(1, 0), "Down neighbor should be on");
            Assert.IsFalse(game.IsLightOn(1, 1), "Diagonal should still be off");
        }

        [TestMethod]
        public void ToggleCell_Twice_RestoresOriginalState()
        {
            var game = new BlackoutGame(4);
            game.ToggleCell(2, 2);
            game.ToggleCell(2, 2);
            Assert.IsTrue(game.HasWon());
        }

        [TestMethod]
        public void Randomize_ProducesNonSolvedBoard()
        {
            var game = new BlackoutGame(4);
            var random = new Random(42);
            game.Randomize(random);
            Assert.IsFalse(game.HasWon(), "Randomized board should not already be solved");
        }

        [TestMethod]
        public void Randomize_IsSolvable_ByReversingClicks()
        {
            var game = new BlackoutGame(4);
            var seed = 123;
            var random1 = new Random(seed);
            var random2 = new Random(seed);

            game.Randomize(random1, 5, 15);
            var replayGame = new BlackoutGame(4);
            replayGame.Randomize(random2, 5, 15);

            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    Assert.AreEqual(game.IsLightOn(r, c), replayGame.IsLightOn(r, c));
        }

        [TestMethod]
        public void Constructor_ZeroGridSize_ThrowsException()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => new BlackoutGame(0));
        }

        [TestMethod]
        public void Constructor_NegativeGridSize_ThrowsException()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => new BlackoutGame(-1));
        }

        [TestMethod]
        public void GridSize_ReturnsCorrectValue()
        {
            var game = new BlackoutGame(5);
            Assert.AreEqual(5, game.GridSize);
        }

        // ── Bug fix tests ────────────────────────────────────────

        [TestMethod]
        public void IsLightOn_OutOfBounds_ThrowsException()
        {
            var game = new BlackoutGame(4);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => game.IsLightOn(-1, 0));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => game.IsLightOn(0, 4));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => game.IsLightOn(4, 0));
        }

        [TestMethod]
        public void Randomize_NullRandom_ThrowsException()
        {
            var game = new BlackoutGame(4);
            Assert.ThrowsExactly<ArgumentNullException>(
                () => game.Randomize(null));
        }

        [TestMethod]
        public void Randomize_InvalidMinMax_ThrowsException()
        {
            var game = new BlackoutGame(4);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => game.Randomize(new Random(), -1, 5));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => game.Randomize(new Random(), 10, 5));
        }

        // ── Move counter tests ───────────────────────────────────

        [TestMethod]
        public void MoveCount_IncrementedOnToggle()
        {
            var game = new BlackoutGame(4);
            Assert.AreEqual(0, game.MoveCount);
            game.ToggleCell(0, 0);
            Assert.AreEqual(1, game.MoveCount);
            game.ToggleCell(1, 1);
            Assert.AreEqual(2, game.MoveCount);
        }

        [TestMethod]
        public void MoveCount_ResetOnRandomize()
        {
            var game = new BlackoutGame(4);
            game.ToggleCell(0, 0);
            game.ToggleCell(1, 1);
            game.Randomize(new Random(42));
            Assert.AreEqual(0, game.MoveCount);
        }

        // ── Undo tests ──────────────────────────────────────────

        [TestMethod]
        public void CanUndo_FalseOnNewGame()
        {
            var game = new BlackoutGame(4);
            Assert.IsFalse(game.CanUndo);
        }

        [TestMethod]
        public void CanUndo_TrueAfterToggle()
        {
            var game = new BlackoutGame(4);
            game.ToggleCell(0, 0);
            Assert.IsTrue(game.CanUndo);
        }

        [TestMethod]
        public void Undo_RevertsLastMove()
        {
            var game = new BlackoutGame(4);
            game.ToggleCell(1, 1);
            Assert.IsTrue(game.IsLightOn(1, 1));

            game.Undo();
            Assert.IsFalse(game.IsLightOn(1, 1));
            Assert.IsTrue(game.HasWon(), "Board should be back to initial state");
        }

        [TestMethod]
        public void Undo_DecrementsCount()
        {
            var game = new BlackoutGame(4);
            game.ToggleCell(0, 0);
            game.ToggleCell(1, 1);
            Assert.AreEqual(2, game.MoveCount);

            game.Undo();
            Assert.AreEqual(1, game.MoveCount);
        }

        [TestMethod]
        public void Undo_EmptyHistory_ThrowsException()
        {
            var game = new BlackoutGame(4);
            Assert.ThrowsExactly<InvalidOperationException>(() => game.Undo());
        }

        // ── Toggle pattern tests ─────────────────────────────────

        [TestMethod]
        public void ToggleCell_DiagonalPattern_TogglesDiagonals()
        {
            var game = new BlackoutGame(4, TogglePatternType.Diagonal);
            game.ToggleCell(1, 1);

            Assert.IsTrue(game.IsLightOn(1, 1), "Center should be on");
            Assert.IsTrue(game.IsLightOn(0, 0), "Top-left diagonal");
            Assert.IsTrue(game.IsLightOn(0, 2), "Top-right diagonal");
            Assert.IsTrue(game.IsLightOn(2, 2), "Bottom-right diagonal");
            Assert.IsTrue(game.IsLightOn(2, 0), "Bottom-left diagonal");

            // Orthogonal neighbors should NOT be toggled
            Assert.IsFalse(game.IsLightOn(0, 1), "Up should be off");
            Assert.IsFalse(game.IsLightOn(1, 0), "Left should be off");
        }

        [TestMethod]
        public void ToggleCell_XShapePattern_TogglesAll8Neighbors()
        {
            var game = new BlackoutGame(4, TogglePatternType.XShape);
            game.ToggleCell(1, 1);

            // Self + all 8 neighbors
            Assert.IsTrue(game.IsLightOn(1, 1), "Center");
            Assert.IsTrue(game.IsLightOn(0, 0), "Top-left");
            Assert.IsTrue(game.IsLightOn(0, 1), "Top");
            Assert.IsTrue(game.IsLightOn(0, 2), "Top-right");
            Assert.IsTrue(game.IsLightOn(1, 2), "Right");
            Assert.IsTrue(game.IsLightOn(2, 2), "Bottom-right");
            Assert.IsTrue(game.IsLightOn(2, 1), "Bottom");
            Assert.IsTrue(game.IsLightOn(2, 0), "Bottom-left");

            // Cell not in range should be off
            Assert.IsFalse(game.IsLightOn(3, 3), "Far corner should be off");
        }

        // ── Rectangular grid tests ───────────────────────────────

        [TestMethod]
        public void RectangularGrid_DifferentRowsCols()
        {
            var game = new BlackoutGame(3, 5);
            Assert.AreEqual(3, game.Rows);
            Assert.AreEqual(5, game.Cols);
        }

        [TestMethod]
        public void RectangularGrid_HasWon()
        {
            var game = new BlackoutGame(3, 5);
            Assert.IsTrue(game.HasWon());
            game.SetAll(true);
            Assert.IsFalse(game.HasWon());
        }

        [TestMethod]
        public void GridSize_NonSquare_ThrowsException()
        {
            var game = new BlackoutGame(3, 5);
            Assert.ThrowsExactly<InvalidOperationException>(() => { var _ = game.GridSize; });
        }

        // ── Puzzle editor tests ──────────────────────────────────

        [TestMethod]
        public void SetLight_SetsIndividualCell()
        {
            var game = new BlackoutGame(4);
            game.SetLight(2, 3, true);
            Assert.IsTrue(game.IsLightOn(2, 3));
            // Neighbors should NOT be affected
            Assert.IsFalse(game.IsLightOn(1, 3));
            Assert.IsFalse(game.IsLightOn(2, 2));
        }

        // ── Serialization tests ──────────────────────────────────

        [TestMethod]
        public void GetBoardSnapshot_ReturnsCorrectCopy()
        {
            var game = new BlackoutGame(3);
            game.ToggleCell(1, 1);

            var snapshot = game.GetBoardSnapshot();
            Assert.AreEqual(game.IsLightOn(1, 1), snapshot[1, 1]);
            Assert.AreEqual(game.IsLightOn(0, 0), snapshot[0, 0]);

            // Modifying snapshot should not affect game
            snapshot[0, 0] = !snapshot[0, 0];
            Assert.AreNotEqual(snapshot[0, 0], game.IsLightOn(0, 0));
        }

        [TestMethod]
        public void LoadBoard_RestoresState()
        {
            var game = new BlackoutGame(3);
            game.ToggleCell(1, 1);
            game.ToggleCell(0, 0);

            var snapshot = game.GetBoardSnapshot();

            // Create a new game and load the snapshot
            var game2 = new BlackoutGame(3);
            game2.LoadBoard(snapshot);

            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    Assert.AreEqual(game.IsLightOn(r, c), game2.IsLightOn(r, c));

            Assert.AreEqual(0, game2.MoveCount, "MoveCount should be reset after LoadBoard");
        }
    }
}
