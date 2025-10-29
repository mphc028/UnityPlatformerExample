using UnityEngine;

namespace Platformer2DSystem
{
    [RequireComponent(typeof(Actor))]
    [AddComponentMenu("Platformer 2D System/Movement/Motion Stats")]
    public class MotionStats : MonoBehaviour
    {
        private Actor actor;

        public float RestingTime { get; private set; }
        public float MovingTime { get; private set; }
        public float GroundedTime { get; private set; }
        public float AirborneTime { get; private set; }
        public float JumpingTime { get; private set; }
        public float FallingTime { get; private set; }

        private void Awake()
        {
            actor = GetComponent<Actor>();
        }

        private void OnEnable()
        {
            Clear();
        }

        private void Update()
        {
            UpdateHorizontalMotionTime();
            UpdateVerticalMotionTime();
        }

        private void UpdateHorizontalMotionTime()
        {
            if (Mathf.Approximately(actor.velocity.x, 0f))
            {
                RestingTime += Time.deltaTime;
                MovingTime = 0f;
            }
            else
            {
                RestingTime = 0f;
                MovingTime += Time.deltaTime;
            }
        }

        private void UpdateVerticalMotionTime()
        {
            if (actor.IsOnGround)
            {
                GroundedTime += Time.deltaTime;
                AirborneTime = 0f;
                JumpingTime = 0f;
                FallingTime = 0f;
            }
            else
            {
                GroundedTime = 0f;
                AirborneTime += Time.deltaTime;

                if (Mathf.Approximately(actor.velocity.y, 0f))
                {
                    JumpingTime = 0f;
                    FallingTime = 0f;
                }
                else if (actor.velocity.y > 0f)
                {
                    JumpingTime += Time.deltaTime;
                    FallingTime = 0f;
                }
                else
                {
                    JumpingTime = 0f;
                    FallingTime += Time.deltaTime;
                }
            }
        }

        public void Clear()
        {
            RestingTime = 0f;
            MovingTime = 0f;
            GroundedTime = 0f;
            AirborneTime = 0f;
            JumpingTime = 0f;
            FallingTime = 0f;
        }
    }
}
