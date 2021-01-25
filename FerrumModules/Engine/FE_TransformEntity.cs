using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FerrumModules.Engine
{
    public abstract class FE_TransformEntity : FE_Entity
    {
        public Vector2 Position = new Vector2(0.0f, 0.0f);
        public Vector2 Scale = new Vector2(1.0f, 1.0f);
        public bool Centered = true;

        protected Vector2 RenderPosition { get; private set; }
        protected Vector2 RenderScale { get; private set; }

        public override void Render(SpriteBatch spriteBatch, SpriteEffects spriteBatchEffects)
        {
            base.Render(spriteBatch, spriteBatchEffects);
            var canvasTransform = CanvasTransformation(Position, Scale, Scene.Camera);
            RenderPosition = canvasTransform.Position;
            RenderScale = canvasTransform.Scale;
        }

        public struct FE_Transformation { public Vector2 Position, Scale; }

        public static FE_Transformation CanvasTransformation(Vector2 position, Vector2 scale, FE_Camera camera)
        {
            var transformedScale = scale * camera.Scale;
            var transformedPosition = (position - camera.Position) / scale * transformedScale;
            return new FE_Transformation { Position = transformedPosition, Scale = transformedScale };
        }
    }
}