# 📅 Development Schedule - 5 Week MVP (Beginner Edition)

> **Note:** This schedule is designed for complete beginners working with AI assistance. Each task includes detailed steps.

---

## 🎯 Overview

- **Total Time:** 5 weeks
- **Daily Commitment:** 2-4 hours
- **Approach:** Follow steps exactly, ask AI for code
- **Testing:** Test every day before finishing

---

## Week 1: Foundation & Magic System

**Goal:** Get player moving, gathering resources, and using magic

### Day 1-2: Project Setup & Player Character

**Time:** 4-6 hours total

#### Day 1 Morning: Unity Setup (2 hours)
- [ ] Follow `BEGINNER_SETUP_GUIDE.md` completely
- [ ] Install Unity 2022.3 LTS
- [ ] Create new project
- [ ] Import Survival Engine
- [ ] Create folder structure

**❓ Ask AI:** "Help me create the folder structure from BEGINNER_SETUP_GUIDE.md"

#### Day 1 Afternoon: Player Character (2 hours)
- [ ] Download player character from Mixamo (see `ASSET_INTEGRATION_GUIDE.md`)
- [ ] Download Idle and Walk animations
- [ ] Import to Unity (`Assets/Game/Art/Mixamo/`)
- [ ] Configure as Humanoid rig
- [ ] Test in empty scene

**❓ Ask AI:** "Walk me through importing Mixamo character step by step"

#### Day 2 Morning: Player Movement (2 hours)
- [ ] Create InputManager script (from `PLATFORM_GUIDE.md`)
- [ ] Set up player controller from Survival Engine
- [ ] Test keyboard movement (WASD)
- [ ] Add camera follow

**❓ Ask AI:** "Give me the complete InputManager.cs code"

#### Day 2 Afternoon: Scene Setup (2 hours)
- [ ] Create main scene: "MainGame"
- [ ] Add terrain or ground plane
- [ ] Add lighting (Directional Light)
- [ ] Place player in scene
- [ ] Test: Can walk around

**✅ Checkpoint:** Player moves with WASD, camera follows

---

### Day 3-4: Magic System & Resources

**Time:** 4-6 hours total

#### Day 3 Morning: Mana Attribute (2 hours)
- [ ] Read Survival Engine docs on Attributes
- [ ] Create Mana AttributeData ScriptableObject
- [ ] Set max mana: 100
- [ ] Set regen rate: 5 per second
- [ ] Add to player

**❓ Ask AI:** "How do I create an AttributeData ScriptableObject for Mana?"

#### Day 3 Afternoon: Magic Wand Item (2 hours)
- [ ] Create MagicWand ItemData ScriptableObject
- [ ] Set icon (use placeholder or AI-generated)
- [ ] Set as equippable item
- [ ] Add to player starting inventory

**❓ Ask AI:** "Give me the settings for a Magic Wand ItemData"

#### Day 4 Morning: Growth Spell Action (2-3 hours)
- [ ] Create `CastGrowthSpellAction.cs` script
- [ ] Implement spell casting logic:
  - Use 20 mana
  - Raycast to target
  - Show particle effect
- [ ] Add to Magic Wand item

**❓ Ask AI:** "Write CastGrowthSpellAction.cs script for Survival Engine"

#### Day 4 Afternoon: Testing & Polish (2 hours)
- [ ] Add magic VFX (free particle from Asset Store)
- [ ] Add casting sound effect
- [ ] Test complete magic loop
- [ ] Fix any bugs

**✅ Checkpoint:** Can cast spell with magic wand, uses mana

---

### Day 5: Basic Resources & Gathering

**Time:** 3-4 hours

#### Day 5 Morning: Resource Items (2 hours)
- [ ] Create ItemData for: Wood, Stone, Crystal
- [ ] Create simple 3D models or use placeholders (cubes)
- [ ] Set stack sizes (Wood: 99, Stone: 99, Crystal: 50)
- [ ] Create item prefabs with Selectable component

**❓ Ask AI:** "How do I create gatherable resource items in Survival Engine?"

#### Day 5 Afternoon: World Placement & Testing (2 hours)
- [ ] Place resources around scene (10-15 of each)
- [ ] Test gathering (click to pick up)
- [ ] Verify inventory updates
- [ ] Add pickup particle effect
- [ ] Add pickup sound

**✅ Checkpoint:** Can gather wood, stone, crystals

**🎉 Week 1 Deliverable:** Player can move, gather resources, use magic wand

## Week 2: Crafting & Plants

**Goal:** Create working crafting system and magical garden

