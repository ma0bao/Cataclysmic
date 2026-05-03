using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

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

        Ghost ghostPortal;

        public void Start(RenderComponent renderData, MoveComponent moveData)
        {
            charge = Game1.sound_ChargeUp.CreateInstance();
            charge.Volume = Game1.volume;
            charge.Play();        
            done = false;
            shakeOffset = Vector2.Zero;
            slowDownMultiplier = 0.7f;
            originalSpeedModifier = moveData.speedModifiers;
            Rectangle ghostRect = new Rectangle((int)renderData.Position.X,
                                                (int)renderData.Position.Y,
                                                renderData.DestRect.Width + 15,
                                                renderData.DestRect.Height + 15);
            ghostPortal = new Ghost(Game1.texture_firePortal, ghostRect, 150);
            ghostPortal.renderData.color = Color.Red;
        }

        public void Update(RenderComponent renderData, MoveComponent moveData)
        {
            int MouseX = Mouse.GetState().X;
            int MouseY = Mouse.GetState().Y;
            float directionX = MouseX - (renderData.Position.X + renderData.DestRect.Width / 2);
            float directionY = MouseY - (renderData.Position.Y + renderData.DestRect.Height / 2);
            Vector2 direction = new Vector2(directionX, directionY);
            direction.Normalize();

            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.X))
            {
                chargeFactor += moveData.deltaTime * 1.75f; //How fast the charge increases
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

                ghostPortal.renderData.Position = renderData.Position + direction * Math.Min(distance * chargeFactor, 700);
                

                if (!ghostPortal.renderData.IsOnScreen())
                    ghostPortal.renderData.Position = ghostPortal.renderData.GetPointClosestToScreen();

                ghostPortal.renderData.rotation++;
            }
            else
            {
                if (charge.State == SoundState.Playing)
                    charge.Stop();
                done = true;
                moveData.speedModifiers = originalSpeedModifier;

                RenderComponent targetPortal = new RenderComponent(Game1.texture_firePortal, ghostPortal.renderData.DestRect);
                RenderComponent playerPortal = new RenderComponent(Game1.texture_firePortal, new Rectangle(
                    (int)renderData.Position.X,
                    (int)renderData.Position.Y,
                    renderData.DestRect.Width + 15,
                    renderData.DestRect.Height + 15
                    ));

                targetPortal.color = Color.Red;
                playerPortal.color = Color.Blue;
                Game1.visuals.Add(new Visual (_renderData: targetPortal, seconds: .5f, update: Visual.RotateEveryFrame));
                Game1.visuals.Add(new Visual(playerPortal, .5f, Visual.RotateEveryFrame));
                //Game1.sound_Teleport.Play();
                Game1.sound_Teleport.Play(Game1.volume, 0, 0);

                renderData.Position = ghostPortal.renderData.Position;
                Game1.Shake(.2f, 10f);
            }
        }

        public void Draw(RenderComponent renderData, MoveComponent moveData)
        {
            ghostPortal.Draw();
        }

        public void DrawDescription()
        {
            throw new NotImplementedException();
        }

        public Texture2D GetTexture()
        {
            throw new NotImplementedException();
        }
    }
}

