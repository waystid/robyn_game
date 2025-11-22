using UnityEngine;

namespace CozyGame.Magic
{
    /// <summary>
    /// Teleport Spell - blink/teleport player to target location.
    /// Includes validation to prevent teleporting into walls or off cliffs.
    /// </summary>
    public class TeleportSpell : MonoBehaviour
    {
        [Header("Spell Settings")]
        [Tooltip("Spell data reference")]
        public SpellData teleportSpellData;

        [Tooltip("Maximum teleport distance")]
        public float maxTeleportDistance = 15f;

        [Tooltip("Minimum teleport distance")]
        public float minTeleportDistance = 2f;

        [Tooltip("Height above ground to place player")]
        public float groundOffset = 0.1f;

        [Header("Validation")]
        [Tooltip("Check for obstacles before teleporting")]
        public bool validateTeleport = true;

        [Tooltip("Player collision radius for validation")]
        public float playerRadius = 0.5f;

        [Tooltip("Player height for validation")]
        public float playerHeight = 2f;

        [Tooltip("Layers that block teleportation")]
        public LayerMask obstacleLayers = -1;

        [Tooltip("Layers for ground detection")]
        public LayerMask groundLayers = -1;

        [Header("Visual Feedback")]
        [Tooltip("Particle effect at teleport start")]
        public GameObject teleportStartPrefab;

        [Tooltip("Particle effect at teleport destination")]
        public GameObject teleportEndPrefab;

        [Tooltip("Particle effect duration")]
        public float particleDuration = 2f;

        [Tooltip("Teleport trail effect")]
        public GameObject teleportTrailPrefab;

        [Header("Audio")]
        [Tooltip("Sound when teleporting")]
        public string teleportSoundName = "spell_teleport";

        [Header("Camera")]
        [Tooltip("Camera shake intensity")]
        public float cameraShakeIntensity = 0.2f;

        [Tooltip("Camera shake duration")]
        public float cameraShakeDuration = 0.3f;

        /// <summary>
        /// Cast teleport spell to mouse/touch position
        /// Returns true if teleport was successful
        /// </summary>
        public bool CastTeleportSpell()
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

            // Get player position
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("Player not found!");
                return false;
            }

            Vector3 startPosition = player.transform.position;

            // Validate teleport
            if (!ValidateTeleport(startPosition, targetPosition, out Vector3 validatedPosition))
            {
                Debug.Log("Cannot teleport to that location!");

                // Show feedback
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.Show("Cannot teleport here!", targetPosition, Color.red);
                }

