namespace Starlight_Decay
{
    public static class Rng
    {
        private static readonly Random Source = new Random();

        public static float Float()
        {
            return (float)Source.NextDouble();
        }

        public static float Range(float min, float max)
        {
            return min + (max - min) * Float();
        }

        public static int Int(int maxExclusive)
        {
            return Source.Next(maxExclusive);
        }

        public static bool Chance(float probability)
        {
            return Float() < probability;
        }

        public static float Angle()
        {
            return Float() * MathHelper.TwoPi;
        }
    }

    public static class Util
    {
        public static Vector2 FromAngle(float angle)
        {
            return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        }

        public static float AngleOf(Vector2 direction)
        {
            return MathF.Atan2(direction.Y, direction.X);
        }

        /// <summary>Unit vector between two points. Safe when the points are the same.</summary>
        public static Vector2 Direction(Vector2 from, Vector2 to)
        {
            Vector2 difference = to - from;
            float length = difference.Length();
            return length > 0.001f ? difference / length : Vector2.UnitX;
        }

        public static float TurnToward(float current, float target, float maxStep)
        {
            float difference = MathHelper.WrapAngle(target - current);
            return current + Math.Clamp(difference, -maxStep, maxStep);
        }

        public static Vector2 ClampToArena(Vector2 position, float margin)
        {
            return new Vector2(
                Math.Clamp(position.X, margin, Game1.ArenaWidth - margin),
                Math.Clamp(position.Y, margin, Game1.ArenaHeight - margin));
        }
    }
}
