using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CozyGame
{
    /// <summary>
    /// Manages voice line playback and audio synthesis for dialogue.
    /// Supports pre-recorded voice lines and procedural voice blips.
    /// </summary>
    public class VoiceLineManager : MonoBehaviour
    {
        public static VoiceLineManager Instance { get; private set; }

        [Header("Voice Line Settings")]
        [Tooltip("Library of voice line audio clips by name")]
        public List<VoiceLineEntry> voiceLineLibrary = new List<VoiceLineEntry>();

        [Tooltip("Default voice blip for typing effects")]
        public AudioClip defaultVoiceBlip;

        [Tooltip("Voice blip pitch range for variety")]
        public Vector2 blipPitchRange = new Vector2(0.9f, 1.1f);

        [Header("Character Voice Presets")]
        [Tooltip("Voice presets for different characters")]
        public List<CharacterVoicePreset> characterVoices = new List<CharacterVoicePreset>();

        [Header("Audio Settings")]
        [Tooltip("Master volume for voice lines")]
        [Range(0f, 1f)]
        public float voiceLineVolume = 0.8f;

        [Tooltip("Master volume for voice blips")]
        [Range(0f, 1f)]
        public float voiceBlipVolume = 0.5f;

        [Tooltip("Fade in duration for voice lines (seconds)")]
        public float fadeInDuration = 0.1f;

        [Tooltip("Fade out duration for voice lines (seconds)")]
        public float fadeOutDuration = 0.3f;

        // Audio sources
        private AudioSource voiceLineSource;
        private AudioSource voiceBlipSource;

        // Voice line library lookup
        private Dictionary<string, AudioClip> voiceLineDict = new Dictionary<string, AudioClip>();
        private Dictionary<string, CharacterVoicePreset> voicePresetDict = new Dictionary<string, CharacterVoicePreset>();

        // Playback state
        private Coroutine currentFadeCoroutine;

        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
                BuildVoiceLibrary();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAudioSources()
        {
            // Create audio source for voice lines
            voiceLineSource = gameObject.AddComponent<AudioSource>();
            voiceLineSource.playOnAwake = false;
            voiceLineSource.loop = false;
            voiceLineSource.volume = voiceLineVolume;

            // Create audio source for voice blips
            voiceBlipSource = gameObject.AddComponent<AudioSource>();
            voiceBlipSource.playOnAwake = false;
            voiceBlipSource.loop = false;
            voiceBlipSource.volume = voiceBlipVolume;
        }

        private void BuildVoiceLibrary()
        {
            // Build voice line dictionary
            voiceLineDict.Clear();
            foreach (VoiceLineEntry entry in voiceLineLibrary)
            {
                if (!string.IsNullOrEmpty(entry.lineName) && entry.audioClip != null)
                {
                    voiceLineDict[entry.lineName] = entry.audioClip;
                }
            }

            // Build voice preset dictionary
            voicePresetDict.Clear();
            foreach (CharacterVoicePreset preset in characterVoices)
            {
                if (!string.IsNullOrEmpty(preset.characterName))
                {
                    voicePresetDict[preset.characterName] = preset;
                }
            }
        }

        /// <summary>
        /// Play a voice line by name
        /// </summary>
        public bool PlayVoiceLine(string lineName, bool fadeIn = true)
        {
            if (string.IsNullOrEmpty(lineName))
                return false;

            if (!voiceLineDict.TryGetValue(lineName, out AudioClip clip))
            {
                Debug.LogWarning($"[VoiceLineManager] Voice line '{lineName}' not found in library");
                return false;
            }

            return PlayVoiceLine(clip, fadeIn);
        }

        /// <summary>
        /// Play a voice line audio clip
        /// </summary>
        public bool PlayVoiceLine(AudioClip clip, bool fadeIn = true)
        {
            if (clip == null)
                return false;

            StopVoiceLine();

            voiceLineSource.clip = clip;

            if (fadeIn && fadeInDuration > 0f)
            {
                if (currentFadeCoroutine != null)
                    StopCoroutine(currentFadeCoroutine);

                currentFadeCoroutine = StartCoroutine(FadeInVoiceLine());
            }
            else
            {
                voiceLineSource.volume = voiceLineVolume;
                voiceLineSource.Play();
            }

            return true;
        }

        /// <summary>
        /// Stop currently playing voice line
        /// </summary>
        public void StopVoiceLine(bool fadeOut = true)
        {
            if (!voiceLineSource.isPlaying)
                return;

            if (fadeOut && fadeOutDuration > 0f)
            {
                if (currentFadeCoroutine != null)
                    StopCoroutine(currentFadeCoroutine);

                currentFadeCoroutine = StartCoroutine(FadeOutVoiceLine());
            }
            else
            {
                voiceLineSource.Stop();
            }
        }

        /// <summary>
        /// Play a voice blip sound
        /// </summary>
        public void PlayVoiceBlip(AudioClip blipClip = null, float pitchVariation = 0.1f)
        {
            AudioClip clipToPlay = blipClip != null ? blipClip : defaultVoiceBlip;

            if (clipToPlay == null)
                return;

            // Randomize pitch for variety
            float randomPitch = Random.Range(
                Mathf.Max(blipPitchRange.x, 1f - pitchVariation),
                Mathf.Min(blipPitchRange.y, 1f + pitchVariation)
            );

            voiceBlipSource.pitch = randomPitch;
            voiceBlipSource.PlayOneShot(clipToPlay, voiceBlipVolume);
        }

        /// <summary>
        /// Play voice blip for a specific character
        /// </summary>
        public void PlayCharacterVoiceBlip(string characterName)
        {
            CharacterVoicePreset preset = GetVoicePreset(characterName);

            if (preset != null && preset.voiceBlip != null)
            {
                PlayVoiceBlip(preset.voiceBlip, preset.blipPitchVariation);
            }
            else
            {
                PlayVoiceBlip();
            }
        }

        /// <summary>
        /// Get voice preset for a character
        /// </summary>
        public CharacterVoicePreset GetVoicePreset(string characterName)
        {
            if (voicePresetDict.TryGetValue(characterName, out CharacterVoicePreset preset))
            {
                return preset;
            }

            return null;
        }

        /// <summary>
        /// Add voice line to library at runtime
        /// </summary>
        public void AddVoiceLine(string lineName, AudioClip clip)
        {
            if (string.IsNullOrEmpty(lineName) || clip == null)
                return;

            voiceLineDict[lineName] = clip;
        }

        /// <summary>
        /// Check if a voice line exists in library
        /// </summary>
        public bool HasVoiceLine(string lineName)
        {
            return voiceLineDict.ContainsKey(lineName);
        }

        /// <summary>
        /// Check if a voice line is currently playing
        /// </summary>
        public bool IsPlaying()
        {
            return voiceLineSource.isPlaying;
        }

        /// <summary>
        /// Set master voice line volume
        /// </summary>
        public void SetVoiceLineVolume(float volume)
        {
            voiceLineVolume = Mathf.Clamp01(volume);
            voiceLineSource.volume = voiceLineVolume;
        }

        /// <summary>
        /// Set master voice blip volume
        /// </summary>
        public void SetVoiceBlipVolume(float volume)
        {
            voiceBlipVolume = Mathf.Clamp01(volume);
            voiceBlipSource.volume = voiceBlipVolume;
        }

        // Fade in coroutine
        private IEnumerator FadeInVoiceLine()
        {
            voiceLineSource.volume = 0f;
            voiceLineSource.Play();

            float elapsed = 0f;

            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                voiceLineSource.volume = Mathf.Lerp(0f, voiceLineVolume, elapsed / fadeInDuration);
                yield return null;
            }

            voiceLineSource.volume = voiceLineVolume;
            currentFadeCoroutine = null;
        }

        // Fade out coroutine
        private IEnumerator FadeOutVoiceLine()
        {
            float startVolume = voiceLineSource.volume;
            float elapsed = 0f;

            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                voiceLineSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeOutDuration);
                yield return null;
            }

            voiceLineSource.volume = 0f;
            voiceLineSource.Stop();
            currentFadeCoroutine = null;
        }
    }

    /// <summary>
    /// Voice line library entry
    /// </summary>
    [System.Serializable]
    public class VoiceLineEntry
    {
        [Tooltip("Unique name for this voice line")]
        public string lineName = "dragon_greeting_01";

        [Tooltip("Audio clip for this voice line")]
        public AudioClip audioClip;
    }

    /// <summary>
    /// Character voice preset
    /// </summary>
    [System.Serializable]
    public class CharacterVoicePreset
    {
        [Tooltip("Character name (should match NPC name)")]
        public string characterName = "Dragon";

        [Tooltip("Voice blip sound for this character")]
        public AudioClip voiceBlip;

        [Tooltip("Pitch variation for voice blips")]
        [Range(0f, 0.5f)]
        public float blipPitchVariation = 0.1f;

        [Tooltip("Speaking speed multiplier")]
        [Range(0.5f, 2f)]
        public float speakingSpeed = 1f;

        [Tooltip("Voice line pitch")]
        [Range(0.5f, 2f)]
        public float voicePitch = 1f;

        [Tooltip("Character-specific voice lines")]
        public List<VoiceLineEntry> characterVoiceLines = new List<VoiceLineEntry>();
    }
}
