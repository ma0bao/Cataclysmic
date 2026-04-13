using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        public HealthComponent healthData;
        public CollisionComponent collision;



        //Wandering 
        public Vector2 targetPos;
        public Vector2 targetVelocity;
        public float desiredSpeed;
        public float turnSpeed = 1200;

        //How far away to start decelerating when close to target
        public int slowRadius = 150;

        //When considered to be at target
        public int distanceToBeAtTarget = 30;


        public Enemy(Texture2D texture, Rectangle destRect, float width, float height)
        {
            renderData = new RenderComponent(texture, destRect);
            moveData = new MoveComponent();
            healthData = new HealthComponent(50);
            collision = CollisionComponent.CreateRect(new Vector2(destRect.X, destRect.Y), width, height);
            

        }

        public override string ToString()
        {
            return "X: " + (int)renderData.Position.X + " Y: " + (int)renderData.Position.Y + " HP: " + healthData.currentHealth;
            // return base.ToString();
        }
        public override void Draw(float opacity)
        {
            if (healthData.invincible)
            {
                renderData.DrawFlash();
            }

            renderData.DefualtDraw();
            collision.DrawDebug();
            
        }

        public override void DrawEx(float opacity) { return; } // Extra Renders, such as health bars. These are to be ignored by shaders and render on top of most elements except GUI.
        public override void Update(GameTime gameTime)
        {
            moveData.deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            IncreaseVelocity();
            renderData.Position += moveData.velocity * moveData.deltaTime * moveData.speedModifiers;
            //Clamp movement 
            renderData.SetX(MathHelper.Clamp(renderData.Position.X, Game1.BOUNDS.Left, Game1.BOUNDS.Right));
            renderData.SetY(MathHelper.Clamp(renderData.Position.Y, Game1.BOUNDS.Top, Game1.BOUNDS.Height));

            renderData.ResetHitBox();
            collision.Update(renderData.Position, renderData.rotation);
            healthData.Update();
            
        }

        public virtual void UpdatePos(int ticks)
        {
            IncreaseVelocity();
            renderData.Position += moveData.velocity * moveData.deltaTime * moveData.speedModifiers * ticks;
            //Clamp movement 
            renderData.SetX(MathHelper.Clamp(renderData.Position.X, 0, Game1.WIDTH));
            renderData.SetY(MathHelper.Clamp(renderData.Position.Y, 0, Game1.HEIGHT));

            renderData.ResetHitBox();
            collision.Update(renderData.Position, renderData.rotation);
        }

        public override void Damage(Entity cause, int amount)
        {
            healthData.isAlive = healthData.Damage(cause, amount);
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

        public bool IsAtTarget()
        {
            return renderData.GetDistanceToTarget(targetPos) < distanceToBeAtTarget;
        }

        public void SetNewTargetPosition(Vector2 pos)
        {
            targetPos = pos;
            desiredSpeed = moveData.maxSpeed;
        }

        public void SetNewTargetPosition(Point pos)
        {
            targetPos = new Vector2(pos.X, pos.Y);
            desiredSpeed = moveData.maxSpeed;
        }

        public override bool IsAlive()
        {
            return healthData.isAlive;
        }

        public override Entity Clone()
        {
            return this;
        }

        public override void ApplyEffect(Effect effect)
        {
            return;
        }

        public virtual void Spawn(Vector2 spawnpoint)
        {
            throw new NotImplementedException();
        }

        override public void OnCollision() {
            throw new NotImplementedException();
        }

    }
}
