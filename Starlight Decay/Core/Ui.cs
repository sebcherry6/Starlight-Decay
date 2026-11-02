namespace Starlight_Decay
{
    /// <summary>Text and simple shape drawing helpers shared by every screen.</summary>
    public static class Ui
    {
        public static void Text(SpriteBatch batch, SpriteFont font, string text, Vector2 position, Color color, float scale = 1f)
        {
            batch.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        public static void Centered(SpriteBatch batch, SpriteFont font, string text, float centerX, float y, Color color, float scale = 1f)
        {
            float width = font.MeasureString(text).X * scale;
            Text(batch, font, text, new Vector2(centerX - width / 2f, y), color, scale);
        }

        public static void RightAligned(SpriteBatch batch, SpriteFont font, string text, float rightX, float y, Color color, float scale = 1f)
        {
            float width = font.MeasureString(text).X * scale;
            Text(batch, font, text, new Vector2(rightX - width, y), color, scale);
        }

        /// <summary>Breaks text into lines no wider than maxWidth.</summary>
        public static List<string> Wrap(SpriteFont font, string text, float maxWidth, float scale = 1f)
        {
            List<string> lines = new List<string>();
            string current = "";
            foreach (string word in text.Split(' '))
            {
                string candidate = current.Length == 0 ? word : current + " " + word;
                if (font.MeasureString(candidate).X * scale > maxWidth && current.Length > 0)
                {
                    lines.Add(current);
                    current = word;
                }
                else
                {
                    current = candidate;
                }
            }
            if (current.Length > 0)
            {
                lines.Add(current);
            }
            return lines;
        }

        public static void Fill(SpriteBatch batch, Rectangle area, Color color)
        {
            batch.Draw(Assets.Pixel, area, color);
        }

        public static void Outline(SpriteBatch batch, Rectangle area, Color color, int thickness = 2)
        {
            Fill(batch, new Rectangle(area.X, area.Y, area.Width, thickness), color);
            Fill(batch, new Rectangle(area.X, area.Bottom - thickness, area.Width, thickness), color);
            Fill(batch, new Rectangle(area.X, area.Y, thickness, area.Height), color);
            Fill(batch, new Rectangle(area.Right - thickness, area.Y, thickness, area.Height), color);
        }

        public static void Line(SpriteBatch batch, Vector2 from, Vector2 to, Color color, float thickness = 2f)
        {
            Vector2 difference = to - from;
            float rotation = Util.AngleOf(difference);
            Vector2 size = new Vector2(difference.Length(), thickness);
            batch.Draw(Assets.Pixel, from, null, color, rotation, new Vector2(0f, 0.5f), size, SpriteEffects.None, 0f);
        }

        /// <summary>Draws the ring texture so that it reaches the given radius.</summary>
        public static void Ring(SpriteBatch batch, Vector2 center, float radius, Color color)
        {
            Texture2D ring = Assets.Tex("ring");
            Vector2 origin = new Vector2(ring.Width / 2f, ring.Height / 2f);
            float scale = radius * 2f / ring.Width;
            batch.Draw(ring, center, null, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
