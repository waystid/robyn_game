using UnityEngine;

namespace CozyGame
{
    /// <summary>
    /// Third-person camera controller with smooth following, rotation, and collision detection.
    /// Supports mouse, touch, and gamepad input for camera control.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("Target to follow (usually the player)")]
        public Transform target;

        [Tooltip("Offset from target position")]
        public Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);

        [Header("Camera Position")]
        [Tooltip("Distance from target")]
        public float distance = 5f;

        [Tooltip("Minimum distance (zoom in limit)")]
        public float minDistance = 2f;

        [Tooltip("Maximum distance (zoom out limit)")]
        public float maxDistance = 10f;

        [Tooltip("Height above target")]
        public float height = 2f;

        [Header("Camera Rotation")]
        [Tooltip("Initial horizontal angle (degrees)")]
        public float initialYaw = 0f;

        [Tooltip("Initial vertical angle (degrees)")]
        public float initialPitch = 20f;

        [Tooltip("Minimum pitch angle (looking down)")]
        public float minPitch = -20f;

        [Tooltip("Maximum pitch angle (looking up)")]
        public float maxPitch = 80f;

        [Header("Mouse/Touch Input")]
        [Tooltip("Mouse sensitivity for camera rotation")]
        public float mouseSensitivity = 3f;

        [Tooltip("Invert vertical mouse axis")]
        public bool invertY = false;

        [Tooltip("Key to hold for camera rotation (or use right mouse button)")]
        public KeyCode rotateKey = KeyCode.None;

        [Tooltip("Use right mouse button for rotation")]
        public bool useRightMouseButton = true;

        [Header("Mobile Input")]
        [Tooltip("Touch sensitivity for camera rotation")]
        public float touchSensitivity = 2f;

        [Tooltip("Two-finger pinch to zoom")]
        public bool enablePinchZoom = true;

        [Tooltip("Touch area for camera control (null = entire screen)")]
        public RectTransform touchArea;

        [Header("Smoothing")]
        [Tooltip("Position follow smoothing (0 = instant, higher = smoother)")]
        public float positionSmoothing = 5f;

        [Tooltip("Rotation smoothing")]
        public float rotationSmoothing = 10f;

        [Tooltip("Zoom smoothing")]
        public float zoomSmoothing = 5f;

        [Header("Collision")]
        [Tooltip("Enable camera collision detection")]
        public bool enableCollision = true;

        [Tooltip("Layers to check for collision")]
        public LayerMask collisionLayers = 1;

        [Tooltip("Camera collision radius")]
        public float collisionRadius = 0.3f;

        [Tooltip("Collision smoothing")]
        public float collisionSmoothing = 10f;

        [Header("Auto-Rotation")]
        [Tooltip("Auto-rotate camera behind player when moving")]
        public bool autoRotateBehindPlayer = false;

        [Tooltip("Delay before auto-rotation starts (seconds)")]
        public float autoRotateDelay = 2f;

        [Tooltip("Auto-rotation speed")]
        public float autoRotateSpeed = 2f;

        // Camera state
        private float currentYaw;
        private float currentPitch;
        private float currentDistance;
        private float targetDistance;
        private Vector3 currentPosition;
        private Quaternion currentRotation;

        // Input state
        private Vector2 rotationInput;
        private float zoomInput;
        private bool isRotating;
        private float timeSinceLastRotation;

        // Touch state
        private Vector2 lastTouchPosition;
        private float lastPinchDistance;

        private void Start()
        {
            // Initialize camera angles
            currentYaw = initialYaw;
            currentPitch = initialPitch;
            currentDistance = distance;
            targetDistance = distance;

            // Find target if not assigned
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
                else
                {
                    Debug.LogWarning("[CameraController] No target assigned and no Player found!");
                }
            }

            // Initial position
            if (target != null)
            {
                UpdateCameraPosition();
                transform.position = currentPosition;
                transform.rotation = currentRotation;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            HandleInput();
            HandleAutoRotation();
            UpdateCameraPosition();
            ApplySmoothing();
            HandleCollision();
        }

        /// <summary>
        /// Handle camera input from mouse, touch, or gamepad
        /// </summary>
        private void HandleInput()
        {
            rotationInput = Vector2.zero;
            zoomInput = 0f;
            isRotating = false;

            // Check if we should accept rotation input
            bool canRotate = rotateKey == KeyCode.None || Input.GetKey(rotateKey);
            if (useRightMouseButton)
            {
                canRotate = canRotate || Input.GetMouseButton(1);
            }

            // Mouse/PC input
            if (canRotate && !InputManager.Instance.IsMobile())
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
                {
                    rotationInput = new Vector2(mouseX, mouseY) * mouseSensitivity;
                    isRotating = true;
                }

                // Mouse wheel zoom
                zoomInput = -Input.GetAxis("Mouse ScrollWheel") * 2f;
            }

            // Mobile touch input
            if (InputManager.Instance != null && InputManager.Instance.IsMobile())
            {
                HandleTouchInput();
            }

            // Apply rotation input
            if (isRotating)
            {
                currentYaw += rotationInput.x;
                currentPitch -= invertY ? -rotationInput.y : rotationInput.y;
                currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

                timeSinceLastRotation = 0f;
            }
            else
            {
                timeSinceLastRotation += Time.deltaTime;
            }

            // Apply zoom input
            if (Mathf.Abs(zoomInput) > 0.01f)
            {
                targetDistance = Mathf.Clamp(targetDistance + zoomInput, minDistance, maxDistance);
            }
        }

        /// <summary>
        /// Handle touch input for mobile
        /// </summary>
        private void HandleTouchInput()
        {
            if (Input.touchCount == 1)
            {
                // Single touch - rotate camera
                Touch touch = Input.GetTouch(0);

                // Check if touch is in valid area
                if (touchArea != null)
                {
                    Vector2 localPoint;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        touchArea, touch.position, null, out localPoint
                    );

                    if (!touchArea.rect.Contains(localPoint))
                        return;
                }

                if (touch.phase == TouchPhase.Moved)
                {
                    Vector2 delta = touch.deltaPosition;
                    rotationInput = delta * touchSensitivity * 0.1f;
                    isRotating = true;
                }
            }
            else if (enablePinchZoom && Input.touchCount == 2)
            {
                // Two touches - pinch to zoom
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                Vector2 touch0Prev = touch0.position - touch0.deltaPosition;
                Vector2 touch1Prev = touch1.position - touch1.deltaPosition;

                float prevMagnitude = (touch0Prev - touch1Prev).magnitude;
                float currentMagnitude = (touch0.position - touch1.position).magnitude;

                float difference = currentMagnitude - prevMagnitude;
                zoomInput = -difference * 0.01f;
            }
        }

        /// <summary>
        /// Auto-rotate camera behind player when moving
        /// </summary>
        private void HandleAutoRotation()
        {
            if (!autoRotateBehindPlayer || isRotating)
                return;

            if (timeSinceLastRotation < autoRotateDelay)
                return;

            // Check if player is moving
            if (PlayerController.Instance != null && PlayerController.Instance.IsMoving())
            {
                Vector3 playerForward = target.forward;
                float targetYaw = Mathf.Atan2(playerForward.x, playerForward.z) * Mathf.Rad2Deg;

                // Smoothly rotate towards player facing
                currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, autoRotateSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Calculate desired camera position and rotation
        /// </summary>
        private void UpdateCameraPosition()
        {
            // Smoothly zoom
            currentDistance = Mathf.Lerp(currentDistance, targetDistance, zoomSmoothing * Time.deltaTime);

            // Calculate target position
            Vector3 targetPosition = target.position + targetOffset;

            // Calculate camera rotation
            Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

            // Calculate camera position
            Vector3 offset = rotation * new Vector3(0f, height, -currentDistance);
            Vector3 desiredPosition = targetPosition + offset;

            currentPosition = desiredPosition;
            currentRotation = rotation;
        }

        /// <summary>
        /// Apply smoothing to camera movement
        /// </summary>
        private void ApplySmoothing()
        {
            // Smooth position
            if (positionSmoothing > 0f)
            {
                transform.position = Vector3.Lerp(
                    transform.position,
                    currentPosition,
                    positionSmoothing * Time.deltaTime
                );
            }
            else
            {
                transform.position = currentPosition;
            }

            // Smooth rotation
            if (rotationSmoothing > 0f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    currentRotation,
                    rotationSmoothing * Time.deltaTime
                );
            }
            else
            {
                transform.rotation = currentRotation;
            }
        }

        /// <summary>
        /// Handle camera collision with environment
        /// </summary>
        private void HandleCollision()
        {
            if (!enableCollision || target == null)
                return;

            Vector3 targetPosition = target.position + targetOffset;
            Vector3 direction = transform.position - targetPosition;
            float desiredDistance = direction.magnitude;

            // Raycast from target to camera
            if (Physics.SphereCast(
                targetPosition,
                collisionRadius,
                direction.normalized,
                out RaycastHit hit,
                desiredDistance,
                collisionLayers
            ))
            {
                // Collision detected - move camera closer
                float collisionDistance = hit.distance - collisionRadius;
                Vector3 newPosition = targetPosition + direction.normalized * collisionDistance;

                // Smoothly move to collision position
                transform.position = Vector3.Lerp(
                    transform.position,
                    newPosition,
                    collisionSmoothing * Time.deltaTime
                );
            }
        }

        /// <summary>
        /// Set camera target
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>
        /// Set camera distance
        /// </summary>
        public void SetDistance(float newDistance)
        {
            targetDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);
        }

        /// <summary>
        /// Set camera angles
        /// </summary>
        public void SetAngles(float yaw, float pitch)
        {
            currentYaw = yaw;
            currentPitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        /// <summary>
        /// Reset camera to initial position
        /// </summary>
        public void ResetCamera()
        {
            currentYaw = initialYaw;
            currentPitch = initialPitch;
            targetDistance = distance;
            currentDistance = distance;
        }

        /// <summary>
        /// Snap camera behind player
        /// </summary>
        public void SnapBehindPlayer()
        {
            if (target != null)
            {
                Vector3 playerForward = target.forward;
                currentYaw = Mathf.Atan2(playerForward.x, playerForward.z) * Mathf.Rad2Deg;
            }
        }

        /// <summary>
        /// Get camera forward direction (flattened to horizontal plane)
        /// </summary>
        public Vector3 GetForwardDirection()
        {
            Vector3 forward = transform.forward;
            forward.y = 0f;
            return forward.normalized;
        }

        /// <summary>
        /// Get camera right direction (flattened to horizontal plane)
        /// </summary>
        public Vector3 GetRightDirection()
        {
            Vector3 right = transform.right;
            right.y = 0f;
            return right.normalized;
        }
    }
}
