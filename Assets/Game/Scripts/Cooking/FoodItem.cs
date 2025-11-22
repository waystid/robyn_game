using UnityEngine;
using CozyGame.Inventory;

namespace CozyGame.Cooking
{
    /// <summary>
    /// Food quality tier
    /// </summary>
    public enum FoodQuality
    {
        Poor,           // Burnt/failed
        Normal,         // Standard cooking
        Good,           // Well-cooked
        Excellent,      // Perfect cooking
        Masterpiece     // Critical success
    }

    /// <summary>
    /// Food category
    /// </summary>
    public enum FoodCategory
    {
        Meal,           // Full meals
        Snack,          // Small foods
        Drink,          // Beverages
        Dessert,        // Sweets
        Ingredient,     // Cooking ingredients
        Custom          // Custom type
    }

    /// <summary>
    /// Food item ScriptableObject.
    /// Extends Item with food-specific properties and buffs.
    /// Create via: Right-click → Create → Cozy Game → Cooking → Food Item
    /// </summary>
    [CreateAssetMenu(fileName = "New Food Item", menuName = "Cozy Game/Cooking/Food Item", order = 10)]
    public class FoodItem : Item
    {
        [Header("Food Properties")]
        [Tooltip("Food category")]
        public FoodCategory foodCategory = FoodCategory.Meal;

        [Tooltip("Food quality")]
        public FoodQuality foodQuality = FoodQuality.Normal;

        [Tooltip("Hunger restoration")]
        public float hungerRestore = 20f;

        [Tooltip("Health restoration (instant)")]
        public float healthRestore = 10f;

        [Tooltip("Mana restoration (instant)")]
        public float manaRestore = 0f;

        [Tooltip("Stamina restoration")]
        public float staminaRestore = 0f;

        [Header("Buffs")]
        [Tooltip("Buffs granted when consumed")]
        public FoodBuff[] buffs;

        [Tooltip("Negative effects (debuffs)")]
        public FoodBuff[] debuffs;

        [Header("Cooking")]
        [Tooltip("Is this a cooked food?")]
        public bool isCooked = true;

        [Tooltip("Can be used as cooking ingredient")]
        public bool isIngredient = false;

        [Tooltip("Cooking time (if cooked)")]
        public float cookingTime = 10f;

        [Tooltip("Spoilage time (0 = never spoils)")]
        public float spoilageTime = 0f;

        [Header("Consumption")]
        [Tooltip("Eating time (seconds)")]
        public float eatingTime = 2f;

        [Tooltip("Eating sound")]
        public string eatingSound = "eating";

        [Tooltip("Can be consumed multiple times")]
        public bool isConsumable = true;

        [Header("Visual")]
        [Tooltip("Food model prefab (for world)")]
        public GameObject foodPrefab;

        [Tooltip("Eating particle effect")]
        public GameObject eatingParticles;

        /// <summary>
        /// Consume food item
        /// </summary>
        public void Consume(GameObject consumer)
        {
            if (!isConsumable)
            {
                Debug.LogWarning($"[FoodItem] {itemName} is not consumable!");
                return;
            }

            // Restore instant values
            ApplyInstantEffects(consumer);

            // Apply buffs
            ApplyBuffs();

            // Apply debuffs
            ApplyDebuffs();

            // Play eating sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(eatingSound))
            {
                AudioManager.Instance.PlaySound(eatingSound);
            }

            // Show floating text
            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                string text = $"Ate {itemName}";
                if (healthRestore > 0)
                {
                    text += $" (+{healthRestore} HP)";
                }
                FloatingTextManager.Instance.Show(text, Camera.main.transform.position + Camera.main.transform.forward * 3f, Color.green);
            }

            Debug.Log($"[FoodItem] Consumed {itemName}");
        }

        /// <summary>
        /// Apply instant effects (health, mana, stamina)
        /// </summary>
        private void ApplyInstantEffects(GameObject consumer)
        {
            // Restore health
            if (healthRestore > 0f && PlayerStats.Instance != null)
            {
                PlayerStats.Instance.Heal(healthRestore);
            }

            // Restore mana
            if (manaRestore > 0f && Magic.MagicSystem.Instance != null)
            {
                Magic.MagicSystem.Instance.RestoreMana(manaRestore);
            }

            // TODO: Restore stamina, hunger when those systems exist
        }

        /// <summary>
        /// Apply buffs
        /// </summary>
        private void ApplyBuffs()
        {
            if (buffs == null || buffs.Length == 0 || BuffSystem.Instance == null)
                return;

            foreach (var buff in buffs)
            {
                BuffSystem.Instance.AddBuff(
                    buff.buffType,
                    $"{itemName} - {buff.buffType}",
                    buff.magnitude,
                    buff.duration,
                    buff.canStack,
                    icon
                );
            }
        }

        /// <summary>
        /// Apply debuffs (negative effects)
        /// </summary>
        private void ApplyDebuffs()
        {
            if (debuffs == null || debuffs.Length == 0 || BuffSystem.Instance == null)
                return;

            foreach (var debuff in debuffs)
            {
                BuffSystem.Instance.AddBuff(
                    debuff.buffType,
                    $"{itemName} - {debuff.buffType}",
                    debuff.magnitude,
                    debuff.duration,
                    debuff.canStack,
                    icon
                );
            }
        }

        /// <summary>
        /// Get total buff duration
        /// </summary>
        public float GetTotalBuffDuration()
        {
            float total = 0f;

            if (buffs != null)
            {
                foreach (var buff in buffs)
                {
                    total = Mathf.Max(total, buff.duration);
                }
            }

            return total;
        }

        /// <summary>
        /// Get quality multiplier
        /// </summary>
        public float GetQualityMultiplier()
        {
            switch (foodQuality)
            {
                case FoodQuality.Poor: return 0.5f;
                case FoodQuality.Normal: return 1f;
                case FoodQuality.Good: return 1.2f;
                case FoodQuality.Excellent: return 1.5f;
                case FoodQuality.Masterpiece: return 2f;
                default: return 1f;
            }
        }

        /// <summary>
        /// Get quality color
        /// </summary>
        public Color GetQualityColor()
        {
            switch (foodQuality)
            {
                case FoodQuality.Poor: return new Color(0.5f, 0.3f, 0.2f); // Brown
                case FoodQuality.Normal: return Color.white;
                case FoodQuality.Good: return Color.green;
                case FoodQuality.Excellent: return Color.cyan;
                case FoodQuality.Masterpiece: return new Color(1f, 0.5f, 0f); // Gold
                default: return Color.white;
            }
        }

        /// <summary>
        /// Get description with buffs
        /// </summary>
        public string GetFullDescription()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine(description);
            sb.AppendLine();

            // Instant effects
            if (healthRestore > 0)
                sb.AppendLine($"Restores {healthRestore} health");
            if (manaRestore > 0)
                sb.AppendLine($"Restores {manaRestore} mana");
            if (hungerRestore > 0)
                sb.AppendLine($"Restores {hungerRestore} hunger");

            // Buffs
            if (buffs != null && buffs.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Buffs:");
                foreach (var buff in buffs)
                {
                    sb.AppendLine($"  • {buff.GetDisplayString()}");
                }
            }

            // Debuffs
            if (debuffs != null && debuffs.Length > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Side Effects:");
                foreach (var debuff in debuffs)
                {
                    sb.AppendLine($"  • {debuff.GetDisplayString()}");
                }
            }

            return sb.ToString();
        }
    }
}
