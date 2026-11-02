namespace Starlight_Decay
{
    /// <summary>
    /// Owns the window, the fixed-size canvas everything is drawn onto, and the active screen.
    /// The game always plays at 1600x900; the canvas is scaled to fit whatever window it is in.
    /// </summary>
    public class Game1 : Game
    {
        public const int ArenaWidth = 1600;
        public const int ArenaHeight = 900;

        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _batch;
        private RenderTarget2D _canvas;
        private Screen _screen;
        private Screen _nextScreen;

        public HiScores HiScores { get; } = new HiScores();

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.HardwareModeSwitch = false;
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            Window.AllowUserResizing = true;
            Window.Title = "Starlight Decay";
        }

        protected override void LoadContent()
        {
            _batch = new SpriteBatch(GraphicsDevice);
            _canvas = new RenderTarget2D(GraphicsDevice, ArenaWidth, ArenaHeight);
            Assets.Load(Content, GraphicsDevice);
            Sfx.Load(Content);
            HiScores.Load();
            Go(new TitleScreen(this));
        }

        /// <summary>Switches screen at the start of the next update, so a screen can safely replace itself.</summary>
        public void Go(Screen screen)
        {
            _nextScreen = screen;
        }

        /// <summary>Begins a sector. Keeps a copy of the run so a retry starts from this exact point.</summary>
        public void StartSector(RunState run)
        {
            run.Checkpoint = run.Clone();
            Go(new PlayScreen(this, run));
        }

        protected override void Update(GameTime gameTime)
        {
            float dt = Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, 1f / 30f);
            Controls.Update();
            Sfx.Tick(dt);

            if (Controls.Pressed(Keys.F11))
            {
                _graphics.ToggleFullScreen();
            }
            if (Controls.Pressed(Keys.M) && !(_screen is NameEntryScreen))
            {
                Sfx.ToggleMute();
            }

            if (_nextScreen != null)
            {
                _screen = _nextScreen;
                _nextScreen = null;
            }
            _screen.Update(dt);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(_canvas);
            GraphicsDevice.Clear(Color.Black);
            _screen.Draw(_batch);

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            _batch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.LinearClamp);
            _batch.Draw(_canvas, LetterboxRectangle(), Color.White);
            _batch.End();

            base.Draw(gameTime);
        }

        private Rectangle LetterboxRectangle()
        {
            int windowWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            int windowHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
            float scale = Math.Min((float)windowWidth / ArenaWidth, (float)windowHeight / ArenaHeight);
            int width = (int)(ArenaWidth * scale);
            int height = (int)(ArenaHeight * scale);
            return new Rectangle((windowWidth - width) / 2, (windowHeight - height) / 2, width, height);
        }
    }
}
