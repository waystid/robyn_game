using UnityEngine;
using System.Collections.Generic;

namespace CozyGame.Magic
{
    /// <summary>
    /// Core magic system - manages spells, mana, and magic casting
    /// Singleton pattern for global access
    /// Integrates with Survival Engine's attribute system
    /// </summary>
    public class MagicSystem : MonoBehaviour
    {
        public static MagicSystem Instance { get; private set; }

        [Header("Mana Settings")]
        [Tooltip("Current mana amount")]
        public float currentMana = 100f;

        [Tooltip("Maximum mana capacity")]
        public float maxMana = 100f;

        [Tooltip("Mana regeneration per second")]
        public float manaRegenRate = 5f;

        [Tooltip("Delay before mana starts regenerating after use (seconds)")]
        public float regenDelay = 1f;

        [Header("Casting Settings")]
        [Tooltip("Global cooldown after casting any spell (seconds)")]
        public float globalCooldown = 0.5f;

        [Tooltip("Maximum casting range for targeted spells")]
        public float maxCastRange = 10f;

        [Tooltip("Layer mask for valid spell targets")]
        public LayerMask targetLayers = -1;

        [Header("Visual Effects")]
        [Tooltip("Particle effect when casting spell")]
        public GameObject castParticlePrefab;

        [Tooltip("Particle effect when out of mana")]
        public GameObject outOfManaPrefab;

        [Header("Audio")]
        [Tooltip("Sound played when casting spell")]
        public string castSoundName = "spell_cast";

        [Tooltip("Sound played when out of mana")]
        public string outOfManaSoundName = "mana_empty";

        [Header("Events")]
        public delegate void ManaEvent(float current, float max);
        public event ManaEvent OnManaChanged;

        public delegate void SpellEvent(SpellData spell);
        public event SpellEvent OnSpellCast;
        public event SpellEvent OnSpellFailed;

        [Header("Debug")]
        public bool showDebugLogs = true;

        // Runtime state
        private float timeSinceLastCast = 0f;
        private float timeSinceLastManaUse = 0f;
        private bool isOnCooldown = false;

        // Spell registry
        private Dictionary<string, SpellData> registeredSpells = new Dictionary<string, SpellData>();

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

        private void Start()
        {
            // Initialize mana to max
            currentMana = maxMana;
            OnManaChanged?.Invoke(currentMana, maxMana);
        }

        private void Update()
        {
            // Update cooldown timer
            if (isOnCooldown)
            {
                timeSinceLastCast += Time.deltaTime;
                if (timeSinceLastCast >= globalCooldown)
                {
                    isOnCooldown = false;
                }
            }

            // Regenerate mana
            RegenerateMana();
        }

        private void RegenerateMana()
        {
            if (currentMana >= maxMana)
                return;

            timeSinceLastManaUse += Time.deltaTime;

            // Only regen after delay
            if (timeSinceLastManaUse >= regenDelay)
            {
                float regenAmount = manaRegenRate * Time.deltaTime;
                AddMana(regenAmount);
            }
        }

        /// <summary>
        /// Use mana (returns true if enough mana available)
        /// </summary>
        public bool UseMana(float amount)
        {
            if (currentMana < amount)
            {
                Log($"Not enough mana! Need {amount}, have {currentMana}");
                PlayOutOfManaEffects();
                return false;
            }

            currentMana = Mathf.Max(0f, currentMana - amount);
            timeSinceLastManaUse = 0f; // Reset regen delay
            OnManaChanged?.Invoke(currentMana, maxMana);

            Log($"Used {amount} mana. Remaining: {currentMana}/{maxMana}");
            return true;
        }

        /// <summary>
        /// Add mana (clamped to max)
        /// </summary>
        public void AddMana(float amount)
        {
            float previousMana = currentMana;
            currentMana = Mathf.Min(maxMana, currentMana + amount);

            if (currentMana != previousMana)
            {
                OnManaChanged?.Invoke(currentMana, maxMana);
            }
        }

        /// <summary>
        /// Set max mana capacity
        /// </summary>
        public void SetMaxMana(float newMax)
        {
            maxMana = newMax;
            currentMana = Mathf.Min(currentMana, maxMana);
            OnManaChanged?.Invoke(currentMana, maxMana);
        }

