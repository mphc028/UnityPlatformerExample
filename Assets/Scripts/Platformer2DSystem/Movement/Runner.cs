using UnityEngine;

namespace Platformer2DSystem
{
    [RequireComponent(typeof(Actor))]
    [AddComponentMenu("Platformer 2D System/Movement/Runner")]
    public class Runner : MonoBehaviour
    {
        [SerializeField] private float moveVelocity = 12f;

        [Header("Acceleration (Grounded)")]
        [SerializeField] private int groundedStartFrames;
        [SerializeField] private int groundedStopFrames;
        [SerializeField] private float groundedReactivity = 1f;

        [Header("Acceleration (Airborne)")]
        [SerializeField] private int airborneStartFrames;
        [SerializeField] private int airborneStopFrames;
        [SerializeField] private float airborneReactivity = 1f;

        [Header("Calculated Parameters")]
        [SerializeField, HideInInspector] private float groundedAcceleration;
        [SerializeField, HideInInspector] private float groundedDeceleration;
        [SerializeField, HideInInspector] private float airborneAcceleration;
        [SerializeField, HideInInspector] private float airborneDeceleration;

        private Actor actor;

        private void Awake()
        {
            actor = GetComponent<Actor>();
        }

        public void Move(float direction, float multiplier = 1f)
        {
            float velocity = Mathf.Sign(direction) * moveVelocity;
            float acceleration = actor.IsOnGround ? groundedAcceleration : airborneAcceleration;
            bool isChangingDirection = actor.velocity.x != 0f && Mathf.Sign(actor.velocity.x) != Mathf.Sign(direction);

            if (isChangingDirection)
            {
                float reactivity = actor.IsOnGround ? groundedReactivity : airborneReactivity;
                acceleration *= reactivity;
            }

            velocity *= multiplier;
            acceleration *= multiplier;
            actor.velocity.x = Mathf.MoveTowards(actor.velocity.x, velocity, acceleration * Time.deltaTime);
        }

        public void Stop()
        {
            float deceleration = actor.IsOnGround ? groundedDeceleration : airborneDeceleration;
            actor.velocity.x = Mathf.MoveTowards(actor.velocity.x, 0f, deceleration * Time.deltaTime);
        }

        private void OnValidate()
        {
            CalculateParameters();
        }

        private void CalculateParameters()
        {
            groundedAcceleration = moveVelocity / Maths.FramesToSeconds(groundedStartFrames);
            groundedDeceleration = moveVelocity / Maths.FramesToSeconds(groundedStopFrames);
            airborneAcceleration = moveVelocity / Maths.FramesToSeconds(airborneStartFrames);
            airborneDeceleration = moveVelocity / Maths.FramesToSeconds(airborneStopFrames);
        }
    }
}
