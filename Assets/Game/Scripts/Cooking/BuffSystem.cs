using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace CozyGame.Cooking
{
    /// <summary>
    /// Buff type
    /// </summary>
    public enum BuffType
    {
        HealthRegen,        // Regenerate health over time
        ManaRegen,          // Regenerate mana over time
        SpeedBoost,         // Increase movement speed
        StrengthBoost,      // Increase damage
        DefenseBoost,       // Reduce damage taken
        LuckBoost,          // Increase critical/drop chance
        ExperienceBoost,    // Increase experience gain
        GatheringBoost,     // Increase resource yield
        Custom              // Custom buff
    }

    /// <summary>
    /// Active buff instance
    /// </summary>
    [System.Serializable]
    public class ActiveBuff
    {
        public BuffType buffType;
        public string buffName;
        public float magnitude;         // Buff strength (e.g., 1.5 = 50% boost)
        public float duration;          // Total duration
        public float remainingTime;     // Time left
        public int stackCount;          // Number of stacks
        public bool canStack;           // Can this buff stack?
        public Sprite icon;

        public ActiveBuff(BuffType type, string name, float mag, float dur, bool stack, Sprite ic = null)
        {
            buffType = type;
            buffName = name;
            magnitude = mag;
            duration = dur;
            remainingTime = dur;
            stackCount = 1;
            canStack = stack;
            icon = ic;
        }

        public float GetProgress()
        {
            return duration > 0 ? remainingTime / duration : 0f;
        }

        public bool IsExpired()
        {
            return remainingTime <= 0f;
        }
    }

    /// <summary>
    /// Buff system singleton.
    /// Manages active buffs and their effects.
    /// </summary>
    public class BuffSystem : MonoBehaviour
    {
        public static BuffSystem Instance { get; private set; }

        [Header("Active Buffs")]
        [Tooltip("Currently active buffs")]
        public List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

        [Tooltip("Max active buffs")]
        public int maxActiveBuffs = 10;

        [Header("Events")]
        public UnityEvent<ActiveBuff> OnBuffAdded;
        public UnityEvent<ActiveBuff> OnBuffRemoved;
        public UnityEvent<ActiveBuff> OnBuffRefreshed;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            UpdateBuffs();
        }

        /// <summary>
        /// Update active buffs
        /// </summary>
        private void UpdateBuffs()
        {
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                ActiveBuff buff = activeBuffs[i];
                buff.remainingTime -= Time.deltaTime;

                if (buff.IsExpired())
                {
                    RemoveBuff(buff);
                }
            }
        }

        /// <summary>
        /// Add buff
        /// </summary>
        public void AddBuff(BuffType type, string name, float magnitude, float duration, bool canStack = false, Sprite icon = null)
        {
            // Check if buff already exists
            ActiveBuff existing = activeBuffs.Find(b => b.buffType == type && b.buffName == name);

            if (existing != null)
            {
                if (existing.canStack && canStack)
                {
                    // Stack buff
                    existing.stackCount++;
                    existing.magnitude += magnitude;
                    existing.remainingTime = Mathf.Max(existing.remainingTime, duration); // Refresh duration
                    OnBuffRefreshed?.Invoke(existing);
                }
                else
                {
                    // Refresh duration
                    existing.remainingTime = duration;
                    OnBuffRefreshed?.Invoke(existing);
                }
            }
            else
            {
                // Add new buff
                if (activeBuffs.Count >= maxActiveBuffs)
                {
                    // Remove oldest buff
                    RemoveBuff(activeBuffs[0]);
                }

                ActiveBuff newBuff = new ActiveBuff(type, name, magnitude, duration, canStack, icon);
                activeBuffs.Add(newBuff);
                OnBuffAdded?.Invoke(newBuff);

                Debug.Log($"[BuffSystem] Added buff: {name} ({magnitude}x for {duration}s)");
            }

            // Show notification
            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show($"+{name}", Camera.main.transform.position + Camera.main.transform.forward * 3f, Color.cyan);
            }
        }

        /// <summary>
        /// Remove buff
        /// </summary>
        public void RemoveBuff(ActiveBuff buff)
        {
            if (buff == null || !activeBuffs.Contains(buff))
                return;

            activeBuffs.Remove(buff);
            OnBuffRemoved?.Invoke(buff);

            Debug.Log($"[BuffSystem] Removed buff: {buff.buffName}");
        }

        /// <summary>
        /// Get buff multiplier for type
        /// </summary>
        public float GetBuffMultiplier(BuffType type)
        {
            float totalMultiplier = 1f;

            foreach (var buff in activeBuffs)
            {
                if (buff.buffType == type)
                {
                    totalMultiplier += (buff.magnitude - 1f); // e.g., 1.5 = +50%, so add 0.5
                }
            }

            return totalMultiplier;
        }

        /// <summary>
        /// Get active buffs of type
        /// </summary>
        public List<ActiveBuff> GetBuffsOfType(BuffType type)
        {
            return activeBuffs.FindAll(b => b.buffType == type);
        }

        /// <summary>
        /// Has buff of type?
        /// </summary>
        public bool HasBuff(BuffType type)
        {
            return activeBuffs.Exists(b => b.buffType == type);
        }

        /// <summary>
        /// Clear all buffs
        /// </summary>
        public void ClearAllBuffs()
        {
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                RemoveBuff(activeBuffs[i]);
            }
        }

        /// <summary>
        /// Get all active buffs
        /// </summary>
        public List<ActiveBuff> GetActiveBuffs()
        {
            return new List<ActiveBuff>(activeBuffs);
        }
    }

    /// <summary>
    /// Food buff definition
    /// </summary>
    [System.Serializable]
    public class FoodBuff
    {
        [Tooltip("Buff type")]
        public BuffType buffType = BuffType.HealthRegen;

        [Tooltip("Buff magnitude (multiplier or amount per second)")]
        public float magnitude = 1.2f;

        [Tooltip("Buff duration (seconds)")]
        public float duration = 60f;

        [Tooltip("Can stack with other instances")]
        public bool canStack = false;

        [Tooltip("Buff description")]
        public string description = "+20% for 60 seconds";

        /// <summary>
        /// Get display string
        /// </summary>
        public string GetDisplayString()
        {
            switch (buffType)
            {
                case BuffType.HealthRegen:
                    return $"Regenerate {magnitude} HP/s for {duration}s";
                case BuffType.ManaRegen:
                    return $"Regenerate {magnitude} MP/s for {duration}s";
                case BuffType.SpeedBoost:
                    return $"+{(magnitude - 1f) * 100f:F0}% Speed for {duration}s";
                case BuffType.StrengthBoost:
                    return $"+{(magnitude - 1f) * 100f:F0}% Damage for {duration}s";
                case BuffType.DefenseBoost:
                    return $"+{(magnitude - 1f) * 100f:F0}% Defense for {duration}s";
                case BuffType.LuckBoost:
                    return $"+{(magnitude - 1f) * 100f:F0}% Luck for {duration}s";
                case BuffType.ExperienceBoost:
                    return $"+{(magnitude - 1f) * 100f:F0}% XP for {duration}s";
                case BuffType.GatheringBoost:
                    return $"+{(magnitude - 1f) * 100f:F0}% Gathering for {duration}s";
                default:
                    return description;
            }
        }

        /// <summary>
        /// Get buff icon color
        /// </summary>
        public Color GetBuffColor()
        {
            switch (buffType)
            {
                case BuffType.HealthRegen: return Color.red;
                case BuffType.ManaRegen: return Color.blue;
                case BuffType.SpeedBoost: return Color.yellow;
                case BuffType.StrengthBoost: return new Color(1f, 0.5f, 0f); // Orange
                case BuffType.DefenseBoost: return Color.gray;
                case BuffType.LuckBoost: return Color.green;
                case BuffType.ExperienceBoost: return new Color(0.5f, 0f, 1f); // Purple
                case BuffType.GatheringBoost: return Color.cyan;
                default: return Color.white;
            }
        }
    }
}
