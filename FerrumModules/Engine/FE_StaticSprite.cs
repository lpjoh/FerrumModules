using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FerrumModules.Engine
{
    public class FE_StaticSprite : FE_Sprite
    {
        public FE_StaticSprite(Texture2D texture, int tileSizeX, int tileSizeY, int frameIndex) : base(texture, tileSizeX, tileSizeY)
        {
            CurrentFrame = frameIndex;
        }
    }
}
