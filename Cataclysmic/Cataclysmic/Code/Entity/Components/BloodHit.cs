using Microsoft.Xna.Framework;

namespace Cataclysmic
{
    /// <summary>
    /// Per-attack splatter description. Combined with a BloodComponent at hit
    /// time to produce particles. The component supplies the visual identity
    /// (texture, tint, base size); the hit supplies the violence (count,
    /// speed, direction).
    /// </summary>
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
            Count = 4,
            SpeedMin = 1f,
            SpeedMax = 6f,
            SizeMult = 1f,
            LifetimeMult = 0.8f,
            DirectionBias = null
        };

        public static BloodHit Medium = new BloodHit
        {
            Count = 10,
            SpeedMin = 2f,
            SpeedMax = 12f,
            SizeMult = 1.1f,
            LifetimeMult = 1f,
            DirectionBias = null
        };

        public static BloodHit Heavy = new BloodHit
        {
            Count = 18,
            SpeedMin = 5f,
            SpeedMax = 18f,
            SizeMult = 1.3f,
            LifetimeMult = 1.4f,
            DirectionBias = null
        };

        /// <summary>
        /// Returns a copy of this hit with a directional cone instead of radial spray.
        /// Use at attack time when you have the strike angle.
        /// </summary>
        public BloodHit WithDirection(Vector2 direction)
        {
            BloodHit copy = this;
            copy.DirectionBias = direction;
            return copy;
        }
    }
}
