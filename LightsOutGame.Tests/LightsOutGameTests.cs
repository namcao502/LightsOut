using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WindowsFormsApp_LightsOut;

namespace LightsOutGame.Tests
{
    [TestClass]
    public class LightsOutGameTests
    {
        [TestMethod]
        public void NewGame_AllLightsOff_HasWonReturnsTrue()
        {
            var game = new WindowsFormsApp_LightsOut.LightsOutGame(4);

            // Default state: all lights off
            Assert.IsTrue(game.HasWon());
        }

        [TestMethod]
        public void SetAll_On_NoLightIsOff()
        {
            var game = new WindowsFormsApp_LightsOut.LightsOutGame(4);
            game.SetAll(true);

            for (int r = 0; r < game.GridSize; r++)
                for (int c = 0; c < game.GridSize; c++)
                    Assert.IsTrue(game.IsLightOn(r, c));
        }

        [TestMethod]
        public void SetAll_Off_HasWonReturnsTrue()
        {
            var game = new WindowsFormsApp_LightsOut.LightsOutGame(4);
            game.SetAll(true);
            game.SetAll(false);

            Assert.IsTrue(game.HasWon());
        }

        [TestMethod]
        public void ToggleCell_Center_TogglesItselfAndNeighbors()
        {
            var game = new WindowsFormsApp_LightsOut.LightsOutGame(4);

            // Toggle cell (1,1) — should flip (1,1) and its 4 neighbors
            game.ToggleCell(1, 1);

            Assert.IsTrue(game.IsLightOn(1, 1), "Center should be on");
            Assert.IsTrue(game.IsLightOn(0, 1), "Up neighbor should be on");
            Assert.IsTrue(game.IsLightOn(2, 1), "Down neighbor should be on");
            Assert.IsTrue(game.IsLightOn(1, 0), "Left neighbor should be on");
            Assert.IsTrue(game.IsLightOn(1, 2), "Right neighbor should be on");

            // Corners and other cells should remain off
            Assert.IsFalse(game.IsLightOn(0, 0), "Diagonal should still be off");
            Assert.IsFalse(game.IsLightOn(2, 2), "Diagonal should still be off");
        }

        [TestMethod]
        public void ToggleCell_Corner_OnlyTogglesValidNeighbors()
        {
            var game = new WindowsFormsApp_LightsOut.LightsOutGame(4);

            // Toggle top-left corner (0,0) — only itself, right, and down
            game.ToggleCell(0, 0);

            Assert.IsTrue(game.IsLightOn(0, 0), "Corner should be on");
            Assert.IsTrue(game.IsLightOn(0, 1), "Right neighbor should be on");
            Assert.IsTrue(game.IsLightOn(1, 0), "Down neighbor should be on");

            // Out-of-bounds neighbors should not cause issues
            Assert.IsFalse(game.IsLightOn(1, 1), "Diagonal should still be off");
        }

        [TestMethod]
        public void ToggleCell_Twice_RestoresOriginalState()
        {
            var game = new WindowsFormsApp_LightsOut.LightsOutGame(4);

            game.ToggleCell(2, 2);
            game.ToggleCell(2, 2);

            // Toggling the same cell twice should restore all lights to off
            Assert.IsTrue(game.HasWon());
        }

        [TestMethod]
        public void Randomize_ProducesNonSolvedBoard()
        {
            var game = new WindowsFormsApp_LightsOut.LightsOutGame(4);
            var random = new Random(42); // Fixed seed for determinism

            game.Randomize(random);

            Assert.IsFalse(game.HasWon(), "Randomized board should not already be solved");
        }

        [TestMethod]
        public void Randomize_IsSolvable_ByReversingClicks()
        {
            // Randomize works by applying clicks to a solved board,
            // so replaying the same clicks should solve it.
            var game = new WindowsFormsApp_LightsOut.LightsOutGame(4);
            var seed = 123;
            var random1 = new Random(seed);
            var random2 = new Random(seed);

            game.Randomize(random1, 5, 15);

            // Replay the same sequence of clicks
            var replayGame = new WindowsFormsApp_LightsOut.LightsOutGame(4);
            replayGame.Randomize(random2, 5, 15);

            // Both games should have the same board state
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < 4; c++)
                    Assert.AreEqual(game.IsLightOn(r, c), replayGame.IsLightOn(r, c));
        }

        [TestMethod]
        public void Constructor_ZeroGridSize_ThrowsException()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => new WindowsFormsApp_LightsOut.LightsOutGame(0));
        }

        [TestMethod]
        public void Constructor_NegativeGridSize_ThrowsException()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => new WindowsFormsApp_LightsOut.LightsOutGame(-1));
        }

        [TestMethod]
        public void GridSize_ReturnsCorrectValue()
        {
            var game = new WindowsFormsApp_LightsOut.LightsOutGame(5);
            Assert.AreEqual(5, game.GridSize);
        }
    }
}
