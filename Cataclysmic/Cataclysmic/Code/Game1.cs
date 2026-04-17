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
        Menu, Credits, Options, Game, Abilities, End
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public static int WIDTH = 1920;
        public static int HEIGHT = 1080;
        public const float MAX_VOLUME = 1.00f; // Vol range : [0.0f -> 1.0f]     Pitch range : [-1.0f -> 1.0f]
        public static Rectangle BOUNDS = new Rectangle(60, 60, 1800, 850);
        public const int FADE_IN_START_FRAME = 0;
        public const int FADE_IN_TIME = 240;
        public SoundEffectInstance music_menu1;

        public static Color ambientColor = new Color(241, 220, 170);
        public static Game1 self;
        GraphicsDeviceManager graphics;
        public RenderTarget2D sceneTarget;
        public RenderTarget2D sceneTargetCRT;
        public SpriteBatch spriteBatch;
        int index;
        public static Random rand = new Random();

        public const int OPTION_COUNT = 4;
        public static bool debugMode = false;
        int pauseMenuPointer = 0;
        

        GameState gameState;
        GameState previousState;
        public static KeyboardState oldKB;
        public static KeyboardState KB;
        public static MouseState oldMS;
        public static MouseState MS;
        public static GamePadState oldGS;
        public static GamePadState GS;
        public static long timer;
        long score;
        public static float volume;
        public static float intensityOfCRT;
        public const float MAX_INTENSITY = 0.5f;
        public const float INTENSITY_INCREMENTER = 0.01f;
        public Cursor cursor;
        public static Player player;

        // Game Loop
        public Environment currentEnvironment;
        public int environmentPointer = 0;
        public Environment[] environments;
        public List<Particle> menu_particles;
        int particleCooldown;
        bool paused;

        // Keyboard Controls
        #region
        public static Keys player1_moveRight = Keys.D;
        public static Keys player1_moveLeft = Keys.A;
        public static Keys player1_moveUp = Keys.W;
        public static Keys player1_moveDown = Keys.S;

        public static Keys menu_select = Keys.Enter;
        public static Keys menu_fullscreen = Keys.D9;
        public static Keys menu_back = Keys.Back;
        public static Keys menu_pause = Keys.P;
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
        public static Texture2D texture_meatballEgypt;
        public static Texture2D texture_clockHand;
        public static Texture2D texture_basicSlash;
        public static Texture2D texture_character1;
        public static Texture2D texture_overlay1;
        public static Texture2D texture_environment1;
        public static Texture2D texture_seraphim;
        public static Texture2D texture_border;
        public static Texture2D texture_star;
        public static Texture2D texture_firePortal;
        public static Texture2D texture_pauseMenuText;
        

        #endregion

        // SoundEffects
        #region
        public static SoundEffect sound_Teleport;
        public static SoundEffect sound_ChargeUp;
        public static SoundEffect sound_whooshDash;

        public static SoundEffect sound_HeavyClick;
        public static SoundEffect sound_HeavyStart;
        public static SoundEffect sound_click;

        public static SoundEffect sfx_explosion_short1;
        public static SoundEffect sfx_weapon_singleshot2;
        public static SoundEffect sfx_sand1;
        public static SoundEffect sfx_sandBurst1;
        public static SoundEffect sfx_hurtSound1;
        public static SoundEffect sfx_spin1;
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
        public Microsoft.Xna.Framework.Graphics.Effect crtEffect;

        public SoundEffectInstance music_desert1;
        public SoundEffectInstance music_desert2;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = WIDTH;
            graphics.PreferredBackBufferHeight = HEIGHT;
            
            graphics.ApplyChanges();
            self = this;
            paused = false;
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
            particleCooldown = 0;
            intensityOfCRT = 0.04f;
            cursor = new Cursor(Content);
            sceneTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            sceneTargetCRT = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            
            // Rectangles
            #region
            rect_screen = new Rectangle(0, 0, WIDTH, HEIGHT);
            chain1R = new Rectangle(1700, 0, 100, 2048);
            chain1RC = new Rectangle(1700, -2048, 100, 2048);
            chain2R = new Rectangle(900, 0, 100, 2048);
            chain2RC = new Rectangle(900, -2048, 100, 2048);
            chain3R = new Rectangle(1100, 0, 100, 2048);
            chain3RC = new Rectangle(1100, -2048, 100, 2048);
            #endregion

            menu_particles = new List<Particle>();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Textures
            #region
            texture_blank = new Texture2D(GraphicsDevice, 1, 1);
            texture_blank.SetData(new []{ Color.White });
            texture_title = Content.Load<Texture2D>("Sprites/GUI/Cataclysmic Title");
            texture_settings = Content.Load<Texture2D>("Sprites/GUI/Settings Title");
            texture_credits = Content.Load<Texture2D>("Sprites/GUI/Credits Title");
            texture_menuSpriteSheet = Content.Load<Texture2D>("Sprites/GUI/MenuSpriteSheet");
            texture_enochianChain_1 = Content.Load<Texture2D>("Sprites/GUI/Enochian Chain 1");
            texture_enochianChain_2 = Content.Load<Texture2D>("Sprites/GUI/Enochian Chain 2");
            texture_character1 = Content.Load<Texture2D>("Sprites/GUI/MainCharacter3");

            texture_player = Content.Load<Texture2D>("Sprites/Player/TestSpritePlayer");
            texture_playerIdle = Content.Load<Texture2D>("Sprites/Player/IdleBlack");
            texture_playerWalk = Content.Load<Texture2D>("Sprites/Player/WalkBlack");
            texture_playerDie = Content.Load<Texture2D>("Sprites/Player/Die");
            texture_hitBox = Content.Load<Texture2D>("Hitbox");
            texture_square = Content.Load<Texture2D>("square");
            texture_flyingLamp = Content.Load<Texture2D>("Sprites/Enemies/FlyingLamp");
            texture_meatballEgypt = Content.Load<Texture2D>("Sprites/Enemies/meatballEgypt");
            texture_overlay1 = Content.Load<Texture2D>("Levels/Overlay1");
            texture_environment1 = Content.Load<Texture2D>("Levels/EgyptianEnvironmentBackground");
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
            texture_clockHand = Content.Load<Texture2D>("Sprites/Abilities/clockHand");
            texture_firePortal = Content.Load<Texture2D>("Sprites/Abilities/firePortal");
            texture_seraphim = Content.Load<Texture2D>("Sprites/GUI/Seraphim");
            texture_star = Content.Load<Texture2D>("Sprites/GUI/Star");
            texture_border = Content.Load<Texture2D>("Sprites/GUI/Border");
            texture_pauseMenuText = Content.Load<Texture2D>("Sprites/GUI/PauseMenuText");
            texture_basicSlash = Content.Load<Texture2D>("sprites/Abilities/swordSheet_64x47");
            #endregion

            //Sounds
            #region
            sound_HeavyClick = Content.Load<SoundEffect>("Sounds/UI/HeavyClick");
            sound_HeavyStart = Content.Load<SoundEffect>("Sounds/UI/HeavyStart");
            sound_click = Content.Load<SoundEffect>("Sounds/UI/click");
            sfx_explosion_short1 = Content.Load<SoundEffect>("Sounds/Abilities/Explosions/sfx_exp_short_soft1");
            sfx_weapon_singleshot2 = Content.Load<SoundEffect>("Sounds/Abilities/Weapons/sfx_weapon_singleshot2");
            sfx_sand1 = Content.Load<SoundEffect>("Sounds/Abilities/Weapons/sandShoot1");
            sfx_sandBurst1 = Content.Load<SoundEffect>("Sounds/Abilities/Weapons/sandBurst1");
            sfx_hurtSound1 = Content.Load<SoundEffect>("Sounds/hitHurt");
            sfx_spin1 = Content.Load<SoundEffect>("Sounds/Abilities/Weapons/spin3");

            sound_Teleport = Content.Load<SoundEffect>("Sounds/Abilities/TeleportSound");
            sound_ChargeUp = Content.Load<SoundEffect>("Sounds/Abilities/Charge");
            sound_whooshDash = Content.Load<SoundEffect>("Sounds/Abilities/WooshDash");
            #endregion

            // Music
            #region
            music_menu1 = Content.Load<SoundEffect>("Sounds/Music/VampPiano").CreateInstance();
            music_desert1 = Content.Load<SoundEffect>("Sounds/Music/desert_loops_2").CreateInstance();
            music_desert1.IsLooped = true;
            #endregion


            //Effects
            #region
            lightEffect = Content.Load<Microsoft.Xna.Framework.Graphics.Effect>("Effects/Light");
            timeEffect = Content.Load<Microsoft.Xna.Framework.Graphics.Effect>("Effects/TimeTravel");
            chainEffect = Content.Load<Microsoft.Xna.Framework.Graphics.Effect>("Effects/ChainFade");
            crtEffect = Content.Load<Microsoft.Xna.Framework.Graphics.Effect>("Effects/CRT");
            crtEffect.Parameters["LightPosition"].SetValue(new Vector2(WIDTH / 2, HEIGHT / 2));
            crtEffect.Parameters["LightRadius"].SetValue(1500f);
            crtEffect.Parameters["ScreenSize"].SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
            crtEffect.Parameters["Intensity"].SetValue(intensityOfCRT); 
            #endregion


            font_credits = Content.Load<SpriteFont>("Fonts/CreditsFont");

            player = new Player(new Rectangle(WIDTH / 2, HEIGHT / 2, 60, 60));
            environments = new Environment[]{ new EgyptEnvironment() };
            currentEnvironment = environments[0];
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
            GS = GamePad.GetState(PlayerIndex.One);
            if (KB.IsKeyDown(Keys.Escape))
                this.Exit();
            if ((KB.IsKeyDown(Keys.F11) && oldKB.IsKeyUp(Keys.F11)))
            {
                graphics.ToggleFullScreen();
            }
            if (gameState.Equals(GameState.Menu))
            {
                if ((KB.IsKeyDown(Keys.Enter) && oldKB.IsKeyUp(Keys.Enter))
                    || (GS.Buttons.A == ButtonState.Pressed && oldGS.Buttons.A == ButtonState.Released)) {
                    if (index == 0)
                    {
                        previousState = gameState;
                        gameState = GameState.Game;
                        sound_HeavyStart.Play(volume, 0, 0);
                        music_menu1.Stop();
                    }
                    else if (index == 1)
                    {
                        previousState = gameState;
                        gameState = GameState.Credits;
                    }
                    else if (index == 2)
                    {
                        previousState = gameState;
                        gameState = GameState.Options;
                    }
                    else if (index == 3) {
                        this.Exit();
                    }


                }

                if (timer == FADE_IN_START_FRAME) {
                    music_menu1.Volume = 0;
                    music_menu1.IsLooped = true;
                    music_menu1.Play();
                }
                if (timer < FADE_IN_START_FRAME + FADE_IN_TIME && timer > FADE_IN_START_FRAME)
                {
                    music_menu1.Volume = timer / (float)(FADE_IN_START_FRAME + FADE_IN_TIME) * volume;
                }
                else if (timer > FADE_IN_START_FRAME) {
                    music_menu1.Volume = volume;
                }

                if ((KB.IsKeyDown(Keys.Down) && oldKB.IsKeyUp(Keys.Down)) ||
                    (KB.IsKeyDown(Keys.S) && oldKB.IsKeyUp(Keys.S)) ||
                    (GS.DPad.Down == ButtonState.Pressed && oldGS.DPad.Down == ButtonState.Released)) {
                    index = index + 1;
                    index %= 4;
                    sound_click.Play(volume, -0.25f + (float) rand.NextDouble() * 0.5f, 0);
                }
                if ((KB.IsKeyDown(Keys.Up) && oldKB.IsKeyUp(Keys.Up)) ||
                    (KB.IsKeyDown(Keys.W) && oldKB.IsKeyUp(Keys.W)) ||
                    (GS.DPad.Up == ButtonState.Pressed && oldGS.DPad.Up == ButtonState.Released))
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

                foreach (Particle p in menu_particles) {
                    p.Update();
                }
                for (int i = menu_particles.Count - 1; i >= 0; i--) {
                    if (!menu_particles[i].IsAlive()) {
                        menu_particles.RemoveAt(i);
                    }
                        

                }
                int size = rand.Next(4, 20);
                if (timer % 20 == 0)
                    menu_particles.Add(
                        new Particle(
                            new Vector2(rand.Next(100, WIDTH-100), rand.Next(50, HEIGHT-50)), 
                            texture_star, 
                            new Rectangle(0, 0, texture_star.Width, texture_star.Height), 
                            size * 5, 
                            size * 7, 
                            240
                        ) 
                        { 
                        fadeInfadeOut = true,
                        Opacity = 0.5f
                        }
                        );

            }
            else if (gameState.Equals(GameState.Credits))
            {
                if ((KB.IsKeyDown(Keys.Back) && oldKB.IsKeyUp(Keys.Back)) ||
                    (GS.Buttons.X == ButtonState.Pressed && oldGS.Buttons.X == ButtonState.Released))
                {
                    gameState = previousState;
                }
            }
            else if (gameState.Equals(GameState.Options))
            {
                if ((KB.IsKeyDown(Keys.Back) && oldKB.IsKeyUp(Keys.Back)) ||
                    (GS.Buttons.X == ButtonState.Pressed && oldGS.Buttons.X == ButtonState.Released))
                {
                    gameState = previousState;
                }

                if ((KB.IsKeyDown(Keys.Down) && oldKB.IsKeyUp(Keys.Down)) ||
                    (KB.IsKeyDown(Keys.S) && oldKB.IsKeyUp(Keys.S)) ||
                    (GS.DPad.Down == ButtonState.Pressed && oldGS.DPad.Down == ButtonState.Released))
                {
                    optionPointer = (optionPointer + 1) % OPTION_COUNT;
                }

                if ((KB.IsKeyDown(Keys.Up) && oldKB.IsKeyUp(Keys.Up)) ||
                    (KB.IsKeyDown(Keys.W) && oldKB.IsKeyUp(Keys.W)) ||
                    (GS.DPad.Up == ButtonState.Pressed && oldGS.DPad.Up == ButtonState.Released))
                {
                    optionPointer = (optionPointer - 1);
                    if (optionPointer < 0)
                        optionPointer = OPTION_COUNT - 1;
                }

                if (optionPointer == 0)
                {
                    if (KB.IsKeyDown(Keys.Left) && timer % 3 == 0 || GS.DPad.Left == ButtonState.Pressed && timer % 3 == 0)
                    {
                        if (volume - 0.01f > 0)
                        {
                            volume = volume - 0.01f;
                            sound_click.Play(volume, -0.1f, 0);
                        }
                    }

                    if (KB.IsKeyDown(Keys.Right) && timer % 3 == 0 || GS.DPad.Right == ButtonState.Pressed && timer % 3 == 0)
                    {
                        if (volume < MAX_VOLUME)
                        {
                            volume = Math.Min(MAX_VOLUME, volume + 0.01f);
                            sound_click.Play(volume, 0.1f, 0);
                        }
                    }
                    music_menu1.Volume = volume;
                }
                else if (optionPointer == 1)
                {
                    if ((KB.IsKeyDown(Keys.Left) && oldKB.IsKeyUp(Keys.Left)) || (GS.DPad.Left == ButtonState.Pressed && oldGS.DPad.Left == ButtonState.Released))
                    {
                        debugMode = false;
                    }
                    if (KB.IsKeyDown(Keys.Right) && oldKB.IsKeyUp(Keys.Right) || (GS.DPad.Right == ButtonState.Pressed && oldGS.DPad.Right == ButtonState.Released))
                    {
                        debugMode = true;
                    }
                    if (KB.IsKeyDown(Keys.Enter) && oldKB.IsKeyUp(Keys.Enter) || (GS.Buttons.A == ButtonState.Pressed && oldGS.Buttons.A == ButtonState.Released))
                    {
                        debugMode = !debugMode;
                    }
                }
                else if (optionPointer == 2)
                { // Intensity
                    if (KB.IsKeyDown(Keys.Left) && timer % 3 == 0 || GS.DPad.Left == ButtonState.Pressed && timer % 3 == 0)
                    {
                        if (intensityOfCRT - INTENSITY_INCREMENTER > 0)
                        {
                            intensityOfCRT = intensityOfCRT - INTENSITY_INCREMENTER;
                        }
                    }

                    if (KB.IsKeyDown(Keys.Right) && timer % 3 == 0 || GS.DPad.Right == ButtonState.Pressed && timer % 3 == 0)
                    {
                        if (intensityOfCRT < MAX_INTENSITY)
                        {
                            intensityOfCRT = Math.Min(MAX_INTENSITY, intensityOfCRT + INTENSITY_INCREMENTER);
                        }
                    }

                }
                else if (optionPointer == 3) {
                    if ((KB.IsKeyDown(Keys.Left) && oldKB.IsKeyUp(Keys.Left)) || (GS.DPad.Left == ButtonState.Pressed && oldGS.DPad.Left == ButtonState.Released))
                    {
                        if (graphics.IsFullScreen) graphics.ToggleFullScreen();
                    }
                    if (KB.IsKeyDown(Keys.Right) && oldKB.IsKeyUp(Keys.Right) || (GS.DPad.Right == ButtonState.Pressed && oldGS.DPad.Right == ButtonState.Released))
                    {
                        if (!graphics.IsFullScreen) graphics.ToggleFullScreen();
                    }
                    if (KB.IsKeyDown(Keys.Enter) && oldKB.IsKeyUp(Keys.Enter) || (GS.Buttons.A == ButtonState.Pressed && oldGS.Buttons.A == ButtonState.Released))
                    {
                        graphics.ToggleFullScreen();
                    }
                    
                }

            }
            else if (gameState.Equals(GameState.Game))
            {
                if (KB.IsKeyDown(menu_pause) && oldKB.IsKeyUp(menu_pause)) {
                    paused = !paused;
                }

                if (paused)
                {
                    if (KB.IsKeyDown(Keys.Down) && oldKB.IsKeyUp(Keys.Down)) { 
                        pauseMenuPointer = (pauseMenuPointer + 1) % 5;
                    }
                    if (KB.IsKeyDown(Keys.Up) && oldKB.IsKeyUp(Keys.Up))
                    {
                        pauseMenuPointer = (pauseMenuPointer - 1);
                        if (pauseMenuPointer < 0) pauseMenuPointer = 4;
                    }
                    if (KB.IsKeyDown(Keys.Enter) && oldKB.IsKeyUp(Keys.Enter)) {
                        if (pauseMenuPointer == 0)
                            paused = false;
                        else if (pauseMenuPointer == 1) {
                            previousState = gameState;
                            gameState = GameState.Abilities;
                        }
                        else if (pauseMenuPointer == 2)
                        {
                            previousState = gameState;
                            gameState = GameState.Options;
                        }
                        else if (pauseMenuPointer == 3)
                        {
                            previousState = gameState;
                            gameState = GameState.Credits;
                        }
                        else if (pauseMenuPointer == 4)
                        {
                            previousState = gameState;
                            gameState = GameState.Menu;
                        }

                    }
                }
                else {
                    if (currentEnvironment.IsComplete())
                    {
                        if (environmentPointer + 1 >= environments.Length - 1)
                        {
                            gameState = GameState.End;
                            goto JumpOut;
                        }
                        currentEnvironment = environments[++environmentPointer];
                    }
                    JumpOut:
                    player.Update(gameTime);
                    currentEnvironment.Update(gameTime);
                }
                
            }
            else if (gameState.Equals(GameState.End)) 
            { 
                
            }

            cursor.Update();
            timer++;
            oldMS = MS;
            oldKB = KB;
            oldGS = GS;
            base.Update(gameTime);
        }

        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            //crtEffect.Parameters["LightPosition"].SetValue(new Vector2(players[0].renderData.Position.X + players[0].renderData.DestRect.Width / 2, players[0].renderData.Position.Y + players[0].renderData.DestRect.Height / 2)); // Center
            crtEffect.Parameters["timer"].SetValue(timer);
            crtEffect.Parameters["Intensity"].SetValue(intensityOfCRT);
            // Mouse Coordinates Clamps to the Screen
            int MCX = Math.Min(Math.Max(0, MS.X), WIDTH) + (int)(Math.Cos(timer / 60.0) * 300.0f);
            int MCY = Math.Min(Math.Max(0, MS.Y), HEIGHT) + (int)(Math.Sin(timer / 60.0) * 300.0f);

            if (gameState.Equals(GameState.Menu))
            {

                GraphicsDevice.SetRenderTarget(sceneTarget);
                GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                if (timer > FADE_IN_START_FRAME) {
                    foreach (Particle p in menu_particles)
                    {
                        float offsetX = (MCX - p.Position.X);
                        float offsetY = (MCY - p.Position.X);
                        float factor = (float)Math.Sin((1 - (double)p.Lifetime / p.startLifetime) * Math.PI);
                        Game1.self.spriteBatch.Draw(p.Texture, 
                            new Rectangle(
                                p.DestRect.X + (int)(offsetX * 0.000001f * p.DestRect.Width * p.DestRect.Width), 
                                p.DestRect.Y + (int)(offsetY * 0.000001f * p.DestRect.Width * p.DestRect.Width), 
                                p.DestRect.Width, 
                                p.DestRect.Height
                                ),
                            p.SourceRect, 
                            Color.White * p.Opacity * factor,
                            p.Angle,
                            p.Origin,
                            SpriteEffects.None,
                            1.0f);
                    }

                    spriteBatch.Draw(texture_border, new Vector2(0, 0), Color.White * 0.3f);
                    spriteBatch.Draw(texture_border, new Vector2(WIDTH/2, 0), Color.White * 0.3f);
                    


                    spriteBatch.Draw(texture_enochianChain_2, chain2R, Color.White * 0.7f);
                    spriteBatch.Draw(texture_enochianChain_2, chain2RC, Color.White * 0.7f);
                    spriteBatch.Draw(texture_enochianChain_1, chain1R, Color.White * 0.6f);
                    spriteBatch.Draw(texture_enochianChain_1, chain1RC, Color.White * 0.6f);
                    spriteBatch.Draw(texture_enochianChain_1, chain3R, Color.White * 0.5f);
                    spriteBatch.Draw(texture_enochianChain_1, chain3RC, Color.White * 0.5f);

                    spriteBatch.Draw(texture_border, new Vector2(0, 0), Color.White*0.01f);

                    spriteBatch.Draw(texture_menuSpriteSheet, new Rectangle(50 - 42, 400 - 42 + 130 * index, 334, 209), new Rectangle(0, 1250, 843, 344), Color.White);
                    spriteBatch.Draw(texture_menuSpriteSheet, new Rectangle(50, 400, 250, 500), new Rectangle(0, 0, 600, 1200), Color.White);

                    spriteBatch.Draw(texture_character1,
                        new Vector2((MCX - 1500) * 0.015f, (MCY - 600) * 0.015f + 100),
                        Color.White);


                    spriteBatch.Draw(texture_title,new Vector2(-20,100), Color.White);
                }
                if (timer > FADE_IN_START_FRAME && timer < FADE_IN_START_FRAME + FADE_IN_TIME)
                {
                    spriteBatch.Draw(texture_blank, rect_screen, null, Color.Black * (float)(1 - ((timer - FADE_IN_START_FRAME) / (float)FADE_IN_TIME)), 0, Vector2.Zero, SpriteEffects.None, 1);
                    
                }

                spriteBatch.End();
                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, crtEffect);
                spriteBatch.Draw(sceneTarget, Vector2.Zero, Color.White);
                spriteBatch.End();
            }
            else if (gameState.Equals(GameState.Credits))
            {
                GraphicsDevice.SetRenderTarget(sceneTarget);
                GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin();

                spriteBatch.Draw(texture_credits, new Vector2(WIDTH / 2 - texture_credits.Width / 2, 10), Color.White);
                spriteBatch.DrawString(font_credits, "Developers >>> Evan Tupper, Zackariya Aggour, & Thomas Liew" +
                    "\n\nCursor Sprites >>> Ivan Voirol" +
                    "\nBullet 24x24 >>> BDragon1727" +
                    "\nMain Menu Assets >>> Keisha Sespene" +
                    "\nMenu Music >>> Tadon", new Vector2(10, 300), Color.White);
                spriteBatch.DrawString(font_credits, "Press Back to return...", new Vector2(WIDTH / 2 - 120, HEIGHT - 50), Color.White);

                spriteBatch.End();

                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, crtEffect);
                spriteBatch.Draw(sceneTarget, Vector2.Zero, Color.White);
                spriteBatch.End();
            }
            else if (gameState.Equals(GameState.Options))
            {
                GraphicsDevice.SetRenderTarget(sceneTarget);
                GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin();

                spriteBatch.Draw(texture_settings, new Vector2(WIDTH / 2 - texture_credits.Width / 2, 10), Color.White);

                spriteBatch.DrawString(font_credits, "Volume >>> ", new Vector2(50, 300), Color.White);
                spriteBatch.DrawString(font_credits, ""+Math.Round(volume*100)+"%", new Vector2(300, 300), Color.White);

                spriteBatch.DrawString(font_credits, "Show Debug >>> ", new Vector2(50, 350), Color.White);
                spriteBatch.DrawString(font_credits, "" + debugMode, new Vector2(300, 350), Color.White);

                spriteBatch.DrawString(font_credits, "CRT Intensity >>> ", new Vector2(50, 400), Color.White);
                spriteBatch.DrawString(font_credits, "" + intensityOfCRT, new Vector2(300, 400), Color.White);

                spriteBatch.DrawString(font_credits, "Fullscreen >>> ", new Vector2(50, 450), Color.White);
                spriteBatch.DrawString(font_credits, "" + graphics.IsFullScreen, new Vector2(300, 450), Color.White);

                spriteBatch.DrawString(font_credits, "->", new Vector2(10, 300 + 50 * optionPointer), Color.White);

                spriteBatch.DrawString(font_credits, "Press Back to return...", new Vector2(WIDTH / 2 - 120, HEIGHT - 50), Color.White);

                spriteBatch.End();

                GraphicsDevice.SetRenderTarget(null);
                GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, crtEffect);
                spriteBatch.Draw(sceneTarget, Vector2.Zero, Color.White);
                spriteBatch.End();
            }
            else if (gameState.Equals(GameState.Game))
            {
                lightEffect.Parameters["LightPosition"].SetValue(new Vector2(player.renderData.Position.X + player.renderData.DestRect.Width / 2, player.renderData.Position.Y + player.renderData.DestRect.Height / 2)); // Center
                lightEffect.Parameters["LightRadius"].SetValue(1300f);
                lightEffect.Parameters["ScreenSize"].SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
                lightEffect.Parameters["LightColor"].SetValue(new Vector3(1.1f, 1.1f, 1.1f)); // Warm yellow
                lightEffect.Parameters["Intensity"].SetValue(1.1f);

                timeEffect.Parameters["LightPosition"].SetValue(new Vector2(player.renderData.Position.X + player.renderData.DestRect.Width / 2, player.renderData.Position.Y + player.renderData.DestRect.Height / 2)); // Center
                timeEffect.Parameters["LightRadius"].SetValue(700f);
                timeEffect.Parameters["ScreenSize"].SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));
                timeEffect.Parameters["LightColor"].SetValue(new Vector3(1f, 0.8f, 1.0f)); // Purple
                timeEffect.Parameters["Intensity"].SetValue(1.0f);
                timeEffect.Parameters["timer"].SetValue(timer);

                


                GraphicsDevice.SetRenderTarget(sceneTarget);
                spriteBatch.Begin();

                currentEnvironment.DrawBackground();
                //spriteBatch.Draw(texture_environment1, new Vector2(0, 0), Color.White);

                // Put all draw methods that are not exclusive from shaders here.


                player.Draw(1.0f);
                currentEnvironment.Draw();
                //foreach(Enemy e in enemies)
                    //e.Draw(1.0f);


                // End of shader section
                spriteBatch.End();
                GraphicsDevice.SetRenderTarget(sceneTargetCRT);

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, lightEffect);
                spriteBatch.Draw(sceneTarget, Vector2.Zero, Color.White);
                spriteBatch.End();

                
                spriteBatch.Begin();
                spriteBatch.Draw(texture_overlay1, new Vector2(0, 0), Color.White);
                player.DrawEx(1.0f);
                spriteBatch.End();

                // Overlays
                GraphicsDevice.SetRenderTarget(null);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, null, null, null, crtEffect);
                spriteBatch.Draw(sceneTargetCRT, Vector2.Zero, Color.White);
                currentEnvironment.DrawEx();
                if (debugMode) {
                    int incrementer = 0;
                    foreach (Enemy e in currentEnvironment.GetEnemies()) {
                        spriteBatch.DrawString(font_credits, ""+e, new Vector2(10, 10 + 30 * incrementer), Color.White);
                        incrementer++;
                    }
                    spriteBatch.DrawString(font_credits, "Wave: "+currentEnvironment.GetCooldown(), new Vector2(10, 10+30*incrementer), Color.White);
                }
                
                
                if (paused) {
                    for (int i = 0; i < 5; i++) {
                        spriteBatch.Draw(texture_pauseMenuText, new Vector2(100, 100 + 132 * i), new Rectangle(((pauseMenuPointer == i) ? 268 : 0),  132 * i, 268, 132), Color.White);
                    }
                }

                spriteBatch.End();
            }
            else if (gameState.Equals(GameState.End))
            {
                spriteBatch.Begin();

                spriteBatch.End();
            }
            spriteBatch.Begin();
            cursor.Draw(1.0f);
            spriteBatch.End();


            base.Draw(gameTime);
        }

        
    }
}
