using System;
using Microsoft.Xna.Framework;

namespace Crossfrog.Ferrum.Engine.Modules
{
    public static class Collision
    {
        public static bool RectsCollide(Rectangle rect1, Rectangle rect2)
        {
            return
                rect1.X <= rect2.X + rect2.Width &&
                rect1.Y <= rect2.Y + rect2.Height &&
                rect1.X + rect1.Width >= rect2.X &&
                rect1.Y + rect1.Height >= rect2.Y;
        }
        private static float DotProduct(Vector2 v1, Vector2 v2)
        {
            return (v1.X * v2.X) + (v1.Y * v2.Y);
        }
        private static Vector2 NormalBetween(Vector2 v1, Vector2 v2)
        {
            return new Vector2(-(v1.Y - v2.Y), v1.X - v2.X);
        }
        private struct ProjectionLine
        {
            public float Start;
            public float End;
        }
        private static ProjectionLine ProjectLine(Vector2[] points, Vector2 normal)
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
        private static bool CheckOverlapSAT(Vector2[] shape1, Vector2[] shape2)
        {
            for (int i = 0; i < shape1.Length; i++)
            {
                var vertex = shape1[i];
                var nextVertex = shape1[(i + 1) % shape1.Length];

                var edgeNormal = NormalBetween(vertex, nextVertex);
                var firstProjection = ProjectLine(shape1, edgeNormal);
                var secondProjection = ProjectLine(shape2, edgeNormal);

                if (!(firstProjection.Start <= secondProjection.End && firstProjection.End >= secondProjection.Start))
                    return false;
            }
            return true;
        }
        public static bool ConvexPolysCollide(Vector2[] shape1, Vector2[] shape2)
        {
            return CheckOverlapSAT(shape1, shape2) && CheckOverlapSAT(shape2, shape1);
        }

        private static float? CollisionResponseAcrossLine(ProjectionLine line1, ProjectionLine line2)
        {
            if (line1.Start <= line2.Start && line1.End > line2.Start)
                return line2.Start - line1.End;
            else if (line2.Start <= line1.Start && line2.End > line1.Start)
                return line2.End - line1.Start;
            return null;
        }
        public static Vector2 MTVBetween(Vector2[] mover, Vector2[] collider)
        {
            if (!ConvexPolysCollide(mover, collider))
                return Vector2.Zero;

            float minResponseMagnitude = float.MaxValue;
            var responseNormal = Vector2.Zero;

            for (int c = 0; c < collider.Length; c++)
            {
                var cPoint = collider[c];
                var cNextPoint = collider[(c + 1) % collider.Length];

                var cEdgeNormal = NormalBetween(cPoint, cNextPoint);

                var cProjected = ProjectLine(collider, cEdgeNormal);
                var mProjected = ProjectLine(mover, cEdgeNormal);

                var responseMagnitude = CollisionResponseAcrossLine(cProjected, mProjected);
                if (responseMagnitude != null && responseMagnitude < minResponseMagnitude)
                {
                    minResponseMagnitude = (float)responseMagnitude;
                    responseNormal = cEdgeNormal;
                }
            }

            var normalLength = responseNormal.Length();
            responseNormal /= normalLength;
            minResponseMagnitude /= normalLength;

            var mtv = responseNormal * minResponseMagnitude;
            
            return mtv;
        }
    }
}
