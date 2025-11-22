using UnityEngine;
using System.Collections.Generic;

namespace CozyGame.Plants
{
    /// <summary>
    /// Defines a growable magical plant
    /// Create instances via: Right-click → Create → Cozy Game → Plant Data
    /// </summary>
    [CreateAssetMenu(fileName = "New Plant", menuName = "Cozy Game/Plants/Plant Data", order = 20)]
    public class PlantData : ScriptableObject
    {
        [Header("Plant Identity")]
        [Tooltip("Display name of the plant")]
        public string plantName = "Magical Plant";

        [Tooltip("Unique identifier")]
        public string plantID;

        [Tooltip("Description")]
        [TextArea(2, 3)]
        public string description = "A magical plant that grows...";

        [Header("Growth Stages")]
        [Tooltip("Prefabs for each growth stage (seed, sprout, growing, mature)")]
        public List<GameObject> stagePrefabs = new List<GameObject>(4);

        [Tooltip("Time in seconds for each stage")]
        public List<float> stageGrowthTimes = new List<float> { 60f, 120f, 180f, 240f };

        [Tooltip("Total number of growth stages")]
        public int totalStages = 4;

        [Header("Harvest")]
        [Tooltip("What item is harvested")]
        public string harvestItemID;

        [Tooltip("How many items per harvest")]
        public int harvestYieldMin = 1;
        public int harvestYieldMax = 3;

        [Tooltip("Can plant be harvested multiple times?")]
        public bool isRenewable = false;

        [Tooltip("If renewable, time to regrow after harvest")]
        public float regrowTime = 300f;

        [Header("Requirements")]
        [Tooltip("Does this plant need watering?")]
        public bool needsWater = true;

        [Tooltip("Water needed per growth stage")]
        public int waterPerStage = 1;

        [Tooltip("Does this plant need specific soil/plot?")]
        public bool needsGardenPlot = true;

        [Header("Special Properties")]
        [Tooltip("Is this a magical plant?")]
        public bool isMagical = false;

        [Tooltip("Does this plant glow at night?")]
        public bool glowsAtNight = false;

        [Tooltip("Particle effect while growing")]
        public GameObject growthParticlePrefab;

        [Tooltip("Particle effect when harvestable")]
        public GameObject readyParticlePrefab;

        [Header("Visual & Audio")]
        [Tooltip("Color tint for this plant")]
        public Color plantColor = Color.green;

        [Tooltip("Sound when watering")]
        public string waterSoundName = "plant_water";

        [Tooltip("Sound when harvesting")]
        public string harvestSoundName = "plant_harvest";

        [Tooltip("Sound when growing to next stage")]
        public string growSoundName = "plant_grow";

        [Header("Value")]
        [Tooltip("Rarity of this plant")]
        public PlantRarity rarity = PlantRarity.Common;

        [Tooltip("Experience gained when harvesting")]
        public int harvestExperience = 10;

        private void OnEnable()
        {
            // Generate unique ID if empty
            if (string.IsNullOrEmpty(plantID))
            {
                plantID = "plant_" + name.ToLower().Replace(" ", "_");
            }

            // Ensure stage lists are correct size
            ValidateStages();
        }

        private void OnValidate()
        {
            ValidateStages();
        }

        private void ValidateStages()
        {
            // Ensure we have the right number of growth times
            while (stageGrowthTimes.Count < totalStages)
            {
                stageGrowthTimes.Add(60f);
            }

            while (stageGrowthTimes.Count > totalStages)
            {
                stageGrowthTimes.RemoveAt(stageGrowthTimes.Count - 1);
            }

            // Warn if prefabs don't match
            if (stagePrefabs.Count != totalStages)
            {
                Debug.LogWarning($"Plant '{plantName}' has {stagePrefabs.Count} prefabs but {totalStages} stages!");
            }
        }

        /// <summary>
        /// Get total growth time from seed to harvest
        /// </summary>
        public float GetTotalGrowthTime()
        {
            float total = 0f;
            foreach (float time in stageGrowthTimes)
            {
                total += time;
            }
            return total;
        }

        /// <summary>
        /// Get prefab for specific growth stage
        /// </summary>
        public GameObject GetStagePrefab(int stage)
        {
            if (stage >= 0 && stage < stagePrefabs.Count)
            {
                return stagePrefabs[stage];
            }
            return null;
        }

        /// <summary>
        /// Get growth time for specific stage
        /// </summary>
        public float GetStageGrowthTime(int stage)
        {
            if (stage >= 0 && stage < stageGrowthTimes.Count)
            {
                return stageGrowthTimes[stage];
            }
            return 60f; // Default 1 minute
        }

        /// <summary>
        /// Check if plant is at final/harvestable stage
        /// </summary>
        public bool IsHarvestableStage(int currentStage)
        {
            return currentStage >= totalStages - 1;
        }

        /// <summary>
        /// Get random harvest yield
        /// </summary>
        public int GetRandomHarvestYield()
        {
            return Random.Range(harvestYieldMin, harvestYieldMax + 1);
        }

        /// <summary>
        /// Get plant info for UI
        /// </summary>
        public string GetPlantInfo()
        {
            string info = $"{plantName}\n";
            info += $"{description}\n\n";
            info += $"Growth Time: {GetTotalGrowthTime() / 60f:F1} minutes\n";
            info += $"Harvest: {harvestYieldMin}-{harvestYieldMax} {harvestItemID}\n";

            if (isRenewable)
                info += "Renewable (harvests multiple times)\n";

            if (needsWater)
                info += $"Needs watering: {waterPerStage} water per stage\n";

            if (isMagical)
                info += "✨ Magical Plant\n";

            return info;
        }
    }

    /// <summary>
    /// Plant rarity levels
    /// </summary>
    public enum PlantRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}
