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
        public List<Ability> abilities;

        public CircleSlashWrapper()
        {
            abilities = new List<Ability>();
        }

        public override void DrawDescription(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Game1.font_blackadder, "The Sweep Hand", new Vector2(1500, 100), Color.Black);
            spriteBatch.DrawString(Game1.font_gabriola, "Damage:     " + CircleSlash.DAMAGE, new Vector2(1500, 200), Color.Black);
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
                    cooldownFrames = (int)(CircleSlash.COOLDOWN * 60);

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
            return (int) (CircleSlash.COOLDOWN * 60);
        }
    }
}
