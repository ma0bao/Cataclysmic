using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Cataclysmic
{
    public abstract class AbilityWrapper
    {
        public int cooldownFrames = 0;

        //Add getter methods for timers
        public abstract Ability GetAbilityInstance(Vector2 Position, float angle);

        public abstract Texture2D GetTexture();

        public abstract void Update();

        public abstract void DrawDescription(SpriteBatch spriteBatch);

        public abstract bool CanUseAbility();

        public virtual bool CanUseAbility(EventTimer attackTimer, EventTimer comboTimer, Type comboClass)
        {
            return CanUseAbility();
        }
    }
}
