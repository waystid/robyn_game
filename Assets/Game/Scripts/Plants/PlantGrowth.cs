using UnityEngine;
using System.Collections;

namespace CozyGame.Plants
{
    /// <summary>
    /// Handles plant growth mechanics
    /// Attach to plant GameObject in the world
    /// Works standalone or with Survival Engine
    /// </summary>
    public class PlantGrowth : MonoBehaviour
    {
        [Header("Plant Configuration")]
        [Tooltip("Plant data defining this plant")]
        public PlantData plantData;

        [Header("Current State")]
        [Tooltip("Current growth stage (0 = seed, max = harvestable)")]
        public int currentStage = 0;

        [Tooltip("Time elapsed in current stage")]
        public float currentStageTime = 0f;

        [Tooltip("Is this plant currently growing?")]
        public bool isGrowing = true;

        [Tooltip("Times this plant has been watered")]
        public int timesWatered = 0;

        [Tooltip("Has this plant been harvested?")]
        public bool hasBeenHarvested = false;

        [Header("Growth Settings")]
        [Tooltip("Growth speed multiplier (2.0 = 2x faster)")]
        public float growthSpeedMultiplier = 1f;

        [Tooltip("Auto-grow without needing water?")]
        public bool autoGrow = false;

        [Header("Visual")]
        [Tooltip("Container for stage models")]
        public Transform modelContainer;

        private GameObject currentStageModel;
        private GameObject particleEffect;

        private void Start()
        {
            if (plantData == null)
            {
                Debug.LogError($"PlantGrowth on {gameObject.name} has no PlantData assigned!");
                enabled = false;
                return;
            }

            // Create model container if it doesn't exist
            if (modelContainer == null)
            {
                GameObject container = new GameObject("ModelContainer");
                container.transform.SetParent(transform);
                container.transform.localPosition = Vector3.zero;
                modelContainer = container.transform;
            }

            // Start at stage 0
            UpdateStageModel();

            // Auto-start growing if enabled
            if (autoGrow)
            {
                StartGrowing();
            }
        }

        private void Update()
        {
            if (!isGrowing || currentStage >= plantData.totalStages - 1)
                return;

            // Check if needs watering
            if (plantData.needsWater && !autoGrow)
            {
                // Check if watered enough for current stage
                if (timesWatered < (currentStage + 1) * plantData.waterPerStage)
                {
                    // Not watered enough, pause growth
                    return;
                }
            }

            // Grow over time
            currentStageTime += Time.deltaTime * growthSpeedMultiplier;

            // Check if ready to advance to next stage
            float timeForThisStage = plantData.GetStageGrowthTime(currentStage);
            if (currentStageTime >= timeForThisStage)
            {
                AdvanceToNextStage();
            }
        }

        /// <summary>
        /// Advance plant to next growth stage
        /// </summary>
        public void AdvanceToNextStage()
        {
            if (currentStage >= plantData.totalStages - 1)
            {
                Debug.Log($"{plantData.plantName} is fully grown!");
                OnReachedMaturity();
                return;
            }

            currentStage++;
            currentStageTime = 0f;

            Debug.Log($"{plantData.plantName} grew to stage {currentStage + 1}/{plantData.totalStages}");

            UpdateStageModel();
            PlayGrowthEffects();

            // Check if now harvestable
            if (plantData.IsHarvestableStage(currentStage))
            {
                OnReachedMaturity();
            }
        }

        /// <summary>
        /// Instantly grow plant to next stage (for magic spell)
        /// </summary>
        public void InstantGrow()
        {
            if (currentStage >= plantData.totalStages - 1)
            {
                Debug.Log("Plant is already fully grown!");
                return;
            }

            AdvanceToNextStage();
        }

        /// <summary>
        /// Instantly grow plant to final stage
        /// </summary>
        public void InstantGrowToMature()
        {
            while (currentStage < plantData.totalStages - 1)
            {
                AdvanceToNextStage();
            }
        }

        /// <summary>
        /// Water this plant
        /// </summary>
        public void Water()
        {
            if (!plantData.needsWater)
            {
                Debug.Log("This plant doesn't need water!");
                return;
            }

            if (currentStage >= plantData.totalStages - 1)
            {
                Debug.Log("Plant is fully grown, doesn't need water!");
                return;
            }

            timesWatered++;
            Debug.Log($"Watered {plantData.plantName}! ({timesWatered} times)");

            // Play water effects
            PlayWaterEffects();

            // Show floating text
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.Show("💧 Watered!", transform.position + Vector3.up, Color.cyan);
            }

            // Check if this watering allows growth to start/continue
            if (!isGrowing)
            {
                StartGrowing();
            }
        }

        /// <summary>
        /// Harvest this plant
        /// </summary>
        public int Harvest()
        {
            if (!plantData.IsHarvestableStage(currentStage))
            {
                Debug.Log("Plant is not ready to harvest yet!");
                return 0;
            }

            if (hasBeenHarvested && !plantData.isRenewable)
            {
                Debug.Log("Plant has already been harvested!");
                return 0;
            }

            int yield = plantData.GetRandomHarvestYield();

            Debug.Log($"Harvested {yield}x {plantData.harvestItemID} from {plantData.plantName}!");

            // Play harvest effects
            PlayHarvestEffects();

            // Show floating text
            if (FloatingTextManager.Instance != null)
            {
                string itemName = SurvivalEngine.SurvivalEngineHelper.GetItemName(plantData.harvestItemID);
                FloatingTextManager.Instance.ShowItemPickup(
                    itemName,
                    yield,
                    transform.position + Vector3.up,
                    plantData.rarity >= PlantRarity.Rare
                );
            }

            // Add items to inventory
            SurvivalEngine.SurvivalEngineHelper.AddItemToInventory(plantData.harvestItemID, yield);

            hasBeenHarvested = true;

            // Handle renewable plants
            if (plantData.isRenewable)
            {
                StartCoroutine(RegrowAfterHarvest());
            }
            else
            {
                // Non-renewable: destroy or reset
                StartCoroutine(DestroyAfterHarvest());
            }

            return yield;
        }

