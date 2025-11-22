# 🐉 Example Quest Data Configurations
## Complete Setup for 3 Dragon Quests

> **How to use:** Follow each quest's configuration in Unity to create the ScriptableObjects.

---

## 💎 Quest 1: The Shiny Things (Starter Quest)

### Basic Info
```
File Name: Quest_ShinyThings.asset
Location: Assets/Game/Data/Quests/

Quest Name: The Shiny Things
Quest ID: quest_shiny_things (auto-generated)
Quest Giver: Dragon
Description: The Dragon has a fondness for shiny objects. She's asked you
             to gather some crystals from around the village. Simple enough!
```

### Requirements
```
Requirements: (Add 1 requirement)
  [0] Item Name: crystal
      Required Quantity: 3
```

### Rewards
```
Rewards: (Add 2 rewards)
  [0] Item Name: fairy_dust
      Quantity: 5
      Description: Magical dust that sparkles in moonlight

  [1] Item Name: coin
      Quantity: 10
      Description: Gold coins for your trouble
```

### Quest State
```
Current State: NotStarted
Is Repeatable: ✅ YES
Cooldown Minutes: 60 (can repeat once per hour)
```

### 📝 Design Notes:
- **Difficulty:** Easy (tutorial quest)
- **Purpose:** Teaches quest system basics
- **Time to Complete:** 5-10 minutes
- **Rewards:** Generous to encourage engagement

### 💬 Suggested Dialogue:

**On Quest Start:**
```
Dragon: "Hello, little one! I do love shiny things. Would you be a dear
         and gather 3 crystals for me? I'll make it worth your while!"

[Accept Quest] [Maybe Later]
```

**Quest In Progress:**
```
Dragon: "Have you found those crystals yet? They should be scattered around
         the village. Look near rocks and trees!"

Crystals: 1/3 ⭘⭘
```

**Quest Complete:**
```
Dragon: "Wonderful! These will look perfect in my hoard. Here's a little
         something for your trouble. Come back anytime!"

[Complete Quest]

*You received 5 Fairy Dust and 10 Gold!*
```

---

## 🌙 Quest 2: Moonlit Medicine (Mid-Game Quest)

### Basic Info
```
File Name: Quest_MoonlitMedicine.asset
Location: Assets/Game/Data/Quests/

Quest Name: Moonlit Medicine
Quest ID: quest_moonlit_medicine
Quest Giver: Dragon
Description: The Dragon's friend, the Sphinx, has fallen ill. She needs
             rare Moonflower Petals to brew a healing potion. These flowers
             only bloom under moonlight and take time to grow.
```

### Requirements
```
Requirements: (Add 2 requirements)
  [0] Item Name: moonflower_petal
      Required Quantity: 4

  [1] Item Name: glowberry
      Required Quantity: 6
```

### Rewards
```
Rewards: (Add 3 rewards)
  [0] Item Name: dragon_scale
      Quantity: 1
      Description: A precious scale from the Dragon herself!

  [1] Item Name: potion_healing
      Quantity: 2
      Description: Magical healing potions

  [2] Item Name: coin
      Quantity: 50
      Description: A generous payment
```

### Quest State
```
Current State: NotStarted
Is Repeatable: ❌ NO (one-time story quest)
Cooldown Minutes: 0
```

### 📝 Design Notes:
- **Difficulty:** Medium
- **Purpose:** Teaches plant cultivation and patience
- **Time to Complete:** 20-30 minutes (includes growing Moonflowers)
- **Rewards:** Epic (Dragon Scale unlocks advanced crafting)
- **Story:** Builds relationship between player, Dragon, and Sphinx

### 💬 Suggested Dialogue:

**On Quest Start:**
```
Dragon: "I need your help with something urgent. My friend, the Sphinx,
         has taken ill. I need Moonflower Petals and Glowberries to brew
         a healing potion. Will you help me gather them?"

[Of course!] [What's in it for me?]

Dragon: "You're very kind. Moonflowers take time to grow, but you're
         a clever one. I'm sure you'll figure it out. I'll reward you
         handsomely - perhaps even one of my scales..."
```

**Quest In Progress:**
```
Dragon: "How goes the gathering? Remember, Moonflowers need care and
         patience. Water them regularly!"

Progress:
  Moonflower Petals: 2/4 ⭘⭘⭘⭘
  Glowberries: 6/6 ✓✓✓✓✓✓
```

