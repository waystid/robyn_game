using UnityEngine;

namespace CozyGame.VFX
{
    /// <summary>
    /// Spell effect controller.
    /// Handles spell casting and hit visual effects.
    /// </summary>
    public class SpellEffect : MonoBehaviour
    {
        [Header("Cast Effect")]
        [Tooltip("Casting particle effect")]
        public ParticleSystem castParticles;

        [Tooltip("Cast sound")]
        public string castSound = "spell_cast";

        [Header("Projectile")]
        [Tooltip("Projectile particle effect")]
        public ParticleSystem projectileParticles;

        [Tooltip("Projectile speed")]
        public float projectileSpeed = 10f;

        [Header("Hit Effect")]
        [Tooltip("Impact particle effect")]
        public ParticleSystem hitParticles;

        [Tooltip("Hit sound")]
        public string hitSound = "spell_hit";

        [Header("Trail")]
        [Tooltip("Trail renderer")]
        public TrailRenderer trail;

        /// <summary>
        /// Play cast effect
        /// </summary>
        public void PlayCastEffect(Vector3 position, Quaternion rotation)
        {
            if (castParticles != null)
            {
                castParticles.transform.position = position;
                castParticles.transform.rotation = rotation;
                castParticles.Play();
            }

            if (AudioManager.Instance != null && !string.IsNullOrEmpty(castSound))
            {
                AudioManager.Instance.PlaySound(castSound);
            }
        }

        /// <summary>
        /// Play hit effect
        /// </summary>
        public void PlayHitEffect(Vector3 position, Vector3 normal)
        {
            if (hitParticles != null)
            {
                hitParticles.transform.position = position;
                hitParticles.transform.rotation = Quaternion.LookRotation(normal);
                hitParticles.Play();
            }

            if (AudioManager.Instance != null && !string.IsNullOrEmpty(hitSound))
            {
                AudioManager.Instance.PlaySound(hitSound);
            }
        }

        /// <summary>
        /// Fire projectile
        /// </summary>
        public void FireProjectile(Vector3 startPosition, Vector3 direction, float distance = 50f)
        {
            if (projectileParticles != null)
            {
                projectileParticles.transform.position = startPosition;
                projectileParticles.transform.rotation = Quaternion.LookRotation(direction);
                projectileParticles.Play();
            }

            // Simple raycast to detect hit
            RaycastHit hit;
            if (Physics.Raycast(startPosition, direction, out hit, distance))
            {
                PlayHitEffect(hit.point, hit.normal);
            }
        }
    }

    /// <summary>
    /// Plant growth effect controller.
    /// Handles plant growth and harvest particle effects.
    /// </summary>
    public class PlantGrowthEffect : MonoBehaviour
    {
        [Header("Growth Effect")]
        [Tooltip("Growth sparkle particles")]
        public ParticleSystem growthParticles;

        [Tooltip("Growth sound")]
        public string growthSound = "plant_grow";

        [Tooltip("Growth duration")]
        public float growthDuration = 2f;

        [Header("Harvest Effect")]
        [Tooltip("Harvest particle burst")]
        public ParticleSystem harvestParticles;

        [Tooltip("Harvest sound")]
        public string harvestSound = "plant_harvest";

        [Header("Colors")]
        [Tooltip("Growth particle color")]
        public Gradient growthColorGradient;

        /// <summary>
        /// Play growth effect
        /// </summary>
        public void PlayGrowthEffect()
        {
            if (growthParticles != null)
            {
                growthParticles.Play();
            }

            if (AudioManager.Instance != null && !string.IsNullOrEmpty(growthSound))
            {
                AudioManager.Instance.PlaySound(growthSound);
            }

            // Stop after duration
            if (growthParticles != null)
            {
                Invoke("StopGrowthEffect", growthDuration);
            }
        }

        /// <summary>
        /// Stop growth effect
        /// </summary>
        public void StopGrowthEffect()
        {
            if (growthParticles != null)
            {
                growthParticles.Stop();
            }
        }

        /// <summary>
        /// Play harvest effect
        /// </summary>
        public void PlayHarvestEffect()
        {
            if (harvestParticles != null)
            {
                harvestParticles.Play();
            }

            if (AudioManager.Instance != null && !string.IsNullOrEmpty(harvestSound))
            {
                AudioManager.Instance.PlaySound(harvestSound);
            }
        }
    }

    /// <summary>
    /// Level up effect controller.
    /// Celebratory particle effect when player levels up.
    /// </summary>
    public class LevelUpEffect : MonoBehaviour
    {
        public static LevelUpEffect Instance { get; private set; }

        [Header("Effect")]
        [Tooltip("Level up particle system")]
        public ParticleSystem levelUpParticles;

        [Tooltip("Effect duration")]
        public float effectDuration = 3f;

