using UnityEngine;

namespace CozyGame.Magic
{
    /// <summary>
    /// Floating light orb that can be summoned by the player.
    /// Provides illumination and follows the player or stays at a location.
    /// </summary>
    public class FloatingLightOrb : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Does this orb follow the player?")]
        public bool followPlayer = true;

        [Tooltip("Follow offset from player")]
        public Vector3 followOffset = new Vector3(0f, 2f, 1f);

        [Tooltip("Follow speed")]
        public float followSpeed = 5f;

        [Tooltip("Bobbing motion speed")]
        public float bobSpeed = 1f;

        [Tooltip("Bobbing motion height")]
        public float bobHeight = 0.3f;

        [Header("Light Settings")]
        [Tooltip("Light component")]
        public Light lightComponent;

        [Tooltip("Light intensity")]
        public float lightIntensity = 2f;

        [Tooltip("Light range")]
        public float lightRange = 10f;

        [Tooltip("Light color")]
        public Color lightColor = new Color(1f, 0.95f, 0.8f); // Warm white

        [Header("Lifetime")]
        [Tooltip("Duration before orb disappears (0 = infinite)")]
        public float lifetime = 60f;

        [Tooltip("Fade duration before disappearing")]
        public float fadeDuration = 2f;

        private Transform playerTransform;
        private Vector3 fixedPosition;
        private float spawnTime;
        private float bobOffset;

