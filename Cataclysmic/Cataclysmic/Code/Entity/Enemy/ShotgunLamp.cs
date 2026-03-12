using Microsoft.Xna.Framework;
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
            Vector2 randomOffset;
            const int MAX_OFFSET = 10;
            const int SIZE = 5;

            public Sand(float frictionMultiplier, Vector2 Position, Vector2 velocity)
            {
                renderData = new RenderComponent(Game1.texture_blank, new Rectangle((int)Position.X, (int)Position.Y, SIZE, SIZE));
                moveData = new MoveComponent(300, 2000, 400 * frictionMultiplier);
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

        const int ANGER_DISTANCE = 20;
        const int FOLLOW_DISTANCE = 200;
        const int AGRO_DISTANCE = 300;
        const float SHAKE_TIME = .7f;
        const float CONE_WIDTH_DEGREES = 30;
        const int PROJECTILES_PER_FRAME = 20;

        EventTimer shakeTimer;

        Vector2 sprayRange = new Vector2(150, 150);
        const float SANDSPEED = 1350f;

        const int MAX_SHAKE_AMT = 2;

        Vector2 baseVelocity;

        AttackState currentState = AttackState.Wander;
        Queue<Sand> sands = new Queue<Sand>();

        float frictionMultiplier = 5;

        public ShotgunLamp(Rectangle destRect, Player[] targets) : base(Game1.texture_player, destRect)
        {
            players = targets;
            SetNewTargetPosition(renderData.GetRandomPoint());
            moveData.maxSpeed = 500;
            moveData.acceleration = 4000f;
            healthData = new HealthComponent(50);
            //slowRadius = 0;
        }
        public override void Update(GameTime gameTime)
        {
            #region Get Target Position

            if (currentState == AttackState.Wander || currentState == AttackState.Run)
            {
                renderData.rotation = renderData.GetRotationToTarget(renderData.Position + moveData.velocity);
                moveData.maxSpeed = 500f;
                renderData.color = Color.AliceBlue;
                if (targetPos == Vector2.Zero)
                    SetNewTargetPosition(renderData.GetRandomPoint());
            }
            else if (currentState == AttackState.Follow)
            {
                moveData.maxSpeed = 500f;
                renderData.color = Color.Red;

                float distance = renderData.GetDistanceToTarget(targetedPlayer.renderData.Position);
                float diffX = renderData.Position.X - targetedPlayer.renderData.Position.X;
                float diffY = renderData.Position.Y - targetedPlayer.renderData.Position.Y;

                float ratio = distance / FOLLOW_DISTANCE;

                float newDiffX = diffX / ratio;
                float newDiffY = diffY / ratio;

                SetNewTargetPosition(new Vector2(targetedPlayer.renderData.Position.X + newDiffX, targetedPlayer.renderData.Position.Y + newDiffY));
                renderData.rotation = renderData.GetRotationToTarget(targetedPlayer.renderData.Position);
            }
            else if (currentState == AttackState.Spray || currentState == AttackState.Charge)
            {
                SetNewTargetPosition(targetedPlayer.renderData.Position);
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
                if(renderData.GetDistanceToTarget(targetPos) > distanceToBeAtTarget)
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

                for (int i = 0; i < PROJECTILES_PER_FRAME; i++)
                {
                    baseVelocity = targetedPlayer.renderData.Position - renderData.Position;

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
            collision.DrawDebug();
            base.Draw(opacity);
            foreach (Sand s in sands)
                s.Draw();
        }

        public override bool IsAlive()
        {
            return base.IsAlive() && sands.Count == 0;
        }
    }
}
