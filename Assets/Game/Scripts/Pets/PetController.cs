using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using CozyGame.Combat;

namespace CozyGame.Pets
{
    /// <summary>
    /// Pet AI state
    /// </summary>
    public enum PetState
    {
        Idle,
        Following,
        Attacking,
        Searching,
        Resting,
        Dead
    }

    /// <summary>
    /// Pet controller.
    /// Manages individual pet instance with stats, AI, and abilities.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class PetController : MonoBehaviour
    {
        [Header("Pet Data")]
        [Tooltip("Pet definition")]
        public Pet petData;

        [Tooltip("Pet instance ID (unique per save)")]
        public string instanceID;

        [Tooltip("Custom pet name")]
        public string customName;

        [Header("Stats")]
        [Tooltip("Current level")]
        public int level = 1;

        [Tooltip("Current experience")]
        public int currentExp = 0;

        [Tooltip("Current health")]
        public float currentHealth = 50f;

        [Tooltip("Current hunger (0-100)")]
        public float currentHunger = 100f;

        [Tooltip("Current happiness (0-100)")]
        public float currentHappiness = 100f;

        [Tooltip("Loyalty (0-100, affects obedience)")]
        public float loyalty = 50f;

        [Header("Customization")]
        [Tooltip("Equipped accessory")]
        public string equippedAccessory = "";

        [Header("State")]
        [Tooltip("Current AI state")]
        public PetState currentState = PetState.Idle;

        [Tooltip("Is active (summoned)")]
        public bool isActive = true;

        [Header("References")]
        [Tooltip("Owner (player)")]
        public Transform owner;

        [Tooltip("Animator")]
        public Animator animator;

        [Tooltip("Health component")]
        public Health health;

        [Header("Events")]
        public UnityEvent<int> OnLevelUp;
        public UnityEvent OnHungry;
        public UnityEvent OnHappy;
        public UnityEvent OnDeath;

        // Components
        private NavMeshAgent navAgent;

        // AI
        private float abilityCooldownTimer = 0f;
        private Transform currentTarget;

        // Timers
        private float idleSoundTimer = 0f;
        private const float idleSoundInterval = 10f;

        private void Awake()
        {
            navAgent = GetComponent<NavMeshAgent>();
            health = GetComponent<Health>();

            // Generate instance ID if empty
            if (string.IsNullOrEmpty(instanceID))
            {
                instanceID = System.Guid.NewGuid().ToString();
            }
        }

        private void Start()
        {
            // Initialize from pet data
            if (petData != null)
            {
                currentHealth = petData.baseHealth;
                currentHunger = petData.maxHunger;
                currentHappiness = petData.maxHappiness;

                if (navAgent != null)
                {
                    navAgent.speed = petData.moveSpeed;
                }

                if (health != null)
                {
                    health.maxHealth = petData.baseHealth;
                    health.currentHealth = currentHealth;
                    health.OnDeath.AddListener(OnPetDeath);
                }
            }

            // Find owner (player)
            if (owner == null && PlayerController.Instance != null)
            {
                owner = PlayerController.Instance.transform;
            }
        }

        private void Update()
        {
            if (!isActive || petData == null)
                return;

            // Update needs
            UpdateNeeds();

            // Update ability cooldown
            if (abilityCooldownTimer > 0f)
            {
                abilityCooldownTimer -= Time.deltaTime;
            }

            // Update AI state
            UpdateAI();

            // Update animator
            UpdateAnimator();

            // Idle sounds
            idleSoundTimer += Time.deltaTime;
            if (idleSoundTimer >= idleSoundInterval)
            {
                PlayIdleSound();
                idleSoundTimer = 0f;
            }
        }

        /// <summary>
        /// Update pet needs (hunger, happiness)
        /// </summary>
        private void UpdateNeeds()
        {
            // Decrease hunger
            currentHunger -= petData.hungerDecreaseRate * Time.deltaTime;
            currentHunger = Mathf.Clamp(currentHunger, 0f, petData.maxHunger);

            // Decrease happiness
            currentHappiness -= petData.happinessDecreaseRate * Time.deltaTime;
            currentHappiness = Mathf.Clamp(currentHappiness, 0f, petData.maxHappiness);

            // Hunger affects happiness
            if (currentHunger < 20f)
            {
                currentHappiness -= 0.5f * Time.deltaTime;

                if (currentHunger <= 0f)
                {
                    OnHungry?.Invoke();
                    PlaySound(petData.hungrySound);
                }
            }

            // Low happiness affects loyalty
            if (currentHappiness < 20f)
            {
                loyalty -= 0.1f * Time.deltaTime;
                loyalty = Mathf.Clamp(loyalty, 0f, 100f);
            }
            else if (currentHappiness > 80f)
            {
                loyalty += 0.05f * Time.deltaTime;
                loyalty = Mathf.Clamp(loyalty, 0f, 100f);
            }
        }

        /// <summary>
        /// Update AI behavior
        /// </summary>
        private void UpdateAI()
        {
            if (health != null && health.isDead)
            {
                currentState = PetState.Dead;
                return;
            }

            switch (currentState)
            {
                case PetState.Idle:
                    UpdateIdle();
                    break;

                case PetState.Following:
                    UpdateFollowing();
                    break;

                case PetState.Attacking:
                    UpdateAttacking();
                    break;

                case PetState.Searching:
                    UpdateSearching();
                    break;

                case PetState.Resting:
                    UpdateResting();
                    break;
            }
        }

        /// <summary>
        /// Idle state
        /// </summary>
        private void UpdateIdle()
        {
            // Check if owner is far away
            if (owner != null)
            {
                float distanceToOwner = Vector3.Distance(transform.position, owner.position);

                if (distanceToOwner > petData.followDistance * 2f)
                {
                    ChangeState(PetState.Following);
                    return;
                }
            }

            // Use abilities if available
            if (abilityCooldownTimer <= 0f)
            {
                TryUseAbility();
            }
        }

        /// <summary>
        /// Following state
        /// </summary>
        private void UpdateFollowing()
        {
            if (owner == null)
            {
                ChangeState(PetState.Idle);
                return;
            }

            float distanceToOwner = Vector3.Distance(transform.position, owner.position);

            // Stop following if close enough
            if (distanceToOwner <= petData.followDistance)
            {
                if (navAgent != null)
                {
                    navAgent.ResetPath();
                }
                ChangeState(PetState.Idle);
                return;
            }

            // Follow owner
            if (navAgent != null)
            {
                navAgent.SetDestination(owner.position);
            }
        }

        /// <summary>
        /// Attacking state
        /// </summary>
        private void UpdateAttacking()
        {
            if (currentTarget == null || !petData.HasAbility(PetAbility.Fight))
            {
                ChangeState(PetState.Following);
                return;
            }

            // Check if target is still alive
            Health targetHealth = currentTarget.GetComponent<Health>();
            if (targetHealth != null && targetHealth.isDead)
            {
                currentTarget = null;
                ChangeState(PetState.Following);
                return;
            }

            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

            // Attack if in range
            if (distanceToTarget <= 2f)
            {
                PerformAttack();
            }
            else
            {
                // Chase target
                if (navAgent != null)
                {
                    navAgent.SetDestination(currentTarget.position);
                }
            }
        }

        /// <summary>
        /// Searching state
        /// </summary>
        private void UpdateSearching()
        {
            // Searching for items/plants
            // This would integrate with item spawners or plant locations
            // For now, just return to idle after a bit
            ChangeState(PetState.Idle);
        }

        /// <summary>
        /// Resting state
        /// </summary>
        private void UpdateResting()
        {
            // Resting slowly recovers happiness
            currentHappiness += 5f * Time.deltaTime;
            currentHappiness = Mathf.Clamp(currentHappiness, 0f, petData.maxHappiness);

            // Stop movement
            if (navAgent != null)
            {
                navAgent.ResetPath();
            }
        }

        /// <summary>
        /// Change AI state
        /// </summary>
        private void ChangeState(PetState newState)
        {
            if (currentState == newState)
                return;

            currentState = newState;
        }

        /// <summary>
        /// Try to use pet abilities
        /// </summary>
        private void TryUseAbility()
        {
            if (petData == null || petData.abilities == null)
                return;

            // Randomly pick an ability to use
            if (petData.abilities.Length > 0)
            {
                PetAbility ability = petData.abilities[Random.Range(0, petData.abilities.Length)];
                UseAbility(ability);
            }
        }

        /// <summary>
        /// Use specific ability
        /// </summary>
        public void UseAbility(PetAbility ability)
        {
            switch (ability)
            {
                case PetAbility.FindItems:
                    FindNearbyItems();
                    break;

                case PetAbility.FindPlants:
                    FindNearbyPlants();
                    break;

                case PetAbility.Fight:
                    FindAndAttackEnemy();
                    break;

                case PetAbility.Heal:
                    HealOwner();
                    break;

                case PetAbility.Gather:
                    GatherNearbyItems();
                    break;
            }

            abilityCooldownTimer = petData.abilityCooldown;
        }

        /// <summary>
        /// Find nearby items
        /// </summary>
        private void FindNearbyItems()
        {
            // Detect nearby item pickups
            Collider[] colliders = Physics.OverlapSphere(transform.position, 15f);

            foreach (var collider in colliders)
            {
                Inventory.ItemPickup item = collider.GetComponent<Inventory.ItemPickup>();
                if (item != null)
                {
                    // Bark/indicate item location
                    PlaySound(petData.happySound);
                    // TODO: Add visual indicator (particle effect, arrow, etc.)
                    break;
                }
            }
        }

        /// <summary>
        /// Find nearby plants
        /// </summary>
        private void FindNearbyPlants()
        {
            // Similar to FindNearbyItems but for plants
            // TODO: Integrate with plant system
        }

        /// <summary>
        /// Find and attack nearest enemy
        /// </summary>
        private void FindAndAttackEnemy()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 10f);

