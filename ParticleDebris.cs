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
    public class ParticleDebris : Particles
    {
        public ParticleDebris(Game1 gameRoot, Vector2 position) : base(
            gameRoot,
            position,
            Color.DarkOliveGreen,
            ((float)gameRoot._random.NextDouble() * 3.0f) + 2.0f)
        {
            _velocity = new Vector2(
                (float)Math.Cos(_gameRoot._random.NextDouble() * Math.Tau),
                (float)Math.Sin(_gameRoot._random.NextDouble() * Math.Tau)) * _gameRoot._random.Next(0, 600);
        }
    }
}
