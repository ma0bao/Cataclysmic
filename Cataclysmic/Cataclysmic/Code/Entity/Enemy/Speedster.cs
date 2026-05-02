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

        public bool HasSpear = true;

        WaveShot spear;
        EventTimer spearCooldown;

        //Dash
        IDash currentDash;
        EventTimer dashTimer;

        //How long to chase
        EventTimer chaseTimer;

        LinkedList<Ability> abilities;

        enum AttackState
        {
            Wander = 0,
            Run = 1,
            Melee = 2
        }

        AttackState currentState;
        public Speedster(Vector2 position) : base(Game1.texture_player, new Rectangle(position.ToPoint().X, position.ToPoint().Y, 50, 50), 32, 32)
        {
            //Components
            player = Game1.player;
            moveData = new MoveComponent(_maxSpeed: 500, _acceleration: 1200);
            spear = new WaveShot(
                pos: new Vector2(renderData.Position.X+20, renderData.Position.Y),
                direction: Vector2.Zero,
                speed: 0f,
                texture: Game1.texture_spear,
                scale: new Point(25, 185)
                );
            

            //Speeds
            turnSpeed = 1200;
            targetPos = renderData.GetRandomPoint();

            //Cooldowns
            spearCooldown = new EventTimer(7);
            spearCooldown.Done = true;
            spearCooldown.Pause();
            abilities = new LinkedList<Ability>();
            dashTimer = new EventTimer(.5f);
            chaseTimer = new EventTimer(6f);
            chaseTimer.Pause();
            dashTimer.Unpause();
            renderData.color = Color.White;

            SetStateToWander();
        }

        public override string ToString()
        {
            return base.ToString() + " "+currentState + "\n\nMax Speed: "+moveData.maxSpeed +"\nCurrentSpeed"+moveData.velocity.Length();
        }

        public override void Draw(float opacity)
        {
            //Set rotation to look at where he is going next
            //renderData.rotation = MathHelper.ToDegrees(renderData.GetRotationToVelocity(moveData.velocity));


            //Draw
            base.Draw(opacity);


            //Draw abilities 
            foreach (Ability abil in abilities)
            {
                if (abil != null)
                    abil.Draw(1.0f);
            }

            //Draw dashes
            if (currentDash != null)
                currentDash.Draw(renderData, moveData);

            if (spear != null)
            {
                spear.Draw();
            }
        }

        public void SetStateToWander()
        { 
            moveData.maxSpeed = 500;
            turnSpeed = 1200;
            slowRadius = 150;
            SetNewTargetPosition(renderData.GetRandomPoint());
            currentState = AttackState.Wander;
        }

        public void SetStateToRun()
        {
            moveData.maxSpeed = 500;
            turnSpeed = 1200;
            slowRadius = 150;
            SetNewTargetPosition(renderData.GetRandomPoint());
            currentState = AttackState.Run;
        }

        public void SetStateToMelee()
        {
            if (renderData.GetDistanceToTarget(player.renderData.Position) > 400)
            {
                currentDash = new SpeedDash();
                currentDash.Start(renderData, moveData);
            }
            moveData.maxSpeed = 600;
            slowRadius = 2;
            SetNewTargetPosition(player.renderData.Position);
            chaseTimer.Restart();
            currentState = AttackState.Melee;
        }

        public override void Update(GameTime gameTime)
        {
            UpdateTimers();
            moveData.deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            HasSpear = spearCooldown.Done;

            #region Get Target Position
            if (currentState == AttackState.Wander || currentState == AttackState.Run)
            {
                if (HasSpear)
                    spear.renderData.rotation = 0;
                if (IsAtTarget())
                {
                    int chance = Game1.rand.Next(1, 11);
                    if (chance <= 8)
                    {
                        SetStateToWander();
                        if (chance <= 6 && HasSpear)
                            Snipe();
                    }
                    else if(HasSpear)
                    {
                        SetStateToMelee();
                    }
                }
            }
            else if (currentState == AttackState.Melee)
            {
                SetNewTargetPosition(player.renderData.Position);
            }
            #endregion


            #region Update Based On State
            if (currentState == AttackState.Wander)
            {
                base.Update(gameTime);
                if (renderData.GetDistanceToTarget(player.renderData.Position) < 225 && HasSpear)
                {
                    SetStateToMelee();
                }
                dashTimer.Update();
            }
            else if (currentState == AttackState.Melee)
            {
                base.Update(gameTime);

                spear.renderData.rotation += 15;

                if (chaseTimer.Done)
                {
                    SetStateToRun();
                }

                chaseTimer.Update();
            }
            else if (currentState == AttackState.Run)
            {
                base.Update(gameTime);
                
            }
            #endregion


            //Set new dash if cooldown is over
            if (currentDash == null && dashTimer.Done)
            {
                currentDash = new SpeedDash();
                currentDash.Start(renderData, moveData);
            }

            //Reset Dash cooldown after dashing
            if (currentDash != null && currentDash.IsFinished)
            {
                currentDash = null;
                dashTimer.Restart(Game1.rand.Next(3, 7));
            }


            //Update stuffs
            if (currentDash != null)
                currentDash.Update(renderData, moveData);

            if (spear != null)
                spear.Update(gameTime);

            if (HasSpear)
            {
                spear.renderData.SetX(renderData.Position.X + 20);
                spear.renderData.SetY(renderData.Position.Y);
            }

            spear.collisionData.UpdateRotation(spear.renderData.rotation);
            if (player.Hitbox.Intersects(spear.collisionData))
                player.Damage(this, 8);
            

            foreach (Ability abil in abilities)
            {
                abil.Update(gameTime);
            }
            spearCooldown.Update();
            renderData.ResetHitBox();
        }

        public void Snipe()
        {
            HasSpear = false;
            spear = new WaveShot(
                pos: renderData.Position,
                direction: renderData.GetDirectionToTarget(player.renderData.Position),
                speed: 1000f,
                texture: Game1.texture_spear,
                scale: new Point(Game1.texture_spear.Width, Game1.texture_spear.Height)
                );
            spear.renderData.rotation = renderData.GetRotationToTarget(player.renderData.Position);
            spearCooldown.Restart();
        }
            

    }
}
