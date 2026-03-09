using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cataclysmic
{
    // AI Generated explanation on SAT its 3 am im too tired of commenting
    // Polygon-based collision using SAT (Separating Axis Theorem)
    // 
    // HOW IT WORKS:
    // - Any shape (rectangle, circle, irregular) is stored as a list of corner points (vertices)
    // - To check collision, we look at each edge and ask: "Is there a gap between the shapes along this edge's perpendicular?"
    // - If ANY edge shows a gap, the shapes don't collide
    // - If NO edge shows a gap, the shapes collide, and we know the smallest overlap (depth) and direction (normal)
    //
    // VISUAL EXPLANATION:
    // Imagine shining a flashlight perpendicular to each edge and looking at the shadows
    // If the shadows overlap on ALL edges, the shapes collide
    // If the shadows have a gap on ANY edge, no collision
    public class CollisionComponent //if anything else is not intuitive tell me ill write a comment
    {
        public Vector2 Center;    // World position of the shape center
        public float Rotation;    // rotation in radians

        // Vertices defined relative to center (0,0), before rotation
        // never change after creation
        // ex for 20x20 rect: (-10,-10), (10,-10), (10,10), (-10,10)
        public Vector2[] LocalVertices;

        // Vertices transformed to world position (rotated and translated)
        // These DO update whenever Center or Rotation changes
        private Vector2[] worldVertices;

        //ONLY WORKS WITH CONVEX POLYGONS ONLY WORKS WITH CONVEX POLYGONS ONLY WORKS WITH CONVEX POLYGONS 
        //ONLY WORKS WITH CONVEX POLYGONS ONLY WORKS WITH CONVEX POLYGONS ONLY WORKS WITH CONVEX POLYGONS 
        //ONLY WORKS WITH CONVEX POLYGONS ONLY WORKS WITH CONVEX POLYGONS ONLY WORKS WITH CONVEX POLYGONS 
        //ONLY WORKS WITH CONVEX POLYGONS ONLY WORKS WITH CONVEX POLYGONS ONLY WORKS WITH CONVEX POLYGONS 
        //ONLY WORKS WITH CONVEX POLYGONS ONLY WORKS WITH CONVEX POLYGONS ONLY WORKS WITH CONVEX POLYGONS 
        public CollisionComponent(Vector2 center, Vector2[] localVertices)
        {
            Center = center;
            Rotation = 0f;
            LocalVertices = localVertices;
            worldVertices = new Vector2[localVertices.Length];
            UpdateWorldVertices();
        }

        // Creates a rectangular collision shape
        // center = center position in world
        public static CollisionComponent CreateRect(Vector2 center, float width, float height)
        {
            float hw = width / 2;
            float hh = height / 2;

            return new CollisionComponent(center, new Vector2[] {
                new Vector2(-hw, -hh), // top left
                new Vector2(hw, -hh),  // top right
                new Vector2(hw, hh),   // bottom right
                new Vector2(-hw, hh)   // bottom left
            }); //is like this cus up is -y and down is +y
        }

        // center = center position in world
        // segments = number of points around the circle
        // default is 8 but can be changed if bad
        public static CollisionComponent CreateCircle(Vector2 center, float radius, int segments = 8)
        {
            var verts = new Vector2[segments];

            for (int i = 0; i < segments; i++)
            {
                // divide full circle into equal parts, i hope u took precalc
                float angle = (float)(i * 2 * Math.PI / segments);

                // more precalc
                verts[i] = new Vector2(
                    (float)Math.Cos(angle) * radius,
                    (float)Math.Sin(angle) * radius
                );
            }

            return new CollisionComponent(center, verts);
        }

        // when object moves
        public void UpdatePosition(Vector2 newCenter)
        {
            Center = newCenter;
            UpdateWorldVertices();
        }

        // when object rotates
        public void UpdateRotation(float newRotation)
        {
            Rotation = newRotation;
            UpdateWorldVertices();
        }

        // when object moves and rotates!!!
        public void Update(Vector2 newCenter, float newRotation)
        {
            Center = newCenter;
            Rotation = newRotation;
            UpdateWorldVertices();
        }

        // Transforms LocalVertices to worldVertices by rotating then translating using rotation math i totaly didnt copy:
        // To rotate point (x,y) by angle θ around origin:
        //   newX = x * cos(θ) - y * sin(θ)
        //   newY = x * sin(θ) + y * cos(θ)
        private void UpdateWorldVertices()
        {
            float cos = (float)Math.Cos(Rotation);
            float sin = (float)Math.Sin(Rotation);

            for (int i = 0; i < LocalVertices.Length; i++)
            {
                // Rotate the local vertex around origin (0,0)
                float x = LocalVertices[i].X * cos - LocalVertices[i].Y * sin;
                float y = LocalVertices[i].X * sin + LocalVertices[i].Y * cos;

                // Translate to world position by adding Center, explanation on center above
                worldVertices[i] = new Vector2(x + Center.X, y + Center.Y);
            }
        }

        // Returns the vertices in world position (for debug drawing)
        public Vector2[] GetWorldVertices()
        {
            return worldVertices;
        }

        // What this does: Checks if this shape collides with another using SAT
        // Le Algorithm:
        // step 1. For each edge of both shapes, get the perpendicular axis (normal)
        // step 2. Project both shapes onto that axis (like shadows from a flashlight, imagine flatland if u read that)
        // step 3. If shadows don't overlap on any axis, no collision (early exit yipee)
        // step 4. If shadows overlap on all axes, collision. Track the smallest overlap so can be pushed later
        // Stuff returned using out:
        // returns true/false: if shapes overlap
        // depth: smallest overlap amount (use this to push apart)
        // normal: direction to push THIS object AWAY from other
        //
        // How To Use : -------------------------------------------------------------------------------------------------------- just read this if nothing else
        // float depth;
        // Vector2 normal;
        // if (a.Intersects(b, out depth, out normal))
        //     a.Position += normal * depth;  // push a away from b
        //     do whatever else. damage, delete bullet etc.
        public bool Intersects(CollisionComponent other, out float depth, out Vector2 normal) //out is the most optimized apparantly
        {
            float minDepth = float.MaxValue;  // Track smallest overlap
            Vector2 minNormal = Vector2.Zero; // Track axis with smallest overlap

            //Check all axes from THIS shape edges
            for (int i = 0; i < worldVertices.Length; i++)
            {
                // wrap around
                int next = i + 1;
                if (next >= worldVertices.Length)
                    next = 0;

                Vector2 edge = worldVertices[next] - worldVertices[i];

                // perpendicular axis (normal to edge)
                Vector2 axis = new Vector2(-edge.Y, edge.X);

                // have to normalize axis length so calculation works 
                float len = axis.Length();
                if (len > 0)
                    axis /= len;

                // project BOTH polygons onto this axis
                // cool way i saw to set variables
                float minA, maxA, minB, maxB;
                ProjectPolygon(worldVertices, axis, out minA, out maxA);
                ProjectPolygon(other.worldVertices, axis, out minB, out maxB);

                // Check for gap between shadows
                if (maxA < minB || maxB < minA)
                {
                    // if gap found, no collision
                    depth = 0;
                    normal = Vector2.Zero;
                    return false;
                }

                // Calculate how much the shadows overlap. get min overlap
                float overlap = Math.Min(maxA - minB, maxB - minA);
                if (overlap < minDepth)
                {
                    minDepth = overlap;
                    minNormal = axis;
                }
                //minNormal * minDepth is the shortest path allegedly
            }

            //now check from other shape
            for (int i = 0; i < other.worldVertices.Length; i++)
            {
                int next = i + 1;
                if (next >= other.worldVertices.Length)
                    next = 0;

                Vector2 edge = other.worldVertices[next] - other.worldVertices[i];
                Vector2 axis = new Vector2(-edge.Y, edge.X);

                float len = axis.Length();
                if (len > 0)
                    axis /= len;


                float minA, maxA, minB, maxB;
                ProjectPolygon(worldVertices, axis, out minA, out maxA);
                ProjectPolygon(other.worldVertices, axis, out minB, out maxB);

                if (maxA < minB || maxB < minA)
                {
                    depth = 0;
                    normal = Vector2.Zero;
                    return false;
                }

                float overlap = Math.Min(maxA - minB, maxB - minA);
                if (overlap < minDepth)
                {
                    minDepth = overlap;
                    minNormal = axis;
                }
            }

            // must be a collision now i hope
            // make sure normal points from this to the other object. uses Dot() to check if pointing correct way (my new fav method)
            Vector2 direction = other.Center - Center;
            if (Vector2.Dot(direction, minNormal) < 0)
                minNormal = -minNormal;

            depth = minDepth;
            normal = minNormal;
            return true;
        }

        // project polygon onto an axis and gets the shadow
        private void ProjectPolygon(Vector2[] poly, Vector2 axis, out float min, out float max)
        {
            min = Vector2.Dot(poly[0], axis); // tells u how far along the axis is this point.
            max = min;

            // Check all other vertices, update shadow
            for (int i = 1; i < poly.Length; i++)
            {
                float proj = Vector2.Dot(poly[i], axis);
                if (proj < min) min = proj;
                if (proj > max) max = proj;
            }
        }

        // finds smallest rectangle around shape. for dubugg if needed. can delete later
        public Rectangle GetBounds()
        {
            float minX = worldVertices[0].X;
            float maxX = worldVertices[0].X;
            float minY = worldVertices[0].Y;
            float maxY = worldVertices[0].Y;

            for (int i = 1; i < worldVertices.Length; i++)
            {
                if (worldVertices[i].X < minX) minX = worldVertices[i].X;
                if (worldVertices[i].X > maxX) maxX = worldVertices[i].X;
                if (worldVertices[i].Y < minY) minY = worldVertices[i].Y;
                if (worldVertices[i].Y > maxY) maxY = worldVertices[i].Y;
            }

            return new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
        }

        // Draws the hitbox outline for debugging
        // color: outline color (default red at 80% opacity)
        // thickness: line thickness in pixels (default 2)
        public void DrawDebug(float thickness = 2f)
        {
            Color drawColor = Color.Red * 0.5f;

            for (int i = 0; i < worldVertices.Length; i++)
            {
                Vector2 start = worldVertices[i];
                Vector2 end = worldVertices[(i + 1) % worldVertices.Length];
                DrawLine(start, end, drawColor, thickness);
            }
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();

            Game1.self.spriteBatch.Draw(Game1.texture_blank, start, null, color, angle, Vector2.Zero, new Vector2(length, thickness), SpriteEffects.None, 1);
        }
    }
}
