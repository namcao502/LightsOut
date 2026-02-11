using System;

namespace Blackout
{
    /// <summary>
    /// Available toggle patterns for the Lights Out puzzle.
    /// </summary>
    public enum TogglePatternType
    {
        Cross,
        Diagonal,
        XShape,
        Plus3
    }

    /// <summary>
    /// Provides offset arrays for each toggle pattern type.
    /// </summary>
    public static class TogglePattern
    {
        // Cross: self + up, right, down, left
        private static readonly int[] CrossRows = { 0, -1, 0, 1, 0 };
        private static readonly int[] CrossCols = { 0, 0, 1, 0, -1 };

        // Diagonal: self + 4 diagonal neighbors
        private static readonly int[] DiagRows = { 0, -1, -1, 1, 1 };
        private static readonly int[] DiagCols = { 0, -1, 1, 1, -1 };

        // XShape: self + all 8 surrounding neighbors
        private static readonly int[] XRows = { 0, -1, -1, -1, 0, 1, 1, 1 };
        private static readonly int[] XCols = { 0, -1, 0, 1, 1, 1, 0, -1 };

        // Plus3: self + 2 cells in each cardinal direction
        private static readonly int[] Plus3Rows = { 0, -1, -2, 0, 0, 1, 2, 0, 0 };
        private static readonly int[] Plus3Cols = { 0, 0, 0, 1, 2, 0, 0, -1, -2 };

        /// <summary>
        /// Returns the (rowOffset[], colOffset[]) arrays for the given pattern type.
        /// </summary>
        public static (int[] rows, int[] cols) GetOffsets(TogglePatternType type)
        {
            switch (type)
            {
                case TogglePatternType.Cross:    return (CrossRows, CrossCols);
                case TogglePatternType.Diagonal: return (DiagRows, DiagCols);
                case TogglePatternType.XShape:   return (XRows, XCols);
                case TogglePatternType.Plus3:    return (Plus3Rows, Plus3Cols);
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        /// <summary>
        /// Returns a user-friendly display name for the pattern.
        /// </summary>
        public static string GetDisplayName(TogglePatternType type)
        {
            switch (type)
            {
                case TogglePatternType.Cross:    return "Cross (+)";
                case TogglePatternType.Diagonal: return "Diagonal (X)";
                case TogglePatternType.XShape:   return "All 8 (*)";
                case TogglePatternType.Plus3:    return "Plus-3 (++)";
                default: return type.ToString();
            }
        }
    }
}
