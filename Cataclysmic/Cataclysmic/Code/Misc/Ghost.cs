using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public class Ghost
    {
        public RenderComponent renderData;

        public Ghost(Texture2D texture, Rectangle destRect, byte opacity = 50)
        {
            renderData = new RenderComponent(texture, destRect);
            renderData.color.A = opacity;
        }

        public Ghost(RenderComponent renderComponent, byte opacity = 50)
        {
            renderData = new RenderComponent(renderComponent.texture, renderComponent.DestRect);
            renderData.color.A = opacity;
        }

        public void Draw()
        {
            renderData.DefualtDraw();
        }
    }
}
