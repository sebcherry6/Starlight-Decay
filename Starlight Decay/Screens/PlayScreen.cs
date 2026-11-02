namespace Starlight_Decay
{
    /// <summary>Runs a World, overlays the HUD and banners, and decides what comes after the sector.</summary>
    public class PlayScreen : Screen
    {
        private readonly RunState _run;
        private readonly World _world;
        private bool _paused;
        private float _time;

        public PlayScreen(Game1 game, RunState run) : base(game)
        {
            _run = run;
            _world = new World(run);
        }

        public override void Update(float dt)
        {
            _time += dt;

            if (_paused)
            {
                UpdatePaused();
                return;
            }

            if (Controls.Back)
            {
                _paused = true;
                return;
            }

#if DEBUG
            // Debug builds only: jump straight to this sector's boss.
            if (Controls.Pressed(Keys.F9))
            {
                _world.SkipToBoss();
            }
#endif

            _world.Update(dt);

            if (_world.IsFinished)
            {
                Finish();
            }
        }

        private void UpdatePaused()
        {
            if (Controls.Pressed(Keys.Space))
            {
                _paused = false;
            }
            if (Controls.Back)
            {
                Sfx.StopMusic();
                Game.Go(new TitleScreen(Game));
            }
        }

        private void Finish()
        {
            if (_world.PlayerLost)
            {
                Game.Go(new GameOverScreen(Game, _run));
            }
            else if (_run.Sector >= Levels.All.Length)
            {
                Game.Go(new VictoryScreen(Game, _run));
            }
            else
            {
                Game.Go(new ShopScreen(Game, _run));
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            _world.Draw(batch);

            batch.Begin();
            Hud.Draw(batch, _world);
            DrawBanners(batch);
            if (_paused)
            {
                DrawPaused(batch);
            }
            batch.End();
        }

        private void DrawBanners(SpriteBatch batch)
        {
            float center = Game1.ArenaWidth / 2f;

            switch (_world.Phase)
            {
                case Phase.Intro:
                    Ui.Centered(batch, Assets.Hud, "SECTOR " + _run.Sector + " OF " + Levels.All.Length, center, 280f, Color.Turquoise);
                    Ui.Centered(batch, Assets.Title, _world.Level.Name, center, 320f, Color.White, 0.8f);
                    Ui.Centered(batch, Assets.Hud, _world.Level.Briefing, center, 430f, Color.LightGray);
                    break;
                case Phase.BossWarning:
                    if ((int)(_time * 5f) % 2 == 0)
                    {
                        Ui.Centered(batch, Assets.Title, "WARNING", center, 300f, Color.Red);
                    }
                    Ui.Centered(batch, Assets.Main, _world.BossData.Name + " APPROACHING", center, 410f, Color.OrangeRed);
                    break;
                case Phase.Clear:
                    Ui.Centered(batch, Assets.Title, "SECTOR CLEAR", center, 320f, Color.GreenYellow, 0.85f);
                    Ui.Centered(batch, Assets.Main, "Score " + _run.Score + "     Scrap " + _run.Scrap, center, 430f, Color.White);
                    break;
            }

            if (_world.PlayerLost)
            {
                Ui.Centered(batch, Assets.Title, "SHIP LOST", center, 340f, Color.Red);
            }
        }

        private static void DrawPaused(SpriteBatch batch)
        {
            float center = Game1.ArenaWidth / 2f;
            Ui.Fill(batch, new Rectangle(0, 0, Game1.ArenaWidth, Game1.ArenaHeight), Color.Black * 0.6f);
            Ui.Centered(batch, Assets.Title, "PAUSED", center, 300f, Color.GreenYellow);
            Ui.Centered(batch, Assets.Main, "press SPACE to RESUME", center, 440f, Color.GreenYellow);
            Ui.Centered(batch, Assets.Main, "press ESCAPE to QUIT", center, 500f, Color.GreenYellow);
        }
    }

    /// <summary>In-game readouts.</summary>
    public static class Hud
    {
        public static void Draw(SpriteBatch batch, World world)
        {
            RunState run = world.Run;
            Player player = world.Player;

            Ui.Text(batch, Assets.Main, run.Score.ToString(), new Vector2(24f, 14f), Color.DarkTurquoise);
            if (world.Combo > 1)
            {
                Ui.Text(batch, Assets.Hud, "COMBO x" + world.Combo, new Vector2(24f, 62f), Color.Gold);
            }
            if (player.DoubleScoreTimer > 0f)
            {
                Ui.Text(batch, Assets.Hud, "SCORE x2", new Vector2(24f, 92f), Color.Gold);
            }

            Ui.RightAligned(batch, Assets.Hud, "SHIPS " + run.Lives + "    SCRAP " + run.Scrap, Game1.ArenaWidth - 24f, 20f, Color.DarkTurquoise);

            string progress = world.Boss != null || world.Phase == Phase.BossWarning
                ? "SECTOR " + run.Sector
                : "SECTOR " + run.Sector + "  -  WAVE " + (world.WaveIndex + 1) + "/" + world.Level.Waves.Length;
            if (world.Boss == null)
            {
                Ui.Centered(batch, Assets.Hud, progress, Game1.ArenaWidth / 2f, 20f, Color.LightGray);
            }

            DrawStatusPanel(batch, run, player);

            if (world.Boss != null && !world.Boss.IsDying)
            {
                DrawBossBar(batch, world.Boss);
            }
        }

        private static void DrawStatusPanel(SpriteBatch batch, RunState run, Player player)
        {
            float y = Game1.ArenaHeight - 44f;

            Ui.Text(batch, Assets.Hud, "HULL", new Vector2(24f, y), Color.LightGreen);
            for (int i = 0; i < run.MaxHull; i++)
            {
                Rectangle pip = new Rectangle(90 + i * 26, (int)y + 2, 20, 22);
                if (i < run.Hull)
                {
                    Ui.Fill(batch, pip, Color.LightGreen);
                }
                else
                {
                    Ui.Outline(batch, pip, Color.LightGreen * 0.5f);
                }
            }

            int shieldX = 110 + run.MaxHull * 26;
            if (run.MaxShield > 0 || run.ShieldCharge > 0)
            {
                Ui.Text(batch, Assets.Hud, "SHIELD", new Vector2(shieldX, y), Color.DeepSkyBlue);
                int shieldPips = Math.Max(run.MaxShield, run.ShieldCharge);
                for (int i = 0; i < shieldPips; i++)
                {
                    Rectangle pip = new Rectangle(shieldX + 100 + i * 26, (int)y + 2, 20, 22);
                    if (i < run.ShieldCharge)
                    {
                        Ui.Fill(batch, pip, Color.DeepSkyBlue);
                    }
                    else
                    {
                        Ui.Outline(batch, pip, Color.DeepSkyBlue * 0.5f);
                    }
                }
            }

            string dash = player.DashCooldown > 0f ? "DASH " + (int)MathF.Ceiling(player.DashCooldown) : "DASH READY";
            Ui.RightAligned(batch, Assets.Hud, "BOMBS " + run.Bombs + "    " + dash, Game1.ArenaWidth - 24f, y, Color.Orange);

            if (player.Weapon != WeaponMode.Standard)
            {
                string weapon = player.Weapon.ToString().ToUpperInvariant() + "  " + (int)MathF.Ceiling(player.WeaponTimer);
                Ui.Centered(batch, Assets.Hud, weapon, Game1.ArenaWidth / 2f, y, Color.Violet);
            }
        }

        private static void DrawBossBar(SpriteBatch batch, Boss boss)
        {
            const int Width = 760;
            const int Height = 16;
            int x = (Game1.ArenaWidth - Width) / 2;
            int y = 52;

            Ui.Centered(batch, Assets.Hud, boss.Name, Game1.ArenaWidth / 2f, 18f, Color.White);
            Ui.Fill(batch, new Rectangle(x, y, Width, Height), Color.DarkRed * 0.5f);
            Color fill = boss.IsShielded ? Color.Gray : Color.Red;
            Ui.Fill(batch, new Rectangle(x, y, (int)(Width * boss.HealthFraction), Height), fill);
            Ui.Outline(batch, new Rectangle(x, y, Width, Height), Color.White * 0.7f);

            // Tick marks where the boss changes phase.
            Ui.Fill(batch, new Rectangle(x + Width / 3, y, 2, Height), Color.White);
            Ui.Fill(batch, new Rectangle(x + Width * 2 / 3, y, 2, Height), Color.White);
        }
    }
}
