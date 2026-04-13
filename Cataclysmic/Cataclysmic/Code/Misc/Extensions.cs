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
    }

    public static class PointExtensions
    {
        public static Vector2 ToVector(this Point value)
        {
            return new Vector2(value.X, value.Y);
        }
    }

}
