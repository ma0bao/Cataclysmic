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
    public class Slash : Ability
    {
        // 1 = height * 2, width * 2.  2 = height * 3, width * 2.  3 = height * 3, width * 2.5
        public const float HEIGHT = 47 * 2;
        public const float WIDTH = 64 * 2;
        public const float COOLDOWN = .5f;
        public const float SPAWN_OFFSET = 50f; // distance from player center to spawn
        public const int DAMAGE = 20;
        public const int PUSH = 2;

        public float energyGain = 10;
        public CollisionComponent Hitbox;
        public Color color;
        Vector2 Position;
        long timer = 0;
        public float angle;
        bool slashRight = true;
        public Slash(Vector2 position, float angle, bool _slashRight)
        {
            this.angle = angle - (float)Math.PI * 0.5f; // rotate by 90degrees
            slashRight = _slashRight;
            float spawnAngle = this.angle;
            Position = new Vector2(
                position.X + (float)Math.Cos(spawnAngle) * SPAWN_OFFSET,
                position.Y + (float)Math.Sin(spawnAngle) * SPAWN_OFFSET
            );
            Hitbox = CollisionComponent.CreateRect(position, WIDTH, HEIGHT);
            Hitbox.Update(Position, this.angle);
            color = Color.Black;
        }


        public override void Update(GameTime gameTime)
        {
            
            Position.X = Game1.player.renderData.Position.X + (float)Math.Cos(angle) * SPAWN_OFFSET;
            Position.Y = Game1.player.renderData.Position.Y + (float)Math.Sin(angle) * SPAWN_OFFSET;
            Hitbox.UpdatePosition(Position);
            ScanDamage();
            timer++;
 
        }

        public override void Draw(float opacity)
        {
            int frameX;
            int frameY;
            if (slashRight)
            {
                frameX = (int)((timer / 2) % 3 * 64);
                frameY = (int)((timer / 6) % 3 * 47);
            } else
            {
                frameX = (int)((timer / 2) % 3);
                frameY = (int)((timer / 6) % 3);
            }
            Game1.self.spriteBatch.Draw(Game1.texture_basicSlash, Position, new Rectangle(frameX, frameY, 64, 47), color, angle, new Vector2(64/2, 47/2 - 5), new Vector2(2f, 2.5f), SpriteEffects.None, 0f);
            Hitbox.DrawDebug();
        }

        public bool ScanDamage()
        {
            foreach (Enemy e in Game1.self.currentEnvironment.GetEnemies())
            {
                float depth;
                Vector2 normal;
                if (Hitbox.Intersects(e.collision, out depth, out normal))
                {
                    Damage(e, DAMAGE);
                    //e.renderData.Position += normal * PUSH;
                    e.Stagger(.1f, false);
                    return true;
                }

            }
            return false;

        }

        public void Damage(Enemy enemy, int amount)
        {
            if (!enemy.healthData.invincible)
            {
                Game1.player.timeEnergy.Add(energyGain);
            }
            enemy.Damage(null, amount);
            
            return;
        }
        public override bool IsAlive()
        {
            if (timer > 18)
            {
                return false;
            }
            return true;
        }
    }
}
