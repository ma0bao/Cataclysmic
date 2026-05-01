using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    class SwapWrapper : AbilityWrapper
    {
        public List<Ability> abilities;

        public SwapWrapper()
        {
            abilities = new List<Ability>();
        }

        public override void DrawDescription(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Game1.font_blackadder, "Swap" +
                "", new Vector2(1500, 100), Color.Black);
            spriteBatch.DrawString(Game1.font_gabriola, "Cost:           " + Swap.MANA_COST, new Vector2(1500, 200), Color.Black);
            spriteBatch.DrawString(Game1.font_gabriola, "Cooldown:  " + Swap.COOLDOWN, new Vector2(1500, 250), Color.Black);

            String desc = "The bells of the Gion monastery solidify this world's only reality.\nThat all is impermanent.";
            spriteBatch.DrawString(Game1.font_gabriola, desc, new Vector2(1200, 400), Color.Black);

            spriteBatch.Draw(Game1.texture_blank, new Rectangle(1210, 90, 270, 270), new Color(64, 44, 28));
            spriteBatch.Draw(Game1.texture_swapWrapper, new Rectangle(1220, 100, 250, 250), Color.White);
        }

        public override Cataclysmic.Ability GetAbilityInstance(Vector2 Position, float angle)
        {
            return new Swap(Position, angle);
        }

        public override Texture2D GetTexture()
        {
            return Game1.texture_swapWrapper;
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
                    cooldownFrames = (int)(Swap.COOLDOWN * 60);

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
            return (int)(Swap.COOLDOWN * 60);
        }
    }
}
