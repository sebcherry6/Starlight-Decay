namespace Starlight_Decay
{
    /// <summary>Keyboard state for this frame and the last, plus the game's key bindings.</summary>
    public static class Controls
    {
        private static KeyboardState _current;
        private static KeyboardState _previous;

        public static void Update()
        {
            _previous = _current;
            _current = Keyboard.GetState();
        }

        public static bool Down(Keys key)
        {
            return _current.IsKeyDown(key);
        }

        public static bool Pressed(Keys key)
        {
            return _current.IsKeyDown(key) && !_previous.IsKeyDown(key);
        }

        public static bool TurnLeft => Down(Keys.A) || Down(Keys.Left);
        public static bool TurnRight => Down(Keys.D) || Down(Keys.Right);
        public static bool Thrust => Down(Keys.W) || Down(Keys.Up);
        public static bool Brake => Down(Keys.S) || Down(Keys.Down);
        public static bool Fire => Down(Keys.Space);
        public static bool Dash => Pressed(Keys.LeftShift) || Pressed(Keys.RightShift);
        public static bool Bomb => Pressed(Keys.E) || Pressed(Keys.Q);
        public static bool Confirm => Pressed(Keys.Space) || Pressed(Keys.Enter);
        public static bool Back => Pressed(Keys.Escape);
        public static bool MenuLeft => Pressed(Keys.A) || Pressed(Keys.Left);
        public static bool MenuRight => Pressed(Keys.D) || Pressed(Keys.Right);
        public static bool MenuUp => Pressed(Keys.W) || Pressed(Keys.Up);
        public static bool MenuDown => Pressed(Keys.S) || Pressed(Keys.Down);
    }
}
