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
        public const float STEP = 5f;
        public const float SPAWN_OFFSET = 20f; // distance from player center to spawn bullet
        public const int DAMAGE = 10;
        public static readonly BloodHit BLOOD = BloodHit.Light;
        public const int MANA_COST = 0;
        Vector2 Position;
        public CollisionComponent Hitbox;
        public float angle;
        public const float COOLDOWN = 0.5f;


        public Revolver(Vector2 position, float angle)
        {
            
            this.angle = angle - (float)Math.PI * 0.5f; // rotate by 90degrees

            float spawnAngle = this.angle;
            Position = new Vector2(
                position.X + (float)Math.Cos(spawnAngle) * SPAWN_OFFSET,
                position.Y + (float)Math.Sin(spawnAngle) * SPAWN_OFFSET
            );
            

            Vector2 direction = new Vector2((float)Math.Cos(angle - Math.PI * 0.5f), (float)Math.Sin(angle - Math.PI * 0.5f));

            float maxDistance = 2000f;
            Vector2 current = Position;
            int raySize = 4;
            CollisionComponent col = CollisionComponent.CreateRect(current, raySize, raySize);
            float depth;
            Vector2 normal;
            for (float point = 0; point < maxDistance; point += STEP) {
                current += direction * STEP;

                Particle p = new Particle(current, Game1.texture_blank, Rectangle.Empty, 2, 2, (int) (point / maxDistance * 30) + Game1.rand.Next(10));

                p.Color = Color.Lerp(Color.Black * 0.2f, Color.Black, point/maxDistance);
                Game1.self.currentEnvironment.GetParticles().Add(p);
                col.UpdatePosition(current);

                foreach (Enemy e in Game1.self.currentEnvironment.GetEnemies()) {
                    if (col.Intersects(e.collision, out depth, out normal)) {
                        Damage(e, DAMAGE);
                    }
                }
            }
            Game1.sfx_weapon_singleshot2.Play(Game1.volume, 0, 0);
        }
        public override void Update(GameTime gameTime)
        {
        }

        public void Damage(Enemy enemy, int amount)
        {
            if (!enemy.healthData.invincible) {
                Game1.sfx_hurtSound1.Play(Game1.volume, -0.3f + (float)Game1.rand.NextDouble() * 0.2f, 0);
                Game1.player.timeEnergy.Add(MANA_COST);
            }
            // Bullet sprays blood roughly along its trajectory
            Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            enemy.Damage(null, amount, BLOOD.WithDirection(dir));

            return;
        }

        public override void Draw(float opacity)
        {

        }

        public override bool IsAlive()
        {
            return false; // Instantaneous ability
        }
    }
}