                return false;
            }

            // Check mana
            float manaCost = teleportSpellData != null ? teleportSpellData.manaCost : 25f;
            if (!MagicSystem.Instance.UseMana(manaCost))
            {
                Debug.Log("Not enough mana!");
                return false;
            }

            // Perform teleport
            PerformTeleport(player, startPosition, validatedPosition);

            // Mark spell as cast
            if (teleportSpellData != null)
            {
                teleportSpellData.MarkAsCast();
            }

            return true;
        }

        /// <summary>
        /// Validate teleport destination
        /// </summary>
        private bool ValidateTeleport(Vector3 start, Vector3 target, out Vector3 validatedPosition)
        {
            validatedPosition = target;

            // Check distance
            float distance = Vector3.Distance(start, target);

            if (distance < minTeleportDistance)
            {
                Debug.Log("Teleport distance too short");
                return false;
            }

            if (distance > maxTeleportDistance)
            {
                // Clamp to max distance
                Vector3 direction = (target - start).normalized;
                validatedPosition = start + direction * maxTeleportDistance;
                target = validatedPosition;
            }

            if (!validateTeleport)
            {
                return true;
            }

            // Find ground at target position
            Ray ray = new Ray(target + Vector3.up * 5f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 10f, groundLayers))
            {
                validatedPosition = hit.point + Vector3.up * groundOffset;
            }
            else
            {
                // No ground found
                Debug.Log("No ground at teleport destination");
                return false;
            }

            // Check for obstacles at destination
            Vector3 checkPosition = validatedPosition + Vector3.up * (playerHeight * 0.5f);

            if (Physics.CheckSphere(checkPosition, playerRadius, obstacleLayers))
            {
                Debug.Log("Obstacle blocking teleport destination");
                return false;
            }

            // Check for ceiling
            if (Physics.Raycast(validatedPosition, Vector3.up, playerHeight, obstacleLayers))
            {
                Debug.Log("Ceiling blocking teleport destination");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Perform the teleport
        /// </summary>
        private void PerformTeleport(GameObject player, Vector3 startPosition, Vector3 endPosition)
        {
            // Play start effect
            PlayTeleportEffects(startPosition, true);

            // Teleport player
            if (player.TryGetComponent<CharacterController>(out var controller))
            {
                controller.enabled = false;
                player.transform.position = endPosition;
                controller.enabled = true;
            }
            else if (player.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.position = endPosition;
                rb.velocity = Vector3.zero;
            }
            else
            {
                player.transform.position = endPosition;
            }

            // Play end effect
            PlayTeleportEffects(endPosition, false);

            // Create trail effect
            if (teleportTrailPrefab != null)
            {
                CreateTeleportTrail(startPosition, endPosition);
            }

            // Camera shake
            if (CameraController.Instance != null)
            {
                CameraController.Instance.ShakeCamera(cameraShakeIntensity, cameraShakeDuration);
            }

            // Spawn VFX
            if (VFX.ParticleEffectManager.Instance != null)
            {
                VFX.ParticleEffectManager.Instance.SpawnEffect(VFX.EffectType.SpellCast, startPosition);
                VFX.ParticleEffectManager.Instance.SpawnEffect(VFX.EffectType.SpellCast, endPosition);
            }

            // Track statistics
            if (Achievements.StatisticsTracker.Instance != null)
            {
                Achievements.StatisticsTracker.Instance.IncrementStatistic("spells_cast", 1f);
            }

            Debug.Log($"Teleported from {startPosition} to {endPosition}");
        }

        /// <summary>
        /// Create visual trail between start and end positions
        /// </summary>
        private void CreateTeleportTrail(Vector3 start, Vector3 end)
        {
            GameObject trail = Instantiate(teleportTrailPrefab);

            // Position trail
            Vector3 midPoint = (start + end) * 0.5f;
            trail.transform.position = midPoint;

            // Orient trail
            Vector3 direction = (end - start).normalized;
            trail.transform.rotation = Quaternion.LookRotation(direction);

            // Scale trail to distance
            float distance = Vector3.Distance(start, end);
            trail.transform.localScale = new Vector3(1f, 1f, distance);

            Destroy(trail, particleDuration);
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
            if (Physics.Raycast(ray, out RaycastHit hit, maxTeleportDistance * 2f))
            {
                return hit.point;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Play visual and audio effects for teleport
        /// </summary>
        private void PlayTeleportEffects(Vector3 position, bool isStart)
        {
            // Spawn particle effect
            GameObject prefab = isStart ? teleportStartPrefab : teleportEndPrefab;

            if (prefab != null)
            {
                GameObject particle = Instantiate(prefab, position, Quaternion.identity);
                Destroy(particle, particleDuration);
            }

            // Play sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(teleportSoundName))
            {
                AudioManager.Instance.PlaySoundAtPosition(teleportSoundName, position);
            }
        }

        /// <summary>
        /// Cast teleport spell to specific position (for AI or scripted events)
        /// </summary>
        public bool CastAtPosition(Vector3 worldPosition)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return false;

            Vector3 startPosition = player.transform.position;

            if (!ValidateTeleport(startPosition, worldPosition, out Vector3 validatedPosition))
            {
                return false;
            }

            float manaCost = teleportSpellData != null ? teleportSpellData.manaCost : 25f;
            if (MagicSystem.Instance != null && !MagicSystem.Instance.UseMana(manaCost))
            {
                return false;
            }

            PerformTeleport(player, startPosition, validatedPosition);

            if (teleportSpellData != null)
            {
                teleportSpellData.MarkAsCast();
            }

            return true;
        }

        // Visualize teleport range in editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, maxTeleportDistance);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, minTeleportDistance);
        }
    }
}
