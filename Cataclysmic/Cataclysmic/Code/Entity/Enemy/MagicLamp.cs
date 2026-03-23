using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public class MagicLamp : Enemy
    {
        class Sand
        {
            EventTimer death = new EventTimer(8);
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
                moveData = new MoveComponent(300, 2000, 150*frictionMultiplier);
                renderData.color = Color.Red;
                moveData.velocity = velocity;
                randomOffset.X = Game1.rand.Next(-MAX_OFFSET, MAX_OFFSET + 1);
                randomOffset.Y = Game1.rand.Next(-MAX_OFFSET, MAX_OFFSET + 1);
                if (moveData.velocity.Length() > 10)
                    moveData.velocity += randomOffset;
            }

            public void IsColliding() //Will take environment as a parameter
            {
                throw new NotImplementedException();
            }

            public void OnCollision(Entity cause) 
            {
                throw new NotImplementedException();
            }

            public void Update(GameTime gameTime)
            {
                moveData.deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
                renderData.color.A = (byte)MathHelper.Lerp(255, 0, death.lerpValue);
                sandHitbox.UpdatePosition(renderData.Position);
                float depth;
                Vector2 normal;
                if (sandHitbox.Intersects(Game1.players[0].Hitbox, out depth, out normal))
                {
                    Game1.players[0].Damage(null, 5);
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

        Player[] players;
        Player targetedPlayer;

        const int ANGER_DISTANCE = 50;
        const int FOLLOW_DISTANCE = 300;
        const int AGRO_DISTANCE = 300;
        const float SHAKE_TIME = .8f;
        const float CONE_WIDTH_DEGREES = 45;
        const int PROJECTILES_PER_FRAME = 5;

        EventTimer shakeTimer;

        float sprayVelocityLerp = 0;
        

        Vector2 sprayRange = new Vector2(150, 150);
        const float SANDSPEED = 500f;

        const int MAX_SHAKE_AMT = 2;

        AttackState currentState = AttackState.Wander;

        Queue<Sand> sands = new Queue<Sand>();

        float frictionMultiplier = 1;

        public MagicLamp(Rectangle destRect, Player[] targets) : base(Game1.texture_flyingLamp, destRect)
        {
            players = targets;
            SetNewTargetPosition(renderData.GetRandomPoint());
            moveData.maxSpeed = 500;
            moveData.acceleration = 4000f;
            healthData = new HealthComponent(50);
        }
        public override void Update(GameTime gameTime)
        {
            #region Get Target Position

            if (currentState == AttackState.Wander || currentState == AttackState.Run)
            {
                moveData.maxSpeed = 500f;
                renderData.color = Color.AliceBlue;
                if (targetPos == Vector2.Zero)
                    SetNewTargetPosition(renderData.GetRandomPoint());
            }
            else if (currentState == AttackState.Follow || currentState == AttackState.Charge)
            {
                moveData.maxSpeed = 150;
                renderData.color = Color.Red;
                SetNewTargetPosition(targetedPlayer.renderData.Position);
            }
            else if (currentState == AttackState.Spray)
            {
                Vector2 min = targetPos - sprayRange;
                Vector2 max = targetPos + sprayRange;
                SetNewTargetPosition(Vector2.Lerp(min, max, sprayVelocityLerp));
            }

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
                foreach (Player p in players)
                {
                    if (p == null)
                        continue;
                    if (renderData.GetDistanceToTarget(p.renderData.Position) < AGRO_DISTANCE)
                    {
                        currentState = AttackState.Follow;
                        targetedPlayer = p;
                    }
                }
            }
            else if (currentState == AttackState.Follow)
            {
                IncreaseVelocity();
                if (renderData.GetDistanceToTarget(targetedPlayer.renderData.Position) > FOLLOW_DISTANCE)
                    base.Update(gameTime);
                if (renderData.GetDistanceToTarget(targetedPlayer.renderData.Position) < ANGER_DISTANCE)
                    currentState = AttackState.Charge;
                if (Game1.rand.Next(650) == 0)
                {
                    currentState = AttackState.Charge;
                }
            }
            else if (currentState == AttackState.Charge)
            {
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
                Vector2 baseVelocity = moveData.velocity;
                baseVelocity.Normalize();
                baseVelocity *= SANDSPEED;

                float x = Game1.rand.Next(-MAX_SHAKE_AMT-1, MAX_SHAKE_AMT + 2);
                float y = Game1.rand.Next(-MAX_SHAKE_AMT-1, MAX_SHAKE_AMT + 2);
                Vector2 shake = new Vector2(x, y);
                renderData.Position += shake;

                float coneSize = MathHelper.ToRadians(CONE_WIDTH_DEGREES);

                float randomAngle = (float)(Game1.rand.NextDouble() * coneSize) - (coneSize / 2f);

                float cos = (float)Math.Cos(randomAngle);
                float sin = (float)Math.Sin(randomAngle);

                Vector2 rotatedVelocity = new Vector2(
                    baseVelocity.X * cos - baseVelocity.Y * sin,
                    baseVelocity.X * sin + baseVelocity.Y * cos
                    );

                for (int i = 0; i < PROJECTILES_PER_FRAME; i++)
                {
                    sands.Enqueue(new Sand(frictionMultiplier, renderData.Position, rotatedVelocity));
                }

                frictionMultiplier += 0.01f;
                if (frictionMultiplier >= 2f)
                {
                    frictionMultiplier = 1;
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
            renderData.rotation = renderData.GetRotationToTarget(renderData.Position + moveData.velocity);

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
            //base.Draw(opacity);
        }

        public override bool IsAlive()
        {
            return base.IsAlive() && sands.Count == 0;
        }
    }
}
