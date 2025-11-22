# 📖 Data Creation Master Guide
## Step-by-Step: Creating All Example Game Data in Unity

> **Your Mission:** Create 3 plants, 5 spells, 3 quests, and 5 riddles in Unity.
> **Time Required:** ~2 hours total
> **Result:** Fully playable game content!

---

## 🎯 Quick Overview

**What You'll Create:**
- 🌱 3 Magical Plants (Glowberry, Moonflower, Crystal Sprout)
- ✨ 5 Magic Spells (Growth, Harvest, Light, Blink, Summon)
- 🐉 3 Dragon Quests (Shiny Things, Moonlit Medicine, Crystal Garden)
- 🦁 5 Sphinx Riddles (Day/Night, Footsteps, Hole, River, Darkness)

**Total Content:** 16 ScriptableObjects = Complete MVP content!

---

## 📋 Before You Start

### Prerequisites Checklist:
- [ ] Unity 2022.3 LTS installed and project open
- [ ] Survival Engine imported (optional, but recommended)
- [ ] All Phase 1 & 2 scripts in place (`Assets/Game/Scripts/`)
- [ ] Managers set up in scene (QuestManager, RiddleManager, MagicSystem)
- [ ] Data folders created (`Assets/Game/Data/`)

### Recommended Order:
1. **Plants first** (needed for quests)
2. **Spells second** (fun to test with plants)
3. **Quests third** (use plants as requirements)
4. **Riddles last** (independent, quick to create)

---

## 🌱 PART 1: Creating Plants (30 minutes)

### Setup (5 minutes)

1. **Create Data Folder:**
   ```
   Right-click: Assets/Game/Data/
   Create → Folder → "Plants"
   ```

2. **Optional: Create Placeholder Models**
   ```
   For each plant, create 4 stages using Unity primitives:
   - Stage 1: Small sphere (seed)
   - Stage 2: Slightly larger (sprout)
   - Stage 3: Cluster of spheres (growing)
   - Stage 4: Large cluster with emission (mature)

   Save as prefabs in: Assets/Game/Prefabs/Plants/
   ```

### Plant 1: Glowberry (10 minutes)

1. **Create ScriptableObject:**
   ```
   Right-click in Assets/Game/Data/Plants/
   Create → Cozy Game → Plants → Plant Data
   Name: "Glowberry"
   ```

2. **Basic Info:**
   ```
   Plant Name: Glowberry Bush
   Description: A magical bush that produces glowing berries.
                Perfect for crafting potions and attracting fireflies.
   ```

3. **Growth Configuration:**
   ```
   Total Stages: 4
   Stage Growth Times:
     Element 0: 60
     Element 1: 90
     Element 2: 120
     Element 3: 90
   ```

4. **Stage Prefabs (if you have them):**
   ```
   Drag your 4 prefabs into the Stage Prefabs list
   Or leave empty for now
   ```

5. **Harvest Settings:**
   ```
   Harvest Item ID: glowberry
   Harvest Yield Min: 3
   Harvest Yield Max: 5
   Is Renewable: ✅
   Regrow Time: 180
   ```

6. **Requirements:**
   ```
   Needs Water: ✅
   Water Per Stage: 1
   Needs Garden Plot: ✅
   ```

7. **Special Properties:**
   ```
   Is Magical: ✅
   Glows At Night: ✅
   ```

8. **Visual & Audio:**
   ```
   Plant Color: Light Green (#90EE90)
   Water Sound Name: plant_water
   Harvest Sound Name: plant_harvest
   Grow Sound Name: plant_grow
   ```

9. **Value:**
   ```
   Rarity: Common
   Harvest Experience: 10
   ```

10. **Save:** `Ctrl+S`

### Plant 2: Moonflower (10 minutes)

Follow same steps with these values:
```
Plant Name: Moonflower
Total Stages: 4
Stage Growth Times: 90, 150, 180, 120
Harvest Item ID: moonflower_petal
Yield: 2-4
Is Renewable: ❌
Water Per Stage: 2 (more needy!)
Rarity: Rare
Harvest Experience: 25
```

### Plant 3: Crystal Sprout (10 minutes)

Follow same steps with these values:
```
Plant Name: Crystal Sprout
Total Stages: 4
Stage Growth Times: 180, 300, 420, 300
Harvest Item ID: crystal_shard
Yield: 1-3
Is Renewable: ✅
Regrow Time: 600
Water Per Stage: 1
Rarity: Epic
Harvest Experience: 50
```

### ✅ Verify Plants:
- [ ] All 3 plants created in Data/Plants/
- [ ] Growth times configured
- [ ] Harvest settings correct
- [ ] No errors in Console

