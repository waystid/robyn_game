# ✨ Example Spell Data Configurations
## Complete Setup for 5 Magic Spells

> **How to use:** Follow each spell's configuration in Unity to create the ScriptableObjects.

---

## 🌱 Spell 1: Growth Spell

### Basic Info
```
File Name: GrowthSpell.asset
Location: Assets/Game/Data/Spells/

Spell Name: Growth Spell
Spell ID: spell_growth (auto-generated)
Description: Channels nature's magic to accelerate plant growth.
             Target a plant to instantly advance it to the next stage.
```

### Mana & Casting
```
Mana Cost: 20
Cast Range: 10 meters
Cast Time: 0 seconds (instant)
Cooldown: 2 seconds
```

### Spell Type & Target
```
Spell Type: Targeted
Target Type: Environment (plants, crops)
```

### Visual & Audio
```
Spell Effect Prefab: (assign green/nature particle effect)
Cast Sound Name: spell_growth
Spell Color: #32CD32 (lime green)
```

### Spell Effects
```
Primary Effect: Growth
Effect Power: 1.0 (advances one stage)
Duration: 0 (instant)
AOE Radius: 0 (single target)
```

### Requirements
```
Required Level: 0 (starter spell)
Can Cast While Moving: ✅ YES
```

### 📝 Usage Notes:
- Perfect starting spell
- Low mana cost = can cast frequently
- Teaches magic system basics
- Saves 6+ minutes per Glowberry
- Essential for efficient farming

---

## 🌾 Spell 2: Harvest Spell

### Basic Info
```
File Name: HarvestSpell.asset
Location: Assets/Game/Data/Spells/

Spell Name: Harvest Spell
Spell ID: spell_harvest
Description: Summons ethereal hands to automatically harvest all mature
             plants within range. A farmer's best friend!
```

### Mana & Casting
```
Mana Cost: 35
Cast Range: 5 meters (caster position)
Cast Time: 1 second (short channel)
Cooldown: 10 seconds
```

### Spell Type & Target
```
Spell Type: AoE (Area of Effect)
Target Type: Environment (plants only)
```

### Visual & Audio
```
Spell Effect Prefab: (assign golden harvest glow with wheat particles)
Cast Sound Name: spell_harvest
Spell Color: #FFD700 (golden yellow)
```

### Spell Effects
```
Primary Effect: Harvest
Effect Power: 1.0
Duration: 0 (instant)
AOE Radius: 5 meters (harvests all plants in range)
```

### Requirements
```
Required Level: 5 (mid-game unlock)
Can Cast While Moving: ❌ NO (must stand still for 1 sec)
```

### 📝 Usage Notes:
- Higher mana cost = use strategically
- Area effect = efficient for large gardens
- 1 second cast = slight risk (can be interrupted)
- Perfect for mass harvesting
- Great for farming efficiency

### 💡 Implementation Tip:
```csharp
// In your HarvestSpell script:
// Find all plants in radius
Collider[] plants = Physics.OverlapSphere(transform.position, 5f);
foreach (var plant in plants)
{
    PlantGrowth growth = plant.GetComponent<PlantGrowth>();
    if (growth != null && growth.plantData.IsHarvestableStage(growth.currentStage))
    {
        growth.Harvest();
    }
}
```

---

## 💡 Spell 3: Light Spell

### Basic Info
```
File Name: LightSpell.asset
Location: Assets/Game/Data/Spells/

Spell Name: Wisp Light
Spell ID: spell_light
Description: Conjures a floating wisp of magical light that follows you.
             Illuminates dark areas and reveals hidden secrets.
```

### Mana & Casting
```
Mana Cost: 15
Cast Range: 0 (self-cast)
Cast Time: 0 (instant)
Cooldown: 0 (can toggle on/off)
```

### Spell Type & Target
```
Spell Type: Instant
Target Type: Self
```

### Visual & Audio
```
Spell Effect Prefab: (assign floating light orb with warm glow)
Cast Sound Name: spell_light
Spell Color: #FFFACD (lemon chiffon - warm white)
```

### Spell Effects
```
Primary Effect: Light
Effect Power: 1.0 (brightness multiplier)
Duration: 300 seconds (5 minutes, or toggle off)
AOE Radius: 10 meters (light radius)
```

### Requirements
```
Required Level: 0 (starter spell)
Can Cast While Moving: ✅ YES
```

### 📝 Usage Notes:
- Utility spell - no combat use
- Low mana cost but sustained drain (3 mana/sec while active)
- Toggle on/off to manage mana
- Essential for night exploration
- Could reveal hidden items/secrets

