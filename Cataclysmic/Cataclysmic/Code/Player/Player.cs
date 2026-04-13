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


        float angle;

        //Movement Display 
        bool isVisible = true;

        IDash currentDash;
        EventTimer dashCooldown;

        //Abilities
        List<Ability> abilities;
        Dictionary<string, EventTimer> abilityCooldowns;
        Dictionary<string, float> abilityCosts;

        public Player(Rectangle _destRect)
        {
            LoadContent();
            renderData = new RenderComponent(Game1.texture_playerIdle, _destRect);
            moveData = new MoveComponent();
            healthData = new HealthComponent(50);
            timeEnergy = new ManaComponent(100);

            // Setup idle animation 32x32 2 frames
            renderData.SetupAnimation(32, 32, 2);
            renderData.Play();

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

            abilities = new List<Ability>();

            // Add abilities here: (start ready to use)
            abilityCooldowns = new Dictionary<string, EventTimer>();
            abilityCooldowns["Revolver"] = new EventTimer(Revolver.COOLDOWN);
            abilityCooldowns["CrackleBurst"] = new EventTimer(CrackleBurst.COOLDOWN);
            abilityCooldowns["CircleSlash"] = new EventTimer(CircleSlash.COOLDOWN);
            abilityCooldowns["Slash"] = new EventTimer(Slash.COOLDOWN);
            foreach (EventTimer cd in abilityCooldowns.Values) cd.Done = true;

            abilityCosts = new Dictionary<string, float>();
            abilityCosts["Revolver"] = 0;
            abilityCosts["CrackleBurst"] = CrackleBurst.MANA_COST;
            abilityCosts["CircleSlash"] = CircleSlash.MANA_COST;
            abilityCosts["Slash"] = 0;

        }

        // check if ability is off cooldown, if so, restart cooldown and return true
        public bool TryUseAbility(string abilityName)
        {
            EventTimer timer = abilityCooldowns[abilityName];
            float cost = abilityCosts[abilityName];
            if (timer.Done && timeEnergy.currentMana >= cost)
            {
                timer.Restart();
                return true;
            }
            return false;
        }

        // for upgrades later
        public void SetAbilityCooldown(string abilityName, float seconds)
        {
            abilityCooldowns[abilityName].SetTime(seconds);
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
                moveData.velocity.X = -Math.Abs(moveData.velocity.X);
                newX = Game1.BOUNDS.Right - renderData.DestRect.Width / 2;
            }
            else if (renderData.Position.X - renderData.DestRect.Width / 2 < Game1.BOUNDS.Left)
            {
                moveData.velocity.X = Math.Abs(moveData.velocity.X);
                newX = Game1.BOUNDS.Left + renderData.DestRect.Width / 2;
            }

            float newY = renderData.Position.Y;
            if (renderData.Position.Y > Game1.BOUNDS.Bottom - renderData.DestRect.Height / 2)
            {
                moveData.velocity.Y = -Math.Abs(moveData.velocity.Y);
                newY = Game1.BOUNDS.Bottom - renderData.DestRect.Height / 2;
            }
            else if (renderData.Position.Y - renderData.DestRect.Height / 2 < Game1.BOUNDS.Top)
            {
                moveData.velocity.Y = Math.Abs(moveData.velocity.Y);
                newY = Game1.BOUNDS.Top + renderData.DestRect.Height / 2;
            }

            renderData.Position = new Vector2(newX, newY);
            #endregion

            // Attacks/Abilities
            if (Game1.MS.LeftButton == ButtonState.Pressed)
            {
                if (TryUseAbility("Revolver"))
                    abilities.Add(new Revolver(renderData.Position, angle));
            }

            if (Game1.MS.RightButton == ButtonState.Pressed)
            {
                if (TryUseAbility("Slash"))
                    abilities.Add(new Slash(renderData.Position, angle));
            }
            
            if (Game1.KB.IsKeyDown(Keys.Q) && !Game1.oldKB.IsKeyDown(Keys.Q))
            {
                if (TryUseAbility("CrackleBurst"))
                    abilities.Add(new CrackleBurst(renderData.Position, angle));
            }

            if (Game1.KB.IsKeyDown(Keys.E) && !Game1.oldKB.IsKeyDown(Keys.E))
            {
                if (TryUseAbility("CircleSlash"))
                    abilities.Add(new CircleSlash(renderData.Position));
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
            foreach (EventTimer cd in abilityCooldowns.Values) cd.Update();
            foreach (Ability abil in abilities)
            {
                abil.Update(gameTime);
            }

            for (int i = abilities.Count - 1; i >= 0; i--)
            {
                if (!abilities[i].IsAlive())
                    abilities.RemoveAt(i);
            }

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

            return (float)(Math.Atan2(directionY, directionX) + (Math.PI * 0.5f));
        }


        public override void Draw(float opacity)
        {
            if (!isVisible)
                return;

            Color color = Color.White;
            

            float rotation = GetAngleToMouse();
            

            
            Game1.self.spriteBatch.Draw(renderData.texture, renderData._destRect, renderData.sourceRect, color * opacity, rotation, renderData.origin, SpriteEffects.None, 0f); 
            if (currentDash != null)
                currentDash.Draw(renderData, moveData);

            foreach (Ability abil in abilities)
            {
                if (abil != null)
                    abil.Draw(1.0f);
            }


            Hitbox.DrawDebug();

        }

        public override void DrawEx(float opacity)
        {
            float percent = dashCooldown.lerpValue;
            staminaRect.Width = (int)(staminaBarRect.Width * percent);
            Game1.self.spriteBatch.Draw(Square, staminaBarRect, Color.DarkGray* opacity);
            Game1.self.spriteBatch.Draw(Square, staminaRect, Color.LightGray * opacity);

            healthBar.Width = (int) (healthData.lerpValue * HEALTHWIDTH);
            Game1.self.spriteBatch.Draw(Square, healthBarRect, Color.Red * opacity);
            Game1.self.spriteBatch.Draw(Square, healthBar, Color.Green * opacity);

            manaBar.Width = (int)(timeEnergy.lerpValue * MANAWIDTH);
            Game1.self.spriteBatch.Draw(Square, manaBarRect, Color.OrangeRed* opacity);
            Game1.self.spriteBatch.Draw(Square, manaBar, Color.Orange * opacity);

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
            healthData.Damage(cause, amount);
            return;
        }

        public override Entity Clone()
        {
            return this;
        }

        public override void ApplyEffect(Effect effect)
        {
            return;
        }

        public override void OnCollision()
        {
            throw new NotImplementedException();
        }
    }
}
