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

        // Slow down effect
        float slowDownMultiplier;
        float originalSpeedModifier;

        //Shake effect
        float shakeIntensity = 7f;
        Vector2 shakeOffset;

        public void Start(RenderComponent renderData, MoveComponent moveData)
        {
            charge = Game1.self.sound_ChargeUp.CreateInstance();
            charge.Volume = 1f;
            charge.Play();
            sizeChange = 0;
            done = false;
            chargeFactor = 0;
            shakeOffset = Vector2.Zero;
            slowDownMultiplier = 0.7f;
            originalSpeedModifier = moveData.speedModifiers;
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
                slowDownMultiplier -= (float)(moveData.deltaTime * 0.7);
                slowDownMultiplier = MathHelper.Clamp(slowDownMultiplier, 0, 0.7f);

                //gradual slowdown
                moveData.speedModifiers = originalSpeedModifier * slowDownMultiplier;

                //Shake intensity increases with charge
                float currentShake = shakeIntensity * (chargeFactor / maxCharge);
                shakeOffset.X = (float)(Game1.rand.NextDouble() * 2 - 1) * currentShake;
                shakeOffset.Y = (float)(Game1.rand.NextDouble() * 2 - 1) * currentShake;

                renderData.Position += shakeOffset;

            }
            else
            {
                if (charge.State == SoundState.Playing)
                    charge.Stop();
                Game1.self.sound_whooshDash.Play();
                done = true;
                moveData.speedModifiers = originalSpeedModifier;
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
