using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public class Atum : Enemy
    {

        Player target;

        enum AttackStates
        {
            Center,
            Teleport,
            Geyser,
            Flame,
            Follow,
            Cooldown,
            Swing
        }

        AttackStates currentState = AttackStates.Center;

        EventTimer cooldownTimer;

        AttackStates next;
        public Atum(Rectangle destRect, Player player) : base(Game1.texture_player, destRect)
        {
            target = player;
        }

        private void SetStateToCenter()
        {
            SetNewTargetPosition(Game1.BOUNDS.Center);
            currentState = AttackStates.Center;
        }

        private void SetStateToCooldown(float time, AttackStates nextState)
        {
            cooldownTimer = new EventTimer(time);
            next = nextState;
            currentState = AttackStates.Cooldown;
        }

        public override void Update(GameTime gameTime)
        {
            #region Get Target Based On State

            #endregion

            #region Update Based On State
            if (currentState == AttackStates.Center)
            {
                base.Update(gameTime);
            }
            #endregion
        }


    }
}
