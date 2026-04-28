using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public class Apesh : Enemy
    {
        enum AttackState
        {
            Track,
            Cooldown,
            Slam,
            Spin,
            Run
        }

        AttackState currentState = AttackState.Track;

        Player player;

        EventTimer spinTimer;

        EventTimer timeToSpin;

        EventTimer cooldownTimer;

        EventTimer CrackTimer;
        Rectangle crackRect;

        const int WIDTH = 64;
        const int HEIGHT = 64;
        const int HITBOX_WIDTH = 64;
        const int HITBOX_HEIGHT = 64;

        CollisionComponent SlamHitbox;

        public Apesh(Vector2 position) : base(Game1.texture_apesh, new Rectangle((int)position.X, (int)position.Y, WIDTH, HEIGHT), HITBOX_WIDTH, HITBOX_HEIGHT)
        {
            staggerResistance = 0.1f;
            player = Game1.player;
            SetNewTargetPosition(player.renderData.Position);
            distanceToBeAtTarget = 100;
            spinTimer = new EventTimer(3);
            cooldownTimer = new EventTimer(.4f);
            timeToSpin = new EventTimer(1.5f);
            turnSpeed = 1000;
            moveData.maxSpeed = 200;
            spinTimer.Unpause();

            staggerResistance = .30f;

            CrackTimer = new EventTimer(2f);
            CrackTimer.Done = true;
            
        }

        public override void Update(GameTime gameTime)
        {
            UpdateTimers();

            
            

            #region Set Target Based On State
            if (currentState == AttackState.Track || currentState == AttackState.Spin)
            {
                SetNewTargetPosition(player.renderData.Position);
            }
            #endregion

            #region Update Based On State
            if (currentState == AttackState.Track)
            {
                base.Update(gameTime);
                if (IsAtTarget())
                {
                    cooldownTimer.Restart();
                    currentState = AttackState.Cooldown;
                }
                if (spinTimer.Done)
                {
                    currentState = AttackState.Spin;
                }
                spinTimer.Update();
            }
            else if (currentState == AttackState.Cooldown)
            {
                if (cooldownTimer.Done)
                    currentState = AttackState.Slam;
                cooldownTimer.Update();
            }
            else if (currentState == AttackState.Slam)
            {
                Slam();
                SetNewTargetPosition(renderData.GetRandomPoint());
                currentState = AttackState.Run;
            }
            else if (currentState == AttackState.Spin)
            {
                turnSpeed = 1800f;
                moveData.maxSpeed = 900f;
                slowRadius = 0;
                renderData.rotation += 8;  
                base.Update(gameTime);
                turnSpeed = 500;
                moveData.maxSpeed = 200;
                slowRadius = 150;

                Game1.sfx_spin1.Play(Game1.volume, -0.1f + (float) Game1.rand.NextDouble() * 0.2f, 0);
                if (Game1.timer % 3 == 0)
                {
                    Particle p = new Particle(renderData.Position, Game1.texture_apesh, renderData.sourceRect, WIDTH, HEIGHT, 60);
                    p.Angle = renderData.rotation;
                    p.Color = Color.Black * 0.1f;
                    Game1.self.currentEnvironment.GetParticles().Add(p);
                }
                if (timeToSpin.Done)
                {
                    SetNewTargetPosition(renderData.GetRandomPoint());
                    currentState = AttackState.Run;
                    spinTimer.Restart();
                    timeToSpin.Restart();
                }

                timeToSpin.Update();
            }
            else if (currentState == AttackState.Run)
            {
                if (IsAtTarget())
                {
                    spinTimer.Restart();
                    currentState = AttackState.Track;
                }
                base.Update(gameTime);
            }
            #endregion

        }

        public override void Stagger(float secondsToStagger, bool UseResistance = true)
        {
            if (currentState == AttackState.Cooldown || currentState == AttackState.Spin || currentState == AttackState.Slam) currentState = AttackState.Track;
            base.Stagger(secondsToStagger, UseResistance);
        }

        public override void Draw(float opacity)
        {
            if(currentState != AttackState.Spin)
                renderData.rotation = renderData.GetRotationToTarget(player.renderData.Position);

            if (!CrackTimer.Done)
            {
                Game1.self.spriteBatch.Draw(Game1.texture_crack, crackRect, Color.White);
                SlamHitbox.DrawDebug();
                CrackTimer.Update();
            }

            collision.DrawDebug();

            base.Draw(opacity);
            //Game1.self.spriteBatch.Draw(renderData.texture, renderData.DestRect, renderData.sourceRect, renderData.color * opacity, renderData.rotation, renderData.origin, renderData.effects, renderData.layerDepth);
        }

        public void Slam()
        {
            SlamHitbox = CollisionComponent.CreateRect(renderData.Position, 200, 200);
            CrackTimer.Restart();
            crackRect = new Rectangle(renderData.hitBox.X-75, renderData.hitBox.Y-75, 200, 200);
            
           

            if (SlamHitbox.Intersects(player.Hitbox))
                player.Damage(this, 3);

        }


    }
}
