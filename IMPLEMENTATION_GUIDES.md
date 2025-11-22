# 🛠️ Implementation Guides

## 🪄 1. Magic System Implementation

### Step 1: Create Mana Attribute

**File:** `Assets/Game/Data/Attributes/Mana.asset`

1. Create new `AttributeData` ScriptableObject
2. Set properties:
   - **Name:** "Mana"
   - **Max Value:** 100
   - **Regeneration Rate:** 1 per second
   - **Start Value:** 50

### Step 2: Add Mana to Player

**File:** `Assets/Game/Scripts/Magic/ManaAttribute.cs`

```csharp
// Extend Survival Engine's Attribute system
// Add mana regeneration in Update()
```

### Step 3: Create Magic Wand Item

**File:** `Assets/Game/Data/Items/Magic/MagicWand.asset`

1. Create new `ItemData` ScriptableObject
2. Set properties:
   - **Name:** "Magic Wand"
   - **Type:** Tool/Weapon
   - **Equip Bonus:** +10% Crafting Speed
   - **Actions:** CastGrowthSpellAction
   - **Mana Cost:** 10

### Step 4: Create Growth Spell Action

**File:** `Assets/Game/Scripts/CustomActions/CastGrowthSpellAction.cs`

**Functionality:**
- Checks if player has enough mana
- Targets nearby plants
- Instantly grows plant to next stage
- Consumes mana

---

## 🐉 2. Dragon NPC Implementation

### Step 1: Create Dragon CharacterData

**File:** `Assets/Game/Data/Characters/Dragon.asset`

1. Create new `CharacterData` ScriptableObject
2. Set properties:
   - **Name:** "Dragon"
   - **Type:** NPC
   - **Friendly:** True
   - **Actions:**
     - TalkAction
     - DeliverItemsAction (Quest)
     - ReceiveRewardAction

### Step 2: Create Dragon Prefab

**File:** `Assets/Game/Prefabs/Creatures/Dragon.prefab`

**Components:**
- Transform
- Character (Character.cs from Survival Engine)
- Selectable (for interaction)
- Collider (for detection)
- Animator (optional, can use sprite renderer for MVP)

**Character Component Settings:**
- Character Data: Dragon.asset
- Default Actions: Talk, Deliver Items

### Step 3: Create Quest System

**File:** `Assets/Game/Scripts/Creatures/QuestSystem.cs`

**Simple MVP Quest Structure:**
```csharp
[System.Serializable]
public class Quest
{
    public string questName;
    public string description;
    public ItemData requiredItem;
    public int requiredAmount;
    public ItemData rewardItem;
    public int rewardAmount;
    public bool isCompleted;
}
```

### Step 4: Create Deliver Items Action

**File:** `Assets/Game/Scripts/CustomActions/DeliverItemsAction.cs`

**Functionality:**
- Checks player inventory for required items
- If player has enough, remove items
- Give reward items
- Mark quest as complete
- Show UI feedback

**Example Quest:**
- **Name:** "Dragon's Request"
- **Description:** "I need 3 Crystal Shards for my collection!"
- **Required:** Crystal Shard x3
- **Reward:** Enchanted Wood x5

---

## 🦁 3. Sphinx NPC Implementation

### Step 1: Create Sphinx CharacterData

**File:** `Assets/Game/Data/Characters/Sphinx.asset`

1. Create new `CharacterData` ScriptableObject
2. Set properties:
   - **Name:** "Sphinx"
   - **Type:** NPC
   - **Friendly:** True
   - **Actions:** AskRiddleAction

### Step 2: Create Sphinx Prefab

**File:** `Assets/Game/Prefabs/Creatures/Sphinx.prefab`

**Components:**
- Transform
- Character
- Selectable
- Collider
- Sprite Renderer (or 3D model)

### Step 3: Create Riddle Data Structure

**File:** `Assets/Game/Scripts/Creatures/RiddleData.cs`

```csharp
[System.Serializable]
public class Riddle
{
    public string question;
    public string[] answers; // 3 options
    public int correctAnswerIndex;
    public ItemData reward;
}
```

### Step 4: Create Ask Riddle Action

**File:** `Assets/Game/Scripts/CustomActions/AskRiddleAction.cs`

**Functionality:**
1. Load random riddle from list
2. Show Riddle UI panel
3. Display question and 3 answer choices
4. On selection:
   - If correct → Give reward, show success message
   - If wrong → Show "Try again later" message
