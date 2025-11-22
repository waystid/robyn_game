using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CozyGame.UI
{
    /// <summary>
    /// Simple virtual joystick for mobile touch input
    /// Attach to a UI panel with background and handle images
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Joystick Components")]
        [Tooltip("The background circle of the joystick")]
        public RectTransform joystickBackground;

        [Tooltip("The handle/knob that moves")]
        public RectTransform joystickHandle;

        [Header("Settings")]
        [Tooltip("Maximum distance the handle can move from center")]
        public float handleRange = 50f;

        [Tooltip("If true, joystick stays in one position. If false, appears where you touch")]
        public bool fixedPosition = true;

        [Tooltip("Dead zone - inputs below this value are ignored")]
        [Range(0f, 1f)]
        public float deadZone = 0.1f;

        private Vector2 inputVector;
        private Vector2 joystickOrigin;
        private Canvas canvas;

        /// <summary>
        /// Current input direction (normalized)
        /// </summary>
        public Vector2 InputDirection
        {
            get
            {
                // Apply dead zone
                if (inputVector.magnitude < deadZone)
                    return Vector2.zero;

                return inputVector;
            }
        }

        private void Start()
        {
            joystickOrigin = joystickBackground.anchoredPosition;
            canvas = GetComponentInParent<Canvas>();

            // Start disabled if on PC
            if (InputManager.Instance != null && InputManager.Instance.isPC)
            {
                gameObject.SetActive(false);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!fixedPosition)
            {
                // Move joystick to touch position
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    transform.parent as RectTransform,
                    eventData.position,
                    eventData.pressEventCamera,
                    out localPoint
                );
                joystickBackground.anchoredPosition = localPoint;
            }

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 position;

            // Convert screen point to local point in joystick space
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickBackground,
                eventData.position,
                eventData.pressEventCamera,
                out position))
            {
                // Clamp position to handle range
                position = Vector2.ClampMagnitude(position, handleRange);
                joystickHandle.anchoredPosition = position;

                // Calculate input vector (normalized)
                inputVector = position / handleRange;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Reset handle to center
            joystickHandle.anchoredPosition = Vector2.zero;
            inputVector = Vector2.zero;

            // Reset joystick position if not fixed
            if (!fixedPosition)
            {
                joystickBackground.anchoredPosition = joystickOrigin;
            }
        }

        /// <summary>
        /// Reset joystick to neutral position
        /// </summary>
        public void ResetJoystick()
        {
            joystickHandle.anchoredPosition = Vector2.zero;
            inputVector = Vector2.zero;
        }

        // Visual debug in editor
        private void OnValidate()
        {
            if (joystickBackground == null)
                joystickBackground = GetComponent<RectTransform>();
        }
    }
}
