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
    public class Revolver : Ability
    {
        //When dealing with bullets, length is the "x" while width is the "y" if the bullet is facing to the right
        public const int WIDTH = 20;
        public const int LENGTH = 40;
        public const float SPEED = .1f;
        Vector2 Position;
        public Rectangle Hitbox;
        public float angle;
        float cooldownTimer = 0f;
        public const float COOLDOWN = 0.25f;
        public Color color;
        long timer;


        public Revolver(Vector2 position, float angle)
        {
            angle = this.angle = angle - (float)Math.PI * 0.5f; // rotate by 90degrees
            Hitbox = new Rectangle((int)position.X, (int)Position.Y - LENGTH * 2, LENGTH, WIDTH);
            Position = position;
            color = Color.White;
            timer = 0;
            Game1.sfx_weapon_singleshot2.Play(Game1.volume, 1, 0);
        }
        public override void Update(GameTime gameTime)
        {
            Position.X += (float)Math.Cos(angle) * SPEED;
            Position.Y += (float)Math.Sin(angle) * SPEED;
            Hitbox.X = (int)Position.X - WIDTH / 2;
            Hitbox.Y = (int)Position.Y - LENGTH /  2;
            timer++;

        }



        public override void Draw(float opacity)
        {
            int frameX = (int)((timer / 10) % 8 * 24);

            Game1.self.spriteBatch.Draw(Game1.texture_bullets3C, Hitbox, new Rectangle(frameX, 48, 24, 24), this.color, angle, new Vector2(12, 12), SpriteEffects.None, 1);

        }
        public override bool IsAlive()
        {
            if (Position.X > Game1.WIDTH || Position.X < 0
                || Position.Y < 0 || Position.Y > Game1.HEIGHT)
            {
                return false;
            }

            return true;
        }
    }
}
