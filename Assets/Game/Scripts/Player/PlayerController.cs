using UnityEngine;

namespace CozyGame
{
    /// <summary>
    /// Main player controller handling movement, rotation, and input.
    /// Integrates with InputManager for cross-platform support.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        [Header("Movement Settings")]
        [Tooltip("Walking speed in units per second")]
        public float walkSpeed = 3f;

        [Tooltip("Running speed in units per second")]
        public float runSpeed = 6f;

        [Tooltip("Rotation speed (degrees per second)")]
        public float rotationSpeed = 720f;

        [Tooltip("Acceleration time to reach max speed")]
        public float accelerationTime = 0.1f;

        [Tooltip("Deceleration time to stop")]
        public float decelerationTime = 0.1f;

        [Header("Jump & Gravity")]
        [Tooltip("Jump height in units")]
        public float jumpHeight = 2f;

        [Tooltip("Gravity multiplier")]
        public float gravity = -15f;

        [Tooltip("Can player jump?")]
        public bool enableJumping = true;

        [Header("Ground Detection")]
        [Tooltip("Ground check sphere radius")]
        public float groundCheckRadius = 0.3f;

        [Tooltip("Ground check offset below player")]
        public float groundCheckDistance = 0.1f;

        [Tooltip("What layers are considered ground")]
        public LayerMask groundMask = 1;

        [Header("Camera-Relative Movement")]
        [Tooltip("Move relative to camera direction")]
        public bool cameraRelativeMovement = true;

        [Tooltip("Main camera reference (auto-found if null)")]
        public Camera mainCamera;

        [Header("Input Settings")]
        [Tooltip("Key to hold for running")]
        public KeyCode runKey = KeyCode.LeftShift;

        [Tooltip("Key for jumping")]
        public KeyCode jumpKey = KeyCode.Space;

        [Tooltip("Enable run toggle (tap shift to toggle run)")]
        public bool runToggle = false;

        [Header("Mobile Settings")]
        [Tooltip("Virtual joystick for mobile")]
        public VirtualJoystick virtualJoystick;

        [Tooltip("Mobile run button")]
        public UnityEngine.UI.Button mobileRunButton;

        // Components
        private CharacterController characterController;
        private PlayerAnimationController animationController;

        // Movement state
        private Vector3 velocity;
        private Vector3 moveDirection;
        private float currentSpeed;
        private float targetSpeed;
        private bool isGrounded;
        private bool isRunning;
        private bool runToggleActive;

