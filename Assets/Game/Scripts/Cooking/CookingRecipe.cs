using UnityEngine;
using System.Collections.Generic;
using CozyGame.Inventory;

namespace CozyGame.Cooking
{
    /// <summary>
    /// Recipe ingredient
    /// </summary>
    [System.Serializable]
    public class RecipeIngredient
    {
        [Tooltip("Ingredient item")]
        public Item ingredient;

        [Tooltip("Quantity required")]
        public int quantity = 1;

        [Tooltip("Is optional (for variations)")]
        public bool isOptional = false;

        [Tooltip("Quality bonus if used")]
        public float qualityBonus = 0f;
    }

    /// <summary>
    /// Recipe discovery method
    /// </summary>
    public enum DiscoveryMethod
    {
        Known,              // Start with recipe
        FindRecipeBook,     // Find recipe item
        ExperimentSuccess,  // Discover by trying combinations
        NPCTeach,           // Learn from NPC
        QuestReward,        // Complete quest
        LevelUnlock         // Reach certain level
    }

    /// <summary>
    /// Cooking recipe ScriptableObject.
    /// Defines ingredient combinations and results.
    /// Create via: Right-click → Create → Cozy Game → Cooking → Cooking Recipe
    /// </summary>
    [CreateAssetMenu(fileName = "New Cooking Recipe", menuName = "Cozy Game/Cooking/Cooking Recipe", order = 11)]
    public class CookingRecipe : ScriptableObject
    {
        [Header("Recipe Info")]
        [Tooltip("Recipe name")]
        public string recipeName = "Stew";

        [Tooltip("Unique recipe ID")]
        public string recipeID;

        [Tooltip("Recipe description")]
        [TextArea(2, 3)]
        public string description = "A hearty stew";

        [Tooltip("Recipe icon")]
        public Sprite icon;

        [Tooltip("Recipe category")]
        public FoodCategory category = FoodCategory.Meal;

        [Header("Ingredients")]
        [Tooltip("Required ingredients")]
        public RecipeIngredient[] ingredients;

        [Tooltip("Result item (food)")]
        public FoodItem resultFood;

        [Tooltip("Result quantity")]
        public int resultQuantity = 1;

        [Header("Cooking")]
        [Tooltip("Cooking time (seconds)")]
        public float cookingTime = 15f;

        [Tooltip("Required cooking station type")]
        public string requiredStationType = ""; // Empty = any

        [Tooltip("Required player level")]
        public int requiredLevel = 1;

        [Tooltip("Cooking difficulty (0-1)")]
        [Range(0f, 1f)]
        public float difficulty = 0.5f;

        [Header("Success/Failure")]
        [Tooltip("Base success chance (0-1)")]
        [Range(0f, 1f)]
        public float baseSuccessChance = 0.9f;

        [Tooltip("Perfect cook chance (0-1)")]
        [Range(0f, 1f)]
        public float perfectChance = 0.1f;

        [Tooltip("Burn chance on failure (0-1)")]
        [Range(0f, 1f)]
        public float burnChance = 0.5f;

        [Tooltip("Result quality on poor cook")]
        public FoodQuality failureQuality = FoodQuality.Poor;

        [Tooltip("Result quality on perfect cook")]
        public FoodQuality perfectQuality = FoodQuality.Excellent;

        [Header("Discovery")]
        [Tooltip("How this recipe is discovered")]
        public DiscoveryMethod discoveryMethod = DiscoveryMethod.Known;

        [Tooltip("Is recipe unlocked by default")]
        public bool startUnlocked = false;

        [Tooltip("Recipe book item (if found)")]
        public Item recipeBookItem;

        [Tooltip("Required quest ID (if quest reward)")]
        public string requiredQuestID = "";

        [Tooltip("Required level to unlock (if level unlock)")]
        public int unlockLevel = 1;

        [Header("Rewards")]
        [Tooltip("Experience granted on cook")]
        public int experienceReward = 10;

        [Tooltip("Bonus experience on perfect")]
        public int perfectBonusExp = 5;

        [Header("Advanced")]
        [Tooltip("Allow experimentation (discover by trying)")]
        public bool allowExperimentation = true;

        [Tooltip("Ingredient order matters")]
        public bool orderMatters = false;

        [Tooltip("Can be cooked in bulk (multiple at once)")]
        public bool allowBulkCooking = true;

        [Tooltip("Max bulk quantity")]
        public int maxBulkQuantity = 5;

        private void OnEnable()
        {
            // Generate unique ID if empty
            if (string.IsNullOrEmpty(recipeID))
            {
                recipeID = "recipe_" + name.ToLower().Replace(" ", "_");
            }
        }

