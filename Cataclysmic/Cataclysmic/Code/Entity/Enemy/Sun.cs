using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    class Sun : Enemy
    {
        const   int WIDTH = 150;
        const   int HEIGHT = 150;

        EventTimer AttackCooldown;

        List<WaveShot> projectiles = new List<WaveShot>();

        public enum  AttackState {
            Wander,
            Spray,
        }
        
        Player target;

        AttackState  currentState;

        bool playerOnLeft => target.renderData.Position.X < Game1.BOUNDS.Center.X;
        Rectangle half
        {
            get
            {
                Rectangle bounds = Game1.BOUNDS;

                if (playerOnLeft)
                    return new Rectangle(bounds.Left, bounds.Top, bounds.Width / 2, bounds.Height);
                else
                return new Rectangle(bounds.Center.X, bounds.Top, bounds.Width / 2, bounds.Height);
            }
        }

        Vector2 rayTarget;
        Vector2 minRayTarget;
        Vector2 maxRayTarget; 

        EventTimer shootTimer = new EventTimer(3);

        public Sun(Vector2 position) : base(Game1.texture_player, new Rectangle((int)position.X, (int)position.Y, WIDTH, HEIGHT), WIDTH, HEIGHT)
        {
            staggerResistance = 0.0f;
            healthData = new HealthComponent(200);
            target = Game1.player;
            AttackCooldown = new EventTimer(6);

            bloodData.Tint = Color.Yellow;
            bloodData.baseSize = 14;

        }

        public void SetStateToWander()
        {
            AttackCooldown.Restart();
            SetNewTargetPosition(renderData.GetRandomPoint(half));
            currentState = AttackState.Wander;
        }

        public void SetStateToSpray()
        {
            int minX, maxX, y;
            if (target.renderData.Position.Y > half.Center.Y)
            {
                SetNewTargetPosition(new Vector2(half.Center.X, half.Top));
                y = half.Bottom;
            }
            else
            {
                SetNewTargetPosition(new Vector2(half.Center.X, half.Bottom));
                y = half.Top;
            }
            if (playerOnLeft)
            {
                minX = half.Left - 50;
                maxX = half.Right+ 50;
            }
            else
            {
                minX = half.Right + 50;
                maxX = half.Left - 50;
            }

            minRayTarget = new Vector2(minX, y);
            maxRayTarget = new Vector2(maxX, y);

            currentState = AttackState.Spray;
            shootTimer.Restart();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateTimers();

            #region Update Target Based On State
            if (currentState == AttackState.Wander)
            {
                if(IsAtTarget() || targetPos == Vector2.Zero || !Game1.BOUNDS.Contains((int)targetPos.X, (int)targetPos.Y))
                    SetNewTargetPosition(renderData.GetRandomPoint(half));
            }
            #endregion

            #region Update Position+Logic On State
            if (currentState == AttackState.Wander)
            {
                base.Update(gameTime);

                if (AttackCooldown.Done)
                {
                    SetStateToSpray();
                }

                AttackCooldown.Update();
            }
            if (currentState == AttackState.Spray)
            {
                renderData.color = Color.Red;
                base.Update(gameTime);

                if (IsAtTarget())
                {
                    rayTarget = Vector2.Lerp(minRayTarget, maxRayTarget, shootTimer.lerpValue);

                    FireWave(5);

                    if (shootTimer.Done)
                    {
                        SetStateToWander();
                    }
                    shootTimer.Update();
                }
            }
            #endregion

            foreach (WaveShot s in projectiles)
            {
                s.Update(gameTime);
                if (s.collisionData.Intersects(target.Hitbox, out _, out _))
                    target.Damage(this, 3);
            }



            projectiles.RemoveAll(p => !p.IsAlive());
        }

        public void FireWave(float amt = 10)
        {
            for (float i = 0; i < amt; i++)
            {
                Vector2 direction = renderData.GetDirectionToTarget(rayTarget);

                if (direction.LengthSquared() > 0)
                    direction.Normalize();

                float coneSize = MathHelper.ToRadians(15);

                float randomAngle = (float)(i / amt * coneSize) - (coneSize / 2f);
                //float randomAngle = (float)(Game1.rand.NextDouble() * coneSize) - (coneSize / 2f);

                float cos = (float)Math.Cos(randomAngle);
                float sin = (float)Math.Sin(randomAngle);

                Vector2 rotatedVelocity = new Vector2(
                    direction.X * cos - direction.Y * sin,
                    direction.X * sin + direction.Y * cos
                    );
                projectiles.Add(new WaveShot(renderData.Position, rotatedVelocity, 20));
            }
        }

        public override void Draw(float opacity)
        {
            foreach (WaveShot s in projectiles)
                s.Draw();

            base.Draw(opacity);
        }

    }

   
}
