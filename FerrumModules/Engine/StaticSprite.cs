using Microsoft.Xna.Framework.Graphics;

namespace FerrumModules.Engine
{
    public class StaticSprite : Sprite
    {
        public StaticSprite(Texture2D texture, int tileWidth, int tileHeight, int frameIndex = 0) : base(texture, tileWidth, tileHeight)
        {
            CurrentFrame = frameIndex;
        }
    }
}
