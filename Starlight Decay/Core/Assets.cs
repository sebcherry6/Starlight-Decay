using Microsoft.Xna.Framework.Content;

namespace Starlight_Decay
{
    /// <summary>Every texture and font, loaded once at startup and looked up by content name.</summary>
    public static class Assets
    {
        private static readonly string[] TextureNames =
        {
            "SpriteRocks", "ship", "wingman", "drone", "gunner", "swarmer", "mine", "lancer", "bulwark",
            "sniper", "carrier", "phantom", "turret",
            "boss1", "boss2", "boss3", "boss4", "boss5", "boss6", "boss7", "boss8", "boss9", "boss10",
            "pick_scrap", "pick_repair", "pick_shield", "pick_bomb", "pick_spread", "pick_rapid",
            "pick_pierce", "pick_homing", "pick_double", "pick_shard",
            "up_thrust", "up_cannon", "up_firerate", "up_multi", "up_magnet", "up_dash", "up_ship",
            "bolt", "glow", "ring", "swirl", "vision", "nebula1", "nebula2", "nebula3"
        };

        private static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

        public static SpriteFont Title;
        public static SpriteFont Main;
        public static SpriteFont Hud;
        public static Texture2D Pixel;

        public static void Load(ContentManager content, GraphicsDevice device)
        {
            foreach (string name in TextureNames)
            {
                Textures[name] = content.Load<Texture2D>(name);
            }

            Title = content.Load<SpriteFont>("TFont");
            Main = content.Load<SpriteFont>("MainFont");
            Hud = content.Load<SpriteFont>("hudFont");

            Pixel = new Texture2D(device, 1, 1);
            Pixel.SetData(new[] { Color.White });
        }

        public static Texture2D Tex(string name)
        {
            return Textures[name];
        }
    }
}
