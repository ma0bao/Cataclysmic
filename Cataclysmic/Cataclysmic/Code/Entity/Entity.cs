using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cataclysmic
{
    public abstract class Entity
    {
        

        public abstract void Draw(float opacity);

        public abstract void DrawEx(float opacity); // Extra Renders, such as health bars. These are to be ignored by shaders and render on top of most elements except GUI.

        public abstract void Update(GameTime gameTime);

        public abstract void Damage(Entity cause, int amount);

        public abstract Boolean IsAlive();

        public abstract Entity Clone();

        public abstract void ApplyEffect(Effect effect);

        public abstract void OnCollision();

    }
}
