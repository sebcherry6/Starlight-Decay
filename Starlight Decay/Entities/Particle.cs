namespace Starlight_Decay
{
    /// <summary>A soft glowing speck. Drawn with additive blending, so overlapping sparks get brighter.</summary>
    public class Particle : Entity
    {
        private readonly float _lifetime;
        private readonly float _startScale;
        private readonly float _drag;
        private float _age;

        public Particle(World world, Vector2 position, Vector2 velocity, Color color, float size, float lifetime, float drag = 0.2f) : base(world)
        {
            SetTexture("glow");
            Position = position;
            Velocity = velocity;
            Tint = color;
            _startScale = size / Source.Width;
            Scale = _startScale;
            _lifetime = lifetime;
            _drag = drag;
        }

        public override void Update(float dt)
        {
            _age += dt;
            if (_age >= _lifetime)
            {
                Dead = true;
                return;
            }
            Velocity *= MathF.Pow(_drag, dt);
            Scale = _startScale * (1f - _age / _lifetime);
            base.Update(dt);
        }
    }

    /// <summary>Score and status text that floats up and fades.</summary>
    public class FloatingText
    {
        private const float Lifetime = 1.1f;

        private readonly string _text;
        private readonly Color _color;
        private Vector2 _position;
        private float _age;

        public bool Dead => _age >= Lifetime;

        public FloatingText(string text, Vector2 position, Color color)
        {
            _text = text;
            _position = position;
            _color = color;
        }

        public void Update(float dt)
        {
            _age += dt;
            _position.Y -= 42f * dt;
        }

        public void Draw(SpriteBatch batch)
        {
            float fade = 1f - _age / Lifetime;
            Ui.Centered(batch, Assets.Hud, _text, _position.X, _position.Y, _color * fade);
        }
    }

    /// <summary>A slowly drifting three-layer star background.</summary>
    public class Starfield
    {
        private const int StarCount = 170;

        private readonly Vector2[] _positions = new Vector2[StarCount];
        private readonly float[] _depths = new float[StarCount];
        private readonly Vector2 _drift;

        public Starfield(Vector2 drift)
        {
            _drift = drift;
            for (int i = 0; i < StarCount; i++)
            {
                _positions[i] = new Vector2(Rng.Range(0f, Game1.ArenaWidth), Rng.Range(0f, Game1.ArenaHeight));
                _depths[i] = 0.25f + (i % 3) * 0.35f;
            }
        }

        public void Update(float dt)
        {
            for (int i = 0; i < StarCount; i++)
            {
                Vector2 next = _positions[i] + _drift * _depths[i] * dt;
                next.X = (next.X + Game1.ArenaWidth) % Game1.ArenaWidth;
                next.Y = (next.Y + Game1.ArenaHeight) % Game1.ArenaHeight;
                _positions[i] = next;
            }
        }

        public void Draw(SpriteBatch batch, Color tint)
        {
            for (int i = 0; i < StarCount; i++)
            {
                int size = _depths[i] > 0.9f ? 3 : 2;
                Vector2 star = _positions[i];
                Ui.Fill(batch, new Rectangle((int)star.X, (int)star.Y, size, size), tint * _depths[i]);
            }
        }
    }
}
