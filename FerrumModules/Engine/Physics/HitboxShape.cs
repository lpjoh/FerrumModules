using Microsoft.Xna.Framework;
using Crossfrog.Ferrum.Engine.Modules;

namespace Crossfrog.Ferrum.Engine.Physics
{
    public class HitboxShape : CollisionShape
    {
        public override float GlobalAngle => 0.0f;
        public Vector2 BoxSize;
        public Vector2 GlobalExtents => BoxSize * GlobalScale / 2;

        public HitboxShape(Vector2 boxSize)
        {
            BoxSize = boxSize;
        }
        public HitboxShape(float sizeX, float sizeY)
        {
            BoxSize = new Vector2(sizeX, sizeY);
        }
        public override Vector2[] GlobalVertices
        {
            get
            {
                var boxVertices = new Vector2[4];

                var halfBoxSize = GlobalExtents;
                boxVertices[0] = -halfBoxSize;
                boxVertices[1] = new Vector2(halfBoxSize.X, -halfBoxSize.Y);
                boxVertices[2] = halfBoxSize;
                boxVertices[3] = new Vector2(-halfBoxSize.X, halfBoxSize.Y);

                for (int i = 0; i < boxVertices.Length; i++)
                    boxVertices[i] += GlobalPosition;

                return boxVertices;
            }
        }
        public override Rectangle BoundingBox
        {
            get
            {
                var size = BoxSize * GlobalScale;
                var position = GlobalPosition - (size / 2);
                return new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y);
            }
        }
    }
}
