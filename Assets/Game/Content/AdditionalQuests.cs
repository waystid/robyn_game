using UnityEngine;
using System.Collections.Generic;

namespace CozyGame.Content
{
    /// <summary>
    /// Additional quest content for the game.
    /// Contains 15 diverse quests across different quest types.
    /// </summary>
    public static class AdditionalQuests
    {
        /// <summary>
        /// Get all additional quests
        /// </summary>
        public static List<QuestDefinition> GetAllQuests()
        {
            return new List<QuestDefinition>
            {
                // Main Story Quests
                new QuestDefinition
                {
                    questID = "main_ancient_library",
                    questName = "The Ancient Library",
                    description = "Mysterious energy readings have been detected from the old library ruins. Investigate the source and uncover the secrets within.",
                    questType = QuestType.Main,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Find the Ancient Library entrance", targetCount = 1 },
                        new QuestObjective { description = "Collect 3 Ancient Tomes", targetCount = 3, targetItemID = "ancient_tome" },
                        new QuestObjective { description = "Defeat the Guardian Spirit", targetCount = 1, targetEnemyID = "library_guardian" },
                        new QuestObjective { description = "Unlock the Forbidden Section", targetCount = 1 }
                    },
                    experienceReward = 500,
                    goldReward = 300,
                    unlockLoreEntry = "library_history"
                },

                new QuestDefinition
                {
                    questID = "main_elemental_balance",
                    questName = "Elemental Imbalance",
                    description = "The four elemental shrines have lost their balance. Restore harmony by completing rituals at each shrine.",
                    questType = QuestType.Main,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Visit the Fire Shrine", targetCount = 1, targetLocationID = "fire_shrine" },
                        new QuestObjective { description = "Visit the Water Shrine", targetCount = 1, targetLocationID = "water_shrine" },
                        new QuestObjective { description = "Visit the Earth Shrine", targetCount = 1, targetLocationID = "earth_shrine" },
                        new QuestObjective { description = "Visit the Air Shrine", targetCount = 1, targetLocationID = "air_shrine" },
                        new QuestObjective { description = "Perform the Unity Ritual", targetCount = 1 }
                    },
                    experienceReward = 600,
                    unlockSpellID = "elemental_harmony",
                    unlockLoreEntry = "elemental_shrines"
                },

                // Side Quests
                new QuestDefinition
                {
                    questID = "side_missing_cat",
                    questName = "The Missing Cat",
                    description = "Old Lady Beatrice's cat has gone missing. Help her find her beloved pet before nightfall.",
                    questType = QuestType.Side,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Search the Forest for clues", targetCount = 3, targetLocationID = "forest" },
                        new QuestObjective { description = "Follow the paw prints", targetCount = 1 },
                        new QuestObjective { description = "Rescue Whiskers from the tree", targetCount = 1 },
                        new QuestObjective { description = "Return Whiskers to Beatrice", targetCount = 1, targetNPCID = "beatrice" }
                    },
                    experienceReward = 150,
                    goldReward = 100,
                    relationshipBonus = new Dictionary<string, int> { { "beatrice", 20 } }
                },

