using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace CozyGame.Economy
{
    /// <summary>
    /// Currency type enum
    /// </summary>
    public enum CurrencyType
    {
        Gold,       // Primary currency
        Gems,       // Premium currency
        Tokens,     // Event currency
        Hearts,     // Friendship currency
        Tickets,    // Special currency
        Custom1,    // Custom currency slot
        Custom2,    // Custom currency slot
        Custom3     // Custom currency slot
    }

    /// <summary>
    /// Currency save data
    /// </summary>
    [System.Serializable]
    public class CurrencySaveData
    {
        public Dictionary<string, int> currencyAmounts = new Dictionary<string, int>();
    }

    /// <summary>
    /// Currency manager singleton.
    /// Manages multiple currency types (gold, gems, tokens, etc.).
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        [Header("Starting Currency")]
        [Tooltip("Starting gold amount")]
        public int startingGold = 100;

        [Tooltip("Starting gems amount")]
        public int startingGems = 0;

        [Tooltip("Starting tokens amount")]
        public int startingTokens = 0;

        [Header("Currency Names")]
        [Tooltip("Custom name for Custom1 currency")]
        public string custom1Name = "Custom1";

        [Tooltip("Custom name for Custom2 currency")]
        public string custom2Name = "Custom2";

        [Tooltip("Custom name for Custom3 currency")]
        public string custom3Name = "Custom3";

        [Header("Events")]
        public UnityEvent<CurrencyType, int> OnCurrencyChanged; // type, newAmount
        public UnityEvent<CurrencyType, int> OnCurrencyGained; // type, amount
        public UnityEvent<CurrencyType, int> OnCurrencySpent; // type, amount

        // Currency storage
        private Dictionary<CurrencyType, int> currencyAmounts = new Dictionary<CurrencyType, int>();

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
        /// Initialize currency manager
        /// </summary>
        private void Initialize()
        {
            // Initialize all currency types to 0
            foreach (CurrencyType type in System.Enum.GetValues(typeof(CurrencyType)))
            {
                currencyAmounts[type] = 0;
            }

            // Set starting amounts
            currencyAmounts[CurrencyType.Gold] = startingGold;
            currencyAmounts[CurrencyType.Gems] = startingGems;
            currencyAmounts[CurrencyType.Tokens] = startingTokens;

            Debug.Log("[CurrencyManager] Initialized");
        }

        /// <summary>
        /// Get currency amount
        /// </summary>
        public int GetCurrency(CurrencyType type)
        {
            if (currencyAmounts.ContainsKey(type))
            {
                return currencyAmounts[type];
            }

            return 0;
        }

        /// <summary>
        /// Add currency
        /// </summary>
        public void AddCurrency(CurrencyType type, int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CurrencyManager] Tried to add {amount} {type}");
                return;
            }

            if (!currencyAmounts.ContainsKey(type))
            {
                currencyAmounts[type] = 0;
            }

            currencyAmounts[type] += amount;

            Debug.Log($"[CurrencyManager] Added {amount} {type}. Total: {currencyAmounts[type]}");

            // Trigger events
            OnCurrencyGained?.Invoke(type, amount);
            OnCurrencyChanged?.Invoke(type, currencyAmounts[type]);

            // Show notification
            ShowCurrencyNotification(type, amount, true);

            // Track statistics
            if (Achievements.StatisticsTracker.Instance != null)
            {
                Achievements.StatisticsTracker.Instance.IncrementStatistic($"{type.ToString().ToLower()}_earned", amount);
            }
        }

        /// <summary>
        /// Remove currency
        /// </summary>
        public bool RemoveCurrency(CurrencyType type, int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CurrencyManager] Tried to remove {amount} {type}");
                return false;
            }

            if (!currencyAmounts.ContainsKey(type))
            {
                currencyAmounts[type] = 0;
            }

            if (currencyAmounts[type] < amount)
            {
                Debug.LogWarning($"[CurrencyManager] Not enough {type}! Have {currencyAmounts[type]}, need {amount}");
                return false;
            }

            currencyAmounts[type] -= amount;

            Debug.Log($"[CurrencyManager] Removed {amount} {type}. Total: {currencyAmounts[type]}");

            // Trigger events
            OnCurrencySpent?.Invoke(type, amount);
            OnCurrencyChanged?.Invoke(type, currencyAmounts[type]);

            // Track statistics
            if (Achievements.StatisticsTracker.Instance != null)
            {
                Achievements.StatisticsTracker.Instance.IncrementStatistic($"{type.ToString().ToLower()}_spent", amount);
            }

            return true;
        }

        /// <summary>
        /// Check if has enough currency
        /// </summary>
        public bool HasCurrency(CurrencyType type, int amount)
        {
            return GetCurrency(type) >= amount;
        }

        /// <summary>
        /// Set currency to specific amount
        /// </summary>
        public void SetCurrency(CurrencyType type, int amount)
        {
            if (amount < 0)
            {
                amount = 0;
            }

            int oldAmount = GetCurrency(type);
            currencyAmounts[type] = amount;

            Debug.Log($"[CurrencyManager] Set {type} to {amount}");

            OnCurrencyChanged?.Invoke(type, amount);

            // Determine if gained or spent
            int difference = amount - oldAmount;
            if (difference > 0)
            {
                OnCurrencyGained?.Invoke(type, difference);
            }
            else if (difference < 0)
            {
                OnCurrencySpent?.Invoke(type, Mathf.Abs(difference));
            }
        }

        /// <summary>
        /// Get currency name
        /// </summary>
        public string GetCurrencyName(CurrencyType type)
        {
            switch (type)
            {
                case CurrencyType.Custom1:
                    return custom1Name;
                case CurrencyType.Custom2:
                    return custom2Name;
                case CurrencyType.Custom3:
                    return custom3Name;
                default:
                    return type.ToString();
            }
        }

        /// <summary>
        /// Get currency icon/symbol
        /// </summary>
        public string GetCurrencySymbol(CurrencyType type)
        {
            switch (type)
            {
                case CurrencyType.Gold:
                    return "G";
                case CurrencyType.Gems:
                    return "💎";
                case CurrencyType.Tokens:
                    return "T";
                case CurrencyType.Hearts:
                    return "❤";
                case CurrencyType.Tickets:
                    return "🎫";
                default:
                    return type.ToString().Substring(0, 1);
            }
        }

        /// <summary>
        /// Get currency color
        /// </summary>
        public Color GetCurrencyColor(CurrencyType type)
        {
            switch (type)
            {
                case CurrencyType.Gold:
                    return new Color(1f, 0.84f, 0f); // Gold
                case CurrencyType.Gems:
                    return new Color(0.4f, 0.7f, 1f); // Light blue
                case CurrencyType.Tokens:
                    return new Color(0.9f, 0.5f, 0.2f); // Orange
                case CurrencyType.Hearts:
                    return new Color(1f, 0.3f, 0.5f); // Pink
                case CurrencyType.Tickets:
                    return new Color(0.6f, 0.4f, 1f); // Purple
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// Show currency gain/loss notification
        /// </summary>
        private void ShowCurrencyNotification(CurrencyType type, int amount, bool isGain)
        {
            if (FloatingTextManager.Instance == null)
                return;

            string symbol = GetCurrencySymbol(type);
            string text = $"{(isGain ? "+" : "-")}{amount} {symbol}";
            Color color = GetCurrencyColor(type);

            // Show floating text
            if (PlayerController.Instance != null)
            {
                FloatingTextManager.Instance.Show(text, PlayerController.Instance.transform.position + Vector3.up * 2f, color);
            }
        }

        /// <summary>
        /// Get formatted currency string
        /// </summary>
        public string GetFormattedCurrency(CurrencyType type)
        {
            int amount = GetCurrency(type);
            string symbol = GetCurrencySymbol(type);
            return $"{amount} {symbol}";
        }

        /// <summary>
        /// Get all currency amounts as dictionary
        /// </summary>
        public Dictionary<CurrencyType, int> GetAllCurrency()
        {
            return new Dictionary<CurrencyType, int>(currencyAmounts);
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public CurrencySaveData GetSaveData()
        {
            CurrencySaveData data = new CurrencySaveData();

            foreach (var kvp in currencyAmounts)
            {
                data.currencyAmounts[kvp.Key.ToString()] = kvp.Value;
            }

            return data;
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(CurrencySaveData data)
        {
            if (data == null)
                return;

            foreach (var kvp in data.currencyAmounts)
            {
                if (System.Enum.TryParse<CurrencyType>(kvp.Key, out CurrencyType type))
                {
                    currencyAmounts[type] = kvp.Value;
                }
            }

            Debug.Log($"[CurrencyManager] Loaded currency data");

            // Trigger update events
            foreach (var kvp in currencyAmounts)
            {
                OnCurrencyChanged?.Invoke(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Reset all currency
        /// </summary>
        public void ResetAllCurrency()
        {
            foreach (CurrencyType type in System.Enum.GetValues(typeof(CurrencyType)))
            {
                currencyAmounts[type] = 0;
            }

            currencyAmounts[CurrencyType.Gold] = startingGold;
            currencyAmounts[CurrencyType.Gems] = startingGems;
            currencyAmounts[CurrencyType.Tokens] = startingTokens;

            Debug.Log("[CurrencyManager] Reset all currency");
        }
    }
}
