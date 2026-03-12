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
            Spin
        }

        AttackState currentState = AttackState.Track;

        Player player;

        EventTimer spinTimer;
        EventTimer agroCooldown;
        EventTimer cooldownTimer;

        public Apesh(Rectangle destRect, Player p) : base(Game1.texture_player, destRect)
        {
            player = p;
            SetNewTargetPosition(player.renderData.Position);
            distanceToBeAtTarget = 100;
            spinTimer = new EventTimer(10);
            agroCooldown = new EventTimer(5);
            cooldownTimer = new EventTimer(.4f);
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
                currentState = AttackState.Track;
            }
            else if (currentState == AttackState.Spin)
            {
                renderData.rotation += 2; //MAKE SPIN TRIGGERED 
            }
            #endregion

        }

        public void Slam()
        {
            //MAKE SLAM
        }


    }
}
