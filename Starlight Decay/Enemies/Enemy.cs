namespace Starlight_Decay
{
    public enum EnemyKind
    {
        Drone, Gunner, Swarmer, Mine, Lancer, Bulwark, Sniper, Carrier, Phantom
    }

    /// <summary>
    /// Shared enemy behaviour: warp-in, health, hit flash, death and rewards.
    /// Each subclass only has to say how it thinks, in Think().
    /// </summary>
    public abstract class Enemy : Entity
    {
        private const float WarpInTime = 0.7f;

        private float _warpIn = WarpInTime;
        private float _flash;

        protected float Health;
        protected int ScoreValue;
        protected int ScrapValue;

        public bool IsWarpingIn => _warpIn > 0f;

        protected Enemy(World world, string texture, Vector2 position, Color tint, float health, int score, int scrap, float scale, float radius) : base(world)
        {
            SetTexture(texture);
            Position = position;
            Tint = tint;
            Health = health * World.EnemyHealthScale;
            ScoreValue = score;
            ScrapValue = scrap;
            Scale = scale;
            Radius = radius;
        }

        public static Enemy Create(World world, EnemyKind kind, Vector2 position)
        {
            switch (kind)
            {
                case EnemyKind.Gunner: return new Gunner(world, position);
                case EnemyKind.Swarmer: return new Swarmer(world, position);
                case EnemyKind.Mine: return new Mine(world, position);
                case EnemyKind.Lancer: return new Lancer(world, position);
                case EnemyKind.Bulwark: return new Bulwark(world, position);
                case EnemyKind.Sniper: return new Sniper(world, position);
                case EnemyKind.Carrier: return new Carrier(world, position);
                case EnemyKind.Phantom: return new Phantom(world, position);
                default: return new Drone(world, position);
            }
        }

        protected Vector2 ToPlayer => Util.Direction(Position, World.Player.Position);

        protected float DistanceToPlayer => Vector2.Distance(Position, World.Player.Position);

        protected void SkipWarpIn()
        {
            _warpIn = 0f;
        }

        protected abstract void Think(float dt);

        public override void Update(float dt)
        {
            if (_warpIn > 0f)
            {
                _warpIn -= dt;
                return;
            }
            _flash -= dt;
            Think(dt);
            base.Update(dt);
            KeepInArena();
        }

        protected virtual void KeepInArena()
        {
            Position = Util.ClampToArena(Position, Radius);
        }

        /// <summary>Applies a hit from the given direction.</summary>
        public virtual void Hit(float damage, Vector2 from)
        {
            Health -= damage;
            _flash = 0.08f;
            if (Health <= 0f)
            {
                Die();
            }
            else
            {
                Sfx.Play("hit", 0.25f, Rng.Range(-0.1f, 0.3f));
            }
        }

        public virtual void Die()
        {
            if (Dead)
            {
                return;
            }
            Dead = true;
            Sfx.Play("explosion", 0.5f, Rng.Range(-0.1f, 0.2f));
            World.Burst(Position, Tint, 16, 300f);
            World.Shake(3f);
            World.Run.Kills++;
            World.AddKillScore(ScoreValue, Position);
            World.DropLoot(Position, ScrapValue, 0.12f);
        }

        public override void Draw(SpriteBatch batch)
        {
            if (_warpIn > 0f)
            {
                float progress = 1f - _warpIn / WarpInTime;
                Ui.Ring(batch, Position, Radius * 2.6f * (1.2f - progress), Tint * progress);
                return;
            }
            Color color = _flash > 0f ? Color.White : Tint;
            batch.Draw(Texture, Position, Source, color, Rotation, Origin, Scale, SpriteEffects.None, 0f);
        }

        protected void FireAtPlayer(float speed, float spread = 0f)
        {
            float angle = Util.AngleOf(ToPlayer) + Rng.Range(-spread, spread);
            FireInDirection(angle, speed);
        }

        protected void FireInDirection(float angle, float speed)
        {
            World.EnemyShots.Add(new Bullet(World, Position, Util.FromAngle(angle) * speed * World.ShotSpeedScale, Tint, 1f, 4f));
            Sfx.Play("enemy_shot", 0.22f, Rng.Range(-0.1f, 0.1f));
        }
    }

    /// <summary>Slow seeker. The first thing you meet.</summary>
    public class Drone : Enemy
    {
        public Drone(World world, Vector2 position) : base(world, "drone", position, Color.LimeGreen, 1.5f, 100, 2, 0.5f, 22f)
        {
        }

        protected override void Think(float dt)
        {
            float speed = 110f + 9f * World.Run.Sector;
            Velocity += ToPlayer * 260f * dt;
            if (Velocity.Length() > speed)
            {
                Velocity = Vector2.Normalize(Velocity) * speed;
            }
            Rotation = Util.AngleOf(Velocity);
        }
    }

    /// <summary>Circles at range and fires aimed shots.</summary>
    public class Gunner : Enemy
    {
        private readonly float _orbitDirection = Rng.Chance(0.5f) ? 1f : -1f;
        private float _cooldown = Rng.Range(0.8f, 2.2f);

        public Gunner(World world, Vector2 position) : base(world, "gunner", position, Color.Gold, 3f, 200, 3, 0.55f, 28f)
        {
        }

        protected override void Think(float dt)
        {
            Vector2 toPlayer = ToPlayer;
            Vector2 sideways = new Vector2(-toPlayer.Y, toPlayer.X) * _orbitDirection;
            float approach = DistanceToPlayer > 430f ? 1f : -1f;
            Vector2 desired = (sideways + toPlayer * approach * 0.7f) * 165f;
            Velocity = Vector2.Lerp(Velocity, desired, Math.Min(1f, 3f * dt));
            RotationSpeed = 1.5f;

            _cooldown -= dt;
            if (_cooldown <= 0f)
            {
                _cooldown = Math.Max(1.1f, 2.5f - 0.13f * World.Run.Sector);
                FireAtPlayer(300f, 0.06f);
            }
        }
    }

    /// <summary>Fast, fragile and arrives in packs, weaving as it comes.</summary>
    public class Swarmer : Enemy
    {
        private readonly float _phase = Rng.Angle();
        private float _time;

        public Swarmer(World world, Vector2 position) : base(world, "swarmer", position, Color.DeepPink, 1f, 60, 1, 0.42f, 16f)
        {
        }

        protected override void Think(float dt)
        {
            _time += dt;
            float weave = MathF.Sin(_time * 5f + _phase) * 1.1f;
            float heading = Util.AngleOf(ToPlayer) + weave;
            Velocity = Util.FromAngle(heading) * (235f + 6f * World.Run.Sector);
            Rotation = Util.AngleOf(Velocity);
        }
    }

    /// <summary>Drifts until you get close, then arms and bursts. Shoot it first and it dies harmlessly.</summary>
    public class Mine : Enemy
    {
        private const float TriggerRange = 175f;
        private const float FuseTime = 0.9f;
        private const int Shards = 10;

        private float _fuse;

        public Mine(World world, Vector2 position) : base(world, "mine", position, Color.OrangeRed, 2f, 120, 2, 0.5f, 22f)
        {
            Velocity = Util.FromAngle(Rng.Angle()) * Rng.Range(12f, 40f);
            RotationSpeed = 0.7f;
        }

        protected override void Think(float dt)
        {
            if (_fuse > 0f)
            {
                _fuse -= dt;
                Tint = (int)(_fuse * 12f) % 2 == 0 ? Color.White : Color.Red;
                if (_fuse <= 0f)
                {
                    Detonate();
                }
            }
            else if (DistanceToPlayer < TriggerRange)
            {
                _fuse = FuseTime;
            }
        }

        private void Detonate()
        {
            Dead = true;
            Sfx.Play("explosion", 0.5f, 0.3f);
            World.Burst(Position, Color.OrangeRed, 20, 320f);
            float offset = Rng.Angle();
            for (int i = 0; i < Shards; i++)
            {
                FireInDirection(offset + i * MathHelper.TwoPi / Shards, 270f);
            }
        }
    }

    /// <summary>Stops, lines you up, then charges straight through. Sidestep it.</summary>
    public class Lancer : Enemy
    {
        private enum Mode { Stalk, Aim, Charge }

        private const float AimTime = 0.75f;
        private const float ChargeTime = 0.7f;

        private Mode _mode = Mode.Stalk;
        private float _timer = Rng.Range(1f, 2f);

        public Lancer(World world, Vector2 position) : base(world, "lancer", position, Color.Cyan, 4f, 250, 4, 0.55f, 24f)
        {
        }

        protected override void Think(float dt)
        {
            _timer -= dt;
            switch (_mode)
            {
                case Mode.Stalk:
                    Rotation = Util.TurnToward(Rotation, Util.AngleOf(ToPlayer), 3.5f * dt);
                    Velocity = Vector2.Lerp(Velocity, ToPlayer * 90f, Math.Min(1f, 3f * dt));
                    if (_timer <= 0f)
                    {
                        _mode = Mode.Aim;
                        _timer = AimTime;
                        Velocity = Vector2.Zero;
                    }
                    break;
                case Mode.Aim:
                    Rotation = Util.TurnToward(Rotation, Util.AngleOf(ToPlayer), 2f * dt);
                    if (_timer <= 0f)
                    {
                        _mode = Mode.Charge;
                        _timer = ChargeTime;
                        Velocity = Util.FromAngle(Rotation) * 780f;
                        Sfx.Play("dash", 0.4f, -0.4f);
                    }
                    break;
                case Mode.Charge:
                    World.Particles.Add(new Particle(World, Position, Vector2.Zero, Tint, 34f, 0.4f));
                    if (_timer <= 0f)
                    {
                        _mode = Mode.Stalk;
                        _timer = Rng.Range(1.5f, 2.6f);
                        Velocity = Vector2.Zero;
                    }
                    break;
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);
            if (_mode == Mode.Aim)
            {
                Vector2 end = Position + Util.FromAngle(Rotation) * 620f;
                Ui.Line(batch, Position, end, Tint * 0.5f, 2f);
            }
        }
    }

    /// <summary>Its front shield blocks every shot. Get behind it, or use homing rounds.</summary>
    public class Bulwark : Enemy
    {
        private const float ShieldArc = 1.05f;
        private float _cooldown = Rng.Range(1.5f, 3f);

        public Bulwark(World world, Vector2 position) : base(world, "bulwark", position, Color.SlateBlue, 6f, 350, 5, 0.6f, 32f)
        {
        }

        protected override void Think(float dt)
        {
            Rotation = Util.TurnToward(Rotation, Util.AngleOf(ToPlayer), 1.15f * dt);
            Velocity = Util.FromAngle(Rotation) * 62f;

            _cooldown -= dt;
            if (_cooldown <= 0f)
            {
                _cooldown = 2.6f;
                FireInDirection(Rotation, 260f);
            }
        }

        public override void Hit(float damage, Vector2 from)
        {
            float incoming = Util.AngleOf(from - Position);
            if (Math.Abs(MathHelper.WrapAngle(incoming - Rotation)) < ShieldArc)
            {
                World.Burst(from, Color.White, 3, 160f);
                Sfx.Play("shield_hit", 0.25f, 0.5f);
                return;
            }
            base.Hit(damage, from);
        }
    }

    /// <summary>Long range. A thin line tracks you, then locks. Move once it stops.</summary>
    public class Sniper : Enemy
    {
        private const float TrackTime = 1.3f;
        private const float LockTime = 0.4f;

        private float _timer = TrackTime + LockTime;
        private float _lockedAngle;
        private int _shotsSinceMove;

        public Sniper(World world, Vector2 position) : base(world, "sniper", position, Color.Tomato, 2.5f, 300, 4, 0.55f, 22f)
        {
        }

        protected override void Think(float dt)
        {
            Velocity = Vector2.Zero;
            _timer -= dt;

            if (_timer > LockTime)
            {
                _lockedAngle = Util.AngleOf(ToPlayer);
            }
            Rotation = _lockedAngle;

            if (_timer <= 0f)
            {
                FireInDirection(_lockedAngle, 880f);
                _timer = TrackTime + LockTime;
                _shotsSinceMove++;
                if (_shotsSinceMove >= 2)
                {
                    _shotsSinceMove = 0;
                    World.Burst(Position, Tint, 10, 200f);
                    Position = World.RandomEdgePoint();
                }
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            base.Draw(batch);
            if (!IsWarpingIn)
            {
                bool locked = _timer <= LockTime;
                Vector2 end = Position + Util.FromAngle(_lockedAngle) * 1900f;
                Ui.Line(batch, Position, end, locked ? Color.White * 0.85f : Tint * 0.3f, locked ? 2.5f : 1.5f);
            }
        }
    }

    /// <summary>Big, slow and tough. Keeps launching swarmers until it is destroyed.</summary>
    public class Carrier : Enemy
    {
        private const int MaxSwarmers = 16;

        private float _launchTimer = 2.5f;

        public Carrier(World world, Vector2 position) : base(world, "carrier", position, Color.MediumSeaGreen, 18f, 800, 12, 0.8f, 56f)
        {
            Velocity = new Vector2(Rng.Chance(0.5f) ? 55f : -55f, Rng.Range(-30f, 30f));
        }

        protected override void Think(float dt)
        {
            if (Position.X <= Radius + 1f || Position.X >= Game1.ArenaWidth - Radius - 1f)
            {
                Velocity.X = -Velocity.X;
            }
            if (Position.Y <= Radius + 1f || Position.Y >= Game1.ArenaHeight - Radius - 1f)
            {
                Velocity.Y = -Velocity.Y;
            }

            _launchTimer -= dt;
            if (_launchTimer <= 0f)
            {
                _launchTimer = 5f;
                if (World.CountEnemies<Swarmer>() < MaxSwarmers)
                {
                    World.Enemies.Add(new Swarmer(World, Position + Vector2.UnitX * 40f));
                    World.Enemies.Add(new Swarmer(World, Position - Vector2.UnitX * 40f));
                }
            }
        }
    }

    /// <summary>Nearly invisible until it fades in to fire. Then it vanishes and relocates.</summary>
    public class Phantom : Enemy
    {
        private readonly Color _visibleTint = Color.MediumSlateBlue;
        private float _timer = Rng.Range(1f, 2.5f);
        private bool _visible;

        public Phantom(World world, Vector2 position) : base(world, "phantom", position, Color.MediumSlateBlue, 4f, 400, 5, 0.55f, 24f)
        {
        }

        protected override void Think(float dt)
        {
            _timer -= dt;
            Velocity = Vector2.Lerp(Velocity, ToPlayer * (_visible ? 30f : 120f), Math.Min(1f, 2f * dt));

            if (_timer <= 0f)
            {
                _visible = !_visible;
                _timer = _visible ? 1.6f : Rng.Range(1.6f, 2.8f);
                if (_visible)
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        FireAtPlayerWithOffset(i * 0.22f);
                    }
                }
            }

            Tint = _visible ? _visibleTint : _visibleTint * 0.13f;
        }

        private void FireAtPlayerWithOffset(float offset)
        {
            World.EnemyShots.Add(new Bullet(World, Position, Util.FromAngle(Util.AngleOf(ToPlayer) + offset) * 300f * World.ShotSpeedScale, _visibleTint, 1f, 4f));
        }
    }

    /// <summary>Orbits the final boss. While any are alive, the boss can't be damaged.</summary>
    public class ShieldNode : Enemy
    {
        private readonly Entity _anchor;
        private float _orbit;
        private float _cooldown;

        public ShieldNode(World world, Entity anchor, float startAngle) : base(world, "turret", anchor.Position, Color.Crimson, 12f, 500, 6, 0.6f, 26f)
        {
            _anchor = anchor;
            _orbit = startAngle;
            _cooldown = Rng.Range(1f, 2.5f);
            SkipWarpIn();
        }

        protected override void Think(float dt)
        {
            _orbit += 0.55f * dt;
            Position = _anchor.Position + Util.FromAngle(_orbit) * 240f;
            Rotation = Util.AngleOf(ToPlayer);

            _cooldown -= dt;
            if (_cooldown <= 0f)
            {
                _cooldown = 2.2f;
                FireAtPlayer(330f, 0.05f);
            }
        }

        protected override void KeepInArena()
        {
        }
    }
}
