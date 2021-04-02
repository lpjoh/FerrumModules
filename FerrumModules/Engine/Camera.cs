using Microsoft.Xna.Framework;

namespace Crossfrog.Ferrum.Engine
{
    public class Camera : Entity
    {
        public float Zoom = 1.0f;
        public override Vector2 GlobalPosition => GlobalPositionNoOffset + PositionOffset;
        public override float GlobalAngle => 0.0f;
        public Vector2 ScrollClampStart;
        public Vector2 ScrollClampEnd;
        public Camera()
        {
            ResetScrollClamp();
        }
        public void ResetScrollClamp()
        {
            ScrollClampStart = new Vector2(float.MinValue, float.MinValue);
            ScrollClampEnd = new Vector2(float.MaxValue, float.MaxValue);
        }
    }
}
