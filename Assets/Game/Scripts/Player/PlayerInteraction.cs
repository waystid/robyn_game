using UnityEngine;
using UnityEngine.Events;
using CozyGame.Plants;

namespace CozyGame
{
    /// <summary>
    /// Handles player interaction with objects in the world.
    /// Uses raycast detection for NPCs, plants, items, and interactable objects.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [Tooltip("Maximum interaction distance")]
        public float interactionRange = 3f;

        [Tooltip("Interaction key (PC)")]
        public KeyCode interactionKey = KeyCode.E;

        [Tooltip("Layers that can be interacted with")]
        public LayerMask interactionLayers = -1; // All layers

        [Tooltip("Show raycast debug line")]
        public bool showDebugRay = true;

        [Header("Raycast Settings")]
        [Tooltip("Raycast from camera or player")]
        public bool raycastFromCamera = true;

        [Tooltip("Use sphere cast instead of raycast")]
        public bool useSphereCast = true;

        [Tooltip("Sphere cast radius")]
        public float sphereCastRadius = 0.3f;

        [Header("UI Feedback")]
        [Tooltip("UI text to show interaction prompt")]
        public TMPro.TextMeshProUGUI interactionPromptText;

        [Tooltip("Interaction prompt message")]
        public string interactionPrompt = "Press E to interact";

        [Tooltip("Mobile interaction prompt")]
        public string mobileInteractionPrompt = "Tap to interact";

        [Tooltip("Show interaction prompt")]
        public bool showInteractionPrompt = true;

        [Header("Highlight Settings")]
        [Tooltip("Highlight interactable objects")]
        public bool highlightInteractables = true;

        [Tooltip("Highlight color")]
        public Color highlightColor = new Color(1f, 1f, 0.5f, 1f);

        [Tooltip("Highlight material (optional)")]
        public Material highlightMaterial;

        // Events
        public UnityEvent<GameObject> OnInteractableDetected;
        public UnityEvent OnInteractableLost;
        public UnityEvent<GameObject> OnInteract;

        // Current state
        private GameObject currentInteractable;
        private IInteractable currentInteractableComponent;
        private Renderer currentHighlightedRenderer;
        private Material[] originalMaterials;
        private Color originalColor;

        // Components
        private Camera playerCamera;

