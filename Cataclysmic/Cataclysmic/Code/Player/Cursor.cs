using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;


namespace Cataclysmic
{
    public enum CursorState
    {
        DEFAULT, TARGET
    }
    public class Cursor
    {
        public Vector2 Position;
        public float Angle;
        MouseState oldMS;
        MouseState MS;

        public CursorState CurrentState;
        Texture2D SpriteSheet;

        public Cursor(ContentManager Content)
        {
            MS = Mouse.GetState();
            oldMS = Mouse.GetState();
            Position = new Vector2(MS.X, MS.Y);
            // Angle = 0.0f;
            CurrentState = CursorState.DEFAULT;

            LoadContent(Content);
        }

        private void LoadContent(ContentManager Content)
        {
            SpriteSheet = Content.Load<Texture2D>("Sprites/Player/cursor");
        }

        public void Update()
        {
            MS = Mouse.GetState();
            GamePadState gamePad = GamePad.GetState(PlayerIndex.One);
            if (gamePad.Buttons.RightShoulder == ButtonState.Pressed) {
                Game1.volume = Game1.MAX_VOLUME;
            }


            Position.X += gamePad.ThumbSticks.Right.X * 20;
            Position.Y -= gamePad.ThumbSticks.Right.Y * 20;
            if (MS.X != oldMS.X || MS.Y != oldMS.Y)
            {
                Position.X = MS.X;
                Position.Y = MS.Y;
            }
            else 
            {
                
                Mouse.SetPosition((int)Position.X, (int)Position.Y);
            }
            // Angle += 0.05f;
            oldMS = MS;
        }

        public void Draw(float opacity)
        {
            if (CurrentState.Equals(CursorState.DEFAULT))
            {
                Game1.self.spriteBatch.Draw(SpriteSheet, new Rectangle((int)Position.X, (int)Position.Y, 9, 14), new Rectangle(15, 111, 9, 14), Game1.ambientColor * opacity);
            }
        }

    }
}
