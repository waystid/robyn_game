using UnityEngine;

namespace CozyGame
{
    /// <summary>
    /// Adds gentle floating/bobbing motion
    /// Perfect for fireflies, fairies, magical orbs, and floating objects
    /// Can be combined with SimpleWanderAI for wandering + floating
    /// </summary>
    public class FloatingMotion : MonoBehaviour
    {
        [Header("Float Settings")]
        [Tooltip("Speed of up/down floating motion")]
        public float floatSpeed = 1f;

        [Tooltip("How high/low the object floats (amplitude)")]
        public float floatHeight = 0.5f;

        [Tooltip("Start each object at random point in animation")]
        public bool randomOffset = true;

        [Header("Sway Settings")]
        [Tooltip("Enable side-to-side swaying motion")]
        public bool enableSway = true;

        [Tooltip("Speed of side-to-side sway")]
        public float swaySpeed = 0.5f;

        [Tooltip("How far to sway left/right")]
        public float swayAmount = 0.3f;

        [Header("Rotation Settings")]
        [Tooltip("Enable gentle rotation while floating")]
        public bool enableRotation = false;

        [Tooltip("Rotation speed (degrees per second)")]
        public Vector3 rotationSpeed = new Vector3(0f, 30f, 0f);

        [Header("Advanced")]
        [Tooltip("Use unscaled time (ignores time scale)")]
        public bool useUnscaledTime = false;

        [Tooltip("Local space movement (relative to parent)")]
        public bool localSpace = false;

        private Vector3 startPosition;
        private Quaternion startRotation;
        private float timeOffset;

        private void Start()
        {
            // Store starting position and rotation
            startPosition = localSpace ? transform.localPosition : transform.position;
            startRotation = transform.localRotation;

            // Random time offset so objects don't all move in sync
            if (randomOffset)
            {
                timeOffset = Random.Range(0f, 100f);
            }
        }

        private void Update()
        {
            ApplyFloating();

            if (enableRotation)
            {
                ApplyRotation();
            }
        }

        private void ApplyFloating()
        {
            float time = GetTime() + timeOffset;

            // Calculate up/down floating
            float yOffset = Mathf.Sin(time * floatSpeed) * floatHeight;

            // Calculate side-to-side swaying
            float xOffset = 0f;
            float zOffset = 0f;

            if (enableSway)
            {
                // Use different frequencies for more natural motion
                xOffset = Mathf.Sin(time * swaySpeed) * swayAmount;
                zOffset = Mathf.Cos(time * swaySpeed * 0.7f) * swayAmount * 0.5f;
            }

            // Apply movement
            Vector3 offset = new Vector3(xOffset, yOffset, zOffset);

            if (localSpace)
            {
                transform.localPosition = startPosition + offset;
            }
            else
            {
                // For world space, need to handle if object is also moving (like with WanderAI)
                if (GetComponent<SimpleWanderAI>() != null)
                {
                    // Update start position continuously when wandering
                    startPosition = transform.position - offset;
                }

                transform.position = startPosition + offset;
            }
        }

        private void ApplyRotation()
        {
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            // Rotate continuously
            Vector3 rotation = rotationSpeed * deltaTime;
            transform.Rotate(rotation, Space.Self);
        }

        private float GetTime()
        {
            return useUnscaledTime ? Time.unscaledTime : Time.time;
        }

        /// <summary>
        /// Reset to starting position and rotation
        /// </summary>
        public void ResetPosition()
        {
            if (localSpace)
            {
                transform.localPosition = startPosition;
            }
            else
            {
                transform.position = startPosition;
            }
            transform.localRotation = startRotation;
        }

        /// <summary>
        /// Set new center position for floating
        /// </summary>
        public void SetFloatingCenter(Vector3 newCenter)
        {
            startPosition = newCenter;
        }

        /// <summary>
        /// Pause floating motion
        /// </summary>
        public void PauseFloating()
        {
            enabled = false;
        }

        /// <summary>
        /// Resume floating motion
        /// </summary>
        public void ResumeFloating()
        {
            enabled = true;
        }

        /// <summary>
        /// Set float speed (useful for speeding up/slowing down dynamically)
        /// </summary>
        public void SetFloatSpeed(float speed)
        {
            floatSpeed = speed;
        }

        /// <summary>
        /// Set float height (useful for dramatic effects)
        /// </summary>
        public void SetFloatHeight(float height)
        {
            floatHeight = height;
        }

        // Visualize floating range in editor
        private void OnDrawGizmosSelected()
        {
            Vector3 center = Application.isPlaying ? startPosition : transform.position;

            // Draw float range
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(center + Vector3.up * floatHeight, 0.1f);
            Gizmos.DrawWireSphere(center - Vector3.up * floatHeight, 0.1f);
            Gizmos.DrawLine(center + Vector3.up * floatHeight, center - Vector3.up * floatHeight);

            // Draw sway range if enabled
            if (enableSway)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(center + Vector3.right * swayAmount, 0.1f);
                Gizmos.DrawWireSphere(center - Vector3.right * swayAmount, 0.1f);
            }
        }
    }
}