### 💡 Implementation Tip:
```csharp
// Create light orb that follows player
GameObject lightOrb = Instantiate(lightPrefab, playerPos + Vector3.up * 2f, Quaternion.identity);
lightOrb.transform.SetParent(player.transform);

// Add Light component
Light light = lightOrb.AddComponent<Light>();
light.range = 10f;
light.intensity = 2f;
light.color = spellData.spellColor;

// Add FloatingMotion for nice effect
FloatingMotion float = lightOrb.AddComponent<FloatingMotion>();
```

---

## 🌀 Spell 4: Teleport Spell

### Basic Info
```
File Name: TeleportSpell.asset
Location: Assets/Game/Data/Spells/

Spell Name: Blink
Spell ID: spell_teleport
Description: Instantly teleport a short distance in the direction you're facing.
             Great for crossing gaps or escaping danger.
```

### Mana & Casting
```
Mana Cost: 30
Cast Range: 8 meters (teleport distance)
Cast Time: 0 (instant)
Cooldown: 5 seconds
```

### Spell Type & Target
```
Spell Type: Instant
Target Type: Ground (teleports to valid ground position)
```

### Visual & Audio
```
Spell Effect Prefab: (assign void/portal swirl particle)
Cast Sound Name: spell_teleport
Spell Color: #9370DB (medium purple)
```

### Spell Effects
```
Primary Effect: Teleport
Effect Power: 8.0 (distance in meters)
Duration: 0 (instant)
AOE Radius: 0
```

### Requirements
```
Required Level: 10 (advanced spell)
Can Cast While Moving: ✅ YES
```

### 📝 Usage Notes:
- Advanced mobility spell
- Higher mana cost = use strategically
- Must have valid ground at destination
- Can't teleport through walls
- Great for exploration
- Can escape from danger (no fail states, but adds strategy)

### 💡 Implementation Tip:
```csharp
// Raycast forward to find teleport destination
Vector3 targetPos = playerPos + player.forward * 8f;

// Raycast down from above to find ground
if (Physics.Raycast(targetPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
{
    // Spawn particles at current position
    Instantiate(teleportOutParticle, playerPos, Quaternion.identity);

    // Teleport player
    player.position = hit.point;

    // Spawn particles at new position
    Instantiate(teleportInParticle, hit.point, Quaternion.identity);

    // Play sound
    AudioManager.Instance.PlaySound("spell_teleport");
}
```

---

## 🦋 Spell 5: Summon Wisp

### Basic Info
```
File Name: SummonWisp.asset
Location: Assets/Game/Data/Spells/

Spell Name: Summon Wisp
Spell ID: spell_summon_wisp
Description: Summons a friendly magical wisp that collects nearby resources
             for you. The wisp lasts for 2 minutes or until dismissed.
```

### Mana & Casting
```
Mana Cost: 40
Cast Range: 2 meters (spawns near player)
Cast Time: 2 seconds (channeled)
Cooldown: 60 seconds
```

### Spell Type & Target
```
Spell Type: Channeled
Target Type: Self (summons companion)
```

### Visual & Audio
```
Spell Effect Prefab: (assign ethereal wisp creature with trail)
Cast Sound Name: spell_summon
Spell Color: #00FFFF (cyan)
```

### Spell Effects
```
Primary Effect: Summon
Effect Power: 1.0 (number of wisps)
Duration: 120 seconds (2 minutes)
AOE Radius: 8 meters (wisp collection range)
```

### Requirements
```
Required Level: 15 (end-game spell)
Can Cast While Moving: ❌ NO (must channel for 2 seconds)
```

### 📝 Usage Notes:
- Highest mana cost = premium spell
- 2 second channel = slight risk
- Wisp auto-collects resources in 8m radius
- Perfect for AFK farming (within limits)
- Combines with other spells for efficiency
- Long cooldown = use wisely

### 💡 Implementation Tip:
```csharp
// Create wisp GameObject
GameObject wisp = Instantiate(wispPrefab, playerPos + Random.insideUnitSphere * 2f, Quaternion.identity);

// Add SimpleWanderAI (wanders near player)
SimpleWanderAI ai = wisp.AddComponent<SimpleWanderAI>();
ai.wanderRadius = 8f;
ai.moveSpeed = 2f;

// Add FloatingMotion (makes it float)
FloatingMotion float = wisp.AddComponent<FloatingMotion>();

// Add resource collection script
WispCollector collector = wisp.AddComponent<WispCollector>();
collector.collectionRadius = 8f;
collector.collectInterval = 2f; // Collect every 2 seconds

// Auto-destroy after duration
Destroy(wisp, 120f);
```

---

## 📊 Spell Comparison Chart

