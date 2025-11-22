using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CozyGame.Inventory;

namespace CozyGame.Building
{
    /// <summary>
    /// Placed building save data
    /// </summary>
    [System.Serializable]
    public class PlacedBuildingSaveData
    {
        public string buildingID;
        public Vector3 position;
        public Quaternion rotation;
        public int currentTier;
        public float buildProgress;
        public List<string> storedItemIDs = new List<string>();
        public List<int> storedItemQuantities = new List<int>();
        public float productionTimer;
    }

    /// <summary>
    /// Placed building component.
    /// Attached to instantiated buildings in the world.
    /// Handles functionality, upgrades, storage, and production.
    /// </summary>
    public class PlacedBuilding : MonoBehaviour
    {
        [Header("Building Data")]
        [Tooltip("Building definition")]
        public BuildingData buildingData;

        [Tooltip("Current upgrade tier (0 = base)")]
        public int currentTier = 0;

        [Header("Construction")]
        [Tooltip("Is currently being built")]
        public bool isUnderConstruction = true;

        [Tooltip("Build progress (0-1)")]
        [Range(0f, 1f)]
        public float buildProgress = 0f;

        [Tooltip("Auto-complete construction")]
        public bool autoCompleteBuild = true;

        [Header("Storage")]
        [Tooltip("Stored items (if has storage)")]
        public List<Item> storedItems = new List<Item>();

        [Tooltip("Stored item quantities")]
        public List<int> storedQuantities = new List<int>();

        [Header("Production")]
        [Tooltip("Production timer")]
        public float productionTimer = 0f;

        [Header("Visual")]
        [Tooltip("Current building model")]
        public GameObject currentModel;

        [Tooltip("Construction particle effect")]
        public GameObject constructionParticles;

        [Header("Events")]
        public UnityEvent OnConstructionComplete;
        public UnityEvent<int> OnUpgraded; // newTier
        public UnityEvent OnProduction; // Item produced
        public UnityEvent OnStorageFull;

        private void Start()
        {
            if (autoCompleteBuild && isUnderConstruction)
            {
                StartConstruction();
            }
        }

        private void Update()
        {
            // Update construction
            if (isUnderConstruction)
            {
                UpdateConstruction();
            }

            // Update production
            if (!isUnderConstruction && buildingData != null && buildingData.functionality.producesItems)
            {
                UpdateProduction();
            }
        }

        /// <summary>
        /// Initialize building
        /// </summary>
        public void Initialize()
        {
            if (buildingData == null)
            {
                Debug.LogError("[PlacedBuilding] No building data assigned!");
                return;
            }

            // Initialize storage
            if (buildingData.functionality.hasStorage)
            {
                int slots = buildingData.GetTotalStorageSlots(currentTier);
                storedItems.Clear();
                storedQuantities.Clear();

                for (int i = 0; i < slots; i++)
                {
                    storedItems.Add(null);
                    storedQuantities.Add(0);
                }
            }

            // Start construction if enabled
            if (autoCompleteBuild && isUnderConstruction)
            {
                StartConstruction();
            }

            Debug.Log($"[PlacedBuilding] Initialized {buildingData.buildingName}");
        }

        /// <summary>
        /// Start construction
        /// </summary>
        public void StartConstruction()
        {
            if (buildingData == null)
                return;

            isUnderConstruction = true;
            buildProgress = 0f;

            // Spawn construction particles
            if (buildingData.basePrefab != null)
            {
                // TODO: Spawn construction VFX
            }

            Debug.Log($"[PlacedBuilding] Started constructing {buildingData.buildingName}");
        }

        /// <summary>
        /// Update construction progress
        /// </summary>
        private void UpdateConstruction()
        {
            if (!isUnderConstruction || buildingData == null)
                return;

            float buildTime = currentTier == 0 ? buildingData.buildTime : buildingData.GetUpgradeTier(currentTier)?.buildTime ?? 5f;

            buildProgress += Time.deltaTime / buildTime;

            if (buildProgress >= 1f)
            {
                CompleteConstruction();
            }
        }

        /// <summary>
        /// Complete construction
        /// </summary>
        public void CompleteConstruction()
        {
            isUnderConstruction = false;
            buildProgress = 1f;

            // Destroy construction particles
            if (constructionParticles != null)
            {
                Destroy(constructionParticles);
            }

            // Spawn completion VFX
            if (VFX.ParticleEffectManager.Instance != null)
            {
                VFX.ParticleEffectManager.Instance.SpawnEffect(VFX.EffectType.Sparkle, transform.position);
            }

            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySoundAtPosition("building_complete", transform.position);
            }

            // Show notification
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.ShowCompletion($"{buildingData.buildingName} completed!", transform.position + Vector3.up * 2f);
            }

