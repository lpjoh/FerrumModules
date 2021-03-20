using Microsoft.Xna.Framework;

namespace Crossfrog.FerrumEngine
{
    public class Camera : Entity
    {
        public float Zoom = 1.0f;
        public override Vector2 GlobalPosition => GlobalPositionNoOffset + PositionOffset;
        public override float GlobalAngle => 0.0f;
    }
}
