using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Blackout
{
    [DataContract]
    public class HighScoreEntry
    {
        [DataMember] public int Rows { get; set; }
        [DataMember] public int Cols { get; set; }
        [DataMember] public int Moves { get; set; }
        [DataMember] public int Seconds { get; set; }
        [DataMember] public string Date { get; set; }
    }

    [DataContract]
    public class HighScoreData
    {
        [DataMember]
        public List<HighScoreEntry> Entries { get; set; } = new List<HighScoreEntry>();
    }

    /// <summary>
    /// Manages local high score persistence in %AppData%/Blackout/highscores.json.
    /// Tracks best (fewest moves, fastest time) per grid size.
    /// </summary>
    public class HighScoreManager
    {
        private readonly string filePath;
        private HighScoreData data;

        public HighScoreManager()
        {
            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Blackout");
            Directory.CreateDirectory(appData);
            filePath = Path.Combine(appData, "highscores.json");
            Load();
        }

        /// <summary>
        /// Records a score if it beats the current best for that grid size.
        /// Returns true if this is a new high score.
        /// </summary>
        public bool RecordScore(int rows, int cols, int moves, int seconds)
        {
            var existing = GetBestScore(rows, cols);
            if (existing == null || moves < existing.Moves ||
                (moves == existing.Moves && seconds < existing.Seconds))
            {
                data.Entries.RemoveAll(e => e.Rows == rows && e.Cols == cols);
                data.Entries.Add(new HighScoreEntry
                {
                    Rows = rows,
                    Cols = cols,
                    Moves = moves,
                    Seconds = seconds,
                    Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm")
                });
                Save();
                return true;
            }
            return false;
        }

        public HighScoreEntry GetBestScore(int rows, int cols)
        {
            return data.Entries.FirstOrDefault(e => e.Rows == rows && e.Cols == cols);
        }

        public List<HighScoreEntry> GetAllScores()
        {
            return data.Entries.OrderBy(e => e.Rows).ThenBy(e => e.Cols).ToList();
        }

        private void Load()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var serializer = new DataContractJsonSerializer(typeof(HighScoreData));
                    using (var stream = File.OpenRead(filePath))
                        data = (HighScoreData)serializer.ReadObject(stream);
                }
            }
            catch { /* Corrupted file, start fresh */ }

            if (data == null)
                data = new HighScoreData();
        }

        private void Save()
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(HighScoreData));
                using (var stream = File.Create(filePath))
                    serializer.WriteObject(stream, data);
            }
            catch { /* Ignore write failures */ }
        }
    }
}
