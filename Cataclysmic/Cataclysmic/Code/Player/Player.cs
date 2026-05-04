using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using System.Timers;

namespace Cataclysmic
{
    public class Player : Entity
    {

        public RenderComponent renderData;
        public MoveComponent moveData;
        public HealthComponent healthData;
        public CollisionComponent Hitbox;
        public ManaComponent timeEnergy;
        public BloodComponent bloodData;
        Texture2D Square;

        Rectangle staminaBarRect;
        Rectangle staminaRect;

        //Healthbar
        Rectangle healthBarRect;
        Rectangle healthBar;
        int HEALTHWIDTH = 200;

        //TimeEnergy Bar
        Rectangle manaBarRect;
        Rectangle manaBar;
        int MANAWIDTH = 200;


        public float angle;

        //Movement Display 
        bool isVisible = true;

        IDash currentDash;
        EventTimer dashCooldown;

        //Abilities
        public AbilityWrapper[] Abilities;
        public EventTimer abilityTimer;

        public float maxSpeed = 500;

        public Player(Rectangle _destRect)
        {
            LoadContent();
            renderData = new RenderComponent(Game1.texture_playerIdle, _destRect);
            moveData = new MoveComponent(maxSpeed, 2000f, 3000f);
            healthData = new HealthComponent(50);
            timeEnergy = new ManaComponent(100);

            // Setup idle animation 32x32 2 frames
            renderData.SetupAnimation(32, 32, 2);
            renderData.Play();

            Abilities = new AbilityWrapper[4];
            Abilities[0] = new RevolverWrapper();
            Abilities[1] = new SwapWrapper();
            Abilities[2] = new CrackleBurstWrapper();
            Abilities[3] = new SlashWrapper();

            for (int i = 0; i < Abilities.Length; i++)
                Abilities[i].abilitySpot = i;

            dashCooldown = new EventTimer(.5f);
            staminaBarRect = new Rectangle(5, 5, 200, 15);
            staminaRect = new Rectangle(5, 5, 1, 15);
            healthBarRect = new Rectangle(305, 5, HEALTHWIDTH, 15);
            healthBar = new Rectangle(305, 5, HEALTHWIDTH, 15);
            manaBarRect = new Rectangle(605, 5, MANAWIDTH, 15);
            manaBar = new Rectangle(605, 5, MANAWIDTH, 15);
            dashCooldown.Unpause();
            angle = 0.0f;

            Hitbox = CollisionComponent.CreateRect(renderData.Position, _destRect.Width - 10, _destRect.Height - 30);

            bloodData = new BloodComponent(() => renderData.Position)
            {
                Tint = Color.Crimson,
                baseSize = 6,
                sizeVariance = 3,
                chunkChance = 0.4f,
                deathCountBonus = 80,
                deathSpeedMult = 2.4f,
                deathLifetimeMult = 2.2f
            };

            abilityTimer = new EventTimer();
        }

