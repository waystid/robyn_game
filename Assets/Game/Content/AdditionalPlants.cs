using UnityEngine;
using System.Collections.Generic;

namespace CozyGame.Content
{
    /// <summary>
    /// Additional plant content for the game.
    /// Contains 12 unique plant types with different growing conditions and yields.
    /// </summary>
    public static class AdditionalPlants
    {
        /// <summary>
        /// Get all additional plants
        /// </summary>
        public static List<PlantDefinition> GetAllPlants()
        {
            return new List<PlantDefinition>
            {
                // Magical Plants
                new PlantDefinition
                {
                    plantID = "plant_moonflower",
                    plantName = "Moonflower",
                    description = "A mystical flower that blooms only under moonlight. Its petals glow with a soft silver luminescence.",
                    plantType = PlantType.Magical,
                    rarity = PlantRarity.Rare,

                    // Growth
                    growthTime = 480f, // 8 hours
                    growthStages = 4,
                    requiresNightTime = true,
                    requiresSeason = Season.Spring,

                    // Yield
                    minYield = 2,
                    maxYield = 4,
                    harvestedItemID = "moonflower",
                    sellValue = 150,

                    // Requirements
                    requiredSoilQuality = 3,
                    waterNeed = WaterNeed.Medium,
                    sunlightNeed = SunlightNeed.Low,
                    temperatureMin = 10f,
                    temperatureMax = 25f,

                    // Special
                    canRegrow = true,
                    regrowthTime = 240f,
                    magicInfused = true,
                    glowsAtNight = true
                },

                new PlantDefinition
                {
                    plantID = "plant_starbloom",
                    plantName = "Starbloom",
                    description = "Rare celestial flowers that absorb starlight. Used in powerful light magic spells.",
                    plantType = PlantType.Magical,
                    rarity = PlantRarity.Epic,

                    growthTime = 720f, // 12 hours
                    growthStages = 5,
                    requiresNightTime = true,
                    requiresSeason = Season.Summer,

                    minYield = 1,
                    maxYield = 3,
                    harvestedItemID = "starbloom",
                    sellValue = 300,

                    requiredSoilQuality = 4,
                    waterNeed = WaterNeed.Low,
                    sunlightNeed = SunlightNeed.None,
                    temperatureMin = 15f,
                    temperatureMax = 30f,

                    canRegrow = false,
                    magicInfused = true,
                    glowsAtNight = true,
                    specialEffect = "Grants Light Affinity buff when consumed"
                },

                new PlantDefinition
                {
                    plantID = "plant_sunpetal",
                    plantName = "Sunpetal",
                    description = "Golden flowers that turn to follow the sun. Their petals store solar energy.",
                    plantType = PlantType.Magical,
                    rarity = PlantRarity.Rare,

                    growthTime = 360f, // 6 hours
                    growthStages = 4,
                    requiresDayTime = true,
                    requiresSeason = Season.Summer,

                    minYield = 3,
                    maxYield = 5,
                    harvestedItemID = "sunpetal",
                    sellValue = 120,

                    requiredSoilQuality = 3,
                    waterNeed = WaterNeed.Low,
                    sunlightNeed = SunlightNeed.High,
                    temperatureMin = 20f,
                    temperatureMax = 40f,

                    canRegrow = true,
                    regrowthTime = 180f,
                    magicInfused = true
                },

                // Herbs
                new PlantDefinition
                {
                    plantID = "plant_healroot",
                    plantName = "Healroot",
                    description = "A medicinal herb with potent healing properties. Essential for healing potions.",
                    plantType = PlantType.Herb,
                    rarity = PlantRarity.Common,

                    growthTime = 300f, // 5 hours
                    growthStages = 3,
                    requiresSeason = Season.All,

                    minYield = 2,
                    maxYield = 6,
                    harvestedItemID = "healroot",
                    sellValue = 50,

                    requiredSoilQuality = 2,
                    waterNeed = WaterNeed.Medium,
                    sunlightNeed = SunlightNeed.Medium,
                    temperatureMin = 5f,
                    temperatureMax = 30f,

                    canRegrow = true,
                    regrowthTime = 150f,
                    medicinal = true
                },

                new PlantDefinition
                {
                    plantID = "plant_dreamleaf",
                    plantName = "Dreamleaf",
                    description = "Silvery leaves that induce vivid dreams. Used in vision potions and sleep aids.",
                    plantType = PlantType.Herb,
                    rarity = PlantRarity.Uncommon,

                    growthTime = 420f, // 7 hours
                    growthStages = 4,
                    requiresNightTime = true,
                    requiresSeason = Season.Autumn,

                    minYield = 2,
                    maxYield = 4,
                    harvestedItemID = "dreamleaf",
                    sellValue = 80,

                    requiredSoilQuality = 2,
                    waterNeed = WaterNeed.High,
                    sunlightNeed = SunlightNeed.Low,
                    temperatureMin = 10f,
                    temperatureMax = 20f,

                    canRegrow = true,
                    regrowthTime = 210f,
                    medicinal = true,
                    magicInfused = true
                },

                // Vegetables
                new PlantDefinition
                {
                    plantID = "plant_crystal_carrot",
                    plantName = "Crystal Carrot",
                    description = "Translucent carrots that grow in mineral-rich soil. Highly nutritious and magically enhanced.",
                    plantType = PlantType.Vegetable,
                    rarity = PlantRarity.Uncommon,

                    growthTime = 480f, // 8 hours
                    growthStages = 4,
                    requiresSeason = Season.Winter,

                    minYield = 3,
                    maxYield = 7,
                    harvestedItemID = "crystal_carrot",
                    sellValue = 60,

                    requiredSoilQuality = 3,
                    waterNeed = WaterNeed.Medium,
                    sunlightNeed = SunlightNeed.Medium,
                    temperatureMin = -5f,
                    temperatureMax = 15f,

                    canRegrow = false,
                    edible = true,
                    healthRestore = 40,
                    magicInfused = true
                },

                new PlantDefinition
                {
                    plantID = "plant_golden_wheat",
                    plantName = "Golden Wheat",
                    description = "Premium wheat with golden grains. Makes the finest bread and pastries.",
                    plantType = PlantType.Grain,
                    rarity = PlantRarity.Common,

                    growthTime = 540f, // 9 hours
                    growthStages = 5,
                    requiresSeason = Season.Summer,

                    minYield = 5,
                    maxYield = 12,
                    harvestedItemID = "golden_wheat",
                    sellValue = 40,

                    requiredSoilQuality = 2,
                    waterNeed = WaterNeed.Low,
                    sunlightNeed = SunlightNeed.High,
                    temperatureMin = 15f,
                    temperatureMax = 35f,

                    canRegrow = false,
                    edible = true
                },

                // Fruits
                new PlantDefinition
                {
                    plantID = "plant_rainbow_berry",
                    plantName = "Rainbow Berry Bush",
                    description = "Magical berries that change color based on their ripeness. Each color has different properties.",
                    plantType = PlantType.Fruit,
                    rarity = PlantRarity.Rare,

                    growthTime = 600f, // 10 hours
                    growthStages = 5,
                    requiresSeason = Season.Spring,

                    minYield = 4,
                    maxYield = 10,
                    harvestedItemID = "rainbow_berry",
                    sellValue = 100,

                    requiredSoilQuality = 3,
                    waterNeed = WaterNeed.High,
                    sunlightNeed = SunlightNeed.Medium,
                    temperatureMin = 12f,
                    temperatureMax = 28f,

                    canRegrow = true,
                    regrowthTime = 300f,
                    edible = true,
                    healthRestore = 30,
                    manaRestore = 30,
                    magicInfused = true
                },

                new PlantDefinition
                {
                    plantID = "plant_frost_melon",
                    plantName = "Frost Melon",
                    description = "Ice-cold melons that grow in winter. Refreshing and grants cold resistance.",
                    plantType = PlantType.Fruit,
                    rarity = PlantRarity.Uncommon,

                    growthTime = 720f, // 12 hours
                    growthStages = 5,
                    requiresSeason = Season.Winter,

                    minYield = 1,
                    maxYield = 3,
                    harvestedItemID = "frost_melon",
                    sellValue = 90,

                    requiredSoilQuality = 3,
                    waterNeed = WaterNeed.High,
                    sunlightNeed = SunlightNeed.Low,
                    temperatureMin = -10f,
                    temperatureMax = 10f,

                    canRegrow = false,
                    edible = true,
                    healthRestore = 50,
                    specialEffect = "Grants Cold Resistance for 5 minutes"
                },

                // Trees
                new PlantDefinition
                {
                    plantID = "plant_mana_tree",
                    plantName = "Mana Tree Sapling",
                    description = "A young mana tree that produces crystallized mana fruits. Takes a long time to mature.",
                    plantType = PlantType.Tree,
                    rarity = PlantRarity.Epic,

                    growthTime = 1440f, // 24 hours
                    growthStages = 6,
                    requiresSeason = Season.All,

                    minYield = 2,
                    maxYield = 5,
                    harvestedItemID = "mana_crystal",
                    sellValue = 250,

                    requiredSoilQuality = 4,
                    waterNeed = WaterNeed.Medium,
                    sunlightNeed = SunlightNeed.Medium,
                    temperatureMin = 10f,
                    temperatureMax = 30f,

                    canRegrow = true,
                    regrowthTime = 720f,
                    magicInfused = true,
                    manaRestore = 100
                },

                // Flowers
                new PlantDefinition
                {
                    plantID = "plant_dragon_lily",
                    plantName = "Dragon Lily",
                    description = "Fiery red flowers that are hot to the touch. Used in fire resistance potions.",
                    plantType = PlantType.Flower,
                    rarity = PlantRarity.Rare,

                    growthTime = 540f, // 9 hours
                    growthStages = 4,
                    requiresSeason = Season.Summer,
                    preferredBiome = "volcano",

                    minYield = 2,
                    maxYield = 4,
                    harvestedItemID = "dragon_lily",
                    sellValue = 140,

                    requiredSoilQuality = 3,
                    waterNeed = WaterNeed.Low,
                    sunlightNeed = SunlightNeed.High,
                    temperatureMin = 25f,
                    temperatureMax = 50f,

                    canRegrow = true,
                    regrowthTime = 270f,
                    magicInfused = true,
                    specialEffect = "Grants Fire Resistance for 5 minutes"
                },

                new PlantDefinition
                {
                    plantID = "plant_phoenix_rose",
                    plantName = "Phoenix Rose",
                    description = "Legendary flowers that burst into flames when fully bloomed, only to regrow from the ash.",
                    plantType = PlantType.Flower,
                    rarity = PlantRarity.Legendary,

                    growthTime = 1080f, // 18 hours
                    growthStages = 5,
                    requiresSeason = Season.Summer,

                    minYield = 1,
                    maxYield = 2,
                    harvestedItemID = "phoenix_rose",
                    sellValue = 500,

                    requiredSoilQuality = 5,
                    waterNeed = WaterNeed.Low,
                    sunlightNeed = SunlightNeed.High,
                    temperatureMin = 30f,
                    temperatureMax = 60f,

                    canRegrow = true,
                    regrowthTime = 540f,
                    magicInfused = true,
                    specialEffect = "Can be used to craft resurrection items"
                }
            };
        }

