using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    class Speedster : Enemy
    {

        Player player;

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
        public Speedster(Rectangle _destRect, Player _player) : base(Game1.texture_player, _destRect, 32, 32)
        {
            //Components
            player = _player;
            moveData = new MoveComponent(_maxSpeed: 500, _acceleration: 1200);

            //Speeds
            turnSpeed = 600;
            targetPos = renderData.GetRandomPoint();
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

            Game1.self.spriteBatch.Draw(renderData.texture, targetPos, Color.Red);

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

        public override void Update(GameTime gameTime)
        {
            UpdateTimers();

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
                        targetPos = renderData.GetRandomPoint();
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
                targetPos = renderData.GetRandomPoint();
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

            base.Update(gameTime);

            

            //Update stuffs
            if (currentDash != null)
                currentDash.Update(renderData, moveData);
            dashTimer.Update();
            chaseTimer.Update();
            foreach (Ability abil in abilities)
            {
                abil.Update(gameTime);
            }
            renderData.ResetHitBox();
        }

        public void Snipe()
        {
            //Shoot a thingy at player
            CrackleBurst temp = new CrackleBurst(renderData.Position, renderData.GetRotationToTarget(player.renderData.Position));
            temp.color = Color.Red;
            abilities.AddFirst(temp);
        }


    }
}
