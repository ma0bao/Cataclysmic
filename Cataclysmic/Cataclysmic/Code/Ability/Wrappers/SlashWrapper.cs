using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    class SlashWrapper : AbilityWrapper
    {
        bool forward = true;
        const float ATTACKTIME = .6f;
        const float COMBOTIME = .4f;

        public override float GetAttackDuration()
        {
            return ATTACKTIME;
        }

        public override float GetComboDuration()
        {
            return COMBOTIME;
        }
        public override void DrawDescription(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Game1.font_blackadder, "Enchanted Dagger" +
                "", new Vector2(1500, 100), Color.Black);
            spriteBatch.DrawString(Game1.font_gabriola, "Damage:     " + Slash.DAMAGE, new Vector2(1500, 200), Color.Black);
            spriteBatch.DrawString(Game1.font_gabriola, "Cost:           " + Slash.MANA_COST, new Vector2(1500, 250), Color.Black);
            spriteBatch.DrawString(Game1.font_gabriola, "Cooldown:  " + Slash.COOLDOWN, new Vector2(1500, 300), Color.Black);

            String desc = "You remember little, but the hilt of this\ndagger finds your hand to be a comfortable home.";
            spriteBatch.DrawString(Game1.font_gabriola, desc, new Vector2(1200, 400), Color.Black);

            spriteBatch.Draw(Game1.texture_blank, new Rectangle(1210, 90, 270, 270), new Color(64, 44, 28));
            spriteBatch.Draw(Game1.texture_slashWrapper, new Rectangle(1220, 100, 250, 250), Color.White);
        }
        
        public override Cataclysmic.Ability GetAbilityInstance(Vector2 Position, float angle)
        {
            return new Slash(Position, angle, forward);
        }

        public override Texture2D GetTexture()
        {
            return Game1.texture_slashWrapper;
        }

        public override bool CanUseAbility(EventTimer attackTimer, EventTimer comboTimer, Type comboClass)
        {
            if (Game1.player.timeEnergy.currentMana < Slash.MANA_COST)
                return false;

            if (comboClass.Equals(this.GetType()) && comboTimer.IsRunning() && comboTimer.lerpValue > .5f && forward)
            {
                forward = false;
                return true;
            }

            if (attackTimer.Done)
            {
                forward = true;
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