        /// <summary>
        /// Restore mana to full
        /// </summary>
        public void RestoreManaFull()
        {
            currentMana = maxMana;
            OnManaChanged?.Invoke(currentMana, maxMana);
            Log("Mana fully restored!");
        }

        /// <summary>
        /// Cast a spell at a target position
        /// </summary>
        public bool CastSpell(SpellData spell, Vector3 targetPosition)
        {
            if (spell == null)
            {
                LogWarning("Cannot cast null spell!");
                return false;
            }

            // Check global cooldown
            if (isOnCooldown)
            {
                Log($"Spell on cooldown ({globalCooldown - timeSinceLastCast:F1}s remaining)");
                return false;
            }

            // Check range
            Vector3 casterPosition = transform.position;
            float distance = Vector3.Distance(casterPosition, targetPosition);
            if (distance > spell.castRange)
            {
                Log($"Target out of range! {distance:F1}m > {spell.castRange}m");
                return false;
            }

            // Check mana cost
            if (!UseMana(spell.manaCost))
            {
                OnSpellFailed?.Invoke(spell);
                return false;
            }

            // Cast successful
            isOnCooldown = true;
            timeSinceLastCast = 0f;

            // Visual and audio feedback
            PlayCastEffects(targetPosition);

            // Trigger event
            OnSpellCast?.Invoke(spell);

            Log($"Cast spell: {spell.spellName} at {targetPosition}");
            return true;
        }

        /// <summary>
        /// Cast a spell on a target GameObject
        /// </summary>
        public bool CastSpellOnTarget(SpellData spell, GameObject target)
        {
            if (target == null)
            {
                LogWarning("Cannot cast spell on null target!");
                return false;
            }

            return CastSpell(spell, target.transform.position);
        }

        /// <summary>
        /// Register a spell for use
        /// </summary>
        public void RegisterSpell(SpellData spell)
        {
            if (spell == null) return;

            if (!registeredSpells.ContainsKey(spell.spellID))
            {
                registeredSpells.Add(spell.spellID, spell);
                Log($"Registered spell: {spell.spellName}");
            }
        }

        /// <summary>
        /// Get registered spell by ID
        /// </summary>
        public SpellData GetSpell(string spellID)
        {
            if (registeredSpells.TryGetValue(spellID, out SpellData spell))
            {
                return spell;
            }
            return null;
        }

        /// <summary>
        /// Check if player can cast spell (has enough mana and not on cooldown)
        /// </summary>
        public bool CanCastSpell(SpellData spell)
        {
            if (spell == null) return false;
            if (isOnCooldown) return false;
            if (currentMana < spell.manaCost) return false;
            return true;
        }

        /// <summary>
        /// Get current mana as percentage (0-1)
        /// </summary>
        public float GetManaPercentage()
        {
            if (maxMana == 0) return 0f;
            return currentMana / maxMana;
        }

        private void PlayCastEffects(Vector3 position)
        {
            // Spawn particle effect
            if (castParticlePrefab != null)
            {
                GameObject particle = Instantiate(castParticlePrefab, position, Quaternion.identity);
                Destroy(particle, 3f); // Clean up after 3 seconds
            }

            // Play sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(castSoundName))
            {
                AudioManager.Instance.PlaySoundAtPosition(castSoundName, position);
            }
        }

        private void PlayOutOfManaEffects()
        {
            // Spawn particle effect
            if (outOfManaPrefab != null)
            {
                GameObject particle = Instantiate(outOfManaPrefab, transform.position, Quaternion.identity);
                Destroy(particle, 2f);
            }

            // Play sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(outOfManaSoundName))
            {
                AudioManager.Instance.PlaySound(outOfManaSoundName);
            }
        }

        // Helper logging methods
        private void Log(string message)
        {
            if (showDebugLogs)
                Debug.Log($"[MagicSystem] {message}");
        }

        private void LogWarning(string message)
        {
            if (showDebugLogs)
                Debug.LogWarning($"[MagicSystem] {message}");
        }

        /// <summary>
        /// Reset magic system (useful for testing)
        /// </summary>
        [ContextMenu("Reset Magic System")]
        public void ResetMagicSystem()
        {
            currentMana = maxMana;
            isOnCooldown = false;
            timeSinceLastCast = 0f;
            timeSinceLastManaUse = 0f;
            OnManaChanged?.Invoke(currentMana, maxMana);
            Log("Magic system reset!");
        }
    }
}