        public override void Update(GameTime gameTime)
        {
            
            GamePadState gamePad = GamePad.GetState(PlayerIndex.One);
            moveData.deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            GetVelocity(gameTime, gamePad);
            
            int MouseX = Game1.MS.X;
            int MouseY = Game1.MS.Y;

            float directionX = Game1.self.cursor.Position.X - (renderData.Position.X);
            float directionY = Game1.self.cursor.Position.Y - (renderData.Position.Y);
            angle = (float)(Math.Atan2(directionY, directionX) + (Math.PI * 0.5f));

            // Collision
            #region 
            renderData.Position += moveData.velocity * moveData.deltaTime * moveData.speedModifiers;
            //Position = new Vector2(MathHelper.Clamp(Position.X, 0, Game1.WIDTH), MathHelper.Clamp(Position.Y, 0, Game1.HEIGHT));
            float newX = renderData.Position.X;
            if (renderData.Position.X > Game1.BOUNDS.Right - renderData.DestRect.Width / 2)
            {
                //moveData.velocity.X = -Math.Abs(moveData.velocity.X);
                newX = Game1.BOUNDS.Right - renderData.DestRect.Width / 2;
            }
            else if (renderData.Position.X - renderData.DestRect.Width / 2 < Game1.BOUNDS.Left)
            {
                //moveData.velocity.X = Math.Abs(moveData.velocity.X);
                newX = Game1.BOUNDS.Left + renderData.DestRect.Width / 2;
            }

            float newY = renderData.Position.Y;
            if (renderData.Position.Y > Game1.BOUNDS.Bottom - renderData.DestRect.Height / 2)
            {
                //moveData.velocity.Y = -Math.Abs(moveData.velocity.Y);
                newY = Game1.BOUNDS.Bottom - renderData.DestRect.Height / 2;
            }
            else if (renderData.Position.Y - renderData.DestRect.Height / 2 < Game1.BOUNDS.Top)
            {
                //moveData.velocity.Y = Math.Abs(moveData.velocity.Y);
                newY = Game1.BOUNDS.Top + renderData.DestRect.Height / 2;
            }

            renderData.Position = new Vector2(newX, newY);
            #endregion

            // Attacks/Abilities
            foreach (AbilityWrapper abilWrap in Abilities) {
                abilWrap.Update(gameTime);
            }

            // Dashes
            if (dashCooldown.Done)
            {
                ScanForDash(gamePad);
            }
            if (currentDash != null)
            {
                currentDash.Update(renderData, moveData);
                if (currentDash.IsFinished)
                {
                    currentDash = null;
                    dashCooldown.Restart();
                }
            }
            dashCooldown.Update();
            

            // switch to walk and idle
            if (moveData.velocity.Length() > 10f)
            {
                renderData.SetState(AnimState.Walk, Game1.texture_playerWalk, 10, 32, 32);
            }
            else
            {
                renderData.SetState(AnimState.Idle, Game1.texture_playerIdle, 2, 32, 32);
            }

            renderData.UpdateAnimation(gameTime);
            Hitbox.Update(renderData.Position, angle);
            healthData.Update();

            ScanDamage();
            abilityTimer.Update();
        }

        public void ScanForDash(GamePadState gamePad)
        {
            if ((gamePad.IsButtonDown(Buttons.A) || Keyboard.GetState().IsKeyDown(Keys.Space)) && currentDash == null)
            {
                currentDash = new SpeedDash();
                currentDash.Start(renderData, moveData);
            }

            if ((gamePad.IsButtonDown(Buttons.B) || Keyboard.GetState().IsKeyDown(Keys.C)) && currentDash == null)
            {
                currentDash = new BlinkDash();
                currentDash.Start(renderData, moveData);
            }
            if ((gamePad.IsButtonDown(Buttons.Y) || Keyboard.GetState().IsKeyDown(Keys.LeftShift)) && currentDash == null)
            {
                currentDash = new ChargeUpDash();
                currentDash.Start(renderData, moveData);
            }
        }

        public void SetDestRectWidth(int val)
        {
            renderData._destRect.Width = val;
        }

        public void SetDestRectHeight(int val)
        {
            renderData._destRect.Height = val;
        }

        public void GetVelocity(GameTime gameTime, GamePadState gpInput)
        {
            Vector2 input = Vector2.Zero;
            
                input = new Vector2(gpInput.ThumbSticks.Left.X, -gpInput.ThumbSticks.Left.Y);
                KeyboardState ks = Keyboard.GetState();
                if (ks.IsKeyDown(Game1.player1_moveUp))
                    input.Y -= 1;
                if (ks.IsKeyDown(Game1.player1_moveDown))
                    input.Y += 1;
                if (ks.IsKeyDown(Game1.player1_moveLeft))
                    input.X -= 1;
                if (ks.IsKeyDown(Game1.player1_moveRight))
                    input.X += 1;
            

            if (input.Length() > 1f)
                input.Normalize();

            moveData.velocity += input * moveData.acceleration * moveData.deltaTime;

            if (moveData.velocity.Length() > moveData.maxSpeed)
            {
                moveData.velocity.Normalize();
                moveData.velocity *= moveData.maxSpeed;
            }

            if (input == Vector2.Zero)
            {
                if (moveData.velocity.Length() > 0)
                {
                    Vector2 frictionForce = -Vector2.Normalize(moveData.velocity) * moveData.friction * moveData.deltaTime;

                    if (frictionForce.Length() > moveData.velocity.Length())
                        moveData.velocity = Vector2.Zero;
                    else
                        moveData.velocity += frictionForce;
                }
            }
            if (moveData.velocity.Length() > 0)
            {
                moveData.lastVelocity.X = moveData.velocity.X;
                moveData.lastVelocity.Y = moveData.velocity.Y;
            }
        }

