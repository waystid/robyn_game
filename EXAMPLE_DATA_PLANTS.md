# 🌱 Example Plant Data Configurations
## Complete Setup for 3 Magical Plants

> **How to use:** Follow each plant's configuration exactly in Unity to create the ScriptableObjects.

---

## 🫐 Plant 1: Glowberry Bush

### Basic Info
```
File Name: Glowberry.asset
Location: Assets/Game/Data/Plants/

Plant Name: Glowberry Bush
Plant ID: glowberry (auto-generated)
Description: A magical bush that produces glowing berries.
             Perfect for crafting potions and attracting fireflies.
```

### Growth Configuration
```
Total Stages: 4

Stage Prefabs: (assign when you have models)
  [0] GlowberrySeed (or empty placeholder)
  [1] GlowberrySprout
  [2] GlowberryBush
  [3] GlowberryBush_Berries

Stage Growth Times:
  [0] 60 seconds  (seed → sprout)
  [1] 90 seconds  (sprout → bush)
  [2] 120 seconds (bush → berries)
  [3] 90 seconds  (mature)

Total Growth Time: 6 minutes
```

### Harvest Settings
```
Harvest Item ID: glowberry
Harvest Yield Min: 3
Harvest Yield Max: 5
Is Renewable: ✅ YES
Regrow Time: 180 seconds (3 minutes)
```

### Requirements
```
Needs Water: ✅ YES
Water Per Stage: 1 (water once per stage = 4 times total)
Needs Garden Plot: ✅ YES
```

### Special Properties
```
Is Magical: ✅ YES
Glows At Night: ✅ YES
Growth Particle Prefab: (assign magic sparkle particle)
Ready Particle Prefab: (assign glow particle)
```

### Visual & Audio
```
Plant Color: #90EE90 (light green with glow)
Water Sound Name: plant_water
Harvest Sound Name: plant_harvest
Grow Sound Name: plant_grow
```

### Value
```
Rarity: Common
Harvest Experience: 10 XP
```

### 📝 Creation Steps in Unity:
1. Right-click in `Assets/Game/Data/Plants/`
2. Create → Cozy Game → Plants → Plant Data
3. Name it: `Glowberry`
4. Copy all settings above
5. Save (Ctrl+S)

---

## 🌙 Plant 2: Moonflower

### Basic Info
```
File Name: Moonflower.asset
Location: Assets/Game/Data/Plants/

Plant Name: Moonflower
Plant ID: moonflower (auto-generated)
Description: A rare flower that blooms only under moonlight.
             Its petals are prized for powerful potions and enchantments.
```

### Growth Configuration
```
Total Stages: 4

Stage Prefabs:
  [0] MoonflowerSeed
  [1] MoonflowerSprout
  [2] MoonflowerBud
  [3] MoonflowerBloom

Stage Growth Times:
  [0] 90 seconds  (seed → sprout)
  [1] 150 seconds (sprout → bud)
  [2] 180 seconds (bud → bloom)
  [3] 120 seconds (full bloom)

Total Growth Time: 9 minutes
```

### Harvest Settings
```
Harvest Item ID: moonflower_petal
Harvest Yield Min: 2
Harvest Yield Max: 4
Is Renewable: ❌ NO (one-time harvest)
Regrow Time: 0 (doesn't regrow)
```

### Requirements
```
Needs Water: ✅ YES
Water Per Stage: 2 (water twice per stage = 8 times total, very needy!)
Needs Garden Plot: ✅ YES
```

### Special Properties
```
Is Magical: ✅ YES
Glows At Night: ✅ YES (bright blue/white glow)
Growth Particle Prefab: (assign moonlight sparkle)
Ready Particle Prefab: (assign ethereal glow)
```

### Visual & Audio
```
Plant Color: #E6E6FA (lavender/pale blue)
Water Sound Name: plant_water
Harvest Sound Name: plant_harvest_rare
Grow Sound Name: plant_grow_magical
```

### Value
```
Rarity: Rare
Harvest Experience: 25 XP
```

### 📝 Special Notes:
- Higher water requirements = more player engagement
- One-time harvest = more valuable
- Requires more care than Glowberry
- Perfect for potion crafting quests

---

## 💎 Plant 3: Crystal Sprout

### Basic Info
```
File Name: CrystalSprout.asset
Location: Assets/Game/Data/Plants/

Plant Name: Crystal Sprout
Plant ID: crystal_sprout (auto-generated)
Description: An extraordinarily rare magical plant that grows living crystals.
             Legend says it can only grow from magical soil touched by dragon fire.
```

### Growth Configuration
```
Total Stages: 4

Stage Prefabs:
  [0] CrystalSeed (glowing seed)
  [1] CrystalSprout (tiny crystal formation)
  [2] CrystalGrowth (medium crystals)
  [3] CrystalCluster (large, harvestable crystals)

Stage Growth Times:
  [0] 180 seconds (3 min - seed → sprout)
  [1] 300 seconds (5 min - sprout → growth)
  [2] 420 seconds (7 min - growth → cluster)
  [3] 300 seconds (5 min - mature)

Total Growth Time: 20 minutes (longest!)
```

### Harvest Settings
```
Harvest Item ID: crystal_shard
Harvest Yield Min: 1
Harvest Yield Max: 3
Is Renewable: ✅ YES
Regrow Time: 600 seconds (10 minutes)
```

