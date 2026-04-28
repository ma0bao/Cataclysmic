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
        public BloodComponent bloodData;

        //Wandering 
        public Vector2 targetPos;
        public Vector2 targetVelocity;
        public float desiredSpeed;
        public float turnSpeed = 1200;

        //How far away to start decelerating when close to target
        public int slowRadius = 150;

        //When considered to be at target
        public int distanceToBeAtTarget = 30;

        //Stagger
        public float staggerResistance = 1.0f;
        EventTimer staggerTimer;

        public enum EnemyState
        {
            Active,
            Staggered
        }

        public EnemyState enemyState;

        public Enemy(Texture2D texture, Rectangle destRect, float width, float height)
        {
            renderData = new RenderComponent(texture, destRect);
            moveData = new MoveComponent();
            healthData = new HealthComponent(50);
            collision = CollisionComponent.CreateRect(new Vector2(destRect.X, destRect.Y), width, height);
            staggerTimer = new EventTimer();
            bloodData = new BloodComponent(() => collision.Center);
        }

        public virtual void Stagger(float secondsToStagger, bool UseResistance = true)
        {
            if (UseResistance)
                staggerTimer = new EventTimer(secondsToStagger * staggerResistance);
            else
                staggerTimer = new EventTimer(secondsToStagger);
            enemyState = EnemyState.Staggered;
        }

        public override string ToString()
        {
            return "X: " + (int)renderData.Position.X + " Y: " + (int)renderData.Position.Y + " HP: " + healthData.currentHealth;
            // return base.ToString();
        }
        public override void Draw(float opacity)
        {
            

            renderData.DefualtDraw();
            collision.DrawDebug();

            if (healthData.invincible)
            {
                renderData.DrawFlash();
            }
        }

        public override void DrawEx(float opacity) {
            if (!Game1.BOUNDS.Contains(renderData.hitBox)) {
                Rectangle rect = new Rectangle(
                    (int)MathHelper.Clamp(renderData.Position.X, Game1.BOUNDS.Left, Game1.BOUNDS.Right),
                    (int)MathHelper.Clamp(renderData.Position.Y, Game1.BOUNDS.Top, Game1.BOUNDS.Bottom),
                    10,
                    10
                );
                Game1.self.spriteBatch.Draw(Game1.texture_blank, rect, Color.Red);
            }

        } 
        public override void Update(GameTime gameTime)
        {
            if (enemyState == EnemyState.Staggered)
            {
                renderData.Position += new Vector2(.5f, .5f).GetRandomized();
                return;
            }

            moveData.deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            IncreaseVelocity();
            renderData.Position += moveData.velocity * moveData.deltaTime * moveData.speedModifiers;
            //Clamp movement 
            renderData.SetX(MathHelper.Clamp(renderData.Position.X, Game1.BOUNDS.Left, Game1.BOUNDS.Right));
            renderData.SetY(MathHelper.Clamp(renderData.Position.Y, Game1.BOUNDS.Top, Game1.BOUNDS.Bottom));

            renderData.ResetHitBox();
            collision.Update(renderData.Position, renderData.rotation);
            healthData.Update();
            
        }

        public void SpewBlood(int amount)
        {
            BloodHit hit = BloodHit.Medium;
            hit.Count = amount;
            bloodData.Spew(hit);
        }

        public void UpdateTimers()
        {
            if (staggerTimer.Done && enemyState == EnemyState.Staggered)
                enemyState = EnemyState.Active;
            staggerTimer.Update();
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
            Damage(cause, amount, BloodHit.Medium);
        }

        public virtual void Damage(Entity cause, int amount, BloodHit hit)
        {
            bool wasAlive = healthData.isAlive;
            int hpBefore = healthData.currentHealth;

            healthData.Damage(cause, amount);

            bool tookDamage = healthData.currentHealth < hpBefore || (wasAlive && !healthData.isAlive);
            if (!tookDamage)
                return;

            bloodData.Spew(hit);

            if (wasAlive && !healthData.isAlive)
                bloodData.Burst();
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
