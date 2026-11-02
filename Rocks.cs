using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ASTEROIDS_2;
using Starlight_Decay;

namespace Asteroids_2
{
    public class Rocks : Sprite
    {
        public int _size;

        public Rocks(Game1 gameRoot, Vector2? startPosition = null, int size = 1, float maxSpeed = 200) : base(gameRoot)
        {
            _size = size;
            _color = Color.YellowGreen;
            _texture = _gameRoot._rocksTxr;
            _sourceBounds = GetSourceRectangle();
            _drawBounds.Size = _sourceBounds.Size;
            _origin = _sourceBounds.Size.ToVector2() / 2.0f;
            _colliding = true;
            _collisionRadius = (float)_sourceBounds.Width / 2.0f;

            if (startPosition == null)
            {
                switch (_gameRoot._random.Next(4))
                {
                    case 0:
                        _position = new Vector2(_gameRoot._random.Next(_gameRoot._screenSize.X), -128);
                        break;
                    case 1:
                        _position = new Vector2(_gameRoot._random.Next(_gameRoot._screenSize.X),
                            _gameRoot._screenSize.Y + 128);
                        break;
                    case 2:
                        _position = new Vector2(-128, _gameRoot._random.Next(_gameRoot._screenSize.Y));
                        break;
                    case 3:
                        _position = new Vector2(_gameRoot._screenSize.X + 128, _gameRoot._random.Next(_gameRoot._screenSize.Y));
                        break;
                }
            }

            else
            {
                _position = (Vector2)startPosition;
            }
            _dead = false;


            _rotation = (float)(_gameRoot._random.NextDouble() * Math.PI * 2.0f);
            _rotationSpeed = (float)(((_gameRoot._random.NextDouble() * 2.0f) - 1.0f) * Math.PI);

            _velocity = new Vector2(
                (float)((_gameRoot._random.NextDouble() * 2.0f) - 1.0f) * maxSpeed,
                (float)((_gameRoot._random.NextDouble() * 2.0f) - 1.0f) * maxSpeed);

            _wrapping = true;



        }
        protected Rectangle GetSourceRectangle()
        {
            List<Rectangle> _rects = new List<Rectangle>();
            _rects.Add(new Rectangle(344, 216, 40, 40));
            _rects.Add(new Rectangle(256, 128, 80, 80));
            _rects.Add(new Rectangle(256, 0, 128, 128));
            _rects.Add(new Rectangle(0, 0, 256, 256));


            return _rects[_size];
        }
        public void Explode()
        {
            _gameRoot._explodeSound.Play();
            _dead = true;
            for (int i = 0; i < (_size + 1) * 5; i++)
            {
                _gameRoot._newParticles.Add(new ParticleDebris(_gameRoot, _position));
            }
            if (_size > 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    _gameRoot._newRocks.Add(new Rocks(_gameRoot, _position, _size - 1, 300));
                }
            }
            _gameRoot._player.AddScore((4 - _size) * 50);

        }
        }
    }
