
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
        public int abilitySpot = 0;

        public abstract Ability GetAbilityInstance(Vector2 Position, float angle);

        public abstract Texture2D GetTexture();

        public abstract void Update(GameTime _gameTime);

        public abstract void DrawDescription(SpriteBatch spriteBatch);

        public abstract void DrawAbilities();

        public abstract int GetMaxCooldown();

    }
}
