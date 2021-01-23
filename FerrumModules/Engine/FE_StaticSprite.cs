using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FerrumModules.Engine
{
    public class FE_StaticSprite : FE_Sprite
    {
        public FE_StaticSprite(Texture2D texture, int tileWidth, int tileHeight, int frameIndex) : base(texture, tileWidth, tileHeight)
        {
            CurrentFrame = frameIndex;
        }
    }
}