### Day 1-2: Crafting System

**Time:** 4-6 hours total

#### Day 1 Morning: Crafting Recipes (2 hours)
- [ ] Read Survival Engine docs on CraftData
- [ ] Create recipe: Wood → Planks (5 Wood = 1 Plank)
- [ ] Create recipe: Potion (2 Berries + 1 Crystal)
- [ ] Create recipe: Magic Wand (3 Wood + 2 Crystals)
- [ ] Test recipes in Survival Engine crafting system

**❓ Ask AI:** "How do I create CraftData recipes in Survival Engine?"

#### Day 1 Afternoon: Magic Workbench (2 hours)
- [ ] Create Magic Workbench 3D model (or use placeholder cube)
- [ ] Create ConstructionData for workbench
- [ ] Set build cost: 10 Wood, 5 Stone
- [ ] Make workbench enable advanced recipes

**❓ Ask AI:** "How do I create a ConstructionData for a crafting station?"

#### Day 2: Crafting Testing & Polish (3-4 hours)
- [ ] Build workbench in scene
- [ ] Test crafting all 3 recipes
- [ ] Add crafting success particle/sound
- [ ] Add UI feedback for recipe completion
- [ ] Verify resource consumption

**✅ Checkpoint:** Can build workbench and craft items

---

### Day 3-4: Plant System

**Time:** 5-6 hours total

#### Day 3 Morning: Plant Data (2 hours)
- [ ] Create PlantData for Glowberry
  - Growth stages: 4 (seed, sprout, growing, harvestable)
  - Growth time: 5 minutes total
  - Harvest yield: 3-5 berries
- [ ] Create PlantData for Moonflower (same structure)
- [ ] Create PlantData for Crystal Sprout

**❓ Ask AI:** "Give me PlantData settings for magical plants in Survival Engine"

#### Day 3 Afternoon: Plant Models (2-3 hours)
- [ ] Create or download low poly plant models
- [ ] Create 4 growth stage prefabs for each plant
- [ ] Add colliders
- [ ] Add Plant component from Survival Engine
- [ ] Test spawning plants

#### Day 4 Morning: Garden Plot (2 hours)
- [ ] Create Garden Plot model (raised dirt bed)
- [ ] Create ConstructionData for garden plot
- [ ] Set build cost: 5 Wood, 10 Dirt
- [ ] Configure to allow planting

**❓ Ask AI:** "How do I create a buildable garden plot for plants?"

#### Day 4 Afternoon: Planting & Growth (2 hours)
- [ ] Build garden plot in scene
- [ ] Plant all 3 seed types
- [ ] Test growth stages (speed up for testing)
- [ ] Test harvesting
- [ ] Add growth complete notification

**✅ Checkpoint:** Plants grow through stages and can be harvested

---

### Day 5: Building System

**Time:** 3-4 hours

#### Day 5 Morning: Small House (2 hours)
- [ ] Create Small House 3D model (simple cube house)
- [ ] Create ConstructionData
- [ ] Set build cost: 20 Wood, 10 Stone, 5 Planks
- [ ] Test placement and building

**❓ Ask AI:** "How do I make a buildable house in Survival Engine?"

#### Day 5 Afternoon: Furniture & Testing (2 hours)
- [ ] Create 2 furniture items (chair, table)
- [ ] Make placeable inside house
- [ ] Test complete building loop
- [ ] Add build complete particle/sound

**✅ Checkpoint:** Can build house and place furniture

**🎉 Week 2 Deliverable:** Complete crafting loop, working garden, can build house

---

## Week 3: NPCs Part 1 - Dragon

**Goal:** Create interactive Dragon NPC with quest system

### Day 1-2: Quest System Foundation

**Time:** 5-6 hours total

#### Day 1 Morning: Quest Data Structure (2 hours)
- [ ] Create `QuestData.cs` ScriptableObject script
- [ ] Define quest properties:
  - Quest name, description
  - Required items and quantities
  - Reward items
  - Quest state (inactive, active, completed)

**❓ Ask AI:** "Create QuestData ScriptableObject script for item delivery quests"

#### Day 1 Afternoon: Quest Manager (2 hours)
- [ ] Create `QuestManager.cs` singleton script
- [ ] Implement quest tracking
- [ ] Add quest start/complete functions
- [ ] Create quest UI panel (simple text display)

**❓ Ask AI:** "Write QuestManager.cs for tracking active quests"

#### Day 2: Deliver Items Action (3 hours)
- [ ] Create `DeliverItemsAction.cs` script
- [ ] Check player inventory for required items
- [ ] Remove items on delivery
- [ ] Grant rewards
- [ ] Test with dummy data

