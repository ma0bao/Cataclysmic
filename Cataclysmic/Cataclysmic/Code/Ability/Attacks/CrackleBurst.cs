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
    public class CrackleBurst : Ability
    {
        private class Crackle
        {
            public const int WIDTH = 20;
            public const float SPEED = 3.0f;
            public const int DAMAGE = 10;
            public Vector2 Position;
            public CollisionComponent Hitbox;
            float angle;

            public Crackle(Vector2 Position, float angle)
            {
                this.angle = angle + (float)Math.PI * 0.5f;
                this.Position = Position;
                Hitbox = CollisionComponent.CreateRect(Position, WIDTH, WIDTH);
            }

            public void Update()
            {
                ScanDamage();
                Position.X += (float)Math.Cos(angle) * SPEED;
                Position.Y += (float)Math.Sin(angle) * SPEED;
                Hitbox.UpdatePosition(Position);
            }

            public void Draw(float opacity, Color color, int frameX)
            {
                Game1.self.spriteBatch.Draw(Game1.texture_bullets5C, Position, new Rectangle(frameX, 240, 24, 24), color, angle, new Vector2(12, 12), 1f, SpriteEffects.None, 1);
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
                        return true;
                    }
                }
                return false;

            }

            public void Damage(Enemy enemy, int amount)
            {
                if (!enemy.healthData.invincible)
                {
                    Game1.sfx_hurtSound1.Play(Game1.volume, -0.1f + (float)Game1.rand.NextDouble() * 0.2f, 0);
                }
                enemy.healthData.Damage(null, amount);
                
                return;
            }

            public bool IsAlive()
            {
                if (Position.X > Game1.WIDTH || Position.X < 0
                    || Position.Y < 0 || Position.Y > Game1.HEIGHT)
                {
                    return false;
                }

                return true;
            }
        }


        public const int HITBOX_WIDTH = 50;
        public const float SPEED = 7.0f;
        public const float FRAMES_TO_BURST = 60;
        public const float MANA_COST = 60;
        public const float SPAWN_OFFSET = 20f;
        public const float COOLDOWN = 1.0f;
        public const int DAMAGE = 20;

        public CollisionComponent Hitbox;
        public Color color;
        Vector2 Position;
        float Angle;
        long timer;
        LinkedList<Crackle> crackles;

        public CrackleBurst(Vector2 position, float angle)
        {
            Angle = angle - (float)Math.PI * 0.5f;

            float spawnAngle = Angle;
            Position = new Vector2(position.X + (float)Math.Cos(spawnAngle) * SPAWN_OFFSET, position.Y + (float)Math.Sin(spawnAngle) * SPAWN_OFFSET);

            Hitbox = CollisionComponent.CreateRect(Position, HITBOX_WIDTH, HITBOX_WIDTH);
            Hitbox.UpdateRotation(Angle);

            color = Color.White;
            timer = 0;
            Game1.sfx_weapon_singleshot2.Play(Game1.volume, 0, 0);
        }


        public override void Update(GameTime gameTime)
        {

            if (timer < FRAMES_TO_BURST)
            {
                Position.X += (float)Math.Cos(Angle) * SPEED;
                Position.Y += (float)Math.Sin(Angle) * SPEED;
                Hitbox.Update(Position, Angle);
            }
            else if (timer == FRAMES_TO_BURST)
            {
                crackles = new LinkedList<Crackle>();
                int n = 10;
                for (int i = 0; i < n; i++)
                {
                    crackles.AddFirst(new Crackle(Position, (float)Math.PI * 2 * ((float)i / n)));
                }
                
                Game1.sfx_explosion_short1.Play(Game1.volume, 0, 0);

            }
            else
            {
                foreach (Crackle crack in crackles)
                {
                    crack.Update();
                }
            }


            ScanDamage();
            timer++;
        }

        public override void Draw(float opacity)
        {
            int frameX = (int)((timer / 10) % 8 * 24);

            if (timer <= FRAMES_TO_BURST)
            {
                Game1.self.spriteBatch.Draw(Game1.texture_bullets3C, Position, new Rectangle(frameX, 288, 24, 24), this.color, Angle, new Vector2(12, 12), 1f, SpriteEffects.None, 1);
                Hitbox.DrawDebug();
            }
            else
            {
                foreach (Crackle crack in crackles)
                    crack.Draw(opacity, this.color, frameX);
            }
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
                    return true;
                }
            }
            return false;

        }

        public void Damage(Enemy enemy, int amount)
        {
            enemy.healthData.Damage(null, amount);
            return;
        }
        public override bool IsAlive()
        {
            if (Position.X > Game1.WIDTH || Position.X < 0 || Position.Y < 0 || Position.Y > Game1.HEIGHT)
                return false;

            return true;
        }
    }
}
