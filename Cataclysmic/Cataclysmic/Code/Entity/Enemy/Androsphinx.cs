using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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

        Player player;
        Player targetedPlayer;
        Player lastTargetedPlayer;

        const int AGRO_DISTANCE = 300;

        const int MAX_SHAKE_AMT = 2;

        const int WIDTH = 48;
        const int HEIGHT = 64;
        const int HITBOX_WIDTH = 48;
        const int HITBOX_HEIGHT = 64;

        public Androsphinx(Vector2 position) : base(Game1.texture_meatballEgypt, new Rectangle((int)position.X, (int)position.Y, WIDTH, HEIGHT), HITBOX_WIDTH, HITBOX_HEIGHT)
        {
            staggerResistance = .8f;
            currentRegion = top;
            player = Game1.player;
            lastTargetedPlayer = player;
            attackCooldown.Unpause();
            moveData.maxSpeed = 500f;
            SetNewTargetPosition(renderData.GetRandomPoint());

            bloodData.BaseSize = 9;
 
        }

        public override void Update(GameTime gameTime)
        {
            UpdateTimers();

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
                moveData.maxSpeed = 9000f;
                turnSpeed = 5000f;
                if (currentRegion.Equals(top) && renderData.Position.Y > bottom.Top)
                {
                    agroCooldown.Restart();
                    currentState = AttackState.Wander;
                    //moveData.velocity = Vector2.One;
                    SetNewTargetPosition(renderData.GetRandomPoint());
                }
                if (currentRegion.Equals(bottom) && renderData.Position.Y < top.Bottom)
                {
                    agroCooldown.Restart();
                    currentState = AttackState.Wander;
                    //moveData.velocity = Vector2.One;
                    SetNewTargetPosition(renderData.GetRandomPoint());
                }

            }

            #endregion

            #region Update Based On State
            if (currentState == AttackState.Wander)
            {
                base.Update(gameTime);

                if (renderData.GetDistanceToTarget(player.renderData.Position) < AGRO_DISTANCE)
                {
                    targetedPlayer = player;
                    lastTargetedPlayer = player;
                }
                else {
                    targetedPlayer = null;
                }
                

                if (targetedPlayer != null && agroCooldown.Done)
                {
                    agroCooldown.Restart();
                    currentState = AttackState.Swipe;
                }

                if (Game1.rand.Next(0, 600) == 0 || Keyboard.GetState().IsKeyDown(Keys.K))
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
                    targetedPlayer.Damage(this, 5);
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
                if (Game1.rand.Next(500) == 0)
                {
                    shakeTimer.Restart();
                    currentState = AttackState.Shake;
                }
            }
            else if (currentState == AttackState.Dash)
            {
                if (lastTargetedPlayer.renderData.hitBox.Intersects(Swipe()))
                    lastTargetedPlayer.Damage(this, 15);
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

        public override void Stagger(float secondsToStagger, bool UseResistance = true)
        {
            if (currentState == AttackState.Shake || currentState == AttackState.Dash || currentState == AttackState.Swipe) currentState = AttackState.Wander;
            base.Stagger(secondsToStagger, UseResistance);
        }

        public override void Draw(float opacity)
        {
            if(currentState == AttackState.Swipe)
                Game1.self.spriteBatch.Draw(Game1.texture_blank, Swipe(), Color.Red); //REPLACE WITH ATTACK ANIMATION WHEN WE HAVE ART
            base.Draw(opacity);
        }

        public Rectangle Swipe()
        {
            return new Rectangle(renderData.hitBox.X-25, renderData.hitBox.Y-25, renderData.hitBox.Width+45, renderData.hitBox.Height + 45 );
        }
    }

}
