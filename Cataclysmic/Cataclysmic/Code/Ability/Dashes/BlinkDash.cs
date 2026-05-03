using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    class BlinkDash : IDash
    {
        float distance = 300f;
        bool finished;

        int timer = 20;
        float rotation = 0;
        Vector2 pos1;
        Vector2 pos2;
        public bool IsFinished => finished;

        public void Start(RenderComponent renderData, MoveComponent moveData)
        {
            SoundEffectInstance sound = Game1.sound_Teleport.CreateInstance();
            sound.Volume = Game1.volume;
            sound.Play();
            pos1 = renderData.Position;
            Vector2 direction = moveData.GetDirection();
            renderData.Position += direction * distance;
            renderData.ResetHitBox();
            pos2 = renderData.Position;
        }

        public void Update(RenderComponent renderData, MoveComponent moveData)
        {
            timer--;

            if (timer == 0)
                finished = true;
        }

        public void Draw(RenderComponent renderData, MoveComponent moveData)
        {
            Texture2D texture = Game1.texture_firePortal;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);
            rotation += 15;
            Rectangle destRect1 = new Rectangle((int)pos1.X, (int)pos1.Y, renderData.DestRect.Width+15, renderData.DestRect.Height+15);
            Rectangle destRect2 = new Rectangle((int)pos2.X, (int)pos2.Y, renderData.DestRect.Width+15, renderData.DestRect.Height+15);
            Game1.self.spriteBatch.Draw(texture, destRect1, null, Color.DarkOrange, MathHelper.ToRadians(rotation), origin, SpriteEffects.None, 0f);
            Game1.self.spriteBatch.Draw(texture, destRect2, null, Color.Red, MathHelper.ToRadians(rotation), origin, SpriteEffects.None, 0f);
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
