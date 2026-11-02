using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Starlight_Decay;

namespace Asteroids_2
{
    public class Particles : Sprite
    {
        public Particles(Game1 gameRoot, Vector2 position, Color color, float lifeSpan = 0.0f) : base(gameRoot)
        {
            _gameRoot = gameRoot;
            _color = color;
            _lifeSpan = lifeSpan;
            _position = position;
            _texture = _gameRoot._particleTxr;
            _drawBounds = _texture.Bounds;
            _sourceBounds = _texture.Bounds;
            _origin = _sourceBounds.Size.ToVector2() / 2.0f;
            
        }
        public override void UpdateSprite(GameTime gameTime)
        {
            base.UpdateSprite(gameTime);

            if (_lifeSpan > 0.0f)
            {
                _drawBounds.Size = (_drawBounds.Size.ToVector2() * (float)(1.0f - (_lifeCounter / _lifeSpan))).ToPoint();
            }
            if (_position.X < -32.0f || _position.X > _gameRoot._screenSize.X + 32 
                || _position.Y < -32.0f || _position.Y > _gameRoot._screenSize.Y + 32
                )
            {
                _dead = true;
            }

        }
    }
}
