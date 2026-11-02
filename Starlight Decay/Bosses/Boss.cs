namespace Starlight_Decay
{
    /// <summary>
    /// Plays one row of the boss table. Attacks run through three steps: idle, a short telegraph
    /// where the boss glows, then the attack itself.
    /// </summary>
    public class Boss : Entity
    {
        private enum Step { Enter, Idle, Telegraph, Attack, Dying }

        private const float TelegraphTime = 0.55f;
        private const float DeathTime = 2.4f;
        private const int LastPhase = 2;
        private const int NodesPerPhase = 4;

        private readonly BossData _data;
        private readonly Vector2 _homePosition;
        private Step _step = Step.Enter;
        private BossAttack _attack;
        private float _timer;
        private float _burstTimer;
        private int _burstsLeft;
        private int _attackIndex;
        private float _spiralAngle;
        private float _orbitAngle = -MathHelper.PiOver2;
        private float _hoverDirection = 1f;
        private float _flash;
        private float _cloak;
        private float _explosionTimer;
        private Vector2 _chargeDirection;
        private readonly List<ShieldNode> _nodes = new List<ShieldNode>();

        public float Health { get; private set; }
        public int Phase { get; private set; }
        public string Name => _data.Name;
        public float HealthFraction => Math.Max(0f, Health / MaxHealth);
        public float MaxHealth => _data.Health;
        public bool IsDying => _step == Step.Dying;
        public bool IsEntering => _step == Step.Enter;
        public bool IsCloaked => _cloak > 0f;
        public bool IsTargetable => !IsEntering && !IsDying && !IsCloaked;
        public bool IsShielded => _nodes.Count > 0;

        private float PhaseSpeed => 1f + 0.22f * Phase;

        public Boss(World world, BossData data) : base(world)
        {
            _data = data;
            SetTexture(data.Texture);
            Tint = data.Tint;
            Scale = data.Scale;
            Radius = data.Radius;
            Health = data.Health;
            _homePosition = new Vector2(Game1.ArenaWidth / 2f, data.Move == BossMove.Anchor ? Game1.ArenaHeight / 2f : 220f);
            Position = new Vector2(Game1.ArenaWidth / 2f, -220f);
            Velocity = Util.FromAngle(Rng.Range(0.4f, 1.2f)) * data.Speed;
            bool spins = data.Move == BossMove.Drift || data.Move == BossMove.Orbit || data.Move == BossMove.Anchor;
            RotationSpeed = spins ? 0.22f : 0f;
        }

        public override void Update(float dt)
        {
            _flash -= dt;
            _nodes.RemoveAll(node => node.Dead);

            switch (_step)
            {
                case Step.Enter:
                    UpdateEntrance(dt);
                    return;
                case Step.Dying:
                    UpdateDeath(dt);
                    return;
            }

            UpdateMovement(dt);
            UpdateCloak(dt);
            UpdateAttacks(dt);
            Rotation += RotationSpeed * dt;
        }

        private void UpdateEntrance(float dt)
        {
            Position = Vector2.Lerp(Position, _homePosition, Math.Min(1f, 1.9f * dt));
            if (Vector2.Distance(Position, _homePosition) < 6f)
            {
                _step = Step.Idle;
                _timer = 1f;
                if (_data.NodeShield)
                {
                    SpawnNodes();
                }
            }
        }

        private void UpdateDeath(float dt)
        {
            _timer -= dt;
            _explosionTimer -= dt;
            if (_explosionTimer <= 0f)
            {
                _explosionTimer = 0.14f;
                Vector2 spot = Position + Util.FromAngle(Rng.Angle()) * Rng.Range(0f, Radius);
                World.Burst(spot, Tint, 22, 380f);
                Sfx.Play("explosion", 0.7f, Rng.Range(-0.4f, 0.1f));
                World.Shake(7f);
            }
            if (_timer <= 0f)
            {
                Dead = true;
                World.Burst(Position, Color.White, 120, 700f);
                World.Shake(26f);
                Sfx.Play("boss_die", 1f);
                World.OnBossDestroyed(this);
            }
        }

        private void UpdateMovement(float dt)
        {
            if (_step == Step.Attack && _attack == BossAttack.Charge)
            {
                return;
            }

            float margin = Radius * 0.7f;
            switch (_data.Move)
            {
                case BossMove.Drift:
                    Position += Velocity * PhaseSpeed * dt;
                    if (Position.X < margin || Position.X > Game1.ArenaWidth - margin)
                    {
                        Velocity.X = -Velocity.X;
                    }
                    if (Position.Y < margin || Position.Y > Game1.ArenaHeight - margin)
                    {
                        Velocity.Y = -Velocity.Y;
                    }
                    Position = Util.ClampToArena(Position, margin);
                    break;
                case BossMove.Hover:
                    Position.X += _hoverDirection * _data.Speed * PhaseSpeed * dt;
                    if (Position.X < margin + 60f || Position.X > Game1.ArenaWidth - margin - 60f)
                    {
                        _hoverDirection = -_hoverDirection;
                    }
                    Position = Util.ClampToArena(Position, margin);
                    break;
                case BossMove.Orbit:
                    _orbitAngle += _data.Speed * PhaseSpeed * dt;
                    Vector2 center = new Vector2(Game1.ArenaWidth / 2f, Game1.ArenaHeight / 2f);
                    Vector2 target = center + new Vector2(MathF.Cos(_orbitAngle) * 520f, MathF.Sin(_orbitAngle) * 250f);
                    Position = Vector2.Lerp(Position, target, Math.Min(1f, 2.5f * dt));
                    break;
                case BossMove.Chase:
                    Position += Util.Direction(Position, World.Player.Position) * _data.Speed * PhaseSpeed * dt;
                    Position = Util.ClampToArena(Position, margin);
                    break;
                case BossMove.Blink:
                case BossMove.Anchor:
                    break;
            }
        }

        private void UpdateCloak(float dt)
        {
            if (_cloak > 0f)
            {
                _cloak -= dt;
                if (_cloak <= 0f)
                {
                    BlinkAway();
                    FireRing(18, 300f);
                }
            }
        }

        private void UpdateAttacks(float dt)
        {
            _timer -= dt;
            switch (_step)
            {
                case Step.Idle:
                    if (_timer <= 0f)
                    {
                        BossAttack[] rotation = _data.Phases[Phase];
                        _attack = rotation[_attackIndex % rotation.Length];
                        _attackIndex++;
                        _step = Step.Telegraph;
                        _timer = TelegraphTime;
                        if (_data.Move == BossMove.Blink && _attack != BossAttack.Cloak)
                        {
                            BlinkAway();
                        }
                    }
                    break;
                case Step.Telegraph:
                    if (_timer <= 0f)
                    {
                        BeginAttack();
                    }
                    break;
                case Step.Attack:
                    RunAttack(dt);
                    if (_timer <= 0f)
                    {
                        _step = Step.Idle;
                        _timer = _data.AttackDelay / PhaseSpeed;
                        if (_attack == BossAttack.Charge)
                        {
                            Velocity = Util.FromAngle(Rng.Angle()) * _data.Speed;
                        }
                    }
                    break;
            }
        }

        private void BeginAttack()
        {
            _step = Step.Attack;
            _timer = 0.1f;
            int extraShots = Phase * 3;

            switch (_attack)
            {
                case BossAttack.Ring:
                    FireRing(10 + World.Run.Sector + extraShots, 235f);
                    break;
                case BossAttack.Nova:
                    _burstsLeft = 3;
                    _burstTimer = 0f;
                    _timer = 1.1f;
                    break;
                case BossAttack.Fan:
                    _burstsLeft = 2 + Phase;
                    _burstTimer = 0f;
                    _timer = 1.2f;
                    break;
                case BossAttack.Spiral:
                    _timer = 2.6f;
                    _burstTimer = 0f;
                    _spiralAngle = Rng.Angle();
                    break;
                case BossAttack.RockSpit:
                    SpitRocks();
                    break;
                case BossAttack.Summon:
                    Summon();
                    break;
                case BossAttack.Mines:
                    DropMines();
                    break;
                case BossAttack.Charge:
                    _chargeDirection = Util.Direction(Position, World.Player.Position);
                    _timer = 0.85f;
                    Sfx.Play("dash", 0.7f, -0.5f);
                    break;
                case BossAttack.Lightning:
                    CallLightning();
                    break;
                case BossAttack.Beam:
                    World.Hazards.Add(new SweepBeam(World, Rng.Chance(0.5f), Rng.Chance(0.5f)));
                    Sfx.Play("beam", 0.6f);
                    break;
                case BossAttack.Gravity:
                    World.Hazards.Add(new GravityWell(World, World.RandomInnerPoint(), 7f));
                    Sfx.Play("beam", 0.5f, -0.5f);
                    break;
                case BossAttack.Cloak:
                    _cloak = 2.4f;
                    _timer = 0.4f;
                    World.Burst(Position, Tint, 30, 300f);
                    break;
            }
        }

        private void RunAttack(float dt)
        {
            _burstTimer -= dt;
            switch (_attack)
            {
                case BossAttack.Nova:
                    if (_burstsLeft > 0 && _burstTimer <= 0f)
                    {
                        _burstsLeft--;
                        _burstTimer = 0.36f;
                        FireRing(10 + World.Run.Sector + Phase * 3, 190f + _burstsLeft * 55f);
                    }
                    break;
                case BossAttack.Fan:
                    if (_burstsLeft > 0 && _burstTimer <= 0f)
                    {
                        _burstsLeft--;
                        _burstTimer = 0.27f;
                        FireFan(3 + Phase + World.Run.Sector / 4, 0.26f, 325f);
                    }
                    break;
                case BossAttack.Spiral:
                    if (_burstTimer <= 0f)
                    {
                        _burstTimer = 0.13f;
                        _spiralAngle += 0.42f;
                        int arms = 2 + Phase + World.Run.Sector / 5;
                        for (int i = 0; i < arms; i++)
                        {
                            Shoot(_spiralAngle + i * MathHelper.TwoPi / arms, 245f);
                        }
                    }
                    break;
                case BossAttack.Charge:
                    Position += _chargeDirection * 900f * dt;
                    World.Particles.Add(new Particle(World, Position, Vector2.Zero, Tint, 90f, 0.45f));
                    Position = Util.ClampToArena(Position, Radius * 0.7f);
                    break;
            }
        }

        private void Shoot(float angle, float speed)
        {
            World.EnemyShots.Add(new Bullet(World, Position, Util.FromAngle(angle) * speed * World.ShotSpeedScale, Tint, 1f, 5.5f, 0.75f));
            Sfx.Play("enemy_shot", 0.2f);
        }

        private void FireRing(int count, float speed)
        {
            float offset = Rng.Angle();
            for (int i = 0; i < count; i++)
            {
                Shoot(offset + i * MathHelper.TwoPi / count, speed);
            }
        }

        private void FireFan(int count, float gap, float speed)
        {
            float center = Util.AngleOf(World.Player.Position - Position);
            for (int i = 0; i < count; i++)
            {
                Shoot(center + (i - (count - 1) / 2f) * gap, speed);
            }
        }

        private void SpitRocks()
        {
            int count = 3 + Phase;
            float center = Util.AngleOf(World.Player.Position - Position);
            for (int i = 0; i < count; i++)
            {
                Rock rock = new Rock(World, World.Level.RockKind, 1, Position, 150f);
                rock.Velocity = Util.FromAngle(center + (i - (count - 1) / 2f) * 0.4f) * 270f;
                World.Rocks.Add(rock);
            }
            Sfx.Play("explosion", 0.5f, 0.4f);
        }

        private void Summon()
        {
            // Never let summons pile up: at most six extra enemies at a time.
            int room = 6 - World.Enemies.Count + _nodes.Count;
            int count = Math.Min(room, 2 + Phase);
            for (int i = 0; i < count; i++)
            {
                World.Enemies.Add(Enemy.Create(World, _data.Summons, World.RandomEdgePoint()));
            }
        }

        private void DropMines()
        {
            int count = 3 + Phase;
            float offset = Rng.Angle();
            for (int i = 0; i < count; i++)
            {
                Vector2 spot = Position + Util.FromAngle(offset + i * MathHelper.TwoPi / count) * (Radius + 90f);
                World.Enemies.Add(new Mine(World, Util.ClampToArena(spot, 40f)));
            }
        }

        private void CallLightning()
        {
            int count = 4 + Phase * 2;
            for (int i = 0; i < count; i++)
            {
                float reach = i == 0 ? 0f : Rng.Range(120f, 430f);
                Vector2 spot = World.Player.Position + Util.FromAngle(Rng.Angle()) * reach;
                World.Hazards.Add(new IonStrike(World, spot));
            }
        }

        private void BlinkAway()
        {
            World.Burst(Position, Tint, 24, 300f);
            Position = World.RandomBossPoint(Radius);
            World.Burst(Position, Tint, 24, 300f);
        }

        private void SpawnNodes()
        {
            for (int i = 0; i < NodesPerPhase; i++)
            {
                ShieldNode node = new ShieldNode(World, this, i * MathHelper.TwoPi / NodesPerPhase);
                _nodes.Add(node);
                World.Enemies.Add(node);
            }
            World.Float("DESTROY THE SHIELD NODES", Position - Vector2.UnitY * 190f, Color.Crimson);
        }

        public void Hit(float damage, Vector2 from)
        {
            if (!IsTargetable)
            {
                return;
            }
            if (IsShielded)
            {
                World.Burst(from, Color.White, 3, 200f);
                Sfx.Play("shield_hit", 0.25f, 0.5f);
                return;
            }

            _flash = 0.07f;
            Sfx.Play("hit", 0.3f, Rng.Range(-0.3f, 0f));

            // Health stops at each phase boundary, so a big hit can't skip a whole phase.
            float phaseFloor = Phase >= LastPhase ? 0f : MaxHealth * (LastPhase - Phase) / 3f;
            Health = Math.Max(Health - damage, phaseFloor);

            if (Health <= 0f)
            {
                BeginDeath();
            }
            else if (Health <= phaseFloor)
            {
                EnterNextPhase();
            }
        }

        private void EnterNextPhase()
        {
            Phase++;
            _attackIndex = 0;
            _step = Step.Idle;
            _timer = 1.2f;
            World.EnemyShots.Clear();
            FireRing(20, 200f);
            World.Burst(Position, Color.White, 50, 500f);
            World.Shake(14f);
            World.Float("PHASE " + (Phase + 1), Position, Color.White);
            Sfx.Play("explosion", 1f, -0.5f);
            World.DropBossSupply(Position);

            if (_data.NodeShield)
            {
                SpawnNodes();
            }
        }

        private void BeginDeath()
        {
            _step = Step.Dying;
            _timer = DeathTime;
            _cloak = 0f;
            World.BeginBossDeath();
        }

        public override void Draw(SpriteBatch batch)
        {
            Color color = _flash > 0f ? Color.White : Tint;

            if (_step == Step.Telegraph)
            {
                float pulse = 0.5f + 0.5f * MathF.Sin(_timer * 28f);
                color = Color.Lerp(color, Color.White, pulse);
            }
            if (_step == Step.Attack && _attack == BossAttack.Charge)
            {
                color = Color.White;
            }
            if (IsCloaked)
            {
                color = color * 0.1f;
            }
            if (IsDying)
            {
                color = Color.Lerp(Tint, Color.White, 1f - _timer / DeathTime);
            }

            batch.Draw(Texture, Position, Source, color, Rotation, Origin, Scale, SpriteEffects.None, 0f);

            if (IsShielded)
            {
                Ui.Ring(batch, Position, Radius * 1.25f, Color.Crimson * 0.75f);
            }
        }
    }
}
