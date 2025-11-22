using UnityEngine;
using System.Collections.Generic;

namespace CozyGame
{
    /// <summary>
    /// Manages all audio in the game (music and sound effects)
    /// Singleton pattern for easy access
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [Tooltip("Dedicated source for background music")]
        public AudioSource musicSource;

        [Tooltip("Dedicated source for sound effects")]
        public AudioSource sfxSource;

        [Header("Sound Library")]
        [Tooltip("Add all your sound effects here with unique names")]
        public List<NamedAudioClip> soundEffects = new List<NamedAudioClip>();

        [Header("Volume Settings")]
        [Range(0f, 1f)]
        public float masterVolume = 1f;

        [Range(0f, 1f)]
        public float musicVolume = 0.7f;

        [Range(0f, 1f)]
        public float sfxVolume = 1f;

        private Dictionary<string, AudioClip> soundLibrary = new Dictionary<string, AudioClip>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSoundLibrary();
                CreateAudioSources();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void CreateAudioSources()
        {
            // Create music source if not assigned
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            // Create SFX source if not assigned
            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            UpdateVolumes();
        }

        private void InitializeSoundLibrary()
        {
            soundLibrary.Clear();
            foreach (var sound in soundEffects)
            {
                if (sound.clip != null && !soundLibrary.ContainsKey(sound.name))
                {
                    soundLibrary.Add(sound.name, sound.clip);
                }
            }

            Debug.Log($"AudioManager initialized with {soundLibrary.Count} sound effects");
        }

        /// <summary>
        /// Play a sound effect by name
        /// </summary>
        public void PlaySound(string soundName, float volumeMultiplier = 1f)
        {
            if (soundLibrary.TryGetValue(soundName, out AudioClip clip))
            {
                sfxSource.PlayOneShot(clip, volumeMultiplier * sfxVolume * masterVolume);
            }
            else
            {
                Debug.LogWarning($"Sound '{soundName}' not found in library!");
            }
        }

        /// <summary>
        /// Play a sound at a specific position in the world (3D sound)
        /// </summary>
        public void PlaySoundAtPosition(string soundName, Vector3 position, float volumeMultiplier = 1f)
        {
            if (soundLibrary.TryGetValue(soundName, out AudioClip clip))
            {
                AudioSource.PlayClipAtPoint(clip, position, volumeMultiplier * sfxVolume * masterVolume);
            }
            else
            {
                Debug.LogWarning($"Sound '{soundName}' not found in library!");
            }
        }

        /// <summary>
        /// Play music (loops automatically)
        /// </summary>
        public void PlayMusic(AudioClip musicClip, bool fadeIn = false, float fadeDuration = 1f)
        {
            if (musicClip == null)
            {
                Debug.LogWarning("Cannot play null music clip!");
                return;
            }

            // If already playing this music, don't restart
            if (musicSource.clip == musicClip && musicSource.isPlaying)
                return;

            if (fadeIn && musicSource.isPlaying)
            {
                StartCoroutine(FadeMusic(musicSource.clip, musicClip, fadeDuration));
            }
            else
            {
                musicSource.clip = musicClip;
                musicSource.loop = true;
                musicSource.volume = musicVolume * masterVolume;
                musicSource.Play();
            }
        }

        /// <summary>
        /// Play music by name from sound library
        /// </summary>
        public void PlayMusic(string musicName, bool fadeIn = false, float fadeDuration = 1f)
        {
            if (soundLibrary.TryGetValue(musicName, out AudioClip clip))
            {
                PlayMusic(clip, fadeIn, fadeDuration);
            }
            else
            {
                Debug.LogWarning($"Music '{musicName}' not found in library!");
            }
        }

        /// <summary>
        /// Stop music
        /// </summary>
        public void StopMusic(bool fadeOut = false, float fadeDuration = 1f)
        {
            if (fadeOut && musicSource.isPlaying)
            {
                StartCoroutine(FadeOutMusic(fadeDuration));
            }
            else
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// Pause music
        /// </summary>
        public void PauseMusic()
        {
            musicSource.Pause();
        }

        /// <summary>
        /// Resume paused music
        /// </summary>
        public void ResumeMusic()
        {
            musicSource.UnPause();
        }

        /// <summary>
        /// Update all volume settings
        /// </summary>
        public void UpdateVolumes()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;

            if (sfxSource != null)
                sfxSource.volume = sfxVolume * masterVolume;
        }

        /// <summary>
        /// Set master volume (affects all audio)
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        /// <summary>
        /// Set music volume
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        /// <summary>
        /// Set sound effects volume
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        // Coroutine for fading between music tracks
        private System.Collections.IEnumerator FadeMusic(AudioClip fromClip, AudioClip toClip, float duration)
        {
            float elapsed = 0f;
            float startVolume = musicSource.volume;

            // Fade out current music
            while (elapsed < duration / 2f)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (duration / 2f));
                yield return null;
            }

            // Switch to new music
            musicSource.clip = toClip;
            musicSource.Play();

            // Fade in new music
            elapsed = 0f;
            float targetVolume = musicVolume * masterVolume;
            while (elapsed < duration / 2f)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / (duration / 2f));
                yield return null;
            }

            musicSource.volume = targetVolume;
        }

        // Coroutine for fading out music
        private System.Collections.IEnumerator FadeOutMusic(float duration)
        {
            float elapsed = 0f;
            float startVolume = musicSource.volume;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = musicVolume * masterVolume;
        }
    }

    /// <summary>
    /// Helper class for organizing sound effects in the inspector
    /// </summary>
    [System.Serializable]
    public class NamedAudioClip
    {
        [Tooltip("Unique name to reference this sound")]
        public string name;

        [Tooltip("The audio clip file")]
        public AudioClip clip;
    }
}
