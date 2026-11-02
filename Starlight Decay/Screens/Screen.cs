namespace Starlight_Decay
{
    public abstract class Screen
    {
        protected readonly Game1 Game;

        protected Screen(Game1 game)
        {
            Game = game;
        }

        public abstract void Update(float dt);

        public abstract void Draw(SpriteBatch batch);
    }

    /// <summary>Starfield and nebula behind the menu screens.</summary>
    public class MenuBackdrop
    {
        private readonly Starfield _stars = new Starfield(new Vector2(-22f, 8f));
        private readonly Texture2D _nebula = Assets.Tex("nebula2");
        private readonly Color _tint;

        public MenuBackdrop(Color tint)
        {
            _tint = tint;
        }

        public void Update(float dt)
        {
            _stars.Update(dt);
        }

        /// <summary>Call outside a batch. Leaves the sprite batch ended.</summary>
        public void Draw(SpriteBatch batch)
        {
            batch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp);
            batch.Draw(_nebula, new Rectangle(0, 0, Game1.ArenaWidth, Game1.ArenaHeight), _tint);
            _stars.Draw(batch, Color.White);
            batch.End();
        }
    }
}
