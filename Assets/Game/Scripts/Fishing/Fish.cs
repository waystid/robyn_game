using UnityEngine;

namespace CozyGame.Fishing
{
    /// <summary>
    /// Fish rarity tiers
    /// </summary>
    public enum FishRarity
    {
        Common,      // Easy to catch, low value
        Uncommon,    // Moderate difficulty, medium value
        Rare,        // Difficult to catch, high value
        Epic,        // Very difficult, very valuable
        Legendary    // Extremely rare, extremely valuable
    }

    /// <summary>
    /// Time of day when fish is most active
    /// </summary>
    public enum FishTimePreference
    {
        Any,         // Active at all times
        Day,         // Active during daytime
        Night,       // Active during nighttime
        DawnDusk     // Active at dawn/dusk
    }

    /// <summary>
    /// Weather preference for fish
    /// </summary>
    public enum FishWeatherPreference
    {
        Any,         // Any weather
        Clear,       // Clear weather only
        Rain,        // Rainy weather only
        Storm        // Storm weather only
    }

    /// <summary>
    /// Fish ScriptableObject definition.
    /// Defines properties for catchable fish species.
    /// Create instances via: Right-click → Create → Cozy Game → Fishing → Fish
    /// </summary>
    [CreateAssetMenu(fileName = "New Fish", menuName = "Cozy Game/Fishing/Fish", order = 10)]
    public class Fish : ScriptableObject
    {
        [Header("Fish Identity")]
        [Tooltip("Display name of the fish")]
        public string fishName = "Fish";

        [Tooltip("Unique identifier for this fish")]
        public string fishID;

        [Tooltip("Description shown to player")]
        [TextArea(2, 4)]
        public string description = "A fish found in local waters...";

        [Header("Visual")]
        [Tooltip("Fish icon for inventory/UI")]
        public Sprite icon;

        [Tooltip("Fish prefab (3D model)")]
        public GameObject fishPrefab;

        [Tooltip("Fish color tint")]
        public Color fishColor = Color.white;

        [Header("Rarity & Value")]
        [Tooltip("Fish rarity tier")]
        public FishRarity rarity = FishRarity.Common;

        [Tooltip("Base sell value")]
        public int baseValue = 10;

        [Tooltip("Experience gained when caught")]
        public int expReward = 5;

        [Header("Catch Difficulty")]
        [Tooltip("How difficult this fish is to catch (0-1)")]
        [Range(0f, 1f)]
        public float catchDifficulty = 0.3f;

        [Tooltip("Fish struggle strength (higher = harder)")]
        [Range(0f, 1f)]
        public float struggleStrength = 0.5f;

        [Tooltip("How long fish fights before giving up (seconds)")]
        public float fightDuration = 5f;

        [Tooltip("Size of the catch window (larger = easier)")]
        [Range(0.1f, 0.5f)]
        public float catchWindowSize = 0.3f;

        [Tooltip("Speed at which catch window moves")]
        public float catchWindowSpeed = 1f;

        [Header("Spawn Conditions")]
        [Tooltip("Spawn weight (higher = more common)")]
        [Range(0f, 100f)]
        public float spawnWeight = 50f;

        [Tooltip("Time of day preference")]
        public FishTimePreference timePreference = FishTimePreference.Any;

        [Tooltip("Weather preference")]
        public FishWeatherPreference weatherPreference = FishWeatherPreference.Any;

        [Tooltip("Minimum depth required (0 = any)")]
        public float minDepth = 0f;

        [Tooltip("Specific location tags (optional)")]
        public string[] locationTags;

        [Header("Size Variation")]
        [Tooltip("Minimum size (cm)")]
        public float minSize = 10f;

        [Tooltip("Maximum size (cm)")]
        public float maxSize = 30f;

        [Tooltip("Average size (cm)")]
        public float averageSize = 20f;