### Requirements
```
Needs Water: ✅ YES
Water Per Stage: 1
Needs Garden Plot: ✅ YES (special magical garden plot)
```

### Special Properties
```
Is Magical: ✅ YES (extremely magical)
Glows At Night: ✅ YES (pulsing rainbow glow)
Growth Particle Prefab: (assign crystal sparkle with rainbow colors)
Ready Particle Prefab: (assign intense magical aura)
```

### Visual & Audio
```
Plant Color: #87CEEB (sky blue with crystal shine)
Water Sound Name: plant_water_magical
Harvest Sound Name: crystal_harvest
Grow Sound Name: crystal_grow
```

### Value
```
Rarity: Epic
Harvest Experience: 50 XP
```

### 📝 Special Notes:
- Longest growth time = highest reward
- Perfect end-game plant
- Renewable but slow regrow = balanced
- Great for Dragon quests
- Can be used in advanced crafting

---

## 📊 Plant Comparison Chart

| Plant | Growth Time | Yield | Renewable | Rarity | Water Needs | XP |
|-------|-------------|-------|-----------|--------|-------------|-----|
| **Glowberry** | 6 min | 3-5 | ✅ Yes (3m) | Common | 4 waters | 10 |
| **Moonflower** | 9 min | 2-4 | ❌ No | Rare | 8 waters | 25 |
| **Crystal Sprout** | 20 min | 1-3 | ✅ Yes (10m) | Epic | 4 waters | 50 |

---

## 🎮 Gameplay Progression

### Early Game (First 30 minutes)
**Plant: Glowberry**
- Easy to grow (6 min)
- Renewable (constant supply)
- Good for learning mechanics
- Use for: Basic quests, early potions

### Mid Game (After 1 hour)
**Plant: Moonflower**
- Longer growth (9 min)
- Higher water needs (teaches resource management)
- One-time harvest (teaches planning)
- Use for: Advanced potions, rare crafting

### Late Game (After 2+ hours)
**Plant: Crystal Sprout**
- Very long growth (20 min)
- Epic rewards
- Renewable but slow
- Use for: Dragon quests, end-game crafting, trading

---

## 🌟 Usage in Game Systems

### For Quests:
```
Dragon Quest 1: "Bring me 3 Glowberries"
  - Easy, renewable, teaches plant system

Dragon Quest 2: "Bring me 2 Moonflower Petals"
  - Harder, requires planning, teaches patience

Dragon Quest 3: "Bring me 1 Crystal Shard"
  - End-game, very valuable, teaches commitment
```

### For Crafting:
```
Basic Potion: 2 Glowberries
Advanced Potion: 1 Moonflower Petal + 2 Glowberries
Epic Potion: 1 Crystal Shard + 1 Moonflower Petal

Magic Wand Recipe: 3 Wood + 1 Crystal Shard
```

### For Magic Spells:
```
Growth Spell: Works on all plants
  - On Glowberry: Save 6 minutes
  - On Moonflower: Save 9 minutes
  - On Crystal Sprout: Save 20 minutes (huge value!)
```

---

## 🎨 Placeholder Asset Tips

**If you don't have 3D models yet:**

### Glowberry:
- Stage 1: Small green sphere
- Stage 2: Larger green sphere with small spheres
- Stage 3: Cluster of green spheres
- Stage 4: Same but with emission/glow material

### Moonflower:
- Stage 1: Tiny white/blue cylinder
- Stage 2: Cylinder with cone on top (bud)
- Stage 3: Cone expanding (opening)
- Stage 4: Flat disc (open flower) with glow

### Crystal Sprout:
- Stage 1: Small cube
- Stage 2: 3 small cubes clustered
- Stage 3: 5-7 cubes of varying heights
- Stage 4: Large cluster with emission material

**Use Unity primitives with different:**
- Scales (small → large)
- Colors (dull → bright)
- Materials (solid → glowing)
- Particle effects (none → sparkles)

---

## ✅ Quick Creation Checklist

For each plant:
- [ ] Create PlantData ScriptableObject
- [ ] Set all basic info
- [ ] Configure growth stages and times
- [ ] Set harvest settings
- [ ] Enable/disable renewable
- [ ] Set water requirements
- [ ] Check special properties
- [ ] Assign colors
- [ ] Set rarity and XP
- [ ] Create placeholder prefabs (or assign real models)
- [ ] Test in game!

---

## 🧪 Testing Your Plants

### Quick Test Setup:
```csharp
// In Unity, select plant GameObject
// Set these in Inspector:
PlantGrowth plant = GetComponent<PlantGrowth>();
plant.autoGrow = true; // No water needed
plant.growthSpeedMultiplier = 10f; // 10x faster (36 seconds for Glowberry!)

// Or use console commands:
// plant.InstantGrowToMature(); // Instant!
```

### Test Checklist:
- [ ] Plant grows through all 4 stages
- [ ] Visual model changes each stage
- [ ] Watering works (if auto-grow off)
- [ ] Harvest gives correct items
- [ ] Renewable plants regrow
- [ ] Non-renewable plants destroy
- [ ] Particles and sounds play
- [ ] Growth spell works on plant
- [ ] Quest tracking updates when harvested

---

**All 3 plants ready to create!** 🌱

Next up: Spell data examples! →
