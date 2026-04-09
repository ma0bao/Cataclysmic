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
    public abstract class Environment
    {
        abstract public List<Enemy> GetEnemies();
        abstract public List<Particle> GetParticles();

        // Game Loop
        abstract public void Update(GameTime gameTime);
        abstract public bool IsComplete();
        abstract public int GetCooldown();

        // Draw Methodss
        abstract public void DrawBackground();
        abstract public void Draw();
        abstract public void DrawEx();

    }
}