---

## ✨ PART 2: Creating Spells (40 minutes)

### Setup (2 minutes)

```
Right-click: Assets/Game/Data/
Create → Folder → "Spells"
```

### Spell 1: Growth Spell (6 minutes)

1. **Create ScriptableObject:**
   ```
   Right-click in Assets/Game/Data/Spells/
   Create → Cozy Game → Magic → Spell Data
   Name: "GrowthSpell"
   ```

2. **Configure:**
   ```
   Spell Name: Growth Spell
   Description: Channels nature's magic to accelerate plant growth.
   Mana Cost: 20
   Cast Range: 10
   Cast Time: 0
   Cooldown: 2
   Spell Type: Targeted
   Target Type: Environment
   Primary Effect: Growth
   Effect Power: 1.0
   Required Level: 0
   Can Cast While Moving: ✅
   Spell Color: Lime Green (#32CD32)
   Cast Sound Name: spell_growth
   ```

3. **Save:** `Ctrl+S`

### Spell 2: Harvest Spell (6 minutes)

```
Spell Name: Harvest Spell
Description: Auto-harvests all mature plants within range.
Mana Cost: 35
Cast Range: 5
Cast Time: 1
Cooldown: 10
Spell Type: AoE
Target Type: Environment
Primary Effect: Harvest
Effect Power: 1.0
AOE Radius: 5
Required Level: 5
Can Cast While Moving: ❌
Spell Color: Golden Yellow (#FFD700)
Cast Sound Name: spell_harvest
```

### Spell 3: Light Spell (6 minutes)

```
Spell Name: Wisp Light
Description: Conjures a floating light that follows you.
Mana Cost: 15
Cast Range: 0
Cast Time: 0
Cooldown: 0
Spell Type: Instant
Target Type: Self
Primary Effect: Light
Effect Power: 1.0
Duration: 300
AOE Radius: 10
Required Level: 0
Can Cast While Moving: ✅
Spell Color: Lemon Chiffon (#FFFACD)
Cast Sound Name: spell_light
```

### Spell 4: Blink (Teleport) (6 minutes)

```
Spell Name: Blink
Description: Instantly teleport a short distance forward.
Mana Cost: 30
Cast Range: 8
Cast Time: 0
Cooldown: 5
Spell Type: Instant
Target Type: Ground
Primary Effect: Teleport
Effect Power: 8.0
Required Level: 10
Can Cast While Moving: ✅
Spell Color: Medium Purple (#9370DB)
Cast Sound Name: spell_teleport
```

### Spell 5: Summon Wisp (6 minutes)

```
Spell Name: Summon Wisp
Description: Summons a wisp that collects nearby resources.
Mana Cost: 40
Cast Range: 2
Cast Time: 2
Cooldown: 60
Spell Type: Channeled
Target Type: Self
Primary Effect: Summon
Effect Power: 1.0
Duration: 120
AOE Radius: 8
Required Level: 15
Can Cast While Moving: ❌
Spell Color: Cyan (#00FFFF)
Cast Sound Name: spell_summon
```

### ✅ Verify Spells:
- [ ] All 5 spells created in Data/Spells/
- [ ] Mana costs configured
- [ ] Spell types set correctly
- [ ] No errors in Console

---

## 🐉 PART 3: Creating Quests (30 minutes)

### Setup (2 minutes)

```
Right-click: Assets/Game/Data/
Create → Folder → "Quests"
```

### Quest 1: The Shiny Things (8 minutes)

1. **Create ScriptableObject:**
   ```
   Right-click in Assets/Game/Data/Quests/
   Create → Cozy Game → Quest Data
   Name: "Quest_ShinyThings"
   ```

2. **Basic Info:**
   ```
   Quest Name: The Shiny Things
   Quest Giver: Dragon
   Description: The Dragon has a fondness for shiny objects. She's asked
                you to gather some crystals from around the village.
   ```

3. **Requirements:**
   ```
   Click + to add requirement:
     Item Name: crystal
     Required Quantity: 3
   ```

4. **Rewards:**
   ```
   Click + to add rewards (2 total):
     [0] Item Name: fairy_dust
         Quantity: 5
         Description: Magical dust

     [1] Item Name: coin
         Quantity: 10
         Description: Gold coins
   ```

5. **Settings:**
   ```
   Is Repeatable: ✅
   Cooldown Minutes: 60
   ```

6. **Save:** `Ctrl+S`

### Quest 2: Moonlit Medicine (10 minutes)

