using System;
using Microsoft.Xna.Framework;

namespace FerrumModules.Engine
{
    public class Camera : Entity
    {
        public Rectangle BoundingBox;
        public float Zoom = 1.0f;
        public override float GlobalAngle { get { return 0.0f; } }
    }
}
