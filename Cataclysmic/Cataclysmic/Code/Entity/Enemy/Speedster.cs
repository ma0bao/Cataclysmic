using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    class Speedster : Entity
    {
        //Components
        RenderComponent renderData;
        MoveComponent moveData;

        Player player;

        //Wandering 
        Vector2 targetPos;
        Vector2 targetVelocity;
        float desiredSpeed;
        float turnSpeed;

        //How far away to start decelerating when close to target
        const int slowRadius = 150;

        //Dash
        IDash currentDash;
        EventTimer dashTimer;

        //How long to chase
        EventTimer chaseTimer;

        LinkedList<Ability> abilities;

        enum AttackState
        {
            GoOnScreen = -1,
            Wander = 0,
            Run = 1,
            Melee = 2
        }

        AttackState currentState = AttackState.Wander;
        public Speedster(Rectangle _destRect, Player _player)
        {
            //Components
            player = _player;
            renderData = new RenderComponent(Game1.self.texture_player, _destRect);
            moveData = new MoveComponent(_maxSpeed: 500, _acceleration: 1200);

            //Speeds
            turnSpeed = 600;
            targetPos = GetRandomPoint();
            desiredSpeed = moveData.maxSpeed;

            //Cooldowns
            abilities = new LinkedList<Ability>();
            dashTimer = new EventTimer(.5f);
            chaseTimer = new EventTimer(8f);
            chaseTimer.Pause();
            dashTimer.Unpause();
            renderData.color = Color.White;
        }
        public override void Draw(float opacity)
        {
            //Set rotation to look at where he is going next
            renderData.rotation = renderData.GetRotationToTarget(renderData.Position + moveData.velocity);

            //Draw
            renderData.DefualtDraw();

            //Game1.self.spriteBatch.Draw(renderData.texture, targetPos, Color.Red);

            //Draw abilities 
            foreach (Ability abil in abilities)
            {
                if (abil != null)
                    abil.Draw(1.0f);
            }

            //Draw dashes
            if (currentDash != null)
                currentDash.Draw(renderData, moveData);
        }

        public override void DrawEx(float opacity) { }
        public override void Update(GameTime gameTime)
        {
            moveData.deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Get Target Position
            if (currentState == AttackState.Wander || currentState == AttackState.Run)
            {
                if (renderData.GetDistanceToTarget(targetPos) < 15)
                {
                    int chance = Game1.rand.Next(1, 11);
                    if (chance <= 8)
                    {
                        currentState = AttackState.Wander;
                        targetPos = GetRandomPoint();
                        desiredSpeed = moveData.maxSpeed;
                        if (chance <= 6)
                            Snipe();
                    }
                    else
                    {
                        renderData.color = Color.Pink;
                        chaseTimer.Restart();
                        currentState = AttackState.Melee;
                    }
                }
            } else if (currentState == AttackState.Melee)
            {
                targetPos = player.renderData.Position;
            }

            //If you care close to player and not in a chase, attack him
            if (renderData.GetDistanceToTarget(player.renderData.Position) < 300 && currentState == AttackState.Wander)
            {
                currentState = AttackState.Melee;
                renderData.color = Color.Red;
                chaseTimer.Restart();
            }

            //If you are done with the chase, run away to a random point
            if (chaseTimer.Done)
            {
                renderData.color = Color.Aqua;
                currentState = AttackState.Run;
                targetPos = GetRandomPoint();
                desiredSpeed = moveData.maxSpeed;
                chaseTimer.Reset();
            }

            //Set new dash if cooldown is over
            if (currentDash == null && dashTimer.Done)
            {
                int chance = Game1.rand.Next(0, 2);
                if (chance == 1)
                    currentDash = new BlinkDash();
                else
                    currentDash = new SpeedDash();
                currentDash.Start(renderData, moveData);
            }

            //Reset Dash cooldown after dashing
            if (currentDash != null && currentDash.IsFinished)
            {
                currentDash = null;
                dashTimer.Restart(Game1.rand.Next(3, 7));
            }


            //Move around
            IncreaseVelocity();
            renderData.Position += moveData.velocity * moveData.deltaTime * moveData.speedModifiers;
            renderData.ResetHitBox();

            //Clamp movement to be on screen
            if (currentState != AttackState.GoOnScreen)
            {
                renderData.SetX(MathHelper.Clamp(renderData.Position.X, 0, Game1.WIDTH));
                renderData.SetY(MathHelper.Clamp(renderData.Position.Y, 0, Game1.HEIGHT));
            }

            //Update stuffs
            if (currentDash != null)
                currentDash.Update(renderData, moveData);
            dashTimer.Update();
            chaseTimer.Update();
            foreach (Ability abil in abilities)
            {
                abil.Update(gameTime);
            }
        }

        public void Snipe()
        {
            //Shoot a thingy at player
            abilities.AddFirst(new CrackleBurst(renderData.Position, renderData.GetRotationToTarget(player.renderData.Position)));
        }
        public void IncreaseVelocity()
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

        public Vector2 GetRandomPoint()
        {
            float x = Game1.rand.Next(50, Game1.WIDTH-50);
            float y = Game1.rand.Next(50, Game1.HEIGHT-50);

            return new Vector2(x, y);
        }


        public override void Damage(Entity cause, int amount)
        {
            return;
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
