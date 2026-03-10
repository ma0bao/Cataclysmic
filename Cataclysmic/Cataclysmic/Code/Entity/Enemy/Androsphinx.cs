using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public class Androsphinx : Enemy
    {

        enum AttackState
        {
            Wander = 0,
            Track = 1,
            Shake = 2,
            Dash = 3,
            Swipe = 4
        }
        AttackState currentState = AttackState.Wander;

        EventTimer agroCooldown = new EventTimer(2);
        EventTimer attackCooldown = new EventTimer(1);
        EventTimer shakeTimer = new EventTimer(.1f);

        Rectangle top = new Rectangle(Game1.BOUNDS.X, Game1.BOUNDS.Y, Game1.BOUNDS.Width, 150);
        Rectangle bottom = new Rectangle(0, Game1.BOUNDS.Bottom-150, Game1.BOUNDS.Width, 150);

        Rectangle currentRegion;

        Player[] players;
        Player targetedPlayer;
        Player lastTargetedPlayer;

        Vector2 newTarget;

        const int AGRO_DISTANCE = 300;
        const int ATTACK_DISTANCE = 25;

        public Androsphinx(Rectangle destRect, Player[] targets) : base(Game1.texture_player, destRect)
        {
            currentRegion = top;
            players = targets;
            lastTargetedPlayer = players[0];
            attackCooldown.Unpause();
            moveData.maxSpeed = 500f;
        }

        public override void Update(GameTime gameTime)
        {
            #region Get Target Based On State

            if (currentState == AttackState.Wander)
            {
                slowRadius = 150;
                turnSpeed = 2000f;
                moveData.maxSpeed = 500f;
                if (renderData.GetDistanceToTarget(targetPos) < distanceToBeAtTarget)
                    SetNewTargetPosition(renderData.GetRandomPoint());
            }
            else if (currentState == AttackState.Track)
            {
                slowRadius = 30;
                turnSpeed = 2500;
                moveData.maxSpeed = 500f;

                SetNewTargetPosition(new Vector2(lastTargetedPlayer.renderData.Position.X, currentRegion.Center.Y));
            }
            else if (currentState == AttackState.Swipe)
            {
                SetNewTargetPosition(targetedPlayer.renderData.Position);
            }
            else if (currentState == AttackState.Dash)
            {
                moveData.maxSpeed = 90000f;
                turnSpeed = 5000f;
                if (renderData.GetDistanceToTarget(targetPos) < distanceToBeAtTarget+50)
                    currentState = AttackState.Track;
            }

            #endregion

            #region Update Based On State
            if (currentState == AttackState.Wander)
            {
                base.Update(gameTime);
                foreach (Player p in players)
                {
                    if (p == null)
                        continue;
                    if (renderData.GetDistanceToTarget(p.renderData.Position) < AGRO_DISTANCE)
                    {
                        targetedPlayer = p;
                        lastTargetedPlayer = p;
                        break;
                    }
                    targetedPlayer = null;
                }

                if (targetedPlayer != null && agroCooldown.Done)
                {
                    agroCooldown.Restart();
                    currentState = AttackState.Swipe;
                }

                if (Game1.rand.Next(0, 600) == 0)
                {
                    currentState = AttackState.Track;
                    if (lastTargetedPlayer.renderData.Position.Y < Game1.HEIGHT / 2)
                        currentRegion = bottom;
                    else
                        currentRegion = top;
                }
            }
            else if (currentState == AttackState.Swipe)
            {
                base.Update(gameTime);
                if (targetedPlayer.renderData.hitBox.Intersects(Swipe()) && attackCooldown.Done)
                {
                    targetedPlayer.healthData.Damage(this, 5);
                    attackCooldown.Restart();
                }

                attackCooldown.Update();

                if (agroCooldown.Done)
                {
                    currentState = AttackState.Wander;
                    SetNewTargetPosition(renderData.GetRandomPoint());
                    agroCooldown.Restart();
                }

            }
            else if (currentState == AttackState.Track)
            {
                base.Update(gameTime);
                Vector2 center = new Vector2(Game1.WIDTH/2, Game1.HEIGHT / 2);
                if (currentRegion.Equals(top))
                {
                    if (lastTargetedPlayer.renderData.Position.Y < center.Y)
                    {
                        currentRegion = bottom;
                        currentState = AttackState.Dash;
                        SetNewTargetPosition(new Vector2(targetPos.X, currentRegion.Center.Y));
                    }
                }
                else
                {
                    if (lastTargetedPlayer.renderData.Position.Y > center.Y)
                    {
                        currentRegion = top;
                        currentState = AttackState.Dash;
                        SetNewTargetPosition(new Vector2(targetPos.X, currentRegion.Center.Y));
                    }
                }
            }
            else if (currentState == AttackState.Dash)
            {
                if (lastTargetedPlayer.renderData.hitBox.Intersects(Swipe()))
                    lastTargetedPlayer.healthData.Damage(this, 3);
                base.Update(gameTime);
            }
            #endregion
            //ADD SHAKE
            agroCooldown.Update();
        }

        public override void Draw(float opacity)
        {
            //if(currentState == AttackState.Swipe)
                //Game1.self.spriteBatch.Draw(Game1.texture_blank, Swipe(), Color.Red);
            base.Draw(opacity);
        }

        public Rectangle Swipe()
        {
            return new Rectangle(renderData.hitBox.X-20, renderData.hitBox.Y-20, renderData.hitBox.Width+40, renderData.hitBox.Height + 40 );
        }
    }

}
