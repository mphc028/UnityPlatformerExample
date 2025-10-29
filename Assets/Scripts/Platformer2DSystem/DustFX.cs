using UnityEngine;

namespace Platformer2DSystem.Example
{
    public class DustFX : MonoBehaviour
    {
        [SerializeField] private Actor actor;
        [SerializeField] private ParticleSystem particles;
        [SerializeField] private float interval;

        private Timer timer;

        private void Awake()
        {
            timer = Timer.Seconds(interval);
        }

        private void Update()
        {
            if (actor.velocity.x == 0)
            {
                timer.Stop();
            }
            else
            {
                if (timer.IsCompleted && actor.IsOnGround)
                {
                    particles.Play();
                }

                if (!timer.IsRunning)
                {
                    timer.Start();
                }
            }
        }
    }
}
