using UnityEngine;
using UnityEditor;

namespace Platformer2DSystem.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Actor))]
    public class ActorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            DrawPlaceOnGroundButton();
            EditorGUI.EndDisabledGroup();
        }

        private void DrawPlaceOnGroundButton()
        {
            if (GUILayout.Button("Place on Ground"))
            {
                foreach (Actor actor in targets)
                {
                    Bounds bounds = actor.CollisionBox.bounds;
                    Vector2 direction = Vector2.down;
                    const float distance = 50f;
                    RaycastHit2D hit = Physics2D.BoxCast(bounds.center, bounds.size, 0f, direction, distance, actor.CollisionMask);

                    if (!hit)
                    {
                        Debug.LogWarning($"No ground detected within {distance} units", actor);
                        continue;
                    }

                    if (Mathf.Approximately(hit.distance, 0f))
                    {
                        Debug.Log($"Already standing on '{hit.collider}'", actor);
                        continue;
                    }

                    Undo.RecordObject(actor.transform, "Place on Ground");
                    actor.transform.position += (Vector3)(hit.distance * direction);
                }
            }
        }
    }
}
