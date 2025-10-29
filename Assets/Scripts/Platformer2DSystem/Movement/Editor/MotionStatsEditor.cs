using UnityEditor;

namespace Platformer2DSystem.Editor
{
    [CustomEditor(typeof(MotionStats))]
    public class MotionStatsEditor : UnityEditor.Editor
    {
        private bool foldout;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            foldout = EditorGUILayout.Foldout(foldout, "Info");

            if (foldout)
            {
                EditorGUI.BeginDisabledGroup(true);
                DrawProperties();
                EditorGUI.EndDisabledGroup();
            }
        }

        private void DrawProperties()
        {
            MotionStats motionStats = (MotionStats)target;
            EditorGUILayout.LabelField("Horizontal Motion", EditorStyles.boldLabel);
            EditorGUILayout.FloatField("Resting Time", motionStats.RestingTime);
            EditorGUILayout.FloatField("Moving Time", motionStats.MovingTime);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Vertical Motion", EditorStyles.boldLabel);
            EditorGUILayout.FloatField("Grounded Time", motionStats.GroundedTime);
            EditorGUILayout.FloatField("Airborne Time", motionStats.AirborneTime);
            EditorGUILayout.FloatField("Jumping Time", motionStats.JumpingTime);
            EditorGUILayout.FloatField("Falling Time", motionStats.FallingTime);
        }
    }
}