        /// <summary>
        /// Check if player can cook this recipe
        /// </summary>
        public bool CanCook(out string reason)
        {
            reason = "";

            // Check level
            if (PlayerStats.Instance != null && PlayerStats.Instance.level < requiredLevel)
            {
                reason = $"Requires level {requiredLevel}";
                return false;
            }

            // Check ingredients
            if (ingredients == null || ingredients.Length == 0)
            {
                reason = "Invalid recipe";
                return false;
            }

            foreach (var ing in ingredients)
            {
                if (ing.ingredient == null)
                    continue;

                if (ing.isOptional)
                    continue;

                int playerAmount = InventorySystem.Instance.GetItemCount(ing.ingredient.itemID);
                if (playerAmount < ing.quantity)
                {
                    reason = $"Need {ing.quantity}x {ing.ingredient.itemName}";
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if has all ingredients
        /// </summary>
        public bool HasAllIngredients(out List<RecipeIngredient> missing)
        {
            missing = new List<RecipeIngredient>();

            if (ingredients == null)
                return false;

            foreach (var ing in ingredients)
            {
                if (ing.ingredient == null || ing.isOptional)
                    continue;

                int playerAmount = InventorySystem.Instance.GetItemCount(ing.ingredient.itemID);
                if (playerAmount < ing.quantity)
                {
                    missing.Add(ing);
                }
            }

            return missing.Count == 0;
        }

        /// <summary>
        /// Consume ingredients from inventory
        /// </summary>
        public bool ConsumeIngredients()
        {
            string reason;
            if (!CanCook(out reason))
            {
                Debug.LogWarning($"[CookingRecipe] Cannot cook: {reason}");
                return false;
            }

            // Consume ingredients
            foreach (var ing in ingredients)
            {
                if (ing.ingredient == null || ing.isOptional)
                    continue;

                InventorySystem.Instance.RemoveItem(ing.ingredient.itemID, ing.quantity);
            }

            return true;
        }

        /// <summary>
        /// Calculate success chance with modifiers
        /// </summary>
        public float CalculateSuccessChance()
        {
            float chance = baseSuccessChance;

            // Level bonus
            if (PlayerStats.Instance != null)
            {
                int levelDiff = PlayerStats.Instance.level - requiredLevel;
                if (levelDiff > 0)
                {
                    chance += levelDiff * 0.02f; // +2% per level above requirement
                }
                else if (levelDiff < 0)
                {
                    chance += levelDiff * 0.05f; // -5% per level below requirement
                }
            }

            // Optional ingredient bonuses
            if (ingredients != null)
            {
                foreach (var ing in ingredients)
                {
                    if (ing.isOptional && ing.ingredient != null)
                    {
                        int playerAmount = InventorySystem.Instance.GetItemCount(ing.ingredient.itemID);
                        if (playerAmount >= ing.quantity)
                        {
                            chance += ing.qualityBonus;
                        }
                    }
                }
            }

            return Mathf.Clamp01(chance);
        }

        /// <summary>
        /// Roll for cooking result
        /// </summary>
        public FoodQuality RollForQuality(out bool success)
        {
            float successChance = CalculateSuccessChance();
            float roll = Random.value;

            // Check for failure
            if (roll > successChance)
            {
                success = false;

                // Check if burnt
                if (Random.value <= burnChance)
                {
                    return FoodQuality.Poor; // Burnt
                }
                else
                {
                    return FoodQuality.Normal; // Failed but salvageable
                }
            }

            success = true;

            // Check for perfect
            if (Random.value <= perfectChance)
            {
                return perfectQuality;
            }

            // Normal success
            return FoodQuality.Good;
        }

        /// <summary>
        /// Get ingredient list as string
        /// </summary>
        public string GetIngredientList()
        {
            if (ingredients == null || ingredients.Length == 0)
                return "No ingredients";

            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (var ing in ingredients)
            {
                if (ing.ingredient == null)
                    continue;

                string optional = ing.isOptional ? " (optional)" : "";
                sb.AppendLine($"• {ing.quantity}x {ing.ingredient.itemName}{optional}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get difficulty string
        /// </summary>
        public string GetDifficultyString()
        {
            if (difficulty < 0.2f) return "Very Easy";
            if (difficulty < 0.4f) return "Easy";
            if (difficulty < 0.6f) return "Medium";
            if (difficulty < 0.8f) return "Hard";
            return "Very Hard";
        }

        /// <summary>
        /// Get difficulty color
        /// </summary>
        public Color GetDifficultyColor()
        {
            if (difficulty < 0.2f) return Color.white;
            if (difficulty < 0.4f) return Color.green;
            if (difficulty < 0.6f) return Color.yellow;
            if (difficulty < 0.8f) return new Color(1f, 0.5f, 0f); // Orange
            return Color.red;
        }
    }
}
