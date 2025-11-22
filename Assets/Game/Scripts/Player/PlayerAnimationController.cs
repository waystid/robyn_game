using UnityEngine;

namespace CozyGame
{
    /// <summary>
    /// Controls player character animations.
    /// Manages animation states for locomotion, actions, and interactions.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Animation Parameters")]
        [Tooltip("Name of Speed parameter in animator (float)")]
        public string speedParameter = "Speed";

        [Tooltip("Name of IsGrounded parameter in animator (bool)")]
        public string groundedParameter = "IsGrounded";

        [Tooltip("Name of IsRunning parameter in animator (bool)")]
        public string runningParameter = "IsRunning";

        [Tooltip("Name of Jump trigger in animator")]
        public string jumpTrigger = "Jump";

        [Tooltip("Name of Attack trigger in animator")]
        public string attackTrigger = "Attack";

        [Tooltip("Name of Cast trigger in animator")]
        public string castTrigger = "Cast";

        [Tooltip("Name of Harvest trigger in animator")]
        public string harvestTrigger = "Harvest";

        [Tooltip("Name of Interact trigger in animator")]
        public string interactTrigger = "Interact";

        [Tooltip("Name of Hit trigger in animator (when damaged)")]
        public string hitTrigger = "Hit";

        [Tooltip("Name of Death trigger in animator")]
        public string deathTrigger = "Death";

        [Header("Blend Settings")]
        [Tooltip("Smoothing time for speed changes")]
        public float speedSmoothTime = 0.1f;

        [Tooltip("Minimum speed to trigger walk animation")]
        public float minWalkSpeed = 0.1f;

        [Header("Advanced")]
        [Tooltip("Use root motion for character movement")]
        public bool useRootMotion = false;

        [Tooltip("Override animation layer weights at runtime")]
        public bool manageLayerWeights = false;

        // Components
        private Animator animator;

        // Animation state
        private float currentSpeed;
        private float targetSpeed;
        private float speedVelocity;

        // Animation hashes (for performance)
        private int speedHash;
        private int groundedHash;
        private int runningHash;
        private int jumpHash;
        private int attackHash;
        private int castHash;
        private int harvestHash;
        private int interactHash;
        private int hitHash;
        private int deathHash;

        private void Awake()
        {
            animator = GetComponent<Animator>();

            // Cache parameter hashes
            CacheAnimationHashes();

            // Configure root motion
            if (animator != null)
            {
                animator.applyRootMotion = useRootMotion;
            }
        }

        private void Start()
        {
            // Initialize animation state
            if (animator != null)
            {
                animator.SetFloat(speedHash, 0f);
                animator.SetBool(groundedHash, true);
                animator.SetBool(runningHash, false);
            }
        }

        /// <summary>
        /// Cache animation parameter hashes for performance
        /// </summary>
        private void CacheAnimationHashes()
        {
            speedHash = Animator.StringToHash(speedParameter);
            groundedHash = Animator.StringToHash(groundedParameter);
            runningHash = Animator.StringToHash(runningParameter);
            jumpHash = Animator.StringToHash(jumpTrigger);
            attackHash = Animator.StringToHash(attackTrigger);
            castHash = Animator.StringToHash(castTrigger);
            harvestHash = Animator.StringToHash(harvestTrigger);
            interactHash = Animator.StringToHash(interactTrigger);
            hitHash = Animator.StringToHash(hitTrigger);
            deathHash = Animator.StringToHash(deathTrigger);
        }

        private void Update()
        {
            UpdateSpeedSmoothing();
        }

        /// <summary>
        /// Smooth speed parameter changes
        /// </summary>
        private void UpdateSpeedSmoothing()
        {
            if (animator == null)
                return;

            // Smoothly transition to target speed
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, speedSmoothTime);

