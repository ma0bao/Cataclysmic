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
        public const float MANA_COST = 60;
        public const float COOLDOWN = 2f;
        public const int DAMAGE = 20;
        public const int PUSH = 15;
        public const int LIFETIME = 60;

        public CollisionComponent Hitbox;
        public Color color;
        Vector2 Position;
        long timer = 0;
        public float angle;
        public CircleSlash(Vector2 position)
        {
            Position = position;
            Hitbox = CollisionComponent.CreateCircle(position, RADIUS);

            color = Color.White;
        }


        public override void Update(GameTime gameTime)
        {

            Position = Game1.players[0].renderData.Position;
            Hitbox.Update(Position, angle);
            ScanDamage();
            timer++;
            double x = (double)timer / LIFETIME;
            angle += 40 * (float) Math.Abs(Math.Pow(x, 5) - Math.Pow(x, 2)) * 3.0f;
        }

        public override void Draw(float opacity)
        {
            int frameX = (int)((timer / 10) % 8 * 24);

            Game1.self.spriteBatch.Draw(Game1.texture_clockHand, Position, null, color, MathHelper.ToRadians(angle), new Vector2(Game1.texture_clockHand.Width/2, Game1.texture_clockHand.Height/ 2), 1f, SpriteEffects.None, 0f);
            Hitbox.DrawDebug();
        }

        public bool ScanDamage()
        {
            foreach (Enemy e in Game1.enemies)
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
            enemy.healthData.Damage(null, amount);
            return;
        }
        public override bool IsAlive()
        {
            if (timer > LIFETIME)
            {
                return false;
            }
            return true;
        }
    }
}
