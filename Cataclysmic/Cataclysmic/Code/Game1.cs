using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
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
        public const float MAX_VOLUME = 1.00f; // Vol range : [0.0f -> 1.0f]     Pitch range : [-1.0f -> 1.0f]
        public static Rectangle BOUNDS = new Rectangle(50, 50, WIDTH-70, HEIGHT-70);
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
        public static float volume;
        public Cursor[] cursors;
        public static Player[] players;

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
        public static SpriteFont font_credits;
        #endregion

        // Textures
        #region
        public static Texture2D texture_title;
        public static Texture2D texture_blank;
        public static Texture2D texture_credits;
        public static Texture2D texture_settings;
        public static Texture2D texture_grid;
        public static Texture2D texture_enochianChain_1;
        public static Texture2D texture_enochianChain_2;
        public static Texture2D texture_menuSpriteSheet;
        public static Texture2D texture_bullets1C;
        public static Texture2D texture_bullets2C;
        public static Texture2D texture_bullets3C;
        public static Texture2D texture_bullets4C;
        public static Texture2D texture_bullets5C;
        public static Texture2D texture_bullets6C;
        public static Texture2D texture_bullets7C;
        public static Texture2D texture_bullets8C;
        public static Texture2D texture_bullets9C;
        public static Texture2D texture_bullets10C;
        public static Texture2D texture_player;
        public static Texture2D texture_playerIdle;
        public static Texture2D texture_playerWalk;
        public static Texture2D texture_playerDie;
        public static Texture2D texture_hitBox;
        public static Texture2D texture_square;
        public static Texture2D texture_flyingLamp;


        //Level Textures
        #region
        public static Texture2D texture_sand;
        public static Texture2D texture_sandRight;
        public static Texture2D texture_sandBottom;
        public static Texture2D texture_sandLeft;
        public static Texture2D texture_sandTop;
        public static Texture2D texture_sandSE;
        public static Texture2D texture_sandSW;
        public static Texture2D texture_sandNW;
        public static Texture2D texture_sandNE;
        #endregion
        //Dash Textures
        #region
        public Texture2D texture_firePortal;
        #endregion

        #endregion

        // SoundEffects
        #region

        //  Dash Sounds
        #region
        public SoundEffect sound_Teleport;
        public SoundEffect sound_ChargeUp;
        public SoundEffect sound_whooshDash;
        #endregion

        //  UI Sounds
        #region
        SoundEffect sound_HeavyClick;
        SoundEffect sound_HeavyStart;
        SoundEffect sound_click;
        #endregion

        //  Abilities
        #region
        public static SoundEffect sfx_explosion_short1;

        public static SoundEffect sfx_weapon_singleshot2;
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

        int optionPointer;
        #endregion

        public Microsoft.Xna.Framework.Graphics.Effect lightEffect;
        public Microsoft.Xna.Framework.Graphics.Effect timeEffect;
        public Microsoft.Xna.Framework.Graphics.Effect chainEffect;

        //Temporary testing Object
        public static List<Enemy> enemies;

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
            volume = 1.0f;

            optionPointer = 0;
            players = new Player[4];
            cursors = new Cursor[4];
            cursors[0] = new Cursor(Content);
            sceneTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            enemies = new List<Enemy>();
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
            texture_settings = Content.Load<Texture2D>("Sprites/GUI/Settings Title");
            texture_credits = Content.Load<Texture2D>("Sprites/GUI/Credits Title");
            texture_menuSpriteSheet = Content.Load<Texture2D>("Sprites/GUI/MenuSpriteSheet");
            texture_enochianChain_1 = Content.Load<Texture2D>("Sprites/GUI/Enochian Chain 1");
            texture_enochianChain_2 = Content.Load<Texture2D>("Sprites/GUI/Enochian Chain 2");
            texture_bullets1C = Content.Load<Texture2D>("Sprites/Abilities/Bullets/Bullet 24x24 Free Part 1C");
            texture_bullets2C = Content.Load<Texture2D>("Sprites/Abilities/Bullets/Bullet 24x24 Free Part 2C");
            texture_bullets3C = Content.Load<Texture2D>("Sprites/Abilities/Bullets/Bullet 24x24 Free Part 3C");
            texture_bullets4C = Content.Load<Texture2D>("Sprites/Abilities/Bullets/Bullet 24x24 Free Part 4C");
            texture_bullets5C = Content.Load<Texture2D>("Sprites/Abilities/Bullets/Bullet 24x24 Free Part 4C");
            texture_bullets5C = Content.Load<Texture2D>("Sprites/Abilities/Bullets/Bullet 24x24 Part 5C Free");
            texture_bullets6C = Content.Load<Texture2D>("Sprites/Abilities/Bullets/Bullet 24x24 Part 6C Free");
            texture_bullets7C = Content.Load<Texture2D>("Sprites/Abilities/Bullets/Bullet 24x24 Part 7C Free");
            texture_bullets8C = Content.Load<Texture2D>("Sprites/Abilities/Bullets/Bullet 24x24 Part 8C Free");
            texture_bullets9C = Content.Load<Texture2D>("Sprites/Abilities/Bullets/Bullet 24x24 Part 9C Free");
            texture_bullets10C = Content.Load<Texture2D>("Sprites/Abilities/Bullets/Bullet 24x24 Part 10C Free");

            sfx_explosion_short1 = Content.Load<SoundEffect>("Sounds/Abilities/Explosions/sfx_exp_short_soft1");
            sfx_weapon_singleshot2 = Content.Load<SoundEffect>("Sounds/Abilities/Weapons/sfx_weapon_singleshot2");
            #endregion

            //Level Textures
            #region
            texture_grid = Content.Load<Texture2D>("Sprites/Environment/GridBackground");
            texture_sand = Content.Load<Texture2D>("Sprites/Environment/Desert/sand");
            texture_sandRight = Content.Load<Texture2D>("Sprites/Environment/Desert/sandRight");
            texture_sandBottom = Content.Load<Texture2D>("Sprites/Environment/Desert/sandBottom");
            texture_sandLeft = Content.Load<Texture2D>("Sprites/Environment/Desert/sandLeft");
            texture_sandTop = Content.Load<Texture2D>("Sprites/Environment/Desert/sandTop");
            texture_sandSE = Content.Load<Texture2D>("Sprites/Environment/Desert/sandSE");
            texture_sandSW = Content.Load<Texture2D>("Sprites/Environment/Desert/sandSW");
            texture_sandNW = Content.Load<Texture2D>("Sprites/Environment/Desert/sandNW");
            texture_sandNE = Content.Load<Texture2D>("Sprites/Environment/Desert/sandNE");
            #endregion
            //Sounds
            #region
            sound_HeavyClick = Content.Load<SoundEffect>("Sounds/UI/HeavyClick");
            sound_HeavyStart = Content.Load<SoundEffect>("Sounds/UI/HeavyStart");
            sound_click = Content.Load<SoundEffect>("Sounds/UI/click");
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
            texture_playerIdle = Content.Load<Texture2D>("Sprites/Player/Idle");
            texture_playerWalk = Content.Load<Texture2D>("Sprites/Player/Walk");
            texture_playerDie = Content.Load<Texture2D>("Sprites/Player/Die");
            texture_hitBox = Content.Load<Texture2D>("Hitbox");
            texture_square = Content.Load<Texture2D>("square");
            texture_flyingLamp = Content.Load<Texture2D>("Sprites/Enemies/FlyingLamp");

            //Effects
            #region
            lightEffect = Content.Load<Microsoft.Xna.Framework.Graphics.Effect>("Effects/Light");
            timeEffect = Content.Load<Microsoft.Xna.Framework.Graphics.Effect>("Effects/TimeTravel");
            chainEffect = Content.Load<Microsoft.Xna.Framework.Graphics.Effect>("Effects/ChainFade");
            #endregion
            players[0] = new Player(new Rectangle(WIDTH / 2, HEIGHT / 2, 60, 60));
            //speedster = new Speedster(new Rectangle(100, 100, 60, 60), players[0]);
            enemies.Add(new Androsphinx(new Rectangle(200, 200, 40, 40), players));
            enemies.Add(new ShotgunLamp(new Rectangle(200, 200, 40, 40), players));
            enemies.Add(new MagicLamp(new Rectangle(2000, 200, 40, 40), players));
        }
        protected override void UnloadContent()
        {
            
        }

        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            //this.Exit();
            KB = Keyboard.GetState();
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
                        sound_HeavyStart.Play(volume, 0, 0);
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
                    sound_click.Play(volume, -0.25f + (float) rand.NextDouble() * 0.5f, 0);
                }
                if ((KB.IsKeyDown(Keys.Up) && oldKB.IsKeyUp(Keys.Up)) ||
                    (KB.IsKeyDown(Keys.W) && oldKB.IsKeyUp(Keys.W)))
                {
                    index = index - 1;
                    sound_click.Play(volume,  -0.25f + (float)rand.NextDouble() * 0.5f, 0);
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

                if (optionPointer == 0) {
                    if (KB.IsKeyDown(Keys.Left) && timer % 3 == 0) {
                        if (volume - 0.01f > 0)
                        {
                            volume = volume - 0.01f;
                            sound_click.Play(volume, -0.1f, 0);
                        }
                    }
                    
                    if (KB.IsKeyDown(Keys.Right) && timer % 3 == 0)
                    {
                        if (volume < MAX_VOLUME) {
                            volume = Math.Min(MAX_VOLUME, volume + 0.01f);
                            sound_click.Play(volume, 0.1f, 0);
                        }
                    }
                }

            }
            else if (gameState.Equals(GameState.Game))
            {
                players[0].Update(gameTime);
                //speedster.Update(gameTime);
                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    if (!enemies[i].healthData.isAlive)
                    {
                        enemies.RemoveAt(i);
                    }
                    else
                    {
                        enemies[i].Update(gameTime);
                    }
                }
                
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
                spriteBatch.DrawString(font_credits, "Developers >>> Evan Tupper, Zackariya Aggour, & Thomas Liew" +
                    "\n\nCursor Sprites >>> Ivan Voirol" +
                    "\nBullet 24x24 >>> BDragon1727", new Vector2(10, 300), Color.White);
                spriteBatch.DrawString(font_credits, "Press Back to return...", new Vector2(WIDTH / 2 - 120, HEIGHT - 50), Color.White);

                spriteBatch.End();
            }
            else if (gameState.Equals(GameState.Options))
            {
                spriteBatch.Begin();

                spriteBatch.Draw(texture_settings, new Vector2(WIDTH / 2 - texture_credits.Width / 2, 10), Color.White);

                spriteBatch.DrawString(font_credits, "Volume >>> ", new Vector2(10, 300), Color.White);
                spriteBatch.DrawString(font_credits, ""+Math.Round(volume*100)+"%", new Vector2(300, 300), Color.White);

                spriteBatch.DrawString(font_credits, "Press Back to return...", new Vector2(WIDTH / 2 - 120, HEIGHT - 50), Color.White);


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
                //GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin();
                //spriteBatch.Draw(texture_grid, rect_screen, Color.White);

                DrawLevel(CreateLevel(@"Content/Levels/desertbg.txt")); //CAN BE OPTiMIZED - Drawing uneeded stuff
                DrawLevel(CreateLevel(@"Content/Levels/desert.txt"));  

                // Put all draw methods that are not exclusive from shaders here.


                players[0].Draw(1.0f);
                //speedster.Draw(1.0f);
                foreach(Enemy e in enemies)
                    e.Draw(1.0f);


                // End of shader section
                spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, lightEffect);
                spriteBatch.Draw(sceneTarget, Vector2.Zero, Color.White);
                spriteBatch.End();

                // Overlays
                spriteBatch.Begin();
                players[0].DrawEx(1.0f);
                


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

        public int[][] CreateLevel(string levelPath) //returns arr of level
        {
            // Sand - 0
            // sandRight - 1
            // sandBottom - 2
            // sandLeft - 3
            // sandTop - 4
            // sandSE - 5
            // sandSW - 6
            // sandNW - 7
            // sandNE - 8
            //48 by 27 level
            int[][] levelMap = new int[27][];
            
            try
            {
                using (StreamReader reader = new StreamReader(levelPath))
                {
                    for (int c = 0; c < levelMap.Length; c++)
                    {
                        levelMap[c] = new int[48];

                        string line = reader.ReadLine();
                        string[] parts = line.Split(' ');
                        for (int r = 0; r < levelMap[c].Length; r++)
                        {
                            levelMap[c][r] = Convert.ToInt32(parts[r]);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("File could not be read: ");
                Console.WriteLine(e.Message);
            }

            return levelMap;
        }

        public void DrawLevel(int[][] level) //40 by 40 tiles
        {
            int tileHeight = 40; 
            int tileWidth = 40;
            int y = 0;
            for (int c = 0; c < level.Length; c++)
            {
                int x = 0;
                for (int r = 0; r < level[c].Length; r++)
                {
                    int texture = level[c][r];
                    switch (texture)
                    {
                        case 0:
                            Game1.self.spriteBatch.Draw(texture_sand, new Rectangle(x,y, tileHeight, tileWidth), Color.White);
                            break;
                        case 1:
                            Game1.self.spriteBatch.Draw(texture_sandRight, new Rectangle(x, y, tileHeight, tileWidth), Color.White);
                            break;
                        case 2:
                            Game1.self.spriteBatch.Draw(texture_sandBottom, new Rectangle(x, y, tileHeight, tileWidth), Color.White);
                            break;
                        case 3:
                            Game1.self.spriteBatch.Draw(texture_sandLeft, new Rectangle(x, y, tileHeight, tileWidth), Color.White);
                            break;
                        case 4:
                            Game1.self.spriteBatch.Draw(texture_sandTop, new Rectangle(x, y, tileHeight, tileWidth), Color.White);
                            break;
                        case 5:
                            Game1.self.spriteBatch.Draw(texture_sandSE, new Rectangle(x, y, tileHeight, tileWidth), Color.White);
                            break;
                        case 6:
                            Game1.self.spriteBatch.Draw(texture_sandSW, new Rectangle(x, y, tileHeight, tileWidth), Color.White);
                            break;
                        case 7:
                            Game1.self.spriteBatch.Draw(texture_sandNW, new Rectangle(x, y, tileHeight, tileWidth), Color.White);
                            break;
                        case 8:
                            Game1.self.spriteBatch.Draw(texture_sandNE, new Rectangle(x, y, tileHeight, tileWidth), Color.White);
                            break;
                    }
                    x += tileWidth;
                }
                y += tileHeight;
            }
        }
    }
}
