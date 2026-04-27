using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    class EmptyWrapper : AbilityWrapper
    {
        public override void DrawDescription(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Game1.font_blackadder, "This ability is not yet implemented.. \nsorry ):", new Vector2(1200, 100), Color.Black);
        }

        public override Cataclysmic.Ability GetAbilityInstance(Vector2 Position, float angle)
        {
            return new NothingBurger(Position, angle);
        }

        public override Texture2D GetTexture()
        {
            return Game1.texture_emptyWrapper;
        }

        

        public override bool UseAbility()
        {
            if (cooldownFrames <= 0)
            {
                cooldownFrames = 0;
                return true;
            }
            return false;
        }

        public override void Update()
        {
            cooldownFrames--;
        }
    }
}
