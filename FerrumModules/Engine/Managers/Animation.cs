namespace Crossfrog.Ferrum.Engine.Managers
{
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
        public readonly Keyframe<T>[] Keyframes;
        public readonly float TotalDuration = 0.0f;
        public readonly bool Looping;
        public Animation(string name, bool looping, params Keyframe<T>[] keyframes)
        {
            Name = name;
            Looping = looping;
            Keyframes = keyframes;
            foreach (var k in Keyframes) TotalDuration += k.Duration;
        }
    }
}
