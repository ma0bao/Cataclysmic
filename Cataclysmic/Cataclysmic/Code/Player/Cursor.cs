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

        public CursorState CurrentState;
        Texture2D SpriteSheet;

        public Cursor(ContentManager Content)
        {
            MouseState state = Mouse.GetState();
            Position = new Vector2(state.X, state.Y);
            Angle = 0.0f;
            CurrentState = CursorState.DEFAULT;

            LoadContent(Content);
        }

        private void LoadContent(ContentManager Content)
        {
            SpriteSheet = Content.Load<Texture2D>("Sprites/Player/cursor");
        }

        public void Update()
        {
            MouseState state = Mouse.GetState();

            Position = new Vector2(state.X, state.Y);
            Angle += 0.05f;
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
