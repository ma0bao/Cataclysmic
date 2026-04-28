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
    public class Particle
    {
        public Vector2 Position;
        public float Angle;
        public Vector2 Velocity;
        public float AngularVelocity;
        public Vector2 Acceleration;
        public float AngularAcceleration;
        public float drag;

        public Texture2D Texture;
        public Rectangle DestRect;
        public Rectangle SourceRect;

        public int Lifetime;
        public float Opacity;
        public Color Color;
        public bool fadeInfadeOut = false;
        public int startLifetime;

        public Vector2 Origin;

        public Particle(Vector2 _Position, Texture2D _Texture, Rectangle _SourceRect, int Width, int Height, int _Lifetime) {
            Position = _Position;
            Angle = 0;
            Velocity = Vector2.Zero;
            AngularVelocity = 0;
            Acceleration = Vector2.Zero;
            AngularAcceleration = 0;
            drag = 1.0f;

           

            Texture = _Texture;
            SourceRect = _SourceRect;
            Origin = new Vector2(Texture.Width/2, Texture.Height/2);
            DestRect = new Rectangle((int)Position.X - Width / 2, (int)Position.Y - Height / 2, Width, Height);
            Lifetime = _Lifetime;
            startLifetime = Lifetime;
            Opacity = 1.0f;
            Color = Color.White;
        }

        public void Update() {
            Angle = (Angle + AngularVelocity) % 360;
            AngularVelocity = (AngularVelocity + AngularAcceleration) % 360;

            //Position += Velocity;
            Velocity *= drag;
            Position += Velocity;
            DestRect.X = (int)Position.X;
            DestRect.Y = (int)Position.Y;
            Lifetime--;
        }

        public void Draw() {
            if (fadeInfadeOut)
            {
                float factor = (float) Math.Sin((1 - (double)Lifetime/startLifetime) * Math.PI);
                Game1.self.spriteBatch.Draw(Texture, DestRect, SourceRect, Color * Opacity * factor, rotation: Angle, origin: Origin, SpriteEffects.None, 1.0f);
            }
            else {
                Game1.self.spriteBatch.Draw(Texture, DestRect, SourceRect, Color * Opacity, Angle, Origin, SpriteEffects.None, 1.0f);
            }

        }

        public bool IsAlive() {
            return Lifetime > 0;
        }
    }
}
