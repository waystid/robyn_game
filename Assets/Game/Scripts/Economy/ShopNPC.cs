using UnityEngine;
using UnityEngine.Events;

namespace CozyGame.Economy
{
    /// <summary>
    /// Shop NPC component.
    /// Interactable NPC that opens a shop interface.
    /// </summary>
    public class ShopNPC : MonoBehaviour, Interaction.IInteractable
    {
        [Header("Shop Data")]
        [Tooltip("Shop definition")]
        public Shop shop;

        [Header("NPC Info")]
        [Tooltip("Shop keeper name")]
        public string npcName = "Shopkeeper";

        [Tooltip("Greeting message")]
        [TextArea(2, 3)]
        public string greetingMessage = "Welcome to my shop!";

        [Tooltip("Farewell message")]
        public string farewellMessage = "Come again!";

        [Header("Interaction")]
        [Tooltip("Interaction prompt")]
        public string interactionPrompt = "Talk to Shopkeeper";

        [Tooltip("Interaction range")]
        public float interactionRange = 2f;

        [Header("Visual")]
        [Tooltip("Shop indicator (active when player in range)")]
        public GameObject shopIndicator;

        [Tooltip("NPC animator")]
        public Animator npcAnimator;

        [Header("Audio")]
        [Tooltip("Shop open sound")]
        public string shopOpenSoundName = "shop_open";

        [Tooltip("Shop close sound")]
        public string shopCloseSoundName = "shop_close";

        [Header("Events")]
        public UnityEvent OnShopOpened;
        public UnityEvent OnShopClosed;
        public UnityEvent<ShopItem> OnItemPurchased;
        public UnityEvent<ShopItem> OnItemSold;

        private bool isShopOpen = false;

        private void Start()
        {
            if (shopIndicator != null)
            {
                shopIndicator.SetActive(false);
            }

            if (shop != null)
            {
                Debug.Log($"[ShopNPC] {npcName} initialized with shop: {shop.shopName}");
            }
        }

        private void Update()
        {
            // Update shop (restock timers)
            if (shop != null)
            {
                shop.UpdateShop(Time.deltaTime);
            }
        }

        // ========== IInteractable Implementation ==========

        public string GetInteractionPrompt()
        {
            return interactionPrompt;
        }

        public float GetInteractionRange()
        {
            return interactionRange;
        }

        public bool CanInteract(GameObject interactor)
        {
            if (interactor == null || !interactor.CompareTag("Player"))
                return false;

            if (shop == null)
            {
                Debug.LogWarning($"[ShopNPC] {npcName} has no shop assigned!");
                return false;
            }

            // Check if shop is accessible
            string reason;
            if (!shop.CanAccess(out reason))
            {
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.Show(reason, transform.position + Vector3.up * 2f, Color.red);
                }
                return false;
            }

            return true;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
                return;

            OpenShop();
        }

        /// <summary>
        /// Open the shop
        /// </summary>
        public void OpenShop()
        {
            if (shop == null)
            {
                Debug.LogWarning($"[ShopNPC] {npcName} has no shop assigned!");
                return;
            }

            isShopOpen = true;

            // Show greeting
            if (!string.IsNullOrEmpty(greetingMessage))
            {
                ShowDialogue(greetingMessage);
            }

            // Open shop UI
            if (UI.ShopUI.Instance != null)
            {
                UI.ShopUI.Instance.OpenShop(shop, this);
            }
            else
            {
                // Fallback: find in scene
                UI.ShopUI shopUI = FindObjectOfType<UI.ShopUI>();
                if (shopUI != null)
                {
                    shopUI.OpenShop(shop, this);
                }
            }

            // Play animation
            if (npcAnimator != null)
            {
                npcAnimator.SetTrigger("Talk");
            }

            // Play sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(shopOpenSoundName))
            {
                AudioManager.Instance.PlaySound(shopOpenSoundName);
            }

            // Trigger event
            OnShopOpened?.Invoke();

            Debug.Log($"[ShopNPC] Opened shop: {shop.shopName}");
        }

        /// <summary>
        /// Close the shop
        /// </summary>
        public void CloseShop()
        {
            if (!isShopOpen)
                return;

            isShopOpen = false;

            // Show farewell
            if (!string.IsNullOrEmpty(farewellMessage))
            {
                ShowDialogue(farewellMessage);
            }

            // Play sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(shopCloseSoundName))
            {
                AudioManager.Instance.PlaySound(shopCloseSoundName);
            }

            // Trigger event
            OnShopClosed?.Invoke();

            Debug.Log($"[ShopNPC] Closed shop: {shop.shopName}");
        }

        /// <summary>
        /// On item purchased
        /// </summary>
        public void OnPurchase(ShopItem shopItem)
        {
            OnItemPurchased?.Invoke(shopItem);

            // Play animation
            if (npcAnimator != null)
            {
                npcAnimator.SetTrigger("Happy");
            }

            // Show VFX
            if (VFX.ParticleEffectManager.Instance != null)
            {
                VFX.ParticleEffectManager.Instance.SpawnEffect(VFX.EffectType.Sparkle, transform.position + Vector3.up);
            }
        }

        /// <summary>
        /// On item sold
        /// </summary>
        public void OnSell(ShopItem shopItem)
        {
            OnItemSold?.Invoke(shopItem);
        }

        /// <summary>
        /// Show dialogue
        /// </summary>
        private void ShowDialogue(string message)
        {
            // TODO: Show in proper dialogue system
            // For now, use floating text
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.Show(message, transform.position + Vector3.up * 2f, Color.white);
            }

            Debug.Log($"[ShopNPC] {npcName}: {message}");
        }

        /// <summary>
        /// Check if shop is currently open
        /// </summary>
        public bool IsShopOpen()
        {
            return isShopOpen;
        }

        /// <summary>
        /// Get shop
        /// </summary>
        public Shop GetShop()
        {
            return shop;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);

            #if UNITY_EDITOR
            // Draw label
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2.5f, $"{npcName}\n{(shop != null ? shop.shopName : "No Shop")}");
            #endif
        }
    }
}
