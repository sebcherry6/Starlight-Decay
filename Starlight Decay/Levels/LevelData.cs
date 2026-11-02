namespace Starlight_Decay
{
    /// <summary>One group of enemies and rocks. The next wave starts once this one is cleared.</summary>
    public class Wave
    {
        public int Rocks;
        public int RockSize = 3;
        public readonly List<EnemyKind> Enemies = new List<EnemyKind>();

        public Wave(int rocks, int rockSize = 3)
        {
            Rocks = rocks;
            RockSize = rockSize;
        }

        public Wave Add(EnemyKind kind, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Enemies.Add(kind);
            }
            return this;
        }
    }

    /// <summary>One row of the sector table. Change these numbers to rebalance the game.</summary>
    public class LevelData
    {
        public string Name;
        public string Briefing;
        public string Nebula = "nebula1";
        public Color NebulaTint = Color.White;
        public Color StarTint = Color.White;
        public RockKind RockKind = RockKind.Stone;
        public float Drag = 0.62f;           // Share of speed kept each second. Higher is slippier.
        public int GravityWells;
        public float IonInterval;            // Seconds between lightning strikes. 0 means none.
        public float BeamInterval;           // Seconds between laser walls. 0 means none.
        public bool Dark;                    // Limits how far the player can see.
        public Wave[] Waves;
    }

    public static class Levels
    {
        public static readonly LevelData[] All =
        {
            new LevelData
            {
                Name = "THE BELT",
                Briefing = "Clear the rocks and the scout drones.",
                Nebula = "nebula1", NebulaTint = new Color(30, 70, 60), StarTint = Color.White,
                Waves = new[]
                {
                    new Wave(3).Add(EnemyKind.Drone, 2),
                    new Wave(4).Add(EnemyKind.Drone, 4),
                    new Wave(3).Add(EnemyKind.Drone, 3).Add(EnemyKind.Gunner, 2)
                }
            },
            new LevelData
            {
                Name = "THE SCRAPYARD",
                Briefing = "Swarmers nest in the wreckage. They come in packs.",
                Nebula = "nebula2", NebulaTint = new Color(80, 60, 25), StarTint = Color.Wheat,
                RockKind = RockKind.Crystal,
                Waves = new[]
                {
                    new Wave(3).Add(EnemyKind.Swarmer, 5).Add(EnemyKind.Drone, 2),
                    new Wave(3).Add(EnemyKind.Gunner, 3).Add(EnemyKind.Drone, 3),
                    new Wave(4).Add(EnemyKind.Swarmer, 8).Add(EnemyKind.Gunner, 2)
                }
            },
            new LevelData
            {
                Name = "FROZEN REACH",
                Briefing = "Ice rocks are tough, mines are live, and your ship will slide.",
                Nebula = "nebula3", NebulaTint = new Color(30, 70, 110), StarTint = Color.LightCyan,
                RockKind = RockKind.Ice, Drag = 0.9f,
                Waves = new[]
                {
                    new Wave(3).Add(EnemyKind.Mine, 4).Add(EnemyKind.Drone, 3),
                    new Wave(3).Add(EnemyKind.Swarmer, 6).Add(EnemyKind.Mine, 3),
                    new Wave(3).Add(EnemyKind.Gunner, 4).Add(EnemyKind.Mine, 2),
                    new Wave(4).Add(EnemyKind.Gunner, 3).Add(EnemyKind.Swarmer, 6).Add(EnemyKind.Mine, 4)
                }
            },
            new LevelData
            {
                Name = "GRAVITY RIFT",
                Briefing = "A gravity well bends everything. Lancers telegraph, then charge.",
                Nebula = "nebula1", NebulaTint = new Color(70, 30, 100), StarTint = Color.Plum,
                GravityWells = 1,
                Waves = new[]
                {
                    new Wave(3).Add(EnemyKind.Lancer, 2).Add(EnemyKind.Drone, 4),
                    new Wave(3).Add(EnemyKind.Lancer, 2).Add(EnemyKind.Gunner, 3),
                    new Wave(3).Add(EnemyKind.Swarmer, 8).Add(EnemyKind.Lancer, 2),
                    new Wave(4).Add(EnemyKind.Lancer, 4).Add(EnemyKind.Gunner, 3).Add(EnemyKind.Mine, 3)
                }
            },
            new LevelData
            {
                Name = "THE MINEFIELD",
                Briefing = "Shoot mines from range. Bulwarks must be flanked.",
                Nebula = "nebula2", NebulaTint = new Color(100, 40, 20), StarTint = Color.PeachPuff,
                Waves = new[]
                {
                    new Wave(2).Add(EnemyKind.Mine, 8).Add(EnemyKind.Bulwark, 1),
                    new Wave(3).Add(EnemyKind.Mine, 6).Add(EnemyKind.Bulwark, 2).Add(EnemyKind.Drone, 3),
                    new Wave(3).Add(EnemyKind.Mine, 7).Add(EnemyKind.Lancer, 3).Add(EnemyKind.Gunner, 2),
                    new Wave(3).Add(EnemyKind.Mine, 8).Add(EnemyKind.Bulwark, 2).Add(EnemyKind.Swarmer, 6)
                }
            },
            new LevelData
            {
                Name = "ION STORM",
                Briefing = "Lightning follows you. Snipers lock on, so move when the line turns white.",
                Nebula = "nebula3", NebulaTint = new Color(20, 80, 100), StarTint = Color.PaleTurquoise,
                IonInterval = 3.2f,
                Waves = new[]
                {
                    new Wave(3).Add(EnemyKind.Sniper, 2).Add(EnemyKind.Drone, 5),
                    new Wave(3).Add(EnemyKind.Sniper, 2).Add(EnemyKind.Swarmer, 8),
                    new Wave(3).Add(EnemyKind.Sniper, 3).Add(EnemyKind.Gunner, 3).Add(EnemyKind.Mine, 3),
                    new Wave(3).Add(EnemyKind.Sniper, 2).Add(EnemyKind.Lancer, 3).Add(EnemyKind.Bulwark, 2),
                    new Wave(4).Add(EnemyKind.Sniper, 4).Add(EnemyKind.Lancer, 3).Add(EnemyKind.Swarmer, 8)
                }
            },
            new LevelData
            {
                Name = "THE LASER GRID",
                Briefing = "Walls of light sweep the sector. Find the gap.",
                Nebula = "nebula2", NebulaTint = new Color(100, 20, 35), StarTint = Color.LightPink,
                BeamInterval = 6.5f,
                Waves = new[]
                {
                    new Wave(2).Add(EnemyKind.Lancer, 3).Add(EnemyKind.Gunner, 3),
                    new Wave(3).Add(EnemyKind.Bulwark, 3).Add(EnemyKind.Swarmer, 8),
                    new Wave(2).Add(EnemyKind.Sniper, 3).Add(EnemyKind.Lancer, 3).Add(EnemyKind.Drone, 4),
                    new Wave(3).Add(EnemyKind.Gunner, 5).Add(EnemyKind.Bulwark, 2).Add(EnemyKind.Mine, 4),
                    new Wave(3).Add(EnemyKind.Lancer, 5).Add(EnemyKind.Sniper, 3).Add(EnemyKind.Bulwark, 2)
                }
            },
            new LevelData
            {
                Name = "THE FORGE",
                Briefing = "Magma rocks burst into fire. Carriers launch swarmers until destroyed.",
                Nebula = "nebula1", NebulaTint = new Color(120, 45, 10), StarTint = Color.Orange,
                RockKind = RockKind.Magma, IonInterval = 5.5f,
                Waves = new[]
                {
                    new Wave(4).Add(EnemyKind.Carrier, 1).Add(EnemyKind.Drone, 4),
                    new Wave(3).Add(EnemyKind.Carrier, 1).Add(EnemyKind.Gunner, 4).Add(EnemyKind.Mine, 4),
                    new Wave(4).Add(EnemyKind.Lancer, 4).Add(EnemyKind.Sniper, 3).Add(EnemyKind.Swarmer, 8),
                    new Wave(3).Add(EnemyKind.Carrier, 2).Add(EnemyKind.Bulwark, 2),
                    new Wave(4).Add(EnemyKind.Carrier, 2).Add(EnemyKind.Lancer, 4).Add(EnemyKind.Sniper, 3)
                }
            },
            new LevelData
            {
                Name = "THE VOID",
                Briefing = "You can only see what is close. Some enemies can't be seen at all.",
                Nebula = "nebula3", NebulaTint = new Color(30, 25, 70), StarTint = Color.SlateGray,
                GravityWells = 2, Dark = true,
                Waves = new[]
                {
                    new Wave(3).Add(EnemyKind.Phantom, 3).Add(EnemyKind.Drone, 4),
                    new Wave(3).Add(EnemyKind.Phantom, 3).Add(EnemyKind.Swarmer, 8),
                    new Wave(2).Add(EnemyKind.Phantom, 4).Add(EnemyKind.Sniper, 3).Add(EnemyKind.Mine, 4),
                    new Wave(3).Add(EnemyKind.Phantom, 3).Add(EnemyKind.Lancer, 4).Add(EnemyKind.Bulwark, 2),
                    new Wave(3).Add(EnemyKind.Phantom, 5).Add(EnemyKind.Carrier, 1).Add(EnemyKind.Gunner, 4)
                }
            },
            new LevelData
            {
                Name = "THE SINGULARITY",
                Briefing = "Everything at once. End this.",
                Nebula = "nebula1", NebulaTint = new Color(90, 90, 110), StarTint = Color.White,
                RockKind = RockKind.Magma, GravityWells = 1, IonInterval = 5f, BeamInterval = 9f,
                Waves = new[]
                {
                    new Wave(4).Add(EnemyKind.Gunner, 5).Add(EnemyKind.Swarmer, 8),
                    new Wave(3).Add(EnemyKind.Lancer, 5).Add(EnemyKind.Mine, 6),
                    new Wave(3).Add(EnemyKind.Sniper, 4).Add(EnemyKind.Bulwark, 3),
                    new Wave(3).Add(EnemyKind.Phantom, 5).Add(EnemyKind.Swarmer, 10),
                    new Wave(3).Add(EnemyKind.Carrier, 2).Add(EnemyKind.Lancer, 4),
                    new Wave(4).Add(EnemyKind.Carrier, 2).Add(EnemyKind.Phantom, 4).Add(EnemyKind.Sniper, 4).Add(EnemyKind.Bulwark, 3)
                }
            }
        };
    }
}
