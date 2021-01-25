using System;

using Microsoft.Xna.Framework.Graphics;

namespace FerrumModules.Engine
{
    public class FE_ActiveElement
    {
        public virtual void Init() { }
        public virtual void Update(float delta) { }
        public virtual void Render(SpriteBatch spriteBatch, SpriteEffects spriteBatchEffects) { }
    }
}
