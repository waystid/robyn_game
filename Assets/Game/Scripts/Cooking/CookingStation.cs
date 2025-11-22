using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace CozyGame.Cooking
{
    /// <summary>
    /// Cooking station type
    /// </summary>
    public enum CookingStationType
    {
        Campfire,
        Stove,
        Oven,
        Cauldron,
        Grill,
        Custom
    }

    /// <summary>
    /// Cooking station component.
    /// Processes recipes into food items.
    /// </summary>
    public class CookingStation : MonoBehaviour, Interaction.IInteractable
    {
        [Header("Station Info")]
        [Tooltip("Station name")]
        public string stationName = "Cooking Pot";

        [Tooltip("Station type")]
        public CookingStationType stationType = CookingStationType.Campfire;

        [Tooltip("Station type string (for recipe requirements)")]
        public string stationTypeString = "campfire";

        [Tooltip("Available recipes")]
        public CookingRecipe[] availableRecipes;

        [Header("Current Cooking")]
        [Tooltip("Is currently cooking")]
        public bool isCooking = false;

        [Tooltip("Current recipe being cooked")]
        public CookingRecipe currentRecipe;

        [Tooltip("Cooking progress (0-1)")]
        [Range(0f, 1f)]
        public float cookingProgress = 0f;

        [Tooltip("Cooking timer")]
        public float cookingTimer = 0f;

        [Tooltip("Target quality (rolled at start)")]
        public FoodQuality targetQuality = FoodQuality.Normal;

        [Header("Bulk Cooking")]
        [Tooltip("Bulk cooking quantity")]
        public int bulkQuantity = 1;

        [Tooltip("Bulk cooking remaining")]
        public int bulkRemaining = 0;

        [Header("Visual")]
        [Tooltip("Cooking particle effect")]
        public GameObject cookingParticles;

        [Tooltip("Food model spawn point")]
        public Transform foodSpawnPoint;

        [Header("Events")]
        public UnityEvent<CookingRecipe> OnCookingStarted;
        public UnityEvent<FoodItem, FoodQuality, int> OnCookingCompleted; // food, quality, quantity
        public UnityEvent<CookingRecipe> OnCookingFailed;

        private void Update()
        {
            if (isCooking && currentRecipe != null)
            {
                UpdateCooking();
            }
        }

        /// <summary>
        /// IInteractable: Check if can interact
        /// </summary>
        public bool CanInteract(GameObject interactor)
        {
            return !isCooking;
        }

        /// <summary>
        /// IInteractable: Get interaction prompt
        /// </summary>
        public string GetInteractionPrompt()
        {
            return $"Use {stationName}";
        }

        /// <summary>
        /// IInteractable: Interact
        /// </summary>
        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
                return;

            // Open cooking UI
            if (UI.CookingUI.Instance != null)
            {
                UI.CookingUI.Instance.OpenCookingUI(this);
            }
        }

        /// <summary>
        /// Start cooking recipe
        /// </summary>
        public bool StartCooking(CookingRecipe recipe, int quantity = 1)
        {
            if (recipe == null || isCooking)
                return false;

            // Check if can cook
            string reason;
            if (!recipe.CanCook(out reason))
            {
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.Show(reason, transform.position + Vector3.up, Color.red);
                }
                return false;
            }

            // Check station requirement
            if (!string.IsNullOrEmpty(recipe.requiredStationType))
            {
                if (!recipe.requiredStationType.Equals(stationTypeString, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (FloatingTextManager.Instance != null)
                    {
                        FloatingTextManager.Instance.Show($"Requires {recipe.requiredStationType}!", transform.position + Vector3.up, Color.red);
                    }
                    return false;
                }
            }

            // Check bulk cooking
            if (quantity > 1)
            {
                if (!recipe.allowBulkCooking)
                {
                    quantity = 1;
                }
                else
                {
                    quantity = Mathf.Min(quantity, recipe.maxBulkQuantity);
                }
            }

            // Consume ingredients
            for (int i = 0; i < quantity; i++)
            {
                if (!recipe.ConsumeIngredients())
                {
                    if (FloatingTextManager.Instance != null)
                    {
                        FloatingTextManager.Instance.Show($"Only have ingredients for {i} servings!", transform.position + Vector3.up, Color.yellow);
                    }
                    quantity = i;
                    break;
                }
            }

            if (quantity <= 0)
                return false;

            // Start cooking
            currentRecipe = recipe;
            bulkQuantity = quantity;
            bulkRemaining = quantity;
            isCooking = true;
            cookingProgress = 0f;
            cookingTimer = 0f;

            // Roll for quality
            bool success;
            targetQuality = recipe.RollForQuality(out success);

            // Start visual effects
            if (cookingParticles != null)
            {
                cookingParticles.SetActive(true);
            }

            // Trigger event
            OnCookingStarted?.Invoke(recipe);

            Debug.Log($"[CookingStation] Started cooking {quantity}x {recipe.recipeName}");

            return true;
        }

        /// <summary>
        /// Update cooking progress
        /// </summary>
        private void UpdateCooking()
        {
            if (currentRecipe == null)
                return;

            cookingTimer += Time.deltaTime;
            cookingProgress = cookingTimer / currentRecipe.cookingTime;

            if (cookingProgress >= 1f)
            {
                CompleteCooking();
            }
        }

        /// <summary>
        /// Complete cooking
        /// </summary>
        private void CompleteCooking()
        {
            if (currentRecipe == null || currentRecipe.resultFood == null)
                return;

            // Create food item with quality
            FoodItem resultFood = Instantiate(currentRecipe.resultFood);
            resultFood.foodQuality = targetQuality;

            // Apply quality multiplier to effects
            float qualityMult = resultFood.GetQualityMultiplier();
            resultFood.healthRestore *= qualityMult;
            resultFood.manaRestore *= qualityMult;
            resultFood.hungerRestore *= qualityMult;

            // Add to inventory
            int quantity = currentRecipe.resultQuantity;
            Inventory.InventorySystem.Instance.AddItem(resultFood.itemID, quantity);

            // Grant experience
            int exp = currentRecipe.experienceReward;
            if (targetQuality == currentRecipe.perfectQuality)
            {
                exp += currentRecipe.perfectBonusExp;
            }

            if (PlayerStats.Instance != null && exp > 0)
            {
                PlayerStats.Instance.GainExperience(exp);
            }

            // Show notification
            if (FloatingTextManager.Instance != null)
            {
                string text = $"+{quantity}x {resultFood.itemName}";
                if (targetQuality == currentRecipe.perfectQuality)
                {
                    text += " (Perfect!)";
                }
                FloatingTextManager.Instance.ShowItemPickup(
                    resultFood.itemName,
                    quantity,
                    transform.position + Vector3.up,
                    targetQuality == currentRecipe.perfectQuality || targetQuality == FoodQuality.Masterpiece
                );
            }

            // Spawn food model
            if (foodSpawnPoint != null && resultFood.foodPrefab != null)
            {
                GameObject foodObj = Instantiate(resultFood.foodPrefab, foodSpawnPoint.position, Quaternion.identity);
                Destroy(foodObj, 2f); // Auto-destroy after 2 seconds
            }

            // Trigger event
            OnCookingCompleted?.Invoke(resultFood, targetQuality, quantity);

            Debug.Log($"[CookingStation] Completed cooking: {quantity}x {resultFood.itemName} ({targetQuality})");

            // Check if bulk cooking remaining
            bulkRemaining--;
            if (bulkRemaining > 0)
            {
                // Cook next
                cookingProgress = 0f;
                cookingTimer = 0f;

                // Roll for quality
                bool success;
                targetQuality = currentRecipe.RollForQuality(out success);
            }
            else
            {
                // All done
                StopCooking();
            }
        }

        /// <summary>
        /// Stop cooking
        /// </summary>
        public void StopCooking()
        {
            isCooking = false;
            currentRecipe = null;
            cookingProgress = 0f;
            cookingTimer = 0f;
            bulkQuantity = 1;
            bulkRemaining = 0;

            // Stop visual effects
            if (cookingParticles != null)
            {
                cookingParticles.SetActive(false);
            }
        }

        /// <summary>
        /// Cancel current cooking (with partial refund)
        /// </summary>
        public void CancelCooking()
        {
            if (!isCooking || currentRecipe == null)
                return;

            // Refund ingredients based on progress (50% if < 50% done, 25% otherwise)
            float refundPercent = cookingProgress < 0.5f ? 0.5f : 0.25f;

            foreach (var ing in currentRecipe.ingredients)
            {
                if (ing.ingredient == null || ing.isOptional)
                    continue;

                int refundAmount = Mathf.CeilToInt(ing.quantity * bulkRemaining * refundPercent);
                if (refundAmount > 0)
                {
                    Inventory.InventorySystem.Instance.AddItem(ing.ingredient.itemID, refundAmount);
                }
            }

            // Show notification
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.Show("Cooking cancelled", transform.position + Vector3.up, Color.yellow);
            }

            StopCooking();

            Debug.Log($"[CookingStation] Cooking cancelled (refunded {refundPercent * 100}%)");
        }

        /// <summary>
        /// Get available recipes for this station
        /// </summary>
        public List<CookingRecipe> GetAvailableRecipes()
        {
            List<CookingRecipe> recipes = new List<CookingRecipe>();

            if (availableRecipes != null)
            {
                recipes.AddRange(availableRecipes);
            }

            // Filter by station type
            recipes.RemoveAll(r => !string.IsNullOrEmpty(r.requiredStationType) &&
                                  !r.requiredStationType.Equals(stationTypeString, System.StringComparison.OrdinalIgnoreCase));

            return recipes;
        }

        /// <summary>
        /// Get cookable recipes (player has ingredients)
        /// </summary>
        public List<CookingRecipe> GetCookableRecipes()
        {
            List<CookingRecipe> cookable = new List<CookingRecipe>();

            foreach (var recipe in GetAvailableRecipes())
            {
                string reason;
                if (recipe.CanCook(out reason))
                {
                    cookable.Add(recipe);
                }
            }

            return cookable;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.5f);

            #if UNITY_EDITOR
            string label = $"{stationName} ({stationType})";
            if (isCooking && currentRecipe != null)
            {
                label += $"\nCooking: {currentRecipe.recipeName}";
                label += $"\n{cookingProgress * 100f:F0}%";
                if (bulkRemaining > 1)
                {
                    label += $" ({bulkRemaining} remaining)";
                }
            }
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, label);
            #endif
        }
    }
}
