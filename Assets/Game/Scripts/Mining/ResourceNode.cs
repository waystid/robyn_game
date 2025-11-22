using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace CozyGame.Mining
{
    /// <summary>
    /// Resource node component.
    /// Represents a mineable resource in the world.
    /// Handles mining interactions, health, respawning.
    /// </summary>
    public class ResourceNode : MonoBehaviour, Interaction.IInteractable
    {
        [Header("Resource Data")]
        [Tooltip("Mineable resource definition")]
        public MineableResource resourceData;

        [Header("Current State")]
        [Tooltip("Current health (hits remaining)")]
        public int currentHealth;

        [Tooltip("Is currently being mined")]
        public bool isMining = false;

        [Tooltip("Is depleted (mined out)")]
        public bool isDepleted = false;

        [Header("Visual")]
        [Tooltip("Current visual model")]
        public GameObject currentModel;

        [Tooltip("Model container")]
        public Transform modelContainer;

        [Header("Respawn")]
        [Tooltip("Respawn timer")]
        public float respawnTimer = 0f;

        [Header("Events")]
        public UnityEvent<int> OnMiningHit; // remaining hits
        public UnityEvent OnDepleted;
        public UnityEvent OnRespawned;

        // State
        private int maxHealth;
        private GameObject particleEffect;

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            // Update respawn timer
            if (isDepleted && resourceData != null && resourceData.canRespawn)
            {
                respawnTimer -= Time.deltaTime;
                if (respawnTimer <= 0f)
                {
                    Respawn();
                }
            }
        }

        /// <summary>
        /// Initialize resource node
        /// </summary>
        public void Initialize()
        {
            if (resourceData == null)
            {
                Debug.LogError($"[ResourceNode] {gameObject.name} has no resource data assigned!");
                return;
            }

            // Set initial health
            maxHealth = resourceData.baseHitsRequired;
            currentHealth = maxHealth;

            // Create visual model
            if (currentModel == null && resourceData.nodePrefab != null)
            {
                if (modelContainer == null)
                {
                    GameObject container = new GameObject("ModelContainer");
                    container.transform.SetParent(transform);
                    container.transform.localPosition = Vector3.zero;
                    modelContainer = container.transform;
                }

                currentModel = Instantiate(resourceData.nodePrefab, modelContainer);
                currentModel.transform.localPosition = Vector3.zero;
                currentModel.transform.localRotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// IInteractable: Check if can interact
        /// </summary>
        public bool CanInteract(GameObject interactor)
        {
            if (isDepleted || isMining)
                return false;

            if (resourceData == null)
                return false;

            // Check if player has required tool
            // TODO: Check player's equipped tool
            // For now, allow interaction
            return true;
        }

        /// <summary>
        /// IInteractable: Get interaction prompt
        /// </summary>
        public string GetInteractionPrompt()
        {
            if (isDepleted)
                return "";

            return $"Mine {resourceData.resourceName}";
        }

        /// <summary>
        /// IInteractable: Interact (start mining)
        /// </summary>
        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
                return;

            // Start mining mini-game or direct mining
            if (UI.MiningMiniGameUI.Instance != null && resourceData.hardness > 0.3f)
            {
                // Use mini-game for harder resources
                UI.MiningMiniGameUI.Instance.StartMiniGame(this, interactor);
            }
            else
            {
                // Direct hit for easy resources
                OnMiningAttempt(interactor, true, false);
            }
        }

        /// <summary>
        /// Called when player attempts mining (from mini-game or direct)
        /// </summary>
        public void OnMiningAttempt(GameObject miner, bool success, bool isCritical = false)
        {
            if (isDepleted || resourceData == null)
                return;

            // Get player's tool tier
            int toolTier = GetPlayerToolTier(miner);

            // Check if tool can mine this resource
            if (!resourceData.CanMineWithTool(toolTier))
            {
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Requires Tier {resourceData.minToolTier} tool!",
                        transform.position + Vector3.up,
                        Color.red
                    );
                }
                return;
            }

            if (success)
            {
                // Successful hit
                currentHealth--;

                // Play hit effects
                PlayMiningHitEffects(isCritical);

                // Show damage
                if (FloatingTextManager.Instance != null)
                {
                    string text = isCritical ? "CRITICAL!" : $"-1";
                    Color color = isCritical ? Color.yellow : Color.white;
                    FloatingTextManager.Instance.Show(text, transform.position + Vector3.up, color);
                }

                // Damage tool durability
                DamagePlayerTool(miner, resourceData.toolDurabilityDamage);

                // Update visual
                UpdateDamagedVisual();

                // Trigger event
                OnMiningHit?.Invoke(currentHealth);

                // Check if depleted
                if (currentHealth <= 0)
                {
                    OnNodeDepleted(miner, toolTier, isCritical);
                }
            }
            else
            {
                // Failed hit
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.Show("Miss!", transform.position + Vector3.up, Color.gray);
                }

                // Small durability damage on miss
                DamagePlayerTool(miner, 1);
            }
        }

        /// <summary>
        /// Called when node is fully depleted
        /// </summary>
        private void OnNodeDepleted(GameObject miner, int toolTier, bool isCritical)
        {
            isDepleted = true;

            // Generate drops
            Item[] items;
            int[] quantities;
            resourceData.GenerateDrops(toolTier, isCritical, out items, out quantities);

            // Add drops to inventory
            for (int i = 0; i < items.Length; i++)
            {
                Inventory.InventorySystem.Instance.AddItem(items[i].itemID, quantities[i]);

                // Show floating text
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.ShowItemPickup(
                        items[i].itemName,
                        quantities[i],
                        transform.position + Vector3.up,
                        resourceData.rarity >= ResourceRarity.Rare
                    );
                }
            }

            // Grant experience
            if (PlayerStats.Instance != null && resourceData.experienceReward > 0)
            {
                PlayerStats.Instance.GainExperience(resourceData.experienceReward);
            }

            // Play destruction effects
            PlayDestructionEffects();

            // Trigger event
            OnDepleted?.Invoke();

            // Hide model
            if (currentModel != null)
            {
                currentModel.SetActive(false);
            }

            // Start respawn timer
            if (resourceData.canRespawn)
            {
                respawnTimer = resourceData.GetRandomRespawnTime();
            }
            else
            {
                // Destroy node if can't respawn
                Destroy(gameObject, 2f);
            }

            Debug.Log($"[ResourceNode] {resourceData.resourceName} depleted! Drops: {items.Length} items");
        }

        /// <summary>
        /// Respawn the resource node
        /// </summary>
        private void Respawn()
        {
            isDepleted = false;
            currentHealth = maxHealth;
            respawnTimer = 0f;

            // Show model
            if (currentModel != null)
            {
                currentModel.SetActive(true);
            }
            else
            {
                Initialize();
            }

            // Reset visual to pristine
            UpdateDamagedVisual();

            // Play respawn effect
            if (VFX.ParticleEffectManager.Instance != null)
            {
                VFX.ParticleEffectManager.Instance.SpawnEffect(VFX.EffectType.Sparkle, transform.position);
            }

            // Trigger event
            OnRespawned?.Invoke();

            Debug.Log($"[ResourceNode] {resourceData.resourceName} respawned!");
        }

        /// <summary>
        /// Update damaged visual based on current health
        /// </summary>
        private void UpdateDamagedVisual()
        {
            if (resourceData == null || currentModel == null)
                return;

            float healthPercent = (float)currentHealth / maxHealth;
            GameObject damagedPrefab = resourceData.GetDamagedPrefab(healthPercent);

            if (damagedPrefab != null && damagedPrefab != currentModel)
            {
                // Swap to damaged model
                Vector3 pos = currentModel.transform.localPosition;
                Quaternion rot = currentModel.transform.localRotation;

                Destroy(currentModel);

                currentModel = Instantiate(damagedPrefab, modelContainer);
                currentModel.transform.localPosition = pos;
                currentModel.transform.localRotation = rot;
            }
        }

        /// <summary>
        /// Play mining hit effects
        /// </summary>
        private void PlayMiningHitEffects(bool isCritical)
        {
            // Spawn particles
            if (resourceData.miningParticlePrefab != null)
            {
                GameObject particles = Instantiate(
                    resourceData.miningParticlePrefab,
                    transform.position + Vector3.up * 0.5f,
                    Quaternion.identity
                );
                Destroy(particles, 2f);
            }

            // Play sound
            string soundName = isCritical ? resourceData.miningSuccessSound : resourceData.miningHitSound;
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(soundName))
            {
                AudioManager.Instance.PlaySoundAtPosition(soundName, transform.position);
            }
        }

        /// <summary>
        /// Play destruction effects
        /// </summary>
        private void PlayDestructionEffects()
        {
            // Spawn particles
            if (resourceData.destructionParticlePrefab != null)
            {
                GameObject particles = Instantiate(
                    resourceData.destructionParticlePrefab,
                    transform.position,
                    Quaternion.identity
                );
                Destroy(particles, 3f);
            }

            // Play sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(resourceData.destructionSound))
            {
                AudioManager.Instance.PlaySoundAtPosition(resourceData.destructionSound, transform.position);
            }
        }

        /// <summary>
        /// Get player's tool tier
        /// </summary>
        private int GetPlayerToolTier(GameObject player)
        {
            // TODO: Get from player's equipped tool
            // For now, return default tier
            return 0;
        }

        /// <summary>
        /// Damage player's tool durability
        /// </summary>
        private void DamagePlayerTool(GameObject player, int damage)
        {
            // TODO: Damage equipped tool's durability
            // This will be implemented when we create the tool system
        }

        /// <summary>
        /// Get current health percentage
        /// </summary>
        public float GetHealthPercent()
        {
            return maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        }

        private void OnDrawGizmosSelected()
        {
            if (resourceData == null)
                return;

            // Draw resource info
            Gizmos.color = resourceData.GetRarityColor();
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.5f);

            #if UNITY_EDITOR
            string label = $"{resourceData.resourceName}\n";
            if (isDepleted)
            {
                label += $"Depleted\nRespawn: {Mathf.CeilToInt(respawnTimer)}s";
            }
            else
            {
                label += $"Health: {currentHealth}/{maxHealth}";
            }
            UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, label);
            #endif
        }
    }
}
