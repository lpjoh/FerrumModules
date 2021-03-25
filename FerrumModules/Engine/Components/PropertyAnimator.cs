using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace Crossfrog.Ferrum.Engine.Components
{
    public enum Interpolation { Linear, Cosine, Constant, Preset }

    public class PropertyAnimator<T> : Component
    {
        private readonly Dictionary<string, Animation<T>> animationNameDict = new Dictionary<string, Animation<T>>();
        private Animation<T> _currentAnimation;
        public Animation<T> CurrentAnimation
        {
            get => _currentAnimation;
            set
            {
                TimePosition = 0.0f;
                _currentAnimation = value;
            }
        }

        public PropertyAnimator(params Animation<T>[] animations)
        {
            SetInterpolationAction();
            foreach (var a in animations) AddAnimation(a);
        }

        public float TimePosition = 0.0f;
        public override void Update(float delta)
        {
            base.Update(delta);
            if (CurrentAnimation != null)
            {
                var animationDuration = CurrentAnimation.TotalDuration;
                if (TimePosition >= animationDuration)
                {
                    while (TimePosition > animationDuration)
                        TimePosition -= animationDuration;

                    if (!CurrentAnimation.Looping)
                        Paused = true;
                }
                TimePosition += delta;
            }
        }

        public Animation<T> AddAnimation(Animation<T> animation)
        {
#if DEBUG
            if (animationNameDict.ContainsKey(animation.Name)) throw new Exception("An animation named \"" + animation.Name + "\" already existed in the player.");
#endif
            animationNameDict[animation.Name] = animation;
            return animation;
        }

        public void PlayAnimation(string name)
        {
#if DEBUG
            if (!animationNameDict.ContainsKey(name)) throw new Exception("Animation \"" + name + "\" does not exist in the player.");
#endif
            CurrentAnimation = animationNameDict[name];
            TimePosition = 0.0f;
            Paused = false;
        }

        private bool Stopping = false;
        public void Stop()
        {
            TimePosition = 0.0f;
            Paused = true;
            Stopping = true;
        }

        public void Set(ref T value)
        {
            if (Stopping)
            {
                value = ValueAtTime(0);
                CurrentAnimation = null;
                Stopping = false;
            }
            else if (CurrentAnimation != null) value = ValueAtTime(TimePosition);
        }

        private const Interpolation DefaultInterpolationMode = Interpolation.Linear;
        private Interpolation InterpolationMode = DefaultInterpolationMode;

        private static readonly HashSet<Type> InterpolatedTypes = new HashSet<Type>() { typeof(int), typeof(float), typeof(Vector2) };
        private Func<object, object, float, object> InterpolationAction = null;

        private void SetInterpolationAction()
        {
            var type = typeof(T);

            if (type == typeof(int))
                InterpolationAction = InterpolateInt;
            else if (type == typeof(float))
                InterpolationAction = InterpolateFloat;
            else if (type == typeof(Vector2))
                InterpolationAction = InterpolateVector2;
        }

        private static float InterpolateCosine(float start, float end, float factor)
        {
            var cosFactor = (1 - MathF.Cos(factor * MathF.PI)) / 2;
            return start * (1 - cosFactor) + end * cosFactor;
        }

        private float Interpolate(float start, float end, float factor)
        {
            return InterpolationMode switch
            {
                Interpolation.Linear => MathHelper.Lerp(start, end, factor),
                Interpolation.Cosine => InterpolateCosine(start, end, factor),
                Interpolation.Constant => start,
                Interpolation.Preset => start,
                _ => start,
            };
        }

        private object InterpolateInt(object start, object end, float factor)
        {
            return (int)Interpolate((int)start, (int)end, factor);
        }
        private object InterpolateFloat(object start, object end, float factor)
        {
            return Interpolate((float)start, (float)end, factor);
        }
        private object InterpolateVector2(object start, object end, float factor)
        {
            var startVec2 = (Vector2)start;
            var endVec2 = (Vector2)end;

            return new Vector2(Interpolate(startVec2.X, endVec2.X, factor), Interpolate(startVec2.Y, endVec2.Y, factor));
        }

        public T ValueAtTime(float time)
        {
            var keyframes = CurrentAnimation.Keyframes;

            var keyframesSize = keyframes.Length;
            var lastKeyIndex = keyframesSize - 1;

            if (CurrentAnimation.Looping)
            {
                time = Math.Abs(time);
                time %= CurrentAnimation.TotalDuration;
            }
            else if (time < 0) return keyframes[0].Value;

            var keyStartTime = 0.0f;
            var keyIndex = 0;

            foreach (var k in keyframes)
            {
                if (keyStartTime + k.Duration >= time) break;
                keyStartTime += k.Duration;
                keyIndex++;
            }

            var nextKeyIndex = keyIndex + 1;
            if (CurrentAnimation.Looping)
            {
                keyIndex %= keyframesSize;
                nextKeyIndex %= keyframesSize;
            }
            else if (keyIndex >= lastKeyIndex)
            {
                Paused = true;
                return keyframes[lastKeyIndex].Value;
            } 

            var keyframe = keyframes[keyIndex];

            if (InterpolatedTypes.Contains(typeof(T)))
            {
                var keyInterpolationMode = keyframe.InterpolationMode;
                if ((keyInterpolationMode == Interpolation.Preset) && (keyInterpolationMode != Interpolation.Constant))
                {
                    for (int i = keyIndex; i >= 0; i--)
                    {
                        var prevInterpolationMode = keyframes[i].InterpolationMode;
                        if (prevInterpolationMode != Interpolation.Preset)
                        {
                            InterpolationMode = prevInterpolationMode;
                            break;
                        }
                        InterpolationMode = DefaultInterpolationMode;
                    }
                }
                else InterpolationMode = keyframe.InterpolationMode;

                var interpolationFactor = (time - keyStartTime) / keyframe.Duration;
                var nextKeyframe = keyframes[nextKeyIndex];
                return (T)InterpolationAction.Invoke(keyframe.Value, nextKeyframe.Value, interpolationFactor);
            }

            return keyframes[keyIndex].Value;
        }
    }
}
