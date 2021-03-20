using System;

using Microsoft.Xna.Framework;

using Crossfrog.FerrumEngine.Modules;

namespace Crossfrog.FerrumEngine.Entities
{
    public class CollisionBody : Entity
    {
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
                    transformedVertices[i] = Rotation.Rotate(transformedVertices[i] * scale, angle) + position;

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
        private float DotProduct(Vector2 v1, Vector2 v2)
        {
            return (v1.X * v2.X) + (v1.Y * v2.Y);
        }
        private struct ProjectionLine
        {
            public float Start;
            public float End;
        }
        private ProjectionLine FindProjectedLine(Vector2[] points, Vector2 normal)
        {
            var projectionLine = new ProjectionLine() { Start = float.MaxValue, End = float.MinValue };
            foreach (var p in points)
            {
                var projectionScale = DotProduct(p, normal);
                projectionLine.Start = Math.Min(projectionScale, projectionLine.Start);
                projectionLine.End = Math.Max(projectionScale, projectionLine.End);
            }
            return projectionLine;
        }
        private bool CheckOverlapSAT(Vector2[] shape1, Vector2[] shape2)
        {
            for (int i = 0; i < shape1.Length; i++)
            {
                var nextIndex = (i + 1) % shape1.Length;

                var vertex = shape1[i];
                var nextVertex = shape1[nextIndex];

                var edgeNormal = new Vector2(-(vertex.Y - nextVertex.Y), vertex.X - nextVertex.X);
                var projection = FindProjectedLine(shape1, edgeNormal);
                var bodyProjection = FindProjectedLine(shape2, edgeNormal);

                if (!(projection.Start <= bodyProjection.End && projection.End >= bodyProjection.Start))
                    return false;
            }

            return true;
        }
        public bool CollidesWith(CollisionBody body)
        {
            var globalVertices = GlobalVertices;
            var bodyGlobalVertices = body.GlobalVertices;

            return CheckOverlapSAT(globalVertices, bodyGlobalVertices) && CheckOverlapSAT(bodyGlobalVertices, globalVertices);
        }

        public override void Init()
        {
            base.Init();
            Scene.PhysicsWorld.Add(this);
        }

        public override void Exit()
        {
            base.Exit();
            Scene.PhysicsWorld.Remove(this);
        }

        public void SetPoints(params Vector2[] points)
        {
            Vertices = points;
        }
        public void SetAsRegularShape(int pointCount, Vector2 scale)
        {
            Vertices = new Vector2[pointCount];

            var angleIncrement = MathHelper.TwoPi / pointCount;
            for (int i = 0; i < Vertices.Length; i++)
            {
                var angle =  i * angleIncrement;
                Vertices[i] = new Vector2(MathF.Sin(angle), MathF.Cos(angle)) * scale / 2;
            }
        }
        public void SetAsRegularShape(int pointCount, float scale = 1.0f)
        {
            SetAsRegularShape(pointCount, new Vector2(scale, scale));
        }
        public void SetAsBox(Vector2 scale)
        {
            Vertices = new Vector2[4];

            var halfScale = scale / 2;
            Vertices[0] = -halfScale;
            Vertices[1] = new Vector2(halfScale.X, -halfScale.Y);
            Vertices[2] = halfScale;
            Vertices[3] = new Vector2(-halfScale.X, halfScale.Y);
        }
        public void SetAsBox(float scale = 1.0f)
        {
            SetAsBox(new Vector2(scale, scale));
        }
    }
}
