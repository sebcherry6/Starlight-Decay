namespace Starlight_Decay
{
    public enum Phase
    {
        Intro, Waves, BossWarning, BossFight, Clear
    }

    /// <summary>
    /// One sector in play: every entity, the collision pass, and the director that moves the sector
    /// from its intro through the waves, the boss warning, the boss and the clear.
    /// </summary>
    public class World
    {
        private const float IntroTime = 3f;
        private const float WarningTime = 3.2f;
        private const float ClearTime = 3.5f;
        private const float SpawnGap = 0.32f;
        private const float WaveGap = 1.2f;
        private const float ComboWindow = 3f;
        private const int MaxCombo = 8;
        private const int MaxParticles = 1400;

        public readonly RunState Run;
        public readonly LevelData Level;
        public readonly BossData BossData;
        public Player Player;
        public Boss Boss;

        public readonly List<Rock> Rocks = new List<Rock>();
        public readonly List<Enemy> Enemies = new List<Enemy>();
        public readonly List<Bullet> PlayerShots = new List<Bullet>();
        public readonly List<Bullet> EnemyShots = new List<Bullet>();
        public readonly List<Pickup> Pickups = new List<Pickup>();
        public readonly List<Particle> Particles = new List<Particle>();
        public readonly List<Hazard> Hazards = new List<Hazard>();

        private readonly List<FloatingText> _texts = new List<FloatingText>();
        private readonly List<EnemyKind> _spawnQueue = new List<EnemyKind>();
        private readonly Starfield _stars;
        private readonly Texture2D _nebula;
        private float _phaseTimer;
        private float _spawnTimer;
        private float _ionTimer;
        private float _beamTimer;
        private float _shake;
        private float _timeScale = 1f;
        private float _comboTimer;
        private bool _bombFlash;
        private bool _shardDroppedThisWave;

        public Phase Phase { get; private set; } = Phase.Intro;
        public int WaveIndex { get; private set; }
        public int Combo { get; private set; } = 1;
        public bool IsFinished { get; private set; }
        public bool PlayerLost { get; private set; }

        public float EnemyHealthScale => 1f + 0.14f * (Run.Sector - 1);
        public float ShotSpeedScale => 0.78f + 0.05f * (Run.Sector - 1);
        public int TotalShards => TotalShardsInGame();
        public bool CollectAll => Phase == Phase.Clear;
        public float PhaseTimer => _phaseTimer;

        public World(RunState run)
        {
            Run = run;
            Level = Levels.All[run.Sector - 1];
            BossData = Bosses.All[run.Sector - 1];
            _stars = new Starfield(new Vector2(-14f, 6f));
            _nebula = Assets.Tex(Level.Nebula);
            _phaseTimer = IntroTime;
            _ionTimer = Level.IonInterval;
            _beamTimer = Level.BeamInterval;

            Player = new Player(this, new Vector2(Game1.ArenaWidth / 2f, Game1.ArenaHeight / 2f));
            AddLevelHazards();
            Sfx.Play("start", 0.6f);
            Sfx.PlaySectorMusic();
        }

        private static int TotalShardsInGame()
        {
            int total = 0;
            foreach (LevelData level in Levels.All)
            {
                total += level.Waves.Length;
            }
            return total;
        }

        private void AddLevelHazards()
        {
            for (int i = 0; i < Level.GravityWells; i++)
            {
                Hazards.Add(new GravityWell(this, RandomFarPoint()));
            }
        }

        // ------------------------------------------------------------------ update

        public void Update(float dt)
        {
            float scaled = dt * _timeScale;
            _shake = Math.Max(0f, _shake - 40f * dt);
            _bombFlash = false;
            _stars.Update(dt);
            UpdateCombo(dt);

            Player.Update(scaled);
            UpdateAll(Rocks, scaled);
            UpdateAll(Enemies, scaled);
            UpdateAll(PlayerShots, scaled);
            UpdateAll(EnemyShots, scaled);
            UpdateAll(Pickups, scaled);
            UpdateAll(Particles, scaled);
            UpdateAll(Hazards, scaled);
            if (Boss != null)
            {
                Boss.Update(scaled);
            }
            foreach (FloatingText text in _texts)
            {
                text.Update(dt);
            }

            ResolveCollisions();
            UpdateDirector(dt);
            RemoveDead();
        }

        // Index loops, because updating an entity can add new ones to the same list.
        private static void UpdateAll<T>(List<T> entities, float dt) where T : Entity
        {
            for (int i = 0; i < entities.Count; i++)
            {
                entities[i].Update(dt);
            }
        }

        private void RemoveDead()
        {
            Rocks.RemoveAll(entity => entity.Dead);
            Enemies.RemoveAll(entity => entity.Dead);
            PlayerShots.RemoveAll(entity => entity.Dead);
            EnemyShots.RemoveAll(entity => entity.Dead);
            Pickups.RemoveAll(entity => entity.Dead);
            Particles.RemoveAll(entity => entity.Dead);
            Hazards.RemoveAll(entity => entity.Dead);
            _texts.RemoveAll(text => text.Dead);

            if (Boss != null && Boss.Dead)
            {
                Boss = null;
            }
        }

        private void UpdateCombo(float dt)
        {
            if (_comboTimer > 0f)
            {
                _comboTimer -= dt;
                if (_comboTimer <= 0f)
                {
                    Combo = 1;
                }
            }
        }

        // ------------------------------------------------------------------ director

        private void UpdateDirector(float dt)
        {
            _phaseTimer -= dt;
            UpdateHazardTimers(dt);

            switch (Phase)
            {
                case Phase.Intro:
                    if (_phaseTimer <= 0f)
                    {
                        StartWave(0);
                    }
                    break;
                case Phase.Waves:
                    UpdateWaves(dt);
                    break;
                case Phase.BossWarning:
                    if (_phaseTimer <= 0f)
                    {
                        Boss = new Boss(this, BossData);
                        Phase = Phase.BossFight;
                        Sfx.PlayBossMusic();
                    }
                    break;
                case Phase.BossFight:
                    break;
                case Phase.Clear:
                    if (_phaseTimer <= 0f)
                    {
                        IsFinished = true;
                    }
                    break;
            }

            if (PlayerLost && _phaseTimer <= 0f)
            {
                IsFinished = true;
            }
        }

        private void UpdateHazardTimers(float dt)
        {
            if (Phase != Phase.Waves && Phase != Phase.BossFight)
            {
                return;
            }

            if (Level.IonInterval > 0f)
            {
                _ionTimer -= dt;
                if (_ionTimer <= 0f)
                {
                    _ionTimer = Level.IonInterval;
                    Vector2 near = Player.Position + Util.FromAngle(Rng.Angle()) * Rng.Range(0f, 220f);
                    Hazards.Add(new IonStrike(this, near));
                }
            }

            if (Level.BeamInterval > 0f)
            {
                _beamTimer -= dt;
                if (_beamTimer <= 0f)
                {
                    _beamTimer = Level.BeamInterval;
                    Hazards.Add(new SweepBeam(this, Rng.Chance(0.5f), Rng.Chance(0.5f)));
                    Sfx.Play("beam", 0.5f);
                }
            }
        }

        private void StartWave(int index)
        {
            Phase = Phase.Waves;
            WaveIndex = index;
            _shardDroppedThisWave = false;
            Wave wave = Level.Waves[index];

            for (int i = 0; i < wave.Rocks; i++)
            {
                Rocks.Add(new Rock(this, Level.RockKind, wave.RockSize - Rng.Int(2), RandomEdgePoint(), 120f));
            }

            _spawnQueue.Clear();
            _spawnQueue.AddRange(wave.Enemies);
            _spawnTimer = WaveGap;
        }

        private void UpdateWaves(float dt)
        {
            if (_spawnQueue.Count > 0)
            {
                _spawnTimer -= dt;
                if (_spawnTimer <= 0f)
                {
                    _spawnTimer = SpawnGap;
                    int last = _spawnQueue.Count - 1;
                    Enemies.Add(Enemy.Create(this, _spawnQueue[last], RandomEdgePoint()));
                    _spawnQueue.RemoveAt(last);
                }
                return;
            }

            // A few leftover rocks don't hold up the next wave. Chasing the last tiny one isn't fun.
            if (Enemies.Count > 0 || Rocks.Count > 3)
            {
                return;
            }

            if (!_shardDroppedThisWave)
            {
                _shardDroppedThisWave = true;
                _phaseTimer = WaveGap;
                Pickups.Add(new Pickup(this, PickupKind.Shard, RandomFarPoint()));
                return;
            }

            if (_phaseTimer > 0f)
            {
                return;
            }

            if (WaveIndex + 1 < Level.Waves.Length)
            {
                StartWave(WaveIndex + 1);
            }
            else
            {
                StartBossWarning();
            }
        }

        private void StartBossWarning()
        {
            Phase = Phase.BossWarning;
            _phaseTimer = WarningTime;
            foreach (Rock rock in Rocks.ToArray())
            {
                rock.Shatter(false);
            }
            Sfx.StopMusic();
            Sfx.Play("warning", 0.8f);
        }

        public void BeginBossDeath()
        {
            _timeScale = 0.45f;
            EnemyShots.Clear();
            foreach (Enemy enemy in Enemies.ToArray())
            {
                enemy.Die();
            }
            foreach (Hazard hazard in Hazards.ToArray())
            {
                if (hazard is SweepBeam || hazard is IonStrike)
                {
                    hazard.Dead = true;
                }
            }
            Sfx.StopMusic();
        }

        public void OnBossDestroyed(Boss boss)
        {
            _timeScale = 1f;
            AddScore(2500 * Run.Sector, boss.Position);
            DropLoot(boss.Position, 16 + Run.Sector * 3, 0f);
            Phase = Phase.Clear;
            _phaseTimer = ClearTime;
            Sfx.Play("sector_clear", 0.8f);
        }

        public void OnPlayerDestroyed()
        {
            PlayerLost = true;
            _phaseTimer = 2.5f;
            Sfx.StopMusic();
        }

        public void SkipToBoss()
        {
            _spawnQueue.Clear();
            foreach (Enemy enemy in Enemies)
            {
                enemy.Dead = true;
            }
            WaveIndex = Level.Waves.Length - 1;
            if (Phase == Phase.Waves || Phase == Phase.Intro)
            {
                StartBossWarning();
            }
        }

        // ------------------------------------------------------------------ collisions

        private void ResolveCollisions()
        {
            ResolvePlayerShots();
            if (Player.Dead)
            {
                return;
            }

            for (int i = EnemyShots.Count - 1; i >= 0 && i < EnemyShots.Count; i--)
            {
                Bullet shot = EnemyShots[i];
                if (!shot.Dead && shot.Touches(Player))
                {
                    shot.Dead = true;
                    Player.TakeHit();
                }
            }

            for (int i = 0; i < Rocks.Count; i++)
            {
                Rock rock = Rocks[i];
                if (!rock.Dead && Player.Invulnerable <= 0f && rock.Touches(Player))
                {
                    Player.TakeHit();
                    rock.Shatter(true);
                }
            }

            for (int i = 0; i < Enemies.Count; i++)
            {
                Enemy enemy = Enemies[i];
                if (!enemy.Dead && !enemy.IsWarpingIn && Player.Invulnerable <= 0f && enemy.Touches(Player))
                {
                    Player.TakeHit();
                    Player.Velocity = Util.Direction(enemy.Position, Player.Position) * 460f;
                    enemy.Hit(2f, Player.Position);
                }
            }

            if (Boss != null && Boss.IsTargetable && Boss.Touches(Player))
            {
                Player.TakeHit();
                Player.Velocity = Util.Direction(Boss.Position, Player.Position) * 620f;
            }

            for (int i = 0; i < Pickups.Count; i++)
            {
                Pickup pickup = Pickups[i];
                if (!pickup.Dead && pickup.Touches(Player))
                {
                    pickup.Collect();
                }
            }
        }

        private void ResolvePlayerShots()
        {
            for (int s = 0; s < PlayerShots.Count; s++)
            {
                Bullet shot = PlayerShots[s];
                if (!shot.Dead)
                {
                    HitRocks(shot);
                }
                if (!shot.Dead)
                {
                    HitEnemies(shot);
                }
                if (!shot.Dead && Boss != null && shot.Touches(Boss) && Boss.IsTargetable && shot.FirstHitOn(Boss))
                {
                    Boss.Hit(shot.Damage, shot.Position);
                    shot.Dead = !shot.Piercing;
                }
            }
        }

        private void HitRocks(Bullet shot)
        {
            for (int i = 0; i < Rocks.Count; i++)
            {
                Rock rock = Rocks[i];
                if (!rock.Dead && shot.Touches(rock) && shot.FirstHitOn(rock))
                {
                    rock.Hit(shot.Damage);
                    if (!shot.Piercing)
                    {
                        shot.Dead = true;
                        return;
                    }
                }
            }
        }

        private void HitEnemies(Bullet shot)
        {
            for (int i = 0; i < Enemies.Count; i++)
            {
                Enemy enemy = Enemies[i];
                if (!enemy.Dead && !enemy.IsWarpingIn && shot.Touches(enemy) && shot.FirstHitOn(enemy))
                {
                    enemy.Hit(shot.Damage, shot.Position);
                    if (!shot.Piercing)
                    {
                        shot.Dead = true;
                        return;
                    }
                }
            }
        }

        // ------------------------------------------------------------------ services for entities

        public void AddScore(int points, Vector2 position)
        {
            int total = points * (Player.DoubleScoreTimer > 0f ? 2 : 1);
            Run.Score += total;
        }

        public void AddKillScore(int points, Vector2 position)
        {
            AddScore(points * Combo, position);
            Float((points * Combo).ToString(), position, Color.White * 0.8f);
            Combo = Math.Min(Combo + 1, MaxCombo);
            _comboTimer = ComboWindow;
        }

        public void BreakCombo()
        {
            Combo = 1;
            _comboTimer = 0f;
        }

        public void DropLoot(Vector2 position, int scrap, float powerUpChance)
        {
            for (int i = 0; i < scrap; i++)
            {
                Pickups.Add(new Pickup(this, PickupKind.Scrap, position));
            }
            if (Rng.Chance(powerUpChance))
            {
                Pickups.Add(new Pickup(this, RandomPowerUp(), position));
            }
        }

        public void DropBossSupply(Vector2 position)
        {
            Pickups.Add(new Pickup(this, PickupKind.Repair, position));
            Pickups.Add(new Pickup(this, RandomWeaponPickup(), position));
            DropLoot(position, 6, 0f);
        }

        private PickupKind RandomPowerUp()
        {
            int roll = Rng.Int(10);
            if (roll < 2)
            {
                return PickupKind.Repair;
            }
            if (roll < 3)
            {
                return PickupKind.ShieldCell;
            }
            if (roll < 4)
            {
                return PickupKind.Bomb;
            }
            if (roll < 5)
            {
                return PickupKind.DoubleScore;
            }
            return RandomWeaponPickup();
        }

        private static PickupKind RandomWeaponPickup()
        {
            PickupKind[] weapons = { PickupKind.Spread, PickupKind.Rapid, PickupKind.Pierce, PickupKind.Homing };
            return weapons[Rng.Int(weapons.Length)];
        }

        public void Float(string text, Vector2 position, Color color)
        {
            _texts.Add(new FloatingText(text, position, color));
        }

        public void Shake(float amount)
        {
            _shake = Math.Min(24f, Math.Max(_shake, amount));
        }

        public void Burst(Vector2 position, Color color, int count, float speed)
        {
            for (int i = 0; i < count && Particles.Count < MaxParticles; i++)
            {
                Vector2 velocity = Util.FromAngle(Rng.Angle()) * Rng.Range(speed * 0.2f, speed);
                Particles.Add(new Particle(this, position, velocity, color, Rng.Range(8f, 24f), Rng.Range(0.35f, 0.9f)));
            }
        }

        public void DetonateBomb()
        {
            Sfx.Play("bomb", 1f);
            Shake(20f);
            _bombFlash = true;
            EnemyShots.Clear();
            Burst(Player.Position, Color.Orange, 70, 760f);
            float damage = 8f + Run.Damage * 4f;

            for (int i = 0; i < Enemies.Count; i++)
            {
                Enemy enemy = Enemies[i];
                if (!enemy.Dead && !enemy.IsWarpingIn)
                {
                    enemy.Hit(damage, enemy.Position + Util.FromAngle(enemy.Rotation + MathHelper.Pi));
                }
            }
            for (int i = 0; i < Rocks.Count; i++)
            {
                if (!Rocks[i].Dead)
                {
                    Rocks[i].Hit(damage);
                }
            }
            if (Boss != null)
            {
                Boss.Hit(damage * 2f, Boss.Position);
            }
        }

        public Entity NearestTarget(Vector2 from, float range)
        {
            Entity best = null;
            float bestDistance = range * range;

            foreach (Enemy enemy in Enemies)
            {
                float distance = Vector2.DistanceSquared(from, enemy.Position);
                if (!enemy.IsWarpingIn && distance < bestDistance)
                {
                    best = enemy;
                    bestDistance = distance;
                }
            }
            if (Boss != null && Boss.IsTargetable && !Boss.IsShielded)
            {
                float distance = Vector2.DistanceSquared(from, Boss.Position);
                if (distance < bestDistance)
                {
                    best = Boss;
                }
            }
            return best;
        }

        public int CountEnemies<T>() where T : Enemy
        {
            int count = 0;
            foreach (Enemy enemy in Enemies)
            {
                if (enemy is T)
                {
                    count++;
                }
            }
            return count;
        }

        /// <summary>A point just inside the arena edge, away from the player when possible.</summary>
        public Vector2 RandomEdgePoint()
        {
            Vector2 point = Vector2.Zero;
            for (int attempt = 0; attempt < 8; attempt++)
            {
                switch (Rng.Int(4))
                {
                    case 0:
                        point = new Vector2(Rng.Range(60f, Game1.ArenaWidth - 60f), 60f);
                        break;
                    case 1:
                        point = new Vector2(Rng.Range(60f, Game1.ArenaWidth - 60f), Game1.ArenaHeight - 60f);
                        break;
                    case 2:
                        point = new Vector2(60f, Rng.Range(60f, Game1.ArenaHeight - 60f));
                        break;
                    default:
                        point = new Vector2(Game1.ArenaWidth - 60f, Rng.Range(60f, Game1.ArenaHeight - 60f));
                        break;
                }
                if (Vector2.Distance(point, Player.Position) > 340f)
                {
                    break;
                }
            }
            return point;
        }

        public Vector2 RandomInnerPoint()
        {
            return new Vector2(Rng.Range(220f, Game1.ArenaWidth - 220f), Rng.Range(180f, Game1.ArenaHeight - 180f));
        }

        public Vector2 RandomFarPoint()
        {
            Vector2 point = RandomInnerPoint();
            for (int attempt = 0; attempt < 8 && Vector2.Distance(point, Player.Position) < 380f; attempt++)
            {
                point = RandomInnerPoint();
            }
            return point;
        }

        public Vector2 RandomBossPoint(float radius)
        {
            Vector2 point = RandomFarPoint();
            return Util.ClampToArena(point, radius);
        }

        // ------------------------------------------------------------------ drawing

        public void Draw(SpriteBatch batch)
        {
            Matrix camera = CameraShake();

            batch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp);
            batch.Draw(_nebula, new Rectangle(0, 0, Game1.ArenaWidth, Game1.ArenaHeight), Level.NebulaTint);
            _stars.Draw(batch, Level.StarTint);
            batch.End();

            batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null, null, camera);
            if (!Level.Dark)
            {
                DrawAll(Hazards, batch);
            }
            DrawAll(Pickups, batch);
            DrawAll(Rocks, batch);
            DrawAll(Enemies, batch);
            if (Boss != null)
            {
                Boss.Draw(batch);
            }
            Player.Draw(batch);
            if (Level.Dark && !Player.Dead)
            {
                DrawDarkness(batch);
            }
            if (Level.Dark)
            {
                // Hazards stay visible in the dark, so you can always see what can hurt you.
                DrawAll(Hazards, batch);
            }
            batch.End();

            batch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, null, null, null, camera);
            DrawAll(PlayerShots, batch);
            DrawAll(EnemyShots, batch);
            DrawAll(Particles, batch);
            batch.End();

            batch.Begin();
            foreach (FloatingText text in _texts)
            {
                text.Draw(batch);
            }
            if (_bombFlash)
            {
                Ui.Fill(batch, new Rectangle(0, 0, Game1.ArenaWidth, Game1.ArenaHeight), Color.White * 0.5f);
            }
            batch.End();
        }

        private static void DrawAll<T>(List<T> entities, SpriteBatch batch) where T : Entity
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (!entities[i].Dead)
                {
                    entities[i].Draw(batch);
                }
            }
        }

        private void DrawDarkness(SpriteBatch batch)
        {
            const int Size = 1100;
            Texture2D vision = Assets.Tex("vision");
            int left = (int)Player.Position.X - Size / 2;
            int top = (int)Player.Position.Y - Size / 2;
            int margin = 400;
            int width = Game1.ArenaWidth + margin * 2;
            int height = Game1.ArenaHeight + margin * 2;

            batch.Draw(vision, new Rectangle(left, top, Size, Size), Color.White);
            Ui.Fill(batch, new Rectangle(-margin, -margin, width, top + margin), Color.Black);
            Ui.Fill(batch, new Rectangle(-margin, top + Size, width, height), Color.Black);
            Ui.Fill(batch, new Rectangle(-margin, top, left + margin, Size), Color.Black);
            Ui.Fill(batch, new Rectangle(left + Size, top, width, Size), Color.Black);
        }

        private Matrix CameraShake()
        {
            if (_shake <= 0f)
            {
                return Matrix.Identity;
            }
            return Matrix.CreateTranslation(Rng.Range(-_shake, _shake), Rng.Range(-_shake, _shake), 0f);
        }
    }
}
