using UnityEngine;
using UnityEngine.Events;

namespace CozyGame.Combat
{
    /// <summary>
    /// Health component for any damageable entity.
    /// Attach to player, enemies, destructible objects, etc.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Header("Health Settings")]
        [Tooltip("Maximum health")]
        public float maxHealth = 100f;

        [Tooltip("Current health")]
        public float currentHealth = 100f;

        [Tooltip("Regenerate health over time")]
        public bool regenerateHealth = false;

        [Tooltip("Health regeneration rate (per second)")]
        public float healthRegenRate = 1f;

        [Header("Defense")]
        [Tooltip("Base armor value")]
        public float baseArmor = 0f;

        [Tooltip("Damage reduction percentage (0-1)")]
        [Range(0f, 1f)]
        public float damageReduction = 0f;

        [Header("Death Settings")]
        [Tooltip("Is entity dead?")]
        public bool isDead = false;

        [Tooltip("Destroy on death")]
        public bool destroyOnDeath = false;

        [Tooltip("Delay before destroying (seconds)")]
        public float destroyDelay = 2f;

        [Tooltip("Death effect prefab")]
        public GameObject deathEffectPrefab;

        [Header("Invincibility")]
        [Tooltip("Is currently invincible?")]
        public bool isInvincible = false;

        [Tooltip("Invincibility duration after hit (seconds)")]
        public float invincibilityDuration = 0.5f;

        private float invincibilityTimer = 0f;

        [Header("Events")]
        public UnityEvent<DamageInfo> OnDamaged;
        public UnityEvent<float, float> OnHealthChanged; // current, max
        public UnityEvent OnDeath;
        public UnityEvent OnRevived;

        private void Start()
        {
            // Initialize health
            if (currentHealth <= 0)
            {
                currentHealth = maxHealth;
            }
        }

        private void Update()
        {
            // Health regeneration
            if (regenerateHealth && !isDead && currentHealth < maxHealth)
            {
                Heal(healthRegenRate * Time.deltaTime);
            }

            // Invincibility timer
            if (invincibilityTimer > 0f)
            {
                invincibilityTimer -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Take damage
        /// </summary>
        public virtual void TakeDamage(DamageInfo damageInfo)
        {
            if (isDead || isInvincible || invincibilityTimer > 0f)
                return;

            // Calculate final damage
            float finalDamage = CalculateDamage(damageInfo);

            // Apply damage
            currentHealth -= finalDamage;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

            // Trigger events
            OnDamaged?.Invoke(damageInfo);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Spawn damage number
            if (VFX.DamageNumberSpawner.Instance != null)
            {
                VFX.DamageNumberSpawner.Instance.SpawnDamageNumber(damageInfo);
            }

            // Check death
            if (currentHealth <= 0f && !isDead)
            {
                Die();
            }

            // Start invincibility
            if (invincibilityDuration > 0f)
            {
                invincibilityTimer = invincibilityDuration;
            }

            // Track statistics (if player)
            if (gameObject.CompareTag("Player") && Achievements.StatisticsTracker.Instance != null)
            {
                Achievements.StatisticsTracker.Instance.TrackDamageTaken(finalDamage);
            }

            // Track statistics (if enemy)
            if (gameObject.CompareTag("Enemy") && damageInfo.attacker != null &&
                damageInfo.attacker.CompareTag("Player") && Achievements.StatisticsTracker.Instance != null)
            {
                Achievements.StatisticsTracker.Instance.TrackDamageDealt(finalDamage);
            }
        }

        /// <summary>
        /// Calculate final damage after armor and reductions
        /// </summary>
        protected virtual float CalculateDamage(DamageInfo damageInfo)
        {
            float damage = damageInfo.amount;

            // True damage ignores armor
            if (damageInfo.damageType == DamageType.True)
            {
                return damage;
            }

            // Apply armor reduction (simple formula)
            float totalArmor = baseArmor;

            // Add equipment armor if player
            if (gameObject.CompareTag("Player") && Inventory.EquipmentSystem.Instance != null)
            {
                totalArmor += Inventory.EquipmentSystem.Instance.GetTotalArmor();
            }

            if (totalArmor > 0)
            {
                float armorReduction = totalArmor / (totalArmor + 100f);
                damage *= (1f - armorReduction);
            }

            // Apply general damage reduction
            if (damageReduction > 0f)
            {
                damage *= (1f - damageReduction);
            }

            // Blocked damage
            if (damageInfo.isBlocked)
            {
                damage *= 0.5f; // 50% reduction when blocked
            }

            return Mathf.Max(1f, damage); // Minimum 1 damage
        }

        /// <summary>
        /// Heal health
        /// </summary>
        public virtual void Heal(float amount)
        {
            if (isDead)
                return;

            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Spawn heal number
            if (VFX.DamageNumberSpawner.Instance != null && amount > 0)
            {
                VFX.DamageNumberSpawner.Instance.SpawnHealNumber(amount, transform.position);
            }
        }

        /// <summary>
        /// Die
        /// </summary>
        protected virtual void Die()
        {
            if (isDead)
                return;

            isDead = true;

            // Spawn death effect
            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }

            // Trigger event
            OnDeath?.Invoke();

            // Track death (if player)
            if (gameObject.CompareTag("Player") && Achievements.StatisticsTracker.Instance != null)
            {
                Achievements.StatisticsTracker.Instance.TrackDeath();
            }

            // Track enemy killed (if enemy)
            if (gameObject.CompareTag("Enemy") && Achievements.StatisticsTracker.Instance != null)
            {
                Achievements.StatisticsTracker.Instance.TrackEnemyKilled(gameObject.name);
            }

            // Destroy after delay
            if (destroyOnDeath)
            {
                Destroy(gameObject, destroyDelay);
            }
        }

        /// <summary>
        /// Revive/resurrect
        /// </summary>
        public virtual void Revive(float healthPercent = 1f)
        {
            isDead = false;
            currentHealth = maxHealth * healthPercent;

            OnRevived?.Invoke();
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Set max health
        /// </summary>
        public void SetMaxHealth(float newMaxHealth, bool healToFull = false)
        {
            maxHealth = newMaxHealth;

            if (healToFull)
            {
                currentHealth = maxHealth;
            }
            else
            {
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Get health percentage (0-1)
        /// </summary>
        public float GetHealthPercentage()
        {
            if (maxHealth <= 0)
                return 0f;

            return currentHealth / maxHealth;
        }

        /// <summary>
        /// Check if at full health
        /// </summary>
        public bool IsFullHealth()
        {
            return currentHealth >= maxHealth;
        }

        /// <summary>
        /// Check if at low health
        /// </summary>
        public bool IsLowHealth(float threshold = 0.25f)
        {
            return GetHealthPercentage() <= threshold;
        }

        /// <summary>
        /// Set invincibility
        /// </summary>
        public void SetInvincible(bool invincible, float duration = 0f)
        {
            isInvincible = invincible;

            if (duration > 0f)
            {
                invincibilityTimer = duration;
            }
        }
    }
}
