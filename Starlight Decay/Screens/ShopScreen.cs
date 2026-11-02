namespace Starlight_Decay
{
    /// <summary>The hangar between sectors, where scrap buys permanent upgrades for the run.</summary>
    public class ShopScreen : Screen
    {
        private const int Columns = 4;
        private const int CardWidth = 366;
        private const int CardHeight = 170;
        private const int CardGap = 16;
        private const int GridLeft = 44;
        private const int GridTop = 136;

        private readonly RunState _run;
        private readonly MenuBackdrop _backdrop = new MenuBackdrop(new Color(25, 50, 70));
        private readonly int _launchIndex = Upgrades.All.Length;
        private readonly bool _retrying;
        private readonly int _nextSector;
        private int _selected;

        public ShopScreen(Game1 game, RunState run, bool retrying = false) : base(game)
        {
            _run = run;
            _retrying = retrying;

            // Fixed here because launching changes the run, and this screen still draws
            // for one more frame before the next screen takes over.
            _nextSector = retrying ? run.Sector : run.Sector + 1;
            _selected = _launchIndex;
            Sfx.StopMusic();
        }

        public override void Update(float dt)
        {
            _backdrop.Update(dt);

            if (Controls.MenuLeft)
            {
                Move(-1);
            }
            if (Controls.MenuRight)
            {
                Move(1);
            }
            if (Controls.MenuUp)
            {
                _selected = _selected == _launchIndex ? _launchIndex - 2 : Math.Max(0, _selected - Columns);
            }
            if (Controls.MenuDown)
            {
                _selected = Math.Min(_launchIndex, _selected + Columns);
            }
            if (Controls.Confirm)
            {
                Activate();
            }
        }

        private void Move(int step)
        {
            if (_selected != _launchIndex)
            {
                _selected = Math.Clamp(_selected + step, 0, _launchIndex - 1);
            }
        }

        private void Activate()
        {
            if (_selected == _launchIndex)
            {
                if (!_retrying)
                {
                    _run.AdvanceSector();
                }
                Game.StartSector(_run);
                return;
            }

            bool bought = _run.TryBuy(Upgrades.All[_selected]);
            Sfx.Play(bought ? "buy" : "denied", 0.7f);
        }

        public override void Draw(SpriteBatch batch)
        {
            _backdrop.Draw(batch);
            batch.Begin();

            Ui.Text(batch, Assets.Title, "HANGAR", new Vector2(44f, 22f), Color.Turquoise, 0.75f);
            Ui.RightAligned(batch, Assets.Main, "SCRAP " + _run.Scrap, Game1.ArenaWidth - 44f, 30f, Color.Gold);
            Ui.RightAligned(batch, Assets.Hud,
                "HULL " + _run.Hull + "/" + _run.MaxHull + "    SHIPS " + _run.Lives + "    SHARDS " + _run.Shards,
                Game1.ArenaWidth - 44f, 84f, Color.LightGray);

            for (int i = 0; i < Upgrades.All.Length; i++)
            {
                DrawCard(batch, Upgrades.All[i], i);
            }
            DrawLaunchButton(batch);

            batch.End();
        }

        private void DrawCard(SpriteBatch batch, UpgradeInfo upgrade, int index)
        {
            int x = GridLeft + (index % Columns) * (CardWidth + CardGap);
            int y = GridTop + (index / Columns) * (CardHeight + CardGap);
            Rectangle card = new Rectangle(x, y, CardWidth, CardHeight);
            bool selected = index == _selected;
            bool maxed = _run.IsMaxed(upgrade);
            int cost = _run.Cost(upgrade);
            bool affordable = _run.Scrap >= cost;

            Ui.Fill(batch, card, (selected ? Color.DarkSlateBlue : Color.Black) * 0.75f);
            Ui.Outline(batch, card, selected ? Color.GreenYellow : Color.SlateGray, selected ? 3 : 1);

            Texture2D icon = Assets.Tex(upgrade.Icon);
            batch.Draw(icon, new Rectangle(x + 14, y + 16, 62, 62), Color.Turquoise);
            Ui.Text(batch, Assets.Hud, upgrade.Name, new Vector2(x + 90f, y + 14f), Color.White);

            List<string> lines = Ui.Wrap(Assets.Hud, upgrade.Description, CardWidth - 104f, 0.8f);
            for (int i = 0; i < lines.Count; i++)
            {
                Ui.Text(batch, Assets.Hud, lines[i], new Vector2(x + 90f, y + 46f + i * 23f), Color.LightGray, 0.8f);
            }

            if (upgrade.MaxLevel > 0)
            {
                int level = _run.Level(upgrade.Kind);
                for (int i = 0; i < upgrade.MaxLevel; i++)
                {
                    Rectangle pip = new Rectangle(x + 14 + i * 22, y + CardHeight - 30, 16, 16);
                    if (i < level)
                    {
                        Ui.Fill(batch, pip, Color.GreenYellow);
                    }
                    else
                    {
                        Ui.Outline(batch, pip, Color.SlateGray, 1);
                    }
                }
            }

            string price = maxed ? (upgrade.Kind == UpgradeKind.Repair ? "FULL" : "MAX") : cost.ToString();
            Color priceColor = maxed ? Color.SlateGray : (affordable ? Color.Gold : Color.IndianRed);
            Ui.RightAligned(batch, Assets.Hud, price, x + CardWidth - 14f, y + CardHeight - 34f, priceColor);
        }

        private void DrawLaunchButton(SpriteBatch batch)
        {
            bool selected = _selected == _launchIndex;
            Rectangle button = new Rectangle(GridLeft, 708, Game1.ArenaWidth - GridLeft * 2, 72);
            string label = "LAUNCH TO SECTOR " + _nextSector + "  -  " + Levels.All[_nextSector - 1].Name;

            Ui.Fill(batch, button, (selected ? Color.DarkGreen : Color.Black) * 0.8f);
            Ui.Outline(batch, button, selected ? Color.GreenYellow : Color.SlateGray, selected ? 3 : 1);
            Ui.Centered(batch, Assets.Main, label, Game1.ArenaWidth / 2f, 722f, selected ? Color.GreenYellow : Color.LightGray);

            Ui.Centered(batch, Assets.Hud, "ARROWS or WASD to choose    SPACE to buy or launch", Game1.ArenaWidth / 2f, 810f, Color.LightGray);
        }
    }
}
