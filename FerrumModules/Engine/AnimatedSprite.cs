using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

namespace FerrumModules.Engine
{
    public class SpriteAnimation
    {
        public string Name { get; private set; }

        public List<int> Frames;
        public int FPS;
        public int LoopPoint;

        public SpriteAnimation(string name, List<int> frames, int fps = 12, int loopPoint = 0)
        {
            if (loopPoint >= frames.Count)
                throw new Exception("The loop point of sprite animation \"" + name + "\" exceeded its frame count.");

            Name = name;
            Frames = frames;
            FPS = fps;
            LoopPoint = loopPoint;
        }
    }

    public delegate void AnimationStarted(string animation);
    public delegate void AnimationEnded(string animation);

    public class AnimatedSprite : Sprite
    {
        public string CurrentAnimationName { get => _currentAnimation.Name; }

        public event AnimationStarted AnimationStarted;
        public event AnimationEnded AnimationEnded;

        private readonly Dictionary<string, SpriteAnimation> _animations = new Dictionary<string, SpriteAnimation>();
        private SpriteAnimation _currentAnimation;

        private int _fIndex;
        private int FrameIndex
        {
            get => _fIndex;
            set
            {
                _fIndex = value;
                CurrentFrame = _currentAnimation.Frames[_fIndex];
            }
        }

        private float _timeSinceFrameChange;
        private readonly Queue<string> _animationQueue = new Queue<string>();

        private void StartAnimation(params SpriteAnimation[] animations)
        {
            if (animations.Length > 0)
            {
                foreach (var a in animations) AddAnimation(a);
                PlayAnimation(animations[0].Name);
            }
        }

        public AnimatedSprite(params SpriteAnimation[] animations) : base()
        {
            StartAnimation(animations);
        }
        public AnimatedSprite(int tileWidth, int tileHeight, params SpriteAnimation[] animations)
            : base(tileWidth, tileHeight)
        {
            StartAnimation(animations);
        }
        public AnimatedSprite(Texture2D texture, int tileWidth, int tileHeight, params SpriteAnimation[] animations)
            : base(texture, tileWidth, tileHeight)
        {
            StartAnimation(animations);
        }

        public override void Update(float delta)
        {
            base.Update(delta);
            if (_currentAnimation != null)
            {
                _timeSinceFrameChange += delta;

                var fpsTime = 1f / _currentAnimation.FPS;
                while (_timeSinceFrameChange >= fpsTime)
                {
                    if (FrameIndex >= _currentAnimation.Frames.Count - 1)
                    {
                        if (_animationQueue.Count > 0)
                        {
                            PlayAnimation(_animationQueue.Dequeue(), false);
                            break;
                        }
                        FrameIndex = _currentAnimation.LoopPoint;
                    }
                    else FrameIndex++;

                    _timeSinceFrameChange -= fpsTime;
                }
            }
        }

        public SpriteAnimation AddAnimation(SpriteAnimation animation)
        {
            var name = animation.Name;
            if (_animations.ContainsKey(name)) throw new Exception("An animation with the name \"" + name + "\" already exists in the sprite.");
            _animations[name] = animation;

            if (_currentAnimation == null) PlayAnimation(name);

            return animation;
        }

        public void PlayAnimation(string name, bool interceptQueue = true)
        {
            if (!_animations.ContainsKey(name)) throw new Exception("Animation \"" + name + "\" did not exist, and could not be played in sprite \"" + Name + "\".");

            if (_currentAnimation != null) AnimationEnded?.Invoke(_currentAnimation.Name);
            
            var referredAnimation = _animations[name];
            if (_currentAnimation != referredAnimation)
            {
                _currentAnimation = referredAnimation;

                FrameIndex = 0;
                _timeSinceFrameChange = 0;
            }

            if (interceptQueue) _animationQueue.Clear();
            AnimationStarted?.Invoke(name);
        }

        public void QueueAnimation(string name)
        {
            if (!_animations.ContainsKey(name)) throw new Exception("Animation \"" + name + "\" did not exist, and could not be queued in the sprite.");

            _animationQueue.Enqueue(name);
        }
    }
}