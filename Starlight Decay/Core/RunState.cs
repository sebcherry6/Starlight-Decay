namespace Starlight_Decay
{
    public enum UpgradeKind
    {
        Hull, Shield, Thrusters, Cannon, FireRate, Multishot, Magnet, Wingman, Dash, BombBay,
        Repair, ExtraShip
    }

    public class UpgradeInfo
    {
        public UpgradeKind Kind;
        public string Name;
        public string Description;
        public string Icon;
        public int MaxLevel;      // 0 means it can be bought again and again.
        public int BaseCost;
        public int CostStep;

        public UpgradeInfo(UpgradeKind kind, string name, string description, string icon, int maxLevel, int baseCost, int costStep)
        {
            Kind = kind;
            Name = name;
            Description = description;
            Icon = icon;
            MaxLevel = maxLevel;
            BaseCost = baseCost;
            CostStep = costStep;
        }
    }

    public static class Upgrades
    {
        public static readonly UpgradeInfo[] All =
        {
            new UpgradeInfo(UpgradeKind.Hull, "HULL PLATING", "+1 maximum hull, and repairs 1 hull.", "pick_repair", 4, 90, 70),
            new UpgradeInfo(UpgradeKind.Shield, "SHIELD CELL", "+1 shield charge. Shields recharge when you avoid damage.", "pick_shield", 3, 110, 90),
            new UpgradeInfo(UpgradeKind.Thrusters, "THRUSTERS", "Faster acceleration and top speed.", "up_thrust", 4, 60, 50),
            new UpgradeInfo(UpgradeKind.Cannon, "CANNON POWER", "Every shot deals 50% more base damage.", "up_cannon", 6, 80, 60),
            new UpgradeInfo(UpgradeKind.FireRate, "AUTO-LOADER", "Fire more shots per second.", "up_firerate", 5, 70, 60),
            new UpgradeInfo(UpgradeKind.Multishot, "TWIN LINK", "+1 parallel shot.", "up_multi", 3, 200, 180),
            new UpgradeInfo(UpgradeKind.Magnet, "SCRAP MAGNET", "Pull pickups in from further away.", "up_magnet", 3, 50, 50),
            new UpgradeInfo(UpgradeKind.Wingman, "WINGMAN DRONE", "An escort that orbits you and fires at the nearest target.", "wingman", 2, 240, 200),
            new UpgradeInfo(UpgradeKind.Dash, "PHASE DASH", "Shorter dash cooldown.", "up_dash", 3, 80, 70),
            new UpgradeInfo(UpgradeKind.BombBay, "BOMB BAY", "+1 bomb capacity. Bay refills each sector.", "pick_bomb", 3, 100, 80),
            new UpgradeInfo(UpgradeKind.Repair, "FIELD REPAIR", "Restore all hull.", "pick_repair", 0, 60, 0),
            new UpgradeInfo(UpgradeKind.ExtraShip, "RESERVE SHIP", "+1 life.", "up_ship", 0, 350, 150)
        };
    }

    /// <summary>Everything that carries over between sectors, and the ship stats derived from it.</summary>
    public class RunState
    {
        public int Sector = 1;
        public int Score;
        public int Scrap;
        public int Lives = 2;
        public int Hull = 4;
        public int ShieldCharge;
        public int Bombs = 2;
        public int Shards;
        public int Kills;
        public int ExtraShipsBought;
        public RunState Checkpoint;

        private int[] _levels = new int[Upgrades.All.Length];

        public int Level(UpgradeKind kind)
        {
            return _levels[(int)kind];
        }

        public int MaxHull => 4 + Level(UpgradeKind.Hull);
        public int MaxShield => Level(UpgradeKind.Shield);
        public int MaxBombs => 2 + Level(UpgradeKind.BombBay);
        public float Damage => 1f + 0.5f * Level(UpgradeKind.Cannon);
        public float FireInterval => 0.30f - 0.03f * Level(UpgradeKind.FireRate);
        public int ShotCount => 1 + Level(UpgradeKind.Multishot);
        public float Acceleration => 620f + 110f * Level(UpgradeKind.Thrusters);
        public float MaxSpeed => 480f + 50f * Level(UpgradeKind.Thrusters);
        public float MagnetRange => 120f + 80f * Level(UpgradeKind.Magnet);
        public float DashCooldown => 3.6f - 0.7f * Level(UpgradeKind.Dash);
        public int WingmanCount => Level(UpgradeKind.Wingman);

        public int Cost(UpgradeInfo upgrade)
        {
            if (upgrade.Kind == UpgradeKind.ExtraShip)
            {
                return upgrade.BaseCost + upgrade.CostStep * ExtraShipsBought;
            }
            return upgrade.BaseCost + upgrade.CostStep * Level(upgrade.Kind);
        }

        public bool IsMaxed(UpgradeInfo upgrade)
        {
            if (upgrade.Kind == UpgradeKind.Repair)
            {
                return Hull >= MaxHull;
            }
            return upgrade.MaxLevel > 0 && Level(upgrade.Kind) >= upgrade.MaxLevel;
        }

        public bool TryBuy(UpgradeInfo upgrade)
        {
            int cost = Cost(upgrade);
            if (IsMaxed(upgrade) || Scrap < cost)
            {
                return false;
            }

            Scrap -= cost;
            switch (upgrade.Kind)
            {
                case UpgradeKind.Repair:
                    Hull = MaxHull;
                    break;
                case UpgradeKind.ExtraShip:
                    Lives++;
                    ExtraShipsBought++;
                    break;
                default:
                    _levels[(int)upgrade.Kind]++;
                    break;
            }

            if (upgrade.Kind == UpgradeKind.Hull)
            {
                Hull = Math.Min(Hull + 1, MaxHull);
            }
            if (upgrade.Kind == UpgradeKind.Shield)
            {
                ShieldCharge = MaxShield;
            }
            return true;
        }

        public void AdvanceSector()
        {
            Sector++;
            RefreshForSector();
        }

        public void RefreshForSector()
        {
            Bombs = MaxBombs;
            ShieldCharge = MaxShield;
        }

        /// <summary>
        /// Rewinds to the start of the sector after a death. Half the scrap earned on the failed
        /// attempt is kept, so each retry can buy something and nobody gets stuck.
        /// </summary>
        public RunState RetryFromCheckpoint()
        {
            RunState retry = Checkpoint.Clone();
            retry.Scrap += Math.Max(0, Scrap - retry.Scrap) / 2;
            retry.Score = Score / 2;
            retry.Hull = retry.MaxHull;
            retry.Lives = Math.Max(retry.Lives, 2);
            retry.RefreshForSector();
            return retry;
        }

        public RunState Clone()
        {
            RunState copy = (RunState)MemberwiseClone();
            copy._levels = (int[])_levels.Clone();
            copy.Checkpoint = null;
            return copy;
        }
    }
}
