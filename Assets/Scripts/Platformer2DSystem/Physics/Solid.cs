using UnityEngine;

namespace Platformer2DSystem
{
    [SelectionBase]
    [AddComponentMenu("Platformer 2D System/Physics/Solid")]
    public class Solid : KinematicBody, IRaycastFilter
    {
        [SerializeField] private LayerMask passengersMask;
        [SerializeField] public Vector2 velocity;

        public bool IsOneWay => CompareTag(Tags.OneWay);

        private void Update()
        {
            Move(velocity * Time.deltaTime);
        }

        private void Move(Vector2 amount)
        {
            amount.x = Maths.Snap(amount.x, 0f);
            amount.y = Maths.Snap(amount.y, 0f);

            if (!IsOneWay)
            {
                if (amount.x != 0f)
                {
                    Vector2 direction = Mathf.Sign(amount.x) * Vector2.right;
                    PushActors(direction, Mathf.Abs(amount.x));
                }

                if (amount.y < 0f)
                {
                    PushActors(Vector2.down, Mathf.Abs(amount.y));
                }
            }

            if (amount.x != 0f || amount.y != 0f)
            {
                CarryActors(amount);
                Teleport(Position + amount);
            }
        }

        private void PushActors(Vector2 direction, float distance)
        {
            foreach (RaycastHit2D hit in CastAll(direction, distance, passengersMask, this))
            {
                Actor actor = hit.transform.GetComponent<Actor>();
                Vector2 amount = GetPushAmount(direction, distance, hit);
                actor.MoveX(amount.x);
                actor.MoveY(amount.y);
            }
        }

        private void CarryActors(Vector2 amount)
        {
            float distance = (amount.y <= 0f) ? SkinWidth : amount.y;

            foreach (RaycastHit2D hit in CastAll(Vector2.up, distance, passengersMask, this))
            {
                Actor actor = hit.transform.GetComponent<Actor>();

                if (actor.IsRiding(this))
                {
                    CarryActor(actor, amount);
                }
                else if (amount.y > 0f)
                {
                    Vector2 pushAmount = GetPushAmount(Vector2.up, distance, hit);
                    CarryActor(actor, pushAmount);
                }
            }
        }

        private void CarryActor(Actor actor, Vector2 amount)
        {
            Physics2D.IgnoreCollision(CollisionBox, actor.CollisionBox);
            actor.MoveX(amount.x, false);
            actor.MoveY(amount.y, false);
            Physics2D.IgnoreCollision(CollisionBox, actor.CollisionBox, false);
        }

        private Vector2 GetPushAmount(Vector2 direction, float distance, RaycastHit2D hit)
        {
            float pushDistance = distance - (hit.distance - SkinWidth);
            return pushDistance * direction;
        }

        public bool IsFiltering(RaycastHit2D hit, Vector2 direction)
        {
            // Ignore actors overlapping against one way solids.
            return Mathf.Approximately(hit.distance, 0f) && IsOneWay;
        }
    }
}
