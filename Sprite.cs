using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using Starlight_Decay;

namespace Asteroids_2
{
    public class Sprite
    {
        protected Game1 _gameRoot;
        protected Texture2D _texture;
        protected Rectangle _drawBounds, _sourceBounds;
        protected Vector2 _position, _velocity, _origin;
        protected Color _color;
        protected float _rotation, _rotationSpeed, _collisionRadius;
        protected double _lifeSpan, _lifeCounter;
        protected bool _wrapping;
        public bool _dead ,_colliding;

        public Sprite(Game1 gameRoot)
        {
            _gameRoot = gameRoot;


        }
        public virtual void UpdateSprite(GameTime gameTime)
        {
            _lifeCounter += gameTime.ElapsedGameTime.TotalSeconds;

            if (_lifeSpan > 0 && _lifeCounter >= _lifeSpan)
            {
                _dead = true;
                return;
            }

            _position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            _rotation += _rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_wrapping)
            {
                if (_velocity.X < 0 && _position.X < -_sourceBounds.Width / 2)
                {
                    _position.X = _gameRoot._screenSize.X + _sourceBounds.Width / 2;
                }
                else if (_velocity.X > 0 && _position.X > _gameRoot._screenSize.X + _sourceBounds.Width / 2)
                {
                    _position.X = -_sourceBounds.Width / 2;
                }
                if (_velocity.Y < 0 && _position.Y < -_sourceBounds.Height / 2)
                {
                    _position.Y = _gameRoot._screenSize.Y + _sourceBounds.Height / 2;
                }
                else if (_velocity.Y > 0 && _position.Y > _gameRoot._screenSize.Y + _sourceBounds.Height / 2)
                {
                    _position.Y = -_sourceBounds.Height / 2;
                }
            }


   
        }
        public virtual void DrawSprite(SpriteBatch spriteBatch)
        {
            if (_dead) return;

            _drawBounds.X = (int)Math.Round(_position.X);
            _drawBounds.Y = (int)Math.Round(_position.Y);

            spriteBatch.Draw(_texture,
                             _drawBounds,
                             _sourceBounds,
                             _color,
                             _rotation,
                             _origin,
                             SpriteEffects.None,
                             0.0f);

        }
        public bool IsColliding(Sprite otherSprite)
        {
            if (_dead || !_colliding) return false;
            if (otherSprite == null || otherSprite == this) return false;
            if (otherSprite._dead || !otherSprite._colliding) return false;

            float dist = Vector2.Distance(_position, otherSprite._position);
            return dist <= _collisionRadius + otherSprite._collisionRadius;


        }

    }
}