**✅ Checkpoint:** Quest system can track and complete quests

---

### Day 3-4: Dragon Implementation

**Time:** 5-6 hours total

#### Day 3 Morning: Dragon Character (2 hours)
- [ ] Download dragon model (Mixamo or Asset Store)
- [ ] Import and configure
- [ ] Create Dragon CharacterData
- [ ] Create Dragon prefab with Selectable component
- [ ] Add idle animation

**❓ Ask AI:** "How do I set up an NPC character in Survival Engine?"

#### Day 3 Afternoon: Dragon Dialogue (2 hours)
- [ ] Use Survival Engine dialogue system
- [ ] Create dialogue for quest introduction
- [ ] Create dialogue for quest in progress
- [ ] Create dialogue for quest completion
- [ ] Test talking to Dragon

#### Day 4: First Quest (3-4 hours)
- [ ] Create quest: "Gather 3 Crystals"
- [ ] Set reward: 5 Fairy Dust
- [ ] Connect to Dragon NPC
- [ ] Test complete quest loop:
  1. Talk to Dragon
  2. Get quest
  3. Gather crystals
  4. Return to Dragon
  5. Complete quest

**✅ Checkpoint:** Can complete "Gather Crystals" quest

---

### Day 5: Polish & Additional Quests

**Time:** 3-4 hours

- [ ] Add quest accepted sound/particle
- [ ] Add quest complete celebration
- [ ] Create 2 more quests:
  - "Build Magic Workbench"
  - "Craft a Potion"
- [ ] Test all quests
- [ ] Add quest tracker UI (shows current objective)

**✅ Checkpoint:** Dragon has 3 working quests

**🎉 Week 3 Deliverable:** Working Dragon NPC with quest system

---

## Week 4: NPCs Part 2 - Sphinx & Creatures

**Goal:** Add Sphinx riddles and magical creatures

### Day 1-2: Sphinx Implementation

**Time:** 5-6 hours total

#### Day 1 Morning: Sphinx Character (2 hours)
- [ ] Download/create Sphinx model
- [ ] Create Sphinx CharacterData
- [ ] Create Sphinx prefab
- [ ] Place in special area of world
- [ ] Add idle animation

#### Day 1 Afternoon: Riddle Data (2 hours)
- [ ] Create `RiddleData.cs` ScriptableObject
- [ ] Define structure:
  - Question text
  - 3-4 answer options
  - Correct answer
  - Reward
- [ ] Create 5 riddle assets

**❓ Ask AI:** "Create RiddleData ScriptableObject for riddle system"

#### Day 2 Morning: Riddle Action (2-3 hours)
- [ ] Create `AskRiddleAction.cs` script
- [ ] Show riddle UI panel
- [ ] Check answer
- [ ] Grant reward if correct
- [ ] Give hint if wrong (no punishment)

**❓ Ask AI:** "Write AskRiddleAction.cs for Survival Engine"

#### Day 2 Afternoon: Riddle UI (2 hours)
- [ ] Create Riddle UI panel
- [ ] Add question text
- [ ] Add answer buttons (4 options)
- [ ] Add result feedback (correct/wrong)
- [ ] Style for cozy aesthetic

**✅ Checkpoint:** Sphinx asks riddles and gives rewards

---

### Day 3: Riddle Content & Cooldowns

**Time:** 3-4 hours

- [ ] Write 10 total riddles (easy to medium difficulty)
- [ ] Add riddle cooldown system (1 riddle per hour)
- [ ] Add "next riddle in X minutes" timer
- [ ] Test all riddles
- [ ] Balance rewards

**Examples:**
- "I fall but never break, I break but never fall. What am I?"
- "The more you take, the more you leave behind. What am I?"

**✅ Checkpoint:** Sphinx has 10 riddles with cooldown

---

### Day 4: Magical Creatures

**Time:** 3-4 hours

#### Firefly (1.5 hours)
- [ ] Create firefly model (simple glowing orb with wings)
- [ ] Add gentle floating animation
- [ ] Add glow particle effect
- [ ] Add simple wander AI
- [ ] Place 10-15 in world (near water/forest)

**❓ Ask AI:** "Create simple wander AI script for passive creatures"

#### Baby Slime (1.5 hours)
- [ ] Create baby slime model (cute blob)
- [ ] Add bounce animation
- [ ] Add simple wander AI
- [ ] Place 5-10 in world (near village)

