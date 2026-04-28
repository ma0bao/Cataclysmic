using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    public static class VectorExtensions
    {
        public static Point ToPoint(this Vector2 value)
        {
            return new Point((int)value.X, (int)value.Y);
        }
        public static Vector2 GetRandomized(this Vector2 value)
        {
            return new Vector2(Game1.rand.NextFloat(-value.X, value.X), Game1.rand.NextFloat(-value.Y, value.Y));
        }
    }

    public static class PointExtensions
    {
        public static Vector2 ToVector(this Point value)
        {
            return new Vector2(value.X, value.Y);
        }
    }

    public static class RandomExtensions
    {
        public static float NextFloat(this Random rand, float upperBound = 1)
        {
            return (float)(rand.NextDouble() * upperBound);
        }

        public static float NextFloat(this Random rand, float lowerBound, float upperBound)
        {
            return (float)(rand.NextDouble() * (upperBound-lowerBound) + lowerBound);
        }
    }

}
