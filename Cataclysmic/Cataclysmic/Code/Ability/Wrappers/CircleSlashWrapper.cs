using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Cataclysmic
{
    class CircleSlashWrapper : AbilityWrapper
    {

        const float ATTACKTIME = .5f;

        public override float GetAttackDuration()
        {
            return ATTACKTIME;
        }
        public override void DrawDescription(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Game1.font_blackadder, "The Sweep Hand", new Vector2(1500, 100), Color.Black);
            spriteBatch.DrawString(Game1.font_gabriola, "Damage:     "+CircleSlash.DAMAGE, new Vector2(1500, 200), Color.Black);
            spriteBatch.DrawString(Game1.font_gabriola, "Cost:           " + CircleSlash.MANA_COST, new Vector2(1500, 250), Color.Black);
            spriteBatch.DrawString(Game1.font_gabriola, "Cooldown:  " + CircleSlash.COOLDOWN, new Vector2(1500, 300), Color.Black);

            String desc = "This mighty sword requires great strength to wield.\nUse time energy to propel it in a deadly circle around you,\n pushing back any enemies near.";
            spriteBatch.DrawString(Game1.font_gabriola, desc, new Vector2(1200, 400), Color.Black);
            
            spriteBatch.Draw(Game1.texture_blank, new Rectangle(1210, 90, 270, 270), new Color(64, 44, 28));
            spriteBatch.Draw(Game1.texture_circleSlashWrapper, new Rectangle(1220, 100, 250, 250), Color.White);
        }

        public override Cataclysmic.Ability GetAbilityInstance(Vector2 Position, float angle)
        {
            return new CircleSlash(Position);
        }

        public override Texture2D GetTexture()
        {
            return Game1.texture_circleSlashWrapper;
        }

        public override bool CanUseAbility(EventTimer attackTimer, EventTimer comboTimer, Type comboClass)
        {
            if (Game1.player.timeEnergy.currentMana < CircleSlash.MANA_COST)
                return false;

            return attackTimer.Done;
        }

        public override void Update()
        {
            cooldownFrames--;
        }
    }
}
