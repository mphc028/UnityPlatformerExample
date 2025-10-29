using UnityEngine;

namespace Platformer2DSystem
{
    public static class Maths
    {
        public static float Snap(float value, float target, float epsilon = 1E-5f)
        {
            if (Mathf.Abs(target - value) <= epsilon)
            {
                return target;
            }

            return value;
        }

        // From https://forum.unity.com/threads/range-mapping-function.917069/#post-6007313
        public static float Remap(float value, float valueMin, float valueMax, float targetMin, float targetMax)
        {
            float fraction = Mathf.InverseLerp(valueMin, valueMax, value);
            return Mathf.Lerp(targetMin, targetMax, fraction);
        }

        // From https://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp
        public static float Damp(float current, float target, float speed, float deltaTime)
        {
            return Mathf.Lerp(current, target, 1f - Mathf.Exp(-speed * deltaTime));
        }

        public static Vector2 Damp(Vector2 current, Vector2 target, float speed, float deltaTime)
        {
            Vector2 result = new();
            result.x = Damp(current.x, target.x, speed, deltaTime);
            result.y = Damp(current.y, target.y, speed, deltaTime);
            return result;
        }

        public static Vector3 Damp(Vector3 current, Vector3 target, float speed, float deltaTime)
        {
            Vector3 result = new();
            result.x = Damp(current.x, target.x, speed, deltaTime);
            result.y = Damp(current.y, target.y, speed, deltaTime);
            result.z = Damp(current.z, target.z, speed, deltaTime);
            return result;
        }

        public static float FramesToSeconds(int frames, int framerate = 60)
        {
            return frames / (float)framerate;
        }
    }
}
