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
    public class HarmonicStrike : Ability
    {
        public const int HEIGHT = 47 * 2;
        public const int WIDTH = 64 * 2;
        public const int RADIUS = 120;
        public const float MANA_COST = 60;
        public const float COOLDOWN = 1.5f;
        public const float SPAWN_OFFSET = 50f; // distance from player center to spawn
        public  int DAMAGE = 20;
        public const int PUSH = 2;

        public CollisionComponent Hitbox;
        public Color color;
        Vector2 Position;
        long timer = 0;
        public float angle;
        public HarmonicStrike(Vector2 position, float angle)
        {
            this.angle = angle - (float)Math.PI * 0.5f; // rotate by 90degrees

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
            if (timer > 10 && timer < 20) {
                if (Game1.MS.RightButton == ButtonState.Pressed && Game1.oldMS.RightButton == ButtonState.Released)
                {
                    Game1.sfx_punch2.Play(Game1.volume, -0.1f + (float)Game1.rand.NextDouble() * 0.2f, 0);
                    DAMAGE *= 2;
                    timer = 21;
                }
            }
            else if (timer == 20) {
                Game1.sfx_punch1.Play(Game1.volume, -0.1f + (float)Game1.rand.NextDouble() * 0.2f, 0);
            }
            else if (timer > 20) {
                ScanDamage();
            }
            timer++;

        }

        public override void Draw(float opacity)
        {

            int frameX = (int)((timer / 2) % 3 * 64);
            int frameY = (int)((timer / 6) % 3 * 47);

            Game1.self.spriteBatch.Draw(Game1.texture_basicSlash, Position, new Rectangle(frameX, frameY, 64, 47), color, angle, new Vector2(64 / 2, 47 / 2), 2f, SpriteEffects.None, 0f);
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
                    e.renderData.Position += normal * PUSH;
                    return true;
                }

            }
            return false;

        }

        public void Damage(Enemy enemy, int amount)
        {
            if (!enemy.healthData.invincible)
            {
                Game1.sfx_hurt1.Play(Game1.volume, -0.1f + (float)Game1.rand.NextDouble() * 0.2f, 0);
            }
            enemy.healthData.Damage(null, amount);
            return;
        }
        public override bool IsAlive()
        {
            if (timer > 60)
            {
                return false;
            }
            return true;
        }
    }
}