            float closestDistance = float.MaxValue;
            Transform closestEnemy = null;

            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Enemy"))
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = collider.transform;
                    }
                }
            }

            if (closestEnemy != null)
            {
                currentTarget = closestEnemy;
                ChangeState(PetState.Attacking);
            }
        }

        /// <summary>
        /// Perform attack on current target
        /// </summary>
        private void PerformAttack()
        {
            if (currentTarget == null)
                return;

            Health targetHealth = currentTarget.GetComponent<Health>();
            if (targetHealth != null)
            {
                float damage = petData.baseDamage + (level * 2f);

                DamageInfo damageInfo = new DamageInfo(
                    damage,
                    DamageType.Physical,
                    gameObject,
                    currentTarget.gameObject
                );

                targetHealth.TakeDamage(damageInfo);
            }

            // Trigger attack animation
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }

        /// <summary>
        /// Heal owner
        /// </summary>
        private void HealOwner()
        {
            if (owner == null)
                return;

            Health ownerHealth = owner.GetComponent<Health>();
            if (ownerHealth != null)
            {
                float healAmount = 20f + (level * 5f);
                ownerHealth.Heal(healAmount);

                PlaySound(petData.happySound);

                // Spawn heal effect
                if (VFX.ParticleEffectManager.Instance != null)
                {
                    VFX.ParticleEffectManager.Instance.SpawnEffect(
                        VFX.EffectType.Heal,
                        owner.position,
                        Quaternion.identity
                    );
                }
            }
        }

        /// <summary>
        /// Gather nearby items
        /// </summary>
        private void GatherNearbyItems()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);

            foreach (var collider in colliders)
            {
                Inventory.ItemPickup item = collider.GetComponent<Inventory.ItemPickup>();
                if (item != null && item.autoPickup == false)
                {
                    // Auto-collect the item
                    item.autoPickup = true;
                    PlaySound(petData.happySound);
                }
            }
        }

        /// <summary>
        /// Feed pet
        /// </summary>
        public void Feed(float amount)
        {
            currentHunger += amount;
            currentHunger = Mathf.Clamp(currentHunger, 0f, petData.maxHunger);

            // Increase happiness when fed
            currentHappiness += amount * 0.5f;
            currentHappiness = Mathf.Clamp(currentHappiness, 0f, petData.maxHappiness);

            PlaySound(petData.happySound);
        }

        /// <summary>
        /// Pet the pet (increase happiness)
        /// </summary>
        public void PetPet()
        {
            currentHappiness += 10f;
            currentHappiness = Mathf.Clamp(currentHappiness, 0f, petData.maxHappiness);

            loyalty += 1f;
            loyalty = Mathf.Clamp(loyalty, 0f, 100f);

            PlaySound(petData.happySound);
            OnHappy?.Invoke();

            // Spawn sparkle effect
            if (VFX.ParticleEffectManager.Instance != null)
            {
                VFX.ParticleEffectManager.Instance.SpawnEffect(
                    VFX.EffectType.Sparkle,
                    transform.position + Vector3.up,
                    Quaternion.identity
                );
            }
        }

        /// <summary>
        /// Add experience
        /// </summary>
        public void AddExp(int exp)
        {
            if (!petData.canLevelUp || level >= petData.maxLevel)
                return;

            currentExp += exp;

            int expRequired = petData.GetExpRequiredForLevel(level + 1);

            while (currentExp >= expRequired && level < petData.maxLevel)
            {
                LevelUp();
                currentExp -= expRequired;
                expRequired = petData.GetExpRequiredForLevel(level + 1);
            }
        }

        /// <summary>
        /// Level up
        /// </summary>
        private void LevelUp()
        {
            level++;

            // Increase stats
            currentHealth = petData.baseHealth + (level * 10f);
            if (health != null)
            {
                health.SetMaxHealth(currentHealth, true);
            }

            OnLevelUp?.Invoke(level);
            PlaySound(petData.happySound);

            // Spawn level up effect
            if (VFX.ParticleEffectManager.Instance != null)
            {
                VFX.ParticleEffectManager.Instance.SpawnEffect(
                    VFX.EffectType.LevelUp,
                    transform.position,
                    Quaternion.identity
                );
            }
        }

        /// <summary>
        /// Rename pet
        /// </summary>
        public bool Rename(string newName)
        {
            if (!petData.canRename)
                return false;

            customName = newName;
            return true;
        }

        /// <summary>
        /// Get display name
        /// </summary>
        public string GetDisplayName()
        {
            return string.IsNullOrEmpty(customName) ? petData.petName : customName;
        }

        /// <summary>
        /// Pet death callback
        /// </summary>
        private void OnPetDeath()
        {
            currentState = PetState.Dead;
            OnDeath?.Invoke();

            // Stop NavMeshAgent
            if (navAgent != null)
            {
                navAgent.ResetPath();
                navAgent.enabled = false;
            }
        }

        /// <summary>
        /// Update animator
        /// </summary>
        private void UpdateAnimator()
        {
            if (animator == null)
                return;

            float speed = navAgent != null ? navAgent.velocity.magnitude : 0f;
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsResting", currentState == PetState.Resting);
            animator.SetBool("IsDead", currentState == PetState.Dead);
        }

        /// <summary>
        /// Play idle sound
        /// </summary>
        private void PlayIdleSound()
        {
            PlaySound(petData.idleSound);
        }

        /// <summary>
        /// Play sound
        /// </summary>
        private void PlaySound(string soundName)
        {
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(soundName))
            {
                AudioManager.Instance.PlaySound(soundName);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (petData == null)
                return;

            // Draw follow distance
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, petData.followDistance);

            // Draw search radius for abilities
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 10f);
        }
    }
}
