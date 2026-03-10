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
        EventTimer shakeTimer = new EventTimer(.4f);

        Rectangle top = new Rectangle(Game1.BOUNDS.X, Game1.BOUNDS.Y, Game1.BOUNDS.Width, 150);
        Rectangle bottom = new Rectangle(0, Game1.BOUNDS.Bottom-150, Game1.BOUNDS.Width, 150);

        Rectangle currentRegion;
        Rectangle oppisiteRegion;

        Player[] players;
        Player targetedPlayer;
        Player lastTargetedPlayer;

        const int AGRO_DISTANCE = 300;

        const int MAX_SHAKE_AMT = 2;

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
                renderData.color = Color.AliceBlue;
                slowRadius = 150;
                turnSpeed = 2000f;
                moveData.maxSpeed = 500f;
                if (renderData.GetDistanceToTarget(targetPos) < distanceToBeAtTarget)
                {
                    SetNewTargetPosition(renderData.GetRandomPoint());
                }
            }
            else if (currentState == AttackState.Track)
            {
                slowRadius = 30;
                turnSpeed = 2000f;
                moveData.maxSpeed = 500f;

                SetNewTargetPosition(new Vector2(lastTargetedPlayer.renderData.Position.X, currentRegion.Center.Y));
            }
            else if (currentState == AttackState.Swipe)
            {
                SetNewTargetPosition(targetedPlayer.renderData.Position);
            }
            else if (currentState == AttackState.Dash)
            {
                SetNewTargetPosition(new Vector2(targetPos.X, oppisiteRegion.Center.Y));
                renderData.color = Color.Red;
                moveData.maxSpeed = 90000f;
                turnSpeed = 5000f;
                if (renderData.hitBox.Intersects(oppisiteRegion))
                    currentState = AttackState.Wander;
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
                if (Game1.rand.Next(200) == 0)
                {
                    shakeTimer.Restart();
                    currentState = AttackState.Shake;
                }
            }
            else if (currentState == AttackState.Dash)
            {
                if (lastTargetedPlayer.renderData.hitBox.Intersects(Swipe()))
                    lastTargetedPlayer.healthData.Damage(this, 3);
                base.Update(gameTime);
            }
            else if (currentState == AttackState.Shake)
            {

                float x = Game1.rand.Next(-MAX_SHAKE_AMT, MAX_SHAKE_AMT + 1);
                float y = Game1.rand.Next(-MAX_SHAKE_AMT, MAX_SHAKE_AMT + 1);
                Vector2 shake = new Vector2(x, y);
                renderData.Position += shake;

                if (shakeTimer.Done)
                {
                    if (currentRegion.Equals(top))
                        oppisiteRegion = bottom;
                    else if (currentRegion.Equals(bottom))
                        oppisiteRegion = top;
                    currentState = AttackState.Dash;
                }

                shakeTimer.Update();
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
