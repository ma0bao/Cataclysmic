using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public class Enemy : Entity
    {
        public RenderComponent renderData;
        public MoveComponent moveData;

        //Wandering 
        public Vector2 targetPos;
        public Vector2 targetVelocity;
        public float desiredSpeed;
        public float turnSpeed;

        //How far away to start decelerating when close to target
        public int slowRadius = 150;


        public Enemy()
        {
            
        }
        public override void Draw(float opacity)
        {
            renderData.DefualtDraw();
        }

        public override void DrawEx(float opacity) { return; } // Extra Renders, such as health bars. These are to be ignored by shaders and render on top of most elements except GUI.
        public override void Update(GameTime gameTime)
        {
            IncreaseVelocity();
            renderData.Position += moveData.velocity * moveData.deltaTime * moveData.speedModifiers;
            renderData.ResetHitBox();
        }

        public override void Damage(Entity cause, int amount)
        {
            
        }

        public virtual void IncreaseVelocity()
        {
            //Get Direction to target
            Vector2 direction = targetPos - renderData.Position;
            if (direction.Length() > 0.1f)
                direction.Normalize();

            //Get max speed we should accelerate to (desired speed)
            float distance = renderData.GetDistanceToTarget(targetPos);
            if (distance < slowRadius)
            {
                desiredSpeed = moveData.maxSpeed * (distance / slowRadius);
            }
            else
                desiredSpeed = moveData.maxSpeed;

            //Get Target Velocity we should accelerate to
            targetVelocity = direction * desiredSpeed;

            //How much to we need to increase velocity by to get to the target
            Vector2 steering = targetVelocity - moveData.velocity;

            //How much we will allow to steer
            float maxSteeringThisFrame = turnSpeed * moveData.deltaTime;
            if (steering.Length() > maxSteeringThisFrame)
            {
                steering.Normalize();
                steering *= maxSteeringThisFrame;
            }

            //add steering
            moveData.velocity += steering;

            //Clamp to be under maxSpeed
            if (moveData.velocity.Length() > moveData.maxSpeed)
            {
                moveData.velocity.Normalize();
                moveData.velocity *= moveData.maxSpeed;
            }
        }
        
        
        public override bool IsAlive()
        {
            return true;
        }

        public override Entity Clone()
        {
            return this;
        }

        public override void ApplyEffect(Effect effect)
        {
            return;
        }

    }
}
