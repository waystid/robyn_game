using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CozyGame.Inventory;

namespace CozyGame.Mining
{
    /// <summary>
    /// Smelting recipe data
    /// </summary>
    [System.Serializable]
    public class SmeltingRecipe
    {
        [Header("Recipe Info")]
        [Tooltip("Recipe name")]
        public string recipeName = "Iron Bar";

        [Tooltip("Input item (ore)")]
        public Item inputItem;

        [Tooltip("Input quantity")]
        public int inputQuantity = 1;

        [Tooltip("Output item (bar/ingot)")]
        public Item outputItem;

        [Tooltip("Output quantity")]
        public int outputQuantity = 1;

        [Header("Requirements")]
        [Tooltip("Smelting time (seconds)")]
        public float smeltingTime = 10f;

        [Tooltip("Fuel cost (coal/wood)")]
        public int fuelCost = 1;

        [Tooltip("Required smelting level")]
        public int requiredLevel = 1;

        [Tooltip("Experience granted")]
        public int experienceReward = 10;

        [Header("Advanced")]
        [Tooltip("Success chance (0-1, 1 = always succeeds)")]
        [Range(0f, 1f)]
        public float successChance = 1f;

        [Tooltip("Bonus output on critical success")]
        public int criticalBonusOutput = 0;

        [Tooltip("Critical chance (0-1)")]
        [Range(0f, 1f)]
        public float criticalChance = 0.1f;

        /// <summary>
        /// Check if recipe can be crafted
        /// </summary>
        public bool CanCraft(out string reason)
        {
            reason = "";

            // Check input items
            if (inputItem == null || outputItem == null)
            {
                reason = "Invalid recipe";
                return false;
            }

            int playerAmount = InventorySystem.Instance.GetItemCount(inputItem.itemID);
            if (playerAmount < inputQuantity)
            {
                reason = $"Need {inputQuantity}x {inputItem.itemName}";
                return false;
            }

            // Check level
            if (PlayerStats.Instance != null && PlayerStats.Instance.level < requiredLevel)
            {
                reason = $"Requires level {requiredLevel}";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Roll for critical success
        /// </summary>
        public bool RollForCritical()
        {
            return Random.value <= criticalChance;
        }

        /// <summary>
        /// Roll for success
        /// </summary>
        public bool RollForSuccess()
        {
            return Random.value <= successChance;
        }
    }

    /// <summary>
    /// Smelting station component.
    /// Processes ores into bars/ingots.
    /// </summary>
    public class SmeltingStation : MonoBehaviour, Interaction.IInteractable
    {
        [Header("Station Info")]
        [Tooltip("Station name")]
        public string stationName = "Furnace";

        [Tooltip("Available recipes")]
        public SmeltingRecipe[] recipes;

        [Header("Fuel")]
        [Tooltip("Fuel item (coal, wood, etc.)")]
        public Item fuelItem;

        [Tooltip("Current fuel amount")]
        public int currentFuel = 0;

        [Tooltip("Max fuel capacity")]
        public int maxFuelCapacity = 100;

        [Header("Current Smelting")]
        [Tooltip("Is currently smelting")]
        public bool isSmelting = false;

        [Tooltip("Current recipe being smelted")]
        public SmeltingRecipe currentRecipe;

        [Tooltip("Smelting progress (0-1)")]
        [Range(0f, 1f)]
        public float smeltingProgress = 0f;

        [Tooltip("Smelting timer")]
        public float smeltingTimer = 0f;

        [Header("Visual")]
        [Tooltip("Smelting particle effect")]
        public GameObject smeltingParticles;

        [Tooltip("Fire/flame object")]
        public GameObject fireObject;

        [Header("Events")]
        public UnityEvent<SmeltingRecipe> OnSmeltingStarted;
        public UnityEvent<SmeltingRecipe, int> OnSmeltingCompleted; // recipe, output quantity
        public UnityEvent<SmeltingRecipe> OnSmeltingFailed;

        private void Update()
        {
            if (isSmelting && currentRecipe != null)
            {
                UpdateSmelting();
            }
        }

        /// <summary>
        /// IInteractable: Check if can interact
        /// </summary>
        public bool CanInteract(GameObject interactor)
        {
            return !isSmelting;
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

            // Open smelting UI
            if (UI.SmeltingUI.Instance != null)
            {
                UI.SmeltingUI.Instance.OpenSmeltingUI(this);
            }
        }

        /// <summary>
        /// Start smelting recipe
        /// </summary>
        public bool StartSmelting(SmeltingRecipe recipe)
        {
            if (recipe == null || isSmelting)
                return false;

            // Check if can craft
            string reason;
            if (!recipe.CanCraft(out reason))
            {
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.Show(reason, transform.position + Vector3.up, Color.red);
                }
                return false;
            }

            // Check fuel
            if (currentFuel < recipe.fuelCost)
            {
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.Show($"Need {recipe.fuelCost} fuel!", transform.position + Vector3.up, Color.red);
                }
                return false;
            }

            // Consume inputs
            InventorySystem.Instance.RemoveItem(recipe.inputItem.itemID, recipe.inputQuantity);
            currentFuel -= recipe.fuelCost;

            // Start smelting
            currentRecipe = recipe;
            isSmelting = true;
            smeltingProgress = 0f;
            smeltingTimer = 0f;

            // Start visual effects
            if (smeltingParticles != null)
            {
                smeltingParticles.SetActive(true);
            }

            if (fireObject != null)
            {
                fireObject.SetActive(true);
            }

            // Trigger event
            OnSmeltingStarted?.Invoke(recipe);

            Debug.Log($"[SmeltingStation] Started smelting {recipe.recipeName}");

            return true;
        }

