using UnityEngine;
using UnityEngine.Events;

namespace CozyGame.Fishing
{
    /// <summary>
    /// Fishing rod tool component.
    /// Handles casting, detecting fishing spots, and initiating the fishing mini-game.
    /// </summary>
    public class FishingRod : MonoBehaviour
    {
        [Header("Rod Settings")]
        [Tooltip("Rod name")]
        public string rodName = "Basic Fishing Rod";

        [Tooltip("Maximum cast distance")]
        public float maxCastDistance = 15f;

        [Tooltip("Cast power (affects distance)")]
        [Range(0f, 1f)]
        public float castPower = 0.8f;

        [Tooltip("Rod quality (affects catch chance)")]
        [Range(0f, 1f)]
        public float rodQuality = 0.5f;

        [Header("Visual")]
        [Tooltip("Fishing line renderer")]
        public LineRenderer fishingLine;

        [Tooltip("Bobber/float prefab")]
        public GameObject bobberPrefab;

        [Tooltip("Rod tip transform (where line starts)")]
        public Transform rodTip;

        [Header("Casting")]
        [Tooltip("Cast time (seconds)")]
        public float castTime = 0.5f;

        [Tooltip("Cast particle effect")]
        public GameObject castEffectPrefab;

        [Tooltip("Layers that can be cast on (water, etc.)")]
        public LayerMask castableLayers = -1;

        [Header("Bite Detection")]
        [Tooltip("Min time before bite (seconds)")]
        public float minBiteTime = 2f;

        [Tooltip("Max time before bite (seconds)")]
        public float maxBiteTime = 10f;

        [Tooltip("Bite indicator particle")]
        public GameObject biteIndicatorPrefab;

        [Header("Audio")]
        [Tooltip("Cast sound")]
        public string castSoundName = "fishing_cast";

        [Tooltip("Reel sound")]
        public string reelSoundName = "fishing_reel";

        [Tooltip("Splash sound")]
        public string splashSoundName = "water_splash";

        [Header("Events")]
        public UnityEvent OnCast;
        public UnityEvent OnBite;
        public UnityEvent<Fish, float> OnCatch; // Fish caught and size
        public UnityEvent OnMiss;
        public UnityEvent OnReel;

        // State
        public enum FishingState
        {
            Idle,
            Casting,
            WaitingForBite,
            Hooked,
            Reeling,
            MiniGame
        }

        private FishingState currentState = FishingState.Idle;
        private GameObject currentBobber;
        private Vector3 castPosition;
        private float biteTimer = 0f;
        private FishingSpot currentSpot;
        private Fish hookedFish;
        private float hookedFishSize;

        private void Start()
        {
            if (rodTip == null)
            {
                rodTip = transform;
            }

            if (fishingLine != null)
            {
                fishingLine.enabled = false;
            }
        }

        private void Update()
        {
            // Update state machine
            switch (currentState)
            {
                case FishingState.Idle:
                    UpdateIdle();
                    break;

                case FishingState.Casting:
                    UpdateCasting();
                    break;

                case FishingState.WaitingForBite:
                    UpdateWaitingForBite();
                    break;

                case FishingState.Hooked:
                    UpdateHooked();
                    break;

                case FishingState.Reeling:
                    UpdateReeling();
                    break;

                case FishingState.MiniGame:
                    UpdateMiniGame();
                    break;
            }

            // Update fishing line
            UpdateFishingLine();
        }