            OnConstructionComplete?.Invoke();

            Debug.Log($"[PlacedBuilding] {buildingData.buildingName} construction complete!");
        }

        /// <summary>
        /// Can this building be upgraded?
        /// </summary>
        public bool CanUpgrade()
        {
            if (buildingData == null || !buildingData.canUpgrade)
                return false;

            if (isUnderConstruction)
                return false;

            if (currentTier >= buildingData.GetMaxTier())
                return false;

            // Check resources
            BuildingUpgradeTier nextTier = buildingData.GetUpgradeTier(currentTier + 1);
            if (nextTier == null)
                return false;

            // Check resource costs
            if (nextTier.resourceCosts != null)
            {
                foreach (var cost in nextTier.resourceCosts)
                {
                    if (cost.item == null)
                        continue;

                    int playerAmount = InventorySystem.Instance.GetItemCount(cost.item.itemID);
                    if (playerAmount < cost.quantity)
                        return false;
                }
            }

            // Check currency costs
            if (nextTier.currencyCosts != null)
            {
                foreach (var cost in nextTier.currencyCosts)
                {
                    if (!Economy.CurrencyManager.Instance.HasCurrency(cost.currencyType, cost.amount))
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Upgrade building to next tier
        /// </summary>
        public bool Upgrade()
        {
            if (!CanUpgrade())
                return false;

            BuildingUpgradeTier nextTier = buildingData.GetUpgradeTier(currentTier + 1);
            if (nextTier == null)
                return false;

            // Consume resources
            if (nextTier.resourceCosts != null)
            {
                foreach (var cost in nextTier.resourceCosts)
                {
                    if (cost.item != null)
                    {
                        InventorySystem.Instance.RemoveItem(cost.item.itemID, cost.quantity);
                    }
                }
            }

            // Consume currency
            if (nextTier.currencyCosts != null)
            {
                foreach (var cost in nextTier.currencyCosts)
                {
                    Economy.CurrencyManager.Instance.RemoveCurrency(cost.currencyType, cost.amount);
                }
            }

            // Start upgrade construction
            currentTier++;
            isUnderConstruction = true;
            buildProgress = 0f;

            // Update visual
            UpdateVisualModel();

            // Trigger event
            OnUpgraded?.Invoke(currentTier);

            // Show notification
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.ShowCompletion($"Upgrading to {nextTier.tierName}...", transform.position + Vector3.up * 2f);
            }

            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySoundAtPosition("building_upgrade", transform.position);
            }

            Debug.Log($"[PlacedBuilding] Upgrading {buildingData.buildingName} to tier {currentTier}");

            return true;
        }

        /// <summary>
        /// Update visual model for current tier
        /// </summary>
        private void UpdateVisualModel()
        {
            if (buildingData == null)
                return;

            // Destroy current model
            if (currentModel != null)
            {
                Destroy(currentModel);
            }

            // Get prefab for tier
            GameObject prefab = buildingData.GetPrefabForTier(currentTier);
            if (prefab == null)
                return;

            // Instantiate new model
            currentModel = Instantiate(prefab, transform);
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Update production
        /// </summary>
        private void UpdateProduction()
        {
            if (buildingData == null || !buildingData.functionality.producesItems)
                return;

            if (buildingData.functionality.productionItem == null)
                return;

            float interval = buildingData.functionality.productionInterval;
            float speedMultiplier = buildingData.GetProductionSpeedMultiplier(currentTier);
            float adjustedInterval = interval / speedMultiplier;

            productionTimer += Time.deltaTime;

            if (productionTimer >= adjustedInterval)
            {
                productionTimer = 0f;
                ProduceItem();
            }
        }

        /// <summary>
        /// Produce item
        /// </summary>
        private void ProduceItem()
        {
            if (buildingData == null || buildingData.functionality.productionItem == null)
                return;

            int quantity = buildingData.functionality.productionQuantity;

            // Try to add to internal storage first
            if (buildingData.functionality.hasStorage)
            {
                if (AddToStorage(buildingData.functionality.productionItem, quantity))
                {
                    OnProduction?.Invoke();

                    // Show notification
                    if (FloatingTextManager.Instance != null)
                    {
                        FloatingTextManager.Instance.ShowItemPickup(
                            buildingData.functionality.productionItem.itemName,
                            quantity,
                            transform.position + Vector3.up,
                            false
                        );
                    }

                    Debug.Log($"[PlacedBuilding] Produced {quantity}x {buildingData.functionality.productionItem.itemName}");
                }
                else
                {
                    OnStorageFull?.Invoke();
                }
            }
            else
            {
                // No storage, add directly to player inventory
                InventorySystem.Instance.AddItem(buildingData.functionality.productionItem.itemID, quantity);
                OnProduction?.Invoke();
            }
        }

        /// <summary>
        /// Add item to storage
        /// </summary>
        public bool AddToStorage(Item item, int quantity)
        {
            if (!buildingData.functionality.hasStorage)
                return false;

            // Find empty slot or existing stack
            for (int i = 0; i < storedItems.Count; i++)
            {
                if (storedItems[i] == null)
                {
                    storedItems[i] = item;
                    storedQuantities[i] = quantity;
                    return true;
                }
                else if (storedItems[i] == item && item.isStackable)
                {
                    storedQuantities[i] += quantity;
                    return true;
                }
            }

            return false; // Storage full
        }

        /// <summary>
        /// Remove item from storage
        /// </summary>
        public bool RemoveFromStorage(Item item, int quantity)
        {
            if (!buildingData.functionality.hasStorage)
                return false;

            int remaining = quantity;

            for (int i = 0; i < storedItems.Count; i++)
            {
                if (storedItems[i] == item && storedQuantities[i] > 0)
                {
                    int removeAmount = Mathf.Min(remaining, storedQuantities[i]);
                    storedQuantities[i] -= removeAmount;
                    remaining -= removeAmount;

                    if (storedQuantities[i] <= 0)
                    {
                        storedItems[i] = null;
                        storedQuantities[i] = 0;
                    }

                    if (remaining <= 0)
                        return true;
                }
            }

            return remaining <= 0;
        }

        /// <summary>
        /// Get stored item count
        /// </summary>
        public int GetStoredItemCount(Item item)
        {
            if (!buildingData.functionality.hasStorage)
                return 0;

            int count = 0;
            for (int i = 0; i < storedItems.Count; i++)
            {
                if (storedItems[i] == item)
                {
                    count += storedQuantities[i];
                }
            }

            return count;
        }

        /// <summary>
        /// Get storage capacity
        /// </summary>
        public int GetStorageCapacity()
        {
            if (!buildingData.functionality.hasStorage)
                return 0;

            return buildingData.GetTotalStorageSlots(currentTier);
        }

        /// <summary>
        /// Is storage full?
        /// </summary>
        public bool IsStorageFull()
        {
            if (!buildingData.functionality.hasStorage)
                return false;

            for (int i = 0; i < storedItems.Count; i++)
            {
                if (storedItems[i] == null)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public PlacedBuildingSaveData GetSaveData()
        {
            PlacedBuildingSaveData data = new PlacedBuildingSaveData
            {
                buildingID = buildingData.buildingID,
                position = transform.position,
                rotation = transform.rotation,
                currentTier = currentTier,
                buildProgress = buildProgress,
                productionTimer = productionTimer
            };

            // Save storage
            if (buildingData.functionality.hasStorage)
            {
                for (int i = 0; i < storedItems.Count; i++)
                {
                    if (storedItems[i] != null)
                    {
                        data.storedItemIDs.Add(storedItems[i].itemID);
                        data.storedItemQuantities.Add(storedQuantities[i]);
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(PlacedBuildingSaveData data)
        {
            if (data == null)
                return;

            currentTier = data.currentTier;
            buildProgress = data.buildProgress;
            productionTimer = data.productionTimer;

            isUnderConstruction = buildProgress < 1f;

            // Load storage
            if (buildingData.functionality.hasStorage && data.storedItemIDs.Count > 0)
            {
                for (int i = 0; i < data.storedItemIDs.Count; i++)
                {
                    string itemID = data.storedItemIDs[i];
                    int quantity = data.storedItemQuantities[i];

                    // TODO: Load item by ID
                    // Item item = Resources.Load<Item>($"Items/{itemID}");
                    // if (item != null)
                    // {
                    //     AddToStorage(item, quantity);
                    // }
                }
            }

            // Update visual
            UpdateVisualModel();
        }

        private void OnDrawGizmosSelected()
        {
            if (buildingData == null)
                return;

            // Draw building bounds
            Gizmos.color = Color.cyan;
            Vector3 size = new Vector3(
                buildingData.gridSize.x * buildingData.gridCellSize,
                buildingData.buildingHeight,
                buildingData.gridSize.y * buildingData.gridCellSize
            );
            Gizmos.DrawWireCube(transform.position + Vector3.up * (size.y * 0.5f), size);

            #if UNITY_EDITOR
            // Draw label
            string label = $"{buildingData.buildingName}\nTier {currentTier}/{buildingData.GetMaxTier()}";
            if (isUnderConstruction)
            {
                label += $"\nBuilding: {buildProgress * 100f:F0}%";
            }
            UnityEditor.Handles.Label(transform.position + Vector3.up * size.y, label);
            #endif
        }
    }
}
