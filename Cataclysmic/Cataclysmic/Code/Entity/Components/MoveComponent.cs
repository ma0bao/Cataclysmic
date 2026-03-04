using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public class MoveComponent
    {
        public Vector2 velocity;
        public Vector2 lastVelocity = new Vector2();

        public float maxSpeed;
        public float acceleration;
        public float friction;
        public float deltaTime;
        public float speedModifiers;

        public MoveComponent(float _maxSpeed = 300f, float _acceleration = 2000f, float _friction = 2500f)
        {
            velocity = new Vector2();
            maxSpeed = _maxSpeed;
            acceleration = _acceleration;
            friction = _friction;
            speedModifiers = 1f;
            deltaTime = 1;
        }

        public Vector2 GetDirection()
        {
            Vector2 direction;

            if (velocity != Vector2.Zero)
            {
                direction = Vector2.Normalize(velocity);
            }
            else if (lastVelocity != Vector2.Zero)
            {
                direction = Vector2.Normalize(lastVelocity);
            }
            else
            {
                direction = Vector2.UnitX;
            }
            return direction;
        }

        public void ApplyFriction()
        {
            Vector2 frictionForce = -Vector2.Normalize(velocity) * friction * deltaTime;

            if (frictionForce.Length() > velocity.Length())
                velocity = Vector2.Zero;
            else
                velocity += frictionForce;
        }

    }
}
