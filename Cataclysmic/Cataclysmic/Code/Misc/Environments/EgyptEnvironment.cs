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
    class EgyptEnvironment : Environment
    {
        Texture2D background;
        SpriteBatch spriteBatch;


        bool started = false;
        int timer;

        public int  wavePointer;
        public int  enemyPointer;
        const int   MAX_ENEMIES = 4;

        int cooldown;
        const int MAX_COOLDOWN_FRAMES = 300;

        SoundEffectInstance[] tracks = { 
            Game1.self.music_desert1, // 1
            null, // 2
            null, // 3
            null, // 4
            null, // 5
            null, // 6
            null, // 7
            null, // 8
            null, // 9
            null // 10
        };
        String[] titles =
        {
            "Wave 1",
            "Wave 2",
            "Wave 3",
            "Wave 4",
            "Wave 5",
            "Wave 6",
            "Wave 7",
            "Wave 8",
            "Wave 9",
            "Wave 10"
        };
        static Vector2 EnemyStartPos => new Vector2(
            (float) (Game1.HEIGHT / 2 + Math.Cos(Game1.rand.Next(-Game1.HEIGHT*3, Game1.HEIGHT*3)) * Game1.HEIGHT*3),
            (float)Math.Sin(Game1.rand.Next(-Game1.HEIGHT * 3, Game1.HEIGHT * 3) * Game1.HEIGHT * 3)
            );

        Enemy[][] enemies = {

            // Wave 1
            new Enemy[]{ new ShotgunLamp(EnemyStartPos),
                new ShotgunLamp(EnemyStartPos),
                new MagicLamp(EnemyStartPos),
                new Androsphinx(EnemyStartPos),
                new Apesh(EnemyStartPos),
                new ShotgunLamp(EnemyStartPos)
            },

            // Wave 2
            new Enemy[]{ new Apesh(new Vector2(-400, 400)),
                new Apesh(new Vector2(Game1.WIDTH + 400, 400)),
                new Apesh(new Vector2(Game1.WIDTH/2, -1000)),
                new MagicLamp(new Vector2(Game1.WIDTH + 600, 400)),
                new ShotgunLamp(new Vector2(-400, -1000)),
                new ShotgunLamp(new Vector2(Game1.WIDTH + 400, -1000))
            },

            // Wave 3
            new Enemy[]{ },

            // Wave 4
            new Enemy[]{ },

            // Wave 5
            new Enemy[]{ },

            // Wave 6
            new Enemy[]{ },

            // Wave 7
            new Enemy[]{ },

            // Wave 8
            new Enemy[]{ },

            // Wave 9
            new Enemy[]{ },

            // Wave 10
            new Enemy[]{ }
        };

        List<Enemy> currentEnemies;
        public List<Particle> particles;
        public EgyptEnvironment() {
            cooldown = MAX_COOLDOWN_FRAMES;
            currentEnemies = new List<Enemy>();
            particles = new List<Particle>();

            timer = 0;
            background = Game1.texture_environment1;
            spriteBatch = Game1.self.spriteBatch;
        }

        override public List<Enemy> GetEnemies() {
            return currentEnemies;
        }
        override public List<Particle> GetParticles() {
            return particles;
        }

        private void Populate() {
            started = true;
            if (cooldown > 0)
                return;
            while (currentEnemies.Count < MAX_ENEMIES && enemyPointer < enemies[wavePointer].Length)
            {
                currentEnemies.Add(enemies[wavePointer][enemyPointer++]);
            }
            
        }

        // Game Loop
        override public void Update(GameTime gameTime) {

            cooldown--;
            timer++;

            // Actual Updates
            if (started == false)
                tracks[0].Play();
            Populate();
            if (currentEnemies.Count == 0 && cooldown <= 0) {
                wavePointer++;
                while (wavePointer < enemies.Length - 1 && enemies[wavePointer].Length == 0) {
                    wavePointer++;
                }
                if (wavePointer < tracks.Length && tracks[wavePointer] != null) {
                    tracks[wavePointer - 1].Stop();
                    tracks[wavePointer].Play();
                }
                enemyPointer = 0;
                cooldown = MAX_COOLDOWN_FRAMES;
            }

            foreach (Enemy e in currentEnemies)
                e.Update(gameTime);
            for (int i = currentEnemies.Count-1; i >= 0; i--) 
                if (!currentEnemies[i].IsAlive())
                    currentEnemies.RemoveAt(i);

            foreach (Particle p in particles) 
                p.Update();
            for (int i = particles.Count - 1; i >= 0; i--)
                if (!particles[i].IsAlive())
                    particles.RemoveAt(i);
            if (Game1.KB.IsKeyDown(Keys.K) && Game1.oldKB.IsKeyDown(Keys.K))
            {
                foreach (Enemy e in currentEnemies)
                {
                    e.Stagger(5f);
                }
            }
            
        }

        public override bool IsComplete()
        {
            if (wavePointer == enemies.Length - 1 && currentEnemies.Count == 0) {
                if (tracks[wavePointer-1] != null)
                    tracks[wavePointer-1].Stop();
                return true;
            }
            return false;
        }

        // Draw Methodss
        override public void DrawBackground() {
            spriteBatch.Draw(background, Vector2.Zero, Color.White);
        }
        override public void DrawParticles() {
            foreach (Particle p in particles) {
                p.Draw();
            }
        }
        override public void Draw() {
            foreach (Particle p in particles)
            {
                p.Draw();
            }

            foreach (Enemy e in currentEnemies) {
                e.Draw(1.0f);
            }
        }
        override public void DrawEx() {
            foreach (Enemy e in currentEnemies) {
                e.DrawEx(1.0f);
            }

            
            spriteBatch.DrawString(Game1.font_credits, titles[wavePointer], new Vector2(Game1.WIDTH / 2, 10), Color.White);
            
        }

        public override int GetCooldown()
        {
            return cooldown;
        }

    }
}
