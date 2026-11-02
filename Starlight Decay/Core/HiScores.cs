using System.IO;

namespace Starlight_Decay
{
    public class HiScoreEntry
    {
        public string Name;
        public int Score;
    }

    /// <summary>Top five scores, saved as alternating name and score lines in local app data.</summary>
    public class HiScores
    {
        public const int MaxEntries = 5;

        private readonly string _filePath;

        public List<HiScoreEntry> Entries { get; } = new List<HiScoreEntry>();

        public HiScores()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _filePath = Path.Combine(appData, "StarlightDecay", "hiScore.sav");
        }

        public void Load()
        {
            Entries.Clear();
            if (!File.Exists(_filePath))
            {
                return;
            }

            string[] lines = File.ReadAllLines(_filePath);
            for (int i = 0; i + 1 < lines.Length; i += 2)
            {
                if (lines[i].Length > 0 && int.TryParse(lines[i + 1], out int score))
                {
                    Add(lines[i], score);
                }
            }
        }

        public bool Qualifies(int score)
        {
            if (score <= 0)
            {
                return false;
            }
            return Entries.Count < MaxEntries || score > Entries[Entries.Count - 1].Score;
        }

        public void Submit(string name, int score)
        {
            Add(name.Length > 0 ? name : "PILOT", score);
            Save();
        }

        private void Add(string name, int score)
        {
            int index = 0;
            while (index < Entries.Count && Entries[index].Score >= score)
            {
                index++;
            }

            HiScoreEntry entry = new HiScoreEntry();
            entry.Name = name;
            entry.Score = score;
            Entries.Insert(index, entry);

            if (Entries.Count > MaxEntries)
            {
                Entries.RemoveAt(Entries.Count - 1);
            }
        }

        private void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
            List<string> lines = new List<string>();
            foreach (HiScoreEntry entry in Entries)
            {
                lines.Add(entry.Name);
                lines.Add(entry.Score.ToString());
            }
            File.WriteAllLines(_filePath, lines);
        }
    }
}
