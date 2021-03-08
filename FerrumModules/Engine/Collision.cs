using System;
using Microsoft.Xna.Framework;

namespace FerrumModules.Engine
{
    public static class Collision
    {
        public static bool RectsCollide(Rectangle rect1, Rectangle rect2)
        {
            return
                rect1.X < rect2.X + rect2.Width &&
                rect1.Y < rect2.Y + rect2.Height &&
                rect1.X + rect1.Width > rect2.X &&
                rect1.Y + rect1.Height > rect2.Y;
        }
    }
}
