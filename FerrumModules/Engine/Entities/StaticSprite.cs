namespace Crossfrog.Ferrum.Engine.Entities
{
    public class StaticSprite : Sprite
    {
        public StaticSprite() { }
        public StaticSprite(int tileWidth, int tileHeight) : base(tileWidth, tileHeight) { }
        public StaticSprite(string textureName, int tileWidth, int tileHeight, int frameIndex = 0) : base(textureName, tileWidth, tileHeight)
        {
            CurrentFrame = frameIndex;
        }
    }
}