            // Apply to animator
            animator.SetFloat(speedHash, currentSpeed);
        }

        /// <summary>
        /// Set movement speed (0 = idle, 1 = walk, 2 = run)
        /// </summary>
        public void SetMovementSpeed(float speed)
        {
            targetSpeed = speed;
        }

        /// <summary>
        /// Set grounded state
        /// </summary>
        public void SetGrounded(bool grounded)
        {
            if (animator != null)
            {
                animator.SetBool(groundedHash, grounded);
            }
        }

        /// <summary>
        /// Set running state
        /// </summary>
        public void SetRunning(bool running)
        {
            if (animator != null)
            {
                animator.SetBool(runningHash, running);
            }
        }

        /// <summary>
        /// Trigger jump animation
        /// </summary>
        public void TriggerJump()
        {
            if (animator != null)
            {
                animator.SetTrigger(jumpHash);
            }
        }

        /// <summary>
        /// Trigger attack animation
        /// </summary>
        public void TriggerAttack()
        {
            if (animator != null)
            {
                animator.SetTrigger(attackHash);
            }
        }

        /// <summary>
        /// Trigger spell cast animation
        /// </summary>
        public void TriggerCast()
        {
            if (animator != null)
            {
                animator.SetTrigger(castHash);
            }
        }

        /// <summary>
        /// Trigger harvest animation
        /// </summary>
        public void TriggerHarvest()
        {
            if (animator != null)
            {
                animator.SetTrigger(harvestHash);
            }
        }

        /// <summary>
        /// Trigger interact animation
        /// </summary>
        public void TriggerInteract()
        {
            if (animator != null)
            {
                animator.SetTrigger(interactHash);
            }
        }

        /// <summary>
        /// Trigger hit/damaged animation
        /// </summary>
        public void TriggerHit()
        {
            if (animator != null)
            {
                animator.SetTrigger(hitHash);
            }
        }

        /// <summary>
        /// Trigger death animation
        /// </summary>
        public void TriggerDeath()
        {
            if (animator != null)
            {
                animator.SetTrigger(deathHash);
            }
        }

        /// <summary>
        /// Set a custom bool parameter
        /// </summary>
        public void SetBool(string parameterName, bool value)
        {
            if (animator != null && animator.HasParameterWithName(parameterName))
            {
                animator.SetBool(parameterName, value);
            }
        }

        /// <summary>
        /// Set a custom int parameter
        /// </summary>
        public void SetInt(string parameterName, int value)
        {
            if (animator != null && animator.HasParameterWithName(parameterName))
            {
                animator.SetInt(parameterName, value);
            }
        }

        /// <summary>
        /// Set a custom float parameter
        /// </summary>
        public void SetFloat(string parameterName, float value)
        {
            if (animator != null && animator.HasParameterWithName(parameterName))
            {
                animator.SetFloat(parameterName, value);
            }
        }

        /// <summary>
        /// Trigger a custom trigger parameter
        /// </summary>
        public void SetTrigger(string parameterName)
        {
            if (animator != null && animator.HasParameterWithName(parameterName))
            {
                animator.SetTrigger(parameterName);
            }
        }

        /// <summary>
        /// Get current animation state info
        /// </summary>
        public AnimatorStateInfo GetCurrentStateInfo(int layerIndex = 0)
        {
            if (animator != null)
            {
                return animator.GetCurrentAnimatorStateInfo(layerIndex);
            }

            return default(AnimatorStateInfo);
        }

        /// <summary>
        /// Check if animator is in a specific state
        /// </summary>
        public bool IsInState(string stateName, int layerIndex = 0)
        {
            if (animator != null)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
                return stateInfo.IsName(stateName);
            }

            return false;
        }

        /// <summary>
        /// Check if animator is currently transitioning
        /// </summary>
        public bool IsInTransition(int layerIndex = 0)
        {
            if (animator != null)
            {
                return animator.IsInTransition(layerIndex);
            }

            return false;
        }

        /// <summary>
        /// Get animator component
        /// </summary>
        public Animator GetAnimator()
        {
            return animator;
        }

        /// <summary>
        /// Set animation layer weight
        /// </summary>
        public void SetLayerWeight(int layerIndex, float weight)
        {
            if (animator != null && layerIndex < animator.layerCount)
            {
                animator.SetLayerWeight(layerIndex, weight);
            }
        }

        /// <summary>
        /// Play animation by name
        /// </summary>
        public void PlayAnimation(string stateName, int layerIndex = 0, float normalizedTime = 0f)
        {
            if (animator != null)
            {
                animator.Play(stateName, layerIndex, normalizedTime);
            }
        }

        /// <summary>
        /// Crossfade to animation
        /// </summary>
        public void CrossfadeAnimation(string stateName, float duration = 0.2f, int layerIndex = 0)
        {
            if (animator != null)
            {
                animator.CrossFade(stateName, duration, layerIndex);
            }
        }
    }

    /// <summary>
    /// Extension methods for Animator
    /// </summary>
    public static class AnimatorExtensions
    {
        /// <summary>
        /// Check if animator has a specific parameter
        /// </summary>
        public static bool HasParameterWithName(this Animator animator, string parameterName)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == parameterName)
                    return true;
            }
            return false;
        }
    }
}
