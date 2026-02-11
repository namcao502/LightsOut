using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Blackout
{
    /// <summary>
    /// Serializable snapshot of a game for save/load functionality.
    /// </summary>
    [DataContract]
    public class GameState
    {
        [DataMember] public int Rows { get; set; }
        [DataMember] public int Cols { get; set; }
        [DataMember] public int PatternIndex { get; set; }
        [DataMember] public int MoveCount { get; set; }
        [DataMember] public int ElapsedSeconds { get; set; }
        [DataMember] public bool[] BoardFlat { get; set; }

        public TogglePatternType Pattern
        {
            get => (TogglePatternType)PatternIndex;
            set => PatternIndex = (int)value;
        }

        /// <summary>
        /// Creates a GameState from the current game and timer state.
        /// </summary>
        public static GameState FromGame(BlackoutGame game, int elapsedSeconds)
        {
            var board = game.GetBoardSnapshot();
            var flat = new bool[game.Rows * game.Cols];
            for (int r = 0; r < game.Rows; r++)
                for (int c = 0; c < game.Cols; c++)
                    flat[r * game.Cols + c] = board[r, c];

            return new GameState
            {
                Rows = game.Rows,
                Cols = game.Cols,
                Pattern = game.Pattern,
                MoveCount = game.MoveCount,
                ElapsedSeconds = elapsedSeconds,
                BoardFlat = flat
            };
        }

        /// <summary>
        /// Converts the flat board array back to a 2D array.
        /// </summary>
        public bool[,] GetBoard()
        {
            var board = new bool[Rows, Cols];
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    board[r, c] = BoardFlat[r * Cols + c];
            return board;
        }

        public void Save(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            var serializer = new DataContractJsonSerializer(typeof(GameState));
            using (var stream = File.Create(path))
                serializer.WriteObject(stream, this);
        }

        public static GameState Load(string path)
        {
            var serializer = new DataContractJsonSerializer(typeof(GameState));
            using (var stream = File.OpenRead(path))
                return (GameState)serializer.ReadObject(stream);
        }
    }
}
