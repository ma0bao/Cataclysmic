using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    class ChargeUpDash : IDash
    {
        public bool IsFinished => done;

        bool done;
        float distance = 200f;

        //Charge player
        float chargeFactor;
        float maxCharge = 7;
        SoundEffectInstance charge;

        //How fast to squeeze player
        float sizeChange;
        float sizeChangeSpeed = .1f; //Runs every frame, ticks once it is >= 1.0f

        //How much to change size per tick
        int sizeChangeIncrement = 2;
        int oldHeight;
        int oldWidth;
        public void Start(RenderComponent renderData, MoveComponent moveData)
        {
            charge = Game1.self.sound_ChargeUp.CreateInstance();
            charge.Volume = 1f;
            charge.Play();
            sizeChange = 0;
            done = false;
            chargeFactor = 0;
            oldWidth = renderData.DestRect.Width;
            oldHeight = renderData.DestRect.Height;
        }

        public void Update(RenderComponent renderData, MoveComponent moveData)
        {
            int MouseX = Mouse.GetState().X;
            int MouseY = Mouse.GetState().Y;
            float directionX = MouseX - (renderData.Position.X + renderData.DestRect.Width / 2);
            float directionY = MouseY - (renderData.Position.Y + renderData.DestRect.Height / 2);
            Vector2 direction = new Vector2(directionX, directionY);
            direction.Normalize();

            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Y))
            {
                chargeFactor += moveData.deltaTime * 3f;
                chargeFactor = MathHelper.Clamp(chargeFactor, 0, maxCharge);
                sizeChange += sizeChangeSpeed;
                if (sizeChange >= 1f)
                {
                    if (chargeFactor == maxCharge)
                        return;
                    renderData.SetWidth(renderData.DestRect.Width + sizeChangeIncrement*2);
                    renderData.SetHeight(renderData.DestRect.Height - sizeChangeIncrement);
                    sizeChange -= 1f;
                }

            }
            else
            {
                if (charge.State == SoundState.Playing)
                    charge.Stop();
                Game1.self.sound_whooshDash.Play();
                done = true;
                renderData.Position += direction * distance * chargeFactor;
                renderData.SetWidth(oldWidth);
                renderData.SetHeight(oldHeight);
            }
        }

        public void Draw(RenderComponent renderData, MoveComponent moveData)
        {

        }
    }
}
