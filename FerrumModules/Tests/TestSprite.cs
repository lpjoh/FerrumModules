using System;
using System.Collections.Generic;
using System.Text;

using FerrumModules.Engine;

namespace FerrumModules.Tests
{
    public class TestSprite : AnimatedSprite
    {
        public TestSprite() : base(16, 16)
        {
            Texture = Assets.Textures["mario"];
            AddAnimation(new SpriteAnimation("default", new List<int>() { 3, 4, 5 }));
            CurrentFrame = 6;
        }

        public override void Init()
        {
            //Engine.FPS = 10;
        }
    }
}