| Spell | Mana | Range | Cast Time | Cooldown | Level | Effect |
|-------|------|-------|-----------|----------|-------|--------|
| **Growth** | 20 | 10m | Instant | 2s | 0 | Grow plant one stage |
| **Harvest** | 35 | 5m AOE | 1s | 10s | 5 | Auto-harvest all plants |
| **Light** | 15 | Self | Instant | Toggle | 0 | Create light source |
| **Blink** | 30 | 8m | Instant | 5s | 10 | Teleport forward |
| **Summon Wisp** | 40 | 2m | 2s | 60s | 15 | Summon collector |

---

## 🎮 Spell Progression & Unlocks

### Starter Spells (Level 0)
**Growth Spell** + **Light Spell**
- Low mana cost
- Easy to use
- Teach magic basics
- Immediately useful

### Mid-Game Spells (Level 5-10)
**Harvest Spell** + **Blink**
- Higher mana cost
- More powerful effects
- Require strategy
- Reward mastery

### End-Game Spell (Level 15+)
**Summon Wisp**
- Highest mana cost
- Most powerful effect
- Long cooldown
- Ultimate farming tool

---

## 🌟 Spell Combinations

### Efficient Farming Combo:
```
1. Plant seeds in grid pattern
2. Cast Light Spell (see in dark)
3. Cast Growth Spell on each plant (instant growth)
4. Wait for plants to mature
5. Cast Harvest Spell (collect all at once)
6. Summon Wisp (auto-collect drops)

Result: Maximize efficiency!
```

### Exploration Combo:
```
1. Cast Light Spell (illuminate area)
2. Use Blink (traverse terrain quickly)
3. Summon Wisp (collect resources while exploring)

Result: Fast, efficient exploration!
```

### Quest Speed-Run:
```
Dragon Quest: "Harvest 10 Glowberries"

1. Plant 10 Glowberry seeds
2. Cast Growth Spell on all (instant growth)
3. Cast Harvest Spell (instant collection)
4. Complete quest in 2 minutes!

Without spells: 6+ minutes per plant = 60 minutes
With spells: 2 minutes total!
```

---

## 🎨 Visual Effect Suggestions

### Growth Spell:
- Green spiraling particles from ground
- Leaves and sparkles
- Nature sounds (birds, wind)
- Plant glows briefly

### Harvest Spell:
- Golden glow around caster
- Ghostly hands materialize
- Wheat/grain particles
- Satisfying "harvest" sounds

### Light Spell:
- Soft white/yellow orb
- Gentle pulsing
- Subtle trail when moving
- Warm, welcoming sound

### Blink (Teleport):
- Purple void swirl at start
- Player fades out
- Purple swirl at destination
- Player fades in
- "Whoosh" sound

### Summon Wisp:
- Cyan summoning circle
- Particles coalesce into wisp
- Ethereal chiming
- Wisp has trailing particles

---

## 🧪 Testing Your Spells

### Test Setup:
```csharp
// Give max mana for testing
MagicSystem.Instance.SetMaxMana(1000f);
MagicSystem.Instance.RestoreManaFull();

// Remove cooldowns for testing
MagicSystem.Instance.globalCooldown = 0f;

// Test each spell:
MagicSystem.Instance.CastSpell(growthSpell, targetPos);
// Verify: Plant advanced one stage

MagicSystem.Instance.CastSpell(harvestSpell, playerPos);
// Verify: All nearby plants harvested

// Etc.
```

### Test Checklist Per Spell:
- [ ] Mana cost is deducted
- [ ] Mana regenerates after delay
- [ ] Cast range is enforced
- [ ] Cooldown prevents spam
- [ ] Visual effects play
- [ ] Sound effects play
- [ ] Spell effect works as intended
- [ ] Works on correct targets only
- [ ] Fails gracefully on invalid targets
- [ ] UI updates properly

---

## ✅ Quick Creation Checklist

For each spell:
- [ ] Create SpellData ScriptableObject
- [ ] Set name, ID, description
- [ ] Configure mana cost and range
- [ ] Set cast time and cooldown
- [ ] Choose spell type and target
- [ ] Assign visual effect prefab (or placeholder)
- [ ] Set cast sound name
- [ ] Choose spell color
- [ ] Configure primary effect
- [ ] Set level requirement
- [ ] Test in game!

---

## 💡 Custom Spell Ideas

Once you're comfortable, create your own:

**Healing Spring:**
- Restores health/mana over time
- Creates visual spring effect
- Players stand in area to heal

**Speed Boost:**
- Temporary movement speed increase
- Blue trail effect
- Duration-based

**Reveal:**
- Shows hidden items/secrets on map
- Pulse wave visual
- Exploration aid

**Transform:**
- Turn one item into another
- Alchemical effect
- Advanced crafting

**Weather Control:**
- Change time of day
- Affect plant growth
- Atmospheric effects

---

**All 5 spells ready to create!** ✨

Next: Quest and Riddle data! →
