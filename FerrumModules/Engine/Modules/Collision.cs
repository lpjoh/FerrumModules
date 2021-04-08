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
        private static bool CheckOverlapSAT(Vector2[] shape1, Vector2[] shape2, bool includeGrazing = false)
        {
            for (int i = 0; i < shape1.Length; i++)
            {
                var vertex = shape1[i];
                var nextVertex = shape1[(i + 1) % shape1.Length];

                var edgeNormal = NormalBetween(vertex, nextVertex);
                var firstProjection = ProjectLine(shape1, edgeNormal);
                var secondProjection = ProjectLine(shape2, edgeNormal);

                if (includeGrazing)
                {
                    if (!(firstProjection.Start <= secondProjection.End && firstProjection.End >= secondProjection.Start))
                        return false;
                }
                else
                {
                    if (!(firstProjection.Start < secondProjection.End && firstProjection.End > secondProjection.Start))
                        return false;
                }
            }
            return true;
        }
        public static bool ConvexPolysCollide(Vector2[] shape1, Vector2[] shape2, bool includeGrazing = false)
        {
            return CheckOverlapSAT(shape1, shape2, includeGrazing) && CheckOverlapSAT(shape2, shape1, includeGrazing);
        }

        private static float? ResponseAcrossLine(ProjectionLine line1, ProjectionLine line2)
        {
            if (line1.Start <= line2.Start && line1.End >= line2.Start) // use the >= operator
                return line2.Start - line1.End;
            else if (line2.Start <= line1.Start && line2.End >= line1.Start) // use the >= operator
                return line2.End - line1.Start;
            return null;
        }
        public struct TranslationVector
        {
            public Vector2 Normal;
            public float Magnitude;
        }
        public static TranslationVector MTVBetween(Vector2[] mover, Vector2[] collider)
        {
            var minResponseMagnitude = float.MaxValue;
            var responseNormal = Vector2.Zero;

            for (int i = 0; i < collider.Length; i++)
            {
                var cPoint1 = collider[i];
                var cPoint2 = collider[(i + 1) % collider.Length];
                var cEdgeNormal = NormalBetween(cPoint1, cPoint2);
                cEdgeNormal.Normalize();

                var mProjected = ProjectLine(mover, cEdgeNormal);
                var cProjected = ProjectLine(collider, cEdgeNormal);

                var responseMagnitude = ResponseAcrossLine(mProjected, cProjected);
                if (responseMagnitude != null && Math.Abs(responseMagnitude.Value) < Math.Abs(minResponseMagnitude))
                {
                    minResponseMagnitude = responseMagnitude.Value;
                    responseNormal = cEdgeNormal;
                }
            }
            return new TranslationVector() { Normal = responseNormal, Magnitude = minResponseMagnitude };
        }
        public static Vector2? ResolutionFor(Vector2[] mover, Vector2[] collider)
        {
            if (!ConvexPolysCollide(mover, collider))
                return null;

            var response1 = MTVBetween(mover, collider);
            var response2 = MTVBetween(collider, mover);
            response2.Normal *= -1;

            if (Math.Abs(response1.Magnitude) < Math.Abs(response2.Magnitude))
                return response1.Normal * response1.Magnitude;
            else
                return response2.Normal * response2.Magnitude;
        }
    }
}