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
    public class Ship : Sprite
    {
        protected float _acceleration, _friction, _turnSpeed, _maxSpeed, _shotSpeed;
        public Ship(Game1 gameRoot, Vector2 startPosition, float startRotation) : base (gameRoot)
        {
            _texture = _gameRoot._shipTxr;
            _drawBounds = _texture.Bounds;
            _sourceBounds = _texture.Bounds;
            _origin = _sourceBounds.Size.ToVector2() / 2.0f;
            _colliding = true;
            _collisionRadius = (float)_sourceBounds.Width / 4.0f;
            _position = startPosition;
            _rotation = startRotation;

            _acceleration = 10.0f;
            _friction = 0.005f;
            _turnSpeed = (float)Math.PI;
            _maxSpeed = 500.0f;
            _shotSpeed = 750.0f;

            _wrapping = true;
        }
        public override void UpdateSprite(GameTime gameTime)
        {
            if (_dead) return;

            foreach (Rocks eachRock in _gameRoot._rocks)
            {
                if (IsColliding(eachRock))
                {
                    OnHit();
                    eachRock.Explode();

                }
            }

            _velocity *= 1.0f - _friction;

            float speed = _velocity.Length();
            if (speed > _maxSpeed)
            {
                Vector2 normalisedVelocity = _velocity;
                normalisedVelocity.Normalize();
                _velocity = normalisedVelocity * _maxSpeed;
            }
            _gameRoot._particles.Add(new ParticleTrail(_gameRoot, _position + GetForwardVector() * -22, _color));



                base.UpdateSprite(gameTime);
        }
        public Vector2 GetForwardVector()
        {
            return new Vector2((float)Math.Cos(_rotation), (float)Math.Sin(_rotation));
        }
        protected void Shoot()
        {
            _gameRoot._zapSound.Play();
            _gameRoot._particles.Add(new ParticleShot(_gameRoot, _position, GetForwardVector() * _shotSpeed));
        }
        protected virtual void OnHit()
        {
            _dead = true;
            for (int i = 0; i < 32; i++)
            {
                _gameRoot._newParticles.Add(new ParticleDebris(_gameRoot, _position));
            }
        }
    }
}
