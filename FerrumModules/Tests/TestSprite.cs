using Crossfrog.Ferrum.Engine.Entities;
using Crossfrog.Ferrum.Engine.Modules;

namespace Crossfrog.Ferrum.Tests
{
    public class TestSprite : AnimatedSprite
    {
        public TestSprite() : base(16, 16)
        {
            Texture = Assets.Textures["mario"];
            AddAnimation(new SpriteAnimation("default", 12, 0, new int[] { 3, 4, 5 }));
            CurrentFrame = 6;
        }

        public override void Init()
        {
            //Engine.FPS = 10;
        }
    }
}
