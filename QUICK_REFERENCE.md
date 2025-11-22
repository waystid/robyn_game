# ⚡ Quick Reference Guide

## 🚀 Getting Started (First 30 Minutes)

1. **Open Unity** and create/import your project
2. **Import Survival Engine** (if not already done)
3. **Create folder structure** (see MVP_STRUCTURE.md)
4. **Start with Magic System:**
   - Create Mana AttributeData
   - Create Magic Wand ItemData
   - Test in scene

## 📋 Daily Workflow

### Morning Setup
- [ ] Open Unity project
- [ ] Check today's tasks in DEVELOPMENT_SCHEDULE.md
- [ ] Create any needed ScriptableObjects first
- [ ] Then implement scripts

### During Development
- [ ] Test frequently (don't wait until end)
- [ ] Use Debug.Log for troubleshooting
- [ ] Keep Unity Console open
- [ ] Save ScriptableObjects before testing

### End of Day
- [ ] Test all changes
- [ ] Commit to version control
- [ ] Note any bugs/issues
- [ ] Plan tomorrow's tasks

## 🎯 MVP Priority Order

1. **Player Movement** (Week 1, Day 1)
2. **Magic System** (Week 1, Day 3-4)
3. **Basic Gathering** (Week 1, Day 5)
4. **Crafting** (Week 2, Day 1-2)
5. **Plants** (Week 2, Day 3-4)
6. **Building** (Week 2, Day 5)
7. **Dragon Quest** (Week 3)
8. **Sphinx Riddles** (Week 4, Day 1-2)
9. **Creatures** (Week 4, Day 4)
10. **World Building** (Week 5)

## 🔑 Key ScriptableObjects to Create First

### Essential Items
- Wood
- Stone
- Crystal Shard
- Magic Wand
- Potion

### Essential Characters
- Dragon
- Sphinx

### Essential Plants
- Glowberry
- Moonflower

### Essential Buildings
- Magic Workbench
- Small House
- Garden Plot

## 🛠️ Common Tasks

### Creating a New Item
1. Right-click in `Assets/Game/Data/Items/`
2. Create → Survival Engine → Item Data
3. Name it, set properties
4. Create prefab if needed for world placement

### Creating a New NPC
1. Create CharacterData ScriptableObject
2. Create prefab with Character component
3. Add Selectable component
4. Assign CharacterData
5. Add actions (Talk, Quest, etc.)

### Creating a New Plant
1. Create PlantData ScriptableObject
2. Set growth stages and times
3. Create prefab with Plant component
4. Add sprites for each growth stage
5. Test planting in garden plot

### Creating a New Crafting Recipe
1. Create CraftData ScriptableObject
2. Set required materials
3. Set result item
4. Assign to crafting station
5. Test crafting

## 🐛 Quick Troubleshooting

| Problem | Solution |
|---------|----------|
| NPC not interactable | Check Selectable component, verify collider |
| Item not appearing | Check ItemData is assigned, verify prefab |
| Crafting not working | Check recipe materials, verify crafting station |
| Plant not growing | Check PlantData settings, verify time values |
| Quest not completing | Check inventory has items, verify quest logic |
| Riddle UI not showing | Check RiddleUI exists in scene, verify references |

## 📞 Survival Engine Integration Points

### Key Components to Use
- `PlayerCharacter` - Player controller
- `Selectable` - For interactable objects
- `Inventory` - Item management
- `Plant` - Plant system
- `Buildable` - Building system
- `Character` - NPC system
- `SAction` - Base for custom actions

### Where to Extend
- Create custom `SAction` subclasses for new interactions
- Extend `AttributeData` for mana system
- Use `ItemData` for all items
- Use `CharacterData` for NPCs
- Use `PlantData` for plants
- Use `BuildableData` for buildings

## 🎨 Art Asset Checklist (MVP)

### Minimum Needed
- [ ] Player sprite/model
- [ ] Ground texture
- [ ] Building sprites (house, workbench)
- [ ] NPC sprites (Dragon, Sphinx)
- [ ] Plant sprites (4 stages each)
- [ ] Item icons
- [ ] Basic UI elements

### Can Use Placeholders
- Unity primitives
- Colored squares
- Text labels
- Free asset store assets

## 📊 Progress Tracking

### Week 1 Checklist
- [ ] Player moves
- [ ] Can gather resources
- [ ] Magic wand works
- [ ] Mana regenerates

### Week 2 Checklist
- [ ] Can craft items
- [ ] Can plant seeds
- [ ] Plants grow
- [ ] Can build structures

### Week 3 Checklist
- [ ] Dragon appears
- [ ] Can talk to Dragon
- [ ] Quest system works
- [ ] Can complete quest

### Week 4 Checklist
- [ ] Sphinx appears
- [ ] Can answer riddles
- [ ] Creatures in world
- [ ] All NPCs work

### Week 5 Checklist
- [ ] World is built
- [ ] All systems integrated
- [ ] Gameplay loop complete
- [ ] MVP is playable

## 💡 Pro Tips

1. **Start Small:** Get one system working before moving to the next
2. **Test Often:** Don't build everything then test - test as you go
3. **Use Placeholders:** Don't wait for perfect art - use cubes/sprites
4. **Document Issues:** Keep notes of bugs to fix later
5. **Balance Later:** Get it working first, balance numbers after
6. **Version Control:** Commit daily, use descriptive messages
7. **Take Breaks:** Don't burn out - steady progress is better

## 📚 Documentation Files

- **README.md** - Overview and getting started
- **MVP_STRUCTURE.md** - Folder structure and feature list
- **IMPLEMENTATION_GUIDES.md** - Step-by-step guides
- **DEVELOPMENT_SCHEDULE.md** - 5-week schedule
- **SCRIPTABLE_OBJECT_TEMPLATES.md** - Ready-to-use templates
- **CODE_TEMPLATES.md** - Custom script templates
- **QUICK_REFERENCE.md** - This file

## 🎮 Testing Your MVP

### Core Loop Test
1. Spawn in world
2. Gather wood and stone
3. Craft planks
4. Build Magic Workbench
5. Craft Magic Wand
6. Plant Glowberry seed
7. Use growth spell on plant
8. Harvest plant
9. Talk to Dragon
10. Complete quest
11. Answer Sphinx riddle
12. Build Small House

If all of these work, your MVP is complete! 🎉

---

**Remember:** This is an MVP. Focus on gameplay, not perfection. You can always polish later!

