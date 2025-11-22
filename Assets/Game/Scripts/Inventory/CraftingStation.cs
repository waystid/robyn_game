using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace CozyGame.Inventory
{
    /// <summary>
    /// Crafting station types
    /// </summary>
    public enum CraftingStationType
    {
        Workbench,    // Basic crafting
        Cauldron,     // Alchemy/potions
        Forge,        // Metal working
        Loom,         // Textiles
        CookingPot,   // Food preparation
        Anvil,        // Equipment upgrades
        EnchantingTable, // Enchantments
        Custom        // Custom station type
    }

    /// <summary>
    /// Crafting station component.
    /// Interactable station for crafting items (workbench, cauldron, forge, etc.).
    /// Provides access to station-specific recipes.
    /// </summary>
    public class CraftingStation : MonoBehaviour, Interaction.IInteractable
    {
        [Header("Station Info")]
        [Tooltip("Station name")]
        public string stationName = "Crafting Station";

        [Tooltip("Station type")]
        public CraftingStationType stationType = CraftingStationType.Workbench;

        [Tooltip("Custom station type (if using Custom enum)")]
        public string customStationType = "";

        [Header("Recipes")]
        [Tooltip("Station-specific recipes")]
        public CraftingRecipe[] stationRecipes;

        [Tooltip("Allow all basic recipes")]
        public bool allowBasicRecipes = true;

        [Header("Requirements")]
        [Tooltip("Required level to use")]
        public int requiredLevel = 1;

        [Tooltip("Requires tool/item to use")]
        public bool requiresTool = false;

        [Tooltip("Required tool item ID")]
        public string requiredToolID = "";

        [Header("Interaction")]
        [Tooltip("Interaction prompt")]
        public string interactionPrompt = "Use Crafting Station";

        [Tooltip("Interaction range")]
        public float interactionRange = 2f;

        [Header("Visual")]
        [Tooltip("Crafting effect when in use")]
        public GameObject craftingEffectPrefab;

        [Tooltip("Active indicator (enabled when player is using)")]
        public GameObject activeIndicator;

        [Header("Audio")]
        [Tooltip("Ambient sound when in use")]
        public string ambientSoundName = "crafting_ambient";

        [Header("Events")]
        public UnityEvent OnStationUsed;
        public UnityEvent<CraftingRecipe> OnItemCrafted;
        public UnityEvent OnStationClosed;

        private bool isInUse = false;
        private GameObject effectInstance;

        private void Start()
        {
            if (activeIndicator != null)
            {
                activeIndicator.SetActive(false);
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

            // Check level requirement
            if (PlayerStats.Instance != null && PlayerStats.Instance.level < requiredLevel)
            {
                return false;
            }

            // Check tool requirement
            if (requiresTool && !string.IsNullOrEmpty(requiredToolID))
            {
                if (InventorySystem.Instance == null)
                    return false;

                if (InventorySystem.Instance.GetItemCount(requiredToolID) == 0)
                    return false;
            }

            return true;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
            {
                // Show message
                if (PlayerStats.Instance != null && PlayerStats.Instance.level < requiredLevel)
                {
                    if (FloatingTextManager.Instance != null)
                    {
                        FloatingTextManager.Instance.Show(
                            $"Requires Level {requiredLevel}",
                            transform.position,
                            Color.red
                        );
                    }
                }
                else if (requiresTool)
                {
                    if (FloatingTextManager.Instance != null)
                    {
                        FloatingTextManager.Instance.Show(
                            "Missing required tool",
                            transform.position,
                            Color.red
                        );
                    }
                }
                return;
            }

            OpenCraftingUI();
        }

        /// <summary>
        /// Open crafting UI for this station
        /// </summary>
        public void OpenCraftingUI()
        {
            if (CraftingManager.Instance == null)
            {
                Debug.LogWarning("[CraftingStation] CraftingManager not found!");
                return;
            }

            // Set current station
            CraftingManager.Instance.SetCurrentStation(this);

            // Open UI
            if (UI.CraftingUI.Instance != null)
            {
                UI.CraftingUI.Instance.Show();
            }
            else
            {
                // Fallback: find in scene
                UI.CraftingUI craftingUI = FindObjectOfType<UI.CraftingUI>();
                if (craftingUI != null)
                {
                    craftingUI.Show();
                }
            }

            // Mark as in use
            isInUse = true;

            if (activeIndicator != null)
            {
                activeIndicator.SetActive(true);
            }

            // Spawn effect
            if (craftingEffectPrefab != null && effectInstance == null)
            {
                effectInstance = Instantiate(craftingEffectPrefab, transform);
            }

            // Play sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(ambientSoundName))
            {
                AudioManager.Instance.PlayLoopingSoundAtPosition(ambientSoundName, transform.position);
            }

            OnStationUsed?.Invoke();

            Debug.Log($"[CraftingStation] Opened {stationName}");
        }

        /// <summary>
        /// Close crafting UI
        /// </summary>
        public void CloseCraftingUI()
        {
            isInUse = false;

            if (activeIndicator != null)
            {
                activeIndicator.SetActive(false);
            }

            // Destroy effect
            if (effectInstance != null)
            {
                Destroy(effectInstance);
                effectInstance = null;
            }

            // Stop sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(ambientSoundName))
            {
                AudioManager.Instance.StopSound(ambientSoundName);
            }

            OnStationClosed?.Invoke();

            Debug.Log($"[CraftingStation] Closed {stationName}");
        }

        /// <summary>
        /// On item crafted at this station
        /// </summary>
        public void OnCrafted(CraftingRecipe recipe)
        {
            OnItemCrafted?.Invoke(recipe);

            // Spawn VFX
            if (VFX.ParticleEffectManager.Instance != null)
            {
                VFX.ParticleEffectManager.Instance.SpawnEffect(VFX.EffectType.Sparkle, transform.position);
            }

            Debug.Log($"[CraftingStation] Crafted {recipe.recipeName} at {stationName}");
        }

        /// <summary>
        /// Get station type string
        /// </summary>
        public string GetStationTypeString()
        {
            if (stationType == CraftingStationType.Custom)
            {
                return customStationType;
            }

            return stationType.ToString();
        }

        /// <summary>
        /// Get all available recipes for this station
        /// </summary>
        public List<CraftingRecipe> GetAvailableRecipes()
        {
            List<CraftingRecipe> recipes = new List<CraftingRecipe>();

            // Add station-specific recipes
            if (stationRecipes != null)
            {
                recipes.AddRange(stationRecipes);
            }

            // Add basic recipes if allowed
            if (allowBasicRecipes && CraftingManager.Instance != null)
            {
                recipes.AddRange(CraftingManager.Instance.GetBasicRecipes());
            }

            // Filter by station requirement
            List<CraftingRecipe> filteredRecipes = new List<CraftingRecipe>();
            string stationTypeStr = GetStationTypeString();

            foreach (var recipe in recipes)
            {
                // If recipe doesn't require a station, or requires this station type
                if (!recipe.requiresCraftingStation ||
                    string.IsNullOrEmpty(recipe.requiredStationType) ||
                    recipe.requiredStationType.Equals(stationTypeStr, System.StringComparison.OrdinalIgnoreCase))
                {
                    filteredRecipes.Add(recipe);
                }
            }

            return filteredRecipes;
        }

        /// <summary>
        /// Check if station is currently in use
        /// </summary>
        public bool IsInUse()
        {
            return isInUse;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);

            #if UNITY_EDITOR
            // Draw label
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, stationName);
            #endif
        }
    }
}
