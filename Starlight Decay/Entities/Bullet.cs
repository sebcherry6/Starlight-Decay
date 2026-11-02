namespace Starlight_Decay
{
    /// <summary>A shot from either side. Which list it is in decides who it can hit.</summary>
    public class Bullet : Entity
    {
        private readonly HashSet<Entity> _alreadyHit = new HashSet<Entity>();
        private float _life;

        public float Damage = 1f;
        public bool Piercing;
        public bool Homing;

        public Bullet(World world, Vector2 position, Vector2 velocity, Color color, float damage = 1f, float life = 1.5f, float scale = 0.6f) : base(world)
        {
            SetTexture("bolt");
            Position = position;
            Velocity = velocity;
            Tint = color;
            Damage = damage;
            _life = life;
            Scale = scale;
            Radius = 7f * scale + 2f;
            Rotation = Util.AngleOf(velocity);
        }

        /// <summary>True the first time this bullet touches a target. Lets piercing shots hit each target once.</summary>
        public bool FirstHitOn(Entity target)
        {
            return _alreadyHit.Add(target);
        }

        public override void Update(float dt)
        {
            _life -= dt;
            if (Homing)
            {
                Entity target = World.NearestTarget(Position, 520f);
                if (target != null)
                {
                    float speed = Velocity.Length();
                    float heading = Util.TurnToward(Util.AngleOf(Velocity), Util.AngleOf(target.Position - Position), 5.5f * dt);
                    Velocity = Util.FromAngle(heading) * speed;
                }
            }
            Rotation = Util.AngleOf(Velocity);
            base.Update(dt);

            if (_life <= 0f || IsOffArena(40f))
            {
                Dead = true;
            }
        }
    }
}
