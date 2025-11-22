using UnityEngine;

namespace CozyGame.Magic
{
    /// <summary>
    /// Growth Spell - makes plants grow faster or advance to next stage
    /// This is a standalone component that can be attached to the magic wand
    /// or used as a reference for Survival Engine integration
    /// </summary>
    public class GrowthSpell : MonoBehaviour
    {
        [Header("Spell Settings")]
        [Tooltip("Spell data reference")]
        public SpellData growthSpellData;

        [Tooltip("How much to accelerate plant growth (multiplier)")]
        public float growthBoost = 2f;

        [Tooltip("Should spell instantly advance plant to next stage?")]
        public bool instantGrowth = true;

        [Header("Visual Feedback")]
        [Tooltip("Particle effect when spell hits plant")]
        public GameObject growthParticlePrefab;

        [Tooltip("Duration of particle effect")]
        public float particleDuration = 2f;

        [Header("Audio")]
        [Tooltip("Sound when spell is cast")]
        public string growthSoundName = "spell_growth";

        [Header("Targeting")]
        [Tooltip("Maximum range to cast spell")]
        public float maxRange = 10f;

        [Tooltip("Layers that can be targeted")]
        public LayerMask targetLayers = -1;

        /// <summary>
        /// Cast growth spell at mouse/touch position
        /// Returns true if cast was successful
        /// </summary>
        public bool CastGrowthSpell()
        {
            // Check if we have a magic system
            if (MagicSystem.Instance == null)
            {
                Debug.LogWarning("MagicSystem not found in scene!");
                return false;
            }

            // Get target position from input
            Vector3 targetPosition = GetTargetPosition();
            if (targetPosition == Vector3.zero)
            {
                Debug.Log("No valid target position");
                return false;
            }

            // Raycast to find what we're targeting
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxRange, targetLayers))
            {
                // Check if it's a plant
                GameObject target = hit.collider.gameObject;

                // Try to grow the plant
                if (TryGrowPlant(target, hit.point))
                {
                    // Use mana through magic system
                    if (growthSpellData != null)
                    {
                        MagicSystem.Instance.CastSpell(growthSpellData, hit.point);
                    }
                    else
                    {
                        // Fallback: use mana directly
                        if (!MagicSystem.Instance.UseMana(20f))
                        {
                            return false;
                        }
                    }

                    // Visual and audio feedback
                    PlayGrowthEffects(hit.point);
                    return true;
                }
            }

            Debug.Log("No plant targeted");
            return false;
        }

        /// <summary>
        /// Try to grow a plant at target
        /// </summary>
        private bool TryGrowPlant(GameObject target, Vector3 position)
        {
            // TODO: Integrate with Survival Engine Plant system
            // Example integration:
            /*
            Plant plant = target.GetComponent<Plant>();
            if (plant != null)
            {
                if (instantGrowth)
                {
                    plant.GrowPlant(); // Advance to next stage
                }
                else
                {
                    plant.growth_time *= growthBoost; // Speed up growth
                }
                return true;
            }
            */

            // For now, check if it has "Plant" in name or tag
            if (target.name.ToLower().Contains("plant") || target.tag == "Plant")
            {
                Debug.Log($"Growing plant: {target.name}");

                // Show floating text
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.Show("Growth!", position, Color.green);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get target position from input
        /// </summary>
        private Vector3 GetTargetPosition()
        {
            if (InputManager.Instance != null)
            {
                return InputManager.Instance.GetPointerWorldPosition();
            }

            // Fallback: use mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxRange))
            {
                return hit.point;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Play visual and audio effects for growth spell
        /// </summary>
        private void PlayGrowthEffects(Vector3 position)
        {
            // Spawn particle effect
            if (growthParticlePrefab != null)
            {
                GameObject particle = Instantiate(growthParticlePrefab, position, Quaternion.identity);
                Destroy(particle, particleDuration);
            }

            // Play sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(growthSoundName))
            {
                AudioManager.Instance.PlaySoundAtPosition(growthSoundName, position);
            }

            // Show floating text
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.ShowCompletion("✨ Growth!", position);
            }
        }

        /// <summary>
        /// Cast growth spell at specific position (for AI or scripted events)
        /// </summary>
        public bool CastAtPosition(Vector3 worldPosition)
        {
            // Raycast down from above the position to find target
            Vector3 rayStart = worldPosition + Vector3.up * 5f;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 10f, targetLayers))
            {
                if (TryGrowPlant(hit.collider.gameObject, hit.point))
                {
                    // Use mana
                    if (MagicSystem.Instance != null)
                    {
                        if (growthSpellData != null)
                        {
                            MagicSystem.Instance.CastSpell(growthSpellData, hit.point);
                        }
                        else
                        {
                            MagicSystem.Instance.UseMana(20f);
                        }
                    }

                    PlayGrowthEffects(hit.point);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Cast growth spell on all plants in radius
        /// </summary>
        public int CastAreaGrowth(Vector3 center, float radius)
        {
            int plantsGrown = 0;

            // Find all colliders in radius
            Collider[] colliders = Physics.OverlapSphere(center, radius, targetLayers);

            foreach (Collider col in colliders)
            {
                if (col.gameObject.name.ToLower().Contains("plant") || col.gameObject.tag == "Plant")
                {
                    if (TryGrowPlant(col.gameObject, col.transform.position))
                    {
                        plantsGrown++;
                        PlayGrowthEffects(col.transform.position);
                    }
                }
            }

            if (plantsGrown > 0)
            {
                // Use mana based on number of plants
                float totalManaCost = growthSpellData != null ? growthSpellData.manaCost * plantsGrown : 20f * plantsGrown;

                if (MagicSystem.Instance != null)
                {
                    MagicSystem.Instance.UseMana(totalManaCost);
                }

                Debug.Log($"Grew {plantsGrown} plants in area!");
            }

            return plantsGrown;
        }

        // Visualize casting range in editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, maxRange);
        }
    }
}