```
Quest Name: Moonlit Medicine
Quest Giver: Dragon
Description: The Sphinx has fallen ill. Gather Moonflower Petals
             and Glowberries to brew healing potion.

Requirements:
  [0] moonflower_petal, 4
  [1] glowberry, 6

Rewards:
  [0] dragon_scale, 1 (Epic!)
  [1] potion_healing, 2
  [2] coin, 50

Is Repeatable: ❌ (one-time story quest)
```

### Quest 3: The Crystal Garden (12 minutes)

```
Quest Name: The Crystal Garden
Quest Giver: Dragon
Description: Help the Dragon create an eternal Crystal Garden.

Requirements:
  [0] crystal_shard, 5
  [1] moonflower_petal, 3
  [2] fairy_dust, 20
  [3] enchanted_wood, 10

Rewards:
  [0] dragon_heart_crystal, 1 (Legendary!)
  [1] spell_tome_advanced, 1
  [2] crystal_sprout_seed, 3
  [3] coin, 200

Is Repeatable: ❌ (ultimate quest)
```

### ✅ Verify Quests:
- [ ] All 3 quests created in Data/Quests/
- [ ] Requirements added correctly
- [ ] Rewards added correctly
- [ ] Repeatable settings configured

---

## 🦁 PART 4: Creating Riddles (20 minutes)

### Setup (2 minutes)

```
Right-click: Assets/Game/Data/
Create → Folder → "Riddles"
```

### Riddle 1: Day and Night (3 minutes)

1. **Create ScriptableObject:**
   ```
   Right-click in Assets/Game/Data/Riddles/
   Create → Cozy Game → Riddle Data
   Name: "Riddle_DayAndNight"
   ```

2. **Configure:**
   ```
   Question: I fall but never break, I break but never fall. What am I?

   Difficulty: Easy

   Answers (4 total):
     [0] Day and Night
     [1] Water
     [2] Glass
     [3] Shadow

   Correct Answer Index: 0

   Correct Feedback: Excellent! Day falls and night breaks.
   Wrong Feedback: Not quite. Think about cycles...
   Hint Text: Consider what happens every 24 hours.

   Rewards:
     [0] fairy_dust, 3

   Cooldown Minutes: 60
   Is Repeatable: ✅
   ```

3. **Save**

### Riddle 2: Footsteps (3 minutes)

```
Question: The more you take, the more you leave behind. What am I?
Difficulty: Easy
Answers: Footsteps, Time, Memories, Money
Correct Index: 0
Feedback: Precisely! More steps = more footprints.
Rewards: glowberry (5), coin (10)
Cooldown: 60
```

### Riddle 3: A Hole (Sponge) (4 minutes)

```
Question: I have holes in my top and bottom, left and right,
          and in my middle. Yet I still hold water. What am I?
Difficulty: Medium
Answers: A Sponge, A Bucket, A Net, A Cloud
Correct Index: 0
Feedback: Brilliant! A sponge has holes yet absorbs water.
Rewards: crystal (2), moonflower_petal (1)
Cooldown: 120 (2 hours)
```

### Riddle 4: The River (4 minutes)

```
Question: I run but never walk, have a mouth but never talk,
          have a bed but never sleep. What am I?
Difficulty: Medium
Answers: A River, A Road, Time, A Watch
Correct Index: 0
Feedback: Exactly! Rivers run, have mouths and beds.
Rewards: crystal_shard (1), fairy_dust (10)
Cooldown: 120
```

### Riddle 5: Darkness (4 minutes)

```
Question: I cannot be seen, felt, heard, or smelt. I lie behind
          stars and under hills, and empty holes I fill. What am I?
Difficulty: Hard
Answers: Darkness, Time, Death, Nothing
Correct Index: 0
Feedback: Incredible! One of the ancient riddles!
Rewards: sphinx_blessing (1), crystal_shard (3), coin (100)
Cooldown: 240 (4 hours)
```

### ✅ Verify Riddles:
- [ ] All 5 riddles created in Data/Riddles/
- [ ] Answers configured (4 each)
- [ ] Correct answer index set
- [ ] Feedback text entered
- [ ] Rewards added

---

## 🔗 PART 5: Connecting to Managers (10 minutes)

### Add Riddles to RiddleManager:

1. **Find RiddleManager in scene** (Hierarchy)
2. **Select it**
3. **In Inspector, find "All Riddles" list**
4. **Set Size: 5**
5. **Drag all 5 riddles from Project into the list:**
   - Element 0: Riddle_DayAndNight
   - Element 1: Riddle_Footsteps
   - Element 2: Riddle_Hole
   - Element 3: Riddle_River
   - Element 4: Riddle_Darkness