        private void Start()
        {
            // Get camera
            playerCamera = Camera.main;

            // Hide interaction prompt initially
            if (interactionPromptText != null)
            {
                interactionPromptText.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            DetectInteractable();
            HandleInteractionInput();
            UpdateInteractionPrompt();
        }

        /// <summary>
        /// Detect interactable object in front of player
        /// </summary>
        private void DetectInteractable()
        {
            GameObject detectedObject = null;
            RaycastHit hit;
            bool hitSomething = false;

            // Determine raycast origin and direction
            Vector3 origin;
            Vector3 direction;

            if (raycastFromCamera && playerCamera != null)
            {
                // Raycast from camera center
                origin = playerCamera.transform.position;
                direction = playerCamera.transform.forward;
            }
            else
            {
                // Raycast from player position
                origin = transform.position + Vector3.up * 1f; // Offset up slightly
                direction = transform.forward;
            }

            // Perform raycast or spherecast
            if (useSphereCast)
            {
                hitSomething = Physics.SphereCast(
                    origin,
                    sphereCastRadius,
                    direction,
                    out hit,
                    interactionRange,
                    interactionLayers
                );
            }
            else
            {
                hitSomething = Physics.Raycast(
                    origin,
                    direction,
                    out hit,
                    interactionRange,
                    interactionLayers
                );
            }

            // Debug visualization
            if (showDebugRay)
            {
                Debug.DrawRay(origin, direction * interactionRange, hitSomething ? Color.green : Color.red);
            }

            // Check if we hit an interactable
            if (hitSomething)
            {
                GameObject hitObject = hit.collider.gameObject;

                // Check for IInteractable component
                IInteractable interactable = hitObject.GetComponent<IInteractable>();
                if (interactable == null)
                {
                    // Check parent
                    interactable = hitObject.GetComponentInParent<IInteractable>();
                    if (interactable != null)
                    {
                        hitObject = ((MonoBehaviour)interactable).gameObject;
                    }
                }

                if (interactable != null && interactable.CanInteract())
                {
                    detectedObject = hitObject;
                }
            }

            // Update current interactable
            if (detectedObject != currentInteractable)
            {
                // Lost previous interactable
                if (currentInteractable != null)
                {
                    RemoveHighlight();
                    OnInteractableLost?.Invoke();
                }

                // Found new interactable
                currentInteractable = detectedObject;

                if (currentInteractable != null)
                {
                    currentInteractableComponent = currentInteractable.GetComponent<IInteractable>();
                    ApplyHighlight(currentInteractable);
                    OnInteractableDetected?.Invoke(currentInteractable);
                }
                else
                {
                    currentInteractableComponent = null;
                }
            }
        }

        /// <summary>
        /// Handle interaction input
        /// </summary>
        private void HandleInteractionInput()
        {
            if (currentInteractable == null || currentInteractableComponent == null)
                return;

            bool interactPressed = false;

            // PC input
            if (Input.GetKeyDown(interactionKey))
            {
                interactPressed = true;
            }

            // Mobile input - handled by NPCInteractable tap detection
            // But we can also support general tap-to-interact here if needed

            if (interactPressed)
            {
                PerformInteraction();
            }
        }

        /// <summary>
        /// Perform interaction with current interactable
        /// </summary>
        private void PerformInteraction()
        {
            if (currentInteractableComponent != null && currentInteractableComponent.CanInteract())
            {
                currentInteractableComponent.Interact(gameObject);
                OnInteract?.Invoke(currentInteractable);
            }
        }

        /// <summary>
        /// Update interaction prompt UI
        /// </summary>
        private void UpdateInteractionPrompt()
        {
            if (!showInteractionPrompt || interactionPromptText == null)
                return;

            if (currentInteractable != null && currentInteractableComponent != null)
            {
                // Show prompt
                interactionPromptText.gameObject.SetActive(true);

                // Get interaction text from interactable
                string promptText = currentInteractableComponent.GetInteractionPrompt();

                if (string.IsNullOrEmpty(promptText))
                {
                    // Use default prompt
                    if (InputManager.Instance != null && InputManager.Instance.IsMobile())
                    {
                        promptText = mobileInteractionPrompt;
                    }
                    else
                    {
                        promptText = interactionPrompt;
                    }
                }

                interactionPromptText.text = promptText;
            }
            else
            {
                // Hide prompt
                interactionPromptText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Apply highlight to interactable object
        /// </summary>
        private void ApplyHighlight(GameObject obj)
        {
            if (!highlightInteractables)
                return;

            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer == null)
            {
                renderer = obj.GetComponentInChildren<Renderer>();
            }

            if (renderer != null)
            {
                currentHighlightedRenderer = renderer;

                if (highlightMaterial != null)
                {
                    // Store original materials
                    originalMaterials = renderer.materials;

                    // Apply highlight material
                    Material[] newMaterials = new Material[renderer.materials.Length + 1];
                    renderer.materials.CopyTo(newMaterials, 0);
                    newMaterials[newMaterials.Length - 1] = highlightMaterial;
                    renderer.materials = newMaterials;
                }
                else
                {
                    // Tint existing material
                    if (renderer.material.HasProperty("_Color"))
                    {
                        originalColor = renderer.material.color;
                        renderer.material.color = highlightColor;
                    }
                }
            }
        }

        /// <summary>
        /// Remove highlight from current object
        /// </summary>
        private void RemoveHighlight()
        {
            if (currentHighlightedRenderer == null)
                return;

            if (highlightMaterial != null && originalMaterials != null)
            {
                // Restore original materials
                currentHighlightedRenderer.materials = originalMaterials;
                originalMaterials = null;
            }
            else
            {
                // Restore original color
                if (currentHighlightedRenderer.material.HasProperty("_Color"))
                {
                    currentHighlightedRenderer.material.color = originalColor;
                }
            }

            currentHighlightedRenderer = null;
        }

        /// <summary>
        /// Force interact with specific object
        /// </summary>
        public void InteractWith(GameObject obj)
        {
            IInteractable interactable = obj.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract())
            {
                interactable.Interact(gameObject);
                OnInteract?.Invoke(obj);
            }
        }

        /// <summary>
        /// Get currently targeted interactable
        /// </summary>
        public GameObject GetCurrentInteractable()
        {
            return currentInteractable;
        }

        /// <summary>
        /// Check if player is currently targeting an interactable
        /// </summary>
        public bool IsTargetingInteractable()
        {
            return currentInteractable != null;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw interaction range sphere
            Gizmos.color = Color.cyan;

            Vector3 origin = raycastFromCamera && Camera.main != null
                ? Camera.main.transform.position
                : transform.position + Vector3.up * 1f;

            Vector3 direction = raycastFromCamera && Camera.main != null
                ? Camera.main.transform.forward
                : transform.forward;

            Gizmos.DrawWireSphere(origin + direction * interactionRange, sphereCastRadius);
        }
    }

    /// <summary>
    /// Interface for interactable objects
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Called when player interacts with this object
        /// </summary>
        void Interact(GameObject player);

        /// <summary>
        /// Check if this object can currently be interacted with
        /// </summary>
        bool CanInteract();

        /// <summary>
        /// Get the interaction prompt text
        /// </summary>
        string GetInteractionPrompt();
    }
}