        /// <summary>
        /// Cast fishing rod
        /// </summary>
        public bool Cast()
        {
            if (currentState != FishingState.Idle)
            {
                Debug.Log("Cannot cast while fishing!");
                return false;
            }

            // Get cast position from input
            Vector3 targetPosition = GetCastTargetPosition();

            if (targetPosition == Vector3.zero)
            {
                Debug.Log("No valid cast position");
                return false;
            }

            // Check if position is in a fishing spot
            currentSpot = FindFishingSpotAtPosition(targetPosition);

            if (currentSpot == null)
            {
                Debug.Log("Not a fishing spot!");
                return false;
            }

            // Start casting
            castPosition = targetPosition;
            currentState = FishingState.Casting;
            biteTimer = castTime;

            // Play cast sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(castSoundName))
            {
                AudioManager.Instance.PlaySoundAtPosition(castSoundName, transform.position);
            }

            // Spawn cast effect
            if (castEffectPrefab != null)
            {
                GameObject effect = Instantiate(castEffectPrefab, rodTip.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            OnCast?.Invoke();

            Debug.Log($"Cast to {castPosition}");
            return true;
        }

        /// <summary>
        /// Reel in the line
        /// </summary>
        public void Reel()
        {
            if (currentState == FishingState.WaitingForBite)
            {
                // Reel in without catching anything
                currentState = FishingState.Reeling;
                OnReel?.Invoke();
            }
            else if (currentState == FishingState.Hooked)
            {
                // Start mini-game
                StartMiniGame();
            }
        }

        /// <summary>
        /// Update idle state
        /// </summary>
        private void UpdateIdle()
        {
            // Waiting for cast input
            // This would typically be called by player input system
        }

        /// <summary>
        /// Update casting state
        /// </summary>
        private void UpdateCasting()
        {
            biteTimer -= Time.deltaTime;

            if (biteTimer <= 0f)
            {
                // Casting complete, spawn bobber
                SpawnBobber(castPosition);

                // Transition to waiting for bite
                currentState = FishingState.WaitingForBite;
                biteTimer = Random.Range(minBiteTime, maxBiteTime);

                // Play splash sound
                if (AudioManager.Instance != null && !string.IsNullOrEmpty(splashSoundName))
                {
                    AudioManager.Instance.PlaySoundAtPosition(splashSoundName, castPosition);
                }

                Debug.Log($"Waiting for bite... ({biteTimer:F1}s)");
            }
        }

        /// <summary>
        /// Update waiting for bite state
        /// </summary>
        private void UpdateWaitingForBite()
        {
            biteTimer -= Time.deltaTime;

            if (biteTimer <= 0f)
            {
                // Fish bites!
                OnFishBite();
            }
        }

        /// <summary>
        /// Update hooked state
        /// </summary>
        private void UpdateHooked()
        {
            // Animate bobber (bobbing motion)
            if (currentBobber != null)
            {
                Vector3 bobPos = currentBobber.transform.position;
                bobPos.y += Mathf.Sin(Time.time * 5f) * 0.1f;
                currentBobber.transform.position = bobPos;
            }

            // Wait for player to start reeling (handled by input)
        }

        /// <summary>
        /// Update reeling state
        /// </summary>
        private void UpdateReeling()
        {
            // Reel in bobber
            if (currentBobber != null)
            {
                Vector3 targetPos = rodTip.position;
                currentBobber.transform.position = Vector3.Lerp(
                    currentBobber.transform.position,
                    targetPos,
                    5f * Time.deltaTime
                );

                // Check if bobber reached rod
                if (Vector3.Distance(currentBobber.transform.position, targetPos) < 0.5f)
                {
                    // Reeling complete
                    DestroyBobber();
                    currentState = FishingState.Idle;
                    currentSpot = null;
                }
            }
        }

        /// <summary>
        /// Update mini-game state
        /// </summary>
        private void UpdateMiniGame()
        {
            // Mini-game is handled by FishingMiniGame component
            // This state waits for the mini-game to complete
        }

        /// <summary>
        /// Fish bite event
        /// </summary>
        private void OnFishBite()
        {
            if (currentSpot == null)
            {
                Debug.LogWarning("No fishing spot!");
                return;
            }

            // Roll for fish
            hookedFish = currentSpot.GetRandomFish();

            if (hookedFish == null)
            {
                Debug.Log("No fish found!");
                currentState = FishingState.Reeling;
                OnMiss?.Invoke();
                return;
            }

            // Generate fish size
            hookedFishSize = hookedFish.GetRandomSize();

            // Fish hooked!
            currentState = FishingState.Hooked;

            // Spawn bite indicator
            if (biteIndicatorPrefab != null && currentBobber != null)
            {
                GameObject indicator = Instantiate(biteIndicatorPrefab, currentBobber.transform.position, Quaternion.identity);
                indicator.transform.SetParent(currentBobber.transform);
                Destroy(indicator, 3f);
            }

            // Show VFX
            if (VFX.ParticleEffectManager.Instance != null)
            {
                VFX.ParticleEffectManager.Instance.SpawnEffect(VFX.EffectType.Sparkle, currentBobber.transform.position);
            }

            OnBite?.Invoke();

            Debug.Log($"Fish bite! {hookedFish.fishName} ({hookedFishSize:F1}cm)");
        }

        /// <summary>
        /// Start the fishing mini-game
        /// </summary>
        private void StartMiniGame()
        {
            if (hookedFish == null)
            {
                Debug.LogWarning("No hooked fish!");
                return;
            }

            currentState = FishingState.MiniGame;

            // Open mini-game UI
            if (UI.FishingMiniGameUI.Instance != null)
            {
                UI.FishingMiniGameUI.Instance.StartMiniGame(hookedFish, hookedFishSize, this);
            }
            else
            {
                Debug.LogWarning("FishingMiniGameUI not found!");

                // Fallback: auto-catch
                OnMiniGameComplete(true);
            }

            Debug.Log($"Mini-game started for {hookedFish.fishName}");
        }

        /// <summary>
        /// Mini-game completion callback
        /// </summary>
        public void OnMiniGameComplete(bool success)
        {
            if (success)
            {
                // Caught the fish!
                CatchFish(hookedFish, hookedFishSize);
            }
            else
            {
                // Fish got away
                Debug.Log($"{hookedFish.fishName} got away!");
                OnMiss?.Invoke();
            }

            // Clean up
            DestroyBobber();
            currentState = FishingState.Idle;
            currentSpot = null;
            hookedFish = null;
        }

        /// <summary>
        /// Catch fish successfully
        /// </summary>
        private void CatchFish(Fish fish, float size)
        {
            Debug.Log($"Caught {fish.fishName}! ({size:F1}cm)");

            // Add to inventory
            if (Inventory.InventorySystem.Instance != null)
            {
                // Fish should be added as items
                // Assuming fish items exist with matching IDs
                Inventory.InventorySystem.Instance.AddItem(fish.fishID, 1);
            }

            // Award experience
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.AddExp(fish.expReward);
            }

            // Track statistics
            if (Achievements.StatisticsTracker.Instance != null)
            {
                Achievements.StatisticsTracker.Instance.IncrementStatistic("fish_caught", 1f);
                Achievements.StatisticsTracker.Instance.IncrementStatistic($"fish_caught_{fish.rarity.ToString().ToLower()}", 1f);
            }

            // Trigger event
            OnCatch?.Invoke(fish, size);

            // Show VFX
            if (VFX.ParticleEffectManager.Instance != null)
            {
                VFX.ParticleEffectManager.Instance.SpawnEffect(VFX.EffectType.ItemPickup, transform.position);
            }
        }

        /// <summary>
        /// Spawn bobber at position
        /// </summary>
        private void SpawnBobber(Vector3 position)
        {
            if (bobberPrefab != null)
            {
                currentBobber = Instantiate(bobberPrefab, position, Quaternion.identity);
            }
            else
            {
                // Create default bobber
                currentBobber = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                currentBobber.name = "Bobber";
                currentBobber.transform.position = position;
                currentBobber.transform.localScale = Vector3.one * 0.3f;

                // Make it float
                Rigidbody rb = currentBobber.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.useGravity = false;
                }

                // Color it
                Renderer renderer = currentBobber.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.red;
                }
            }

