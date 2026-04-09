using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public enum AnimState { Idle, Walk, Die }

    //Compositon > Inehritwuf 
    public class RenderComponent
    {
        public AnimState currentState;
        public Texture2D texture;

        // Animation fields
        public int currentFrame;
        public int totalFrames;
        public int frameWidth;
        public int frameHeight;
        public float frameTime;
        public float elapsedTime;
        public bool isAnimating;
        public bool loop;

        public Rectangle DestRect
        {
            get { return _destRect; }
            set { 
                _destRect = value;
                _position.X = value.X;
                _position.Y = value.Y;
                ResetHitBox();
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
                ResetHitBox();
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

            // Animation defaults
            currentState = AnimState.Idle;
            currentFrame = 0;
            totalFrames = 1;
            frameWidth = texture.Width;
            frameHeight = texture.Height;
            frameTime = 0.1f;
            elapsedTime = 0f;
            isAnimating = false;
            loop = true;

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

            // Animation defaults
            currentState = AnimState.Idle;
            currentFrame = 0;
            totalFrames = 1;
            frameWidth = source.Width;
            frameHeight = source.Height;
            frameTime = 0.1f;
            elapsedTime = 0f;
            isAnimating = false;
            loop = true;

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

        public Vector2 GetDirectionToTarget(Vector2 target)
        {
            Vector2 direction = target - Position;
            direction.Normalize();
            return direction;
        }

        public float GetRotationToTarget(Vector2 target)
        {
            float directionX = target.X - Position.X;
            float directionY = target.Y - Position.Y;

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

        public Vector2 GetPointClosestToScreen()
        {
            float x = MathHelper.Clamp(Position.X, 0, Game1.WIDTH - 50);
            float y = MathHelper.Clamp(Position.Y, 0, Game1.HEIGHT - 50);

            return new Vector2(x, y);
        }

        public Vector2 GetRandomPoint()
        {
            float x = Game1.rand.Next(Game1.BOUNDS.X, Game1.BOUNDS.Width);
            float y = Game1.rand.Next(Game1.BOUNDS.Y, Game1.BOUNDS.Height);

            return new Vector2(x, y);
        }

        public bool IsOnScreen()
        {
            return Position.X > 0 && Position.X < Game1.WIDTH - 20;
        }

        public void DefualtDraw()
        {
            Game1.self.spriteBatch.Draw(texture, DestRect, sourceRect, color, MathHelper.ToRadians(rotation), origin, effects, layerDepth);
        }

        public void SetupAnimation(int width, int height, int frames, float secondsPerFrame = 0.1f) // 0.1 is 10 fps animation, could be changed
        {
            frameWidth = width;
            frameHeight = height;
            totalFrames = frames;
            frameTime = secondsPerFrame;
            currentFrame = 0;
            elapsedTime = 0f;
            origin = new Vector2(width / 2f, height / 2f);
            UpdateSourceRect();
        }

        public void UpdateAnimation(GameTime gameTime)
        {
            if (!isAnimating)
                return;

            elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (elapsedTime >= frameTime)
            {
                elapsedTime -= frameTime;
                currentFrame++;

                if (currentFrame >= totalFrames)
                {
                    if (loop)
                        currentFrame = 0;
                    else
                    {
                        currentFrame = totalFrames - 1;
                        isAnimating = false;
                    }
                }

                UpdateSourceRect();
            }
        }

        private void UpdateSourceRect()
        {
            sourceRect = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
        }

        public void Play()
        {
            isAnimating = true;
        }

        public void Stop()
        {
            isAnimating = false;
        }

        public void Reset()
        {
            currentFrame = 0;
            elapsedTime = 0f;
            UpdateSourceRect();
        }

        public void SetFrame(int frame)
        {
            currentFrame = Math.Max(0, Math.Min(frame, totalFrames - 1));
            UpdateSourceRect();
        }

        public void SetTexture(Texture2D newTexture, int frames, int width, int height)
        {
            texture = newTexture;
            totalFrames = frames;
            frameWidth = width;
            frameHeight = height;
            origin = new Vector2(width / 2f, height / 2f);
            currentFrame = 0;
            elapsedTime = 0f;
            UpdateSourceRect();
        }

        // Switch animation only if state is different. Returns true if state changed.
        public bool SetState(AnimState newState, Texture2D newTexture, int frames, int width, int height)
        {
            if (currentState == newState)
                return false;

            currentState = newState;
            SetTexture(newTexture, frames, width, height);
            Play();
            return true;
        }

    }
}
