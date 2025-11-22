using UnityEngine;

namespace CozyGame.SurvivalEngine
{
    /// <summary>
    /// Helper class for integrating with Survival Engine
    /// Provides wrapper functions to make integration easier
    /// Uncomment and modify based on your Survival Engine version
    /// </summary>
    public static class SurvivalEngineHelper
    {
        /// <summary>
        /// Add item to player inventory
        /// </summary>
        public static bool AddItemToInventory(string itemID, int quantity)
        {
            // TODO: Uncomment when Survival Engine is imported
            /*
            ItemData itemData = ItemData.Get(itemID);
            if (itemData != null)
            {
                PlayerData player = PlayerData.Get();
                if (player != null)
                {
                    player.inventory.AddItem(itemData, quantity);
                    return true;
                }
            }
            */

            Debug.Log($"[SurvivalEngineHelper] Would add {quantity}x {itemID} to inventory");
            return false;
        }

        /// <summary>
        /// Remove item from player inventory
        /// </summary>
        public static bool RemoveItemFromInventory(string itemID, int quantity)
        {
            // TODO: Uncomment when Survival Engine is imported
            /*
            ItemData itemData = ItemData.Get(itemID);
            if (itemData != null)
            {
                PlayerData player = PlayerData.Get();
                if (player != null && player.inventory.HasItem(itemData, quantity))
                {
                    player.inventory.RemoveItem(itemData, quantity);
                    return true;
                }
            }
            */

            Debug.Log($"[SurvivalEngineHelper] Would remove {quantity}x {itemID} from inventory");
            return false;
        }

        /// <summary>
        /// Check if player has item in inventory
        /// </summary>
        public static bool HasItem(string itemID, int quantity)
        {
            // TODO: Uncomment when Survival Engine is imported
            /*
            ItemData itemData = ItemData.Get(itemID);
            if (itemData != null)
            {
                PlayerData player = PlayerData.Get();
                if (player != null)
                {
                    return player.inventory.HasItem(itemData, quantity);
                }
            }
            */

            Debug.Log($"[SurvivalEngineHelper] Checking for {quantity}x {itemID}");
            return false; // For testing, always return false
        }

        /// <summary>
        /// Get player's current attribute value (health, mana, etc.)
        /// </summary>
        public static float GetAttributeValue(string attributeID)
        {
            // TODO: Uncomment when Survival Engine is imported
            /*
            AttributeData attributeData = AttributeData.Get(attributeID);
            if (attributeData != null)
            {
                PlayerData player = PlayerData.Get();
                if (player != null)
                {
                    return player.GetAttributeValue(attributeData);
                }
            }
            */

            Debug.Log($"[SurvivalEngineHelper] Would get attribute: {attributeID}");
            return 0f;
        }

        /// <summary>
        /// Set player's attribute value
        /// </summary>
        public static void SetAttributeValue(string attributeID, float value)
        {
            // TODO: Uncomment when Survival Engine is imported
            /*
            AttributeData attributeData = AttributeData.Get(attributeID);
            if (attributeData != null)
            {
                PlayerData player = PlayerData.Get();
                if (player != null)
                {
                    player.SetAttributeValue(attributeData, value);
                }
            }
            */

            Debug.Log($"[SurvivalEngineHelper] Would set {attributeID} to {value}");
        }

        /// <summary>
        /// Add value to player's attribute
        /// </summary>
        public static void AddAttributeValue(string attributeID, float amount)
        {
            // TODO: Uncomment when Survival Engine is imported
            /*
            AttributeData attributeData = AttributeData.Get(attributeID);
            if (attributeData != null)
            {
                PlayerData player = PlayerData.Get();
                if (player != null)
                {
                    float currentValue = player.GetAttributeValue(attributeData);
                    player.SetAttributeValue(attributeData, currentValue + amount);
                }
            }
            */

            Debug.Log($"[SurvivalEngineHelper] Would add {amount} to {attributeID}");
        }

        /// <summary>
        /// Get reference to player character
        /// </summary>
        public static GameObject GetPlayer()
        {
            // TODO: Uncomment when Survival Engine is imported
            /*
            PlayerCharacter player = PlayerCharacter.Get();
            if (player != null)
            {
                return player.gameObject;
            }
            */

            // Fallback: find player by tag
            return GameObject.FindGameObjectWithTag("Player");
        }

        /// <summary>
        /// Get player's world position
        /// </summary>
        public static Vector3 GetPlayerPosition()
        {
            GameObject player = GetPlayer();
            if (player != null)
            {
                return player.transform.position;
            }
            return Vector3.zero;
        }

        /// <summary>
        /// Show notification to player
        /// </summary>
        public static void ShowNotification(string message, float duration = 3f)
        {
            // TODO: Uncomment when Survival Engine is imported
            /*
            if (UIController.Get() != null)
            {
                UIController.Get().ShowNotification(message, duration);
            }
            */

            Debug.Log($"[NOTIFICATION] {message}");

            // Fallback: use floating text if available
            if (FloatingTextManager.Instance != null)
            {
                Vector3 playerPos = GetPlayerPosition();
                if (playerPos != Vector3.zero)
                {
                    FloatingTextManager.Instance.Show(message, playerPos + Vector3.up * 2f);
                }
            }
        }

        /// <summary>
        /// Play animation on player
        /// </summary>
        public static void PlayPlayerAnimation(string animationName)
        {
            // TODO: Uncomment when Survival Engine is imported
            /*
            PlayerCharacter player = PlayerCharacter.Get();
            if (player != null && player.GetAnimator() != null)
            {
                player.GetAnimator().SetTrigger(animationName);
            }
            */

            Debug.Log($"[SurvivalEngineHelper] Would play animation: {animationName}");
        }

        /// <summary>
        /// Check if item exists in Survival Engine data
        /// </summary>
        public static bool ItemExists(string itemID)
        {
            // TODO: Uncomment when Survival Engine is imported
            /*
            return ItemData.Get(itemID) != null;
            */

            return false;
        }

        /// <summary>
        /// Get item display name from ID
        /// </summary>
        public static string GetItemName(string itemID)
        {
            // TODO: Uncomment when Survival Engine is imported
            /*
            ItemData itemData = ItemData.Get(itemID);
            if (itemData != null)
            {
                return itemData.title;
            }
            */

            return itemID; // Fallback: return ID as name
        }
    }
}
