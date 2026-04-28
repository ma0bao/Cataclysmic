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

        const int WIDTH = 64;
        const int HEIGHT = 64;
        const int HITBOX_WIDTH = 64;
        const int HITBOX_HEIGHT = 64;

        public Apesh(Vector2 position) : base(Game1.texture_player, new Rectangle((int)position.X, (int)position.Y, WIDTH, HEIGHT), HITBOX_WIDTH, HITBOX_HEIGHT)
        {
            staggerResistance = 0.1f;
            player = Game1.player;
            SetNewTargetPosition(player.renderData.Position);
            distanceToBeAtTarget = 100;
            spinTimer = new EventTimer(3);
            cooldownTimer = new EventTimer(.4f);
            timeToSpin = new EventTimer(1.5f);
            turnSpeed = 500;
            moveData.maxSpeed = 200;
            spinTimer.Unpause();

            staggerResistance = .30f;

            bloodData.BaseSize = 12;
  
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
            base.Draw(opacity);
        }

        public void Slam()
        {
            CollisionComponent SlamHitbox = CollisionComponent.CreateCircle(renderData.Position, 10, 12);

            
            
           

            if (SlamHitbox.Intersects(player.Hitbox))
                player.Damage(this, 3);

        }


    }
}
