using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FerrumModules.Engine
{
    public class ConvexPolygon
    {
        public readonly VertexPositionColor[] Vertices;
        public readonly Color Color;

        public ConvexPolygon(Color color, params Vector2[] points)
        {
            Color = color;

            var vertexNumber = points.Length;
            Vertices = new VertexPositionColor[vertexNumber];

            for (int i = 0; i < vertexNumber; i++)
            {
                var point = points[i];
                Vertices[i] = new VertexPositionColor(new Vector3(point.X, point.Y, 0), Color);
            }
        }
    }
}
