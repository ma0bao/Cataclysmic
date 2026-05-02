using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        protected virtual bool CheckCrossAbilityCooldown()
        {
            return Game1.player.abilityTimer.Done;
        }

    }
}
