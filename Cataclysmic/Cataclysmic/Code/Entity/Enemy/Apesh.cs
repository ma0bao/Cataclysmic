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

        public Apesh(Rectangle destRect, Player p) : base(Game1.texture_player, destRect, 64, 64)
        {
            player = p;
            SetNewTargetPosition(player.renderData.Position);
            distanceToBeAtTarget = 100;
            spinTimer = new EventTimer(10);
            cooldownTimer = new EventTimer(.4f);
            timeToSpin = new EventTimer(1.5f);
            turnSpeed = 500;
            moveData.maxSpeed = 200;
            spinTimer.Unpause();
        }

        public override void Update(GameTime gameTime)
        {
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
                    spinTimer.Restart();
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
                turnSpeed = 1600f;
                moveData.maxSpeed = 700f;
                slowRadius = 0;
                renderData.rotation += 2; //MAKE SPIN TRIGGERED 
                base.Update(gameTime);
                turnSpeed = 500;
                moveData.maxSpeed = 200;
                slowRadius = 150;

                Game1.sfx_spin1.Play(Game1.volume, -0.1f + (float) Game1.rand.NextDouble() * 0.2f, 0);

                if (timeToSpin.Done)
                {
                    SetNewTargetPosition(renderData.GetRandomPoint());
                    currentState = AttackState.Run;
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

        public override void Draw(float opacity)
        {
            if(currentState != AttackState.Spin)
            renderData.rotation = renderData.GetRotationToTarget(player.renderData.Position);
            base.Draw(opacity);
        }

        public void Slam()
        {
            //Make Slam
        }


    }
}
