using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Asteroids_2;
using Starlight_Decay;

namespace ASTEROIDS_2
{
    public class Player : Ship
    {
        public float _coolDownCounter, _coolDownLimit, _invicibilityCounter, _invincibilityLimit;
        public int _lives;
        public int _score;
        public float _timer;

        public Player(Game1 gameRoot, Vector2 startPosition, float startRotation) : base(gameRoot, startPosition, startRotation)
        {
            _color = Color.Turquoise;
            _coolDownCounter = -1.0f;
            _coolDownLimit = 0.5f;
            _lives = 3;
            _invincibilityLimit = 3.0f;
        }
        public override void UpdateSprite(GameTime gameTime)
        {
            if (_dead) return;
            _colliding = true;

            if (_invicibilityCounter > 0)
            {
                _invicibilityCounter -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                _colliding = false;
            }

            if (_gameRoot._currentKeys.IsKeyDown(Keys.A))
            {
                _rotationSpeed = -_turnSpeed;
            }
            else if (_gameRoot._currentKeys.IsKeyDown(Keys.D))
            {
                _rotationSpeed = _turnSpeed;
            }
            else
            {
                _rotationSpeed = 0;
            }
            if (_gameRoot._currentKeys.IsKeyDown(Keys.W))
            {
                _velocity += GetForwardVector() * _acceleration;
            }
            if (_gameRoot._currentKeys.IsKeyDown(Keys.Space
                ))
            {
               
                if (_coolDownCounter == 0.0f)
                {
                    Shoot();
                }
                _coolDownCounter += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_coolDownCounter >= _coolDownLimit)
                {
                    _coolDownCounter = 0.0f;
                }
            }
                else
                {
                    _coolDownCounter = 0.0f;
                }


            if (_invicibilityCounter > 0.0f)
            {
                _color = Color.Lerp
                    (Color.Red, Color.Transparent, 
                    (float)Math.Sin(gameTime.TotalGameTime.TotalSeconds * 8) / 2.0f + 0.5f);
            }
            else
            {
                _color = Color.Turquoise;
            }



            base.UpdateSprite(gameTime);
        }
        public void AddScore(int score)
        {
            _score += score;
        }

        public void SubtractScore(int score)
        {
            _score -= score; 
        }

        public void Damage()
        {
            _gameRoot._inviSound.Play();
            _lives--;
            _invicibilityCounter = _invincibilityLimit;
        }
        public string GetLivesString()
        {
            string output = "";
            for (int i = 0; i < _lives; i++)
            {
                output += "*";
            }
            return output;
        }
        protected override void OnHit()
        {
            if (_invicibilityCounter > 0.0f) return;
            Damage();
            if (_lives <= 0)
            {
                _gameRoot.StartGameOver();
                base.OnHit();
            }

        }
    }
}
