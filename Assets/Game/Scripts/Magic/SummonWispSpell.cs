using UnityEngine;
using UnityEngine.AI;

namespace CozyGame.Magic
{
    /// <summary>
    /// Wisp creature that helps the player.
    /// Can collect items, scout areas, or provide light.
    /// </summary>
    public class Wisp : MonoBehaviour
    {
        public enum WispBehavior
        {
            Follow,        // Follow player
            CollectItems,  // Collect nearby items
            Scout,         // Scout ahead of player
            Patrol         // Patrol around player
        }

        [Header("Behavior")]
        [Tooltip("Current behavior mode")]
        public WispBehavior currentBehavior = WispBehavior.Follow;

        [Tooltip("Follow distance from player")]
        public float followDistance = 3f;

        [Tooltip("Collection radius for items")]
        public float collectionRadius = 5f;

        [Tooltip("Scout distance ahead")]
        public float scoutDistance = 10f;

        [Header("Movement")]
        [Tooltip("Movement speed")]
        public float moveSpeed = 5f;

        [Tooltip("Rotation speed")]
        public float rotationSpeed = 10f;

        [Tooltip("Height above ground")]
        public float hoverHeight = 1.5f;

        [Tooltip("Bobbing motion")]
        public float bobSpeed = 2f;
        public float bobHeight = 0.2f;

        [Header("Light")]
        [Tooltip("Light component")]
        public Light wispLight;

        [Tooltip("Light intensity")]
        public float lightIntensity = 3f;

        [Tooltip("Light range")]
        public float lightRange = 15f;

        [Tooltip("Light color")]
        public Color lightColor = new Color(0.5f, 0.8f, 1f); // Soft blue

        [Header("Lifetime")]
        [Tooltip("Duration before wisp disappears (0 = infinite)")]
        public float lifetime = 120f;

        private Transform playerTransform;
        private float spawnTime;
        private float bobOffset;
        private Vector3 targetPosition;

        private void Start()
        {
            spawnTime = Time.time;
            bobOffset = Random.Range(0f, 360f);

            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }

            // Setup light
            if (wispLight == null)
            {
                wispLight = GetComponentInChildren<Light>();
            }

            if (wispLight != null)
            {
                wispLight.intensity = lightIntensity;
                wispLight.range = lightRange;
                wispLight.color = lightColor;
            }

            targetPosition = transform.position;
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                // Try to find player
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
                else
                {
                    return;
                }
            }

            // Execute behavior
            switch (currentBehavior)
            {
                case WispBehavior.Follow:
                    FollowPlayer();
                    break;

                case WispBehavior.CollectItems:
                    CollectNearbyItems();
                    break;

                case WispBehavior.Scout:
                    ScoutAhead();
                    break;

                case WispBehavior.Patrol:
                    PatrolAround();
                    break;
            }

            // Move towards target
            MoveToTarget();