        /// <summary>
        /// Update smelting progress
        /// </summary>
        private void UpdateSmelting()
        {
            if (currentRecipe == null)
                return;

            smeltingTimer += Time.deltaTime;
            smeltingProgress = smeltingTimer / currentRecipe.smeltingTime;

            if (smeltingProgress >= 1f)
            {
                CompleteSmelting();
            }
        }

        /// <summary>
        /// Complete smelting
        /// </summary>
        private void CompleteSmelting()
        {
            if (currentRecipe == null)
                return;

            // Roll for success
            bool success = currentRecipe.RollForSuccess();

            if (success)
            {
                // Roll for critical
                bool isCritical = currentRecipe.RollForCritical();

                int outputQuantity = currentRecipe.outputQuantity;
                if (isCritical && currentRecipe.criticalBonusOutput > 0)
                {
                    outputQuantity += currentRecipe.criticalBonusOutput;
                }

                // Add output to inventory
                InventorySystem.Instance.AddItem(currentRecipe.outputItem.itemID, outputQuantity);

                // Grant experience
                if (PlayerStats.Instance != null && currentRecipe.experienceReward > 0)
                {
                    PlayerStats.Instance.GainExperience(currentRecipe.experienceReward);
                }

                // Show notification
                if (FloatingTextManager.Instance != null)
                {
                    string text = isCritical ? $"CRITICAL! +{outputQuantity}" : $"+{outputQuantity}";
                    Color color = isCritical ? Color.yellow : Color.white;
                    FloatingTextManager.Instance.ShowItemPickup(
                        currentRecipe.outputItem.itemName,
                        outputQuantity,
                        transform.position + Vector3.up,
                        isCritical
                    );
                }

                // Trigger event
                OnSmeltingCompleted?.Invoke(currentRecipe, outputQuantity);

                Debug.Log($"[SmeltingStation] Completed smelting: {outputQuantity}x {currentRecipe.outputItem.itemName}");
            }
            else
            {
                // Failed smelting
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.Show("Smelting failed!", transform.position + Vector3.up, Color.red);
                }

                // Trigger event
                OnSmeltingFailed?.Invoke(currentRecipe);

                Debug.Log($"[SmeltingStation] Smelting failed!");
            }

            // Reset state
            isSmelting = false;
            currentRecipe = null;
            smeltingProgress = 0f;
            smeltingTimer = 0f;

            // Stop visual effects
            if (smeltingParticles != null)
            {
                smeltingParticles.SetActive(false);
            }

            if (currentFuel <= 0 && fireObject != null)
            {
                fireObject.SetActive(false);
            }
        }

        /// <summary>
        /// Add fuel to station
        /// </summary>
        public bool AddFuel(int amount)
        {
            if (fuelItem == null)
                return false;

            // Check if player has fuel
            int playerAmount = InventorySystem.Instance.GetItemCount(fuelItem.itemID);
            if (playerAmount < amount)
            {
                amount = playerAmount;
            }

            if (amount <= 0)
                return false;

            // Check capacity
            int spaceAvailable = maxFuelCapacity - currentFuel;
            amount = Mathf.Min(amount, spaceAvailable);

            if (amount <= 0)
            {
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.Show("Fuel is full!", transform.position + Vector3.up, Color.yellow);
                }
                return false;
            }

            // Consume fuel from inventory
            InventorySystem.Instance.RemoveItem(fuelItem.itemID, amount);
            currentFuel += amount;

            // Show notification
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.Show($"+{amount} fuel", transform.position + Vector3.up, Color.green);
            }

            // Enable fire if has fuel
            if (currentFuel > 0 && fireObject != null)
            {
                fireObject.SetActive(true);
            }

            return true;
        }

        /// <summary>
        /// Get available recipes
        /// </summary>
        public List<SmeltingRecipe> GetAvailableRecipes()
        {
            List<SmeltingRecipe> available = new List<SmeltingRecipe>();

            if (recipes == null)
                return available;

            foreach (var recipe in recipes)
            {
                string reason;
                if (recipe.CanCraft(out reason))
                {
                    available.Add(recipe);
                }
            }

            return available;
        }

        /// <summary>
        /// Get fuel percentage
        /// </summary>
        public float GetFuelPercent()
        {
            return maxFuelCapacity > 0 ? (float)currentFuel / maxFuelCapacity : 0f;
        }

        /// <summary>
        /// Cancel current smelting
        /// </summary>
        public void CancelSmelting()
        {
            if (!isSmelting || currentRecipe == null)
                return;

            // Refund partial resources (50%)
            int refundAmount = Mathf.CeilToInt(currentRecipe.inputQuantity * 0.5f);
            if (refundAmount > 0)
            {
                InventorySystem.Instance.AddItem(currentRecipe.inputItem.itemID, refundAmount);
            }

            // Refund fuel
            currentFuel += currentRecipe.fuelCost;

            // Reset state
            isSmelting = false;
            currentRecipe = null;
            smeltingProgress = 0f;
            smeltingTimer = 0f;

            // Stop visual effects
            if (smeltingParticles != null)
            {
                smeltingParticles.SetActive(false);
            }

            Debug.Log($"[SmeltingStation] Smelting cancelled");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.5f);

            #if UNITY_EDITOR
            string label = $"{stationName}\nFuel: {currentFuel}/{maxFuelCapacity}";
            if (isSmelting && currentRecipe != null)
            {
                label += $"\nSmelting: {currentRecipe.recipeName}\n{smeltingProgress * 100f:F0}%";
            }
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, label);
            #endif
        }
    }
}
