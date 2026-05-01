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
        public List<Ability> abilities;

        public SlashWrapper()
        {
            abilities = new List<Ability>();
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
            return new Slash(Position, angle, true);
        }

        public override Texture2D GetTexture()
        {
            return Game1.texture_slashWrapper;
        }

        public override void Update(GameTime _gameTime)
        {
            if (cooldownFrames > 0)
                cooldownFrames--;

            foreach (Ability abil in abilities)
            {
                abil.Update(_gameTime);
            }
            for (int i = abilities.Count - 1; i >= 0; i--)
            {
                if (!abilities[i].IsAlive())
                    abilities.RemoveAt(i);
            }

            if (Game1.player.IsAbilityPressed(abilitySpot))
            {
                if (cooldownFrames <= 0)
                {
                    cooldownFrames = (int)(Slash.COOLDOWN * 60);

                    abilities.Add(GetAbilityInstance(Game1.player.renderData.Position, Game1.player.angle));
                }
            }

        }

        public override void DrawAbilities()
        {
            foreach (Ability abil in abilities)
            {
                abil.Draw(1.0f);
            }
        }

        public override int GetMaxCooldown()
        {
            return (int)(Slash.COOLDOWN * 60);
        }
    }
}