        private void Start()
        {
            spawnTime = Time.time;
            bobOffset = Random.Range(0f, 360f);

            // Setup light
            if (lightComponent == null)
            {
                lightComponent = GetComponentInChildren<Light>();
            }

            if (lightComponent != null)
            {
                lightComponent.intensity = lightIntensity;
                lightComponent.range = lightRange;
                lightComponent.color = lightColor;
            }

            // Find player
            if (followPlayer)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }
            else
            {
                fixedPosition = transform.position;
            }
        }

        private void Update()
        {
            // Bobbing motion
            float bob = Mathf.Sin((Time.time + bobOffset) * bobSpeed) * bobHeight;
            Vector3 bobVector = Vector3.up * bob;

            // Follow player or stay at fixed position
            if (followPlayer && playerTransform != null)
            {
                Vector3 targetPosition = playerTransform.position + followOffset + bobVector;
                transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = fixedPosition + bobVector;
            }

            // Lifetime management
            if (lifetime > 0f)
            {
                float age = Time.time - spawnTime;

                // Fade out before destroying
                if (age >= lifetime - fadeDuration)
                {
                    float fadeProgress = (age - (lifetime - fadeDuration)) / fadeDuration;
                    float alpha = 1f - fadeProgress;

                    if (lightComponent != null)
                    {
                        lightComponent.intensity = lightIntensity * alpha;
                    }
                }

                // Destroy after lifetime
                if (age >= lifetime)
                {
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// Set orb to follow player
        /// </summary>
        public void SetFollowPlayer(bool follow)
        {
            followPlayer = follow;

            if (!follow)
            {
                fixedPosition = transform.position;
            }
        }

        /// <summary>
        /// Set fixed position for orb
        /// </summary>
        public void SetFixedPosition(Vector3 position)
        {
            followPlayer = false;
            fixedPosition = position;
        }

        /// <summary>
        /// Extend lifetime
        /// </summary>
        public void ExtendLifetime(float additionalTime)
        {
            lifetime += additionalTime;
        }
    }

    /// <summary>
    /// Light Spell - creates a floating light orb.
    /// The orb can follow the player or stay at a fixed location.
    /// </summary>
    public class LightSpell : MonoBehaviour
    {
        [Header("Spell Settings")]
        [Tooltip("Spell data reference")]
        public SpellData lightSpellData;

        [Tooltip("Light orb prefab")]
        public GameObject lightOrbPrefab;

        [Tooltip("Orb lifetime (seconds)")]
        public float orbLifetime = 60f;

        [Tooltip("Should orb follow player?")]
        public bool orbFollowsPlayer = true;

        [Tooltip("Maximum number of active orbs")]
        public int maxActiveOrbs = 3;

        [Header("Visual Feedback")]
        [Tooltip("Particle effect when creating orb")]
        public GameObject castParticlePrefab;

        [Tooltip("Duration of cast effect")]
        public float particleDuration = 1f;

        [Header("Audio")]
        [Tooltip("Sound when spell is cast")]
        public string lightSoundName = "spell_light";

        [Header("Targeting")]
        [Tooltip("Maximum range to cast spell")]
        public float maxRange = 20f;

        [Tooltip("Layers for ground detection")]
        public LayerMask groundLayers = -1;

        // Active orbs
        private System.Collections.Generic.List<GameObject> activeOrbs = new System.Collections.Generic.List<GameObject>();

        /// <summary>
        /// Cast light spell at mouse/touch position or on player
        /// </summary>
        public bool CastLightSpell()
        {
            // Check if we have a magic system
            if (MagicSystem.Instance == null)
            {
                Debug.LogWarning("MagicSystem not found in scene!");
                return false;
            }

            // Check mana first
            float manaCost = lightSpellData != null ? lightSpellData.manaCost : 15f;
            if (!MagicSystem.Instance.UseMana(manaCost))
            {
                Debug.Log("Not enough mana!");
                return false;
            }

            // Remove oldest orb if at max capacity
            if (activeOrbs.Count >= maxActiveOrbs)
            {
                GameObject oldestOrb = activeOrbs[0];
                if (oldestOrb != null)
                {
                    Destroy(oldestOrb);
                }
                activeOrbs.RemoveAt(0);
            }

            // Determine spawn position
            Vector3 spawnPosition;

            if (orbFollowsPlayer)
            {
                // Spawn near player
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    spawnPosition = player.transform.position + Vector3.up * 2f;
                }
                else
                {
                    spawnPosition = transform.position + Vector3.up * 2f;
                }
            }
            else
            {
                // Spawn at target position
                spawnPosition = GetTargetPosition();
                if (spawnPosition == Vector3.zero)
                {
                    // Refund mana
                    MagicSystem.Instance.RestoreMana(manaCost);
                    Debug.Log("No valid target position");
                    return false;
                }
            }

            // Create light orb
            GameObject orb = CreateLightOrb(spawnPosition);

            if (orb != null)
            {
                activeOrbs.Add(orb);

                // Mark spell as cast
                if (lightSpellData != null)
                {
                    lightSpellData.MarkAsCast();
                }

                // Play effects
                PlayLightEffects(spawnPosition);

                Debug.Log($"Created light orb at {spawnPosition}");
                return true;
            }

            // Refund mana if failed
            MagicSystem.Instance.RestoreMana(manaCost);
            return false;
        }

        /// <summary>
        /// Create a light orb at position
        /// </summary>
        private GameObject CreateLightOrb(Vector3 position)
        {
            GameObject orb = null;

            if (lightOrbPrefab != null)
            {
                orb = Instantiate(lightOrbPrefab, position, Quaternion.identity);
            }
            else
            {
                // Create default orb if no prefab assigned
                orb = CreateDefaultOrb(position);
            }

            // Setup orb component
            FloatingLightOrb orbComponent = orb.GetComponent<FloatingLightOrb>();
            if (orbComponent == null)
            {
                orbComponent = orb.AddComponent<FloatingLightOrb>();
            }

            orbComponent.followPlayer = orbFollowsPlayer;
            orbComponent.lifetime = orbLifetime;

            return orb;
        }

        /// <summary>
        /// Create a default light orb (fallback)
        /// </summary>
        private GameObject CreateDefaultOrb(Vector3 position)
        {
            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "Light Orb";
            orb.transform.position = position;
            orb.transform.localScale = Vector3.one * 0.3f;

            // Add light component
            Light light = orb.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 10f;
            light.intensity = 2f;
            light.color = new Color(1f, 0.95f, 0.8f);

            // Make it glow
            Renderer renderer = orb.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.white * 2f);
                renderer.material = mat;
            }

            // Remove collider
            Collider collider = orb.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            return orb;
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
            if (Physics.Raycast(ray, out RaycastHit hit, maxRange, groundLayers))
            {
                return hit.point + Vector3.up * 1.5f; // Spawn above ground
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Play visual and audio effects for light spell
        /// </summary>
        private void PlayLightEffects(Vector3 position)
        {
            // Spawn particle effect
            if (castParticlePrefab != null)
            {
                GameObject particle = Instantiate(castParticlePrefab, position, Quaternion.identity);
                Destroy(particle, particleDuration);
            }

            // Play sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(lightSoundName))
            {
                AudioManager.Instance.PlaySoundAtPosition(lightSoundName, position);
            }

            // Spawn VFX
            if (VFX.ParticleEffectManager.Instance != null)
            {
                VFX.ParticleEffectManager.Instance.SpawnEffect(VFX.EffectType.Sparkle, position);
            }
        }

        /// <summary>
        /// Cast light spell at specific position
        /// </summary>
        public bool CastAtPosition(Vector3 worldPosition)
        {
            if (MagicSystem.Instance == null)
                return false;

            float manaCost = lightSpellData != null ? lightSpellData.manaCost : 15f;
            if (!MagicSystem.Instance.UseMana(manaCost))
                return false;

            // Remove oldest if at capacity
            if (activeOrbs.Count >= maxActiveOrbs)
            {
                if (activeOrbs[0] != null)
                    Destroy(activeOrbs[0]);
                activeOrbs.RemoveAt(0);
            }

            GameObject orb = CreateLightOrb(worldPosition);
            if (orb != null)
            {
                activeOrbs.Add(orb);

                if (lightSpellData != null)
                {
                    lightSpellData.MarkAsCast();
                }

                PlayLightEffects(worldPosition);
                return true;
            }

            MagicSystem.Instance.RestoreMana(manaCost);
            return false;
        }

        /// <summary>
        /// Dismiss all active orbs
        /// </summary>
        public void DismissAllOrbs()
        {
            foreach (GameObject orb in activeOrbs)
            {
                if (orb != null)
                {
                    Destroy(orb);
                }
            }

            activeOrbs.Clear();
        }

        /// <summary>
        /// Get number of active orbs
        /// </summary>
        public int GetActiveOrbCount()
        {
            // Clean up null references
            activeOrbs.RemoveAll(orb => orb == null);
            return activeOrbs.Count;
        }

        private void OnDestroy()
        {
            DismissAllOrbs();
        }

        // Visualize casting range in editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 1f, 0.5f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, maxRange);
        }
    }
}
