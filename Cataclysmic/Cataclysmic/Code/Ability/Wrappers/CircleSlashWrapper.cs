using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Cataclysmic.Code.Ability.Wrappers
{
    class CircleSlashWrapper : AbilityWrapper
    {
        public override void DrawDescription(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Game1.font_blackadder, "Not Yet Implemented", new Vector2(1600, 100), Color.Black);
        }

        public override Cataclysmic.Ability GetAbilityInstance(Vector2 Position, float angle)
        {
            return new CircleSlash(Position, angle);
        }

        public override Texture2D GetTexture()
        {
            return Game1.texture_revolverWrapper;
        }
    }
}
