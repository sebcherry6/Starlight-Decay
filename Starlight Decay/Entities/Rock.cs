namespace Starlight_Decay
{
    public enum RockKind
    {
        Stone,    // The classic.
        Ice,      // Tougher.
        Magma,    // Bursts into fire when broken.
        Crystal   // Rich in scrap.
    }

    public class Rock : Entity
    {
        // Size 0 is the smallest. These are the shapes in the original SpriteRocks sheet.
        private static readonly Rectangle[] Shapes =
        {
            new Rectangle(344, 216, 40, 40),
            new Rectangle(256, 128, 80, 80),
            new Rectangle(256, 0, 128, 128),
            new Rectangle(0, 0, 256, 256)
        };

        private readonly RockKind _kind;
        private readonly int _size;
        private float _hp;
        private float _flash;

        public Rock(World world, RockKind kind, int size, Vector2 position, float maxSpeed) : base(world)
        {
            _kind = kind;
            _size = size;
            Texture = Assets.Tex("SpriteRocks");
            Source = Shapes[size];
            Origin = new Vector2(Source.Width / 2f, Source.Height / 2f);
            Scale = 0.8f;
            Radius = Source.Width * Scale * 0.42f;
            Position = position;
            Rotation = Rng.Angle();
            RotationSpeed = Rng.Range(-2f, 2f);
            Velocity = Util.FromAngle(Rng.Angle()) * Rng.Range(maxSpeed * 0.4f, maxSpeed);
            Wraps = true;

            _hp = 1f + size;
            if (kind == RockKind.Ice)
            {
                _hp *= 2f;
            }
            _hp *= World.EnemyHealthScale;
            Tint = ColorFor(kind);
        }

        private static Color ColorFor(RockKind kind)
        {
            switch (kind)
            {
                case RockKind.Ice: return Color.LightSkyBlue;
                case RockKind.Magma: return Color.OrangeRed;
                case RockKind.Crystal: return Color.MediumOrchid;
                default: return Color.YellowGreen;
            }
        }

        public override void Update(float dt)
        {
            _flash -= dt;
            base.Update(dt);
        }

        public override void Draw(SpriteBatch batch)
        {
            Color color = _flash > 0f ? Color.White : Tint;
            batch.Draw(Texture, Position, Source, color, Rotation, Origin, Scale, SpriteEffects.None, 0f);
        }

        public void Hit(float damage)
        {
            _hp -= damage;
            _flash = 0.07f;
            if (_hp <= 0f)
            {
                Shatter(true);
            }
            else
            {
                Sfx.Play("hit", 0.25f, Rng.Range(-0.2f, 0.2f));
                World.Burst(Position, Tint, 3, 120f);
            }
        }

        /// <summary>Breaks the rock. Rocks cleared by a boss arriving pay nothing and don't split.</summary>
        public void Shatter(bool rewarded)
        {
            if (Dead)
            {
                return;
            }
            Dead = true;
            Sfx.Play("explosion", 0.35f + _size * 0.12f, 0.2f - _size * 0.15f);
            World.Burst(Position, Tint, 5 + _size * 7, 200f + _size * 60f);
            World.Shake(1.5f + _size);

            if (!rewarded)
            {
                return;
            }

            World.AddScore((4 - _size) * 25, Position);
            int scrap = _kind == RockKind.Crystal ? 2 + _size * 2 : (_size > 0 ? 1 : 0);
            World.DropLoot(Position, scrap, 0.05f);

            if (_size > 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    World.Rocks.Add(new Rock(World, _kind, _size - 1, Position, 170f + 30f * (3 - _size)));
                }
            }

            if (_kind == RockKind.Magma && _size >= 1)
            {
                int flames = 4 + _size * 2;
                float offset = Rng.Angle();
                for (int i = 0; i < flames; i++)
                {
                    float angle = offset + i * MathHelper.TwoPi / flames;
                    World.EnemyShots.Add(new Bullet(World, Position, Util.FromAngle(angle) * 240f, Color.Orange, 1f, 1.1f));
                }
            }
        }
    }
}