        public void LoadContent()
        {
            Square = Game1.texture_square;
        }

        public void UpdatePosition(int ticks)
        {
            renderData.Position += (moveData.velocity * moveData.deltaTime * moveData.speedModifiers) * ticks;
        }

        public Vector2 GetUpdatedPosition(int ticks)
        {
            return renderData.Position + (moveData.velocity * moveData.deltaTime * moveData.speedModifiers) * ticks;
        }

        public Vector2 GetDirection()
        {
            Vector2 direction;

            if (moveData.velocity != Vector2.Zero)
            {
                direction = Vector2.Normalize(moveData.velocity);
            }
            else if (moveData.lastVelocity != Vector2.Zero)
            {
                direction = Vector2.Normalize(moveData.lastVelocity);
            }
            else
            {
                direction = Vector2.UnitX;
            }
            return direction;
        }

        public float GetAngleToMouse()
        {
            int MouseX = Mouse.GetState().X;
            int MouseY = Mouse.GetState().Y;
            float directionX = MouseX - renderData.Position.X;
            float directionY = MouseY - renderData.Position.Y;

            return MathHelper.ToDegrees((float)(Math.Atan2(directionY, directionX) + (Math.PI * 0.5f)));
        }


        public override void Draw(float opacity)
        {
            if (!isVisible)
                return;

            renderData.color = Color.White;
            

             renderData.rotation = GetAngleToMouse();
            

            
            Game1.self.spriteBatch.Draw(renderData.texture, renderData._destRect, renderData.sourceRect, renderData.color * opacity, MathHelper.ToRadians(renderData.rotation), renderData.origin, SpriteEffects.None, 0f);

            if (currentDash != null)
                currentDash.Draw(renderData, moveData);

            foreach (AbilityWrapper abilWrap in Abilities) {
                abilWrap.DrawAbilities();
            }
            

            Hitbox.DrawDebug();

        }

        public override void DrawEx(float opacity)
        {
            // Health Bar
            Game1.self.spriteBatch.Draw(Game1.texture_blank, new Rectangle(219, 922, 350, 60), Color.DarkRed);
            Game1.self.spriteBatch.Draw(Game1.texture_blank, new Rectangle(219, 922, 1+(int)(350 * healthData.lerpValue), 60), Color.Red);

            // Mana Bar
            Game1.self.spriteBatch.Draw(Game1.texture_blank, new Rectangle(219, 1003, 350, 60), Color.Purple);
            Game1.self.spriteBatch.Draw(Game1.texture_blank, new Rectangle(219, 1003, 1+(int)(timeEnergy.lerpValue * HEALTHWIDTH), 60), Color.Lavender);

            //1
            Game1.self.spriteBatch.Draw(Abilities[0].GetTexture(), new Rectangle(634, 950, 83, 83), Color.White);
            Game1.self.spriteBatch.Draw(Game1.texture_blank, new Rectangle(634, 950, 83, (int)((double)Abilities[0].cooldownFrames / Abilities[0].GetMaxCooldown() * 83)), Color.White * 0.5f);
            //2
            Game1.self.spriteBatch.Draw(Abilities[1].GetTexture(), new Rectangle(762, 950, 80, 80), Color.White);
            Game1.self.spriteBatch.Draw(Game1.texture_blank, new Rectangle(762, 950, 83, (int)((double)Abilities[1].cooldownFrames / Abilities[1].GetMaxCooldown() * 80)), Color.White * 0.5f);
            //3
            Game1.self.spriteBatch.Draw(Abilities[2].GetTexture(), new Rectangle(894, 950, 83, 83), Color.White);
            Game1.self.spriteBatch.Draw(Game1.texture_blank, new Rectangle(894, 950, 83, (int)((double)Abilities[2].cooldownFrames / Abilities[2].GetMaxCooldown() * 80)), Color.White * 0.5f);
            //4
            Game1.self.spriteBatch.Draw(Abilities[3].GetTexture(), new Rectangle(1026, 950, 83, 83), Color.White);
            Game1.self.spriteBatch.Draw(Game1.texture_blank, new Rectangle(1026, 950, 83, (int)((double)Abilities[3].cooldownFrames / Abilities[3].GetMaxCooldown() * 83)), Color.White * 0.5f);

        }




