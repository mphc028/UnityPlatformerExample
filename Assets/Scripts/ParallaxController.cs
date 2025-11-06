using UnityEngine;

[DisallowMultipleComponent]
public class ParallaxController : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Camera (or any Transform) that the parallax will follow. If null, will try Camera.main.")]
    public Transform cameraTransform;

    [Header("Behavior")]
    [Tooltip("Automaticly create layers from direct children of this GameObject (ignores this GameObject itself).")]
    public bool autoPopulateChildren = true;

    [Tooltip("Smoothing amount. 0 = instant, larger values = smoother (lagged) movement.")]
    [Range(0f, 20f)]
    public float smoothing = 0f;

    [Tooltip("If true, parallax will apply to X axis.")]
    public bool applyX = true;
    [Tooltip("If true, parallax will apply to Y axis.")]
    public bool applyY = true;

    [Header("Layers (manual entries override auto-populate)")]
    public ParallaxLayer[] layers = new ParallaxLayer[0];

    Vector3 prevCamPos;

    void Reset()
    {
        if (Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform == null)
            Debug.LogWarning("ParallaxController: No cameraTransform assigned and Camera.main is null.");

        prevCamPos = cameraTransform ? cameraTransform.position : Vector3.zero;

        if (autoPopulateChildren)
            AutoPopulateLayers();

        foreach (var l in layers)
            if (l != null && l.target != null)
                l.StoreInitialPosition();
    }

    void LateUpdate()
    {
        if (cameraTransform == null || layers == null || layers.Length == 0)
            return;

        Vector3 camPos = cameraTransform.position;
        Vector3 camDelta = camPos - prevCamPos;

        for (int i = 0; i < layers.Length; i++)
        {
            var l = layers[i];
            if (l == null || l.target == null) continue;

            Vector3 offset = Vector3.zero;
            if (applyX) offset.x = camDelta.x * l.parallaxMultiplier.x;
            if (applyY) offset.y = camDelta.y * l.parallaxMultiplier.y;

            Vector3 targetPos = l.target.position + offset;

            if (smoothing > 0f)
            {
                l.target.position = Vector3.Lerp(l.target.position, targetPos, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
            }
            else
            {
                l.target.position = targetPos;
            }
        }

        prevCamPos = camPos;
    }

    public void AutoPopulateLayers()
    {
        int count = transform.childCount;
        layers = new ParallaxLayer[count];
        for (int i = 0; i < count; i++)
        {
            var child = transform.GetChild(i);
            layers[i] = new ParallaxLayer() { target = child, parallaxMultiplier = DefaultMultiplierForIndex(i) };
            layers[i].StoreInitialPosition();
        }
    }


    Vector2 DefaultMultiplierForIndex(int index)
    {
        float baseVal = 1f - (index * 0.15f);
        baseVal = Mathf.Clamp(baseVal, -2f, 2f);
        return new Vector2(baseVal, baseVal);
    }

    [System.Serializable]
    public class ParallaxLayer
    {
        [Tooltip("Transform of the layer object to move")]
        public Transform target;

        [Tooltip("Multiplier applied to camera delta. Values near 0 = almost stationary (far away). Values >1 move faster than the camera (foreground).")]
        public Vector2 parallaxMultiplier = Vector2.one;

        [HideInInspector]
        public Vector3 initialPosition;

        public void StoreInitialPosition()
        {
            if (target != null)
                initialPosition = target.position;
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (smoothing < 0f) smoothing = 0f;
    }
#endif
}
