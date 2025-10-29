using UnityEngine;

namespace Platformer2DSystem.Example
{
    public class SquashAndStretch : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private Actor actor;
        [SerializeField] private Jumper jumper;

        [Header("Airborne")]
        [SerializeField] private Vector2 airborneSquashScale = new(1.2f, 0.9f);
        [SerializeField] private Vector2 airborneStretchScale = new(0.8f, 1.5f);

        [Header("Landing")]
        [SerializeField] private Vector2 landingSquashScaleMin = new(1.5f, 0.6f);
        [SerializeField] private Vector2 landingSquashScaleMax = new(2f, 0.3f);
        [SerializeField] private float recoverySpeed = 15f;

        private Vector3 scaleDefault;
        private Vector3 scaleCurrent;
        private Vector3 scaleFallStart;

        protected void Awake()
        {
            scaleDefault = transform.localScale;
        }

        private void OnEnable()
        {
            scaleCurrent = transform.localScale;
            scaleFallStart = transform.localScale;
            jumper.landed.AddListener(OnLanded);
        }

        public void OnLanded()
        {
            ComputeScale(actor.velocity.y, 0f, jumper.maxFallVelocity, landingSquashScaleMin, landingSquashScaleMax);
        }

        private void Update()
        {
            if (actor.IsOnGround)
            {
                scaleCurrent = Maths.Damp(scaleCurrent, scaleDefault, recoverySpeed, Time.deltaTime);
                scaleFallStart = airborneSquashScale;
            }
            else
            {
                if (actor.velocity.y > jumper.MinJumpVelocity)
                {
                    ComputeScale(actor.velocity.y, jumper.MinJumpVelocity, jumper.MaxJumpVelocity, airborneSquashScale, scaleDefault);
                    scaleFallStart = scaleCurrent;
                }
                else
                {
                    ComputeScale(actor.velocity.y, jumper.MinJumpVelocity, jumper.maxFallVelocity, scaleFallStart, airborneStretchScale);
                }
            }

            transform.localScale = scaleCurrent;
        }

        private void ComputeScale(float velocity, float velocityMin, float velocityMax, Vector2 scaleMin, Vector2 scaleMax)
        {
            scaleCurrent.x = Maths.Remap(velocity, velocityMin, velocityMax, scaleMin.x, scaleMax.x);
            scaleCurrent.y = Maths.Remap(velocity, velocityMin, velocityMax, scaleMin.y, scaleMax.y);
        }

        private void OnDisable()
        {
            jumper.landed.RemoveListener(OnLanded);
        }
    }
}
