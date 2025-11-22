# 📋 ScriptableObject Configuration Templates

## 🎒 ItemData Templates

### Magic Wand
```
Name: "Magic Wand"
Description: "A simple wand that channels magical energy. Use it to cast spells!"
Icon: [Magic wand sprite]
Type: Tool
Stack Size: 1
Max Durability: 100
Equip Slot: Main Hand
Equip Bonuses:
  - Crafting Speed: +10%
Actions:
  - CastGrowthSpellAction (Mana Cost: 10)
Attributes:
  - Mana: -10 (on use)
```

### Crystal Shard
```
Name: "Crystal Shard"
Description: "A glowing crystal fragment. Used in magical crafting."
Icon: [Crystal sprite]
Type: Material
Stack Size: 50
Value: 5 (if selling is implemented)
```

### Fairy Dust
```
Name: "Fairy Dust"
Description: "Sparkling dust left by magical creatures. Essential for enchantments."
Icon: [Sparkle sprite]
Type: Material
Stack Size: 50
Value: 10
```

### Potion
```
Name: "Healing Potion"
Description: "A basic potion that restores health."
Icon: [Potion sprite]
Type: Consumable
Stack Size: 10
Use Action: RestoreHealthAction
Effect: Restore 50 HP
```

---

## 🐉 CharacterData Templates

### Dragon
```
Name: "Dragon"
Display Name: "Ancient Dragon"
Description: "A wise old dragon who loves collecting crystals."
Type: NPC
Friendly: True
Health: 1000 (immortal for MVP)
Default Actions:
  - TalkAction
  - DeliverItemsAction
Dialogue:
  - Greeting: "Hello, little one! I have a favor to ask..."
  - Quest Given: "Could you bring me 3 Crystal Shards? I'll reward you!"
  - Quest Complete: "Thank you! Here's your reward!"
  - No Quest: "Come back if you need anything!"
```

### Sphinx
```
Name: "Sphinx"
Display Name: "Riddling Sphinx"
Description: "A mysterious creature who guards ancient knowledge."
Type: NPC
Friendly: True
Health: 1000
Default Actions:
  - AskRiddleAction
Dialogue:
  - Greeting: "Answer my riddle, and you shall be rewarded!"
  - Correct Answer: "Well done! Take this reward."
  - Wrong Answer: "Incorrect. Return when you are wiser."
  - Cooldown: "You have already attempted today. Return tomorrow."
```

### Firefly
```
Name: "Firefly"
Display Name: "Magical Firefly"
Description: "A tiny glowing creature that flits about."
Type: Creature
Friendly: True
Health: 10
AI Behavior: Wander
Movement Speed: 2
Size: Small (0.5x scale)
```

### Baby Slime
```
Name: "Baby Slime"
Display Name: "Baby Slime"
Description: "A cute, bouncy slime creature."
Type: Creature
Friendly: True
Health: 20
AI Behavior: Idle/Wander
Movement Speed: 1
Size: Small (0.75x scale)
```

---

## 🌾 PlantData Templates

### Glowberry
```
Name: "Glowberry"
Display Name: "Glowberry Bush"
Description: "A magical berry bush that glows in the dark."
Growth Stages: 4
  - Stage 1: Seed (sprite: seed)
  - Stage 2: Sprout (sprite: small plant)
  - Stage 3: Growing (sprite: medium plant)
  - Stage 4: Mature (sprite: full bush with berries)
Growth Time Per Stage: 120 seconds (2 minutes)
Watering Bonus: 50% faster growth
Harvest Item: Glowberry (ItemData)
Harvest Amount: 2-3 (random)
Regrows: Yes (after 5 minutes)
Season: All seasons (for MVP)
```

### Moonflower
```
Name: "Moonflower"
Display Name: "Moonflower"
Description: "A flower that blooms under moonlight. Used in potions."
Growth Stages: 3
  - Stage 1: Seed
  - Stage 2: Growing
  - Stage 3: Blooming
Growth Time Per Stage: 180 seconds (3 minutes)
Watering Bonus: 50% faster growth
Harvest Item: Moonflower Petal (ItemData)
Harvest Amount: 1-2
Regrows: No (replant needed)
```

### Crystal Sprout
```
Name: "Crystal Sprout"
Display Name: "Crystal Sprout"
Description: "A plant that grows crystal formations."
Growth Stages: 4
  - Stage 1: Seed
  - Stage 2: Sprout
  - Stage 3: Growing
  - Stage 4: Crystal Formation
Growth Time Per Stage: 240 seconds (4 minutes)
Watering Bonus: 50% faster growth
Harvest Item: Crystal Shard (ItemData)
Harvest Amount: 1-2
Regrows: Yes (after 10 minutes)
```

---

## 🏗️ BuildableData Templates

### Small House
```
Name: "Small House"
Display Name: "Small House"
Description: "A cozy starter home. Your first building!"
Size: 4x4 units
Required Materials:
  - Planks: 20
  - Stone: 10
Build Time: 5 seconds
Can Enter: True (for future)
Interior: None (for MVP)
Placement Rules:
  - Must be on flat ground
  - Cannot overlap other buildings
  - Minimum distance from other buildings: 2 units
```

