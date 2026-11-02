namespace Starlight_Decay
{
    public class TitleScreen : Screen
    {
        private readonly MenuBackdrop _backdrop = new MenuBackdrop(new Color(30, 70, 60));
        private float _time;

        public TitleScreen(Game1 game) : base(game)
        {
            Sfx.StopMusic();
            Sfx.Play("start", 0.7f);
        }

        public override void Update(float dt)
        {
            _time += dt;
            _backdrop.Update(dt);

            if (Controls.Back)
            {
                Game.Exit();
            }
            if (Controls.Pressed(Keys.Space) || Controls.Pressed(Keys.Enter))
            {
                Game.StartSector(new RunState());
            }
        }

        public override void Draw(SpriteBatch batch)
        {
            _backdrop.Draw(batch);
            batch.Begin();

            float center = Game1.ArenaWidth / 2f;
            Ui.Centered(batch, Assets.Title, "STARLIGHT DECAY", center, 100f, Color.GreenYellow, 1.0f);
            Ui.Centered(batch, Assets.Main, "TEN SECTORS.  TEN BOSSES.  ONE SHIP.", center, 225f, Color.Turquoise);

            if ((int)(_time * 2f) % 2 == 0)
            {
                Ui.Centered(batch, Assets.Main, "press SPACE to START", center, 300f, Color.GreenYellow);
            }

            DrawControls(batch, 200f, 400f);
            DrawScores(batch, 960f, 400f);

            batch.End();
        }

        private static void DrawControls(SpriteBatch batch, float x, float y)
        {
            string[] rows =
            {
                "W / UP", "THRUST", "A D / LEFT RIGHT", "TURN", "S / DOWN", "BRAKE",
                "SPACE", "FIRE", "SHIFT", "DASH", "E or Q", "BOMB",
                "ESC", "PAUSE", "M", "MUTE", "F11", "FULLSCREEN"
            };

            Ui.Text(batch, Assets.Main, "CONTROLS", new Vector2(x, y), Color.Turquoise);
            for (int i = 0; i < rows.Length; i += 2)
            {
                float rowY = y + 56f + i * 21f;
                Ui.Text(batch, Assets.Hud, rows[i], new Vector2(x, rowY), Color.White);
                Ui.Text(batch, Assets.Hud, rows[i + 1], new Vector2(x + 300f, rowY), Color.LightGray);
            }
        }

        private void DrawScores(SpriteBatch batch, float x, float y)
        {
            Ui.Text(batch, Assets.Main, "BEST PILOTS", new Vector2(x, y), Color.Turquoise);
            List<HiScoreEntry> entries = Game.HiScores.Entries;

            if (entries.Count == 0)
            {
                Ui.Text(batch, Assets.Hud, "No scores yet.", new Vector2(x, y + 56f), Color.LightGray);
            }
            for (int i = 0; i < entries.Count; i++)
            {
                float rowY = y + 56f + i * 42f;
                Ui.Text(batch, Assets.Hud, (i + 1) + ".  " + entries[i].Name, new Vector2(x, rowY), Color.White);
                Ui.RightAligned(batch, Assets.Hud, entries[i].Score.ToString(), x + 420f, rowY, Color.GreenYellow);
            }
        }
    }
}