        [Header("Light")]
        [Tooltip("Flash light")]
        public Light flashLight;

        [Tooltip("Light duration")]
        public float lightDuration = 0.5f;

        [Tooltip("Light intensity")]
        public float lightIntensity = 5f;

        [Header("Audio")]
        [Tooltip("Level up sound")]
        public string levelUpSound = "level_up";

        [Header("Camera")]
        [Tooltip("Camera shake intensity")]
        public float shakeIntensity = 0.3f;

        [Tooltip("Camera shake duration")]
        public float shakeDuration = 0.5f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Subscribe to level up event
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnLevelUp.AddListener(OnPlayerLevelUp);
            }
        }

        private void OnDestroy()
        {
            if (PlayerStats.Instance != null)
            {
                PlayerStats.Instance.OnLevelUp.RemoveListener(OnPlayerLevelUp);
            }
        }

        /// <summary>
        /// Player level up callback
        /// </summary>
        private void OnPlayerLevelUp(int newLevel)
        {
            PlayLevelUpEffect();
        }

        /// <summary>
        /// Play level up effect
        /// </summary>
        public void PlayLevelUpEffect()
        {
            // Play particles
            if (levelUpParticles != null)
            {
                levelUpParticles.Play();
            }

            // Flash light
            if (flashLight != null)
            {
                flashLight.enabled = true;
                flashLight.intensity = lightIntensity;
                Invoke("DisableLight", lightDuration);
            }

            // Play sound
            if (AudioManager.Instance != null && !string.IsNullOrEmpty(levelUpSound))
            {
                AudioManager.Instance.PlaySound(levelUpSound);
            }

            // Camera shake (if CameraController exists)
            if (CameraController.Instance != null)
            {
                // CameraController.Instance.Shake(shakeIntensity, shakeDuration);
            }

            // Spawn particle effect at player
            if (ParticleEffectManager.Instance != null && PlayerController.Instance != null)
            {
                ParticleEffectManager.Instance.SpawnEffect(
                    EffectType.LevelUp,
                    PlayerController.Instance.transform.position,
                    Quaternion.identity
                );
            }
        }

        /// <summary>
        /// Disable flash light
        /// </summary>
        private void DisableLight()
        {
            if (flashLight != null)
            {
                flashLight.enabled = false;
            }
        }
    }

    /// <summary>
    /// Environmental particle controller.
    /// Ambient particles like fireflies, leaves, dust, etc.
    /// </summary>
    public class EnvironmentalParticles : MonoBehaviour
    {
        [Header("Particle Systems")]
        [Tooltip("Fireflies")]
        public ParticleSystem fireflies;

        [Tooltip("Falling leaves")]
        public ParticleSystem fallingLeaves;

        [Tooltip("Dust motes")]
        public ParticleSystem dustMotes;

        [Tooltip("Magic sparkles")]
        public ParticleSystem magicSparkles;

        [Header("Time of Day")]
        [Tooltip("Enable fireflies at night")]
        public bool enableFirefliesAtNight = true;

        [Tooltip("Night start hour")]
        [Range(0f, 24f)]
        public float nightStartHour = 20f;

        [Tooltip("Night end hour")]
        [Range(0f, 24f)]
        public float nightEndHour = 6f;

        [Header("Weather")]
        [Tooltip("Disable particles in rain")]
        public bool disableInRain = true;

        private void Start()
        {
            UpdateEnvironmentalParticles();
        }

        private void Update()
        {
            // Update environmental particles based on time/weather
            // This would integrate with a day/night and weather system
            UpdateEnvironmentalParticles();
        }

        /// <summary>
        /// Update environmental particle states
        /// </summary>
        private void UpdateEnvironmentalParticles()
        {
            // TODO: Get actual time of day from game manager
            float currentHour = 12f; // Placeholder

            // Fireflies at night
            if (fireflies != null && enableFirefliesAtNight)
            {
                bool isNight = (currentHour >= nightStartHour || currentHour < nightEndHour);

                if (isNight && !fireflies.isPlaying)
                {
                    fireflies.Play();
                }
                else if (!isNight && fireflies.isPlaying)
                {
                    fireflies.Stop();
                }
            }
        }

        /// <summary>
        /// Set weather state
        /// </summary>
        public void SetWeather(string weather)
        {
            bool isRaining = (weather == "Rain" || weather == "Storm");

            if (disableInRain && isRaining)
            {
                if (dustMotes != null) dustMotes.Stop();
                if (magicSparkles != null) magicSparkles.Stop();
            }
            else
            {
                if (dustMotes != null && !dustMotes.isPlaying) dustMotes.Play();
                if (magicSparkles != null && !magicSparkles.isPlaying) magicSparkles.Play();
            }
        }
    }
}
