using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public class RenderComponent
    {
        public Texture2D texture;
        
        public Rectangle DestRect
        {
            get { return _destRect; }
            set { 
                _destRect = value;
                _position.X = value.X;
                _position.Y = value.Y;
            }
        }
        public Rectangle _destRect;

        public Vector2 Position
        {
            get { return _position; }
            set { 
                _position = value;
                _destRect.X = (int)value.X;
                _destRect.Y = (int)value.Y;
            }
        }
        public Vector2 _position;

        public Rectangle sourceRect, hitBox;

        public Vector2 origin;
        public float rotation, layerDepth;
        public Color color;
        public SpriteEffects effects;

        public RenderComponent(Texture2D tex, Rectangle destRect)
        {
            texture = tex;
            DestRect = destRect;
            Position = new Vector2(destRect.X, destRect.Y);
            sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
            origin = new Vector2(texture.Width / 2, texture.Height / 2);
            rotation = 0.0f;
            layerDepth = 0f;
            color = Color.White;
            effects = SpriteEffects.None;
            ResetHitBox();
        }

        public RenderComponent(Texture2D t, Rectangle destRect, Rectangle source, Vector2 _origin, 
            Color c, SpriteEffects s, float _rotation = 0f, float _depth = 0f) 
        {
            texture = t;
            DestRect = destRect;
            Position = new Vector2(destRect.X, destRect.Y);
            sourceRect = source;
            origin = _origin;
            rotation = _rotation;
            layerDepth = _depth;
            color = c;
            effects = s;
            ResetHitBox();
        }

        public void SetX(float x)
        {
            _position.X = x;
        }

        public void SetY(float y)
        {
            _position.Y = y;
        }

        public void SetWidth(int w)
        {
            _destRect.Width = w;
        }

        public void SetHeight(int h)
        {
            _destRect.Height = h;
        }

        public void ResetHitBox()
        {
            hitBox = new Rectangle((DestRect.X - DestRect.Width / 2) + 5, (DestRect.Y - DestRect.Height / 2) + 5, DestRect.Width - 10, DestRect.Height - 10);
        }

        public float GetRotationToTarget(Vector2 target)
        {

            float directionX = target.X - (Position.X + DestRect.Width / 2);
            float directionY = target.Y - (Position.Y + DestRect.Height / 2);

            return (float)(Math.Atan2(directionY, directionX) + (Math.PI * 0.5f));
        }

        public float GetRotationToVelocity(Vector2 velocity)
        {
            return (float)(Math.Atan2(velocity.Y, velocity.X) + (Math.PI * 0.5f));
        }

        public float GetDistanceToTarget(Vector2 target)
        {
            return (float)Math.Sqrt(Math.Pow(Position.X-target.X, 2) + Math.Pow(Position.Y - target.Y, 2));
        }

        public void DefualtDraw()
        {
            Game1.self.spriteBatch.Draw(texture, DestRect, sourceRect, color, rotation, origin, effects, layerDepth);
        }

    }
}
