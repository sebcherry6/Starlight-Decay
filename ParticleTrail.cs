using ASTEROIDS_2;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using static System.Formats.Asn1.AsnWriter;
using System.Reflection;
using System.Linq;
using Asteroids_2;
using Starlight_Decay;

namespace ASTEROIDS_2
{
    public class ParticleTrail : Particles
    {
        public ParticleTrail(Game1 gameRoot, Vector2 position, Color color) : base(
            gameRoot,
            position,
            color,
            0.5f)
        {
            _color = Color.DarkRed;
        }
        public override void UpdateSprite(GameTime gameTime)
        {
            base.UpdateSprite(gameTime);
        }
    }
}
