using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;


namespace Cataclysmic
{
    public class CrackleBurst : Ability
    {
        private class Crackle
        {
            public const int WIDTH = 20;
            public const float SPEED = 3.0f;
            Vector2 Position;
            public Rectangle Hitbox;
            float angle;

            public Crackle(Vector2 Position, float angle)
            {
                this.angle = angle + (float)Math.PI * 0.5f;
                Hitbox = new Rectangle((int)Position.X - WIDTH / 2, (int)Position.Y - WIDTH / 2, WIDTH, WIDTH);
                this.Position = Position;
            }

            public void Update()
            {
                Position.X += (float)Math.Cos(angle) * SPEED;
                Position.Y += (float)Math.Sin(angle) * SPEED;
                Hitbox.X = (int)Position.X - WIDTH / 2;
                Hitbox.Y = (int)Position.Y - WIDTH / 2;
            }

            public bool IsAlive()
            {
                if (Position.X > Game1.WIDTH || Position.X < 0
                    || Position.Y < 0 || Position.Y > Game1.HEIGHT)
                {
                    return false;
                }

                return true;
            }


        }


        public const int HITBOX_WIDTH = 50;
        public const float SPEED = 7.0f;
        public const float FRAMES_TO_BURST = 60;
        public const float MANA_COST = 60;

        Rectangle Hitbox;
        Vector2 Position;
        float Angle;
        long timer;
        LinkedList<Crackle> crackles;

        public CrackleBurst(Vector2 position, float angle)
        {
            Position = position;
            Angle = angle - ((float)Math.PI * 0.5f);
            timer = 0;
            Hitbox = new Rectangle((int)Position.X - HITBOX_WIDTH / 2, (int)Position.Y - HITBOX_WIDTH / 2, HITBOX_WIDTH, HITBOX_WIDTH);
        }


        public override void Update(GameTime gameTime)
        {

            if (timer < FRAMES_TO_BURST)
            {
                Position.X += (float)Math.Cos(Angle) * SPEED;
                Position.Y += (float)Math.Sin(Angle) * SPEED;
                Hitbox = new Rectangle((int)Position.X - HITBOX_WIDTH / 2, (int)Position.Y - HITBOX_WIDTH / 2, HITBOX_WIDTH, HITBOX_WIDTH);
            }
            else if (timer == FRAMES_TO_BURST)
            {
                crackles = new LinkedList<Crackle>();
                int n = 10;
                for (int i = 0; i < n; i++)
                {
                    crackles.AddFirst(new Crackle(Position, (float)Math.PI * 2 * ((float)i / n)));
                }

            }
            else
            {
                foreach (Crackle crack in crackles)
                {
                    crack.Update();
                }
            }

            timer++;
        }

        public override void Draw(float opacity)
        {
            int frameX = (int)((timer / 10) % 8 * 24);

            if (timer <= FRAMES_TO_BURST)
            {
                Game1.self.spriteBatch.Draw(Game1.texture_bullets3C, Hitbox, new Rectangle(frameX, 288, 24, 24), Color.White, Angle, new Vector2(12, 12), SpriteEffects.None, 1);
            }
            else
            {
                foreach (Crackle crack in crackles)
                    Game1.self.spriteBatch.Draw(Game1.texture_bullets5C, crack.Hitbox, new Rectangle(frameX, 240, 24, 24), Color.White);
            }
        }

        public override bool IsAlive()
        {
            if (Hitbox.X > Game1.WIDTH || Hitbox.X - Hitbox.Width / 2 < 0)
                return false;

            return true;
        }
    }
}