        /// <summary>
        /// Plant type
        /// </summary>
        public enum PlantType
        {
            Vegetable,
            Fruit,
            Flower,
            Herb,
            Grain,
            Tree,
            Magical
        }

        /// <summary>
        /// Plant rarity
        /// </summary>
        public enum PlantRarity
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
        }

        /// <summary>
        /// Season
        /// </summary>
        public enum Season
        {
            Spring,
            Summer,
            Autumn,
            Winter,
            All
        }

        /// <summary>
        /// Water need
        /// </summary>
        public enum WaterNeed
        {
            None,
            Low,
            Medium,
            High
        }

        /// <summary>
        /// Sunlight need
        /// </summary>
        public enum SunlightNeed
        {
            None,
            Low,
            Medium,
            High
        }

        /// <summary>
        /// Plant definition
        /// </summary>
        [System.Serializable]
        public class PlantDefinition
        {
            public string plantID;
            public string plantName;
            public string description;
            public PlantType plantType;
            public PlantRarity rarity;

            // Growth
            public float growthTime; // in seconds
            public int growthStages = 4;
            public bool requiresDayTime;
            public bool requiresNightTime;
            public Season requiresSeason = Season.All;
            public string preferredBiome;

            // Yield
            public int minYield = 1;
            public int maxYield = 3;
            public string harvestedItemID;
            public int sellValue;

            // Requirements
            public int requiredSoilQuality = 1; // 1-5
            public WaterNeed waterNeed = WaterNeed.Medium;
            public SunlightNeed sunlightNeed = SunlightNeed.Medium;
            public float temperatureMin = 0f;
            public float temperatureMax = 40f;

            // Special Properties
            public bool canRegrow;
            public float regrowthTime;
            public bool magicInfused;
            public bool glowsAtNight;
            public bool medicinal;
            public bool edible;
            public int healthRestore;
            public int manaRestore;
            public string specialEffect;
        }
    }
}
