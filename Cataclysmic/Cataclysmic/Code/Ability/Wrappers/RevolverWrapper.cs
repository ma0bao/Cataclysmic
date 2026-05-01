using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;


namespace Cataclysmic
{
    class RevolverWrapper : AbilityWrapper
    {
        const int SHOT_PARTICLES = 16;
        const int WINDUP_TIME = 20;
        const int GRACE_TIME = 60;
        const int SHOTS = 6;

        int shotsTaken = 0;
        int timer = 0;

        bool inCombo;

        public List<Ability> abilities;

        public RevolverWrapper() {
            abilities = new List<Ability>();
            inCombo = false;
        }


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

            spriteBatch.Draw(Game1.texture_blank, new Rectangle(1210, 90, 270, 270), new Color(64, 44, 28));
            spriteBatch.Draw(Game1.texture_revolverWrapper, new Rectangle(1220, 100, 250, 250), Color.White);
        }

        public override Ability GetAbilityInstance(Vector2 Position, float angle)
        {
            Vector2 RightArm = Position;
            Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

            RightArm += dir * (Game1.player.renderData._destRect.Width / 4f);

            dir = new Vector2((float)Math.Cos(angle - Math.PI * 0.5f), (float)Math.Sin(angle - Math.PI * 0.5f));
            RightArm += dir * 16f;
            if (shotsTaken == SHOTS - 1)
            {
                DrawExhaust(RightArm, angle);
                Game1.sfx_revolver_shot2.Play(Game1.volume, 0.2f * (Game1.rand.NextFloat() - 0.5f), 0);
            }
            else {
                Game1.sfx_revolver_shot1.Play(Game1.volume, 0.2f * (Game1.rand.NextFloat() - 0.5f), 0);
            }
            GunshotParticles(RightArm, angle);
            Game1.Shake(0.2f, 1.0f);
            return new Revolver(RightArm, angle);
        }

        public override Texture2D GetTexture()
        {
            return Game1.texture_revolverWrapper;
        }

        public override void Update(GameTime _gameTime)
        {
            if (cooldownFrames > 0)
                cooldownFrames--;

            foreach (Ability abil in abilities) {
                abil.Update(_gameTime);
            }
            for (int i = abilities.Count - 1; i >= 0; i--)
            {
                if (!abilities[i].IsAlive())
                    abilities.RemoveAt(i);
            }

            if (cooldownFrames <= 0) {
                if (inCombo)
                    timer++;

                if (Game1.player.IsAbilityPressed(abilitySpot) && !inCombo) {
                    inCombo = true;
                    Game1.sfx_revolver_draw1.Play(Game1.volume, 0.2f * (Game1.rand.NextFloat() - 0.5f), 0);
                }

                if (timer > WINDUP_TIME + GRACE_TIME || shotsTaken == SHOTS) {
                    cooldownFrames = (int)(Revolver.COOLDOWN * 60);
                    inCombo = false;
                    timer = 0;
                    shotsTaken = 0;
                }

                if (timer == WINDUP_TIME) {
                    shotsTaken++;
                    abilities.Add(GetAbilityInstance(Game1.player.renderData.Position, Game1.player.angle));
                }

                if (timer > WINDUP_TIME) {
                    if (Game1.player.IsAbilityPressed(abilitySpot))
                    {
                        shotsTaken++;
                        float maxDiffer = shotsTaken * 0.01f;
                        float differ = Game1.rand.NextFloat(maxDiffer) - maxDiffer/2f;
                        abilities.Add(GetAbilityInstance(Game1.player.renderData.Position, Game1.player.angle + differ));
                        timer = WINDUP_TIME + 1;
                    }
                }
                
            }
            
        }

        public override void DrawAbilities()
        {
            if (inCombo) {
                //Game1.self.spriteBatch.Draw(renderData.texture, renderData._destRect, renderData.sourceRect, renderData.color * opacity, MathHelper.ToRadians(renderData.rotation), renderData.origin, SpriteEffects.None, 0f);
                Player temp = Game1.player;
                RenderComponent rc = temp.renderData;
                Game1.self.spriteBatch.Draw(Game1.texture_revolverExtension, rc._destRect, null, rc.color, MathHelper.ToRadians(rc.rotation), new Vector2(16, 16), SpriteEffects.None, 0f);
            }
            foreach (Ability abil in abilities) {
                abil.Draw(1.0f);
            }
        }

        public void GunshotParticles(Vector2 Position, float angle) {
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
                    10 + Game1.rand.Next(20)
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
        }


        public void DrawExhaust(Vector2 Position, float angle)
        {
            for (int i = 0; i < SHOT_PARTICLES*3; i++)
            {
                float spread = 0.6f;

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
                    10 + Game1.rand.Next(20)
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
                    p.Color = Color.Gray;
                else if (roll < 95)
                    p.Color = Color.DarkGray;
                else
                    p.Color = Color.Black;

                Game1.self.currentEnvironment.GetParticles().Add(p);
            }
        }

        public override int GetMaxCooldown()
        {
            return (int)(Revolver.COOLDOWN * 60);
        }
    }
}