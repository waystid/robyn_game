# 💻 Beginner's Code Implementation Guide
## Complete Scripts for Your Cozy Magical Game

> **How to use this guide:** Copy the scripts exactly as shown. I'll explain what each part does.

---

## 📋 Table of Contents

1. [Quest System Scripts](#quest-system-scripts)
2. [Riddle System Scripts](#riddle-system-scripts)
3. [Creature AI Scripts](#creature-ai-scripts)
4. [UI Helper Scripts](#ui-helper-scripts)
5. [Utility Scripts](#utility-scripts)

---

## 🎯 Quest System Scripts

### 1. QuestData.cs (ScriptableObject)

**Where:** `Assets/Game/Scripts/NPCs/QuestData.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace CozyGame
{
    /// <summary>
    /// Defines a quest with requirements and rewards
    /// </summary>
    [CreateAssetMenu(fileName = "New Quest", menuName = "Cozy Game/Quest Data")]
    public class QuestData : ScriptableObject
    {
        [Header("Quest Info")]
        public string questName = "My Quest";
        [TextArea(3, 6)]
        public string questDescription = "Quest description here";
        public string questGiver = "Dragon"; // NPC who gives this quest

        [Header("Requirements")]
        public List<QuestRequirement> requirements = new List<QuestRequirement>();

        [Header("Rewards")]
        public List<QuestReward> rewards = new List<QuestReward>();

        [Header("State")]
        public QuestState currentState = QuestState.NotStarted;

        /// <summary>
        /// Check if quest requirements are met
        /// </summary>
        public bool AreRequirementsMet()
        {
            // This will be implemented with Survival Engine integration
            // For now, returns false
            return false;
        }
    }

    [System.Serializable]
    public class QuestRequirement
    {
        public string itemID;        // Reference to ItemData
        public int requiredQuantity;
    }

    [System.Serializable]
    public class QuestReward
    {
        public string itemID;        // Reference to ItemData
        public int quantity;
    }

    public enum QuestState
    {
        NotStarted,
        Active,
        Completed,
        Failed
    }
}
```

**What it does:**
- Creates a blueprint for quests
- Stores what items are needed
- Stores what rewards you get
- Tracks if quest is active/completed

---

### 2. QuestManager.cs (Singleton)

**Where:** `Assets/Game/Scripts/NPCs/QuestManager.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CozyGame
{
    /// <summary>
    /// Manages all active quests in the game
    /// Singleton pattern - only one instance exists
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        [Header("Active Quests")]
        public List<QuestData> activeQuests = new List<QuestData>();

        [Header("Completed Quests")]
        public List<QuestData> completedQuests = new List<QuestData>();

        [Header("Events")]
        public delegate void QuestEvent(QuestData quest);
        public event QuestEvent OnQuestStarted;
        public event QuestEvent OnQuestCompleted;

        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Start a new quest
        /// </summary>
        public void StartQuest(QuestData quest)
        {
            if (quest == null)
            {
                Debug.LogError("Cannot start null quest!");
                return;
            }

            if (activeQuests.Contains(quest))
            {
                Debug.LogWarning($"Quest {quest.questName} is already active!");
                return;
            }

            if (completedQuests.Contains(quest))
            {
                Debug.LogWarning($"Quest {quest.questName} is already completed!");
                return;
            }

            // Add to active quests
            activeQuests.Add(quest);
            quest.currentState = QuestState.Active;

            // Trigger event
            OnQuestStarted?.Invoke(quest);

            Debug.Log($"Quest started: {quest.questName}");
        }

        /// <summary>
        /// Complete a quest and grant rewards
        /// </summary>
        public void CompleteQuest(QuestData quest)
        {
            if (!activeQuests.Contains(quest))
            {
                Debug.LogWarning($"Quest {quest.questName} is not active!");
                return;
            }

            // Move to completed
            activeQuests.Remove(quest);
            completedQuests.Add(quest);
            quest.currentState = QuestState.Completed;

            // Grant rewards (will integrate with Survival Engine)
            GrantRewards(quest);

            // Trigger event
            OnQuestCompleted?.Invoke(quest);

            Debug.Log($"Quest completed: {quest.questName}");
        }

        /// <summary>
        /// Grant quest rewards to player
        /// </summary>
        private void GrantRewards(QuestData quest)
        {
            // TODO: Integrate with Survival Engine inventory
            foreach (var reward in quest.rewards)
            {
                Debug.Log($"Granted reward: {reward.quantity}x {reward.itemID}");
                // PlayerData.Get().inventory.AddItem(reward.itemID, reward.quantity);
            }
        }

        /// <summary>
        /// Check if a quest is active
        /// </summary>
        public bool IsQuestActive(QuestData quest)
        {
            return activeQuests.Contains(quest);
        }

        /// <summary>
        /// Check if a quest is completed
        /// </summary>
        public bool IsQuestCompleted(QuestData quest)
        {
            return completedQuests.Contains(quest);
        }

        /// <summary>
        /// Get all active quests
        /// </summary>
        public List<QuestData> GetActiveQuests()
        {
            return activeQuests;
        }
    }
}
```

**What it does:**
- Keeps track of all quests
- Starts quests when player accepts
- Completes quests and gives rewards
- Only one QuestManager exists (singleton)

---

## 🧩 Riddle System Scripts

### 1. RiddleData.cs (ScriptableObject)

**Where:** `Assets/Game/Scripts/NPCs/RiddleData.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace CozyGame
{
    /// <summary>
    /// Defines a riddle with question, answers, and rewards
    /// </summary>
    [CreateAssetMenu(fileName = "New Riddle", menuName = "Cozy Game/Riddle Data")]
    public class RiddleData : ScriptableObject
    {
        [Header("Riddle")]
        [TextArea(2, 4)]
        public string question = "What is the answer to everything?";

        [Header("Answers")]
        public List<string> answers = new List<string> { "42", "Life", "Love", "Time" };
        public int correctAnswerIndex = 0; // Which answer is correct (0-3)

        [Header("Feedback")]
        public string correctFeedback = "Correct! Well done!";
        public string wrongFeedback = "Not quite. Think about it...";
        public string hintText = "It's related to a famous book.";

        [Header("Rewards")]
        public List<RiddleReward> rewards = new List<RiddleReward>();

        [Header("Cooldown")]
        public float cooldownMinutes = 60f; // 1 hour default

        [HideInInspector]
        public bool hasBeenAnswered = false;
        [HideInInspector]
        public float lastAnsweredTime = 0f;

        /// <summary>
        /// Check if this riddle is available (cooldown passed)
        /// </summary>
        public bool IsAvailable()
        {
            if (!hasBeenAnswered) return true;

            float timeSinceAnswered = Time.time - lastAnsweredTime;
            float cooldownSeconds = cooldownMinutes * 60f;
            return timeSinceAnswered >= cooldownSeconds;
        }

        /// <summary>
        /// Get remaining cooldown time in minutes
        /// </summary>
        public float GetRemainingCooldown()
        {
            if (!hasBeenAnswered) return 0f;

            float timeSinceAnswered = Time.time - lastAnsweredTime;
            float cooldownSeconds = cooldownMinutes * 60f;
            float remaining = cooldownSeconds - timeSinceAnswered;
            return Mathf.Max(0f, remaining / 60f); // Return in minutes
        }

        /// <summary>
        /// Mark riddle as answered
        /// </summary>
        public void MarkAsAnswered()
        {
            hasBeenAnswered = true;
            lastAnsweredTime = Time.time;
        }
    }

    [System.Serializable]
    public class RiddleReward
    {
        public string itemID;
        public int quantity;
    }
}
```

**What it does:**
- Stores riddle questions and answers
- Tracks cooldown timers
- Stores rewards for correct answers

---

### 2. RiddleManager.cs (Singleton)

**Where:** `Assets/Game/Scripts/NPCs/RiddleManager.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CozyGame
{
    /// <summary>
    /// Manages riddle system and tracks answered riddles
    /// </summary>
    public class RiddleManager : MonoBehaviour
    {
        public static RiddleManager Instance { get; private set; }

        [Header("All Riddles")]
        public List<RiddleData> allRiddles = new List<RiddleData>();

        [Header("Events")]
        public delegate void RiddleEvent(RiddleData riddle, bool correct);
        public event RiddleEvent OnRiddleAnswered;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Get a random available riddle
        /// </summary>
        public RiddleData GetRandomAvailableRiddle()
        {
            var available = allRiddles.Where(r => r.IsAvailable()).ToList();

            if (available.Count == 0)
            {
                Debug.LogWarning("No riddles available (all on cooldown)");
                return null;
            }

            int randomIndex = Random.Range(0, available.Count);
            return available[randomIndex];
        }

        /// <summary>
        /// Answer a riddle
        /// </summary>
        public bool AnswerRiddle(RiddleData riddle, int answerIndex)
        {
            if (riddle == null)
            {
                Debug.LogError("Cannot answer null riddle!");
                return false;
            }

            bool correct = (answerIndex == riddle.correctAnswerIndex);

            if (correct)
            {
                // Mark as answered
                riddle.MarkAsAnswered();

                // Grant rewards
                GrantRewards(riddle);

                Debug.Log($"Riddle answered correctly!");
            }
            else
            {
                Debug.Log($"Riddle answered incorrectly.");
            }

            // Trigger event
            OnRiddleAnswered?.Invoke(riddle, correct);

            return correct;
        }

        /// <summary>
        /// Grant riddle rewards
        /// </summary>
        private void GrantRewards(RiddleData riddle)
        {
            // TODO: Integrate with Survival Engine
            foreach (var reward in riddle.rewards)
            {
                Debug.Log($"Granted reward: {reward.quantity}x {reward.itemID}");
                // PlayerData.Get().inventory.AddItem(reward.itemID, reward.quantity);
            }
        }

        /// <summary>
        /// Get count of available riddles
        /// </summary>
        public int GetAvailableRiddleCount()
        {
            return allRiddles.Count(r => r.IsAvailable());
        }
    }
}
```

**What it does:**
- Manages all riddles in the game
- Picks random riddles
- Checks answers
- Gives rewards for correct answers

---

## 🦋 Creature AI Scripts

### 1. SimpleWanderAI.cs

**Where:** `Assets/Game/Scripts/Creatures/SimpleWanderAI.cs`

```csharp
using UnityEngine;

namespace CozyGame
{
    /// <summary>
    /// Simple AI that makes creatures wander around randomly
    /// Perfect for fireflies, slimes, and ambient creatures
    /// </summary>
    public class SimpleWanderAI : MonoBehaviour
    {
        [Header("Wander Settings")]
        public float wanderRadius = 10f;        // How far can it wander?
        public float moveSpeed = 1f;            // How fast does it move?
        public float waitTimeMin = 2f;          // Min wait between moves
        public float waitTimeMax = 5f;          // Max wait between moves

        [Header("Rotation")]
        public bool rotateTowardsTarget = true;
        public float rotationSpeed = 5f;

        private Vector3 startPosition;
        private Vector3 targetPosition;
        private float waitTimer;
        private bool isWaiting;

        private void Start()
        {
            startPosition = transform.position;
            PickNewTarget();
        }

        private void Update()
        {
            if (isWaiting)
            {
                // Wait before moving again
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    isWaiting = false;
                    PickNewTarget();
                }
            }
            else
            {
                // Move toward target
                MoveTowardsTarget();

                // Check if reached target
                if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
                {
                    // Reached target, wait before picking new one
                    isWaiting = true;
                    waitTimer = Random.Range(waitTimeMin, waitTimeMax);
                }
            }
        }

        private void MoveTowardsTarget()
        {
            // Move
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            // Rotate to face direction
            if (rotateTowardsTarget && direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        private void PickNewTarget()
        {
            // Pick random position within radius of start position
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            targetPosition = startPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

            // Make sure target is on the ground (raycast down)
            if (Physics.Raycast(targetPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
            {
                targetPosition = hit.point;
            }
        }

        // Visualize wander radius in editor
        private void OnDrawGizmosSelected()
        {
            Vector3 center = Application.isPlaying ? startPosition : transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, wanderRadius);

            if (Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(targetPosition, 0.3f);
            }
        }
    }
}
```

**What it does:**
- Makes creatures walk around randomly
- They walk, stop, wait, then pick new spot
- Perfect for fireflies and slimes

---

### 2. FloatingMotion.cs (For Fireflies)

**Where:** `Assets/Game/Scripts/Creatures/FloatingMotion.cs`

```csharp
using UnityEngine;

namespace CozyGame
{
    /// <summary>
    /// Adds gentle floating/bobbing motion
    /// Perfect for fireflies, fairies, magical orbs
    /// </summary>
    public class FloatingMotion : MonoBehaviour
    {
        [Header("Float Settings")]
        public float floatSpeed = 1f;
        public float floatHeight = 0.5f;
        public bool randomOffset = true;

        [Header("Sway Settings")]
        public bool enableSway = true;
        public float swaySpeed = 0.5f;
        public float swayAmount = 0.3f;

        private Vector3 startPosition;
        private float timeOffset;

        private void Start()
        {
            startPosition = transform.position;

            if (randomOffset)
            {
                // Each creature starts at different point in animation
                timeOffset = Random.Range(0f, 100f);
            }
        }

        private void Update()
        {
            float time = Time.time + timeOffset;

            // Up and down floating
            float yOffset = Mathf.Sin(time * floatSpeed) * floatHeight;

            // Side to side swaying
            float xOffset = 0f;
            float zOffset = 0f;
            if (enableSway)
            {
                xOffset = Mathf.Sin(time * swaySpeed) * swayAmount;
                zOffset = Mathf.Cos(time * swaySpeed * 0.7f) * swayAmount;
            }

            // Apply movement
            transform.position = startPosition + new Vector3(xOffset, yOffset, zOffset);

            // Update start position if creature is moving (wander AI)
            // This prevents conflicts with wander movement
            if (GetComponent<SimpleWanderAI>() != null)
            {
                startPosition = new Vector3(
                    transform.position.x - xOffset,
                    transform.position.y - yOffset,
                    transform.position.z - zOffset
                );
            }
        }
    }
}
```

**What it does:**
- Makes objects float up and down gently
- Adds slight side-to-side sway
- Perfect for fireflies and magical effects

---

## 🎨 UI Helper Scripts

### 1. SafeAreaHandler.cs (Already in PLATFORM_GUIDE.md)

### 2. FloatingTextManager.cs

**Where:** `Assets/Game/Scripts/UI/FloatingTextManager.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CozyGame.UI
{
    /// <summary>
    /// Shows floating text when picking up items or gaining rewards
    /// Example: "+5 Wood" floats up and fades
    /// </summary>
    public class FloatingTextManager : MonoBehaviour
    {
        public static FloatingTextManager Instance { get; private set; }

        [Header("Prefab")]
        public GameObject floatingTextPrefab; // Assign in inspector

        [Header("Settings")]
        public float floatSpeed = 1f;
        public float fadeSpeed = 1f;
        public float lifetime = 2f;

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

        /// <summary>
        /// Show floating text at world position
        /// </summary>
        public void Show(string text, Vector3 worldPosition, Color? color = null)
        {
            if (floatingTextPrefab == null)
            {
                Debug.LogWarning("Floating text prefab not assigned!");
                return;
            }

            // Create floating text
            GameObject textObj = Instantiate(floatingTextPrefab, transform);
            textObj.transform.position = worldPosition;

            // Set text
            TextMeshProUGUI textComponent = textObj.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = text;
                if (color.HasValue)
                {
                    textComponent.color = color.Value;
                }
            }

            // Animate
            StartCoroutine(AnimateFloatingText(textObj, textComponent));
        }

        private System.Collections.IEnumerator AnimateFloatingText(GameObject textObj, TextMeshProUGUI textComponent)
        {
            float elapsed = 0f;
            Vector3 startPos = textObj.transform.position;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;

                // Float up
                textObj.transform.position = startPos + Vector3.up * (floatSpeed * elapsed);

                // Fade out
                if (textComponent != null)
                {
                    Color color = textComponent.color;
                    color.a = 1f - (elapsed / lifetime);
                    textComponent.color = color;
                }

                yield return null;
            }

            Destroy(textObj);
        }
    }
}
```

**What it does:**
- Shows "+5 Wood" text when you pick up items
- Text floats up and fades away
- Great for feedback!

---

## 🛠️ Utility Scripts

### 1. AudioManager.cs (Simple Version)

**Where:** `Assets/Game/Scripts/Utilities/AudioManager.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace CozyGame
{
    /// <summary>
    /// Simple audio manager for playing sounds and music
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;

        [Header("Sound Library")]
        public List<NamedAudioClip> soundEffects = new List<NamedAudioClip>();

        private Dictionary<string, AudioClip> soundLibrary = new Dictionary<string, AudioClip>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSoundLibrary();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeSoundLibrary()
        {
            foreach (var sound in soundEffects)
            {
                if (!soundLibrary.ContainsKey(sound.name))
                {
                    soundLibrary.Add(sound.name, sound.clip);
                }
            }
        }

        /// <summary>
        /// Play a sound effect by name
        /// </summary>
        public void PlaySound(string soundName)
        {
            if (soundLibrary.TryGetValue(soundName, out AudioClip clip))
            {
                sfxSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning($"Sound '{soundName}' not found!");
            }
        }

        /// <summary>
        /// Play music (loops)
        /// </summary>
        public void PlayMusic(AudioClip musicClip)
        {
            if (musicSource.clip == musicClip && musicSource.isPlaying)
                return; // Already playing

            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.Play();
        }

        /// <summary>
        /// Stop music
        /// </summary>
        public void StopMusic()
        {
            musicSource.Stop();
        }
    }

    [System.Serializable]
    public class NamedAudioClip
    {
        public string name;
        public AudioClip clip;
    }
}
```

**What it does:**
- Plays sound effects
- Plays background music
- Easy to use: `AudioManager.PlaySound("pickup");`

---

## 📝 How to Use These Scripts

### Step 1: Copy Scripts
1. Create the folder path listed in "Where:" above
2. Right-click in Unity → Create → C# Script
3. Name it exactly as shown (including .cs)
4. Double-click to open in Visual Studio
5. Delete everything
6. Copy and paste the script from this guide
7. Save (Ctrl+S)

### Step 2: Create Manager GameObjects
In your main scene:
1. Create empty GameObject: "Managers"
2. Add child objects:
   - QuestManager (add QuestManager.cs)
   - RiddleManager (add RiddleManager.cs)
   - AudioManager (add AudioManager.cs)
   - FloatingTextManager (add FloatingTextManager.cs)

### Step 3: Create Data Assets
1. Right-click in Assets/Game/Data/
2. Create → Cozy Game → Quest Data (or Riddle Data)
3. Fill in the details in Inspector
4. Assign to managers

### Step 4: Add to Creatures
For fireflies and slimes:
1. Add SimpleWanderAI component
2. For fireflies: Also add FloatingMotion component
3. Adjust settings in Inspector

---

## ✅ Testing Checklist

- [ ] QuestManager exists in scene
- [ ] RiddleManager exists in scene
- [ ] Created at least 1 QuestData asset
- [ ] Created at least 1 RiddleData asset
- [ ] Creature has SimpleWanderAI and moves around
- [ ] No console errors

---

## 💡 Beginner Tips

### Understanding Scripts
- **`public`** = You can see/change in Inspector
- **`private`** = Hidden, script uses internally
- **`void Start()`** = Runs once when game starts
- **`void Update()`** = Runs every frame

### Common Errors
**"Type or namespace not found"**
- Make sure `using` statements at top are correct
- Check that namespace matches (CozyGame)

**"Missing component reference"**
- Assign references in Inspector
- Check "Prefab" or "Manager" fields

**"NullReferenceException"**
- Something is null (not assigned)
- Check Inspector for empty fields

### Getting Help
When asking me for help, include:
- Which script has the problem
- Error message from Console (copy/paste)
- What you were trying to do

---

## 🎉 You Have All The Code!

These scripts cover:
- ✅ Quest system (Dragon)
- ✅ Riddle system (Sphinx)
- ✅ Creature AI (Fireflies, Slimes)
- ✅ Audio management
- ✅ UI feedback

**Remaining code (Survival Engine integration) will be done during implementation with specific guidance!**

**Next: Start Week 1 of development schedule!**
