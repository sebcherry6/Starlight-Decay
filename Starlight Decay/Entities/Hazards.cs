namespace Starlight_Decay
{
    /// <summary>Arena dangers that aren't enemies and can't be destroyed.</summary>
    public abstract class Hazard : Entity
    {
        protected Hazard(World world) : base(world)
        {
        }
    }

    /// <summary>Pulls the player, rocks and enemy shots in. The core itself does damage.</summary>
    public class GravityWell : Hazard
    {
        private const float Reach = 430f;
        private const float Pull = 520f;

        private float _life;

        public GravityWell(World world, Vector2 position, float life = 0f) : base(world)
        {
            SetTexture("swirl");
            Position = position;
            Radius = 34f;
            Scale = 0.85f;
            RotationSpeed = 1.6f;
            Tint = Color.MediumPurple;
            Velocity = Util.FromAngle(Rng.Angle()) * 22f;
            Wraps = true;
            _life = life;
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            if (_life > 0f)
            {
                _life -= dt;
                if (_life <= 0f)
                {
                    Dead = true;
                    return;
                }
            }

            Attract(World.Player, dt, 1f);
            foreach (Rock rock in World.Rocks)
            {
                Attract(rock, dt, 0.25f);
            }
            foreach (Bullet shot in World.EnemyShots)
            {
                Attract(shot, dt, 0.5f);
            }

            if (!World.Player.Dead && World.Player.Touches(this))
            {
                World.Player.TakeHit();
            }
        }

        private void Attract(Entity entity, float dt, float strength)
        {
            float distance = Vector2.Distance(Position, entity.Position);
            if (distance < Reach && distance > 1f)
            {
                float falloff = 1f - distance / Reach;
                entity.Velocity += Util.Direction(entity.Position, Position) * Pull * falloff * strength * dt;
            }
        }
    }

    /// <summary>A lightning strike: a warning circle first, then damage to anything still inside it.</summary>
    public class IonStrike : Hazard
    {
        private const float WarningTime = 1.25f;
        private const float BoltTime = 0.28f;

        private float _timer;
        private bool _struck;

        public IonStrike(World world, Vector2 position) : base(world)
        {
            Position = Util.ClampToArena(position, 60f);
            Radius = 96f;
        }

        public override void Update(float dt)
        {
            _timer += dt;
            if (!_struck && _timer >= WarningTime)
            {
                _struck = true;
                Sfx.Play("ion", 0.6f, Rng.Range(-0.2f, 0.2f));
                World.Burst(Position, Color.LightCyan, 16, 340f);
                World.Shake(4f);
                if (!World.Player.Dead && World.Player.Touches(this))
                {
                    World.Player.TakeHit();
                }
            }
            if (_timer >= WarningTime + BoltTime)
            {
                Dead = true;
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            if (!_struck)
            {
                float progress = _timer / WarningTime;
                Ui.Ring(batch, Position, Radius, Color.Cyan * 0.8f);
                Ui.Ring(batch, Position, Radius * (1f - progress), Color.White * 0.8f);
            }
            else
            {
                Vector2 from = new Vector2(Position.X + Rng.Range(-40f, 40f), -10f);
                Vector2 kink = new Vector2(Position.X + Rng.Range(-60f, 60f), Position.Y * 0.55f);
                Ui.Line(batch, from, kink, Color.White, 5f);
                Ui.Line(batch, kink, Position, Color.White, 5f);
                Ui.Ring(batch, Position, Radius, Color.White);
            }
        }
    }

    /// <summary>A laser wall that sweeps across the arena. Fly through its gap.</summary>
    public class SweepBeam : Hazard
    {
        private const float GapSize = 210f;
        private const float WarningTime = 1.1f;
        private const float Speed = 210f;

        private readonly bool _vertical;
        private readonly float _direction;
        private readonly float _gapCenter;
        private float _offset;
        private float _warning = WarningTime;

        /// <param name="vertical">True for a vertical wall moving sideways.</param>
        public SweepBeam(World world, bool vertical, bool reversed) : base(world)
        {
            _vertical = vertical;
            _direction = reversed ? -1f : 1f;
            float length = vertical ? Game1.ArenaHeight : Game1.ArenaWidth;
            float travel = vertical ? Game1.ArenaWidth : Game1.ArenaHeight;
            _offset = reversed ? travel - 12f : 12f;
            _gapCenter = Rng.Range(GapSize, length - GapSize);
        }

        public override void Update(float dt)
        {
            if (_warning > 0f)
            {
                _warning -= dt;
                return;
            }

            _offset += Speed * _direction * dt;
            float travel = _vertical ? Game1.ArenaWidth : Game1.ArenaHeight;
            if (_offset < -10f || _offset > travel + 10f)
            {
                Dead = true;
                return;
            }

            Player player = World.Player;
            float across = _vertical ? player.Position.X : player.Position.Y;
            float along = _vertical ? player.Position.Y : player.Position.X;
            bool onBeam = Math.Abs(across - _offset) < player.Radius + 5f;
            bool inGap = Math.Abs(along - _gapCenter) < GapSize / 2f;
            if (!player.Dead && onBeam && !inGap)
            {
                player.TakeHit();
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            bool warning = _warning > 0f;
            Color color = warning ? Color.Red * (0.35f + 0.35f * MathF.Sin(_warning * 24f)) : Color.Red;
            float thickness = warning ? 3f : 7f;
            float length = _vertical ? Game1.ArenaHeight : Game1.ArenaWidth;

            Ui.Line(batch, Point(0f), Point(_gapCenter - GapSize / 2f), color, thickness);
            Ui.Line(batch, Point(_gapCenter + GapSize / 2f), Point(length), color, thickness);
        }

        private Vector2 Point(float along)
        {
            return _vertical ? new Vector2(_offset, along) : new Vector2(along, _offset);
        }
    }
}
