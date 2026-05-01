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
            static Color[] colors = { new Color(255, 251, 199), new Color(255, 247, 154), new Color(255, 196, 0), new Color(255, 166, 0) };
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
                if (Game1.rand.Next(5) == 1) {
                    Particle p = new Particle(Position, Game1.texture_blank, new Rectangle(0, 0, 1, 1), 3, 3, Game1.rand.Next(50, 120));
                    p.Color = colors[Game1.rand.Next(3)];
                    p.fadeHalf = true;
                    float speed = 1.0f;
                    p.Velocity = new Vector2(speed * ((float)Game1.rand.NextDouble() - 0.5f), speed * ((float)Game1.rand.NextDouble() - 0.5f));// halfSpeed*((float)Game1.rand.NextDouble() - 0.5f);
                    Game1.self.currentEnvironment.GetParticles().Add(p);
                }
                Hitbox.UpdatePosition(Position);
            }

            public void Draw(float opacity, Color color, int frameY)
            {
                Game1.self.spriteBatch.Draw(Game1.texture_crackleParticle, new Rectangle((int)Position.X, (int)Position.Y, WIDTH, WIDTH), new Rectangle(0, 12*frameY, 12, 12), color, angle+Game1.timer/10%360, new Vector2(6, 6), SpriteEffects.None, 1);
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
                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                enemy.Damage(null, amount, CRACKLE_BLOOD.WithDirection(dir));

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
        public const float MANA_COST = 30;
        public const float SPAWN_OFFSET = 20f;
        public const float COOLDOWN = 1.0f;
        public const int DAMAGE = 20;
        static Color[] trailColors = { Color.White, Color.Red, Color.Blue };
        public static readonly BloodHit BLOOD = new BloodHit
        {
            Count = 20,
            SpeedMin = 8f,
            SpeedMax = 22f,
            SizeMult = 1.2f,
            LifetimeMult = 0.9f
        };
        public static readonly BloodHit CRACKLE_BLOOD = new BloodHit
        {
            Count = 6,
            SpeedMin = 3f,
            SpeedMax = 10f,
            SizeMult = 1f,
            LifetimeMult = 0.8f
        };


        public CollisionComponent Hitbox;
        public Color color;
        Vector2 Position;
        float Angle;
        long timer;
        LinkedList<Crackle> crackles;

        public CrackleBurst(Vector2 position, float angle)
        {
            Game1.player.timeEnergy.Decrease(MANA_COST);
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
                ScanDamage();

                // Trail Particles
                float angleOfProjectile = Angle + (float)Math.Cos(timer / 10.0) * 0.3f;
                if (Game1.rand.Next(1) == 0)
                {
                    Particle p = new Particle(Position, Game1.texture_blank, new Rectangle(0, 0, 1, 1), 3, 3, Game1.rand.Next(50, 120));
                    p.Color = trailColors[Game1.rand.Next(3)];
                    float speed = 1.0f;
                    p.fadeHalf = true;
                    p.Velocity = new Vector2(speed * ((float)Game1.rand.NextDouble() - 0.5f), speed * ((float)Game1.rand.NextDouble() - 0.5f));// halfSpeed*((float)Game1.rand.NextDouble() - 0.5f);
                    Game1.self.currentEnvironment.GetParticles().Add(p);
                }

            }
            else if (timer == FRAMES_TO_BURST)
            {
                crackles = new LinkedList<Crackle>();
                int n = 15;
                for (int i = 0; i < n; i++)
                {
                    float angleOfTraj = (float)Math.PI * 2 * ((float)i / n);
                    angleOfTraj += ((float)Game1.rand.NextDouble() - 0.5f) * angleOfTraj*2/n;
                    crackles.AddFirst(new Crackle(Position, angleOfTraj));
                }
                Game1.Shake(0.5f, 2.0f);
                Game1.sfx_explosion_short1.Play(Game1.volume, 0, 0);

            }
            else
            {
                foreach (Crackle crack in crackles)
                {
                    crack.Update();
                }
            }

            
            timer++;
        }

        public override void Draw(float opacity)
        {
            int frameX = (int)((timer / 10) % 4);

            if (timer <= FRAMES_TO_BURST)
            {
                Game1.self.spriteBatch.Draw(Game1.texture_crackleBurstMissile, new Rectangle((int)Position.X, (int)Position.Y, 48, 26), new Rectangle(0, 0, 24, 13), this.color, Angle+(float)Math.Cos(timer / 10.0) * 0.2f, new Vector2(12, 6.5f), SpriteEffects.None, 1.0f);
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
            enemy.Damage(null, amount, BLOOD);
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
