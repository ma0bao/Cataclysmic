using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public class SpeedDash : IDash
    {
        float duration = 0.5f;
        float timer;
        float multiplier = 2.5f;
        public bool IsFinished => timer <= 0;

        public void Start(RenderComponent renderData, MoveComponent moveData)
        {
            timer = duration;
            moveData.speedModifiers *= multiplier;
        }

        public void Update(RenderComponent renderData, MoveComponent moveData)
        {
            timer -= moveData.deltaTime;

            if (IsFinished)
                moveData.speedModifiers /= multiplier;
        }

        public void Draw(RenderComponent renderData, MoveComponent moveData) { }
    }
}
