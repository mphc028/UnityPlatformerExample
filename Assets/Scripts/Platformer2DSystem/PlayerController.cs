using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Platformer2DSystem.Example
{
    [RequireComponent(typeof(Actor))]
    [RequireComponent(typeof(Runner))]
    [RequireComponent(typeof(Jumper))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Jump Settings")]
        [SerializeField] private int maxJumps = 2;
        [SerializeField] private int jumpBufferFrames = 5;
        [SerializeField] private float jumpMovementMultiplier = 0.8f;
        [SerializeField] private float doubleJumpMultiplier = 0.9f;


        private Actor actor;
        private Runner runner;
        private Jumper jumper;

        private int remainingJumps;
        private Timer jumpBufferTimer;

        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction downAction;

        private Vector2 moveInput;
        private bool jumpPressed;
        private bool jumpHeld;
        private bool downPressed;

        private void Awake()
        {
            actor = GetComponent<Actor>();
            runner = GetComponent<Runner>();
            jumper = GetComponent<Jumper>();

            jumpBufferTimer = Timer.Frames(jumpBufferFrames);
            remainingJumps = maxJumps;

            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s");

            moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            moveAction.canceled += ctx => moveInput = Vector2.zero;

            jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
            jumpAction.performed += ctx => jumpPressed = true;
            jumpAction.canceled += ctx => jumpHeld = false;

            downAction = new InputAction("Down", InputActionType.Button, "<Keyboard>/s");
            downAction.performed += ctx => downPressed = true;
            downAction.canceled += ctx => downPressed = false;
        }


        private void OnEnable()
        {
            moveAction.Enable();
            jumpAction.Enable();
            downAction.Enable();

            actor.GroundEntered += OnGroundEntered;
            actor.CeilingHit += OnCeilingHit;
        }

        private void OnDisable()
        {
            moveAction.Disable();
            jumpAction.Disable();
            downAction.Disable();

            actor.GroundEntered -= OnGroundEntered;
            actor.CeilingHit -= OnCeilingHit;
        }

        private void Update()
        {
            UpdateMovement();
            UpdateJumping();
        }

        // --- MOVEMENT ---
        private void UpdateMovement()
        {
            float dirX = moveInput.x;

            if (Mathf.Abs(dirX) < 0.01f)
            {
                runner.Stop();
                return;
            }

            float multiplier = jumper.IsJumping ? jumpMovementMultiplier : 1f;


            runner.Move(dirX, multiplier);
        }

        // --- JUMPING ---
        private void UpdateJumping()
        {
            if (jumpPressed)
            {
                jumpBufferTimer.Start();
                jumpPressed = false; // reset once processed
                jumpHeld = true;

            }

            if (jumpBufferTimer.IsRunning && remainingJumps > 0)
            {
                if (downPressed && actor.IsOnGroundOneWay)
                {
                    jumper.JumpDown();
                }
                else
                {
                    bool isDoubleJump = remainingJumps < maxJumps;
                    float multiplier = isDoubleJump ? doubleJumpMultiplier : 1f;
                    jumper.Jump(multiplier);
                    remainingJumps--;
                }

                jumpBufferTimer.Stop();
            }

            if (!jumpHeld && jumper.IsJumping)
            {
                jumper.CancelJump();
            }
        }

        public void OnGroundEntered(Collider2D ground)
        {
            remainingJumps = maxJumps;
        }

        public void OnCeilingHit(Collider2D ceiling)
        {
            jumper.CancelJump();
        }
    }
}
