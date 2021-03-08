using Microsoft.Xna.Framework;

namespace FerrumModules.Engine
{
    public class Camera : Entity
    {
        public Rectangle BoundingBox;
        public float Zoom = 1.0f;
        public override Vector2 GlobalPosition => GlobalPositionNoOffset + PositionOffset;
        public override float GlobalAngle => 0.0f;

        public bool BoundingBoxOnScreen(Rectangle rect, Vector2 parallaxFactor)
        {
            var parallaxCameraRect = new Rectangle((int)(BoundingBox.X * parallaxFactor.X), (int)(BoundingBox.Y * parallaxFactor.Y), BoundingBox.Width, BoundingBox.Height);
            return Collision.RectsCollide(parallaxCameraRect, rect);
        }
    }
}
