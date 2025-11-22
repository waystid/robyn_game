# 🦁 Example Riddle Data Configurations
## Complete Setup for 5 Sphinx Riddles

> **How to use:** Follow each riddle's configuration in Unity to create the ScriptableObjects.

---

## 🌓 Riddle 1: Day and Night (Easy)

### Basic Info
```
File Name: Riddle_DayAndNight.asset
Location: Assets/Game/Data/Riddles/

Question: I fall but never break, I break but never fall. What am I?

Difficulty: Easy
```

### Answers
```
Answers: (Add 4 answers)
  [0] Day and Night  ← CORRECT
  [1] Water
  [2] Glass
  [3] Shadow

Correct Answer Index: 0
```

### Feedback
```
Correct Feedback: Excellent! Day falls (nightfall) and night breaks (daybreak).
                  You understand the riddle well.

Wrong Feedback: Not quite. Think about cycles that happen every day...

Hint Text: Consider what happens every 24 hours. One 'falls' in the evening,
           the other 'breaks' in the morning.
```

### Rewards
```
Rewards: (Add 1 reward)
  [0] Item Name: fairy_dust
      Quantity: 3
      Description: Magical dust for your cleverness
```

### Cooldown
```
Cooldown Minutes: 60 (1 hour)
Is Repeatable: ✅ YES
```

### 📝 Design Notes:
- **Difficulty:** Easy (good starter riddle)
- **Logic:** Wordplay on "fall" and "break"
- **Teaching:** Introduces riddle system
- **Reward:** Small but motivating

---

## 👣 Riddle 2: Footsteps (Easy)

### Basic Info
```
File Name: Riddle_Footsteps.asset
Location: Assets/Game/Data/Riddles/

Question: The more you take, the more you leave behind. What am I?

Difficulty: Easy
```

### Answers
```
Answers: (Add 4 answers)
  [0] Footsteps  ← CORRECT
  [1] Time
  [2] Memories
  [3] Money

Correct Answer Index: 0
```

### Feedback
```
Correct Feedback: Precisely! The more steps you take, the more footprints
                  you leave behind. Simple yet clever!

Wrong Feedback: Hmm, think more literally. What physical thing do you
                leave behind when you move?

Hint Text: Think about walking. What trail do you leave as you go?
```

### Rewards
```
Rewards: (Add 2 rewards)
  [0] Item Name: glowberry
      Quantity: 5
      Description: Fresh berries

  [1] Item Name: coin
      Quantity: 10
      Description: A small reward
```

### Cooldown
```
Cooldown Minutes: 60
Is Repeatable: ✅ YES
```

### 📝 Design Notes:
- **Difficulty:** Easy
- **Logic:** Literal interpretation riddle
- **Common:** Classic riddle, accessible
- **Reward:** Practical (Glowberries useful for quests)

---

## 🕳️ Riddle 3: A Hole (Medium)

### Basic Info
```
File Name: Riddle_Hole.asset
Location: Assets/Game/Data/Riddles/

Question: I have holes in my top and bottom, left and right,
          and in my middle. Yet I still hold water. What am I?

Difficulty: Medium
```

### Answers
```
Answers: (Add 4 answers)
  [0] A Sponge  ← CORRECT
  [1] A Bucket
  [2] A Net
  [3] A Cloud

Correct Answer Index: 0
```

### Feedback
```
Correct Feedback: Brilliant! A sponge has countless holes yet absorbs
                  and holds water. Your mind is sharp!

Wrong Feedback: Think about something that absorbs water despite being
                full of holes.

Hint Text: It's something you might use for cleaning. It soaks up water
           even though it's riddled with holes.
```

### Rewards
```
Rewards: (Add 2 rewards)
  [0] Item Name: crystal
      Quantity: 2
      Description: Crystals for your wisdom

  [1] Item Name: moonflower_petal
      Quantity: 1
      Description: A rare petal
```

### Cooldown
```
Cooldown Minutes: 120 (2 hours)
Is Repeatable: ✅ YES
```

### 📝 Design Notes:
- **Difficulty:** Medium (requires more thought)
- **Logic:** Visual/physical property riddle
- **Reward:** Better rewards (includes rare Moonflower)
- **Cooldown:** Longer (2 hours) due to better rewards

---

## 🌊 Riddle 4: The River (Medium)

### Basic Info
```
File Name: Riddle_River.asset
Location: Assets/Game/Data/Riddles/

Question: I run but never walk, have a mouth but never talk,
          have a bed but never sleep. What am I?

Difficulty: Medium
```

### Answers
```
Answers: (Add 4 answers)
  [0] A River  ← CORRECT
  [1] A Road
  [2] Time
  [3] A Watch

Correct Answer Index: 0
```

