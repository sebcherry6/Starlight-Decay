namespace Starlight_Decay
{
    public enum PickupKind
    {
        Scrap, Repair, ShieldCell, Bomb, Spread, Rapid, Pierce, Homing, DoubleScore, Shard
    }

    public class Pickup : Entity
    {
        private const float PowerUpDuration = 15f;

        private readonly PickupKind _kind;
        private float _life;

        public Pickup(World world, PickupKind kind, Vector2 position) : base(world)
        {
            _kind = kind;
            SetTexture(TextureFor(kind));
            Tint = ColorFor(kind);
            Position = position;
            Radius = kind == PickupKind.Scrap ? 12f : 22f;
            Scale = kind == PickupKind.Scrap ? 0.4f : 0.75f;
            Wraps = true;

            if (kind == PickupKind.Shard)
            {
                // Shards cross the arena once, so you have to go and get them.
                _life = 9f;
                Velocity = Util.FromAngle(Rng.Angle()) * 170f;
                RotationSpeed = 2.5f;
            }
            else
            {
                _life = kind == PickupKind.Scrap ? 12f : 14f;
                Velocity = Util.FromAngle(Rng.Angle()) * Rng.Range(20f, 130f);
            }
        }

        private static string TextureFor(PickupKind kind)
        {
            switch (kind)
            {
                case PickupKind.Repair: return "pick_repair";
                case PickupKind.ShieldCell: return "pick_shield";
                case PickupKind.Bomb: return "pick_bomb";
                case PickupKind.Spread: return "pick_spread";
                case PickupKind.Rapid: return "pick_rapid";
                case PickupKind.Pierce: return "pick_pierce";
                case PickupKind.Homing: return "pick_homing";
                case PickupKind.DoubleScore: return "pick_double";
                case PickupKind.Shard: return "pick_shard";
                default: return "pick_scrap";
            }
        }

        private static Color ColorFor(PickupKind kind)
        {
            switch (kind)
            {
                case PickupKind.Scrap: return Color.Gold;
                case PickupKind.Repair: return Color.LightGreen;
                case PickupKind.ShieldCell: return Color.DeepSkyBlue;
                case PickupKind.Bomb: return Color.Orange;
                case PickupKind.Spread: return Color.Violet;
                case PickupKind.Rapid: return Color.Yellow;
                case PickupKind.Pierce: return Color.Cyan;
                case PickupKind.Homing: return Color.HotPink;
                case PickupKind.DoubleScore: return Color.Gold;
                default: return Color.White;
            }
        }

        public override void Update(float dt)
        {
            _life -= dt;
            if (_life <= 0f)
            {
                Dead = true;
                return;
            }

            if (_kind != PickupKind.Shard)
            {
                Velocity *= MathF.Pow(0.3f, dt);
            }

            Player player = World.Player;
            float range = World.CollectAll ? 3000f : World.Run.MagnetRange;
            if (!player.Dead && Vector2.Distance(Position, player.Position) < range)
            {
                Velocity = Util.Direction(Position, player.Position) * 620f;
            }

            base.Update(dt);
        }

        public override void Draw(SpriteBatch batch)
        {
            // Blink for the last three seconds before vanishing.
            bool blinkedOff = _life < 3f && (int)(_life * 8f) % 2 == 0;
            if (!blinkedOff)
            {
                base.Draw(batch);
            }
        }

        public void Collect()
        {
            Dead = true;
            RunState run = World.Run;
            Player player = World.Player;

            switch (_kind)
            {
                case PickupKind.Scrap:
                    run.Scrap += 3;
                    Sfx.Play("pickup", 0.35f, Rng.Range(0f, 0.3f));
                    return;
                case PickupKind.Repair:
                    run.Hull = Math.Min(run.Hull + 1, run.MaxHull);
                    Announce("HULL +1", Color.LightGreen);
                    break;
                case PickupKind.ShieldCell:
                    run.ShieldCharge = Math.Min(run.ShieldCharge + 1, run.MaxShield + 1);
                    Announce("SHIELD +1", Color.DeepSkyBlue);
                    break;
                case PickupKind.Bomb:
                    run.Bombs = Math.Min(run.Bombs + 1, run.MaxBombs);
                    Announce("BOMB +1", Color.Orange);
                    break;
                case PickupKind.Spread:
                    player.SetWeapon(WeaponMode.Spread, PowerUpDuration);
                    Announce("SPREAD SHOT", Color.Violet);
                    break;
                case PickupKind.Rapid:
                    player.SetWeapon(WeaponMode.Rapid, PowerUpDuration);
                    Announce("RAPID FIRE", Color.Yellow);
                    break;
                case PickupKind.Pierce:
                    player.SetWeapon(WeaponMode.Pierce, PowerUpDuration);
                    Announce("PIERCING ROUNDS", Color.Cyan);
                    break;
                case PickupKind.Homing:
                    player.SetWeapon(WeaponMode.Homing, PowerUpDuration);
                    Announce("HOMING ROUNDS", Color.HotPink);
                    break;
                case PickupKind.DoubleScore:
                    player.DoubleScoreTimer = PowerUpDuration;
                    Announce("SCORE x2", Color.Gold);
                    break;
                case PickupKind.Shard:
                    run.Shards++;
                    run.Scrap += 25;
                    World.AddScore(1000, Position);
                    Announce("STAR SHARD " + run.Shards + "/" + World.TotalShards, Color.White);
                    break;
            }
            Sfx.Play("powerup", 0.7f);
        }

        private void Announce(string text, Color color)
        {
            World.Float(text, Position, color);
        }
    }
}