        override public Boolean IsAlive() {
            return true;
        }

        public bool ScanDamage()
        {
            foreach (Enemy e in Game1.self.currentEnvironment.GetEnemies())
            {
                float pCollisionDepth;
                Vector2 pCollisionNormal;
                if (Hitbox.Intersects(e.collision, out pCollisionDepth, out pCollisionNormal))
                {
                    Damage(e, 1);
                    return true;
                }
            }
            return false;

        }
        public override void Damage(Entity cause, int amount)
        {
            bool wasAlive = healthData.isAlive;
            int hpBefore = healthData.currentHealth;

            healthData.Damage(cause, amount);

            bool tookDamage = healthData.currentHealth < hpBefore
                              || (wasAlive && !healthData.isAlive);
            if (!tookDamage)
                return;

            Game1.Shake(0.5f, Math.Max(2.0f, amount/2.0f));
            bloodData.Spew(BloodHit.Medium);
            if (wasAlive && !healthData.isAlive)
                bloodData.Burst();
        }
        public override void ApplyEffect(Effect effect)
        {
            return;
        }

        public override void OnCollision()
        {
            throw new NotImplementedException();
        }


        public bool IsAbilityPressed(int _AbilityPosition, bool _HeldOverPressed = false) {
            /* 
             Why did I code it like this?
             Firstly, having the ability slot position being passed in as a parameter makes the bulk of the logic handled within this method, rather than the ability wrapper.

            Secondly, this code is optimized enough. It will only ever check 1 of the ability slots, and will check 3 conditionals after another, return false immediately after rather than checking the other if (abil == x) statements.
             
            This also looks much prettier lol. If you think it is too clunky, collapse the method / region
             */
            if (_AbilityPosition == 0)
            {
                // Mouse Input
                if (Game1.MS.LeftButton == ButtonState.Pressed && (_HeldOverPressed || Game1.oldMS.LeftButton == ButtonState.Released)) { 
                    return true;
                }

                // Keyboard Input
                if (Game1.KB.IsKeyDown(Keys.D1) && (_HeldOverPressed || Game1.oldKB.IsKeyUp(Keys.D1))) {
                    return true;
                }

                // Gamepad Input
                if (Game1.GS.IsButtonDown(Buttons.LeftTrigger) && (_HeldOverPressed || Game1.GS.IsButtonUp(Buttons.LeftTrigger))) {
                    return true;
                }
            }
            else if (_AbilityPosition == 1) {
                // Mouse Input
                if (Game1.MS.RightButton == ButtonState.Pressed && (_HeldOverPressed || Game1.oldMS.RightButton == ButtonState.Released))
                {
                    return true;
                }

                // Keyboard Input
                if (Game1.KB.IsKeyDown(Keys.D2) && (_HeldOverPressed || Game1.oldKB.IsKeyUp(Keys.D2)))
                {
                    return true;
                }

                // Gamepad Input
                if (Game1.GS.IsButtonDown(Buttons.RightStick) && (_HeldOverPressed || Game1.GS.IsButtonUp(Buttons.RightTrigger)))
                {
                    return true;
                }
            }
            else if (_AbilityPosition == 2)
            {
                // Keyboard Input
                if (Game1.KB.IsKeyDown(Keys.Q) && (_HeldOverPressed || Game1.oldKB.IsKeyUp(Keys.Q)))
                {
                    return true;
                }

                // Gamepad Input
                if (Game1.GS.IsButtonDown(Buttons.X) && (_HeldOverPressed || Game1.GS.IsButtonUp(Buttons.X)))
                {
                    return true;
                }
            }
            else if (_AbilityPosition == 3)
            {
                // Keyboard Input
                if (Game1.KB.IsKeyDown(Keys.E) && (_HeldOverPressed || Game1.oldKB.IsKeyUp(Keys.E)))
                {
                    return true;
                }

                // Gamepad Input
                if (Game1.GS.IsButtonDown(Buttons.A) && (_HeldOverPressed || Game1.GS.IsButtonUp(Buttons.A)))
                {
                    return true;
                }
            }
            return false;
        }

        
    }
}
