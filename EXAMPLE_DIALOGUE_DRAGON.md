# Example Dialogue: Dragon NPC

This document provides complete dialogue tree configurations for the **Dragon NPC** character.

The Dragon is a quest-giving NPC who loves shiny things (crystals, gems) and rewards players for bringing them treasures.

---

## Table of Contents

1. [Dragon Greeting Dialogue](#1-dragon-greeting-dialogue)
2. [Quest Available Dialogue](#2-quest-available-dialogue)
3. [Quest In Progress Dialogue](#3-quest-in-progress-dialogue)
4. [Quest Complete Dialogue](#4-quest-complete-dialogue)
5. [No Quest Dialogue](#5-no-quest-dialogue)
6. [Implementation Guide](#implementation-guide)

---

## 1. Dragon Greeting Dialogue

**Dialogue Name:** `Dragon_Greeting`
**File Name:** `Dragon_Greeting.asset`
**Purpose:** First-time introduction to the Dragon

### Settings
- **Start Node ID:** `start`
- **Is Repeatable:** No (once per session)
- **Cooldown:** 0 minutes
- **Camera Mode:** Close Up

### Dialogue Tree

#### Node: start
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
*The majestic dragon stretches their shimmering wings*

Greetings, little traveler! I am Sparkle, guardian of this magical forest.
I collect beautiful, shiny things... crystals, gems, anything that sparkles!
```
- **Next Node:** `intro_choice`

#### Node: intro_choice
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
Would you like to help me add to my collection? I reward those who bring me treasures!
```
- **Choices:**
  1. "I'd love to help!" → `accept_intro`
  2. "What kind of treasures?" → `explain_treasures`
  3. "Maybe later." → `goodbye`

#### Node: accept_intro
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
Wonderful! I have a feeling we're going to be great friends.
Come back anytime you find something shiny!
```
- **Quest To Start:** `quest_shiny_things`
- **Next Node:** `end`

#### Node: explain_treasures
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
Oh, all sorts of things! Crystals that grow in the forest, rare gems from caves,
magical stones that glow in moonlight... anything beautiful and sparkly!

I'll reward you generously for each treasure you bring me.
```
- **Next Node:** `intro_choice`

#### Node: goodbye
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
No worries! I'll be here whenever you're ready.
Safe travels, little one! ✨
```
- **Is End Node:** Yes

---

## 2. Quest Available Dialogue

**Dialogue Name:** `Dragon_QuestAvailable`
**File Name:** `Dragon_QuestAvailable.asset`
**Purpose:** When dragon has a new quest available

### Settings
- **Start Node ID:** `start`
- **Is Repeatable:** Yes
- **Cooldown:** 0 minutes

### Dialogue Tree

#### Node: start
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
*The dragon's eyes light up with excitement*

Oh, hello again! I've been thinking about something special I'd love to add to my collection...
```
- **Next Node:** `offer_quest`

#### Node: offer_quest
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
Would you be interested in a little treasure hunting for me? I promise to make it worth your while!
```
- **Choices:**
  1. "What do you need?" → `quest_details`
  2. "Show me what you're offering" → `show_rewards`
  3. "Not right now" → `decline`

#### Node: quest_details
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
I'm looking for [QUEST_ITEM]. These treasures are quite rare, but I know you can find them!

Bring me [QUEST_AMOUNT], and I'll give you something special from my hoard.
```
- **Choices:**
  1. "I accept!" → `accept_quest`
  2. "What's the reward?" → `show_rewards`
  3. "Too difficult for me" → `decline`

#### Node: show_rewards
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
For your efforts, I'll give you [REWARD_ITEMS], plus a generous amount of gold!

Dragons always keep their word when it comes to treasure deals. 💎
```
- **Next Node:** `quest_details`

#### Node: accept_quest
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
*The dragon purrs happily*

Excellent! I knew I could count on you. Good luck with your treasure hunting!
```
- **Quest To Start:** `[CURRENT_QUEST_ID]`
- **Animation Trigger:** `Happy`
- **Is End Node:** Yes

#### Node: decline
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
I understand. The offer will still be here when you're ready! ✨
```
- **Is End Node:** Yes

---

## 3. Quest In Progress Dialogue

**Dialogue Name:** `Dragon_QuestInProgress`
**File Name:** `Dragon_QuestInProgress.asset`
**Purpose:** When player has active quest but hasn't completed it

### Settings
- **Start Node ID:** `start`
- **Is Repeatable:** Yes

### Dialogue Tree

#### Node: start
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
Hello again! How goes the treasure hunting?
```
- **Next Node:** `check_progress`

#### Node: check_progress
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
You currently have [CURRENT_AMOUNT] out of [REQUIRED_AMOUNT] [ITEM_NAME].

Keep searching! I believe in you! 🌟
```
- **Choices:**
  1. "Where can I find more?" → `give_hint`
  2. "Thanks for the encouragement!" → `goodbye`

#### Node: give_hint
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
Hmm... I've heard that [ITEM_NAME] can be found near [LOCATION_HINT].

Also, try looking around during [TIME_HINT] - they seem to appear more often then!
```
- **Next Node:** `goodbye`

#### Node: goodbye
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
Happy hunting! I'll be waiting right here. ✨
```
- **Is End Node:** Yes

---

## 4. Quest Complete Dialogue

**Dialogue Name:** `Dragon_QuestComplete`
**File Name:** `Dragon_QuestComplete.asset`
**Purpose:** When player can turn in completed quest

### Settings
- **Start Node ID:** `start`
- **Is Repeatable:** Yes

### Dialogue Tree

#### Node: start
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
*The dragon's eyes sparkle with excitement*

Oh my! You have the [ITEM_NAME]! They're absolutely beautiful! 💎✨
```
- **Animation Trigger:** "Happy"
- **Next Node:** `turn_in`

#### Node: turn_in
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
Are you ready to trade them for your reward?
```
- **Choices:**
  1. "Yes, here you go!" → `complete`
  2. "Actually, I want to keep them" → `keep_items`

#### Node: complete
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
*The dragon carefully takes the treasures and adds them to their collection*

These are perfect! Thank you so much! Here's your reward, as promised.
You're one of my favorite treasure hunters! 🎁
```
- **Quest To Complete:** `[CURRENT_QUEST_ID]`
- **Animation Trigger:** "Celebrate"
- **Auto Advance Delay:** 3 seconds
- **Next Node:** `check_more_quests`

#### Node: keep_items
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
Oh... well, if you change your mind, I'll still be interested!
```
- **Animation Trigger:** "Sad"
- **Is End Node:** Yes

#### Node: check_more_quests
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
If you're up for more treasure hunting, I always have new requests!
Come back anytime!
```
- **Is End Node:** Yes

---

## 5. No Quest Dialogue

**Dialogue Name:** `Dragon_NoQuest`
**File Name:** `Dragon_NoQuest.asset`
**Purpose:** When all quests are complete or on cooldown

### Settings
- **Start Node ID:** `start`
- **Is Repeatable:** Yes

### Dialogue Tree

#### Node: start
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
*The dragon lounges contentedly on their treasure hoard*

Hello, friend! Just taking some time to admire my beautiful collection. ✨
```
- **Next Node:** `chat`

#### Node: chat
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
I don't have any new requests right now, but thank you for all your help!
Would you like to chat for a bit?
```
- **Choices:**
  1. "Tell me about your collection" → `collection`
  2. "How did you become a dragon?" → `backstory`
  3. "See you later!" → `goodbye`

#### Node: collection
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
I've been collecting treasures for centuries! Each piece has a story...
This crystal here reminds me of the first winter snow, and this gem glows
just like the stars on a clear night. ✨

Treasures aren't just shiny objects - they're memories!
```
- **Next Node:** `chat`

#### Node: backstory
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
*The dragon chuckles warmly*

Dragons aren't born, we're awakened! I started as a small magical stone,
gathering magic and memories over hundreds of years, until one day...
I opened my eyes and had wings! 🐉

That's why I love crystals and gems so much - they remind me of where I came from.
```
- **Next Node:** `chat`

#### Node: goodbye
- **Speaker Name:** "Sparkle the Dragon"
- **Dialogue Text:**
```
Safe travels, dear friend! Come visit anytime! ✨
```
- **Is End Node:** Yes

---

## Implementation Guide

### Step 1: Create Dialogue Assets (30 min)

1. In Unity, right-click in Project window → Create → Cozy Game → Dialogue → Dialogue Tree
2. Create 5 dialogue assets:
   - `Dragon_Greeting.asset`
   - `Dragon_QuestAvailable.asset`
   - `Dragon_QuestInProgress.asset`
   - `Dragon_QuestComplete.asset`
   - `Dragon_NoQuest.asset`

### Step 2: Configure Each Dialogue (1 hour)

For each dialogue asset:
1. Set the **Dialogue Name** and **Description**
2. Set **Start Node ID** (usually "start")
3. Configure **Is Repeatable** and **Cooldown** settings
4. Add nodes from the dialogue trees above

### Step 3: Create Dragon GameObject (15 min)

1. Create a 3D GameObject for the dragon
2. Add `DragonNPC.cs` component
3. Add `NPCNameplate.cs` component
4. Assign the 5 dialogue assets:
   - Default Dialogue: `Dragon_Greeting`
   - New Quest Dialogue: `Dragon_QuestAvailable`
   - Quest In Progress Dialogue: `Dragon_QuestInProgress`
   - Quest Complete Dialogue: `Dragon_QuestComplete`
   - No Quest Dialogue: `Dragon_NoQuest`

### Step 4: Assign Quests (5 min)

1. In Dragon NPC component, expand "Available Quests"
2. Add the 3 quest assets created earlier:
   - `Quest_ShinyThings.asset`
   - `Quest_MoonlitMedicine.asset`
   - `Quest_CrystalGarden.asset`

### Step 5: Add Audio & VFX (10 min)

1. Assign audio clips:
   - Greeting Sound
   - Happy Sound (quest completion)
   - Disappointed Sound (quest not ready)
2. Assign particle systems:
   - Happy Particles
   - Quest Available Glow

### Total Time: ~2 hours

---

## Testing Checklist

- [ ] Dragon greets player with intro dialogue on first interaction
- [ ] Dragon offers quests when available
- [ ] Dragon tracks quest progress correctly
- [ ] Dragon celebrates when quest is turned in
- [ ] Dragon gives appropriate rewards
- [ ] Quest indicator shows correct state (!, ?, gray !)
- [ ] All dialogue choices work correctly
- [ ] Audio plays at appropriate times
- [ ] Animations trigger correctly
- [ ] Dialogue cooldowns work

---

## Tips

- **Dynamic Text:** Replace `[QUEST_ITEM]`, `[QUEST_AMOUNT]`, etc. with actual values in code
- **Voice Lines:** Record short dragon voice clips for immersion
- **Portraits:** Create a cute dragon portrait sprite for dialogue UI
- **Personality:** The dragon is friendly, excitable, and loves sparkly things!
