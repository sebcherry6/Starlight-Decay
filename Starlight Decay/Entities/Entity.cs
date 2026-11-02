namespace Starlight_Decay
{
    /// <summary>Anything that exists in the arena: a position, a velocity, a sprite and a collision circle.</summary>
    public abstract class Entity
    {
        public World World;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Rotation;
        public float RotationSpeed;
        public float Radius;
        public float Scale = 1f;
        public Color Tint = Color.White;
        public Texture2D Texture;
        public Rectangle Source;
        public Vector2 Origin;
        public bool Dead;
        public bool Wraps;

        protected Entity(World world)
        {
            World = world;
        }

        protected void SetTexture(string name)
        {
            Texture = Assets.Tex(name);
            Source = Texture.Bounds;
            Origin = new Vector2(Source.Width / 2f, Source.Height / 2f);
        }

        public virtual void Update(float dt)
        {
            Position += Velocity * dt;
            Rotation += RotationSpeed * dt;
            if (Wraps)
            {
                WrapAroundArena();
            }
        }

        public virtual void Draw(SpriteBatch batch)
        {
            batch.Draw(Texture, Position, Source, Tint, Rotation, Origin, Scale, SpriteEffects.None, 0f);
        }

        public bool Touches(Entity other)
        {
            float reach = Radius + other.Radius;
            return Vector2.DistanceSquared(Position, other.Position) <= reach * reach;
        }

        public bool IsOffArena(float margin)
        {
            return Position.X < -margin || Position.X > Game1.ArenaWidth + margin
                || Position.Y < -margin || Position.Y > Game1.ArenaHeight + margin;
        }

        private void WrapAroundArena()
        {
            float margin = Radius + 8f;
            if (Position.X < -margin)
            {
                Position.X = Game1.ArenaWidth + margin;
            }
            else if (Position.X > Game1.ArenaWidth + margin)
            {
                Position.X = -margin;
            }
            if (Position.Y < -margin)
            {
                Position.Y = Game1.ArenaHeight + margin;
            }
            else if (Position.Y > Game1.ArenaHeight + margin)
            {
                Position.Y = -margin;
            }
        }
    }
}
