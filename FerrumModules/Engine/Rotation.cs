using System;
using Microsoft.Xna.Framework;

namespace FerrumModules.Engine
{
    public static class Rotation
    {
        public static Vector2 Rotate(Vector2 v, float angle)
        {
            if (angle == 0.0f) return v;

            var sa = MathF.Sin(angle);
            var ca = MathF.Cos(angle);
            return new Vector2
                (
                    v.X * ca - v.Y * sa,
                    v.Y * ca + v.X * sa
                );
        }

        public static Rectangle RotatedRectAABB(Vector2 rectSize, Vector2 rotationCenterOffset, Vector2 positionOffset, float angle)
        {
            Rectangle rotatedRectAABB;

            var rectPosition = (rectSize / -2) + rotationCenterOffset;

            if (angle == 0.0f)
            {
                rotatedRectAABB = new Rectangle((int)rectPosition.X, (int)rectPosition.Y, (int)rectSize.X, (int)rectSize.Y);
            }
            else
            {
                var topLeft = Rotate(rectPosition, angle);
                var topRight = Rotate(new Vector2(rectPosition.X + rectSize.X, rectPosition.Y), angle);
                var bottomLeft = Rotate(new Vector2(rectPosition.X, rectPosition.Y + rectSize.Y), angle);
                var bottomRight = Rotate(new Vector2(rectPosition.X + rectSize.X, rectPosition.Y + rectSize.Y), angle);

                float minX = Math.Min(topLeft.X, Math.Min(topRight.X, Math.Min(bottomLeft.X, bottomRight.X)));
                float maxX = Math.Max(topLeft.X, Math.Max(topRight.X, Math.Max(bottomLeft.X, bottomRight.X)));
                float minY = Math.Min(topLeft.Y, Math.Min(topRight.Y, Math.Min(bottomLeft.Y, bottomRight.Y)));
                float maxY = Math.Max(topLeft.Y, Math.Max(topRight.Y, Math.Max(bottomLeft.Y, bottomRight.Y)));
                Vector2 min = new Vector2(minX, minY);
                Vector2 max = new Vector2(maxX, maxY);
                rotatedRectAABB = new Rectangle(
                    (int)min.X,
                    (int)min.Y,
                    (int)max.X - (int)min.X,
                    (int)max.Y - (int)min.Y);
            }

            rotatedRectAABB.X -= 1;
            rotatedRectAABB.Y -= 1;
            rotatedRectAABB.Width += 1;
            rotatedRectAABB.Height += 1; // Adding one avoids truncation issues

            rotatedRectAABB.X += (int)positionOffset.X;
            rotatedRectAABB.Y += (int)positionOffset.Y;

            return rotatedRectAABB;
        }
    }
}
