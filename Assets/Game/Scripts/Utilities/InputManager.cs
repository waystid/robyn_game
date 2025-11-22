using UnityEngine;

namespace CozyGame
{
    /// <summary>
    /// Handles input across all platforms (PC, Mobile, WebGL)
    /// Singleton pattern for easy access from any script
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [Header("Platform Detection")]
        public bool isMobile = false;
        public bool isWebGL = false;
        public bool isPC = false;

        [Header("Input Settings")]
        public bool useVirtualJoystick = false;
        public LayerMask groundLayer;

        [Header("Virtual Joystick Reference")]
        public VirtualJoystick virtualJoystick;

        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                DetectPlatform();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void DetectPlatform()
        {
#if UNITY_WEBGL
            isWebGL = true;
            isPC = false;
            isMobile = false;
            useVirtualJoystick = false;
            Debug.Log("Platform: WebGL");
#elif UNITY_ANDROID || UNITY_IOS
            isMobile = true;
            isPC = false;
            isWebGL = false;
            useVirtualJoystick = true;
            Debug.Log("Platform: Mobile");
#else
            isPC = true;
            isMobile = false;
            isWebGL = false;
            useVirtualJoystick = false;
            Debug.Log("Platform: PC");
#endif
        }

        /// <summary>
        /// Get movement input (works on all platforms)
        /// Returns normalized Vector2 for movement direction
        /// </summary>
        public Vector2 GetMovementInput()
        {
            if (isMobile && useVirtualJoystick && virtualJoystick != null)
            {
                return virtualJoystick.InputDirection;
            }
            else
            {
                // PC/WebGL keyboard input
                float horizontal = Input.GetAxis("Horizontal"); // A/D or Arrow Keys
                float vertical = Input.GetAxis("Vertical");     // W/S or Arrow Keys
                return new Vector2(horizontal, vertical);
            }
        }

        /// <summary>
        /// Check if player pressed interact button (E key or touch)
        /// </summary>
        public bool GetInteractInput()
        {
            if (isMobile)
            {
                // Touch input for mobile (simple tap)
                return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
            }
            else
            {
                // E key or Left Click for PC/WebGL
                return Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0);
            }
        }

        /// <summary>
        /// Check if interact button is being held
        /// </summary>
        public bool GetInteractHold()
        {
            if (isMobile)
            {
                return Input.touchCount > 0;
            }
            else
            {
                return Input.GetKey(KeyCode.E) || Input.GetMouseButton(0);
            }
        }

        /// <summary>
        /// Get world position of pointer (mouse or touch)
        /// Useful for click-to-move or targeting
        /// </summary>
        public Vector3 GetPointerWorldPosition()
        {
            Vector3 screenPosition;

            if (isMobile && Input.touchCount > 0)
            {
                screenPosition = Input.GetTouch(0).position;
            }
            else
            {
                screenPosition = Input.mousePosition;
            }

            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
            {
                return hit.point;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Check if pointer is over UI (prevents clicking through UI)
        /// </summary>
        public bool IsPointerOverUI()
        {
            if (isMobile && Input.touchCount > 0)
            {
                return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            }
            else
            {
                return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            }
        }

        /// <summary>
        /// Get jump/action button
        /// </summary>
        public bool GetJumpInput()
        {
            return Input.GetKeyDown(KeyCode.Space);
        }

        /// <summary>
        /// Get cancel/back button
        /// </summary>
        public bool GetCancelInput()
        {
            return Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace);
        }
    }
}
