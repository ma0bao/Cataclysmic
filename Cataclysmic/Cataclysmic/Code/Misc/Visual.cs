using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cataclysmic
{
    public class Visual
    {
        RenderComponent renderData;
        Action<Visual> updateAction;
        EventTimer lifeTime;

        //For simplicity, if you want your visual to do smth in update declare the method as a static variable here

        public static Action<Visual> RotateEveryFrame = (vis) => { vis.renderData.rotation++; };


        public Visual(Texture2D texture, Rectangle destRect, float seconds, Action<Visual> update = null)
        {
            renderData = new RenderComponent(texture, destRect);
            lifeTime = new EventTimer(seconds);
            updateAction = update;
        }

        public Visual(RenderComponent _renderData, float seconds, Action<Visual> update = null)
        {
            renderData = _renderData;
            lifeTime = new EventTimer(seconds);
            updateAction = update;

        }


        public void Update()
        {
            if(updateAction != null)
                updateAction(this);

            lifeTime.Update();
        }

        public void Draw()
        {
            renderData.DefualtDraw();
        }

        public bool IsAlive()
        {
            return lifeTime.IsRunning();
        }
    }
}