            // Lifetime management
            if (lifetime > 0f)
            {
                float age = Time.time - spawnTime;

                if (age >= lifetime - 2f)
                {
                    // Fade out
                    float fadeProgress = (age - (lifetime - 2f)) / 2f;
                    float alpha = 1f - fadeProgress;

                    if (wispLight != null)
                    {
                        wispLight.intensity = lightIntensity * alpha;
                    }
                }

                if (age >= lifetime)
                {
                    Destroy(gameObject);
                }
            }
        }

        /// <summary>
        /// Follow player behavior
        /// </summary>
        private void FollowPlayer()
        {
            Vector3 offsetPosition = playerTransform.position + playerTransform.right * followDistance;
            targetPosition = offsetPosition;
        }

        /// <summary>
        /// Collect nearby items behavior
        /// </summary>
        private void CollectNearbyItems()
        {
            // Look for items nearby
            Collider[] colliders = Physics.OverlapSphere(transform.position, collectionRadius);

            foreach (Collider col in colliders)
            {
                // Check if it's an item pickup
                if (col.CompareTag("Item") || col.GetComponent<Inventory.ItemPickup>() != null)
                {
                    // Move to item
                    targetPosition = col.transform.position;

                    // Auto-pickup if close enough
                    if (Vector3.Distance(transform.position, col.transform.position) < 1f)
                    {
                        Inventory.ItemPickup pickup = col.GetComponent<Inventory.ItemPickup>();
                        if (pickup != null)
                        {
                            // Add to player inventory
                            if (Inventory.InventorySystem.Instance != null)
                            {
                                Inventory.InventorySystem.Instance.AddItem(pickup.itemData.itemID, pickup.quantity);
                                Destroy(col.gameObject);

                                // Show feedback
                                if (VFX.ParticleEffectManager.Instance != null)
                                {
                                    VFX.ParticleEffectManager.Instance.SpawnEffect(VFX.EffectType.ItemPickup, col.transform.position);
                                }
                            }
                        }
                    }

                    return; // Focus on one item at a time
                }
            }

            // No items found, follow player
            FollowPlayer();
        }

        /// <summary>
        /// Scout ahead of player
        /// </summary>
        private void ScoutAhead()
        {
            Vector3 scoutPosition = playerTransform.position + playerTransform.forward * scoutDistance;
            targetPosition = scoutPosition;
        }

        /// <summary>
        /// Patrol around player
        /// </summary>
        private void PatrolAround()
        {
            // Circle around player
            float angle = Time.time * 0.5f;
            float radius = followDistance * 1.5f;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            targetPosition = playerTransform.position + offset;
        }

        /// <summary>
        /// Move towards target position
        /// </summary>
        private void MoveToTarget()
        {
            // Add bobbing motion
            float bob = Mathf.Sin((Time.time + bobOffset) * bobSpeed) * bobHeight;
            Vector3 bobVector = Vector3.up * bob;

            // Calculate target with hover height
            Vector3 finalTarget = targetPosition + Vector3.up * hoverHeight + bobVector;

            // Move towards target
            transform.position = Vector3.Lerp(transform.position, finalTarget, moveSpeed * Time.deltaTime);

            // Rotate towards movement direction
            Vector3 direction = (finalTarget - transform.position).normalized;
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Change wisp behavior
        /// </summary>
        public void SetBehavior(WispBehavior behavior)
        {
            currentBehavior = behavior;
        }

        /// <summary>
        /// Extend wisp lifetime
        /// </summary>
        public void ExtendLifetime(float additionalTime)
        {
            lifetime += additionalTime;
        }
    }

    /// <summary>
    /// Summon Wisp Spell - summons a magical wisp creature to help the player.
    /// The wisp can follow, collect items, scout, or patrol.
    /// </summary>
    public class SummonWispSpell : MonoBehaviour
    {
        [Header("Spell Settings")]
        [Tooltip("Spell data reference")]
        public SpellData summonSpellData;

        [Tooltip("Wisp prefab to summon")]
        public GameObject wispPrefab;

        [Tooltip("Wisp lifetime (seconds)")]
        public float wispLifetime = 120f;

        [Tooltip("Initial wisp behavior")]
        public Wisp.WispBehavior initialBehavior = Wisp.WispBehavior.Follow;

        [Tooltip("Maximum number of active wisps")]
        public int maxActiveWisps = 2;

        [Header("Visual Feedback")]
        [Tooltip("Particle effect when summoning wisp")]
        public GameObject summonParticlePrefab;

        [Tooltip("Duration of summon effect")]
        public float particleDuration = 2f;

        [Header("Audio")]
        [Tooltip("Sound when spell is cast")]
        public string summonSoundName = "spell_summon";

        // Active wisps
        private System.Collections.Generic.List<GameObject> activeWisps = new System.Collections.Generic.List<GameObject>();

        /// <summary>
        /// Cast summon wisp spell
        /// </summary>
        public bool CastSummonWispSpell()
        {
            // Check if we have a magic system
            if (MagicSystem.Instance == null)
            {
                Debug.LogWarning("MagicSystem not found in scene!");
                return false;
            }

            // Check mana first
            float manaCost = summonSpellData != null ? summonSpellData.manaCost : 40f;
            if (!MagicSystem.Instance.UseMana(manaCost))
            {
                Debug.Log("Not enough mana!");
                return false;
            }

            // Check max wisps
            if (activeWisps.Count >= maxActiveWisps)
            {
                Debug.Log("Maximum wisps already summoned!");
                MagicSystem.Instance.RestoreMana(manaCost);
                return false;
            }

            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("Player not found!");
                MagicSystem.Instance.RestoreMana(manaCost);
                return false;
            }

            // Summon wisp near player
            Vector3 spawnPosition = player.transform.position + player.transform.right * 2f + Vector3.up * 1.5f;
            GameObject wisp = SummonWisp(spawnPosition);

            if (wisp != null)
            {
                activeWisps.Add(wisp);

                // Mark spell as cast
                if (summonSpellData != null)
                {
                    summonSpellData.MarkAsCast();
                }

                // Play effects
                PlaySummonEffects(spawnPosition);

                Debug.Log($"Summoned wisp at {spawnPosition}");
                return true;
            }

            // Refund mana if failed
            MagicSystem.Instance.RestoreMana(manaCost);
            return false;
        }

        /// <summary>
        /// Summon a wisp at position
        /// </summary>
        private GameObject SummonWisp(Vector3 position)
        {
            GameObject wisp = null;

            if (wispPrefab != null)
            {
                wisp = Instantiate(wispPrefab, position, Quaternion.identity);
            }
            else
            {
                // Create default wisp if no prefab assigned
                wisp = CreateDefaultWisp(position);
            }

            // Setup wisp component
            Wisp wispComponent = wisp.GetComponent<Wisp>();
            if (wispComponent == null)
            {
                wispComponent = wisp.AddComponent<Wisp>();
            }

            wispComponent.currentBehavior = initialBehavior;
            wispComponent.lifetime = wispLifetime;

            return wisp;
        }

        /// <summary>
        /// Create a default wisp (fallback)
        /// </summary>
        private GameObject CreateDefaultWisp(Vector3 position)
        {
            GameObject wisp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            wisp.name = "Wisp";
            wisp.transform.position = position;
            wisp.transform.localScale = Vector3.one * 0.4f;

            // Add light
            Light light = wisp.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 15f;
            light.intensity = 3f;
            light.color = new Color(0.5f, 0.8f, 1f);

            // Make it glow
            Renderer renderer = wisp.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.5f, 0.8f, 1f) * 3f);
                renderer.material = mat;
            }

            // Remove collider
            Collider collider = wisp.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            return wisp;
        }

        /// <summary>
        /// Play visual and audio effects for summon spell
        /// </summary>
        private void PlaySummonEffects(Vector3 position)
        {
            // Spawn particle effect
            if (summonParticlePrefab != null)
            {
                GameObject particle = Instantiate(summonParticlePrefab, position, Quaternion.identity);
                Destroy(particle, particleDuration);
            }

            // Play sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(summonSoundName))
            {
                AudioManager.Instance.PlaySoundAtPosition(summonSoundName, position);
            }

            // Spawn VFX
            if (VFX.ParticleEffectManager.Instance != null)
            {
                VFX.ParticleEffectManager.Instance.SpawnEffect(VFX.EffectType.SpellCast, position);
                VFX.ParticleEffectManager.Instance.SpawnEffect(VFX.EffectType.Sparkle, position);
            }
        }

        /// <summary>
        /// Summon wisp at specific position
        /// </summary>
        public bool CastAtPosition(Vector3 worldPosition)
        {
            if (MagicSystem.Instance == null)
                return false;

            float manaCost = summonSpellData != null ? summonSpellData.manaCost : 40f;
            if (!MagicSystem.Instance.UseMana(manaCost))
                return false;

            if (activeWisps.Count >= maxActiveWisps)
            {
                MagicSystem.Instance.RestoreMana(manaCost);
                return false;
            }

            GameObject wisp = SummonWisp(worldPosition);
            if (wisp != null)
            {
                activeWisps.Add(wisp);

                if (summonSpellData != null)
                {
                    summonSpellData.MarkAsCast();
                }

                PlaySummonEffects(worldPosition);
                return true;
            }

            MagicSystem.Instance.RestoreMana(manaCost);
            return false;
        }

        /// <summary>
        /// Dismiss all wisps
        /// </summary>
        public void DismissAllWisps()
        {
            foreach (GameObject wisp in activeWisps)
            {
                if (wisp != null)
                {
                    Destroy(wisp);
                }
            }

            activeWisps.Clear();
        }

        /// <summary>
        /// Change behavior of all wisps
        /// </summary>
        public void SetAllWispsBehavior(Wisp.WispBehavior behavior)
        {
            foreach (GameObject wispObj in activeWisps)
            {
                if (wispObj != null)
                {
                    Wisp wisp = wispObj.GetComponent<Wisp>();
                    if (wisp != null)
                    {
                        wisp.SetBehavior(behavior);
                    }
                }
            }
        }

        /// <summary>
        /// Get number of active wisps
        /// </summary>
        public int GetActiveWispCount()
        {
            // Clean up null references
            activeWisps.RemoveAll(wisp => wisp == null);
            return activeWisps.Count;
        }

        private void OnDestroy()
        {
            DismissAllWisps();
        }
    }
}
