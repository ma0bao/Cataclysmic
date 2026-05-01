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
    public class Swap : Ability
    {
        public const float MANA_COST = 30;
        public const float COOLDOWN = 0.2f;
        public const int GRACE_DISTANCE = 70;
        public const int PARTICLE_COUNT = 40;

        public Swap(Vector2 position, float angle)
        {
            foreach (Enemy e in Game1.self.currentEnvironment.GetEnemies()) {
                if (Vector2.Distance(e.renderData.Position, Game1.self.cursor.Position) < GRACE_DISTANCE) {
                    Vector2 temp = e.renderData.Position;
                    e.renderData.Position = Game1.player.renderData.Position;
                    Game1.player.renderData.Position = temp;
                    DrawSwap( Game1.player.renderData.Position);
                    DrawSwap(e.renderData.Position);
                    Game1.sfx_weapon_singleshot2.Play(Game1.volume, -0.9f, 0);
                    return;
                }
                
            }
        }


        public void DrawSwap(Vector2 pos) {
            for (int i = 0; i < PARTICLE_COUNT; i++)
            {
                float _angle = MathHelper.ToRadians(Game1.rand.Next(360));
                float speed = 5f + (float)Game1.rand.NextDouble() * 10f;

                Vector2 velocity = speed * new Vector2(
                    (float)Math.Cos(_angle),
                    (float)Math.Sin(_angle)
                );

                Particle p = new Particle(
                    pos,
                    Game1.texture_blank,
                    new Rectangle(
                        0,
                        0,
                        Game1.texture_blank.Width,
                        Game1.texture_blank.Height
                        ),
                    4, 4,
                    10 + Game1.rand.Next(20)
                );

                p.Velocity = velocity;
                p.drag = 0.85f;
                p.Color = Color.Yellow;
                p.Opacity = 1f;
                p.fadeInfadeOut = true;

                int roll = Game1.rand.Next(100);

                if (roll < 40)
                    p.Color = Color.Purple;
                else if (roll < 75)
                    p.Color = Color.Lavender;
                else if (roll < 95)
                    p.Color = Color.LavenderBlush;
                else
                    p.Color = Color.White;

                Game1.self.currentEnvironment.GetParticles().Add(p);
            }
        }


        public override void Update(GameTime gameTime)
        {

        }

        public override void Draw(float opacity)
        {
        }

        public override bool IsAlive()
        {
            return false;
        }
    }
}
