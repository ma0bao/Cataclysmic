using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    class RevolverWrapper : AbilityWrapper
    {
        public override void DrawDescription(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Game1.font_blackadder, "The Colt", new Vector2(1500, 100), Color.Black);
            spriteBatch.DrawString(Game1.font_gabriola, "Damage:     " + Revolver.DAMAGE, new Vector2(1500, 200), Color.Black);
            spriteBatch.DrawString(Game1.font_gabriola, "Cost:           " + Revolver.MANA_COST, new Vector2(1500, 250), Color.Black);
            spriteBatch.DrawString(Game1.font_gabriola, "Cooldown:  " + Revolver.COOLDOWN, new Vector2(1500, 300), Color.Black);

            String desc = "An old but reliable relic. This colt has held up across generations\nand has been rumored to have special properties.";
            spriteBatch.DrawString(Game1.font_gabriola, desc, new Vector2(1200, 400), Color.Black);

            spriteBatch.Draw(Game1.texture_revolverWrapper, new Rectangle(1210, 90, 270, 270), new Color(64, 44, 28));
            spriteBatch.Draw(Game1.texture_revolverWrapper, new Rectangle(1220, 100, 250, 250), Color.White);
        }

        public override Cataclysmic.Ability GetAbilityInstance(Vector2 Position, float angle)
        {
            return new Revolver(Position, angle);
        }

        public override Texture2D GetTexture()
        {
            return Game1.texture_revolverWrapper;
        }

        public override bool UseAbility()
        {
            if (cooldownFrames <= 0)
            {
                cooldownFrames = (int)(Revolver.COOLDOWN * 60);
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
