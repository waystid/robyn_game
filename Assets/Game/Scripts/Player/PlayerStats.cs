using UnityEngine;
using UnityEngine.Events;

namespace CozyGame
{
    /// <summary>
    /// Manages player statistics: health, mana, stamina, level, and experience.
    /// Provides events for UI updates and game logic.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        [Header("Health")]
        [Tooltip("Maximum health")]
        public float maxHealth = 100f;

        [Tooltip("Current health")]
        public float currentHealth = 100f;

        [Tooltip("Health regeneration per second")]
        public float healthRegenRate = 1f;

        [Tooltip("Delay before health regeneration starts (seconds)")]
        public float healthRegenDelay = 5f;

        [Header("Mana")]
        [Tooltip("Maximum mana")]
        public float maxMana = 100f;

        [Tooltip("Current mana")]
        public float currentMana = 100f;

        [Tooltip("Use MagicSystem for mana (if false, manage locally)")]
        public bool useMagicSystem = true;

        [Header("Stamina")]
        [Tooltip("Maximum stamina")]
        public float maxStamina = 100f;

        [Tooltip("Current stamina")]
        public float currentStamina = 100f;

        [Tooltip("Stamina drain per second when running")]
        public float staminaDrainRate = 10f;

        [Tooltip("Stamina regeneration per second")]
        public float staminaRegenRate = 20f;

        [Tooltip("Delay before stamina regeneration starts")]
        public float staminaRegenDelay = 2f;

        [Header("Level & Experience")]
        [Tooltip("Current player level")]
        public int level = 1;

        [Tooltip("Current experience points")]
        public int currentExp = 0;

        [Tooltip("Experience required for next level")]
        public int expToNextLevel = 100;

        [Tooltip("Experience multiplier per level")]
        public float expMultiplier = 1.5f;

        [Header("Death Settings")]
        [Tooltip("Is player currently dead")]
        public bool isDead = false;

        [Tooltip("Respawn delay (seconds)")]
        public float respawnDelay = 3f;

        [Tooltip("Respawn position (if null, respawn at current position)")]
        public Transform respawnPoint;

        // Events
        public UnityEvent<float, float> OnHealthChanged; // current, max
        public UnityEvent<float, float> OnManaChanged; // current, max
        public UnityEvent<float, float> OnStaminaChanged; // current, max
        public UnityEvent<int> OnLevelUp; // new level
        public UnityEvent<int, int> OnExpChanged; // current exp, exp to next level
        public UnityEvent OnDeath;
        public UnityEvent OnRespawn;

