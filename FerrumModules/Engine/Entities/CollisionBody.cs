using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
                var scale = Parent?.GlobalScale ?? ScaleOffset;
                var angle = GlobalAngle;
                for (int i = 0; i < transformedVertices.Length; i++)
                    transformedVertices[i] = Rotation.Rotate(transformedVertices[i] * scale, angle) + position;

                return transformedVertices;
            }
        }


#if DEBUG
        private Sprite DebugSprite;
        private readonly Color DebugColor = Color.Red;
        private const float DebugOpacity = 0.5f;

        public override void Init()
        {
            base.Init();
            DebugSprite = AddChild(new StaticSprite(1, 1));
            DebugSprite.Centered = false;
        }

        private void UpdateDebugSprite()
        {
            Texture2D debugTexture = new Texture2D(Engine.GraphicsDevice, 1, 1);
            Color[] colorData = new Color[] { DebugColor };
            debugTexture.SetData(colorData);
            DebugSprite.Texture = debugTexture;

            var position = new Vector2(float.MaxValue, float.MaxValue);
            var scale = new Vector2(float.MinValue, float.MinValue);

            foreach (var v in Vertices)
            {
                position = new Vector2(Math.Min(position.X, v.X), Math.Min(position.Y, v.Y));
                scale = new Vector2(Math.Max(scale.X, v.X), Math.Max(scale.Y, v.Y));
            }
            DebugSprite.PositionOffset = position;
            DebugSprite.ScaleOffset = scale - position;
            Console.WriteLine(DebugSprite.PositionOffset);
            Console.WriteLine(DebugSprite.ScaleOffset);
        }

        public CollisionBody()
        {
            DebugColor = new Color(Color.Red, DebugOpacity);
        }
#endif
        public CollisionBody(Color color)
        {
#if DEBUG
            DebugColor = new Color(color, DebugOpacity);
#endif
        }

        public void SetPoints(params Vector2[] points)
        {
            Vertices = points;
#if DEBUG
            UpdateDebugSprite();
#endif
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
#if DEBUG
            UpdateDebugSprite();
#endif
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
#if DEBUG
            UpdateDebugSprite();
#endif
        }
        public void SetAsBox(float scale = 1.0f)
        {
            SetAsBox(new Vector2(scale, scale));
        }
    }
}
