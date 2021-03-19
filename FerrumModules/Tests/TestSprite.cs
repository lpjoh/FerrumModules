using Crossfrog.FerrumEngine.Entities;
using Crossfrog.FerrumEngine.Modules;

namespace FerrumXF.Tests
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
