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

        public void SetStateToCenter()
        {
            SetNewTargetPosition(Game1.BOUNDS.Center);
            currentState = AttackStates.Center;
        }

        public Atum(Rectangle destRect, Player player) : base(Game1.texture_player, destRect)
        {
            target = player;
        }

        public override void Update(GameTime gameTime)
        {
            #region Get Target Based On State
            
            #endregion
        }


    }
}
