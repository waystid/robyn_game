using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace CozyGame.Inventory
{
    /// <summary>
    /// Crafting save data
    /// </summary>
    [System.Serializable]
    public class CraftingSaveData
    {
        public List<string> unlockedRecipeIDs = new List<string>();
        public int totalItemsCrafted = 0;
        public Dictionary<string, int> itemsCraftedByType = new Dictionary<string, int>();
    }

    /// <summary>
    /// Crafting manager singleton.
    /// Manages recipe unlocks, crafting stations, and success/failure mechanics.
    /// </summary>
    public class CraftingManager : MonoBehaviour
    {
        public static CraftingManager Instance { get; private set; }

        [Header("Recipe Database")]
        [Tooltip("All available recipes")]
        public CraftingRecipe[] allRecipes;

        [Tooltip("Basic recipes (available without stations)")]
        public CraftingRecipe[] basicRecipes;

        [Header("Success/Failure")]
        [Tooltip("Enable crafting success chance")]
        public bool enableSuccessChance = true;

        [Tooltip("Base success chance (0-1)")]
        [Range(0f, 1f)]
        public float baseSuccessChance = 0.9f;

        [Tooltip("Skill affects success chance")]
        public bool skillAffectsSuccess = true;

        [Tooltip("Player level bonus per level")]
        [Range(0f, 0.05f)]
        public float levelBonusPerLevel = 0.01f;

        [Header("Bonuses")]
        [Tooltip("Crafting station bonus")]
        [Range(0f, 0.3f)]
        public float stationBonus = 0.1f;

        [Tooltip("High quality ingredients bonus")]
        [Range(0f, 0.2f)]
        public float qualityBonus = 0.1f;

        [Header("Events")]
        public UnityEvent<CraftingRecipe> OnRecipeUnlocked;
        public UnityEvent<CraftingRecipe, bool> OnCraftingAttempt; // recipe, success
        public UnityEvent<CraftingRecipe> OnCraftingSuccess;
        public UnityEvent<CraftingRecipe> OnCraftingFailure;

        // State
        private HashSet<string> unlockedRecipeIDs = new HashSet<string>();
        private Dictionary<string, int> itemsCraftedCount = new Dictionary<string, int>();
        private int totalCrafted = 0;
        private CraftingStation currentStation;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initialize crafting manager
        /// </summary>
        private void Initialize()
        {
            // Unlock default recipes
            if (allRecipes != null)
            {
                foreach (var recipe in allRecipes)
                {
                    if (recipe != null && recipe.isDiscoveredByDefault)
                    {
                        UnlockRecipe(recipe.recipeID, silent: true);
                    }
                }
            }

            if (basicRecipes != null)
            {
                foreach (var recipe in basicRecipes)
                {
                    if (recipe != null && recipe.isDiscoveredByDefault)
                    {
                        UnlockRecipe(recipe.recipeID, silent: true);
                    }
                }
            }

            Debug.Log("[CraftingManager] Initialized");
        }

        /// <summary>
        /// Unlock a recipe
        /// </summary>
        public bool UnlockRecipe(string recipeID, bool silent = false)
        {
            if (string.IsNullOrEmpty(recipeID))
                return false;

            if (unlockedRecipeIDs.Contains(recipeID))
            {
                if (!silent)
                {
                    Debug.Log($"[CraftingManager] Recipe already unlocked: {recipeID}");
                }
                return false;
            }

            unlockedRecipeIDs.Add(recipeID);

            // Find recipe
            CraftingRecipe recipe = GetRecipeByID(recipeID);

            if (!silent)
            {
                Debug.Log($"[CraftingManager] Unlocked recipe: {recipe != null ? recipe.recipeName : recipeID}");

                // Show notification
                if (FloatingTextManager.Instance != null && recipe != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Recipe Unlocked: {recipe.recipeName}!",
                        Vector3.zero,
                        Color.yellow
                    );
                }

                // Trigger event
                if (recipe != null)
                {
                    OnRecipeUnlocked?.Invoke(recipe);
                }

                // Track achievement
                if (Achievements.StatisticsTracker.Instance != null)
                {
                    Achievements.StatisticsTracker.Instance.IncrementStatistic("recipes_unlocked", 1f);
                }
            }

            return true;
        }

        /// <summary>
        /// Check if recipe is unlocked
        /// </summary>
        public bool IsRecipeUnlocked(string recipeID)
        {
            return unlockedRecipeIDs.Contains(recipeID);
        }

        /// <summary>
        /// Get recipe by ID
        /// </summary>
        public CraftingRecipe GetRecipeByID(string recipeID)
        {
            if (allRecipes != null)
            {
                foreach (var recipe in allRecipes)
                {
                    if (recipe != null && recipe.recipeID == recipeID)
                    {
                        return recipe;
                    }
                }
            }

            if (basicRecipes != null)
            {
                foreach (var recipe in basicRecipes)
                {
                    if (recipe != null && recipe.recipeID == recipeID)
                    {
                        return recipe;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get all unlocked recipes
        /// </summary>
        public List<CraftingRecipe> GetUnlockedRecipes()
        {
            List<CraftingRecipe> recipes = new List<CraftingRecipe>();

            foreach (string recipeID in unlockedRecipeIDs)
            {
                CraftingRecipe recipe = GetRecipeByID(recipeID);
                if (recipe != null)
                {
                    recipes.Add(recipe);
                }
            }

            return recipes;
        }

        /// <summary>
        /// Get basic recipes
        /// </summary>
        public List<CraftingRecipe> GetBasicRecipes()
        {
            List<CraftingRecipe> recipes = new List<CraftingRecipe>();

            if (basicRecipes != null)
            {
                foreach (var recipe in basicRecipes)
                {
                    if (recipe != null && IsRecipeUnlocked(recipe.recipeID))
                    {
                        recipes.Add(recipe);
                    }
                }
            }

            return recipes;
        }

        /// <summary>
        /// Attempt to craft recipe with success/failure chance
        /// </summary>
        public bool AttemptCraft(CraftingRecipe recipe)
        {
            if (recipe == null)
                return false;

            // Check if can craft
            string reason;
            if (!recipe.CanCraft(out reason))
            {
                Debug.Log($"[CraftingManager] Cannot craft: {reason}");
                return false;
            }

            // Calculate success chance
            float successChance = CalculateSuccessChance(recipe);

            // Roll for success
            bool success = !enableSuccessChance || Random.value <= successChance;

            // Trigger attempt event
            OnCraftingAttempt?.Invoke(recipe, success);

            if (success)
            {
                // Craft the item
                if (InventorySystem.Instance != null)
                {
                    // Remove ingredients
                    foreach (var ingredient in recipe.ingredients)
                    {
                        InventorySystem.Instance.RemoveItem(ingredient.item.itemID, ingredient.quantity);
                    }

                    // Add result
                    InventorySystem.Instance.AddItem(recipe.resultItem.itemID, recipe.resultQuantity);
                }

                // Record crafting
                RecordCraft(recipe);

                // Notify station
                if (currentStation != null)
                {
                    currentStation.OnCrafted(recipe);
                }

                // Trigger success event
                OnCraftingSuccess?.Invoke(recipe);

                // Show VFX
                if (VFX.ParticleEffectManager.Instance != null)
                {
                    Vector3 pos = currentStation != null ? currentStation.transform.position : Vector3.zero;
                    VFX.ParticleEffectManager.Instance.SpawnEffect(VFX.EffectType.Sparkle, pos);
                }

                Debug.Log($"[CraftingManager] Successfully crafted {recipe.recipeName}!");
                return true;
            }
            else
            {
                // Crafting failed - only consume half of ingredients
                if (InventorySystem.Instance != null)
                {
                    foreach (var ingredient in recipe.ingredients)
                    {
                        int lostAmount = Mathf.CeilToInt(ingredient.quantity * 0.5f);
                        InventorySystem.Instance.RemoveItem(ingredient.item.itemID, lostAmount);
                    }
                }

                // Trigger failure event
                OnCraftingFailure?.Invoke(recipe);

                // Show feedback
                if (FloatingTextManager.Instance != null)
                {
                    Vector3 pos = currentStation != null ? currentStation.transform.position : Vector3.zero;
                    FloatingTextManager.Instance.Show(
                        "Crafting Failed!",
                        pos,
                        Color.red
                    );
                }

                Debug.Log($"[CraftingManager] Crafting failed for {recipe.recipeName}!");
                return false;
            }
        }

        /// <summary>
        /// Calculate success chance for recipe
        /// </summary>
        private float CalculateSuccessChance(CraftingRecipe recipe)
        {
            float chance = baseSuccessChance;

            // Skill/level bonus
            if (skillAffectsSuccess && PlayerStats.Instance != null)
            {
                chance += PlayerStats.Instance.level * levelBonusPerLevel;
            }

            // Station bonus
            if (currentStation != null)
            {
                chance += stationBonus;
            }

            // Recipe difficulty
            if (recipe.requiredLevel > 1 && PlayerStats.Instance != null)
            {
                int levelDifference = recipe.requiredLevel - PlayerStats.Instance.level;
                if (levelDifference > 0)
                {
                    chance -= levelDifference * 0.05f; // -5% per level above player
                }
            }

            // Clamp between 0 and 1
            return Mathf.Clamp01(chance);
        }

        /// <summary>
        /// Record successful craft
        /// </summary>
        private void RecordCraft(CraftingRecipe recipe)
        {
            totalCrafted++;

            string itemID = recipe.resultItem.itemID;
            if (itemsCraftedCount.ContainsKey(itemID))
            {
                itemsCraftedCount[itemID]++;
            }
            else
            {
                itemsCraftedCount[itemID] = 1;
            }

            // Track statistics
            if (Achievements.StatisticsTracker.Instance != null)
            {
                Achievements.StatisticsTracker.Instance.IncrementStatistic("items_crafted", 1f);
                Achievements.StatisticsTracker.Instance.IncrementStatistic($"crafted_{itemID}", 1f);
            }
        }

        /// <summary>
        /// Set current crafting station
        /// </summary>
        public void SetCurrentStation(CraftingStation station)
        {
            currentStation = station;
        }

        /// <summary>
        /// Get current crafting station
        /// </summary>
        public CraftingStation GetCurrentStation()
        {
            return currentStation;
        }

        /// <summary>
        /// Get total items crafted
        /// </summary>
        public int GetTotalCrafted()
        {
            return totalCrafted;
        }

        /// <summary>
        /// Get craft count for item
        /// </summary>
        public int GetCraftCount(string itemID)
        {
            if (itemsCraftedCount.ContainsKey(itemID))
            {
                return itemsCraftedCount[itemID];
            }
            return 0;
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public CraftingSaveData GetSaveData()
        {
            CraftingSaveData data = new CraftingSaveData();
            data.unlockedRecipeIDs = new List<string>(unlockedRecipeIDs);
            data.totalItemsCrafted = totalCrafted;
            data.itemsCraftedByType = new Dictionary<string, int>(itemsCraftedCount);
            return data;
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(CraftingSaveData data)
        {
            if (data == null)
                return;

            unlockedRecipeIDs = new HashSet<string>(data.unlockedRecipeIDs);
            totalCrafted = data.totalItemsCrafted;
            itemsCraftedCount = new Dictionary<string, int>(data.itemsCraftedByType);

            Debug.Log($"[CraftingManager] Loaded {unlockedRecipeIDs.Count} unlocked recipes");
        }
    }
}