                new QuestDefinition
                {
                    questID = "side_blacksmith_materials",
                    questName = "The Blacksmith's Request",
                    description = "Gareth the Blacksmith needs rare materials to forge a legendary weapon. Gather the components he requires.",
                    questType = QuestType.Side,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Collect 5 Mithril Ore", targetCount = 5, targetItemID = "mithril_ore" },
                        new QuestObjective { description = "Collect 3 Dragon Scales", targetCount = 3, targetItemID = "dragon_scale" },
                        new QuestObjective { description = "Collect 1 Phoenix Feather", targetCount = 1, targetItemID = "phoenix_feather" },
                        new QuestObjective { description = "Deliver materials to Gareth", targetCount = 1, targetNPCID = "gareth" }
                    },
                    experienceReward = 250,
                    itemRewards = new List<string> { "legendary_sword_blueprint" },
                    relationshipBonus = new Dictionary<string, int> { { "gareth", 25 } }
                },

                new QuestDefinition
                {
                    questID = "side_mysterious_merchant",
                    questName = "The Mysterious Merchant",
                    description = "A traveling merchant has exotic wares but needs help acquiring goods to trade. Complete his shopping list.",
                    questType = QuestType.Side,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Harvest 10 Golden Wheat", targetCount = 10, targetItemID = "golden_wheat" },
                        new QuestObjective { description = "Catch 5 Rainbow Trout", targetCount = 5, targetItemID = "rainbow_trout" },
                        new QuestObjective { description = "Craft 3 Healing Potions", targetCount = 3, targetItemID = "healing_potion" },
                        new QuestObjective { description = "Trade with the Merchant", targetCount = 1, targetNPCID = "mysterious_merchant" }
                    },
                    experienceReward = 200,
                    goldReward = 250,
                    itemRewards = new List<string> { "exotic_spice", "rare_dye" }
                },

                new QuestDefinition
                {
                    questID = "side_haunted_mill",
                    questName = "The Haunted Mill",
                    description = "Strange noises come from the old windmill at night. Investigate and put the restless spirits to peace.",
                    questType = QuestType.Side,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Visit the Old Mill at night", targetCount = 1, targetLocationID = "old_mill" },
                        new QuestObjective { description = "Collect 3 Spirit Essences", targetCount = 3, targetItemID = "spirit_essence" },
                        new QuestObjective { description = "Perform the Cleansing Ritual", targetCount = 1 },
                        new QuestObjective { description = "Report to the Village Elder", targetCount = 1, targetNPCID = "elder" }
                    },
                    experienceReward = 180,
                    goldReward = 150,
                    unlockLoreEntry = "mill_tragedy"
                },

                // Daily Quests
                new QuestDefinition
                {
                    questID = "daily_garden_bounty",
                    questName = "Garden Bounty",
                    description = "The village needs fresh produce. Harvest crops from your garden to supply the market.",
                    questType = QuestType.Daily,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Harvest 15 vegetables", targetCount = 15 },
                        new QuestObjective { description = "Harvest 10 fruits", targetCount = 10 },
                        new QuestObjective { description = "Deliver to Market Stall", targetCount = 1, targetLocationID = "market" }
                    },
                    experienceReward = 100,
                    goldReward = 75
                },

                new QuestDefinition
                {
                    questID = "daily_monster_hunter",
                    questName = "Monster Hunter",
                    description = "Clear out monsters that have been troubling travelers on the roads.",
                    questType = QuestType.Daily,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Defeat 10 Slimes", targetCount = 10, targetEnemyID = "slime" },
                        new QuestObjective { description = "Defeat 5 Wolves", targetCount = 5, targetEnemyID = "wolf" },
                        new QuestObjective { description = "Defeat 1 Elite Enemy", targetCount = 1 }
                    },
                    experienceReward = 120,
                    goldReward = 80,
                    itemRewards = new List<string> { "monster_token" }
                },

                new QuestDefinition
                {
                    questID = "daily_fishing_competition",
                    questName = "Fishing Competition",
                    description = "Compete in today's fishing contest. Catch the most valuable fish to win prizes!",
                    questType = QuestType.Daily,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Catch 20 fish", targetCount = 20 },
                        new QuestObjective { description = "Catch 1 Rare fish", targetCount = 1 },
                        new QuestObjective { description = "Submit your catch", targetCount = 1, targetNPCID = "fishing_master" }
                    },
                    experienceReward = 90,
                    goldReward = 100
                },

                // Collection Quests
                new QuestDefinition
                {
                    questID = "collection_herb_gathering",
                    questName = "Master Herbalist",
                    description = "The apothecary needs a variety of herbs for rare medicines. Gather samples of each type.",
                    questType = QuestType.Collection,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Collect Moonflower", targetCount = 5, targetItemID = "moonflower" },
                        new QuestObjective { description = "Collect Sunpetal", targetCount = 5, targetItemID = "sunpetal" },
                        new QuestObjective { description = "Collect Nightshade", targetCount = 5, targetItemID = "nightshade" },
                        new QuestObjective { description = "Collect Starbloom", targetCount = 5, targetItemID = "starbloom" },
                        new QuestObjective { description = "Deliver to Apothecary", targetCount = 1, targetNPCID = "apothecary" }
                    },
                    experienceReward = 200,
                    itemRewards = new List<string> { "master_herbalist_badge" },
                    unlockRecipeID = "elixir_of_vitality"
                },

                new QuestDefinition
                {
                    questID = "collection_gemstone_collector",
                    questName = "Gemstone Collector",
                    description = "A jeweler seeks rare gemstones for a royal commission. Find specimens of each type.",
                    questType = QuestType.Collection,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Find 3 Rubies", targetCount = 3, targetItemID = "ruby" },
                        new QuestObjective { description = "Find 3 Sapphires", targetCount = 3, targetItemID = "sapphire" },
                        new QuestObjective { description = "Find 3 Emeralds", targetCount = 3, targetItemID = "emerald" },
                        new QuestObjective { description = "Find 1 Diamond", targetCount = 1, targetItemID = "diamond" },
                        new QuestObjective { description = "Deliver to Jeweler", targetCount = 1, targetNPCID = "jeweler" }
                    },
                    experienceReward = 300,
                    goldReward = 500
                },

                // Exploration Quests
                new QuestDefinition
                {
                    questID = "explore_hidden_temples",
                    questName = "Temple Explorer",
                    description = "Legends speak of hidden temples scattered across the land. Discover them all!",
                    questType = QuestType.Exploration,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Discover the Temple of Sun", targetCount = 1, targetLocationID = "sun_temple" },
                        new QuestObjective { description = "Discover the Temple of Moon", targetCount = 1, targetLocationID = "moon_temple" },
                        new QuestObjective { description = "Discover the Temple of Stars", targetCount = 1, targetLocationID = "star_temple" },
                        new QuestObjective { description = "Discover the Temple of Time", targetCount = 1, targetLocationID = "time_temple" }
                    },
                    experienceReward = 400,
                    itemRewards = new List<string> { "explorer_compass" },
                    unlockLoreEntry = "ancient_temples"
                },

                new QuestDefinition
                {
                    questID = "explore_biome_tourist",
                    questName = "Biome Tourist",
                    description = "Visit every biome in the realm and document your findings.",
                    questType = QuestType.Exploration,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Visit the Desert", targetCount = 1, targetLocationID = "desert" },
                        new QuestObjective { description = "Visit the Tundra", targetCount = 1, targetLocationID = "tundra" },
                        new QuestObjective { description = "Visit the Swamp", targetCount = 1, targetLocationID = "swamp" },
                        new QuestObjective { description = "Visit the Mountains", targetCount = 1, targetLocationID = "mountains" },
                        new QuestObjective { description = "Visit the Volcano", targetCount = 1, targetLocationID = "volcano" }
                    },
                    experienceReward = 350,
                    unlockLoreEntry = "biome_encyclopedia"
                },

                // Crafting Quest
                new QuestDefinition
                {
                    questID = "craft_master_artisan",
                    questName = "Master Artisan",
                    description = "Prove your crafting prowess by creating masterwork items across different disciplines.",
                    questType = QuestType.Crafting,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Craft a Masterwork Sword", targetCount = 1, targetItemID = "masterwork_sword" },
                        new QuestObjective { description = "Craft a Masterwork Armor", targetCount = 1, targetItemID = "masterwork_armor" },
                        new QuestObjective { description = "Craft a Masterwork Potion", targetCount = 1, targetItemID = "masterwork_potion" },
                        new QuestObjective { description = "Craft a Masterwork Meal", targetCount = 1, targetItemID = "masterwork_meal" }
                    },
                    experienceReward = 500,
                    itemRewards = new List<string> { "master_artisan_seal" },
                    goldReward = 300
                },

                // Social Quest
                new QuestDefinition
                {
                    questID = "social_village_festival",
                    questName = "Village Festival",
                    description = "Help organize the annual village festival by completing tasks and making friends.",
                    questType = QuestType.Social,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Gift items to 5 villagers", targetCount = 5 },
                        new QuestObjective { description = "Reach Friendship Level 5 with any NPC", targetCount = 1 },
                        new QuestObjective { description = "Donate 10 items to Festival", targetCount = 10 },
                        new QuestObjective { description = "Attend the Festival", targetCount = 1, targetLocationID = "festival_square" }
                    },
                    experienceReward = 250,
                    goldReward = 200,
                    relationshipBonus = new Dictionary<string, int>
                    {
                        { "all_npcs", 10 }
                    }
                },

                // Challenge Quest
                new QuestDefinition
                {
                    questID = "challenge_speedrun",
                    questName = "Against the Clock",
                    description = "Complete a series of tasks within the time limit to prove your efficiency.",
                    questType = QuestType.Challenge,
                    objectives = new List<QuestObjective>
                    {
                        new QuestObjective { description = "Harvest 30 crops in 5 minutes", targetCount = 30 },
                        new QuestObjective { description = "Defeat 15 enemies in 5 minutes", targetCount = 15 },
                        new QuestObjective { description = "Craft 10 items in 5 minutes", targetCount = 10 }
                    },
                    experienceReward = 300,
                    itemRewards = new List<string> { "speed_charm" }
                }
            };
        }

        /// <summary>
        /// Quest objective definition
        /// </summary>
        [System.Serializable]
        public class QuestObjective
        {
            public string description;
            public int targetCount = 1;
            public int currentCount = 0;
            public string targetItemID;
            public string targetEnemyID;
            public string targetNPCID;
            public string targetLocationID;
        }

        /// <summary>
        /// Quest definition
        /// </summary>
        [System.Serializable]
        public class QuestDefinition
        {
            public string questID;
            public string questName;
            public string description;
            public QuestType questType;
            public List<QuestObjective> objectives;

            // Rewards
            public int experienceReward;
            public int goldReward;
            public List<string> itemRewards;

            // Unlocks
            public string unlockSpellID;
            public string unlockRecipeID;
            public string unlockLoreEntry;

            // Social
            public Dictionary<string, int> relationshipBonus;
        }

        /// <summary>
        /// Quest type
        /// </summary>
        public enum QuestType
        {
            Main,
            Side,
            Daily,
            Collection,
            Exploration,
            Crafting,
            Social,
            Challenge
        }
    }
}
