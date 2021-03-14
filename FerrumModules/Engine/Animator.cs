using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

namespace FerrumModules.Engine
{
    public enum Interpolation { Linear, Cosine, Constant, Preset }
    public class Keyframe<T>
    {
        public T Value;
        public float Duration;
        public Interpolation InterpolationMode;

        public Keyframe(T value, float duration = 1.0f, Interpolation interpolationMode = Interpolation.Preset)
        {
            Value = value;
            Duration = duration;
            InterpolationMode = interpolationMode;
        }
    }

    public class Animation<T>
    {
        public readonly string Name;
        public readonly List<Keyframe<T>> Keyframes = new List<Keyframe<T>>();
        public readonly float TotalDuration = 0.0f;
        public readonly bool Looping;
        public Animation(string name, bool looping, params Keyframe<T>[] keyframes)
        {
            Name = name;
            Looping = looping;
            foreach (var k in keyframes)
            {
                Keyframes.Add(k);
                TotalDuration += k.Duration;
            }
        }
    }

    public class Animator<T> : Manager
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

        public float TimePosition = 0.0f;
        public override void Update(float delta)
        {
            base.Update(delta);
            if (CurrentAnimation != null)
            {
                TimePosition += delta;

                var animationDuration = CurrentAnimation.TotalDuration;
                if (TimePosition >= animationDuration)
                {
                    while (TimePosition > animationDuration)
                        TimePosition -= animationDuration;

                    if (!CurrentAnimation.Looping)
                        Paused = true;
                }
            }
        }

        public Animation<T> AddAnimation(Animation<T> animation)
        {
            if (animationNameDict.ContainsKey(animation.Name)) throw new Exception("An animation named \"" + animation.Name + "\" already existed in the player.");
            animationNameDict[animation.Name] = animation;
            return animation;
        }

        public void PlayAnimation(string name)
        {
            if (!animationNameDict.ContainsKey(name)) throw new Exception("Animation \"" + name + "\" does not exist in the player.");
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

        public Animator()
        {
            SetInterpolationAction();
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

        private static float InterpolateLinear(float start, float end, float factor)
        {
            return (start * (1 - factor) + end * factor);
        }

        private static float InterpolateCosine(float start, float end, float factor)
        {
            var cosFactor = (1 - Rotation.Cos(factor * Rotation.PI)) / 2;
            return start * (1 - cosFactor) + end * cosFactor;
        }

        private float Interpolate(float start, float end, float factor)
        {
            switch (InterpolationMode)
            {
                case Interpolation.Linear:
                    return InterpolateLinear(start, end, factor);
                case Interpolation.Cosine:
                    return InterpolateCosine(start, end, factor);
                default:
                    return start;
            }
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

            var keyframesSize = keyframes.Count;
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