        // Input state
        private Vector2 inputVector;
        private bool jumpPressed;

        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("[PlayerController] Multiple PlayerController instances detected!");
                Destroy(this);
                return;
            }

            // Get components
            characterController = GetComponent<CharacterController>();
            animationController = GetComponent<PlayerAnimationController>();

            // Find camera if not assigned
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            // Find virtual joystick if not assigned
            if (virtualJoystick == null && InputManager.Instance != null && InputManager.Instance.IsMobile())
            {
                virtualJoystick = FindObjectOfType<VirtualJoystick>();
            }

            // Setup mobile run button
            if (mobileRunButton != null)
            {
                mobileRunButton.onClick.AddListener(ToggleRun);
            }
        }

        private void Update()
        {
            CheckGrounded();
            HandleInput();
            HandleMovement();
            HandleRotation();
            ApplyGravity();
            UpdateAnimations();
        }

        /// <summary>
        /// Check if player is on the ground
        /// </summary>
        private void CheckGrounded()
        {
            Vector3 spherePosition = transform.position - new Vector3(0, groundCheckDistance, 0);
            isGrounded = Physics.CheckSphere(spherePosition, groundCheckRadius, groundMask);

            // Reset vertical velocity when grounded
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small downward force to keep grounded
            }
        }

        /// <summary>
        /// Handle player input from keyboard, gamepad, or mobile
        /// </summary>
        private void HandleInput()
        {
            // Get movement input
            if (InputManager.Instance != null)
            {
                inputVector = InputManager.Instance.GetMovementInput();
            }
            else
            {
                // Fallback if InputManager doesn't exist
                inputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            }

            // Clamp input to unit circle (prevent diagonal speed boost)
            if (inputVector.magnitude > 1f)
            {
                inputVector.Normalize();
            }

            // Handle run input
            if (runToggle)
            {
                // Toggle mode
                if (Input.GetKeyDown(runKey))
                {
                    runToggleActive = !runToggleActive;
                }
                isRunning = runToggleActive && inputVector.magnitude > 0.1f;
            }
            else
            {
                // Hold mode
                isRunning = Input.GetKey(runKey) && inputVector.magnitude > 0.1f;
            }

            // Handle jump input
            jumpPressed = false;
            if (enableJumping && isGrounded)
            {
                if (Input.GetKeyDown(jumpKey))
                {
                    jumpPressed = true;
                }
            }
        }

        /// <summary>
        /// Handle player movement
        /// </summary>
        private void HandleMovement()
        {
            // Calculate move direction
            if (inputVector.magnitude > 0.1f)
            {
                // Get camera forward and right vectors
                Vector3 cameraForward = mainCamera.transform.forward;
                Vector3 cameraRight = mainCamera.transform.right;

                if (cameraRelativeMovement)
                {
                    // Flatten camera vectors to horizontal plane
                    cameraForward.y = 0f;
                    cameraRight.y = 0f;
                    cameraForward.Normalize();
                    cameraRight.Normalize();

                    // Calculate movement direction relative to camera
                    moveDirection = (cameraForward * inputVector.y + cameraRight * inputVector.x).normalized;
                }
                else
                {
                    // World-space movement
                    moveDirection = new Vector3(inputVector.x, 0f, inputVector.y).normalized;
                }

                // Set target speed
                targetSpeed = isRunning ? runSpeed : walkSpeed;
            }
            else
            {
                // No input - decelerate
                targetSpeed = 0f;
            }

            // Smooth speed changes
            float acceleration = targetSpeed > currentSpeed ? accelerationTime : decelerationTime;
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / Mathf.Max(acceleration, 0.01f));

            // Calculate movement velocity
            Vector3 horizontalVelocity = moveDirection * currentSpeed;

            // Handle jumping
            if (jumpPressed)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            // Combine horizontal and vertical velocity
            Vector3 finalVelocity = horizontalVelocity + new Vector3(0, velocity.y, 0);

            // Move the character
            characterController.Move(finalVelocity * Time.deltaTime);
        }

        /// <summary>
        /// Handle player rotation to face movement direction
        /// </summary>
        private void HandleRotation()
        {
            if (moveDirection.magnitude > 0.1f)
            {
                // Calculate target rotation
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

                // Smoothly rotate towards target
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        /// <summary>
        /// Apply gravity to player
        /// </summary>
        private void ApplyGravity()
        {
            if (!isGrounded)
            {
                velocity.y += gravity * Time.deltaTime;
            }
        }

        /// <summary>
        /// Update animation controller
        /// </summary>
        private void UpdateAnimations()
        {
            if (animationController != null)
            {
                // Calculate movement speed percentage (0-1 for walk, 1-2 for run)
                float speedPercent = currentSpeed / walkSpeed;

                animationController.SetMovementSpeed(speedPercent);
                animationController.SetGrounded(isGrounded);
                animationController.SetRunning(isRunning);

                if (jumpPressed)
                {
                    animationController.TriggerJump();
                }
            }
        }

        /// <summary>
        /// Toggle run on/off (for mobile button)
        /// </summary>
        public void ToggleRun()
        {
            runToggleActive = !runToggleActive;
        }

        /// <summary>
        /// Set player position
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            characterController.enabled = false;
            transform.position = position;
            characterController.enabled = true;
            velocity = Vector3.zero;
        }

        /// <summary>
        /// Teleport player to position with rotation
        /// </summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            characterController.enabled = false;
            transform.position = position;
            transform.rotation = rotation;
            characterController.enabled = true;
            velocity = Vector3.zero;
        }

        /// <summary>
        /// Check if player is currently moving
        /// </summary>
        public bool IsMoving()
        {
            return currentSpeed > 0.1f;
        }

        /// <summary>
        /// Check if player is running
        /// </summary>
        public bool IsRunning()
        {
            return isRunning;
        }

        /// <summary>
        /// Check if player is grounded
        /// </summary>
        public bool IsGrounded()
        {
            return isGrounded;
        }

        /// <summary>
        /// Get current movement speed
        /// </summary>
        public float GetCurrentSpeed()
        {
            return currentSpeed;
        }

        /// <summary>
        /// Get current move direction
        /// </summary>
        public Vector3 GetMoveDirection()
        {
            return moveDirection;
        }

        /// <summary>
        /// Enable/disable player control
        /// </summary>
        public void SetControlEnabled(bool enabled)
        {
            this.enabled = enabled;

            if (!enabled)
            {
                // Reset movement when disabled
                inputVector = Vector2.zero;
                currentSpeed = 0f;
                targetSpeed = 0f;
                velocity.y = 0f;
            }
        }

        /// <summary>
        /// Draw ground check gizmo
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector3 spherePosition = transform.position - new Vector3(0, groundCheckDistance, 0);
            Gizmos.DrawWireSphere(spherePosition, groundCheckRadius);
        }
    }
}
