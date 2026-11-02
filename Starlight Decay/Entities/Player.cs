namespace Starlight_Decay
{
    public enum WeaponMode
    {
        Standard, Spread, Rapid, Pierce, Homing
    }

    public class Player : Entity
    {
        private const float TurnSpeed = 4.3f;
        private const float ShotSpeed = 900f;
        private const float DashSpeed = 1050f;
        private const float DashDuration = 0.16f;
        private const float ShieldRechargeDelay = 7f;

        private readonly List<Wingman> _wingmen = new List<Wingman>();
        private float _fireCooldown;
        private float _dashTime;
        private float _shieldTimer;
        private float _trailTimer;

        public float Invulnerable;
        public float WeaponTimer;
        public float DoubleScoreTimer;
        public float DashCooldown;
        public WeaponMode Weapon = WeaponMode.Standard;

        public bool IsDashing => _dashTime > 0f;

        public Vector2 Forward => Util.FromAngle(Rotation);

        public Player(World world, Vector2 position) : base(world)
        {
            SetTexture("ship");
            Position = position;
            Rotation = -MathHelper.PiOver2;
            Scale = 0.5f;
            Radius = 18f;
            Wraps = true;
            Tint = Color.Turquoise;
            Invulnerable = 2f;

            for (int i = 0; i < World.Run.WingmanCount; i++)
            {
                _wingmen.Add(new Wingman(World, this, i));
            }
        }

        public void SetWeapon(WeaponMode weapon, float duration)
        {
            Weapon = weapon;
            WeaponTimer = duration;
        }

        public override void Update(float dt)
        {
            if (Dead)
            {
                return;
            }

            UpdateTimers(dt);
            UpdateMovement(dt);
            UpdateWeapons(dt);
            base.Update(dt);

            foreach (Wingman wingman in _wingmen)
            {
                wingman.Update(dt);
            }
        }

        private void UpdateTimers(float dt)
        {
            Invulnerable = Math.Max(0f, Invulnerable - dt);
            DoubleScoreTimer = Math.Max(0f, DoubleScoreTimer - dt);
            DashCooldown = Math.Max(0f, DashCooldown - dt);
            _dashTime = Math.Max(0f, _dashTime - dt);
            _fireCooldown = Math.Max(0f, _fireCooldown - dt);

            if (WeaponTimer > 0f)
            {
                WeaponTimer -= dt;
                if (WeaponTimer <= 0f)
                {
                    Weapon = WeaponMode.Standard;
                }
            }

            RunState run = World.Run;
            if (run.ShieldCharge < run.MaxShield)
            {
                _shieldTimer += dt;
                if (_shieldTimer >= ShieldRechargeDelay)
                {
                    _shieldTimer = 0f;
                    run.ShieldCharge++;
                    World.Float("SHIELD RECHARGED", Position, Color.DeepSkyBlue);
                }
            }
        }

        private void UpdateMovement(float dt)
        {
            RunState run = World.Run;

            if (Controls.TurnLeft)
            {
                Rotation -= TurnSpeed * dt;
            }
            if (Controls.TurnRight)
            {
                Rotation += TurnSpeed * dt;
            }
            if (Controls.Thrust)
            {
                Velocity += Forward * run.Acceleration * dt;
                _trailTimer -= dt;
                if (_trailTimer <= 0f)
                {
                    _trailTimer = 0.02f;
                    Vector2 exhaust = Position - Forward * 24f;
                    Vector2 spray = -Forward * 170f + Util.FromAngle(Rng.Angle()) * 45f;
                    World.Particles.Add(new Particle(World, exhaust, spray, Color.OrangeRed, 20f, 0.35f));
                }
            }
            if (Controls.Brake)
            {
                Velocity *= MathF.Pow(0.05f, dt);
            }

            Velocity *= MathF.Pow(World.Level.Drag, dt);

            if (Controls.Dash && DashCooldown <= 0f)
            {
                _dashTime = DashDuration;
                DashCooldown = run.DashCooldown;
                Velocity = Forward * DashSpeed;
                Sfx.Play("dash", 0.6f);
                World.Burst(Position, Color.Turquoise, 10, 260f);
            }

            if (!IsDashing && Velocity.Length() > run.MaxSpeed)
            {
                Velocity = Vector2.Normalize(Velocity) * run.MaxSpeed;
            }
        }

        private void UpdateWeapons(float dt)
        {
            if (Controls.Fire && _fireCooldown <= 0f)
            {
                Shoot();
            }
            if (Controls.Bomb && World.Run.Bombs > 0)
            {
                World.Run.Bombs--;
                World.DetonateBomb();
            }
        }

        private void Shoot()
        {
            RunState run = World.Run;
            int count = run.ShotCount;
            Vector2 side = new Vector2(-Forward.Y, Forward.X);

            _fireCooldown = Weapon == WeaponMode.Rapid ? run.FireInterval * 0.55f : run.FireInterval;
            Sfx.Play("laser", 0.28f, Rng.Range(-0.05f, 0.05f));

            for (int i = 0; i < count; i++)
            {
                Vector2 muzzle = Position + Forward * 26f + side * ((i - (count - 1) / 2f) * 16f);
                FireBolt(muzzle, Rotation);
                if (Weapon == WeaponMode.Spread)
                {
                    FireBolt(muzzle, Rotation - 0.24f);
                    FireBolt(muzzle, Rotation + 0.24f);
                }
            }
        }

        private void FireBolt(Vector2 from, float angle)
        {
            RunState run = World.Run;
            Bullet bolt = new Bullet(World, from, Util.FromAngle(angle) * ShotSpeed + Velocity * 0.35f, ColorForWeapon(), run.Damage, 1.05f);
            bolt.Piercing = Weapon == WeaponMode.Pierce;
            bolt.Homing = Weapon == WeaponMode.Homing;
            World.PlayerShots.Add(bolt);
        }

        private Color ColorForWeapon()
        {
            switch (Weapon)
            {
                case WeaponMode.Spread: return Color.Violet;
                case WeaponMode.Rapid: return Color.Yellow;
                case WeaponMode.Pierce: return Color.Cyan;
                case WeaponMode.Homing: return Color.HotPink;
                default: return Color.Aquamarine;
            }
        }

        /// <summary>Applies one point of damage: shield first, then hull, then the next reserve ship.</summary>
        public void TakeHit()
        {
            if (Dead || Invulnerable > 0f || IsDashing)
            {
                return;
            }

            RunState run = World.Run;
            _shieldTimer = 0f;
            World.BreakCombo();

            if (run.ShieldCharge > 0)
            {
                run.ShieldCharge--;
                Invulnerable = 0.9f;
                Sfx.Play("shield_hit", 0.8f);
                World.Burst(Position, Color.DeepSkyBlue, 14, 240f);
                World.Shake(5f);
                return;
            }

            run.Hull--;
            Invulnerable = 2f;
            Sfx.Play("invi", 0.5f);
            World.Burst(Position, Color.Red, 18, 280f);
            World.Shake(10f);

            if (run.Hull <= 0)
            {
                LoseShip();
            }
        }

        private void LoseShip()
        {
            RunState run = World.Run;
            Sfx.Play("explosion", 1f, -0.4f);
            World.Burst(Position, Color.Turquoise, 60, 480f);
            World.Shake(18f);
            World.EnemyShots.Clear();

            if (run.Lives > 0)
            {
                run.Lives--;
                run.Hull = run.MaxHull;
                Invulnerable = 3.2f;
                World.Float("RESERVE SHIP", Position, Color.Turquoise);
            }
            else
            {
                Dead = true;
                World.OnPlayerDestroyed();
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            if (Dead)
            {
                return;
            }

            foreach (Wingman wingman in _wingmen)
            {
                wingman.Draw(batch);
            }

            bool blinkedOff = Invulnerable > 0f && (int)(Invulnerable * 12f) % 2 == 0;
            if (!blinkedOff)
            {
                base.Draw(batch);
            }

            if (World.Run.ShieldCharge > 0)
            {
                Ui.Ring(batch, Position, 40f, Color.DeepSkyBlue * 0.55f);
            }
        }
    }

    /// <summary>Escort drone bought in the hangar. Orbits the player and fires at the nearest target.</summary>
    public class Wingman : Entity
    {
        private const float OrbitRadius = 62f;

        private readonly Player _owner;
        private float _orbit;
        private float _cooldown;

        public Wingman(World world, Player owner, int index) : base(world)
        {
            SetTexture("wingman");
            _owner = owner;
            _orbit = index * MathHelper.Pi;
            Scale = 0.42f;
            Tint = Color.Aquamarine;
        }

        public override void Update(float dt)
        {
            _orbit += 2.1f * dt;
            Position = _owner.Position + Util.FromAngle(_orbit) * OrbitRadius;
            _cooldown -= dt;

            Entity target = World.NearestTarget(Position, 620f);
            if (target != null)
            {
                Vector2 aim = Util.Direction(Position, target.Position);
                Rotation = Util.AngleOf(aim);
                if (_cooldown <= 0f)
                {
                    _cooldown = 0.55f;
                    World.PlayerShots.Add(new Bullet(World, Position, aim * 820f, Color.Aquamarine, World.Run.Damage * 0.6f, 0.9f, 0.45f));
                }
            }
        }
    }
}