**Quest Complete:**
```
Dragon: "You did it! Thank you, dear friend. With these, I can brew the
         medicine for Sphinx. Please, take this scale - it's very precious
         to me, but you've earned it. Use it wisely in your crafting!"

[Complete Quest]

*You received Dragon Scale, 2 Healing Potions, and 50 Gold!*
*New crafting recipes unlocked!*
```

---

## 💎 Quest 3: The Crystal Garden (End-Game Quest)

### Basic Info
```
File Name: Quest_CrystalGarden.asset
Location: Assets/Game/Data/Quests/

Quest Name: The Crystal Garden
Quest ID: quest_crystal_garden
Quest Giver: Dragon
Description: The Dragon dreams of having her own Crystal Garden - a place
             where Crystal Sprouts grow eternally. She needs your help to
             establish it. This will take time, dedication, and powerful magic.
```

### Requirements
```
Requirements: (Add 4 requirements)
  [0] Item Name: crystal_shard
      Required Quantity: 5

  [1] Item Name: moonflower_petal
      Required Quantity: 3

  [2] Item Name: fairy_dust
      Required Quantity: 20

  [3] Item Name: enchanted_wood
      Required Quantity: 10
```

### Rewards
```
Rewards: (Add 4 rewards - epic!)
  [0] Item Name: dragon_heart_crystal
      Quantity: 1
      Description: The Dragon's most precious treasure!

  [1] Item Name: spell_tome_advanced
      Quantity: 1
      Description: Unlocks advanced spells

  [2] Item Name: crystal_sprout_seed
      Quantity: 3
      Description: Renewable Crystal Sprout seeds!

  [3] Item Name: coin
      Quantity: 200
      Description: A fortune in gold!
```

### Quest State
```
Current State: NotStarted
Is Repeatable: ❌ NO (ultimate quest)
Cooldown Minutes: 0
```

### 📝 Design Notes:
- **Difficulty:** Hard (end-game)
- **Purpose:** Final Dragon questline, epic conclusion
- **Time to Complete:** 1-2 hours (gathering all materials)
- **Rewards:** Legendary (Dragon Heart Crystal is ultimate item)
- **Unlocks:** Advanced spells, renewable Crystal Sprouts, end-game content
- **Requirement:** Must complete previous Dragon quests first

### 💬 Suggested Dialogue:

**Prerequisites Check:**
```
Dragon: "Hmm, you're not quite ready for this yet, young one. Come back
         when you've proven yourself worthy."

[Requires: Quest "Moonlit Medicine" completed]
```

**On Quest Start:**
```
Dragon: "You've become a true friend, and I trust you with my greatest
         dream. I wish to create a Crystal Garden - a eternal place of
         beauty and magic. But the materials are rare and precious.

         I need:
         • 5 Crystal Shards (the rarest gems)
         • 3 Moonflower Petals (for magical enhancement)
         • 20 Fairy Dust (to maintain the enchantment)
         • 10 Enchanted Wood (for the garden structure)

         This is no small task. Take your time. When you're ready,
         return to me, and together we'll create something magnificent."

[I accept this quest] [I need more time]
```

**Quest In Progress:**
```
Dragon: "The Crystal Garden will be a place of wonder. I can already
         envision it... Take your time gathering the materials. Quality
         over speed, dear friend."

Progress:
  Crystal Shards: 3/5 ⭘⭘⭘⭘⭘
  Moonflower Petals: 3/3 ✓✓✓
  Fairy Dust: 15/20 ⭘⭘⭘⭘
  Enchanted Wood: 7/10 ⭘⭘⭘

(Hint: Use Growth Spells to speed up Crystal Sprout farming!)
```

**Quest Complete:**
```
Dragon: "You've done it! You've gathered everything we need. Watch now,
         as we create something truly magical..."

*Magical cinematic: Dragon and player create the Crystal Garden*
*Crystals grow, sparkle, and form a beautiful garden*

Dragon: "It's perfect. Thank you, my dearest friend. You've given me
         something I've dreamed of for centuries. Please, accept this -
         my Heart Crystal. It contains a piece of my magic. You've
         earned the title of Dragon Friend.

         The garden will provide Crystal Sprout seeds forever. Use them
         wisely. And this spell tome... I think you're ready for advanced
         magic now."

[Complete Quest]

*Epic rewards granted!*
*Achievement Unlocked: Dragon Friend*
*New area unlocked: Crystal Garden*
*New spell book added to inventory*
```

