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
    public class CircleSlash : Ability
    {
        public const int RADIUS = 120;
        public const float MANA_COST = 50;
        public const float COOLDOWN = 2f;
        public const int DAMAGE = 20;
        public const int PUSH = 15;
        public const int LIFETIME = 60;

        public CollisionComponent Hitbox;
        public Color color;
        Vector2 Position;
        long timer = 0;
        public float angle;
        public float rotateSpeed;
        public CircleSlash(Vector2 position)
        {
            Game1.player.timeEnergy.Decrease(MANA_COST);
            Position = position;
            Hitbox = CollisionComponent.CreateCircle(position, RADIUS);

            color = Color.White;
        }


        public override void Update(GameTime gameTime)
        {
            Game1.player.healthData.frames = 100;
            Game1.player.moveData.maxSpeed += 10;
            Position = Game1.player.renderData.Position;
            Hitbox.Update(Position, angle);
            ScanDamage();
            timer++;
            double x = (double)timer / LIFETIME;
            rotateSpeed += 1;// 40 * (float) Math.Abs(Math.Pow(x, 5) - Math.Pow(x, 2)) * 3.0f;
            angle += rotateSpeed;
        }

        public override void Draw(float opacity)
        {
            int frameX = (int)((timer / 10) % 8 * 24);

            Game1.self.spriteBatch.Draw(Game1.texture_clockHand, Position, null, color, MathHelper.ToRadians(angle), new Vector2(Game1.texture_clockHand.Width/2, Game1.texture_clockHand.Height/ 2), 1f, SpriteEffects.None, 1f);
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
                    e.Stagger(.1f, true);
                    e.moveData.velocity = Game1.player.moveData.velocity;
                    e.UpdatePos(2);
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
            if (timer > LIFETIME)
            {
                Game1.player.moveData.maxSpeed = Game1.player.maxSpeed;
                return false;
            }
            
            return true;
        }
    }
}
