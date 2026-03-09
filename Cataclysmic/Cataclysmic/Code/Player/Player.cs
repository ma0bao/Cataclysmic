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
        Texture2D Square;

        Rectangle staminaBarRect;
        Rectangle staminaRect;

        //Healthbar
        Rectangle healthBarRect;
        Rectangle healthBar;
        int HEALTHWIDTH = 20;

        float angle;

        //Movement Display 
        bool isVisible = true;

        IDash currentDash;
        EventTimer dashCooldown;

        //Abilities
        LinkedList<Ability> abilities;
        Dictionary<string, EventTimer> abilityCooldowns;

        public Player(Rectangle _destRect)
        {
            LoadContent();
            renderData = new RenderComponent(Game1.texture_playerIdle, _destRect);
            moveData = new MoveComponent();
            healthData = new HealthComponent(10);

            // Setup idle animation 32x32 2 frames
            renderData.SetupAnimation(32, 32, 2);
            renderData.Play();

            dashCooldown = new EventTimer(.5f);
            staminaBarRect = new Rectangle(5, 5, 200, 15);
            staminaRect = new Rectangle(5, 5, 1, 15);
            healthBarRect = new Rectangle(305, 5, healthData.maxHealth * HEALTHWIDTH, 15);
            healthBar = new Rectangle(305, 5, healthData.currentHealth * HEALTHWIDTH, 15);
            dashCooldown.Unpause();
            angle = 0.0f;

            Hitbox = CollisionComponent.CreateRect(renderData.Position, _destRect.Width - 10, _destRect.Height - 30);

            abilities = new LinkedList<Ability>();

            // Add abilities here: (start ready to use)
            abilityCooldowns = new Dictionary<string, EventTimer>();
            abilityCooldowns["Revolver"] = new EventTimer(Revolver.COOLDOWN);
            abilityCooldowns["CrackleBurst"] = new EventTimer(CrackleBurst.COOLDOWN);
            foreach (EventTimer cd in abilityCooldowns.Values) cd.Done = true;
        }

        // check if ability is off cooldown, if so, restart cooldown and return true
        public bool TryUseAbility(string abilityName)
        {
            EventTimer timer = abilityCooldowns[abilityName];
            if (timer.Done)
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

            float directionX = Game1.self.cursors[0].Position.X - (renderData.Position.X);
            float directionY = Game1.self.cursors[0].Position.Y - (renderData.Position.Y);
            angle = (float)(Math.Atan2(directionY, directionX) + (Math.PI * 0.5f));

            // Collision
            #region 
            renderData.Position += moveData.velocity * moveData.deltaTime * moveData.speedModifiers;
            //Position = new Vector2(MathHelper.Clamp(Position.X, 0, Game1.WIDTH), MathHelper.Clamp(Position.Y, 0, Game1.HEIGHT));
            float newX = renderData.Position.X;
            if (renderData.Position.X > Game1.WIDTH - renderData.DestRect.Width / 2)
            {
                moveData.velocity.X = -Math.Abs(moveData.velocity.X);
                newX = Game1.WIDTH - renderData.DestRect.Width / 2;
            }
            else if (renderData.Position.X - renderData.DestRect.Width / 2 < 0)
            {
                moveData.velocity.X = Math.Abs(moveData.velocity.X);
                newX = renderData.DestRect.Width / 2;
            }

            float newY = renderData.Position.Y;
            if (renderData.Position.Y > Game1.HEIGHT - renderData.DestRect.Height / 2)
            {
                moveData.velocity.Y = -Math.Abs(moveData.velocity.Y);
                newY = Game1.HEIGHT - renderData.DestRect.Height / 2;
            }
            else if (renderData.Position.Y - renderData.DestRect.Height / 2 < 0)
            {
                moveData.velocity.Y = Math.Abs(moveData.velocity.Y);
                newY = renderData.DestRect.Height / 2;
            }

            renderData.Position = new Vector2(newX, newY);
            #endregion

            // Attacks/Abilities
            if (Game1.MS.LeftButton == ButtonState.Pressed)
            {
                if (TryUseAbility("Revolver"))
                    abilities.AddFirst(new Revolver(renderData.Position, angle));
            }

            if (Game1.self.KB.IsKeyDown(Keys.Q) && !Game1.self.oldKB.IsKeyDown(Keys.Q))
            {
                if (TryUseAbility("CrackleBurst"))
                    abilities.AddFirst(new CrackleBurst(renderData.Position, angle));
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
            if (GamePad.GetState(PlayerIndex.One).IsConnected)
                input = new Vector2(gpInput.ThumbSticks.Left.X, -gpInput.ThumbSticks.Left.Y);
            else
            {
                KeyboardState ks = Keyboard.GetState();
                if (ks.IsKeyDown(Game1.player1_moveUp))
                    input.Y -= 1;
                if (ks.IsKeyDown(Game1.player1_moveDown))
                    input.Y += 1;
                if (ks.IsKeyDown(Game1.player1_moveLeft))
                    input.X -= 1;
                if (ks.IsKeyDown(Game1.player1_moveRight))
                    input.X += 1;
            }

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
            

            
            Game1.self.spriteBatch.Draw(renderData.texture, renderData._destRect, renderData.sourceRect, color * opacity, rotation, renderData.origin, SpriteEffects.None, 1.0f); 
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
            Game1.self.spriteBatch.Draw(Square, staminaBarRect, Color.Red * opacity);
            Game1.self.spriteBatch.Draw(Square, staminaRect, Color.Orange * opacity);

            healthBar.Width = healthData.currentHealth * HEALTHWIDTH;
            Game1.self.spriteBatch.Draw(Square, healthBarRect, Color.Red * opacity);
            Game1.self.spriteBatch.Draw(Square, healthBar, Color.Green * opacity);

        }




        override public Boolean IsAlive() {
            return true;
        }

        public override void Damage(Entity cause, int amount)
        {
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

    }
}