### Magic Workbench
```
Name: "Magic Workbench"
Display Name: "Magic Workbench"
Description: "A crafting station for magical items."
Size: 2x1 units
Required Materials:
  - Wood: 10
  - Crystal Shard: 3
Build Time: 3 seconds
Crafting Station: True
Crafting Recipes:
  - Magic Wand Recipe
  - Potion Recipe
  - Enchanted Wood Recipe
Placement Rules:
  - Can be placed indoors or outdoors
  - Must be on flat surface
```

### Garden Plot
```
Name: "Garden Plot"
Display Name: "Garden Plot"
Description: "A prepared area for planting magical seeds."
Size: 2x2 units
Required Materials:
  - Wood: 5
  - Stone: 3
Build Time: 2 seconds
Allows Planting: True
Plant Slots: 4 (one per square unit)
Placement Rules:
  - Must be on flat ground
  - Cannot overlap other plots
```

---

## 🔨 CraftData Templates

### Magic Wand Recipe
```
Name: "Magic Wand Recipe"
Display Name: "Magic Wand"
Description: "Craft a basic magic wand."
Crafting Station: Magic Workbench
Required Materials:
  - Wood: 5
  - Crystal Shard: 2
  - Fairy Dust: 1
Craft Time: 10 seconds
Result Item: Magic Wand
Result Amount: 1
Unlocked By: Default (available from start)
```

### Potion Recipe
```
Name: "Potion Recipe"
Display Name: "Healing Potion"
Description: "Brew a healing potion."
Crafting Station: Magic Workbench
Required Materials:
  - Glowberry: 2
  - Moonflower Petal: 1
  - Water: 1 (if implemented, or skip for MVP)
Craft Time: 5 seconds
Result Item: Healing Potion
Result Amount: 1
Unlocked By: Default
```

### Planks Recipe
```
Name: "Planks Recipe"
Display Name: "Wooden Planks"
Description: "Process wood into planks for building."
Crafting Station: Magic Workbench (or any crafting station)
Required Materials:
  - Wood: 2
Craft Time: 3 seconds
Result Item: Planks
Result Amount: 1
Unlocked By: Default
```

### Enchanted Wood Recipe
```
Name: "Enchanted Wood Recipe"
Display Name: "Enchanted Wood"
Description: "Infuse wood with magical energy."
Crafting Station: Magic Workbench
Required Materials:
  - Wood: 3
  - Fairy Dust: 1
Craft Time: 5 seconds
Result Item: Enchanted Wood
Result Amount: 1
Unlocked By: Default
```

---

## ⚙️ AttributeData Templates

### Mana
```
Name: "Mana"
Display Name: "Mana"
Description: "Magical energy used to cast spells."
Max Value: 100
Start Value: 50
Regeneration Rate: 1 per second
Regeneration Delay: 0 seconds (starts immediately)
Show In UI: True
Icon: [Mana icon sprite]
Color: Blue/Purple
```

---

## 🎯 Quest Templates

### Dragon's Crystal Quest
```
Quest Name: "Dragon's Request"
Quest Description: "The ancient dragon needs 3 Crystal Shards for his collection. Help him out!"
Quest Giver: Dragon
Required Items:
  - Crystal Shard: 3
Reward Items:
  - Enchanted Wood: 5
  - Experience: 50 (if XP system exists)
Quest Type: Item Delivery
Repeatable: Yes (can be done multiple times)
Cooldown: None (for MVP)
```

---

## 📝 Riddle Templates

### Riddle 1
```
Question: "I speak without a mouth and hear without ears. I have no body, but I come alive with wind. What am I?"
Answers:
  - "An echo" (Correct)
  - "A shadow"
  - "A ghost"
Reward: Crystal Shard x2
```

### Riddle 2
```
Question: "The more you take, the more you leave behind. What am I?"
Answers:
  - "Footsteps" (Correct)
  - "Memories"
  - "Time"
Reward: Fairy Dust x3
```

### Riddle 3
```
Question: "I have cities, but no houses. I have mountains, but no trees. I have water, but no fish. What am I?"
Answers:
  - "A map" (Correct)
  - "A dream"
  - "A painting"
Reward: Enchanted Wood x3
```

### Riddle 4
```
Question: "What has keys but no locks, space but no room, and you can enter but not go inside?"
Answers:
  - "A keyboard" (Correct)
  - "A door"
  - "A book"
Reward: Magic Potion x1
```

### Riddle 5
```
Question: "I'm tall when I'm young, and short when I'm old. What am I?"
Answers:
  - "A candle" (Correct)
  - "A tree"
  - "A person"
Reward: Glowberry x5
```

---

## 💡 Tips for Creating ScriptableObjects

1. **Naming Convention:**
   - Use descriptive names
   - Keep consistent naming (e.g., "MagicWand" not "magic_wand" or "Magic Wand Item")

2. **Testing:**
   - Create items in Play mode to test
   - Use Unity's Inspector to verify values
   - Test interactions immediately after creation

3. **Organization:**
   - Keep related assets in same folders
   - Use subfolders for categories
   - Name prefabs clearly

4. **Iteration:**
   - Start with placeholder values
   - Adjust based on gameplay feel
   - Balance numbers through playtesting