### Register Spells with MagicSystem (optional):

If your MagicSystem has a spell registry:
1. Select MagicSystem in Hierarchy
2. Add all 5 spells to registered spells list

---

## 🧪 PART 6: Quick Testing (20 minutes)

### Test 1: Plants (5 minutes)

1. **Create test plant in scene:**
   ```
   GameObject → Create Empty
   Name: "TestGlowberry"
   Add Component → PlantGrowth
   Assign PlantData: Glowberry
   Set Auto Grow: ✅
   Set Growth Speed Multiplier: 10 (for fast testing)
   ```

2. **Run game**
   - Plant should cycle through growth stages
   - Watch console for growth logs
   - Wait for harvestable stage

3. **Test harvest:**
   ```
   In Console, while game is running:
   Click on TestGlowberry in Hierarchy
   In Inspector, find PlantGrowth script
   Right-click on component → Call "Harvest()"
   ```

### Test 2: Spells (5 minutes)

1. **In Console while game running:**
   ```
   // Give yourself mana
   MagicSystem.Instance.RestoreManaFull();

   // Test Growth Spell
   SpellData spell = // Load your Growth Spell asset
   MagicSystem.Instance.CastSpell(spell, transform.position);
   ```

2. **Check Console for:**
   - Mana deduction
   - Spell cast log
   - No errors

### Test 3: Quests (5 minutes)

1. **In Console while game running:**
   ```
   // Start quest
   QuestManager.Instance.StartQuest(questData);

   // Check it's active
   Debug.Log(QuestManager.Instance.IsQuestActive(questData));

   // Give yourself quest items (for testing)
   SurvivalEngineHelper.AddItemToInventory("crystal", 3);

   // Complete quest
   QuestManager.Instance.CompleteQuest(questData);
   ```

### Test 4: Riddles (5 minutes)

1. **In Console while game running:**
   ```
   // Get random riddle
   RiddleData riddle = RiddleManager.Instance.GetRandomAvailableRiddle();
   Debug.Log(riddle.question);

   // Answer it (0 = first answer, which is correct for Day/Night)
   bool correct = RiddleManager.Instance.AnswerRiddle(riddle, 0);
   Debug.Log("Correct: " + correct);
   ```

---

## ✅ Final Verification Checklist

### Data Created:
- [ ] 3 Plants in Assets/Game/Data/Plants/
- [ ] 5 Spells in Assets/Game/Data/Spells/
- [ ] 3 Quests in Assets/Game/Data/Quests/
- [ ] 5 Riddles in Assets/Game/Data/Riddles/

### Configured:
- [ ] All plants have growth times set
- [ ] All spells have mana costs set
- [ ] All quests have requirements and rewards
- [ ] All riddles have 4 answers and correct index set
- [ ] Riddles added to RiddleManager

### Tested:
- [ ] Plant grows through stages
- [ ] Spell can be cast
- [ ] Quest can be started and completed
- [ ] Riddle can be answered
- [ ] No errors in Console

---

## 🎉 Congratulations!

You now have:
- **16 ScriptableObjects** of game content
- **Complete data** for MVP gameplay
- **Tested systems** ready to play

### What You Can Do Now:

**Play the Full Loop:**
```
1. Plant Glowberry seeds
2. Use Growth Spell to speed them up
3. Harvest berries
4. Complete "Shiny Things" quest with crystals
5. Answer Sphinx riddle
6. Get rewards
7. Plant Moonflowers for next quest
8. Repeat!
```

---

## 📊 Time Tracking

| Task | Estimated Time | Your Time |
|------|----------------|-----------|
| Plants (3) | 30 min | ___ |
| Spells (5) | 40 min | ___ |
| Quests (3) | 30 min | ___ |
| Riddles (5) | 20 min | ___ |
| Manager Setup | 10 min | ___ |
| Testing | 20 min | ___ |
| **TOTAL** | **~2.5 hours** | ___ |

---

## 🚀 Next Steps

**Now that you have data:**
1. Create actual plant prefabs (replace placeholders)
2. Add spell visual effects (particles)
3. Implement Survival Engine integration
4. Create NPC dialogues
5. Build UI panels
6. Add more content!

**Or continue with me:**
- "Create more spells"
- "Build the building system"
- "Help me create plant models"
- "Let's integrate with Survival Engine"

---

**All example data ready to create!** 🎮

See individual guides for detailed configurations:
- EXAMPLE_DATA_PLANTS.md
- EXAMPLE_DATA_SPELLS.md
- EXAMPLE_DATA_QUESTS.md
- EXAMPLE_DATA_RIDDLES.md
