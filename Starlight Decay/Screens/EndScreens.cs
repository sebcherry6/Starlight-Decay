namespace Starlight_Decay
{
    public class GameOverScreen : Screen
    {
        private readonly RunState _run;
        private readonly MenuBackdrop _backdrop = new MenuBackdrop(new Color(90, 20, 20));

        public GameOverScreen(Game1 game, RunState run) : base(game)
        {
            _run = run;
            Sfx.StopMusic();
        }

        public override void Update(float dt)
        {
            _backdrop.Update(dt);

            if (Controls.Pressed(Keys.Enter))
            {
                Game.Go(new ShopScreen(Game, _run.RetryFromCheckpoint(), true));
            }
            if (Controls.Back)
            {
                Game.Go(NameEntryScreen.AfterRun(Game, _run.Score));
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            _backdrop.Draw(batch);
            batch.Begin();

            float center = Game1.ArenaWidth / 2f;
            Ui.Centered(batch, Assets.Title, "YOU DIED", center, 210f, Color.Red);
            Ui.Centered(batch, Assets.Main, "Sector " + _run.Sector + "     Score " + _run.Score, center, 360f, Color.White);
            Ui.Centered(batch, Assets.Main, "press ENTER to RETRY THIS SECTOR", center, 500f, Color.GreenYellow);
            Ui.Centered(batch, Assets.Hud, "Retrying halves your score. You keep your upgrades and half the scrap you just earned.", center, 550f, Color.LightGray);
            Ui.Centered(batch, Assets.Main, "press ESCAPE to END THE RUN", center, 620f, Color.GreenYellow);

            batch.End();
        }
    }

    public class VictoryScreen : Screen
    {
        private readonly RunState _run;
        private readonly MenuBackdrop _backdrop = new MenuBackdrop(new Color(50, 80, 90));
        private readonly int _shardBonus;
        private float _time;

        public VictoryScreen(Game1 game, RunState run) : base(game)
        {
            _run = run;
            _shardBonus = run.Shards * 2000 + run.Lives * 10000;
            _run.Score += _shardBonus;
            Sfx.StopMusic();
            Sfx.Play("sector_clear", 0.9f);
        }

        public override void Update(float dt)
        {
            _time += dt;
            _backdrop.Update(dt);
            if (_time > 1.5f && Controls.Confirm)
            {
                Game.Go(NameEntryScreen.AfterRun(Game, _run.Score));
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            _backdrop.Draw(batch);
            batch.Begin();

            float center = Game1.ArenaWidth / 2f;
            Ui.Centered(batch, Assets.Title, "THE BELT IS CLEAR", center, 170f, Color.GreenYellow, 0.75f);
            Ui.Centered(batch, Assets.Main, "The Singularity is gone. You can fly home.", center, 300f, Color.White);
            Ui.Centered(batch, Assets.Main, "Kills " + _run.Kills + "     Star Shards " + _run.Shards, center, 400f, Color.Turquoise);
            Ui.Centered(batch, Assets.Main, "Survivor and shard bonus " + _shardBonus, center, 455f, Color.Gold);
            Ui.Centered(batch, Assets.Main, "FINAL SCORE " + _run.Score, center, 530f, Color.White);
            if (_time > 1.5f)
            {
                Ui.Centered(batch, Assets.Main, "press SPACE to continue", center, 680f, Color.GreenYellow);
            }

            batch.End();
        }
    }

    public class NameEntryScreen : Screen
    {
        private const int MaxNameLength = 10;

        private readonly int _score;
        private readonly MenuBackdrop _backdrop = new MenuBackdrop(new Color(30, 70, 60));
        private string _name = "";
        private float _time;

        private NameEntryScreen(Game1 game, int score) : base(game)
        {
            _score = score;
        }

        /// <summary>Asks for a name if the score makes the table, otherwise goes straight back to the title.</summary>
        public static Screen AfterRun(Game1 game, int score)
        {
            if (game.HiScores.Qualifies(score))
            {
                return new NameEntryScreen(game, score);
            }
            return new TitleScreen(game);
        }

        public override void Update(float dt)
        {
            _time += dt;
            _backdrop.Update(dt);

            for (int code = (int)Keys.A; code <= (int)Keys.Z; code++)
            {
                Keys key = (Keys)code;
                if (Controls.Pressed(key) && _name.Length < MaxNameLength)
                {
                    _name += key.ToString();
                }
            }
            if (Controls.Pressed(Keys.Back) && _name.Length > 0)
            {
                _name = _name.Substring(0, _name.Length - 1);
            }
            if (Controls.Pressed(Keys.Enter))
            {
                Game.HiScores.Submit(_name, _score);
                Game.Go(new TitleScreen(Game));
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            _backdrop.Draw(batch);
            batch.Begin();

            float center = Game1.ArenaWidth / 2f;
            Ui.Centered(batch, Assets.Title, "NEW HIGH SCORE", center, 190f, Color.GreenYellow, 0.9f);
            Ui.Centered(batch, Assets.Main, _score.ToString(), center, 320f, Color.Gold);
            string cursor = (int)(_time * 2f) % 2 == 0 ? "_" : " ";
            Ui.Centered(batch, Assets.Main, _name + cursor, center, 420f, Color.Turquoise);
            Ui.Centered(batch, Assets.Hud, "Type your name, then press ENTER", center, 500f, Color.LightGray);

            batch.End();
        }
    }
}
