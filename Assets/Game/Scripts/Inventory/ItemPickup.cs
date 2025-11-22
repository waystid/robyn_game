using UnityEngine;

namespace CozyGame.Inventory
{
    /// <summary>
    /// Item pickup component for world items.
    /// Attach to item prefabs dropped in the world.
    /// </summary>
    public class ItemPickup : MonoBehaviour
    {
        [Header("Item Data")]
        [Tooltip("Item to give when picked up")]
        public Item item;

        [Tooltip("Quantity")]
        [Range(1, 99)]
        public int quantity = 1;

        [Header("Interaction")]
        [Tooltip("Pickup key")]
        public KeyCode pickupKey = KeyCode.E;

        [Tooltip("Pickup radius")]
        public float pickupRadius = 2f;

        [Tooltip("Auto pickup (no key press needed)")]
        public bool autoPickup = false;

        [Header("Visual")]
        [Tooltip("Prompt text (when in range)")]
        public GameObject promptUI;

        [Header("Audio")]
        [Tooltip("Pickup sound")]
        public string pickupSound = "item_pickup";

        private Transform playerTransform;
        private bool playerInRange = false;

        private void Start()
        {
            if (PlayerController.Instance != null)
            {
                playerTransform = PlayerController.Instance.transform;
            }

            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }
        }

        private void Update()
        {
            if (playerTransform == null || item == null)
                return;

            // Check distance to player
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            playerInRange = (distance <= pickupRadius);

            // Show/hide prompt
            if (promptUI != null)
            {
                promptUI.SetActive(playerInRange);
            }

            // Handle pickup
            if (playerInRange)
            {
                if (autoPickup || Input.GetKeyDown(pickupKey))
                {
                    TryPickup();
                }
            }
        }

        /// <summary>
        /// Try to pickup item
        /// </summary>
        private void TryPickup()
        {
            if (InventorySystem.Instance == null)
            {
                Debug.LogWarning("[ItemPickup] InventorySystem not available!");
                return;
            }

            // Add to inventory
            bool added = InventorySystem.Instance.AddItem(item.itemID, quantity);

            if (added)
            {
                // Play sound
                if (AudioManager.Instance != null && !string.IsNullOrEmpty(pickupSound))
                {
                    AudioManager.Instance.PlaySound(pickupSound);
                }

                // Destroy pickup
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning("[ItemPickup] Inventory full or item rejected!");
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw pickup radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
    }
}
