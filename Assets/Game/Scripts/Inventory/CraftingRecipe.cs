using UnityEngine;
using System.Collections.Generic;

namespace CozyGame.Inventory
{
    /// <summary>
    /// Ingredient for crafting recipe
    /// </summary>
    [System.Serializable]
    public class CraftingIngredient
    {
        [Tooltip("Required item")]
        public Item item;

        [Tooltip("Required quantity")]
        [Range(1, 99)]
        public int quantity = 1;
    }

    /// <summary>
    /// Crafting recipe ScriptableObject.
    /// Defines how to craft an item from ingredients.
    /// </summary>
    [CreateAssetMenu(fileName = "New Recipe", menuName = "Cozy Game/Inventory/Crafting Recipe")]
    public class CraftingRecipe : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Unique recipe ID")]
        public string recipeID;

        [Tooltip("Recipe name")]
        public string recipeName;

        [Tooltip("Recipe description")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("Recipe icon (uses result item icon if null)")]
        public Sprite icon;

        [Header("Crafting")]
        [Tooltip("Required ingredients")]
        public CraftingIngredient[] ingredients;

        [Tooltip("Result item")]
        public Item resultItem;

        [Tooltip("Result quantity")]
        [Range(1, 99)]
        public int resultQuantity = 1;

        [Header("Requirements")]
        [Tooltip("Required player level")]
        public int requiredLevel = 1;

        [Tooltip("Required skill level (future use)")]
        public int requiredSkillLevel = 0;

        [Tooltip("Crafting time in seconds")]
        public float craftingTime = 2f;

        [Tooltip("Requires crafting station")]
        public bool requiresCraftingStation = false;

        [Tooltip("Required station type")]
        public string requiredStationType = "";

        [Header("Discovery")]
        [Tooltip("Is recipe discovered by default?")]
        public bool isDiscoveredByDefault = false;

        [Tooltip("How to discover this recipe")]
        [TextArea(2, 3)]
        public string discoveryHint = "";

        /// <summary>
        /// Check if player can craft this recipe
        /// </summary>
        public bool CanCraft(out string reason)
        {
            reason = "";

            // Check level requirement
            if (PlayerStats.Instance != null && PlayerStats.Instance.level < requiredLevel)
            {
                reason = $"Requires level {requiredLevel}";
                return false;
            }

            // Check ingredients
            if (InventorySystem.Instance == null)
            {
                reason = "Inventory system not available";
                return false;
            }

            foreach (var ingredient in ingredients)
            {
                int available = InventorySystem.Instance.GetItemCount(ingredient.item.itemID);
                if (available < ingredient.quantity)
                {
                    reason = $"Need {ingredient.quantity - available} more {ingredient.item.itemName}";
                    return false;
                }
            }

            // Check crafting station
            if (requiresCraftingStation)
            {
                // TODO: Check if near required crafting station
                // For now, always return true
            }

            return true;
        }

        /// <summary>
        /// Craft this recipe
        /// </summary>
        public bool Craft()
        {
            string reason;
            if (!CanCraft(out reason))
            {
                Debug.LogWarning($"[CraftingRecipe] Cannot craft {recipeName}: {reason}");
                return false;
            }

            if (InventorySystem.Instance == null)
            {
                Debug.LogError("[CraftingRecipe] InventorySystem not available!");
                return false;
            }

            // Remove ingredients
            foreach (var ingredient in ingredients)
            {
                InventorySystem.Instance.RemoveItem(ingredient.item.itemID, ingredient.quantity);
            }

            // Add result
            InventorySystem.Instance.AddItem(resultItem.itemID, resultQuantity);

            Debug.Log($"[CraftingRecipe] Crafted {resultQuantity}x {resultItem.itemName}");
            return true;
        }

        /// <summary>
        /// Get formatted recipe text
        /// </summary>
        public string GetRecipeText()
        {
            string text = $"<b>{recipeName}</b>\n\n";
            text += $"{description}\n\n";

            text += "<b>Ingredients:</b>\n";
            foreach (var ingredient in ingredients)
            {
                int available = 0;
                if (InventorySystem.Instance != null)
                {
                    available = InventorySystem.Instance.GetItemCount(ingredient.item.itemID);
                }

                Color color = available >= ingredient.quantity ? Color.green : Color.red;
                text += $"- {ingredient.item.itemName} ({available}/{ingredient.quantity})\n";
            }

            text += $"\n<b>Result:</b> {resultQuantity}x {resultItem.itemName}\n";

            if (requiredLevel > 1)
                text += $"\nRequired Level: {requiredLevel}";

            if (craftingTime > 0)
                text += $"\nCrafting Time: {craftingTime}s";

            return text;
        }

        /// <summary>
        /// Validate recipe data
        /// </summary>
        private void OnValidate()
        {
            // Auto-generate ID from name if empty
            if (string.IsNullOrEmpty(recipeID) && !string.IsNullOrEmpty(recipeName))
            {
                recipeID = recipeName.ToLower().Replace(" ", "_");
            }

            // Use result item icon if no icon specified
            if (icon == null && resultItem != null)
            {
                icon = resultItem.icon;
            }
        }
    }
}
