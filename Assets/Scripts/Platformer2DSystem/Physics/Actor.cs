using System;
using UnityEngine;

namespace Platformer2DSystem
{
    [SelectionBase]
    [AddComponentMenu("Platformer 2D System/Physics/Actor")]
    public class Actor : KinematicBody, IRaycastFilter
    {
        public const float MaxSlopeAngle = 45f;
        private PlayerAnimationController anims;

        [SerializeField] private LayerMask collisionMask;
        [SerializeField] public Vector2 velocity;
        [SerializeField] public float gravity;

        public LayerMask CollisionMask => collisionMask;
        public Collider2D GroundCollider { get; private set; }
        public Vector2 GroundNormal { get; private set; }
        public bool IsOnGround { get; private set; }
        public bool IsOnGroundOneWay => IsOnGround && GroundCollider.CompareTag(Tags.OneWay);

        public event Action Updating;
        public event Action Updated;
        public event Action<Collider2D> GroundEntered;
        public event Action<Collider2D> GroundExited;
        public event Action<Collider2D> WallHit;
        public event Action<Collider2D> CeilingHit;

        private void Start()
        {
            anims = GetComponentInChildren<PlayerAnimationController>();
        }

        private void OnEnable()
        {
            // Initial ground check.
            if (Cast(Vector2.down, SkinWidth, collisionMask, this))
            {
                MoveY(-SkinWidth);
            }
        }

        private void Update()
        {
            Updating?.Invoke();

            // Store velocity for animation before movement
            Vector2 animationVelocity = velocity;

            MoveX(velocity.x * Time.deltaTime);
            MoveY((velocity.y * Time.deltaTime) + (0.5f * gravity * Time.deltaTime * Time.deltaTime));
            velocity.y = IsOnGround ? 0f : velocity.y + (gravity * Time.deltaTime);

            // Update animations with the proper velocity values
            UpdateAnimations(animationVelocity);

            Updated?.Invoke();
        }

        private void UpdateAnimations(Vector2 currentVelocity)
        {
            // Update horizontal animations
            if (!Mathf.Approximately(currentVelocity.x, 0f))
            {
                if (currentVelocity.x > 0f) anims.FacingRight = true;
                else if (currentVelocity.x < 0f) anims.FacingRight = false;
            }

            anims.HorizontalSpeed = Mathf.Abs(currentVelocity.x);
            anims.VerticalSpeed = currentVelocity.y;
            anims.IsGrounded = IsOnGround;

            // Trigger jump animation when moving upward and not on ground
            if (currentVelocity.y > 0f && !IsOnGround)
            {
                anims.TriggerJump();
            }
        }

        internal void MoveX(float amount, bool groundSnap = true)
        {
            if (Mathf.Approximately(amount, 0f))
            {
                return;
            }

            Vector2 axis = -Vector2.Perpendicular(IsOnGround ? GroundNormal : Vector2.up);
            Vector2 direction = Mathf.Sign(amount) * axis;
            float distance = Mathf.Abs(amount);
            RaycastHit2D hit = MoveAndSlide(direction, distance);

            if (groundSnap && IsOnGround)
            {
                SnapToGround(distance);
            }

            if (hit && !IsGround(hit.normal))
            {
                WallHit?.Invoke(hit.collider);
            }
        }

        internal void MoveY(float amount, bool groundCheck = true)
        {
            if (Mathf.Approximately(amount, 0f))
            {
                return;
            }

            Vector2 direction = Mathf.Sign(amount) * Vector2.up;
            float distance = Mathf.Abs(amount);
            RaycastHit2D hit = MoveAndCollide(direction, distance);

            if (groundCheck)
            {
                if (hit && amount < 0f)
                {
                    // Fallback to an upward ground normal if the hit's normal surpasses the max slope angle, this
                    // can happen if during a Boxcast one of the box's corners hits an intersection of two edges
                    // (the box's corner is fractionally overlapping both edges) resulting in inconsistent normals.
                    Vector2 normal = IsGround(hit.normal) ? hit.normal : Vector2.up;

                    EnterGroundedState(hit.collider, normal);
                }
                else if (IsOnGround)
                {
                    ExitGroundedState();
                }
            }

            if (hit && amount > 0f)
            {
                CeilingHit?.Invoke(hit.collider);
            }
        }

        private RaycastHit2D MoveAndCollide(Vector2 direction, float distance)
        {
            RaycastHit2D hit = Cast(direction, distance, collisionMask, this);

            if (hit)
            {
                Translate((hit.distance - SkinWidth) * direction);
            }
            else
            {
                Translate(distance * direction);
            }

            return hit;
        }

        private RaycastHit2D MoveAndSlide(Vector2 direction, float distance, int iterations = 3)
        {
            RaycastHit2D hit;

            do
            {
                hit = MoveAndCollide(direction, distance);

                if (hit && IsGround(hit.normal))
                {
                    // Calculate new move direction and remaining distance.
                    direction = Mathf.Sign(direction.x) * -Vector2.Perpendicular(hit.normal);
                    distance -= Mathf.Max(hit.distance - SkinWidth, 0f);
                    iterations--;
                }
                else
                {
                    break;
                }
            }
            while (iterations > 0 && distance > 0f);

            return hit;
        }

        private void SnapToGround(float walkDistance)
        {
            // Snap with a distance proportional to the walk distance.
            float slopeTangent = Mathf.Tan(MaxSlopeAngle * Mathf.Deg2Rad);
            float snapDistance = (walkDistance * slopeTangent) + SkinWidth;

            Vector2 direction = Vector2.down;
            RaycastHit2D hit = Cast(direction, snapDistance, collisionMask, this);

            if (hit)
            {
                Translate((hit.distance - SkinWidth) * direction);
            }
        }

        private void EnterGroundedState(Collider2D ground, Vector2 normal)
        {
            if (!IsOnGround)
            {
                GroundEntered?.Invoke(ground);
            }

            GroundCollider = ground;
            GroundNormal = normal;
            IsOnGround = true;
        }

        private void ExitGroundedState()
        {
            if (IsOnGround)
            {
                GroundExited?.Invoke(GroundCollider);
            }

            GroundCollider = null;
            GroundNormal = Vector2.zero;
            IsOnGround = false;
        }

        private bool IsGround(Vector2 normal)
        {
            float angle = Vector2.Angle(Vector2.up, normal);
            return angle < MaxSlopeAngle || Mathf.Approximately(angle, MaxSlopeAngle);
        }

        public bool IsRiding(Solid solid)
        {
            return IsOnGround && GroundCollider.gameObject == solid.gameObject;
        }

        public bool IsFiltering(RaycastHit2D hit, Vector2 direction)
        {
            if (hit.transform.CompareTag(Tags.OneWay))
            {
                // Ignore overlaps against one way solids.
                if (Mathf.Approximately(hit.distance, 0f))
                {
                    return true;
                }

                // Ignore lateral and upward collisions against one way solids.
                if (direction != Vector2.down && !IsGround(hit.normal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}