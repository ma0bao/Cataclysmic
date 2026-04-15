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
    public class Revolver : Ability
    {
        public const int WIDTH = 20;   // bullet thickness (perpendicular to travel direction)
        public const int LENGTH = 40;  // bullet length (along travel direction)
        public const float SPEED = 30f;
        public const float SPAWN_OFFSET = 20f; // distance from player center to spawn bullet
        public const int DAMAGE = 10;
        Vector2 Position;
        public CollisionComponent Hitbox;
        public float angle;
        public const float COOLDOWN = 0.5f;
        public Color color;
        long timer;
        public float energyGain = 5;


        public Revolver(Vector2 position, float angle)
        {
            
            this.angle = angle - (float)Math.PI * 0.5f; // rotate by 90degrees

            float spawnAngle = this.angle;
            Position = new Vector2(
                position.X + (float)Math.Cos(spawnAngle) * SPAWN_OFFSET,
                position.Y + (float)Math.Sin(spawnAngle) * SPAWN_OFFSET
            );

            Hitbox = CollisionComponent.CreateRect(Position, LENGTH, WIDTH);
            Hitbox.UpdateRotation(this.angle);

            color = Color.Blue;
            timer = 0;
            Game1.sfx_weapon_singleshot2.Play(Game1.volume, 0, 0);
        }
        public override void Update(GameTime gameTime)
        {
            Position.X += (float)Math.Cos(angle) * SPEED;
            Position.Y += (float)Math.Sin(angle) * SPEED;
            Hitbox.UpdatePosition(Position);
            timer++;
            ScanDamage();
        }

        public bool ScanDamage()
        {
            foreach(Enemy e in Game1.self.currentEnvironment.GetEnemies())
            {
                float depth;
                Vector2 normal;
                if (Hitbox.Intersects(e.collision, out depth, out normal))
                {
                    Damage(e, DAMAGE);
                    return true;
                }
            }
            return false;

        }

        public void Damage(Enemy enemy, int amount)
        {
            if (!enemy.healthData.invincible) {
                Game1.sfx_hurtSound1.Play(Game1.volume, -0.3f + (float)Game1.rand.NextDouble() * 0.2f, 0);
                Game1.player.timeEnergy.Add(energyGain);
            }
            enemy.healthData.Damage(null, amount);
            
            return;
        }

        public override void Draw(float opacity)
        {
            int frameX = (int)((timer / 10) % 8 * 24);

            Game1.self.spriteBatch.Draw(Game1.texture_bullets3C, Position, new Rectangle(frameX, 48, 24, 24), this.color, angle, new Vector2(12, 12), 1f, SpriteEffects.None, 1);

            // Debug, draw rotated hitbox
            Hitbox.DrawDebug();
        }

        public override bool IsAlive()
        {
            if (Position.X > Game1.WIDTH || Position.X < 0 || Position.Y < 0 || Position.Y > Game1.HEIGHT)
            {
                return false;
            }

            return true;
        }
    }
}
