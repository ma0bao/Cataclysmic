using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Cataclysmic
{
    class RevolverWrapper : AbilityWrapper
    {
        const int SHOT_PARTICLES = 16;


        public override void DrawDescription(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(Game1.font_blackadder, "The Colt", new Vector2(1500, 100), Color.Black);
            spriteBatch.DrawString(Game1.font_gabriola, "Damage:     " + Revolver.DAMAGE, new Vector2(1500, 200), Color.Black);
            spriteBatch.DrawString(Game1.font_gabriola, "Cost:           " + Revolver.MANA_COST, new Vector2(1500, 250), Color.Black);
            spriteBatch.DrawString(Game1.font_gabriola, "Cooldown:  " + Revolver.COOLDOWN, new Vector2(1500, 300), Color.Black);

            string desc =
                "An old but reliable relic. This colt has held up across generations\n" +
                "and has been rumored to have special properties.";

            spriteBatch.DrawString(Game1.font_gabriola, desc, new Vector2(1200, 400), Color.Black);

            spriteBatch.Draw(Game1.texture_revolverWrapper, new Rectangle(1210, 90, 270, 270), new Color(64, 44, 28));
            spriteBatch.Draw(Game1.texture_revolverWrapper, new Rectangle(1220, 100, 250, 250), Color.White);
        }

        public override Ability GetAbilityInstance(Vector2 Position, float angle)
        {
            for (int i = 0; i < SHOT_PARTICLES; i++)
            {
                float spread = 0.4f;

                float _angle = angle + ((float)Game1.rand.NextDouble() - 0.5f) * spread - (float)Math.PI * 0.5f;
                float speed = 5f + (float)Game1.rand.NextDouble() * 10f;

                Vector2 velocity = speed * new Vector2(
                    (float)Math.Cos(_angle),
                    (float)Math.Sin(_angle)
                );

                Particle p = new Particle(
                    Position,
                    Game1.texture_blank,
                    new Rectangle(
                        0, 
                        0, 
                        Game1.texture_blank.Width, 
                        Game1.texture_blank.Height
                        ),
                    4, 4,
                    10+Game1.rand.Next(20)
                );

                p.Velocity = velocity;
                p.drag = 0.85f;
                p.Color = Color.Yellow;
                p.Opacity = 1f;
                p.fadeInfadeOut = true;

                int roll = Game1.rand.Next(100);

                if (roll < 40)
                    p.Color = Color.White;
                else if (roll < 75)
                    p.Color = Color.Yellow;
                else if (roll < 95)
                    p.Color = new Color(255, 140, 0);
                else
                    p.Color = new Color(255, 60, 0);

                Game1.self.currentEnvironment.GetParticles().Add(p);
            }

            return new Revolver(Position, angle);
        }

        public override Texture2D GetTexture()
        {
            return Game1.texture_revolverWrapper;
        }

        public override bool UseAbility()
        {
            if (cooldownFrames > 0)
                return false;

            
             cooldownFrames = (int)(Revolver.COOLDOWN * 60); 
            return true;

            /* How to make satisfying "6 shot revolver"?
             1. 


             */

            //return false;
        }

        public override void Update()
        {

            if (cooldownFrames > 0)
                cooldownFrames--;
        }
    }
}