using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cataclysmic
{
    public enum GameState { 
        Menu, Credits, Options, Game, End
    }
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public static int WIDTH = 1920;
        public static int HEIGHT = 1080;
        public static Color ambientColor = new Color(241, 220, 170);
        public static Game1 self;
        GraphicsDeviceManager graphics;
        public RenderTarget2D sceneTarget;
        public SpriteBatch spriteBatch;
        int index;
        public static Random rand = new Random();
        

        GameState gameState;
        public KeyboardState oldKB;
        public KeyboardState KB;
        public static MouseState oldMS;
        public static MouseState MS;
        long timer;
        long score;
        public Cursor[] cursors;
        Player[] players;

        // Keyboard Controls
        #region
        public static Keys player1_moveRight = Keys.D;
        public static Keys player1_moveLeft = Keys.A;
        public static Keys player1_moveUp = Keys.W;
        public static Keys player1_moveDown = Keys.S;

        public static Keys menu_select = Keys.Enter;
        public static Keys menu_fullscreen = Keys.D9;
        public static Keys menu_back = Keys.Back;
        #endregion

        // Fonts
        #region
        SpriteFont font_credits;
        #endregion

        // Textures
        #region
        public static Texture2D texture_title;
        public static Texture2D texture_blank;
        public static Texture2D texture_credits;
        public static Texture2D texture_grid;
        public static Texture2D texture_demoPlayer;
        public static Texture2D texture_enochianChain_1;
        public static Texture2D texture_enochianChain_2;
        public static Texture2D texture_menuSpriteSheet;
        public static Texture2D texture_spinningBlade;
        public static Texture2D texture_player;
        public static Texture2D texture_hitBox;
        public static Texture2D texture_square;

        //Dash Textures
        #region
        public Texture2D texture_firePortal;
        #endregion

        #endregion

        //SoundEffects
        #region

        //Dash Sounds
        #region
        public SoundEffect sound_Teleport;
        public SoundEffect sound_ChargeUp;
        public SoundEffect sound_whooshDash;
        #endregion

        //UI Sounds
        #region
        SoundEffect sound_HeavyClick;
        SoundEffect sound_HeavyStart;
        #endregion

        #endregion

        // Main Menu
        #region
        Rectangle chain1R;
        Rectangle chain1RC;
        Rectangle chain2R;
        Rectangle chain2RC;
        Rectangle chain3R;
        Rectangle chain3RC;
        Rectangle rect_screen;
        #endregion

        public Microsoft.Xna.Framework.Graphics.Effect lightEffect;
        public Microsoft.Xna.Framework.Graphics.Effect timeEffect;
        public Microsoft.Xna.Framework.Graphics.Effect chainEffect;

        //Temporary testing objects
        Speedster speedster;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = WIDTH;
            graphics.PreferredBackBufferHeight = HEIGHT;
            
            graphics.ApplyChanges();
            self = this;
        }

        protected override void Initialize()
        {
            gameState = GameState.Menu;
            IsMouseVisible = false;
            oldKB = Keyboard.GetState();
            timer = 0;
            index = 0;

            players = new Player[4];
            cursors = new Cursor[4];
            cursors[0] = new Cursor(Content);
            sceneTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            // Rectangles
            #region
            rect_screen = new Rectangle(0, 0, WIDTH, HEIGHT);
            chain1R = new Rectangle(50, 0, 100, 2048);
            chain1RC = new Rectangle(50, -2048, 100, 2048);
            chain2R = new Rectangle(900, 0, 100, 2048);
            chain2RC = new Rectangle(900, -2048, 100, 2048);
            chain3R = new Rectangle(1100, 0, 100, 2048);
            chain3RC = new Rectangle(1100, -2048, 100, 2048);
            #endregion

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //Title 
            #region

            //Textures
            #region
            texture_blank = new Texture2D(GraphicsDevice, 1, 1);
            texture_blank.SetData(new []{ Color.White });
            texture_title = Content.Load<Texture2D>("Sprites/GUI/Cataclysmic Title");
            texture_credits = Content.Load<Texture2D>("Sprites/GUI/Credits Title");
            texture_menuSpriteSheet = Content.Load<Texture2D>("Sprites/GUI/MenuSpriteSheet");
            texture_enochianChain_1 = Content.Load<Texture2D>("Sprites/GUI/Enochian Chain 1");
            texture_enochianChain_2 = Content.Load<Texture2D>("Sprites/GUI/Enochian Chain 2");
            texture_grid = Content.Load<Texture2D>("Sprites/Environment/GridBackground");
            texture_spinningBlade = Content.Load<Texture2D>("Sprites/Abilities/Bullet 24x24 Part 5C Free");
            #endregion

            //Sounds
            #region
            sound_HeavyClick = Content.Load<SoundEffect>("Sounds/UI/HeavyClick");
            sound_HeavyStart = Content.Load<SoundEffect>("Sounds/UI/HeavyStart");
            #endregion

            #endregion

            //Dash
            #region
            //SoundEffects
            #region
            sound_Teleport = Content.Load<SoundEffect>("Sounds/Abilities/TeleportSound");
            sound_ChargeUp = Content.Load<SoundEffect>("Sounds/Abilities/Charge");
            sound_whooshDash = Content.Load<SoundEffect>("Sounds/Abilities/WooshDash");
            #endregion

            //Textures
            #region
            texture_firePortal = Content.Load<Texture2D>("Sprites/Abilities/firePortal");
            #endregion

            #endregion

            font_credits = Content.Load<SpriteFont>("Fonts/CreditsFont");
            texture_player = Content.Load<Texture2D>("Sprites/Player/TestSpritePlayer");
            texture_hitBox = Content.Load<Texture2D>("Hitbox");
            texture_square = Content.Load<Texture2D>("square");

            //Effects
            #region
            lightEffect = Content.Load<Microsoft.Xna.Framework.Graphics.Effect>("Effects/Light");
            timeEffect = Content.Load<Microsoft.Xna.Framework.Graphics.Effect>("Effects/TimeTravel");
            chainEffect = Content.Load<Microsoft.Xna.Framework.Graphics.Effect>("Effects/ChainFade");
            #endregion
            players[0] = new Player(new Rectangle(WIDTH / 2, HEIGHT / 2, 60, 60));
            speedster = new Speedster(new Rectangle(100, 100, 60, 60), players[0]);
        }
        protected override void UnloadContent()
        {
            
        }

        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            //this.Exit();
            KeyboardState KB = Keyboard.GetState();
            MS = Mouse.GetState();
            if (KB.IsKeyDown(Keys.Escape))
                this.Exit();
            if (KB.IsKeyDown(Keys.D9) && oldKB.IsKeyUp(Keys.D9)) 
            {
                graphics.ToggleFullScreen();
            }
            if (gameState.Equals(GameState.Menu))
            {
                if (KB.IsKeyDown(Keys.Enter) && oldKB.IsKeyUp(Keys.Enter)) {
                    if (timer < 300)
                    {
                        timer = 300;
                    }
                    else if (index == 0)
                    {
                        gameState = GameState.Game;
                        sound_HeavyStart.Play();
                    }
                    else if (index == 1)
                    {
                        gameState = GameState.Credits;
                    }
                    else if (index == 2)
                    {
                        gameState = GameState.Options;
                    }
                    else if (index == 3) {
                        this.Exit();
                    }

                    
                }

                if ((KB.IsKeyDown(Keys.Down) && oldKB.IsKeyUp(Keys.Down)) ||
                    (KB.IsKeyDown(Keys.S) && oldKB.IsKeyUp(Keys.S))) {
                    index = index + 1;
                    index %= 4;
                    sound_HeavyClick.Play();
                }
                if ((KB.IsKeyDown(Keys.Up) && oldKB.IsKeyUp(Keys.Up)) ||
                    (KB.IsKeyDown(Keys.W) && oldKB.IsKeyUp(Keys.W)))
                {
                    index = index - 1;
                    sound_HeavyClick.Play();
                    if (index < 0) index = 3;
                }

                chain1R.Y = (int)(timer * 1.5) % 2048;
                chain1RC.Y = -2048 + (int)(timer * 1.5) % 2048;
                chain2R.Y = (int)(timer * 2) % 2048;
                chain2RC.Y = -2048 + (int)(timer * 2) % 2048;
                chain3R.Y = (int)(timer + 1024) % 2048;
                chain3RC.Y = -2048 + (int)(timer + 1024) % 2048;

            }
            else if (gameState.Equals(GameState.Credits))
            {
                if (KB.IsKeyDown(Keys.Back) && oldKB.IsKeyUp(Keys.Back))
                {
                    gameState = GameState.Menu;
                }
            }
            else if (gameState.Equals(GameState.Options))
            {
                if (KB.IsKeyDown(Keys.Back) && oldKB.IsKeyUp(Keys.Back))
                {
                    gameState = GameState.Menu;
                }
            }
            else if (gameState.Equals(GameState.Game))
            {
                players[0].Update(gameTime);
                speedster.Update(gameTime);
            }
            else if (gameState.Equals(GameState.End)) 
            { 
                
            }

            cursors[0].Update();
            timer++;
            oldMS = MS;
            oldKB = KB;
            base.Update(gameTime);
        }

        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            
            if (gameState.Equals(GameState.Menu))
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                if (timer > 300) {
                    spriteBatch.Draw(texture_enochianChain_2, chain2R, Color.White * 0.7f);
                    spriteBatch.Draw(texture_enochianChain_2, chain2RC, Color.White * 0.7f);
                    spriteBatch.Draw(texture_enochianChain_1, chain1R, Color.White * 0.7f);
                    spriteBatch.Draw(texture_enochianChain_1, chain1RC, Color.White * 0.7f);
                    spriteBatch.Draw(texture_enochianChain_1, chain3R, Color.White * 0.7f);
                    spriteBatch.Draw(texture_enochianChain_1, chain3RC, Color.White * 0.7f);

                    spriteBatch.Draw(texture_menuSpriteSheet, new Rectangle(250 - 42, 400 - 42 + 130 * index, 334, 209), new Rectangle(0, 1250, 843, 344), Color.White);
                    spriteBatch.Draw(texture_menuSpriteSheet, new Rectangle(250, 400, 250, 500), new Rectangle(0, 0, 600, 1200), Color.White);
                    


                    spriteBatch.Draw(texture_title,new Vector2(180,100), Color.White);
                }
                if (timer > 300 && timer < 600)
                {
                    spriteBatch.Draw(texture_blank, rect_screen, null, Color.Black * (float)(1 - ((timer - 300) / 300.0)), 0, Vector2.Zero, SpriteEffects.None, 1);
                    
                }

                spriteBatch.End();
            }
            else if (gameState.Equals(GameState.Credits))
            {
                spriteBatch.Begin();

                spriteBatch.Draw(texture_credits, new Vector2(WIDTH / 2 - texture_credits.Width / 2, 10), Color.White);
                spriteBatch.DrawString(font_credits, "Developers >>> Evan Tupper, Zackariya Aggour, & Thomas Liew\n\nCursor Sprites >>> Ivan Voirol", new Vector2(10, 300), Color.White);
                spriteBatch.DrawString(font_credits, "Press Back to return...", new Vector2(WIDTH / 2 - 120, HEIGHT - 50), Color.White);

                spriteBatch.End();
            }
            else if (gameState.Equals(GameState.Options))
            {
                spriteBatch.Begin();

                spriteBatch.End();
            }
            else if (gameState.Equals(GameState.Game))
            {
                lightEffect.Parameters["LightPosition"].SetValue(new Vector2(players[0].renderData.Position.X + players[0].renderData.DestRect.Width / 2, players[0].renderData.Position.Y + players[0].renderData.DestRect.Height / 2)); // Center
                lightEffect.Parameters["LightRadius"].SetValue(1300f);
                lightEffect.Parameters["ScreenSize"].SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
                lightEffect.Parameters["LightColor"].SetValue(new Vector3(1f, 1f, 0.8f)); // Warm yellow
                lightEffect.Parameters["Intensity"].SetValue(1.0f);

                timeEffect.Parameters["LightPosition"].SetValue(new Vector2(players[0].renderData.Position.X + players[0].renderData.DestRect.Width / 2, players[0].renderData.Position.Y + players[0].renderData.DestRect.Height / 2)); // Center
                timeEffect.Parameters["LightRadius"].SetValue(700f);
                timeEffect.Parameters["ScreenSize"].SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
                timeEffect.Parameters["LightColor"].SetValue(new Vector3(1f, 0.8f, 1.0f)); // Purple
                timeEffect.Parameters["Intensity"].SetValue(1.0f);
                timeEffect.Parameters["timer"].SetValue(timer);


                GraphicsDevice.SetRenderTarget(sceneTarget);
                spriteBatch.Begin();
                spriteBatch.Draw(texture_grid, rect_screen, Color.White);


                players[0].Draw(1.0f);
                speedster.Draw(1.0f);

                spriteBatch.End();

                GraphicsDevice.SetRenderTarget(null);

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, lightEffect);
                spriteBatch.Draw(sceneTarget, Vector2.Zero, Color.White);
                spriteBatch.End();

                // Overlays
                spriteBatch.Begin();
                players[0].DrawEx(1.0f);
                speedster.Draw(1.0f);
                spriteBatch.End();




            }
            else if (gameState.Equals(GameState.End))
            {
                spriteBatch.Begin();

                spriteBatch.End();
            }
            spriteBatch.Begin();
            cursors[0].Draw(1.0f);
            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
