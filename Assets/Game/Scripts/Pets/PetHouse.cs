using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace CozyGame.Pets
{
    /// <summary>
    /// Pet house/stable building.
    /// Provides resting area for pets with faster recovery rates.
    /// Interactable to open pet management UI.
    /// </summary>
    public class PetHouse : MonoBehaviour, Interaction.IInteractable
    {
        [Header("House Settings")]
        [Tooltip("House name")]
        public string houseName = "Pet House";

        [Tooltip("Maximum pets that can rest here")]
        public int capacity = 5;

        [Tooltip("Hunger recovery rate multiplier (1.0 = normal, 2.0 = double)")]
        public float hungerRecoveryMultiplier = 2f;

        [Tooltip("Happiness recovery rate multiplier")]
        public float happinessRecoveryMultiplier = 2f;

        [Tooltip("Passive hunger decrease rate when resting (0 = no decrease)")]
        public float restingHungerDecreaseRate = 0f;

        [Tooltip("Passive happiness increase rate when resting")]
        public float restingHappinessIncreaseRate = 1f;

        [Header("Visual")]
        [Tooltip("Door/entrance position for pets")]
        public Transform entrancePosition;

        [Tooltip("Resting positions for pets")]
        public Transform[] restingPositions;

        [Tooltip("House particle effects (sparkles, etc.)")]
        public GameObject houseEffectPrefab;

        [Header("Interaction")]
        [Tooltip("Interaction prompt text")]
        public string interactionPrompt = "Open Pet House";

        [Tooltip("Interaction range")]
        public float interactionRange = 3f;

        [Header("Events")]
        public UnityEvent<PetController> OnPetEntered;
        public UnityEvent<PetController> OnPetExited;
        public UnityEvent OnHouseOpened;

        // State
        private List<PetController> restingPets = new List<PetController>();
        private GameObject houseEffect;

        private void Start()
        {
            // Spawn house effect
            if (houseEffectPrefab != null)
            {
                houseEffect = Instantiate(houseEffectPrefab, transform);
            }

            // Initialize entrance position if not set
            if (entrancePosition == null)
            {
                entrancePosition = transform;
            }
        }

        private void Update()
        {
            // Apply resting bonuses to pets in house
            foreach (var pet in restingPets)
            {
                if (pet != null && pet.currentState == PetState.Resting)
                {
                    ApplyRestingBonus(pet);
                }
            }
        }

        /// <summary>
        /// Apply resting bonuses to pet
        /// </summary>
        private void ApplyRestingBonus(PetController pet)
        {
            // Slower hunger decrease (or no decrease)
            if (restingHungerDecreaseRate >= 0 && pet.petData != null)
            {
                float normalDecrease = pet.petData.hungerDecreaseRate * Time.deltaTime;
                float restingDecrease = restingHungerDecreaseRate * Time.deltaTime;
                float savedHunger = normalDecrease - restingDecrease;
                pet.currentHunger = Mathf.Min(pet.currentHunger + savedHunger, pet.petData.maxHunger);
            }

            // Passive happiness increase
            if (restingHappinessIncreaseRate > 0 && pet.petData != null)
            {
                pet.currentHappiness += restingHappinessIncreaseRate * Time.deltaTime;
                pet.currentHappiness = Mathf.Min(pet.currentHappiness, pet.petData.maxHappiness);
            }
        }

        /// <summary>
        /// Pet enters house
        /// </summary>
        public bool PetEnter(PetController pet)
        {
            if (pet == null)
                return false;

            // Check capacity
            if (restingPets.Count >= capacity)
            {
                Debug.LogWarning($"[PetHouse] {houseName} is at full capacity!");
                return false;
            }

            // Add to resting list
            if (!restingPets.Contains(pet))
            {
                restingPets.Add(pet);

                // Move pet to resting position
                Vector3 restPos = GetAvailableRestingPosition();
                if (pet.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
                {
                    agent.SetDestination(restPos);
                }

                // Set pet to resting state
                pet.currentState = PetState.Resting;

                OnPetEntered?.Invoke(pet);

                Debug.Log($"[PetHouse] {pet.GetDisplayName()} entered {houseName}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Pet exits house
        /// </summary>
        public bool PetExit(PetController pet)
        {
            if (pet == null)
                return false;

            if (restingPets.Contains(pet))
            {
                restingPets.Remove(pet);

                // Move pet to entrance
                if (pet.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
                {
                    agent.SetDestination(entrancePosition.position);
                }

                // Change pet state back to idle
                if (pet.currentState == PetState.Resting)
                {
                    pet.currentState = PetState.Idle;
                }

                OnPetExited?.Invoke(pet);

                Debug.Log($"[PetHouse] {pet.GetDisplayName()} exited {houseName}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get available resting position
        /// </summary>
        private Vector3 GetAvailableRestingPosition()
        {
            // Use defined resting positions if available
            if (restingPositions != null && restingPositions.Length > 0)
            {
                int index = restingPets.Count % restingPositions.Length;
                return restingPositions[index].position;
            }

            // Otherwise use random position near house
            Vector3 randomOffset = new Vector3(
                Random.Range(-2f, 2f),
                0f,
                Random.Range(-2f, 2f)
            );

            return transform.position + randomOffset;
        }

        /// <summary>
        /// Get resting pets
        /// </summary>
        public List<PetController> GetRestingPets()
        {
            return new List<PetController>(restingPets);
        }

        /// <summary>
        /// Get available capacity
        /// </summary>
        public int GetAvailableCapacity()
        {
            return capacity - restingPets.Count;
        }

        /// <summary>
        /// Check if house has space
        /// </summary>
        public bool HasSpace()
        {
            return restingPets.Count < capacity;
        }

        // ========== IInteractable Implementation ==========

        public string GetInteractionPrompt()
        {
            return interactionPrompt;
        }

        public float GetInteractionRange()
        {
            return interactionRange;
        }

        public bool CanInteract(GameObject interactor)
        {
            return interactor != null && interactor.CompareTag("Player");
        }

        public void Interact(GameObject interactor)
        {
            // Open pet UI when interacting with house
            if (UI.PetUI.Instance != null)
            {
                UI.PetUI.Instance.Show();
            }
            else
            {
                // Fallback: search for PetUI in scene
                UI.PetUI petUI = FindObjectOfType<UI.PetUI>();
                if (petUI != null)
                {
                    petUI.Show();
                }
            }

            OnHouseOpened?.Invoke();

            Debug.Log($"[PetHouse] Opened {houseName}");
        }

        private void OnDrawGizmosSelected()
        {
            // Draw interaction range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);

            // Draw resting positions
            if (restingPositions != null)
            {
                Gizmos.color = Color.cyan;
                foreach (var pos in restingPositions)
                {
                    if (pos != null)
                    {
                        Gizmos.DrawWireSphere(pos.position, 0.5f);
                    }
                }
            }

            // Draw entrance
            if (entrancePosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(entrancePosition.position, 0.3f);
            }
        }
    }
}
