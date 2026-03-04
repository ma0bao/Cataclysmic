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
    public class EnvironmentManager
    {
        Environment current;

        public EnvironmentManager() {
            current = new EgyptianEnvironment();
        }
    }

    public abstract class Environment 
    {
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(float opacity);
        public abstract void DrawBack();
        public abstract void DrawEx(float opacity);
    }


    public class EgyptianEnvironment : Environment {
        Texture2D[] backgrounds;
        Enemy[][] waves;

        int currentWave;
        int currentEnemy;

        public bool isFinished;


        public EgyptianEnvironment() {
            isFinished = false;
            currentEnemy = 0;
            currentWave = 0;

            backgrounds = new Texture2D[1];
            waves = new Enemy[][] {
                new Enemy[] { }, // 3x Lamp, 1x Turtle
                new Enemy[] { }, // 5x Turtle
                new Enemy[] { }, // 4x Lamp, 2x Turtle
                new Enemy[] { }, // 1x Androsphinx, 2x Lamp
                new Enemy[] { }, // 4x Lamp, 3x Turtle
                new Enemy[] { }, // 2x Androsphinx
                new Enemy[] { }, // Set
                new Enemy[] { }, // 2x Androsphinx, 2x Lamp, 1x Turtle
                new Enemy[] { }, // 2x Set
                new Enemy[] { } // Atum
            };
        }

        public override void Update(GameTime gameTime) { 
            
        }
        public override void Draw(float opacity) { 
        
        }
        public override void DrawBack() { 
        
        }
        public override void DrawEx(float opacity) { 
        
        }
    }
}
