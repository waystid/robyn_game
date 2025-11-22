using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace CozyGame.Pets
{
    /// <summary>
    /// Pet save data
    /// </summary>
    [System.Serializable]
    public class PetSaveData
    {
        public string instanceID;
        public string petID;
        public string customName;
        public int level;
        public int currentExp;
        public float currentHealth;
        public float currentHunger;
        public float currentHappiness;
        public float loyalty;
        public string equippedAccessory;
        public bool isActive;

        public PetSaveData()
        {
        }

        public PetSaveData(PetController pet)
        {
            instanceID = pet.instanceID;
            petID = pet.petData != null ? pet.petData.petID : "";
            customName = pet.customName;
            level = pet.level;
            currentExp = pet.currentExp;
            currentHealth = pet.currentHealth;
            currentHunger = pet.currentHunger;
            currentHappiness = pet.currentHappiness;
            loyalty = pet.loyalty;
            equippedAccessory = pet.equippedAccessory;
            isActive = pet.isActive;
        }
    }

    /// <summary>
    /// Pet manager singleton.
    /// Manages pet collection, summoning, and storage.
    /// </summary>
    public class PetManager : MonoBehaviour
    {
        public static PetManager Instance { get; private set; }

        [Header("Settings")]
        [Tooltip("Max pets that can be owned")]
        public int maxPetsOwned = 10;

        [Tooltip("Max active pets (summoned)")]
        public int maxActivePets = 1;

        [Tooltip("Pet spawn point offset")]
        public Vector3 spawnOffset = new Vector3(2f, 0f, 0f);

        [Header("Pet Database")]
        [Tooltip("All available pets")]
        public Pet[] petDatabase;

        [Header("Events")]
        public UnityEvent<PetController> OnPetAdded;
        public UnityEvent<PetController> OnPetRemoved;
        public UnityEvent<PetController> OnPetSummoned;
        public UnityEvent<PetController> OnPetDismissed;

        // Pet collection
        private Dictionary<string, Pet> petLookup = new Dictionary<string, Pet>();
        private List<PetController> ownedPets = new List<PetController>();
        private List<PetController> activePets = new List<PetController>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initialize pet manager
        /// </summary>
        private void Initialize()
        {
            // Build pet lookup
            LoadPetDatabase();

            Debug.Log($"[PetManager] Initialized with {petLookup.Count} pet types");
        }

        /// <summary>
        /// Load pet database
        /// </summary>
        private void LoadPetDatabase()
        {
            petLookup.Clear();

            // Load from assigned database
            if (petDatabase != null)
            {
                foreach (var pet in petDatabase)
                {
                    if (pet != null && !string.IsNullOrEmpty(pet.petID))
                    {
                        petLookup[pet.petID] = pet;
                    }
                }
            }

            // Also load from Resources
            Pet[] resourcePets = Resources.LoadAll<Pet>("Pets");
            foreach (var pet in resourcePets)
            {
                if (pet != null && !string.IsNullOrEmpty(pet.petID))
                {
                    if (!petLookup.ContainsKey(pet.petID))
                    {
                        petLookup[pet.petID] = pet;
                    }
                }
            }
        }

        /// <summary>
        /// Get pet by ID
        /// </summary>
        public Pet GetPetData(string petID)
        {
            if (petLookup.TryGetValue(petID, out Pet pet))
            {
                return pet;
            }

            return null;
        }

        /// <summary>
        /// Add pet to collection
        /// </summary>
        public PetController AddPet(string petID, string customName = "")
        {
            if (ownedPets.Count >= maxPetsOwned)
            {
                Debug.LogWarning("[PetManager] Max pets owned reached!");
                return null;
            }

            Pet petData = GetPetData(petID);
            if (petData == null)
            {
                Debug.LogError($"[PetManager] Pet not found: {petID}");
                return null;
            }

            // Create pet instance
            GameObject petObj = Instantiate(petData.prefab, transform);
            petObj.name = $"Pet_{petData.petName}";

            PetController pet = petObj.GetComponent<PetController>();
            if (pet == null)
            {
                pet = petObj.AddComponent<PetController>();
            }

            pet.petData = petData;
            pet.customName = customName;
            pet.instanceID = System.Guid.NewGuid().ToString();
            pet.isActive = false; // Not summoned yet

            // Deactivate until summoned
            petObj.SetActive(false);

            ownedPets.Add(pet);
            OnPetAdded?.Invoke(pet);

            Debug.Log($"[PetManager] Added pet: {petData.petName}");
            return pet;
        }

        /// <summary>
        /// Remove pet from collection
        /// </summary>
        public bool RemovePet(string instanceID)
        {
            PetController pet = ownedPets.FirstOrDefault(p => p.instanceID == instanceID);
            if (pet == null)
                return false;

            // Dismiss if active
            if (pet.isActive)
            {
                DismissPet(instanceID);
            }

            ownedPets.Remove(pet);
            OnPetRemoved?.Invoke(pet);

            Destroy(pet.gameObject);

            Debug.Log($"[PetManager] Removed pet: {pet.GetDisplayName()}");
            return true;
        }

        /// <summary>
        /// Summon pet (make active)
        /// </summary>
        public bool SummonPet(string instanceID)
        {
            if (activePets.Count >= maxActivePets)
            {
                Debug.LogWarning("[PetManager] Max active pets reached!");
                return false;
            }

            PetController pet = ownedPets.FirstOrDefault(p => p.instanceID == instanceID);
            if (pet == null)
            {
                Debug.LogWarning($"[PetManager] Pet not found: {instanceID}");
                return false;
            }

            if (pet.isActive)
            {
                Debug.LogWarning($"[PetManager] Pet already active: {pet.GetDisplayName()}");
                return false;
            }

            // Position pet near player
            if (PlayerController.Instance != null)
            {
                pet.transform.position = PlayerController.Instance.transform.position + spawnOffset;
                pet.transform.rotation = PlayerController.Instance.transform.rotation;
            }

            pet.gameObject.SetActive(true);
            pet.isActive = true;
            pet.owner = PlayerController.Instance != null ? PlayerController.Instance.transform : null;

            activePets.Add(pet);
            OnPetSummoned?.Invoke(pet);

            // Spawn summon effect
            if (VFX.ParticleEffectManager.Instance != null)
            {
                VFX.ParticleEffectManager.Instance.SpawnEffect(
                    VFX.EffectType.Sparkle,
                    pet.transform.position,
                    Quaternion.identity
                );
            }

            Debug.Log($"[PetManager] Summoned pet: {pet.GetDisplayName()}");
            return true;
        }

        /// <summary>
        /// Dismiss pet (make inactive)
        /// </summary>
        public bool DismissPet(string instanceID)
        {
            PetController pet = activePets.FirstOrDefault(p => p.instanceID == instanceID);
            if (pet == null)
                return false;

            pet.isActive = false;
            pet.gameObject.SetActive(false);

            activePets.Remove(pet);
            OnPetDismissed?.Invoke(pet);

            Debug.Log($"[PetManager] Dismissed pet: {pet.GetDisplayName()}");
            return true;
        }

        /// <summary>
        /// Dismiss all pets
        /// </summary>
        public void DismissAllPets()
        {
            foreach (var pet in activePets.ToList())
            {
                DismissPet(pet.instanceID);
            }
        }

        /// <summary>
        /// Get owned pets
        /// </summary>
        public List<PetController> GetOwnedPets()
        {
            return new List<PetController>(ownedPets);
        }

        /// <summary>
        /// Get active pets
        /// </summary>
        public List<PetController> GetActivePets()
        {
            return new List<PetController>(activePets);
        }

        /// <summary>
        /// Get pet by instance ID
        /// </summary>
        public PetController GetPet(string instanceID)
        {
            return ownedPets.FirstOrDefault(p => p.instanceID == instanceID);
        }

        /// <summary>
        /// Check if has pet
        /// </summary>
        public bool HasPet(string instanceID)
        {
            return ownedPets.Any(p => p.instanceID == instanceID);
        }

        /// <summary>
        /// Get pet count
        /// </summary>
        public int GetPetCount()
        {
            return ownedPets.Count;
        }

        /// <summary>
        /// Get active pet count
        /// </summary>
        public int GetActivePetCount()
        {
            return activePets.Count;
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public List<PetSaveData> GetSaveData()
        {
            List<PetSaveData> saveData = new List<PetSaveData>();

            foreach (var pet in ownedPets)
            {
                saveData.Add(new PetSaveData(pet));
            }

            return saveData;
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(List<PetSaveData> saveData)
        {
            // Clear existing pets
            foreach (var pet in ownedPets.ToList())
            {
                RemovePet(pet.instanceID);
            }

            if (saveData == null)
                return;

            // Create pets from save data
            foreach (var data in saveData)
            {
                Pet petData = GetPetData(data.petID);
                if (petData == null)
                    continue;

                // Create pet instance
                GameObject petObj = Instantiate(petData.prefab, transform);
                PetController pet = petObj.GetComponent<PetController>();
                if (pet == null)
                {
                    pet = petObj.AddComponent<PetController>();
                }

                // Load data
                pet.petData = petData;
                pet.instanceID = data.instanceID;
                pet.customName = data.customName;
                pet.level = data.level;
                pet.currentExp = data.currentExp;
                pet.currentHealth = data.currentHealth;
                pet.currentHunger = data.currentHunger;
                pet.currentHappiness = data.currentHappiness;
                pet.loyalty = data.loyalty;
                pet.equippedAccessory = data.equippedAccessory;

                petObj.SetActive(false); // Start inactive
                ownedPets.Add(pet);

                // Re-summon if was active
                if (data.isActive)
                {
                    SummonPet(data.instanceID);
                }
            }

            Debug.Log($"[PetManager] Loaded {saveData.Count} pets");
        }
    }
}
