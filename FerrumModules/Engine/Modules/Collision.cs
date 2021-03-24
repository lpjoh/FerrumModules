using System;
using Microsoft.Xna.Framework;

namespace Crossfrog.Ferrum.Engine.Modules
{
    public static class Collision
    {
        public static bool RectsCollide(Rectangle rect1, Rectangle rect2, bool includeGrazing = false)
        {
            if (includeGrazing)
                return
                    rect1.X <= rect2.X + rect2.Width &&
                    rect1.Y <= rect2.Y + rect2.Height &&
                    rect1.X + rect1.Width >= rect2.X &&
                    rect1.Y + rect1.Height >= rect2.Y;

            return
                rect1.X < rect2.X + rect2.Width &&
                rect1.Y < rect2.Y + rect2.Height &&
                rect1.X + rect1.Width > rect2.X &&
                rect1.Y + rect1.Height > rect2.Y;
        }
        public static bool RectsCollide(Vector2 rect1Pos, Vector2 rect1Size, Vector2 rect2Pos, Vector2 rect2Size, bool includeGrazing = false)
        {
            if (includeGrazing)
                return
                rect1Pos.X <= rect2Pos.X + rect2Size.X &&
                rect1Pos.Y <= rect2Pos.Y + rect2Size.Y &&
                rect1Pos.X + rect1Size.X >= rect2Pos.X &&
                rect1Pos.Y + rect1Size.Y >= rect2Pos.Y;

            return
                rect1Pos.X < rect2Pos.X + rect2Size.X &&
                rect1Pos.Y < rect2Pos.Y + rect2Size.Y &&
                rect1Pos.X + rect1Size.X > rect2Pos.X &&
                rect1Pos.Y + rect1Size.Y > rect2Pos.Y;
        }
        public static float DifferenceWindow(float moverStart, float moverEnd, float colliderStart, float colliderEnd, float scale)
        {
            if (moverStart <= colliderStart && moverEnd > colliderStart)
                return (colliderStart - moverEnd) / scale;
            else if (colliderStart <= moverStart && colliderEnd > moverStart)
                return (colliderEnd - moverStart) / scale;
            return 0.0f;
        }
        public static void Resolve1D(float moverStart, float moverEnd, float colliderStart, float colliderEnd, ref float velocity, ref float axisRef, float scale, float bounceback)
        {
            if (velocity > 0)
                axisRef -= (moverEnd - colliderStart) / scale;
            else if (velocity < 0)
                axisRef += (colliderEnd - moverStart) / scale;
            velocity = -velocity * bounceback;
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
    }
}