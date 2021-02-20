using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace FerrumModules.Engine
{
    public class Animation
    {
        public IList<int> frames;
        public int FPS;
        public int loopPoint;

        public Animation(IList<int> frames, int FPS = 12, int loopPoint = 0)
        {
            this.frames = frames;
            this.FPS = FPS;
            this.loopPoint = loopPoint;
        }
    }

    public class AnimatedSprite : Sprite
    {
        private readonly Animation CurrentAnimation;
        private float _currentFrameTime;

        public AnimatedSprite(Texture2D texture, int tileWidth, int tileHeight, Animation startingAnimation)
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
                    CurrentFrame++;
                
                _currentFrameTime = 0;
            }
            _currentFrameTime += delta;
            base.Update(delta);
        }
    }
}
