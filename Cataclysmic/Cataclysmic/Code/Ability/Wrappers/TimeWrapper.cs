using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    class TimeWrapper : AbilityWrapper
    {
        public LinkedList<RenderComponent> memories;

        public TimeWrapper()
        {
            memories = new LinkedList<RenderComponent>();
        }

        public override void DrawDescription(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Game1.font_blackadder, "Time Manipulation", new Vector2(1500, 100), Color.Black);

            spriteBatch.Draw(Game1.texture_blank, new Rectangle(1210, 90, 270, 270), new Color(64, 44, 28));
            spriteBatch.Draw(Game1.texture_timeManip, new Rectangle(1220, 100, 250, 250), Color.White);
        }

        public override Cataclysmic.Ability GetAbilityInstance(Vector2 Position, float angle)
        {
            throw new NotImplementedException();
        }

        public override Texture2D GetTexture()
        {
            return Game1.texture_timeManip;
        }

        public override void Update(GameTime _gameTime)
        {
            Game1.self.timeTraveling = false;
            if (Game1.player.IsAbilityPressed(abilitySpot, true))
            {
                if (memories.Count > 0) {
                    Game1.self.timeTraveling = true;
                    Game1.player.renderData = memories.First.Value;
                    memories.RemoveFirst();
                }
                
            }
            else {
                if (Game1.timer % 2 == 0) 
                    memories.AddFirst(Game1.player.renderData.Clone());
                if (memories.Count > 300)
                    memories.RemoveLast();
            }

        }

        public override void DrawAbilities()
        {
            //Game1.self.spriteBatch.DrawString(Game1.font_credits, ""+memories.Count, new Vector2(100, 100), Color.Black);
        }

        public override bool CheckManaCost()
        {
            return GetMana() - Slash.MANA_COST >= 0;
        }
        public override int GetMaxCooldown()
        {
            return (int)(Slash.COOLDOWN * 60);
        }
    }
}
