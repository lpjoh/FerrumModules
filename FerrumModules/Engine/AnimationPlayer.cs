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
    public class AnimationTrack<T>
    {
        private static readonly HashSet<Type> InterpolatedTypes = new HashSet<Type>() { typeof(int), typeof(float), typeof(Vector2) };
        private Func<object, object, float, object> InterpolationAction = null;

        public AnimationTrack()
        {
            var type = typeof(T);

            if (type == typeof(int))
                InterpolationAction = InterpolateInt;
            else if (type == typeof(float))
                InterpolationAction = InterpolateFloat;
            else if (type == typeof(Vector2))
                InterpolationAction = InterpolateVector2;
            
        }

        public List<Keyframe<T>> Keyframes;

        private const Interpolation DefaultInterpolationMode = Interpolation.Linear;
        private Interpolation InterpolationMode = DefaultInterpolationMode;

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
            var keyStartTime = 0.0f;
            var keyIndex = 0;

            foreach (var k in Keyframes)
            {
                if (keyStartTime + k.Duration >= time) break;
                keyStartTime += k.Duration;
                keyIndex++;
            }

            var keyframe = Keyframes[keyIndex];
            

            if (InterpolatedTypes.Contains(typeof(T)))
            {
                var keyInterpolationMode = keyframe.InterpolationMode;
                if ((keyInterpolationMode == Interpolation.Preset) && (keyInterpolationMode != Interpolation.Constant))
                {
                    for (int i = keyIndex; i >= 0; i--)
                    {
                        var prevInterpolationMode = Keyframes[i].InterpolationMode;
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
                var nextKeyframe = Keyframes[keyIndex + 1];
                return (T)InterpolationAction.Invoke(keyframe.Value, nextKeyframe.Value, interpolationFactor);
            }

            return Keyframes[keyIndex].Value;
        }
    }
}