### Feedback
```
Correct Feedback: Exactly right! Rivers run (flow), have mouths (where they
                  meet the sea), and beds (the ground beneath). You see
                  through nature's poetry!

Wrong Feedback: Think about something in nature. Each clue relates to
                different parts of the same thing.

Hint Text: It flows constantly. Think of the parts: where it ends (mouth),
           where water flows (bed), and how it moves (runs).
```

### Rewards
```
Rewards: (Add 2 rewards)
  [0] Item Name: crystal_shard
      Quantity: 1
      Description: A valuable crystal shard!

  [1] Item Name: fairy_dust
      Quantity: 10
      Description: Generous helping of fairy dust
```

### Cooldown
```
Cooldown Minutes: 120 (2 hours)
Is Repeatable: ✅ YES
```

### 📝 Design Notes:
- **Difficulty:** Medium
- **Logic:** Multiple wordplay elements
- **Classic:** Well-known riddle, but timeless
- **Reward:** Crystal Shard (valuable for crafting)

---

## ⏰ Riddle 5: Time (Hard)

### Basic Info
```
File Name: Riddle_Time.asset
Location: Assets/Game/Data/Riddles/

Question: I cannot be seen, cannot be felt, cannot be heard,
          cannot be smelt. I lie behind stars and under hills,
          and empty holes I fill. I come first and follow after,
          end life, kill laughter. What am I?

Difficulty: Hard
```

### Answers
```
Answers: (Add 4 answers)
  [0] Darkness  ← CORRECT (if using classic riddle interpretation)
  [1] Time
  [2] Death
  [3] Nothing

Correct Answer Index: 0

Alternative interpretation:
  [0] Time  ← CORRECT (if emphasizing "come first and follow after")
  [1] Darkness
  [2] Air
  [3] Silence

Correct Answer Index: 0
```

### Feedback
```
Correct Feedback: Incredible! This is one of the ancient riddles, and you
                  solved it! Darkness fills spaces unseen, exists beyond
                  stars and beneath mountains. Your wisdom matches the
                  greatest scholars!

Wrong Feedback: This is a difficult one. Think about what exists everywhere
                but cannot be perceived by the senses. What fills the voids
                between all things?

Hint Text: It's the absence of something. It exists in caves, in space,
           and when the sun sets. It comes before dawn and after dusk.
```

### Rewards
```
Rewards: (Add 3 rewards - epic!)
  [0] Item Name: sphinx_blessing
      Quantity: 1
      Description: A rare blessing from the Sphinx herself!

  [1] Item Name: crystal_shard
      Quantity: 3
      Description: Three precious crystal shards

  [2] Item Name: coin
      Quantity: 100
      Description: A fortune for your brilliance!
```

### Cooldown
```
Cooldown Minutes: 240 (4 hours)
Is Repeatable: ✅ YES
```

### 📝 Design Notes:
- **Difficulty:** Hard (philosophical riddle)
- **Logic:** Abstract, requires deep thought
- **Source:** Tolkien-inspired classic
- **Reward:** Epic (Sphinx Blessing is unique item)
- **Cooldown:** Longest (4 hours) for balance

---

## 📊 Riddle Difficulty Progression

| Riddle | Difficulty | Answer | Best Reward | Cooldown |
|--------|-----------|--------|-------------|----------|
| **Day and Night** | Easy | Day and Night | Fairy Dust (3) | 1h |
| **Footsteps** | Easy | Footsteps | Glowberries (5) | 1h |
| **A Hole** | Medium | Sponge | Moonflower Petal | 2h |
| **The River** | Medium | River | Crystal Shard | 2h |
| **Time/Darkness** | Hard | Darkness | Sphinx Blessing | 4h |

---

## 🎮 Riddle System Integration

### Player Experience Flow:

```
First Visit to Sphinx:
  ↓
Sphinx offers easy riddle (Day and Night or Footsteps)
  ↓
Player solves → Gets small reward + confidence
  ↓
1 hour later: Can answer another riddle
  ↓
Gradually progresses to medium riddles
  ↓
Eventually challenges hard riddle
  ↓
Earns Sphinx's respect and best rewards
```

### Riddle Pool Strategy:
```
5 easy riddles: Always available for new players
5 medium riddles: Available after solving 3 easy
3 hard riddles: Available after solving 5 medium
1 legendary riddle: Available after solving all others

Total: 14 riddles for full content
```

---

## 💡 Additional Riddle Ideas

### Easy Riddles:

**"The Echo"**
```
Question: I speak without a mouth and hear without ears.
          I have no body, but come alive with wind. What am I?
Answer: An Echo
```

**"A Candle"**
```
Question: The more of me you use, the shorter I become.
          What am I?
Answer: A Candle
```

### Medium Riddles:

**"The Mirror"**
```
Question: I show you the world, yet I have no eyes.
          I speak your words, yet I have no voice. What am I?
Answer: A Mirror
```

**"Your Name"**
```
Question: I belong to you, but others use me more than you do.
          What am I?
Answer: Your Name
```

### Hard Riddles:

