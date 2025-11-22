using UnityEngine;
using System.Collections.Generic;

namespace CozyGame.Magic
{
    /// <summary>
    /// Harvest Spell - automatically harvests all plants in an area.
    /// Detects harvestable plants within radius and collects their yield.
    /// </summary>
    public class HarvestSpell : MonoBehaviour
    {
        [Header("Spell Settings")]
        [Tooltip("Spell data reference")]
        public SpellData harvestSpellData;

        [Tooltip("Harvest radius (area of effect)")]
        public float harvestRadius = 5f;

        [Tooltip("Maximum number of plants to harvest per cast")]
        public int maxPlantsPerCast = 10;

        [Header("Visual Feedback")]
        [Tooltip("Particle effect when harvesting")]
        public GameObject harvestParticlePrefab;

        [Tooltip("Duration of particle effect")]
        public float particleDuration = 1.5f;

        [Tooltip("Harvest effect color")]
        public Color harvestColor = new Color(1f, 0.84f, 0f); // Gold

        [Header("Audio")]
        [Tooltip("Sound when spell is cast")]
        public string harvestSoundName = "spell_harvest";

        [Header("Targeting")]
        [Tooltip("Maximum range to cast spell")]
        public float maxRange = 15f;

        [Tooltip("Layers that can be targeted")]
        public LayerMask targetLayers = -1;

        /// <summary>
        /// Cast harvest spell at mouse/touch position
        /// Returns number of plants harvested
        /// </summary>
        public int CastHarvestSpell()
        {
            // Check if we have a magic system
            if (MagicSystem.Instance == null)
            {
                Debug.LogWarning("MagicSystem not found in scene!");
                return 0;
            }

            // Get target position from input
            Vector3 targetPosition = GetTargetPosition();
            if (targetPosition == Vector3.zero)
            {
                Debug.Log("No valid target position");
                return 0;
            }

            // Check mana first
            float manaCost = harvestSpellData != null ? harvestSpellData.manaCost : 30f;
            if (!MagicSystem.Instance.UseMana(manaCost))
            {
                Debug.Log("Not enough mana!");
                return 0;
            }

            // Harvest plants in area
            int harvested = HarvestPlantsInArea(targetPosition, harvestRadius);

            if (harvested > 0)
            {
                // Mark spell as cast
                if (harvestSpellData != null)
                {
                    harvestSpellData.MarkAsCast();
                }

                // Play effects at center
                PlayHarvestEffects(targetPosition);

                Debug.Log($"Harvested {harvested} plants!");
            }
            else
            {
                // Refund mana if nothing was harvested
                MagicSystem.Instance.RestoreMana(manaCost);
                Debug.Log("No harvestable plants in area");
            }

            return harvested;
        }

        /// <summary>
        /// Harvest all plants in radius
        /// </summary>
        private int HarvestPlantsInArea(Vector3 center, float radius)
        {
            int plantsHarvested = 0;

            // Find all colliders in radius
            Collider[] colliders = Physics.OverlapSphere(center, radius, targetLayers);

            foreach (Collider col in colliders)
            {
                if (plantsHarvested >= maxPlantsPerCast)
                    break;

                GameObject target = col.gameObject;

                // Try to harvest this plant
                if (TryHarvestPlant(target, col.transform.position))
                {
                    plantsHarvested++;
                }
            }

            return plantsHarvested;
        }

        /// <summary>
        /// Try to harvest a single plant
        /// </summary>
        private bool TryHarvestPlant(GameObject target, Vector3 position)
        {
            // Check if it's a plant or harvestable
            if (!IsHarvestable(target))
                return false;

            // TODO: Integrate with actual plant/farming system
            // Example integration:
            /*
            Plant plant = target.GetComponent<Plant>();
            if (plant != null && plant.IsReadyToHarvest())
            {
                plant.Harvest();

                // Add items to inventory
                if (Inventory.InventorySystem.Instance != null)
                {
                    Inventory.InventorySystem.Instance.AddItem(plant.harvestItemID, plant.harvestYield);
                }

                return true;
            }
            */

            // Temporary: just detect plants
            if (target.name.ToLower().Contains("plant") || target.tag == "Plant")
            {
                Debug.Log($"Harvesting plant: {target.name}");

                // Play individual harvest effect
                PlayHarvestEffects(position);

                // Show floating text
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.Show("+Harvest", position, harvestColor);
                }

                // Spawn particle effect from VFX system
                if (VFX.ParticleEffectManager.Instance != null)
                {
                    VFX.ParticleEffectManager.Instance.SpawnEffect(VFX.EffectType.PlantHarvest, position);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if target is harvestable
        /// </summary>
        private bool IsHarvestable(GameObject target)
        {
            // Check for plant tag
            if (target.tag == "Plant")
                return true;

            // Check for plant in name
            if (target.name.ToLower().Contains("plant"))
                return true;

            // Check for harvestable component
            // TODO: Create IHarvestable interface for better integration
            // if (target.GetComponent<IHarvestable>() != null)
            //     return true;

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
        /// Play visual and audio effects for harvest spell
        /// </summary>
        private void PlayHarvestEffects(Vector3 position)
        {
            // Spawn particle effect
            if (harvestParticlePrefab != null)
            {
                GameObject particle = Instantiate(harvestParticlePrefab, position, Quaternion.identity);
                Destroy(particle, particleDuration);
            }

            // Play sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(harvestSoundName))
            {
                AudioManager.Instance.PlaySoundAtPosition(harvestSoundName, position);
            }
        }

        /// <summary>
        /// Cast harvest spell at specific position (for AI or scripted events)
        /// </summary>
        public int CastAtPosition(Vector3 worldPosition)
        {
            // Check mana
            float manaCost = harvestSpellData != null ? harvestSpellData.manaCost : 30f;
            if (MagicSystem.Instance != null && !MagicSystem.Instance.UseMana(manaCost))
            {
                return 0;
            }

            int harvested = HarvestPlantsInArea(worldPosition, harvestRadius);

            if (harvested > 0)
            {
                if (harvestSpellData != null)
                {
                    harvestSpellData.MarkAsCast();
                }

                PlayHarvestEffects(worldPosition);
            }
            else if (MagicSystem.Instance != null)
            {
                // Refund mana
                MagicSystem.Instance.RestoreMana(manaCost);
            }

            return harvested;
        }

        // Visualize casting range and harvest radius in editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, maxRange);

            Gizmos.color = harvestColor;
            Gizmos.DrawWireSphere(transform.position, harvestRadius);
        }
    }
}
