using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public interface IDash
    {
        void Start(RenderComponent renderData, MoveComponent moveData);
        void Update(RenderComponent renderData, MoveComponent moveData);

        void Draw(RenderComponent renderData, MoveComponent moveData);

        void DrawDescription();
        Texture2D GetTexture();
        bool IsFinished { get; }
    }
}