**"Tomorrow"**
```
Question: It always comes but never arrives. What is it?
Answer: Tomorrow
```

**"The Present"**
```
Question: What is always in front of you but can't be seen?
Answer: The Future
```

---

## 🌟 Sphinx Character Integration

### Sphinx Dialogue Examples:

**First Meeting:**
```
Sphinx: "Greetings, traveler. I am the Sphinx, keeper of riddles and
         guardian of wisdom. Those who prove their cleverness shall be
         rewarded. Those who fail... well, let us not speak of failure.
         Are you ready to test your mind?"

[Yes, give me a riddle!] [Maybe later]
```

**After Correct Answer:**
```
Sphinx: "Impressive! Your mind is sharper than most. Take this reward,
         you've earned it. Return in one hour if you wish to challenge
         yourself again."

*Sphinx purrs contentedly*
```

**After Wrong Answer:**
```
Sphinx: "Hmm, not quite. But do not despair - even the wisest stumble
         on their first attempt. Here's a hint: [hint text]

         Think on it, and return when you believe you have the answer."

*Sphinx remains patient*
```

**After Many Correct Answers:**
```
Sphinx: "You have proven yourself a worthy riddle-solver, [player name].
         Few possess such wit. I name you 'Friend of the Sphinx.'
         You may call upon me for counsel anytime."

*Achievement Unlocked: Sphinx's Friend*
```

---

## 🎨 Riddle UI Design Tips

### Riddle Panel Layout:
```
┌─────────────────────────────────┐
│   🦁 THE SPHINX'S RIDDLE 🦁      │
├─────────────────────────────────┤
│                                 │
│  [Riddle question text here,    │
│   nicely formatted with good    │
│   line breaks for readability]  │
│                                 │
├─────────────────────────────────┤
│  ○ Answer Option 1              │
│  ○ Answer Option 2              │
│  ○ Answer Option 3              │
│  ○ Answer Option 4              │
├─────────────────────────────────┤
│  [Submit Answer]   [Need a Hint]│
└─────────────────────────────────┘
```

### Result Panel:
```
┌─────────────────────────────────┐
│         ✨ CORRECT! ✨           │
├─────────────────────────────────┤
│                                 │
│  [Feedback text from Sphinx]    │
│                                 │
│  Rewards Received:              │
│  • 3 Fairy Dust                 │
│  • 10 Gold                      │
│                                 │
│  Next riddle available in:      │
│  ⏰ 59:45                        │
│                                 │
│        [Continue]                │
└─────────────────────────────────┘
```

---

## 🧪 Testing Your Riddles

### Test Commands:
```csharp
// Get random riddle
RiddleData riddle = RiddleManager.Instance.GetRandomAvailableRiddle();

// Check answer
bool correct = RiddleManager.Instance.AnswerRiddle(riddle, answerIndex);

// Reset cooldowns (testing)
foreach (var r in RiddleManager.Instance.allRiddles)
{
    r.ResetRiddle();
}

// Check stats
RiddleStats stats = RiddleManager.Instance.GetStats();
Debug.Log($"Accuracy: {stats.accuracyPercentage}%");
```

### Test Checklist:
- [ ] Riddle appears in UI correctly
- [ ] All 4 answers display
- [ ] Answers are shuffled (correct answer not always [0])
- [ ] Clicking correct answer shows success
- [ ] Clicking wrong answer shows hint
- [ ] Rewards are granted on success
- [ ] Cooldown timer starts
- [ ] Can't answer same riddle during cooldown
- [ ] Different riddle available after cooldown
- [ ] Statistics track correctly

---

## ✅ Riddle Creation Checklist

For each riddle:
- [ ] Create RiddleData ScriptableObject
- [ ] Write clear, interesting question
- [ ] Add 4 plausible answers
- [ ] Mark correct answer index
- [ ] Write encouraging correct feedback
- [ ] Write helpful wrong feedback
- [ ] Write useful hint
- [ ] Add appropriate rewards
- [ ] Set difficulty level
- [ ] Set cooldown time
- [ ] Test in game
- [ ] Verify answer shuffling works
- [ ] Check that hints help without giving away

---

## 📚 Riddle Writing Tips

### Good Riddles Are:
- **Clear:** Question is understandable
- **Fair:** Answer is logically deducible
- **Creative:** Uses wordplay or lateral thinking
- **Memorable:** Sticks in player's mind

### Avoid:
- **Too obscure:** Requiring specific knowledge
- **Too easy:** Obvious at first glance (unless Easy difficulty)
- **Trick questions:** That feel unfair
- **Cultural specific:** That some players won't understand

### Testing:
- Show riddle to someone who hasn't seen it
- If they solve it in ~1 minute (Easy) or ~5 minutes (Medium) = good!
- If they need hint = that's okay, hints exist for a reason
- If they can't solve even with hint = too hard, revise

---

**All 5 Sphinx riddles ready to create!** 🦁

Next: Master creation guide! →
