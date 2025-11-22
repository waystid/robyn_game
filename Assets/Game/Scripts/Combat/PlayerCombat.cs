using UnityEngine;

namespace CozyGame.Combat
{
    /// <summary>
    /// Player combat controller.
    /// Handles player attacks, combos, and combat abilities.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Combat Settings")]
        [Tooltip("Base attack damage")]
        public float baseDamage = 15f;

        [Tooltip("Attack range")]
        public float attackRange = 2.5f;

        [Tooltip("Attack cooldown (seconds)")]
        public float attackCooldown = 0.5f;

        private float attackTimer = 0f;

        [Tooltip("Damage type")]
        public DamageType attackDamageType = DamageType.Physical;

        [Header("Combo System")]
        [Tooltip("Enable combo attacks")]
        public bool enableCombo = true;

        [Tooltip("Max combo count")]
        public int maxComboCount = 3;

        [Tooltip("Combo window (seconds)")]
        public float comboWindow = 1f;

        private int currentComboCount = 0;
        private float comboTimer = 0f;

        [Header("Critical Hits")]
        [Tooltip("Critical hit chance (0-1)")]
        [Range(0f, 1f)]
        public float criticalChance = 0.1f;

        [Tooltip("Critical damage multiplier")]
        public float criticalMultiplier = 2f;

        [Header("References")]
        [Tooltip("Attack point (raycast origin)")]
        public Transform attackPoint;

        [Tooltip("Attack layers")]
        public LayerMask attackLayers = -1;

        [Tooltip("Animator")]
        public Animator animator;

        [Header("Animation")]
        [Tooltip("Attack animation triggers")]
        public string[] attackAnimationTriggers = { "Attack1", "Attack2", "Attack3" };

        [Header("Audio")]
        [Tooltip("Attack sound")]
        public string attackSound = "player_attack";

        [Tooltip("Hit sound")]
        public string hitSound = "hit_impact";

        private void Update()
        {
            // Update attack timer
            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
            }

            // Update combo timer
            if (comboTimer > 0f)
            {
                comboTimer -= Time.deltaTime;

                if (comboTimer <= 0f)
                {
                    ResetCombo();
                }
            }

            // Attack input
            if (Input.GetButtonDown("Fire1") && attackTimer <= 0f)
            {
                PerformAttack();
            }
        }

        /// <summary>
        /// Perform attack
        /// </summary>
        private void PerformAttack()
        {
            // Calculate damage
            float finalDamage = CalculateDamage();
            bool isCritical = RollCritical();

            if (isCritical)
            {
                finalDamage *= criticalMultiplier;
            }

            // Get attack animation
            string attackTrigger = GetAttackAnimation();

            // Trigger animation
            if (animator != null && !string.IsNullOrEmpty(attackTrigger))
            {
                animator.SetTrigger(attackTrigger);
            }

            // Play sound
            PlaySound(attackSound);

            // Detect hit targets
            Collider[] hitColliders = Physics.OverlapSphere(
                attackPoint != null ? attackPoint.position : transform.position,
                attackRange,
                attackLayers
            );

            bool hitSomething = false;

            foreach (var collider in hitColliders)
            {
                if (collider.gameObject == gameObject)
                    continue;

                Health targetHealth = collider.GetComponent<Health>();
                if (targetHealth != null)
                {
                    DamageInfo damageInfo = new DamageInfo(
                        finalDamage,
                        attackDamageType,
                        gameObject,
                        collider.gameObject,
                        collider.ClosestPoint(transform.position),
                        (collider.transform.position - transform.position).normalized
                    );

                    damageInfo.isCritical = isCritical;

                    targetHealth.TakeDamage(damageInfo);

                    hitSomething = true;
                }
            }

            if (hitSomething)
            {
                PlaySound(hitSound);
            }

            // Update combo
            if (enableCombo)
            {
                currentComboCount++;
                if (currentComboCount > maxComboCount)
                {
                    currentComboCount = 1;
                }
                comboTimer = comboWindow;
            }

            // Set cooldown
            attackTimer = attackCooldown;
        }

        /// <summary>
        /// Calculate attack damage
        /// </summary>
        private float CalculateDamage()
        {
            float damage = baseDamage;

            // Add weapon damage
            if (Inventory.EquipmentSystem.Instance != null)
            {
                damage += Inventory.EquipmentSystem.Instance.GetTotalDamage();
            }

            // Add player level scaling
            if (PlayerStats.Instance != null)
            {
                damage += PlayerStats.Instance.level * 2f;
            }

            // Combo multiplier
            if (enableCombo && currentComboCount > 1)
            {
                damage *= (1f + (currentComboCount - 1) * 0.1f); // +10% per combo
            }

            return damage;
        }

        /// <summary>
        /// Roll for critical hit
        /// </summary>
        private bool RollCritical()
        {
            return Random.value <= criticalChance;
        }

        /// <summary>
        /// Get attack animation based on combo
        /// </summary>
        private string GetAttackAnimation()
        {
            if (attackAnimationTriggers == null || attackAnimationTriggers.Length == 0)
                return "";

            if (!enableCombo || currentComboCount <= 0)
                return attackAnimationTriggers[0];

            int index = Mathf.Clamp(currentComboCount - 1, 0, attackAnimationTriggers.Length - 1);
            return attackAnimationTriggers[index];
        }

        /// <summary>
        /// Reset combo
        /// </summary>
        private void ResetCombo()
        {
            currentComboCount = 0;
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
            // Draw attack range
            Gizmos.color = Color.red;
            Vector3 attackPos = attackPoint != null ? attackPoint.position : transform.position;
            Gizmos.DrawWireSphere(attackPos, attackRange);
        }
    }
}
