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
    public class ParticleShot : Particles
    {
        public ParticleShot(Game1 gameRoot, Vector2 position, Vector2 velocity) : base(
            gameRoot,
            position,
            Color.Silver,
            0.0f)
        {
            _velocity = velocity;
            _colliding = true;
            _collisionRadius = (float)_sourceBounds.Width / 2.0f;
        }
        public override void UpdateSprite(GameTime gameTime)
        {
            base.UpdateSprite(gameTime);

            foreach (Rocks eachRock in _gameRoot._rocks)
            {
                if (IsColliding(eachRock))
                {
                    eachRock.Explode();
                    _dead = true;
                }
            }
        }
    }
}
