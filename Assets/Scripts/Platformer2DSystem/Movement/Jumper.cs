using UnityEngine;
using UnityEngine.Events;

namespace Platformer2DSystem
{
    [RequireComponent(typeof(Actor))]
    [AddComponentMenu("Platformer 2D System/Movement/Jumper")]
    public class Jumper : MonoBehaviour
    {
        [SerializeField] private int maxJumpFrames = 20;
        [SerializeField] private float maxJumpHeight = 7.5f;
        [SerializeField] private float minJumpHeight = 0.5f;

        [Header("Falling")]
        [SerializeField] public float fallMultiplier = 1.5f;
        [SerializeField] public float fallMultiplierThreshold = -15f;
        [SerializeField] public float maxFallVelocity = -64f;

        [Header("Calculated Parameters")]
        [SerializeField, HideInInspector] private float gravity;
        [SerializeField, HideInInspector] private float maxJumpVelocity;
        [SerializeField, HideInInspector] private float minJumpVelocity;

        [Header("Events")]
        [SerializeField] public UnityEvent jumpedGrounded;
        [SerializeField] public UnityEvent jumpedAirborne;
        [SerializeField] public UnityEvent jumpedDown;
        [SerializeField] public UnityEvent landed;

        private Actor actor;

        public bool IsJumping { get; private set; }
        public float MaxJumpVelocity => maxJumpVelocity;
        public float MinJumpVelocity => minJumpVelocity;

        private void Awake()
        {
            actor = GetComponent<Actor>();
        }

        private void OnEnable()
        {
            actor.Updating += OnUpdating;
            actor.Updated += OnUpdated;
            actor.GroundEntered += OnGroundEntered;
        }

        public void Jump(float multiplier = 1f)
        {
            actor.velocity.y = maxJumpVelocity * multiplier;
            IsJumping = true;

            if (actor.IsOnGround)
            {
                jumpedGrounded.Invoke();
            }
            else
            {
                jumpedAirborne.Invoke();
            }
        }

        public void CancelJump()
        {
            actor.velocity.y = Mathf.Min(actor.velocity.y, minJumpVelocity);
        }

        public void JumpDown()
        {
            if (!actor.IsOnGroundOneWay)
            {
                return;
            }

            Physics2D.IgnoreCollision(actor.CollisionBox, actor.GroundCollider);
            actor.velocity.y = 0f;
            actor.MoveY(-KinematicBody.SkinWidth * 2f, false);
            Physics2D.IgnoreCollision(actor.CollisionBox, actor.GroundCollider, false);
            jumpedDown.Invoke();
        }

        public void OnUpdating()
        {
            float multiplier = (actor.velocity.y < fallMultiplierThreshold) ? fallMultiplier : 1f;
            actor.gravity = gravity * multiplier;
        }

        public void OnUpdated()
        {
            actor.velocity.y = Mathf.Max(actor.velocity.y, maxFallVelocity);
        }

        public void OnGroundEntered(Collider2D ground)
        {
            IsJumping = false;

            if (actor.velocity.y != 0f)
            {
                landed.Invoke();
            }
        }

        private void OnDisable()
        {
            actor.Updating -= OnUpdating;
            actor.Updated -= OnUpdated;
            actor.GroundEntered -= OnGroundEntered;
        }

        private void OnValidate()
        {
            CalculateParameters();
        }

        private void CalculateParameters()
        {
            float maxJumpTime = Maths.FramesToSeconds(maxJumpFrames);
            gravity = -(2f * maxJumpHeight) / Mathf.Pow(maxJumpTime, 2f);
            maxJumpVelocity = Mathf.Abs(gravity) * maxJumpTime;
            minJumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(gravity) * minJumpHeight);
        }
    }
}
