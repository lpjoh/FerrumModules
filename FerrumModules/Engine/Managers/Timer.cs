namespace Crossfrog.FerrumEngine
{
    public delegate void Timeout();

    public class Timer : Manager
    {
        public float TimeoutSeconds;
        public float TimeLeft;
        public bool Looping;

        public event Timeout Timeout;

        public Timer(float timeoutSeconds, bool looping = false, bool autoStart = false)
        {
            TimeoutSeconds = timeoutSeconds;
            Looping = looping;

            if (autoStart) Start();
            else if (looping) Paused = true;
        }

        public void Start()
        {
            TimeLeft = TimeoutSeconds;
            Paused = false;
        }

        public override void Update(float delta)
        {
            base.Update(delta);

            if (Looping)
            {
                if (TimeLeft <= 0)
                {
                    Timeout.Invoke();
                    Start();
                }
                else TimeLeft -= delta;
            }
            else
            {
                if (TimeLeft > 0)
                {
                    if (TimeLeft - delta <= 0)
                    {
                        TimeLeft = 0;
                        Timeout.Invoke();
                    }
                    else TimeLeft -= delta;
                }
            }
        }
    }
}
