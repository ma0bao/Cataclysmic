using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public class ShotgunLamp : Enemy
    {
        class Sand
        {
            EventTimer death = new EventTimer(3);
            RenderComponent renderData;
            MoveComponent moveData;
            CollisionComponent sandHitbox;
            Vector2 randomOffset;
            const int MAX_OFFSET = 10;
            const int SIZE = 5;

            public Sand(float frictionMultiplier, Vector2 Position, Vector2 velocity)
            {
                sandHitbox = CollisionComponent.CreateRect(Position, SIZE, SIZE);
                renderData = new RenderComponent(Game1.texture_blank, new Rectangle((int)Position.X, (int)Position.Y, SIZE, SIZE));
                moveData = new MoveComponent(300, 2000, 400 * frictionMultiplier);
                //renderData.color = Color.Red;
                renderData.color = new Color(119, 96, 66);
                moveData.velocity = velocity;
                randomOffset.X = Game1.rand.Next(-MAX_OFFSET, MAX_OFFSET + 1);
                randomOffset.Y = Game1.rand.Next(-MAX_OFFSET, MAX_OFFSET + 1);
                if (moveData.velocity.Length() > 10)
                    moveData.velocity += randomOffset;
            }

            public void Update(GameTime gameTime)
            {
                moveData.deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                renderData.color.A = (byte)MathHelper.Lerp(255, 200, death.lerpValue);
                sandHitbox.UpdatePosition(renderData.Position);
                float depth;
                Vector2 normal;
                if (sandHitbox.Intersects(Game1.player.Hitbox, out depth, out normal))
                {
                    Game1.player.Damage(null, 5);
                }

                renderData.Position += moveData.velocity * moveData.deltaTime;
                moveData.ApplyFriction();

                death.Update();
            }

            public bool IsAlive()
            {
                return !death.Done;
            }

            public void Draw()
            {
                renderData.DefualtDraw();
                sandHitbox.DrawDebug();
            }
        }

        enum AttackState
        {
            Wander = 0,
            Follow = 1,
            Charge = 2,
            Spray = 3,
            Run = 4
        }

        Player player;
        Player targetedPlayer;

        const int ANGER_DISTANCE = 20;
        const int FOLLOW_DISTANCE = 200;
        const int AGRO_DISTANCE = 300;
        const float SHAKE_TIME = .5f;
        const float CONE_WIDTH_DEGREES = 30;
        const int PROJECTILES_PER_FRAME = 20;
        const int MAX_COOLDOWN_FRAMES = 240;
        const int MIN_COOLDOWN_FRAMES = 60;

        int cooldown_frames;

        EventTimer shakeTimer;

        Vector2 sprayRange = new Vector2(150, 150);
        const float SANDSPEED = 1350f;

        const int MAX_SHAKE_AMT = 2;

        Vector2 baseVelocity;

        AttackState currentState = AttackState.Wander;
        Queue<Sand> sands = new Queue<Sand>();

        float frictionMultiplier = 5;

        const int WIDTH = 40;
        const int HEIGHT = 40;
        const int HITBOX_WIDTH = 40;
        const int HITBOX_HEIGHT = 40;
        public ShotgunLamp(Vector2 position) : base(Game1.texture_flyingLamp, new Rectangle((int)position.X, (int)position.Y, WIDTH, HEIGHT), HITBOX_WIDTH, HITBOX_HEIGHT)
        {
            player = Game1.player;

            SetNewTargetPosition(renderData.GetRandomPoint());
            moveData.maxSpeed = 500;
            moveData.acceleration = 4000f;
            healthData = new HealthComponent(50);
            //slowRadius = 0;
            cooldown_frames = Game1.rand.Next(MIN_COOLDOWN_FRAMES, MAX_COOLDOWN_FRAMES);
            staggerResistance = 0.8f;

            bloodData.Tint = Color.SandyBrown;
            bloodData.BaseSize = 5;
   
        }

        public override void Stagger(float secondsToStagger, bool UseResistance = true)
        {
            if (currentState == AttackState.Charge || currentState == AttackState.Spray)
            {
                currentState = AttackState.Wander;
                shakeTimer = null;
                frictionMultiplier = 5;
            }
            base.Stagger(secondsToStagger, UseResistance);
        }
        public override void Update(GameTime gameTime)
        {
            UpdateTimers();

            #region Get Target Position

            if (currentState == AttackState.Wander || currentState == AttackState.Run)
            {
                renderData.rotation = renderData.GetRotationToTarget(renderData.Position + moveData.velocity);
                moveData.maxSpeed = 500f;
                //renderData.color = Color.AliceBlue;
                if (targetPos == Vector2.Zero)
                    SetNewTargetPosition(renderData.GetRandomPoint());
            }
            else if (currentState == AttackState.Follow)
            {
                moveData.maxSpeed = 500f;
                //renderData.color = Color.Red;

                float distance = renderData.GetDistanceToTarget(targetedPlayer.renderData.Position);
                float diffX = renderData.Position.X - targetedPlayer.renderData.Position.X;
                float diffY = renderData.Position.Y - targetedPlayer.renderData.Position.Y;

                float ratio = distance / FOLLOW_DISTANCE;

                float newDiffX = diffX / ratio;
                float newDiffY = diffY / ratio;

                SetNewTargetPosition(new Vector2(targetedPlayer.renderData.Position.X + newDiffX, targetedPlayer.renderData.Position.Y + newDiffY));
                renderData.rotation = renderData.GetRotationToTarget(targetedPlayer.renderData.Position);
            }
            //else if (currentState == AttackState.Spray || currentState == AttackState.Charge)
            //{
            //    SetNewTargetPosition(targetedPlayer.renderData.Position);
            //}

            #endregion


            #region Update Based On State
            
            if (currentState == AttackState.Run)
            {
                base.Update(gameTime);
                if (IsAtNode())
                {
                    targetPos = Vector2.Zero;
                    currentState = AttackState.Wander;
                }
            }
            else if (currentState == AttackState.Wander)
            {
                base.Update(gameTime);
                if (IsAtNode() || targetPos == Vector2.Zero)
                {
                    SetNewTargetPosition(renderData.GetRandomPoint());
                }
                
                    if (renderData.GetDistanceToTarget(player.renderData.Position) < AGRO_DISTANCE)
                    {
                        currentState = AttackState.Follow;
                        targetedPlayer = player;
                    }
                
            }
            else if (currentState == AttackState.Follow)
            {
                cooldown_frames--;
                if (renderData.GetDistanceToTarget(targetPos) > distanceToBeAtTarget)
                    base.Update(gameTime);
                if (renderData.GetDistanceToTarget(targetedPlayer.renderData.Position) < ANGER_DISTANCE)
                {
                    currentState = AttackState.Charge;
                    SetNewTargetPosition(targetedPlayer.renderData.Position);
                }
                if (cooldown_frames <= 0) //Keyboard.GetState().IsKeyDown(Keys.L))
                {
                    currentState = AttackState.Charge;
                    cooldown_frames = Game1.rand.Next(MIN_COOLDOWN_FRAMES, MAX_COOLDOWN_FRAMES);
                    SetNewTargetPosition(targetedPlayer.renderData.Position);
                }
            }
            else if (currentState == AttackState.Charge)
            {
                renderData.rotation = renderData.GetRotationToTarget(targetPos);
                IncreaseVelocity();
                if (shakeTimer == null)
                    shakeTimer = new EventTimer(SHAKE_TIME);

                float x = Game1.rand.Next(-MAX_SHAKE_AMT, MAX_SHAKE_AMT + 1);
                float y = Game1.rand.Next(-MAX_SHAKE_AMT, MAX_SHAKE_AMT + 1);
                Vector2 shake = new Vector2(x, y);
                renderData.Position += shake;
                shakeTimer.Update();

                if (shakeTimer.Done)
                {
                    shakeTimer = null;
                    currentState = AttackState.Spray;
                    
                }
            }
            else if (currentState == AttackState.Spray)
            {
                renderData.rotation = renderData.GetRotationToTarget(targetPos);
                IncreaseVelocity();
                float x = Game1.rand.Next(-MAX_SHAKE_AMT - 1, MAX_SHAKE_AMT + 2);
                float y = Game1.rand.Next(-MAX_SHAKE_AMT - 1, MAX_SHAKE_AMT + 2);
                Vector2 shake = new Vector2(x, y);

                renderData.Position += shake;
                UpdatePos(-2);
                Game1.sfx_sandBurst1.Play(Game1.volume, -0.2f + (float) Game1.rand.NextDouble() * 0.4f, 0);
                for (int i = 0; i < PROJECTILES_PER_FRAME; i++)
                {
                    baseVelocity = targetPos - renderData.Position;

                    if (baseVelocity.LengthSquared() > 0)
                        baseVelocity.Normalize();

                    baseVelocity *= SANDSPEED;


                    float coneSize = MathHelper.ToRadians(CONE_WIDTH_DEGREES);

                float randomAngle = (float)(Game1.rand.NextDouble() * coneSize) - (coneSize / 2f);

                float cos = (float)Math.Cos(randomAngle);
                float sin = (float)Math.Sin(randomAngle);

                Vector2 rotatedVelocity = new Vector2(
                    baseVelocity.X * cos - baseVelocity.Y * sin,
                    baseVelocity.X * sin + baseVelocity.Y * cos
                    );

                
                    sands.Enqueue(new Sand(frictionMultiplier, renderData.Position, rotatedVelocity));
                }

                frictionMultiplier += .2f;
                if (frictionMultiplier >= 6f)
                {
                    frictionMultiplier = 5;
                    currentState = AttackState.Run;
                    SetNewTargetPosition(renderData.GetRandomPoint());
                }
            }
            #endregion

            foreach (Sand s in sands)
                s.Update(gameTime);

            while (sands.Count > 0 && !sands.Peek().IsAlive())
            {
                sands.Dequeue();
            }
        }

        public bool IsAtNode()
        {
            return renderData.GetDistanceToTarget(targetPos) < distanceToBeAtTarget;
        }

        public override void Draw(float opacity)
        {
            if (healthData.invincible)
            {
                renderData.DrawFlash();
            }
            // base.Draw(opacity);
            // Test for Rotation : Game1.self.spriteBatch.DrawString(Game1.font_credits, "" + renderData.rotation, renderData.Position, Color.White);
            if (renderData.rotation > Math.PI || renderData.rotation < 0)
            {
                renderData.effects = Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipVertically;
            }
            else
            {
                renderData.effects = Microsoft.Xna.Framework.Graphics.SpriteEffects.None;
            }
            Game1.self.spriteBatch.Draw(renderData.texture, renderData.DestRect, renderData.sourceRect, renderData.color * opacity, renderData.rotation - MathHelper.ToRadians(90), renderData.origin, renderData.effects, renderData.layerDepth);
            foreach (Sand s in sands)
                s.Draw();
            collision.DrawDebug();
            
        }

        public override bool IsAlive()
        {
            return base.IsAlive();
        }
    }
}