        private IEnumerator RegrowAfterHarvest()
        {
            // Go back to earlier stage
            currentStage = Mathf.Max(0, currentStage - 2);
            currentStageTime = 0f;
            UpdateStageModel();

            yield return new WaitForSeconds(plantData.regrowTime);

            // Continue growing
            isGrowing = true;
        }

        private IEnumerator DestroyAfterHarvest()
        {
            // Wait a moment then destroy
            yield return new WaitForSeconds(1f);

            // Fade out or destroy
            Destroy(gameObject);
        }

        /// <summary>
        /// Start/resume growth
        /// </summary>
        public void StartGrowing()
        {
            isGrowing = true;
            Debug.Log($"{plantData.plantName} started growing!");
        }

        /// <summary>
        /// Pause growth
        /// </summary>
        public void PauseGrowing()
        {
            isGrowing = false;
        }

        /// <summary>
        /// Update visual model for current stage
        /// </summary>
        private void UpdateStageModel()
        {
            // Remove old model
            if (currentStageModel != null)
            {
                Destroy(currentStageModel);
            }

            // Spawn new stage model
            GameObject prefab = plantData.GetStagePrefab(currentStage);
            if (prefab != null)
            {
                currentStageModel = Instantiate(prefab, modelContainer);
                currentStageModel.transform.localPosition = Vector3.zero;
                currentStageModel.transform.localRotation = Quaternion.identity;
            }
        }

        private void OnReachedMaturity()
        {
            isGrowing = false;
            Debug.Log($"{plantData.plantName} is now harvestable!");

            // Spawn ready particle effect
            if (plantData.readyParticlePrefab != null)
            {
                particleEffect = Instantiate(
                    plantData.readyParticlePrefab,
                    transform.position + Vector3.up * 0.5f,
                    Quaternion.identity,
                    transform
                );
            }

            // Play sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(plantData.growSoundName))
            {
                AudioManager.Instance.PlaySoundAtPosition(plantData.growSoundName, transform.position);
            }

            // Show notification
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.ShowCompletion(
                    $"{plantData.plantName} ready!",
                    transform.position + Vector3.up * 2f
                );
            }
        }

        private void PlayGrowthEffects()
        {
            // Spawn growth particle
            if (plantData.growthParticlePrefab != null)
            {
                GameObject particle = Instantiate(
                    plantData.growthParticlePrefab,
                    transform.position,
                    Quaternion.identity
                );
                Destroy(particle, 2f);
            }

            // Play sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(plantData.growSoundName))
            {
                AudioManager.Instance.PlaySoundAtPosition(plantData.growSoundName, transform.position);
            }
        }

        private void PlayWaterEffects()
        {
            // Play water sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(plantData.waterSoundName))
            {
                AudioManager.Instance.PlaySoundAtPosition(plantData.waterSoundName, transform.position);
            }

            // TODO: Spawn water particle effect
        }

        private void PlayHarvestEffects()
        {
            // Play harvest sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(plantData.harvestSoundName))
            {
                AudioManager.Instance.PlaySoundAtPosition(plantData.harvestSoundName, transform.position);
            }

            // TODO: Spawn harvest particle effect
        }

        /// <summary>
        /// Get growth progress as percentage (0-1)
        /// </summary>
        public float GetGrowthProgress()
        {
            if (plantData == null) return 0f;

            float totalTime = plantData.GetTotalGrowthTime();
            float elapsedTime = 0f;

            // Add completed stages
            for (int i = 0; i < currentStage; i++)
            {
                elapsedTime += plantData.GetStageGrowthTime(i);
            }

            // Add current stage progress
            elapsedTime += currentStageTime;

            return Mathf.Clamp01(elapsedTime / totalTime);
        }

        /// <summary>
        /// Get time remaining until mature
        /// </summary>
        public float GetTimeUntilMature()
        {
            if (plantData == null) return 0f;

            float remaining = 0f;

            // Add time remaining in current stage
            remaining += plantData.GetStageGrowthTime(currentStage) - currentStageTime;

            // Add time for future stages
            for (int i = currentStage + 1; i < plantData.totalStages; i++)
            {
                remaining += plantData.GetStageGrowthTime(i);
            }

            return remaining / growthSpeedMultiplier;
        }

        // Debug visualization
        private void OnDrawGizmosSelected()
        {
            if (plantData == null) return;

            // Show growth progress
            Gizmos.color = plantData.plantColor;
            Gizmos.DrawWireSphere(transform.position + Vector3.up, 0.5f);

            // Show stage
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 1.5f,
                $"{plantData.plantName}\nStage: {currentStage + 1}/{plantData.totalStages}\nProgress: {GetGrowthProgress() * 100f:F0}%"
            );
        }
    }
}
