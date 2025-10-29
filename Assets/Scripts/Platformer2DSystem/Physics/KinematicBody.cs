using System.Collections.Generic;
using UnityEngine;

namespace Platformer2DSystem
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class KinematicBody : MonoBehaviour
    {
        public const float SkinWidth = 0.05f;

        [SerializeField] private BoxCollider2D collisionBox;

        private Rigidbody2D body;
        private List<RaycastHit2D> hits = new(8);

        public Vector2 Position { get; private set; }
        public Vector2 Scale => transform.localScale;
        public Bounds Bounds => new(Position + (collisionBox.offset * Scale), collisionBox.bounds.size);
        public BoxCollider2D CollisionBox => collisionBox;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            Position = body.position;
        }

        public RaycastHit2D Cast(Vector2 direction, float distance, LayerMask mask, IRaycastFilter filter)
        {
            CastAll(direction, distance, mask, hits);

            foreach (RaycastHit2D hit in hits)
            {
                if (Physics2D.GetIgnoreCollision(collisionBox, hit.collider) || filter.IsFiltering(hit, direction))
                {
                    continue;
                }

                return hit;
            }

            return new RaycastHit2D();
        }

        public List<RaycastHit2D> CastAll(Vector2 direction, float distance, LayerMask mask, IRaycastFilter filter)
        {
            CastAll(direction, distance, mask, hits);

            for (int i = hits.Count - 1; i >= 0; i--)
            {
                RaycastHit2D hit = hits[i];

                if (Physics2D.GetIgnoreCollision(collisionBox, hit.collider) || filter.IsFiltering(hit, direction))
                {
                    hits.RemoveAt(i);
                }
            }

            return hits;
        }

        private void CastAll(Vector2 direction, float distance, LayerMask mask, List<RaycastHit2D> results)
        {
            Bounds bounds = Bounds;
            bounds.Expand(-SkinWidth * 2f);
            distance += SkinWidth;
            Physics2D.BoxCast(bounds.center, bounds.size, 0f, direction, mask.ToContactFilter(), results, distance);
        }

        public void Translate(Vector2 distance)
        {
            Position += distance;
            body.MovePosition(Position);
        }

        public void Teleport(Vector2 position)
        {
            Position = position;
            body.position = position;
        }
    }
}
