using System;
using System.Collections.Generic;
using System.Text;

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
            RenderScale = Scale * Scene.Camera.Scale;
            RenderPosition = (Position - Scene.Camera.Position) / Scale * RenderScale;
        }
    }
}
