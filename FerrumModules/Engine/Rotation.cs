using System;
using Microsoft.Xna.Framework;

namespace FerrumModules.Engine
{
    public static class Rotation
    {
        public static Vector2 Rotate(Vector2 v, float angle)
        {
            var ca = (float)Math.Cos(angle);
            var sa = (float)Math.Sin(angle);
            return new Vector2
                (
                    v.X * ca - v.Y * sa,
                    v.Y * ca + v.X * sa
                );
        }
        public static float PI
        {
            get { return (float)Math.PI; }
        }

        public static Rectangle RotatedRectAABB(Rectangle rectangle, float angle)
        {
            var topLeft = Rotate(new Vector2(rectangle.X, rectangle.Y), angle);
            var topRight = Rotate(new Vector2(rectangle.X + rectangle.Width, rectangle.Y), angle);
            var bottomLeft = Rotate(new Vector2(rectangle.X, rectangle.Y + rectangle.Height), angle);
            var bottomRight = Rotate(new Vector2(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height), angle);

            float minX = Math.Min(topLeft.X, Math.Min(topRight.X, Math.Min(bottomLeft.X, bottomRight.X)));
            float maxX = Math.Max(topLeft.X, Math.Max(topRight.X, Math.Max(bottomLeft.X, bottomRight.X)));
            float minY = Math.Min(topLeft.Y, Math.Min(topRight.Y, Math.Min(bottomLeft.Y, bottomRight.Y)));
            float maxY = Math.Max(topLeft.Y, Math.Max(topRight.Y, Math.Max(bottomLeft.Y, bottomRight.Y)));
            Vector2 min = new Vector2(minX, minY);
            Vector2 max = new Vector2(maxX, maxY);

            return new Rectangle(
                (int)min.X-1,
                (int)min.Y - 1,
                (int)max.X - (int)min.X + 1,
                (int)max.Y - (int)min.Y + 1); // Adding one avoids truncation issues
        }

        public static Vector2 RotatedRectSizeByCenter(Vector2 rectSize, float angle)
        {
            var rectWidth = (int)rectSize.X;
            var rectHeight = (int)rectSize.Y;

            var rotatedRect = RotatedRectAABB(
                new Rectangle(rectWidth / -2, rectHeight / -2, rectWidth, rectHeight), angle);

            return new Vector2(rotatedRect.Width, rotatedRect.Height);
        }
    }
}
