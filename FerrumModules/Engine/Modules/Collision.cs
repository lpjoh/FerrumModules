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
        private static ProjectionLine FindProjectedLine(Vector2[] points, Vector2 normal)
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
                var nextIndex = (i + 1) % shape1.Length;

                var vertex = shape1[i];
                var nextVertex = shape1[nextIndex];

                var edgeNormal = NormalBetween(vertex, nextVertex);
                var projection = FindProjectedLine(shape1, edgeNormal);
                var bodyProjection = FindProjectedLine(shape2, edgeNormal);

                if (!(projection.Start <= bodyProjection.End && projection.End >= bodyProjection.Start))
                    return false;
            }
            return true;
        }
        public static bool ConvexPolysCollide(Vector2[] shape1, Vector2[] shape2)
        {
            return CheckOverlapSAT(shape1, shape2) && CheckOverlapSAT(shape2, shape1);
        }
    }
}