**✅ Checkpoint:** Creatures spawn and wander naturally

---

### Day 5: Integration & Testing

**Time:** 3-4 hours

- [ ] Test all NPCs (Dragon, Sphinx)
- [ ] Test all creatures
- [ ] Verify quest system works
- [ ] Verify riddle system works
- [ ] Fix any bugs
- [ ] Add ambient creature sounds

**🎉 Week 4 Deliverable:** Sphinx with riddles, magical creatures in world

---

## Week 5: World Building & Polish

**Goal:** Create cohesive world and polish all systems

### Day 1-2: World Design

**Time:** 6-8 hours total

#### Day 1: Village Area (3-4 hours)
- [ ] Design village layout (sketch on paper first)
- [ ] Place ground textures/terrain
- [ ] Add village buildings (3-5 small houses)
- [ ] Add decorations (flowers, fences, paths)
- [ ] Place Dragon NPC
- [ ] Add lighting

#### Day 2 Morning: Forest Zone (2 hours)
- [ ] Create forest gathering area
- [ ] Place trees (from Asset Store)
- [ ] Place rocks for stone gathering
- [ ] Place crystal resource nodes
- [ ] Add creature spawns (fireflies, slimes)

#### Day 2 Afternoon: Sphinx Area (2 hours)
- [ ] Create special mystical area (ruins? temple?)
- [ ] Place Sphinx
- [ ] Add atmospheric elements (fog, particles)
- [ ] Make it feel special/hidden

**✅ Checkpoint:** Complete playable world

---

### Day 3: Content Balance

**Time:** 3-4 hours

- [ ] Playtest full game loop (30 min session)
- [ ] Adjust resource respawn rates
- [ ] Adjust crafting costs (not too cheap/expensive)
- [ ] Adjust plant growth times (5-10 min feels good)
- [ ] Balance quest rewards
- [ ] Adjust riddle cooldowns

**✅ Checkpoint:** Game feels balanced

---

### Day 4: Multi-Platform Testing

**Time:** 3-4 hours

- [ ] Build for PC, test thoroughly
- [ ] Build for WebGL, test in browser
- [ ] Add virtual joystick for mobile (if targeting mobile)
- [ ] Test touch controls
- [ ] Optimize performance (target 30 FPS minimum)
- [ ] Fix platform-specific bugs

**❓ Ask AI:** "Help me optimize for WebGL and mobile"

**✅ Checkpoint:** Runs on all target platforms

---

### Day 5: Final Polish & Release Prep

**Time:** 4-6 hours

#### Morning: Bug Fixes & Polish (2-3 hours)
- [ ] Fix all critical bugs
- [ ] Add tutorial text prompts (first 5 minutes)
- [ ] Add basic sound effects (pickup, craft, complete)
- [ ] Add background music (find free track)
- [ ] Test complete 1-hour play session

#### Afternoon: Release Preparation (2-3 hours)
- [ ] Create build for PC
- [ ] Create build for WebGL
- [ ] Test final builds
- [ ] Take screenshots for itch.io
- [ ] Write game description
- [ ] Upload to itch.io (optional)

**✅ Checkpoint:** MVP complete and playable!

**🎉 Week 5 Deliverable:** Complete playable MVP with all core features

---

## 🎯 Milestone Checklist

### End of Week 1
- ✅ Player movement works
- ✅ Magic system functional
- ✅ Basic gathering works

### End of Week 2
- ✅ Can craft items
- ✅ Can grow plants
- ✅ Can build structures

### End of Week 3
- ✅ Dragon quest system works
- ✅ Can complete quests

### End of Week 4
- ✅ Sphinx riddles work
- ✅ Creatures in world
- ✅ All NPCs functional

### End of Week 5
- ✅ Complete world built
- ✅ All systems balanced
- ✅ MVP is playable end-to-end

---

## 📝 Daily Development Tips

### Keep It Simple
- Use placeholder art/sprites for MVP
- Focus on gameplay loop, not graphics
- Test each feature as you build it

### Survival Engine Best Practices
- Use existing systems when possible
- Extend, don't replace
- Test ScriptableObject changes in Play mode

### Debugging
- Use Unity Console for errors
- Test interactions in Play mode
- Keep a notepad of bugs to fix

### Version Control
- Commit daily
- Use descriptive commit messages
- Tag major milestones

---

## 🚀 Post-MVP Ideas (Future)

- More quest types
- More magical creatures
- Player house interior
- More crafting recipes
- Seasonal events
- Multiplayer (if desired)
- More biomes/areas
- Character customization
- Achievement system