            // Enable fishing line
            if (fishingLine != null)
            {
                fishingLine.enabled = true;
            }
        }

        /// <summary>
        /// Destroy bobber
        /// </summary>
        private void DestroyBobber()
        {
            if (currentBobber != null)
            {
                Destroy(currentBobber);
                currentBobber = null;
            }

            if (fishingLine != null)
            {
                fishingLine.enabled = false;
            }
        }

        /// <summary>
        /// Update fishing line visual
        /// </summary>
        private void UpdateFishingLine()
        {
            if (fishingLine != null && currentBobber != null)
            {
                fishingLine.SetPosition(0, rodTip.position);
                fishingLine.SetPosition(1, currentBobber.transform.position);
            }
        }

        /// <summary>
        /// Get cast target position from input
        /// </summary>
        private Vector3 GetCastTargetPosition()
        {
            if (InputManager.Instance != null)
            {
                return InputManager.Instance.GetPointerWorldPosition();
            }

            // Fallback: use mouse raycast
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxCastDistance, castableLayers))
            {
                return hit.point;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Find fishing spot at position
        /// </summary>
        private FishingSpot FindFishingSpotAtPosition(Vector3 position)
        {
            // Find fishing spots near cast position
            Collider[] colliders = Physics.OverlapSphere(position, 2f);

            foreach (Collider col in colliders)
            {
                FishingSpot spot = col.GetComponent<FishingSpot>();
                if (spot != null)
                {
                    return spot;
                }
            }

            return null;
        }

        /// <summary>
        /// Get current state
        /// </summary>
        public FishingState GetCurrentState()
        {
            return currentState;
        }

        /// <summary>
        /// Is fishing active?
        /// </summary>
        public bool IsFishing()
        {
            return currentState != FishingState.Idle;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw cast range
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, maxCastDistance);
        }
    }
}