        // Regeneration tracking
        private float timeSinceLastDamage;
        private float timeSinceLastStaminaUse;

        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("[PlayerStats] Multiple PlayerStats instances detected!");
                Destroy(this);
            }
        }

        private void Start()
        {
            // Initialize stats
            currentHealth = maxHealth;
            currentStamina = maxStamina;

            if (!useMagicSystem)
            {
                currentMana = maxMana;
            }

            // Trigger initial UI updates
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            OnExpChanged?.Invoke(currentExp, expToNextLevel);
        }

        private void Update()
        {
            if (isDead)
                return;

            HandleHealthRegeneration();
            HandleStaminaRegeneration();
            UpdateManaFromMagicSystem();
        }

        /// <summary>
        /// Handle health regeneration
        /// </summary>
        private void HandleHealthRegeneration()
        {
            timeSinceLastDamage += Time.deltaTime;

            if (timeSinceLastDamage >= healthRegenDelay && currentHealth < maxHealth)
            {
                AddHealth(healthRegenRate * Time.deltaTime);
            }
        }

        /// <summary>
        /// Handle stamina regeneration
        /// </summary>
        private void HandleStaminaRegeneration()
        {
            timeSinceLastStaminaUse += Time.deltaTime;

            if (timeSinceLastStaminaUse >= staminaRegenDelay && currentStamina < maxStamina)
            {
                AddStamina(staminaRegenRate * Time.deltaTime);
            }
        }

        /// <summary>
        /// Update mana from MagicSystem if enabled
        /// </summary>
        private void UpdateManaFromMagicSystem()
        {
            if (useMagicSystem && Magic.MagicSystem.Instance != null)
            {
                float magicSystemMana = Magic.MagicSystem.Instance.GetCurrentMana();
                float magicSystemMaxMana = Magic.MagicSystem.Instance.GetMaxMana();

                if (magicSystemMana != currentMana || magicSystemMaxMana != maxMana)
                {
                    currentMana = magicSystemMana;
                    maxMana = magicSystemMaxMana;
                    OnManaChanged?.Invoke(currentMana, maxMana);
                }
            }
        }

        /// <summary>
        /// Add health
        /// </summary>
        public void AddHealth(float amount)
        {
            if (isDead)
                return;

            currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// Remove health (take damage)
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (isDead)
                return;

            currentHealth = Mathf.Clamp(currentHealth - damage, 0f, maxHealth);
            timeSinceLastDamage = 0f;

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Trigger hit animation
            PlayerAnimationController animController = GetComponent<PlayerAnimationController>();
            if (animController != null)
            {
                animController.TriggerHit();
            }

            // Check for death
            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Add mana (only if not using MagicSystem)
        /// </summary>
        public void AddMana(float amount)
        {
            if (useMagicSystem)
            {
                if (Magic.MagicSystem.Instance != null)
                {
                    Magic.MagicSystem.Instance.AddMana(amount);
                }
                return;
            }

            currentMana = Mathf.Clamp(currentMana + amount, 0f, maxMana);
            OnManaChanged?.Invoke(currentMana, maxMana);
        }

        /// <summary>
        /// Use mana (only if not using MagicSystem)
        /// </summary>
        public bool UseMana(float amount)
        {
            if (useMagicSystem)
            {
                if (Magic.MagicSystem.Instance != null)
                {
                    return Magic.MagicSystem.Instance.UseMana(amount);
                }
                return false;
            }

            if (currentMana >= amount)
            {
                currentMana -= amount;
                OnManaChanged?.Invoke(currentMana, maxMana);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add stamina
        /// </summary>
        public void AddStamina(float amount)
        {
            currentStamina = Mathf.Clamp(currentStamina + amount, 0f, maxStamina);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }

        /// <summary>
        /// Use stamina
        /// </summary>
        public bool UseStamina(float amount)
        {
            if (currentStamina >= amount)
            {
                currentStamina -= amount;
                timeSinceLastStaminaUse = 0f;
                OnStaminaChanged?.Invoke(currentStamina, maxStamina);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Drain stamina over time (for running)
        /// </summary>
        public void DrainStamina(float drainRate)
        {
            if (currentStamina > 0f)
            {
                currentStamina = Mathf.Max(0f, currentStamina - drainRate * Time.deltaTime);
                timeSinceLastStaminaUse = 0f;
                OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            }
        }

        /// <summary>
        /// Add experience points
        /// </summary>
        public void AddExperience(int amount)
        {
            currentExp += amount;

            // Check for level up
            while (currentExp >= expToNextLevel)
            {
                LevelUp();
            }

            OnExpChanged?.Invoke(currentExp, expToNextLevel);
        }

        /// <summary>
        /// Level up player
        /// </summary>
        private void LevelUp()
        {
            level++;
            currentExp -= expToNextLevel;

            // Calculate new exp requirement
            expToNextLevel = Mathf.RoundToInt(expToNextLevel * expMultiplier);

            // Increase stats on level up
            maxHealth += 10f;
            maxMana += 10f;
            maxStamina += 5f;

            // Restore health/mana/stamina on level up
            currentHealth = maxHealth;
            currentMana = maxMana;
            currentStamina = maxStamina;

            // Trigger events
            OnLevelUp?.Invoke(level);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnManaChanged?.Invoke(currentMana, maxMana);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);

            // Play level up effects
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("level_up");
            }

            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.ShowFloatingText(
                    transform.position + Vector3.up * 2f,
                    $"Level Up! {level}",
                    Color.yellow,
                    2f
                );
            }

            Debug.Log($"[PlayerStats] Level up! Now level {level}");
        }

        /// <summary>
        /// Handle player death
        /// </summary>
        private void Die()
        {
            if (isDead)
                return;

            isDead = true;
            currentHealth = 0f;

            // Trigger death animation
            PlayerAnimationController animController = GetComponent<PlayerAnimationController>();
            if (animController != null)
            {
                animController.TriggerDeath();
            }

            // Disable player control
            PlayerController controller = GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.SetControlEnabled(false);
            }

            OnDeath?.Invoke();

            // Start respawn timer
            Invoke(nameof(Respawn), respawnDelay);

            Debug.Log("[PlayerStats] Player died!");
        }

        /// <summary>
        /// Respawn player
        /// </summary>
        private void Respawn()
        {
            isDead = false;

            // Restore stats
            currentHealth = maxHealth;
            currentMana = maxMana;
            currentStamina = maxStamina;

            // Teleport to respawn point
            if (respawnPoint != null)
            {
                PlayerController controller = GetComponent<PlayerController>();
                if (controller != null)
                {
                    controller.Teleport(respawnPoint.position, respawnPoint.rotation);
                }
                else
                {
                    transform.position = respawnPoint.position;
                    transform.rotation = respawnPoint.rotation;
                }
            }

            // Re-enable player control
            PlayerController ctrl = GetComponent<PlayerController>();
            if (ctrl != null)
            {
                ctrl.SetControlEnabled(true);
            }

            OnRespawn?.Invoke();
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnManaChanged?.Invoke(currentMana, maxMana);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);

            Debug.Log("[PlayerStats] Player respawned!");
        }

        /// <summary>
        /// Get health percentage (0-1)
        /// </summary>
        public float GetHealthPercent()
        {
            return maxHealth > 0f ? currentHealth / maxHealth : 0f;
        }

        /// <summary>
        /// Get mana percentage (0-1)
        /// </summary>
        public float GetManaPercent()
        {
            return maxMana > 0f ? currentMana / maxMana : 0f;
        }

        /// <summary>
        /// Get stamina percentage (0-1)
        /// </summary>
        public float GetStaminaPercent()
        {
            return maxStamina > 0f ? currentStamina / maxStamina : 0f;
        }

        /// <summary>
        /// Get experience percentage to next level (0-1)
        /// </summary>
        public float GetExpPercent()
        {
            return expToNextLevel > 0 ? (float)currentExp / expToNextLevel : 0f;
        }

        /// <summary>
        /// Check if player has enough stamina
        /// </summary>
        public bool HasStamina(float amount)
        {
            return currentStamina >= amount;
        }

        /// <summary>
        /// Check if player is alive
        /// </summary>
        public bool IsAlive()
        {
            return !isDead;
        }
    }
}