---

## 📊 Quest Progression Chart

| Quest | Difficulty | Time | Key Reward | Unlocks |
|-------|-----------|------|------------|---------|
| **Shiny Things** | Easy | 10 min | Fairy Dust | Quest system |
| **Moonlit Medicine** | Medium | 30 min | Dragon Scale | Advanced crafting |
| **Crystal Garden** | Hard | 2 hours | Dragon Heart | End-game content |

---

## 🎮 Questline Flow

### The Complete Dragon Story Arc:

```
Act 1: Introduction (Quest 1)
  ↓
Player meets Dragon
Dragon asks for simple favor (crystals)
Player learns quest system
Dragon becomes friendly

Act 2: Building Trust (Quest 2)
  ↓
Dragon reveals she has a friend (Sphinx)
Sphinx is ill - Dragon needs help
Player must grow rare plants
Shows player mastery of game systems
Dragon gives precious scale (trust deepens)

Act 3: Ultimate Bond (Quest 3)
  ↓
Dragon shares her dream (Crystal Garden)
Massive undertaking requires dedication
Player demonstrates mastery and loyalty
Dragon gives her Heart Crystal (ultimate trust)
Player becomes "Dragon Friend"
Crystal Garden provides renewable resources
```

### Quest Gating:
```
Quest 1: Always available (starter quest)
  ↓
Quest 2: Requires Quest 1 completion + Level 5
  ↓
Quest 3: Requires Quest 2 completion + Level 15
```

---

## 🌟 Integration with Other Systems

### With Plants:
- Quest 1: Encourages resource gathering
- Quest 2: Requires plant cultivation (Moonflowers)
- Quest 3: Mastery of Crystal Sprouts

### With Magic:
- Growth Spell: Speeds up Quest 2 and 3
- Harvest Spell: Efficient gathering for all quests
- Summon Wisp: Helps collect required items

### With Crafting:
- Dragon Scale (Quest 2 reward): Unlocks new recipes
- Dragon Heart Crystal (Quest 3 reward): Ultimate crafting material
- Spell Tome (Quest 3 reward): Unlocks advanced spells

---

## 💡 Additional Quest Ideas

### For Sphinx (After Dragon Quests):

**"The Riddle Master"**
- Complete 10 riddles correctly
- Reward: Sphinx's Amulet (wisdom bonus)

**"Ancient Knowledge"**
- Find 5 lost scrolls hidden in world
- Reward: New riddle types unlocked

### For Village NPCs:

**"Feed the Fireflies"**
- Place 5 Glowberries in firefly areas
- Reward: Fireflies follow you

**"Help the Gardener"**
- Plant and grow 10 different crops
- Reward: Master Gardener title, special seeds

---

## ✅ Quest Creation Checklist

For each quest:
- [ ] Create QuestData ScriptableObject
- [ ] Write compelling name and description
- [ ] Set quest giver
- [ ] Add all requirements with correct IDs
- [ ] Add all rewards with quantities
- [ ] Set repeatable/cooldown appropriately
- [ ] Write dialogue for all quest states
- [ ] Test requirements tracking
- [ ] Test reward granting
- [ ] Verify quest gates (if any)
- [ ] Play through entire quest

---

## 🧪 Testing Your Quests

### Test Commands:
```csharp
// Give yourself quest items instantly (for testing)
SurvivalEngineHelper.AddItemToInventory("crystal", 3);
SurvivalEngineHelper.AddItemToInventory("moonflower_petal", 4);

// Start quest
QuestManager.Instance.StartQuest(questData);

// Check progress
bool canComplete = QuestManager.Instance.CheckQuestCompletion(questData);

// Force complete (testing)
QuestManager.Instance.CompleteQuest(questData);
```

### Test Checklist:
- [ ] Quest appears in quest log
- [ ] Requirements track properly
- [ ] Can't complete without items
- [ ] Completing removes required items
- [ ] Rewards are granted
- [ ] Quest marked as complete
- [ ] Repeatable quests reset after cooldown
- [ ] Non-repeatable quests stay completed
- [ ] Dialogue updates based on quest state

---

**All 3 Dragon quests ready to create!** 🐉

Next: Riddle data! →
