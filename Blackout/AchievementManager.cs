using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Blackout
{
    [DataContract]
    public class Achievement
    {
        [DataMember] public string Id { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public string Description { get; set; }
        [DataMember] public bool Unlocked { get; set; }
        [DataMember] public string UnlockedDate { get; set; }
    }

    [DataContract]
    public class AchievementData
    {
        [DataMember]
        public List<Achievement> Achievements { get; set; } = new List<Achievement>();

        [DataMember]
        public List<int> SolvedSizes { get; set; } = new List<int>();

        [DataMember]
        public List<int> SolvedPatterns { get; set; } = new List<int>();

        [DataMember]
        public int PerfectSolveCount { get; set; }
    }

    /// <summary>
    /// Manages achievement tracking and persistence in %AppData%/Blackout/achievements.json.
    /// </summary>
    public class AchievementManager
    {
        private static readonly (string id, string name, string desc)[] Definitions =
        {
            ("first_win",     "First Win",        "Solve any puzzle"),
            ("speed_demon",   "Speed Demon",      "Solve in under 10 seconds"),
            ("minimalist",    "Minimalist",       "Solve in minimum possible moves"),
            ("no_undo",       "No Undo",          "Solve without using undo"),
            ("size_master",   "Size Master",      "Solve on every grid size 2 through 10"),
            ("pattern_exp",   "Pattern Explorer", "Solve with each toggle pattern"),
            ("perfectionist", "Perfectionist",    "Solve 5 different puzzles with minimum moves"),
        };

        /// <summary>
        /// Raised when a new achievement is unlocked.
        /// </summary>
        public event Action<Achievement> AchievementUnlocked;

        private readonly string filePath;
        private AchievementData data;

        public AchievementManager()
        {
            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Blackout");
            Directory.CreateDirectory(appData);
            filePath = Path.Combine(appData, "achievements.json");
            Load();
        }

        public List<Achievement> GetAll()
        {
            return data.Achievements.ToList();
        }

        /// <summary>
        /// Checks and unlocks achievements based on the completed game state.
        /// </summary>
        public void CheckAndUnlock(BlackoutGame game, int seconds, bool usedUndo, int? optimalMoves)
        {
            Unlock("first_win");

            if (seconds < 10)
                Unlock("speed_demon");

            if (!usedUndo)
                Unlock("no_undo");

            if (optimalMoves.HasValue && game.MoveCount <= optimalMoves.Value)
            {
                Unlock("minimalist");
                data.PerfectSolveCount++;
                if (data.PerfectSolveCount >= 5)
                    Unlock("perfectionist");
            }

            // Track solved grid sizes (square grids only)
            if (game.Rows == game.Cols && !data.SolvedSizes.Contains(game.Rows))
            {
                data.SolvedSizes.Add(game.Rows);
                bool allSizes = true;
                for (int s = 2; s <= 10; s++)
                {
                    if (!data.SolvedSizes.Contains(s))
                    {
                        allSizes = false;
                        break;
                    }
                }
                if (allSizes)
                    Unlock("size_master");
            }

            // Track solved patterns
            int patternIdx = (int)game.Pattern;
            if (!data.SolvedPatterns.Contains(patternIdx))
            {
                data.SolvedPatterns.Add(patternIdx);
                if (data.SolvedPatterns.Count >= Enum.GetValues(typeof(TogglePatternType)).Length)
                    Unlock("pattern_exp");
            }

            Save();
        }

        private void Unlock(string id)
        {
            var achievement = data.Achievements.FirstOrDefault(a => a.Id == id);
            if (achievement != null && !achievement.Unlocked)
            {
                achievement.Unlocked = true;
                achievement.UnlockedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                AchievementUnlocked?.Invoke(achievement);
            }
        }

        private void Load()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var serializer = new DataContractJsonSerializer(typeof(AchievementData));
                    using (var stream = File.OpenRead(filePath))
                        data = (AchievementData)serializer.ReadObject(stream);
                }
            }
            catch { /* Corrupted file, start fresh */ }

            if (data == null || data.Achievements.Count == 0)
            {
                data = new AchievementData
                {
                    Achievements = new List<Achievement>(
                        Definitions.Select(d => new Achievement
                        {
                            Id = d.id,
                            Name = d.name,
                            Description = d.desc,
                            Unlocked = false
                        }))
                };
            }
        }

        private void Save()
        {
            try
            {
                var serializer = new DataContractJsonSerializer(typeof(AchievementData));
                using (var stream = File.Create(filePath))
                    serializer.WriteObject(stream, data);
            }
            catch { /* Ignore write failures */ }
        }
    }
}
