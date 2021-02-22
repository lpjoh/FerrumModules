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
    }
}