5. Cooldown: Can only try once per day (or per session for MVP)

### Step 5: Create Riddle UI

**File:** `Assets/Game/Scripts/UI/RiddleUI.cs`

**UI Elements:**
- Question text
- 3 answer buttons
- Close button
- Reward display (on success)

**File:** `Assets/Game/UI/Prefabs/RiddlePanel.prefab`

---

## 🌾 4. Magical Plants Implementation

### Step 1: Create Plant Data

**Example: Glowberry**

**File:** `Assets/Game/Data/Plants/Glowberry.asset`

1. Create new `PlantData` ScriptableObject
2. Set properties:
   - **Name:** "Glowberry"
   - **Growth Stages:** 4 (Seed → Sprout → Growing → Mature)
   - **Growth Time:** 2 minutes per stage (or use Survival Engine defaults)
   - **Watering Bonus:** 50% faster growth
   - **Harvest Item:** Glowberry (ItemData)
   - **Harvest Amount:** 2-3 berries

### Step 2: Create Plant Prefabs

**File:** `Assets/Game/Prefabs/Plants/Glowberry.prefab`

**Components:**
- Transform
- Plant (Plant.cs from Survival Engine)
- Sprite Renderer (different sprite per growth stage)
- Collider

**Plant Component Settings:**
- Plant Data: Glowberry.asset
- Growth Stages: 4 sprites

### Step 3: Create Garden Plot

**File:** `Assets/Game/Data/Buildings/GardenPlot.asset`

1. Create new `BuildableData` ScriptableObject
2. Set properties:
   - **Name:** "Garden Plot"
   - **Required Materials:** Wood x5, Stone x3
   - **Size:** 2x2 grid
   - **Allows Planting:** True

---

## 🏗️ 5. Building System Setup

### Step 1: Create Small House

**File:** `Assets/Game/Data/Buildings/SmallHouse.asset`

1. Create new `BuildableData` ScriptableObject
2. Set properties:
   - **Name:** "Small House"
   - **Required Materials:** 
     - Planks x20
     - Stone x10
   - **Size:** 4x4 units
   - **Can Enter:** True (for future interior)

### Step 2: Create Magic Workbench

**File:** `Assets/Game/Data/Buildings/MagicWorkbench.asset`

1. Create new `BuildableData` ScriptableObject
2. Set properties:
   - **Name:** "Magic Workbench"
   - **Required Materials:**
     - Wood x10
     - Crystal Shard x3
   - **Size:** 2x1 units
   - **Crafting Station:** True
   - **Crafting Recipes:** (Link to recipes)

---

## 🎨 6. Item Creation Guide

### Creating a New Item (Example: Crystal Shard)

1. **Create ItemData:**
   - Right-click in `Assets/Game/Data/Items/Materials/`
   - Create → Survival Engine → Item Data
   - Name: "Crystal Shard"
   - Set icon, description, stack size

2. **Create Item Prefab (if needed for world placement):**
   - Create empty GameObject
   - Add Item component
   - Assign ItemData
   - Add collider
   - Save as prefab

3. **Add to Crafting Recipe:**
   - Open recipe ScriptableObject
   - Add Crystal Shard to required materials

---

## 🧪 7. Testing Checklist

### Magic System
- [ ] Mana regenerates over time
- [ ] Magic wand can be equipped
- [ ] Growth spell works on plants
- [ ] Mana is consumed when casting

### Dragon Quest
- [ ] Can talk to Dragon
- [ ] Quest appears in UI
- [ ] Can deliver required items
- [ ] Reward is given correctly
- [ ] Quest completes properly

### Sphinx Riddles
- [ ] Can interact with Sphinx
- [ ] Riddle UI appears
- [ ] Can select answers
- [ ] Correct answer gives reward
- [ ] Wrong answer shows message
- [ ] Cooldown works (if implemented)

### Plants
- [ ] Can plant seeds in garden plot
- [ ] Plants grow over time
- [ ] Watering speeds up growth
- [ ] Can harvest mature plants
- [ ] Harvest gives correct items

### Crafting
- [ ] Can access Magic Workbench
- [ ] Recipes appear correctly
- [ ] Can craft with required materials
- [ ] Crafted items appear in inventory

### Building
- [ ] Can place buildings
- [ ] Materials are consumed
- [ ] Buildings persist after placement
- [ ] Can interact with placed buildings

