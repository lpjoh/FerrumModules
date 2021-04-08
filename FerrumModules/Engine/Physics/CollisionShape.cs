using System;
using Microsoft.Xna.Framework;
using Crossfrog.Ferrum.Engine.Modules;

namespace Crossfrog.Ferrum.Engine.Physics
{
    public class CollisionShape : Entity
    {
        public override Entity Parent
        {
            set
            {
                Scene?.PhysicsWorld.Remove(this);
                base.Parent = value;
                Scene.PhysicsWorld.Add(this);
            }
        }
        public override void Exit()
        {
            base.Exit();
            Scene.PhysicsWorld.Remove(this);
        }
        public Vector2[] Vertices { get; private set; }
        public Vector2[] GlobalVertices
        {
            get
            {
                var transformedVertices = (Vector2[])Vertices.Clone();

                var position = GlobalPosition;
                var scale = GlobalScale;
                var angle = GlobalAngle;
                for (int i = 0; i < transformedVertices.Length; i++)
                    transformedVertices[i] = Rotation.Rotate(transformedVertices[i] * scale, angle + MathHelper.Pi) + position;

                return transformedVertices;
            }
        }
        public Rectangle BoundingBox
        {
            get
            {
                var rectPosition = new Vector2(float.MaxValue, float.MaxValue);
                var rectSize = new Vector2(float.MinValue, float.MinValue);

                var rectVertices = GlobalVertices;
                foreach (var v in rectVertices)
                {
                    rectPosition.X = Math.Min(rectPosition.X, v.X);
                    rectPosition.Y = Math.Min(rectPosition.Y, v.Y);
                    rectSize.X = Math.Max(rectSize.X, v.X);
                    rectSize.Y = Math.Max(rectSize.Y, v.Y);
                }
                rectSize -= rectPosition;

                return new Rectangle((int)rectPosition.X, (int)rectPosition.Y, (int)rectSize.X, (int)rectSize.Y);
            }
        }
        public CollisionShape(params Vector2[] points)
        {
            SetPoints(points);
        }
        public CollisionShape SetPoints(params Vector2[] points)
        {
            Vertices = points;
            return this;
        }
        public CollisionShape SetAsRegularShape(int pointCount, Vector2 scale)
        {
            Vertices = new Vector2[pointCount];

            var angleIncrement = MathHelper.TwoPi / pointCount;
            for (int i = 0; i < Vertices.Length; i++)
            {
                var angle = i * angleIncrement;
                Vertices[i] = new Vector2(MathF.Sin(angle), MathF.Cos(angle)) * scale / 2;
            }
            return this;
        }
        public CollisionShape SetAsRegularShape(int pointCount, float scale = 1.0f)
        {
            return SetAsRegularShape(pointCount, new Vector2(scale, scale));
        }
        public CollisionShape SetAsBox(Vector2 scale)
        {
            Vertices = new Vector2[4];

            var halfScale = scale / 2;
            Vertices[0] = -halfScale;
            Vertices[1] = new Vector2(halfScale.X, -halfScale.Y);
            Vertices[2] = halfScale;
            Vertices[3] = new Vector2(-halfScale.X, halfScale.Y);
            return this;
        }
        public CollisionShape SetAsBox(float scale = 1.0f)
        {
            return SetAsBox(new Vector2(scale, scale));
        }
        public CollisionShape SetAsBox(float scaleX, float scaleY)
        {
            return SetAsBox(new Vector2(scaleX, scaleY));
        }
    }
}
