using UnityEngine;

/// <summary>
/// Handles sprite animation, animator enabling/disabling, and jump/fall sprite logic.
/// Intended to be driven externally by the PlayerController (movement script).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Animator component (should be on a direct child).")]
    [SerializeField] private Animator childAnimator;
    [Tooltip("Main sprite renderer (on same GameObject).")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animator Settings")]
    [Tooltip("Animator parameter controlling horizontal speed (float).")]
    [SerializeField] private string animatorSpeedParam = "speed";

    [Header("Air Sprite Settings")]
    [Tooltip("Sprites for upward movement (jump). Index 0 = slowest rise, last = fastest rise.")]
    [SerializeField] private Sprite[] jumpSprites;
    [Tooltip("Sprites for downward movement (fall). Index 0 = slowest fall, last = fastest fall.")]
    [SerializeField] private Sprite[] fallSprites;
    [Tooltip("Maximum positive vertical velocity mapped to last jump sprite.")]
    [SerializeField] private float maxRiseSpeed = 12f;
    [Tooltip("Maximum negative vertical velocity mapped to last fall sprite.")]
    [SerializeField] private float maxFallSpeed = -20f;

    // Internal runtime data
    private bool isGrounded;
    private bool wasGrounded;
    private float horizontalSpeed;
    private float verticalSpeed;
    private bool facingRight = true;

    // NEW: Track jump state separately
    private bool isJumping;
    private float jumpStartTime;
    private const float JUMP_BUFFER_TIME = 0.2f;

    // ------------------------------
    //  PUBLIC SETTERS / GETTERS
    // ------------------------------

    /// <summary>Sets current horizontal speed (absolute value used for animation).</summary>
    public float HorizontalSpeed
    {
        get => horizontalSpeed;
        set => horizontalSpeed = value;
    }

    /// <summary>Sets current vertical speed for sprite selection.</summary>
    public float VerticalSpeed
    {
        get => verticalSpeed;
        set => verticalSpeed = value;
    }

    /// <summary>Sets whether the character is grounded.</summary>
    public bool IsGrounded
    {
        get => isGrounded;
        set => isGrounded = value;
    }

    /// <summary>Sets facing direction (true = right, false = left).</summary>
    public bool FacingRight
    {
        get => facingRight;
        set => facingRight = value;
    }

    // NEW: Public method to trigger jump animation
    public void TriggerJump()
    {
        isJumping = true;
        jumpStartTime = Time.time;

        // Immediately disable animator when jump starts
        if (childAnimator != null)
            childAnimator.enabled = false;
    }

    // ------------------------------
    //  UNITY METHODS
    // ------------------------------

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (childAnimator == null)
        {
            // Try to find animator in direct children automatically
            for (int i = 0; i < transform.childCount; i++)
            {
                Animator found = transform.GetChild(i).GetComponent<Animator>();
                if (found != null)
                {
                    childAnimator = found;
                    break;
                }
            }

            if (childAnimator == null)
                Debug.LogWarning($"[{nameof(PlayerAnimationController)}] No Animator found in direct children.");
        }
    }

    private void Update()
    {
        HandleAnimatorEnableDisable();
        UpdateAnimatorSpeed();
        UpdateSpriteDirection();
        UpdateAirSprite();

        // NEW: Handle jump timeout
        if (isJumping && Time.time - jumpStartTime > JUMP_BUFFER_TIME)
        {
            isJumping = false;
        }

        wasGrounded = isGrounded;
    }

    // ------------------------------
    //  ANIMATION / SPRITE LOGIC
    // ------------------------------

    /// <summary>
    /// Enables/disables the Animator depending on grounded state.
    /// When in air, we disable it to take manual control of jump/fall sprites.
    /// </summary>
    private void HandleAnimatorEnableDisable()
    {
        if (childAnimator == null) return;

        if (isGrounded && !wasGrounded)
        {
            // Just landed re-enable animator
            childAnimator.enabled = true;
            isJumping = false; // NEW: Reset jump state when landing
        }
        else if (!isGrounded && wasGrounded && !isJumping)
        {
            // Just left ground disable animator (only if not from a jump)
            childAnimator.enabled = false;
        }
    }

    /// <summary>
    /// Updates the Animator's "speed" parameter to match horizontal movement speed.
    /// </summary>
    private void UpdateAnimatorSpeed()
    {
        if (childAnimator == null || !childAnimator.enabled) return;
        childAnimator.SetFloat(animatorSpeedParam, Mathf.Abs(horizontalSpeed));
    }

    /// <summary>
    /// Flips the sprite horizontally based on facing direction.
    /// </summary>
    private void UpdateSpriteDirection()
    {
        if (spriteRenderer == null) return;
        spriteRenderer.flipX = !facingRight;
    }

    /// <summary>
    /// When in air (animator disabled), manually selects a sprite based on vertical velocity.
    /// </summary>
    private void UpdateAirSprite()
    {
        if (spriteRenderer == null || isGrounded) return;

        // NEW: Force jump sprite for the first part of the jump
        if (isJumping && verticalSpeed > 0)
        {
            ApplySpriteFromArray(jumpSprites, verticalSpeed, 0, maxRiseSpeed);
        }
        else if (verticalSpeed > 0)
        {
            ApplySpriteFromArray(jumpSprites, verticalSpeed, 0, maxRiseSpeed);
        }
        else
        {
            ApplySpriteFromArray(fallSprites, -verticalSpeed, 0, -maxFallSpeed);
        }
    }

    private void ApplySpriteFromArray(Sprite[] sprites, float value, float minVal, float maxVal)
    {
        if (sprites == null || sprites.Length == 0) return;

        float clamped = Mathf.Clamp(Mathf.RoundToInt(value), minVal, maxVal);
        float norm = (maxVal - minVal) <= Mathf.Epsilon ? 0f : (clamped - minVal) / (maxVal - minVal);
        int index = Mathf.RoundToInt(norm * (sprites.Length - 1));
        index = Mathf.Clamp(index, 0, sprites.Length - 1);

        spriteRenderer.sprite = sprites[index];
    }

    // ------------------------------
    //  DEBUG
    // ------------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * 0.1f, 0.05f);
    }
}