namespace Starlight_Decay
{
    public enum BossMove
    {
        Drift,    // Bounces around the arena.
        Hover,    // Strafes along the top.
        Orbit,    // Circles the centre.
        Blink,    // Teleports between attacks.
        Chase,    // Slowly follows the player.
        Anchor    // Sits in the centre.
    }

    public enum BossAttack
    {
        Ring, Fan, Spiral, RockSpit, Summon, Mines, Charge, Lightning, Beam, Gravity, Cloak, Nova
    }

    /// <summary>
    /// One row of the boss table. Phases holds the attack rotation used in each third of the boss's
    /// health, so new patterns appear as it gets weaker.
    /// </summary>
    public class BossData
    {
        public string Name;
        public string Texture;
        public Color Tint;
        public float Health;
        public float Scale = 0.8f;
        public float Radius = 110f;
        public float Speed = 90f;
        public float AttackDelay = 2f;
        public BossMove Move;
        public EnemyKind Summons = EnemyKind.Drone;
        public bool NodeShield;
        public BossAttack[][] Phases;
    }

    public static class Bosses
    {
        public static readonly BossData[] All =
        {
            new BossData
            {
                Name = "THE BOULDER", Texture = "boss1", Tint = Color.YellowGreen,
                Health = 45f, Move = BossMove.Drift, Speed = 75f, AttackDelay = 2.4f,
                Phases = new[]
                {
                    new[] { BossAttack.Ring, BossAttack.RockSpit },
                    new[] { BossAttack.Ring, BossAttack.RockSpit, BossAttack.Fan },
                    new[] { BossAttack.Nova, BossAttack.RockSpit, BossAttack.Fan }
                }
            },
            new BossData
            {
                Name = "DRONE MOTHER", Texture = "boss2", Tint = Color.Gold, Summons = EnemyKind.Drone,
                Health = 85f, Move = BossMove.Hover, Speed = 130f, AttackDelay = 2.2f,
                Phases = new[]
                {
                    new[] { BossAttack.Summon, BossAttack.Fan },
                    new[] { BossAttack.Summon, BossAttack.Fan, BossAttack.Ring },
                    new[] { BossAttack.Summon, BossAttack.Spiral, BossAttack.Fan }
                }
            },
            new BossData
            {
                Name = "CRYO SENTINEL", Texture = "boss3", Tint = Color.LightSkyBlue,
                Health = 135f, Move = BossMove.Orbit, Speed = 0.42f, AttackDelay = 2f,
                Phases = new[]
                {
                    new[] { BossAttack.Spiral, BossAttack.Ring },
                    new[] { BossAttack.Spiral, BossAttack.RockSpit, BossAttack.Fan },
                    new[] { BossAttack.Spiral, BossAttack.Nova, BossAttack.Mines }
                }
            },
            new BossData
            {
                Name = "WARP STRIDER", Texture = "boss4", Tint = Color.MediumPurple,
                Health = 190f, Move = BossMove.Blink, AttackDelay = 1.9f,
                Phases = new[]
                {
                    new[] { BossAttack.Fan, BossAttack.Ring },
                    new[] { BossAttack.Fan, BossAttack.Gravity, BossAttack.Charge },
                    new[] { BossAttack.Fan, BossAttack.Gravity, BossAttack.Nova, BossAttack.Charge }
                }
            },
            new BossData
            {
                Name = "MINE LAYER", Texture = "boss5", Tint = Color.OrangeRed, Summons = EnemyKind.Bulwark,
                Health = 260f, Move = BossMove.Drift, Speed = 125f, AttackDelay = 1.9f,
                Phases = new[]
                {
                    new[] { BossAttack.Mines, BossAttack.Fan },
                    new[] { BossAttack.Mines, BossAttack.Ring, BossAttack.Summon },
                    new[] { BossAttack.Mines, BossAttack.Spiral, BossAttack.Summon, BossAttack.Charge }
                }
            },
            new BossData
            {
                Name = "STORM CORE", Texture = "boss6", Tint = Color.Cyan, Summons = EnemyKind.Sniper,
                Health = 480f, Move = BossMove.Orbit, Speed = -0.5f, AttackDelay = 1.7f,
                Phases = new[]
                {
                    new[] { BossAttack.Lightning, BossAttack.Ring },
                    new[] { BossAttack.Lightning, BossAttack.Spiral, BossAttack.Summon },
                    new[] { BossAttack.Lightning, BossAttack.Nova, BossAttack.Spiral, BossAttack.Gravity }
                }
            },
            new BossData
            {
                Name = "LANCE PRIME", Texture = "boss7", Tint = Color.Crimson, Summons = EnemyKind.Lancer,
                Health = 660f, Move = BossMove.Chase, Speed = 85f, AttackDelay = 1.6f,
                Phases = new[]
                {
                    new[] { BossAttack.Charge, BossAttack.Beam, BossAttack.Fan },
                    new[] { BossAttack.Charge, BossAttack.Beam, BossAttack.Summon, BossAttack.Spiral },
                    new[] { BossAttack.Charge, BossAttack.Beam, BossAttack.Nova, BossAttack.Charge, BossAttack.Summon }
                }
            },
            new BossData
            {
                Name = "MAGMA COLOSSUS", Texture = "boss8", Tint = Color.OrangeRed, Summons = EnemyKind.Swarmer,
                Health = 840f, Move = BossMove.Chase, Speed = 62f, AttackDelay = 1.5f, Scale = 0.95f, Radius = 130f,
                Phases = new[]
                {
                    new[] { BossAttack.RockSpit, BossAttack.Nova, BossAttack.Fan },
                    new[] { BossAttack.RockSpit, BossAttack.Spiral, BossAttack.Summon, BossAttack.Lightning },
                    new[] { BossAttack.Nova, BossAttack.RockSpit, BossAttack.Spiral, BossAttack.Lightning, BossAttack.Beam }
                }
            },
            new BossData
            {
                Name = "THE PHANTOM", Texture = "boss9", Tint = Color.MediumSlateBlue, Summons = EnemyKind.Phantom,
                Health = 960f, Move = BossMove.Blink, AttackDelay = 1.4f,
                Phases = new[]
                {
                    new[] { BossAttack.Cloak, BossAttack.Fan, BossAttack.Ring },
                    new[] { BossAttack.Cloak, BossAttack.Spiral, BossAttack.Summon, BossAttack.Fan },
                    new[] { BossAttack.Cloak, BossAttack.Nova, BossAttack.Gravity, BossAttack.Spiral, BossAttack.Summon }
                }
            },
            new BossData
            {
                Name = "THE SINGULARITY", Texture = "boss10", Tint = Color.White, Summons = EnemyKind.Lancer,
                Health = 1100f, Move = BossMove.Anchor, AttackDelay = 1.3f, Scale = 1.05f, Radius = 140f, NodeShield = true,
                Phases = new[]
                {
                    new[] { BossAttack.Spiral, BossAttack.Fan, BossAttack.Lightning, BossAttack.Ring },
                    new[] { BossAttack.Gravity, BossAttack.Beam, BossAttack.Nova, BossAttack.Summon, BossAttack.Spiral },
                    new[] { BossAttack.Nova, BossAttack.Gravity, BossAttack.Beam, BossAttack.Lightning, BossAttack.Spiral, BossAttack.Mines }
                }
            }
        };
    }
}