        [Header("Bonus Conditions")]
        [Tooltip("Bonus chance during preferred time")]
        [Range(1f, 3f)]
        public float timeBonus = 1.5f;

        [Tooltip("Bonus chance during preferred weather")]
        [Range(1f, 3f)]
        public float weatherBonus = 1.5f;

        private void OnEnable()
        {
            // Generate unique ID if empty
            if (string.IsNullOrEmpty(fishID))
            {
                fishID = "fish_" + name.ToLower().Replace(" ", "_");
            }
        }

        /// <summary>
        /// Get spawn weight modified by current conditions
        /// </summary>
        public float GetModifiedSpawnWeight(FishTimePreference currentTime, FishWeatherPreference currentWeather)
        {
            float weight = spawnWeight;

            // Apply time bonus
            if (timePreference != FishTimePreference.Any && timePreference == currentTime)
            {
                weight *= timeBonus;
            }

            // Apply weather bonus
            if (weatherPreference != FishWeatherPreference.Any && weatherPreference == currentWeather)
            {
                weight *= weatherBonus;
            }

            return weight;
        }

        /// <summary>
        /// Generate random size for this fish
        /// </summary>
        public float GetRandomSize()
        {
            // Use normal distribution centered on average
            float variance = (maxSize - minSize) * 0.3f;
            float size = averageSize + Random.Range(-variance, variance);

            return Mathf.Clamp(size, minSize, maxSize);
        }

        /// <summary>
        /// Get size category name
        /// </summary>
        public string GetSizeCategory(float size)
        {
            float range = maxSize - minSize;
            float normalized = (size - minSize) / range;

            if (normalized < 0.3f)
                return "Small";
            else if (normalized < 0.7f)
                return "Medium";
            else
                return "Large";
        }

        /// <summary>
        /// Get value modified by size
        /// </summary>
        public int GetValueBySize(float size)
        {
            // Larger fish are worth more
            float sizeMultiplier = size / averageSize;
            return Mathf.RoundToInt(baseValue * sizeMultiplier);
        }

        /// <summary>
        /// Get rarity color
        /// </summary>
        public Color GetRarityColor()
        {
            switch (rarity)
            {
                case FishRarity.Common:
                    return new Color(0.8f, 0.8f, 0.8f); // Gray
                case FishRarity.Uncommon:
                    return new Color(0.2f, 0.8f, 0.2f); // Green
                case FishRarity.Rare:
                    return new Color(0.2f, 0.4f, 1f);   // Blue
                case FishRarity.Epic:
                    return new Color(0.8f, 0.2f, 1f);   // Purple
                case FishRarity.Legendary:
                    return new Color(1f, 0.65f, 0f);    // Orange/Gold
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// Get fish info for UI display
        /// </summary>
        public string GetFishInfo()
        {
            string info = $"<b>{fishName}</b>\n";
            info += $"<color=#{ColorUtility.ToHtmlStringRGB(GetRarityColor())}>{rarity}</color>\n\n";
            info += $"{description}\n\n";
            info += $"Value: {baseValue} coins\n";
            info += $"Size: {minSize:F0}-{maxSize:F0}cm\n";
            info += $"Difficulty: {GetDifficultyText()}\n";

            if (timePreference != FishTimePreference.Any)
            {
                info += $"Best Time: {timePreference}\n";
            }

            if (weatherPreference != FishWeatherPreference.Any)
            {
                info += $"Best Weather: {weatherPreference}\n";
            }

            return info;
        }

        /// <summary>
        /// Get difficulty as text
        /// </summary>
        private string GetDifficultyText()
        {
            if (catchDifficulty < 0.2f)
                return "Very Easy";
            else if (catchDifficulty < 0.4f)
                return "Easy";
            else if (catchDifficulty < 0.6f)
                return "Medium";
            else if (catchDifficulty < 0.8f)
                return "Hard";
            else
                return "Very Hard";
        }
    }
}
