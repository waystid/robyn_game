using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace CozyGame.Mobile
{
    /// <summary>
    /// Touch type
    /// </summary>
    public enum TouchType
    {
        Tap,
        DoubleTap,
        Hold,
        Swipe,
        Pinch,
        Drag,
        TwoFingerDrag
    }

    /// <summary>
    /// Swipe direction
    /// </summary>
    public enum SwipeDirection
    {
        None,
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight
    }

    /// <summary>
    /// Touch event data
    /// </summary>
    [System.Serializable]
    public class TouchEventData
    {
        public TouchType touchType;
        public Vector2 position;
        public Vector2 deltaPosition;
        public SwipeDirection swipeDirection;
        public float swipeDistance;
        public float pinchDelta;
        public float holdDuration;
    }

    /// <summary>
    /// Touch events
    /// </summary>
    [System.Serializable]
    public class TouchEvents
    {
        public UnityEvent<TouchEventData> onTap = new UnityEvent<TouchEventData>();
        public UnityEvent<TouchEventData> onDoubleTap = new UnityEvent<TouchEventData>();
        public UnityEvent<TouchEventData> onHold = new UnityEvent<TouchEventData>();
        public UnityEvent<TouchEventData> onSwipe = new UnityEvent<TouchEventData>();
        public UnityEvent<TouchEventData> onPinch = new UnityEvent<TouchEventData>();
        public UnityEvent<TouchEventData> onDrag = new UnityEvent<TouchEventData>();
    }

    /// <summary>
    /// Touch controls manager singleton.
    /// Handles touch input and gesture detection for mobile devices.
    /// </summary>
    public class TouchControlsManager : MonoBehaviour
    {
        public static TouchControlsManager Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("Enable touch controls")]
        public bool touchEnabled = true;

        [Tooltip("Use Unity Input System (new) vs Legacy Input")]
        public bool useNewInputSystem = false;

        [Header("Tap Settings")]
        [Tooltip("Max tap duration")]
        public float maxTapDuration = 0.3f;

        [Tooltip("Max tap movement")]
        public float maxTapMovement = 20f;

        [Tooltip("Double tap time window")]
        public float doubleTapWindow = 0.3f;

        [Header("Hold Settings")]
        [Tooltip("Min hold duration")]
        public float minHoldDuration = 0.5f;

        [Header("Swipe Settings")]
        [Tooltip("Min swipe distance")]
        public float minSwipeDistance = 50f;

        [Tooltip("Max swipe duration")]
        public float maxSwipeDuration = 1f;

        [Header("Pinch Settings")]
        [Tooltip("Pinch sensitivity")]
        public float pinchSensitivity = 0.01f;

        [Header("Drag Settings")]
        [Tooltip("Min drag distance")]
        public float minDragDistance = 10f;

        [Header("Events")]
        public TouchEvents touchEvents = new TouchEvents();

        // State
        private Vector2 touchStartPos;
        private Vector2 touchCurrentPos;
        private float touchStartTime;
        private bool isTouching = false;
        private bool isHolding = false;
        private bool isDragging = false;
        private float lastTapTime = 0f;
        private int tapCount = 0;

        // Pinch
        private float lastPinchDistance = 0f;
        private bool isPinching = false;

        // Multi-touch
        private Dictionary<int, Vector2> activeTouches = new Dictionary<int, Vector2>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (!touchEnabled)
                return;

            if (useNewInputSystem)
            {
                // TODO: Implement new Input System support
                Debug.LogWarning("[TouchControls] New Input System not yet implemented, falling back to legacy");
                ProcessLegacyInput();
            }
            else
            {
                ProcessLegacyInput();
            }
        }

        /// <summary>
        /// Process legacy input system
        /// </summary>
        private void ProcessLegacyInput()
        {
            // Handle touch input
            if (Input.touchCount > 0)
            {
                ProcessTouchInput();
            }
            // Fallback to mouse for testing in editor
            else if (Application.isEditor)
            {
                ProcessMouseInput();
            }
            // Handle touch end
            else if (isTouching)
            {
                HandleTouchEnd();
            }
        }

        /// <summary>
        /// Process touch input
        /// </summary>
        private void ProcessTouchInput()
        {
            // Single touch
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        HandleTouchStart(touch.position);
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        HandleTouchMove(touch.position);
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        HandleTouchEnd();
                        break;
                }
            }
            // Two finger gestures (pinch, two-finger drag)
            else if (Input.touchCount == 2)
            {
                Touch touch1 = Input.GetTouch(0);
                Touch touch2 = Input.GetTouch(1);

                HandleTwoFingerGesture(touch1, touch2);
            }
        }

        /// <summary>
        /// Process mouse input (for editor testing)
        /// </summary>
        private void ProcessMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandleTouchStart(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                HandleTouchMove(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                HandleTouchEnd();
            }
        }

        /// <summary>
        /// Handle touch start
        /// </summary>
        private void HandleTouchStart(Vector2 position)
        {
            touchStartPos = position;
            touchCurrentPos = position;
            touchStartTime = Time.time;
            isTouching = true;
            isHolding = false;
            isDragging = false;
        }

        /// <summary>
        /// Handle touch move
        /// </summary>
        private void HandleTouchMove(Vector2 position)
        {
            touchCurrentPos = position;

            float distance = Vector2.Distance(touchStartPos, touchCurrentPos);
            float duration = Time.time - touchStartTime;

            // Check for hold
            if (!isHolding && duration >= minHoldDuration && distance < maxTapMovement)
            {
                isHolding = true;
                TriggerHold();
            }

            // Check for drag
            if (!isDragging && distance >= minDragDistance)
            {
                isDragging = true;
            }

            if (isDragging)
            {
                TriggerDrag();
            }
        }

        /// <summary>
        /// Handle touch end
        /// </summary>
        private void HandleTouchEnd()
        {
            if (!isTouching)
                return;

            float distance = Vector2.Distance(touchStartPos, touchCurrentPos);
            float duration = Time.time - touchStartTime;

            // Check for tap
            if (distance < maxTapMovement && duration < maxTapDuration)
            {
                // Check for double tap
                if (Time.time - lastTapTime < doubleTapWindow)
                {
                    tapCount++;
                    if (tapCount >= 2)
                    {
                        TriggerDoubleTap();
                        tapCount = 0;
                        lastTapTime = 0f;
                    }
                }
                else
                {
                    tapCount = 1;
                    lastTapTime = Time.time;
                    TriggerTap();
                }
            }
            // Check for swipe
            else if (distance >= minSwipeDistance && duration < maxSwipeDuration)
            {
                TriggerSwipe();
            }

            isTouching = false;
            isHolding = false;
            isDragging = false;
        }

        /// <summary>
        /// Handle two finger gesture
        /// </summary>
        private void HandleTwoFingerGesture(Touch touch1, Touch touch2)
        {
            // Calculate pinch
            float currentDistance = Vector2.Distance(touch1.position, touch2.position);

            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                lastPinchDistance = currentDistance;
                isPinching = true;
            }
            else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                if (isPinching)
                {
                    float pinchDelta = (currentDistance - lastPinchDistance) * pinchSensitivity;
                    lastPinchDistance = currentDistance;
                    TriggerPinch(pinchDelta);
                }
            }
            else if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
            {
                isPinching = false;
            }
        }

        /// <summary>
        /// Trigger tap event
        /// </summary>
        private void TriggerTap()
        {
            TouchEventData data = new TouchEventData
            {
                touchType = TouchType.Tap,
                position = touchCurrentPos,
                deltaPosition = Vector2.zero,
                swipeDirection = SwipeDirection.None
            };

            touchEvents.onTap?.Invoke(data);
        }

        /// <summary>
        /// Trigger double tap event
        /// </summary>
        private void TriggerDoubleTap()
        {
            TouchEventData data = new TouchEventData
            {
                touchType = TouchType.DoubleTap,
                position = touchCurrentPos,
                deltaPosition = Vector2.zero,
                swipeDirection = SwipeDirection.None
            };

            touchEvents.onDoubleTap?.Invoke(data);
        }

        /// <summary>
        /// Trigger hold event
        /// </summary>
        private void TriggerHold()
        {
            TouchEventData data = new TouchEventData
            {
                touchType = TouchType.Hold,
                position = touchCurrentPos,
                deltaPosition = Vector2.zero,
                holdDuration = Time.time - touchStartTime
            };

            touchEvents.onHold?.Invoke(data);
        }

        /// <summary>
        /// Trigger swipe event
        /// </summary>
        private void TriggerSwipe()
        {
            Vector2 delta = touchCurrentPos - touchStartPos;
            float distance = delta.magnitude;
            SwipeDirection direction = GetSwipeDirection(delta);

            TouchEventData data = new TouchEventData
            {
                touchType = TouchType.Swipe,
                position = touchCurrentPos,
                deltaPosition = delta,
                swipeDirection = direction,
                swipeDistance = distance
            };

            touchEvents.onSwipe?.Invoke(data);
        }

        /// <summary>
        /// Trigger pinch event
        /// </summary>
        private void TriggerPinch(float pinchDelta)
        {
            TouchEventData data = new TouchEventData
            {
                touchType = TouchType.Pinch,
                position = Vector2.zero,
                deltaPosition = Vector2.zero,
                pinchDelta = pinchDelta
            };

            touchEvents.onPinch?.Invoke(data);
        }

        /// <summary>
        /// Trigger drag event
        /// </summary>
        private void TriggerDrag()
        {
            Vector2 delta = touchCurrentPos - touchStartPos;

            TouchEventData data = new TouchEventData
            {
                touchType = TouchType.Drag,
                position = touchCurrentPos,
                deltaPosition = delta
            };

            touchEvents.onDrag?.Invoke(data);
        }

        /// <summary>
        /// Get swipe direction from delta
        /// </summary>
        private SwipeDirection GetSwipeDirection(Vector2 delta)
        {
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            if (angle < 0)
                angle += 360f;

            // 8-directional
            if (angle >= 337.5f || angle < 22.5f)
                return SwipeDirection.Right;
            else if (angle >= 22.5f && angle < 67.5f)
                return SwipeDirection.UpRight;
            else if (angle >= 67.5f && angle < 112.5f)
                return SwipeDirection.Up;
            else if (angle >= 112.5f && angle < 157.5f)
                return SwipeDirection.UpLeft;
            else if (angle >= 157.5f && angle < 202.5f)
                return SwipeDirection.Left;
            else if (angle >= 202.5f && angle < 247.5f)
                return SwipeDirection.DownLeft;
            else if (angle >= 247.5f && angle < 292.5f)
                return SwipeDirection.Down;
            else
                return SwipeDirection.DownRight;
        }

        /// <summary>
        /// Get current touch position
        /// </summary>
        public Vector2 GetTouchPosition()
        {
            return touchCurrentPos;
        }

        /// <summary>
        /// Is currently touching
        /// </summary>
        public bool IsTouching()
        {
            return isTouching;
        }

        /// <summary>
        /// Is currently holding
        /// </summary>
        public bool IsHolding()
        {
            return isHolding;
        }

        /// <summary>
        /// Is currently dragging
        /// </summary>
        public bool IsDragging()
        {
            return isDragging;
        }

        /// <summary>
        /// Get touch count
        /// </summary>
        public int GetTouchCount()
        {
            return Input.touchCount;
        }

        /// <summary>
        /// Set touch enabled
        /// </summary>
        public void SetTouchEnabled(bool enabled)
        {
            touchEnabled = enabled;
        }
    }
}
