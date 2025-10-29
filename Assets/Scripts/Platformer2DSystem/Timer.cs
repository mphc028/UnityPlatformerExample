using UnityEngine;

namespace Platformer2DSystem.Example
{
    public sealed class Timer
    {
        public float Duration { get; }
        public float Elapsed => enabled ? Time.time - timeStarted : 0f;
        public bool IsRunning => enabled && Elapsed < Duration;
        public bool IsCompleted => enabled && Elapsed >= Duration;

        private float timeStarted;
        private bool enabled;

        private Timer(float duration)
        {
            Duration = duration;
        }

        public static Timer Seconds(float seconds)
        {
            return new Timer(seconds);
        }

        public static Timer Frames(int frames)
        {
            return new Timer(Maths.FramesToSeconds(frames));
        }

        public void Start()
        {
            timeStarted = Time.time;
            enabled = true;
        }

        public void Stop()
        {
            timeStarted = 0f;
            enabled = false;
        }
    }
}
