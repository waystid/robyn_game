# 🌱 MVP Structure for Cozy Magical Community Game

## 📁 Unity Project Folder Structure

```
Assets/
├── SurvivalEngine/              # (Existing Survival Engine assets)
│
├── Game/
│   ├── Scripts/
│   │   ├── Magic/
│   │   │   ├── MagicSystem.cs
│   │   │   ├── ManaAttribute.cs
│   │   │   └── MagicWandAction.cs
│   │   ├── Creatures/
│   │   │   ├── DragonNPC.cs
│   │   │   ├── SphinxNPC.cs
│   │   │   └── QuestSystem.cs
│   │   ├── UI/
│   │   │   ├── RiddleUI.cs
│   │   │   └── QuestUI.cs
│   │   └── CustomActions/
│   │       ├── AskRiddleAction.cs
│   │       ├── DeliverItemsAction.cs
│   │       └── CastGrowthSpellAction.cs
│   │
│   ├── Data/
│   │   ├── Items/
│   │   │   ├── Magic/
│   │   │   │   ├── MagicWand.asset
│   │   │   │   ├── Spellbook.asset
│   │   │   │   ├── Potion.asset
│   │   │   │   ├── FairyDust.asset
│   │   │   │   └── CrystalShard.asset
│   │   │   ├── Materials/
│   │   │   │   ├── EnchantedWood.asset
│   │   │   │   ├── Wood.asset
│   │   │   │   ├── Stone.asset
│   │   │   │   └── Planks.asset
│   │   │   └── Tools/
│   │   │       ├── BasicAxe.asset
│   │   │       └── BasicPickaxe.asset
│   │   │
│   │   ├── Characters/
│   │   │   ├── Dragon.asset
│   │   │   ├── Sphinx.asset
│   │   │   ├── Firefly.asset
│   │   │   └── BabySlime.asset
│   │   │
│   │   ├── Plants/
│   │   │   ├── Glowberry.asset
│   │   │   ├── Moonflower.asset
│   │   │   └── CrystalSprout.asset
│   │   │
│   │   ├── Crafting/
│   │   │   ├── MagicWandRecipe.asset
│   │   │   ├── PotionRecipe.asset
│   │   │   ├── PlanksRecipe.asset
│   │   │   └── HouseRecipe.asset
│   │   │
│   │   ├── Buildings/
│   │   │   ├── SmallHouse.asset
│   │   │   ├── MagicWorkbench.asset
│   │   │   └── GardenPlot.asset
│   │   │
│   │   └── Attributes/
│   │       └── Mana.asset
│   │
│   ├── Prefabs/
│   │   ├── Player/
│   │   │   └── PlayerCharacter.prefab
│   │   ├── Creatures/
│   │   │   ├── Dragon.prefab
│   │   │   ├── Sphinx.prefab
│   │   │   ├── Firefly.prefab
│   │   │   └── BabySlime.prefab
│   │   ├── Items/
│   │   │   ├── MagicWand.prefab
│   │   │   └── Potion.prefab
│   │   ├── Plants/
│   │   │   ├── Glowberry.prefab
│   │   │   ├── Moonflower.prefab
│   │   │   └── CrystalSprout.prefab
│   │   └── Buildings/
│   │       ├── SmallHouse.prefab
│   │       ├── MagicWorkbench.prefab
│   │       └── GardenPlot.prefab
│   │
│   ├── Scenes/
│   │   ├── MainGame.unity
│   │   └── StartingVillage.unity
│   │
│   └── UI/
│       ├── Prefabs/
│       │   ├── RiddlePanel.prefab
│       │   └── QuestPanel.prefab
│       └── Sprites/
│           └── (UI elements)
│
└── Resources/
    └── (Runtime-loaded assets)
```

## 🎯 MVP Feature Checklist

### ✅ Phase 1: Core Setup (Week 1)
- [ ] Set up folder structure
- [ ] Import Survival Engine (if not already)
- [ ] Create base player prefab with movement
- [ ] Set up basic inventory system
- [ ] Create starting scene with village area

### ✅ Phase 2: Magic System (Week 1-2)
- [ ] Create Mana attribute (AttributeData)
- [ ] Implement mana regeneration
- [ ] Create Magic Wand item (ItemData)
- [ ] Create basic magic action (Cast Growth Spell)
- [ ] Add magic wand to player inventory

### ✅ Phase 3: Resources & Crafting (Week 2)
- [ ] Create basic resource items (Wood, Stone, Crystals)
- [ ] Create magical resource items (Fairy Dust, Crystal Shards)
- [ ] Set up crafting recipes (Planks, Potion, Magic Wand)
- [ ] Create Magic Workbench buildable
- [ ] Test crafting loop

### ✅ Phase 4: Plants & Gardening (Week 2-3)
- [ ] Create plant data (Glowberry, Moonflower, Crystal Sprout)
- [ ] Create plant prefabs with growth stages
- [ ] Implement watering system
- [ ] Create Garden Plot buildable
- [ ] Test plant growth cycle

### ✅ Phase 5: Building System (Week 3)
- [ ] Create Small House buildable
- [ ] Create Magic Workbench buildable
- [ ] Create Garden Plot buildable
- [ ] Test building placement
- [ ] Add basic furniture (1-2 items)

### ✅ Phase 6: Dragon NPC (Week 3-4)
- [ ] Create Dragon CharacterData
- [ ] Create Dragon prefab
- [ ] Implement quest system (simple item delivery)
- [ ] Create "Deliver Items" action
- [ ] Test quest: "Bring 3 Crystals"

### ✅ Phase 7: Sphinx NPC (Week 4)
- [ ] Create Sphinx CharacterData
- [ ] Create Sphinx prefab
- [ ] Create AskRiddleAction
- [ ] Create Riddle UI
- [ ] Add 3-5 riddles to test

### ✅ Phase 8: Magical Creatures (Week 4)
- [ ] Create Firefly prefab (simple sprite)
- [ ] Create Baby Slime prefab
- [ ] Add basic AI (wander/idle)
- [ ] Place in world

### ✅ Phase 9: World Building (Week 4-5)
- [ ] Design starting village area
- [ ] Create forest zone for gathering
- [ ] Create special area for Sphinx
- [ ] Place all NPCs and creatures
- [ ] Add basic decorations

### ✅ Phase 10: Polish & Testing (Week 5)
- [ ] Balance resource gathering rates
- [ ] Test complete gameplay loop
- [ ] Fix bugs
- [ ] Add basic sound effects (optional)
- [ ] Create simple tutorial prompts

## 🎮 Core Gameplay Loop

1. **Explore** → Player walks around world
2. **Gather** → Collect Wood, Stone, Crystals, Plants
3. **Craft** → Use Magic Workbench to create items
4. **Build** → Place Small House, Workbench, Garden Plot
5. **Grow** → Plant seeds, water, harvest
6. **Befriend** → Talk to Dragon, complete quests
7. **Solve** → Answer Sphinx riddles for rewards
8. **Repeat** → Expand, craft more, complete more quests

