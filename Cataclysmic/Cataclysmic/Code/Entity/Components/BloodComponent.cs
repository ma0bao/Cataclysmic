using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Cataclysmic
{
    public struct BloodHit
    {
        public int Count;
        public float SpeedMin;
        public float SpeedMax;
        public float SizeMult;
        public float LifetimeMult;
        public Vector2? DirectionBias;  // null = radial; set = cone in this direction

        public static BloodHit Light = new BloodHit
        {
            Count = 8,
            SpeedMin = 2f,
            SpeedMax = 6f,
            SizeMult = 1f,
            LifetimeMult = 0.8f,
            DirectionBias = null
        };

        public static BloodHit Medium = new BloodHit
        {
            Count = 15,
            SpeedMin = 2f,
            SpeedMax = 12f,
            SizeMult = 1.1f,
            LifetimeMult = 1f,
            DirectionBias = null
        };

        public static BloodHit Heavy = new BloodHit
        {
            Count = 20,
            SpeedMin = 5f,
            SpeedMax = 18f,
            SizeMult = 1.3f,
            LifetimeMult = 1.4f,
            DirectionBias = null
        };

        public BloodHit WithDirection(Vector2 direction)
        {
            BloodHit copy = this;
            copy.DirectionBias = direction;
            return copy;
        }
    }

    public class BloodComponent
    {
        
        public Texture2D Texture;
        public Rectangle SourceRect = new Rectangle(5, 5, 5, 5);
        public Color Tint = Color.Red;
        public int baseSize = 4;
        public int sizeVariance = 4;
        public float drag = 0.96f;
        public int baseLifetime = 50;
        public float chunkChance = 0.3f;
        public int chunkLifetimeMult = 8;

        // tuning
        public int deathCountBonus = 40;
        public float deathSpeedMult = 1.5f;
        public float deathSizeMult = 1.5f;
        public float deathLifetimeMult = 1.8f;

        readonly Func<Vector2> getCenter;

        public BloodComponent(Func<Vector2> centerProvider)
        {
            getCenter = centerProvider;
            Texture = Game1.texture_blank;
        }

        //spawn blood splatter from hit
        public void Spew(BloodHit hit)
        {
            SpawnParticles(
                count: hit.Count,
                speedMin: hit.SpeedMin,
                speedMax: hit.SpeedMax,
                sizeMult: hit.SizeMult,
                lifetimeMult: hit.LifetimeMult,
                dirBias: hit.DirectionBias);
        }

        // death explosion
        public void Burst()
        {
            int count = deathCountBonus;
            float baseSpeed = Math.Max(baseSize * 0.4f, 4f);

            SpawnParticles(
                count: count,
                speedMin: baseSpeed,
                speedMax: baseSpeed * 2 * deathSpeedMult,
                sizeMult: deathSizeMult,
                lifetimeMult: deathLifetimeMult,
                dirBias: null);

        }

        void SpawnParticles(int count, float speedMin, float speedMax, float sizeMult, float lifetimeMult, Vector2? dirBias)
        {
            List<Particle> list = Game1.self.currentEnvironment.GetParticles();
            if (list == null) return;

            Vector2 center = getCenter();
            Random rand = Game1.rand;

            float biasAngle = 0f;
            bool hasBias = dirBias.HasValue && dirBias.Value.LengthSquared() > 0.0001f;

            if (hasBias)
                biasAngle = (float)Math.Atan2(dirBias.Value.Y, dirBias.Value.X);

            for (int i = 0; i < count; i++)
            {
                float angle;
                if (hasBias)
                {
                    // cone around bias direction
                    float jitter = MathHelper.ToRadians(rand.Next(-30, 31));
                    angle = biasAngle + jitter;
                }
                else
                {
                    angle = MathHelper.ToRadians(rand.Next(360));
                }

                float speed = MathHelper.Lerp(speedMin, speedMax, (float)rand.NextDouble());
                int sizeJitter = (sizeVariance > 0) ? rand.Next(-sizeVariance, sizeVariance + 1) : 0;
                int size = Math.Max(1, (int)((baseSize + sizeJitter) * sizeMult));

                float lifeRand = rand.Next(60, 101);
                float life = Math.Max(1, (int)(baseLifetime * lifetimeMult));
                life *= lifeRand/100f;

                float rotation = rand.Next(0, 360);

                Vector2 velocity = new Vector2(
                    speed * (float)Math.Cos(angle),
                    speed * (float)Math.Sin(angle));

                Particle p = new Particle(center, Texture, SourceRect, size, size, life)
                {
                    Velocity = velocity,
                    drag = drag,
                    Color = Tint,
                    Opacity = 1f,
                    fadeHalf = true,
                    Angle = MathHelper.ToRadians(rotation)
                };

                if (rand.NextDouble() < chunkChance)
                    p.Lifetime = life * chunkLifetimeMult;

                list.Add(p);
            }
        }


    }
}
