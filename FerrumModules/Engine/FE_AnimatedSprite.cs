using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace FerrumModules.Engine
{
    public class FE_Animation
    {
        public IList<int> frames;
        public int FPS;
        public int loopPoint;

        public FE_Animation(IList<int> frames, int FPS = 12, int loopPoint = 0)
        {
            this.frames = frames;
            this.FPS = FPS;
            this.loopPoint = loopPoint;
        }
    }

    public class FE_AnimatedSprite : FE_Sprite
    {
        private readonly FE_Animation CurrentAnimation;
        private float _currentFrameTime;

        public FE_AnimatedSprite(Texture2D texture, int tileWidth, int tileHeight, FE_Animation startingAnimation)
            : base(texture, tileWidth, tileHeight)
        {
            CurrentAnimation = startingAnimation;
        }

        public override void Update(float delta)
        {
            if (_currentFrameTime >= 1 / (float)CurrentAnimation.FPS)
            {
                if (CurrentFrame >= CurrentAnimation.frames.Count)
                    CurrentFrame = CurrentAnimation.frames[CurrentAnimation.loopPoint];
                else
                    CurrentFrame += 1;
                
                _currentFrameTime = 0;
            }
            _currentFrameTime += delta;
            base.Update(delta);
        }
    }
}
