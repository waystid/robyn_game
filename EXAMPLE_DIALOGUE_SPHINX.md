# Example Dialogue: Sphinx NPC

This document provides complete dialogue tree configurations for the **Sphinx NPC** character.

The Sphinx is a mysterious riddle-master who tests the player's wit and rewards clever answers.

---

## Table of Contents

1. [Sphinx Greeting Dialogue](#1-sphinx-greeting-dialogue)
2. [Riddle Available Dialogue](#2-riddle-available-dialogue)
3. [Riddle Cooldown Dialogue](#3-riddle-cooldown-dialogue)
4. [All Riddles Complete Dialogue](#4-all-riddles-complete-dialogue)
5. [Correct Answer Response](#5-correct-answer-response)
6. [Wrong Answer Response](#6-wrong-answer-response)
7. [Implementation Guide](#implementation-guide)

---

## 1. Sphinx Greeting Dialogue

**Dialogue Name:** `Sphinx_Greeting`
**File Name:** `Sphinx_Greeting.asset`
**Purpose:** First-time introduction to the Sphinx

### Settings
- **Start Node ID:** `start`
- **Is Repeatable:** No (once per session)
- **Cooldown:** 0 minutes
- **Camera Mode:** Medium Shot

### Dialogue Tree

#### Node: start
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
*An ancient statue stirs to life, stone eyes opening with an ethereal glow*

Greetings, mortal. I am Mysterio, keeper of riddles and guardian of ancient wisdom.
```
- **Animation Trigger:** "Awaken"
- **Next Node:** `introduction`

#### Node: introduction
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
For countless ages, I have tested the minds of travelers who wander this forest.
Answer my riddles correctly, and I shall grant you rewards beyond mere gold...
```
- **Next Node:** `challenge`

#### Node: challenge
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
Do you dare test your wit against the riddles of the Sphinx?
```
- **Choices:**
  1. "I accept your challenge!" → `accept`
  2. "What kind of riddles?" → `explain_riddles`
  3. "I'm not ready yet" → `decline`

#### Node: accept
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
*The Sphinx's eyes glow brighter*

Excellent! A brave soul indeed. Think carefully before you answer...
for while there is no punishment for wrong answers, the truth reveals itself only once.

Are you ready for your first riddle?
```
- **Choices:**
  1. "Yes, give me a riddle!" → `start_riddle`
  2. "Wait, I need to prepare" → `goodbye_prep`

#### Node: explain_riddles
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
My riddles test logic, wordplay, and creative thinking.
Each riddle has four possible answers - but only one is correct.

Choose wisely, for you may only answer each riddle once per day.
Correct answers earn you magical rewards and ancient knowledge.
```
- **Next Node:** `challenge`

#### Node: start_riddle
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
Then let us begin...
```
- **Riddle To Present:** `riddle_day_and_night`
- **Auto Advance Delay:** 1 second
- **Is End Node:** Yes (riddle UI takes over)

#### Node: decline
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
Wisdom knows when the mind is not yet prepared.
Return when you are ready to exercise your intellect.
```
- **Is End Node:** Yes

#### Node: goodbye_prep
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
Take your time. The riddles have waited centuries - they can wait a bit longer.
```
- **Is End Node:** Yes

---

## 2. Riddle Available Dialogue

**Dialogue Name:** `Sphinx_RiddleAvailable`
**File Name:** `Sphinx_RiddleAvailable.asset`
**Purpose:** When sphinx has riddles ready to present

### Settings
- **Start Node ID:** `start`
- **Is Repeatable:** Yes
- **Cooldown:** 0 minutes

### Dialogue Tree

#### Node: start
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
*The stone guardian's eyes flicker with ancient intelligence*

Welcome back, seeker. Your mind hungers for another challenge, I see...
```
- **Next Node:** `offer_riddle`

#### Node: offer_riddle
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
I have a riddle prepared for you. Are you ready to test your wit once more?
```
- **Choices:**
  1. "Yes, I'm ready!" → `choose_difficulty`
  2. "How many riddles remain?" → `check_progress`
  3. "Not right now" → `decline`

#### Node: choose_difficulty
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
I sense you grow wiser with each visit. How difficult a riddle would you prefer?
```
- **Choices:**
  1. "An easy riddle to warm up" → `easy_riddle`
  2. "A moderately challenging riddle" → `medium_riddle`
  3. "Your hardest riddle!" → `hard_riddle`
  4. "Surprise me" → `random_riddle`

#### Node: easy_riddle
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
A gentle exercise for the mind, then...
```
- **Riddle To Present:** `riddle_day_and_night`
- **Auto Advance Delay:** 1 second
- **Is End Node:** Yes

#### Node: medium_riddle
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
A worthy challenge. Listen carefully...
```
- **Riddle To Present:** `riddle_hole`
- **Auto Advance Delay:** 1 second
- **Is End Node:** Yes

#### Node: hard_riddle
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
*The Sphinx's eyes gleam with approval*

Bold! Very well, ponder this deeply...
```
- **Riddle To Present:** `riddle_darkness`
- **Auto Advance Delay:** 1 second
- **Is End Node:** Yes

#### Node: random_riddle
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
Ah, you leave it to fate. How intriguing...
```
- **Custom Event:** `present_random_riddle`
- **Is End Node:** Yes

#### Node: check_progress
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
You have correctly answered [ANSWERED_COUNT] of my [TOTAL_COUNT] riddles.
[REMAINING_COUNT] riddles still await your clever mind.

Most impressive progress, seeker!
```
- **Next Node:** `offer_riddle`

#### Node: decline
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
Return when your mind is sharp and ready. The riddles are eternal.
```
- **Is End Node:** Yes

---

## 3. Riddle Cooldown Dialogue

**Dialogue Name:** `Sphinx_RiddleCooldown`
**File Name:** `Sphinx_RiddleCooldown.asset`
**Purpose:** When riddles are on cooldown

### Settings
- **Start Node ID:** `start`
- **Is Repeatable:** Yes

### Dialogue Tree

#### Node: start
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
*The Sphinx regards you with knowing eyes*

Ah, the eager student returns...
```
- **Next Node:** `cooldown_message`

#### Node: cooldown_message
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
However, you have already answered a riddle recently.
The ancient magic requires time to prepare the next challenge.

Return in [COOLDOWN_TIME] and I shall have another riddle ready for you.
```
- **Choices:**
  1. "Can I do anything while I wait?" → `suggestions`
  2. "Why the wait?" → `explain_cooldown`
  3. "I'll return later" → `goodbye`

#### Node: suggestions
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
Use this time to sharpen your mind with other challenges.
Perhaps the Dragon has quests? Or practice your magic?

A well-rounded intellect makes for better riddle-solving.
```
- **Next Node:** `goodbye`

#### Node: explain_cooldown
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
The riddles are bound by ancient enchantments - each answer resonates
through time and requires the magic to settle before the next can be presented.

This also prevents hasty guessing. Wisdom requires patience, young one.
```
- **Next Node:** `goodbye`

#### Node: goodbye
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
Until next time, seeker of wisdom.
```
- **Is End Node:** Yes

---

## 4. All Riddles Complete Dialogue

**Dialogue Name:** `Sphinx_AllComplete`
**File Name:** `Sphinx_AllComplete.asset`
**Purpose:** When all riddles have been answered correctly

### Settings
- **Start Node ID:** `start`
- **Is Repeatable:** Yes

### Dialogue Tree

#### Node: start
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
*The ancient guardian bows their head in respect*

Master of Riddles... you have answered all of my challenges correctly.
```
- **Animation Trigger:** "Bow"
- **Next Node:** `congratulations`

#### Node: congratulations
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
In all my centuries, few have proven so clever.
You have earned not just rewards, but my deepest respect.

The title of "Riddle Master" is yours to claim!
```
- **Item To Give:** `title_riddlemaster`
- **Item Quantity:** 1
- **Next Node:** `ongoing`

#### Node: ongoing
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
Though you have conquered all current riddles, our relationship need not end here.
Would you like to hear stories of ancient times? Or simply enjoy a philosophical discussion?
```
- **Choices:**
  1. "Tell me ancient stories" → `stories`
  2. "Let's discuss philosophy" → `philosophy`
  3. "Will there be more riddles?" → `future_riddles`
  4. "Thank you, goodbye!" → `farewell`

#### Node: stories
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
*The Sphinx settles into a comfortable position*

Very well... In ages past, this forest was home to a great civilization.
Magical beings of all kinds lived in harmony, sharing wisdom freely...

[The Sphinx tells a randomly selected story about the forest's history]
```
- **Custom Event:** `tell_random_story`
- **Auto Advance Delay:** 0
- **Next Node:** `ongoing`

#### Node: philosophy
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
An excellent choice! Tell me, what is the nature of wisdom?
Is it knowledge accumulated, or the understanding of one's own ignorance?

[A thoughtful dialogue about philosophy ensues]
```
- **Custom Event:** `philosophical_discussion`
- **Next Node:** `ongoing`

#### Node: future_riddles
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
Perhaps... The world is ever-changing, and new mysteries arise with time.
Check back with me after major events - I may have new riddles inspired by current happenings.

But for now, you have mastered all that I currently offer. Well done!
```
- **Next Node:** `ongoing`

#### Node: farewell
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
Farewell, Riddle Master. You are always welcome in my presence.
May wisdom guide your path. 🌟
```
- **Animation Trigger:** "Bow"
- **Is End Node:** Yes

---

## 5. Correct Answer Response

**Dialogue Name:** `Sphinx_CorrectAnswer`
**File Name:** `Sphinx_CorrectAnswer.asset`
**Purpose:** Response when player answers riddle correctly

### Settings
- **Start Node ID:** `start`
- **Is Repeatable:** Yes
- **Camera Mode:** Close Up

### Dialogue Tree

#### Node: start
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
*The Sphinx's eyes shine with approval*

Correct! Your mind is sharp indeed.
```
- **Animation Trigger:** "Approve"
- **Next Node:** `praise`

#### Node: praise
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
Few grasp the answer so clearly. You have earned your reward!
```
- **Item To Give:** `[RIDDLE_REWARD]`
- **Next Node:** `explanation`

#### Node: explanation
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
The answer is [CORRECT_ANSWER] because [RIDDLE_EXPLANATION].

An ancient truth, now part of your wisdom.
```
- **Choices:**
  1. "That was fun! Another riddle?" → `check_more`
  2. "Thank you for the lesson!" → `grateful_exit`

#### Node: check_more
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
Eager to learn! I appreciate that.
However, you must wait [COOLDOWN_TIME] before the next riddle is ready.

Return then, and we shall test your wit once more.
```
- **Is End Node:** Yes

#### Node: grateful_exit
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
You are most welcome, seeker. Until next time... 🔮
```
- **Is End Node:** Yes

---

## 6. Wrong Answer Response

**Dialogue Name:** `Sphinx_WrongAnswer`
**File Name:** `Sphinx_WrongAnswer.asset`
**Purpose:** Response when player answers riddle incorrectly

### Settings
- **Start Node ID:** `start`
- **Is Repeatable:** Yes

### Dialogue Tree

#### Node: start
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
*The Sphinx shakes their head slowly*

No... that is not the answer I seek.
```
- **Animation Trigger:** "Disappointed"
- **Next Node:** `feedback`

#### Node: feedback
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
The correct answer was: [CORRECT_ANSWER]

[RIDDLE_EXPLANATION]
```
- **Choices:**
  1. "Oh! I see it now..." → `understanding`
  2. "That's tricky!" → `acknowledge_difficulty`
  3. "I'll do better next time" → `determined_exit`

#### Node: understanding
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
Ah, yes! The light of comprehension dawns.
This is good - you learn from mistakes. That is wisdom in itself.

Though you did not answer correctly, I sense your mind growing sharper.
```
- **Item To Give:** `consolation_wisdom_orb`
- **Item Quantity:** 1
- **Next Node:** `goodbye`

#### Node: acknowledge_difficulty
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
*The Sphinx's expression softens slightly*

Indeed, the ancient riddles are meant to challenge even the cleverest minds.
Do not be discouraged - many have failed where you attempted.

Return and try another when you're ready.
```
- **Next Node:** `goodbye`

#### Node: determined_exit
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
That is the spirit! Determination is the foundation of wisdom.

Come back in [COOLDOWN_TIME] and I shall present another riddle.
Study well, and may your next answer be true!
```
- **Is End Node:** Yes

#### Node: goodbye
- **Speaker Name:** "Mysterio the Sphinx"
- **Dialogue Text:**
```
Until our next meeting, seeker. May you ponder deeply.
```
- **Is End Node:** Yes

---

## Implementation Guide

### Step 1: Create Dialogue Assets (20 min)

1. In Unity, right-click in Project window → Create → Cozy Game → Dialogue → Dialogue Tree
2. Create 6 dialogue assets:
   - `Sphinx_Greeting.asset`
   - `Sphinx_RiddleAvailable.asset`
   - `Sphinx_RiddleCooldown.asset`
   - `Sphinx_AllComplete.asset`
   - `Sphinx_CorrectAnswer.asset`
   - `Sphinx_WrongAnswer.asset`

### Step 2: Configure Each Dialogue (1.5 hours)

For each dialogue asset:
1. Set the **Dialogue Name** and **Description**
2. Set **Start Node ID** (usually "start")
3. Configure **Is Repeatable** and **Cooldown** settings
4. Add nodes from the dialogue trees above
5. Configure choices and branching

### Step 3: Create Sphinx GameObject (15 min)

1. Create a 3D GameObject for the sphinx (statue-like)
2. Add `SphinxNPC.cs` component
3. Add `NPCNameplate.cs` component
4. Assign the 6 dialogue assets:
   - Default Dialogue: `Sphinx_Greeting`
   - New Riddle Dialogue: `Sphinx_RiddleAvailable`
   - Riddle On Cooldown Dialogue: `Sphinx_RiddleCooldown`
   - All Riddles Complete Dialogue: `Sphinx_AllComplete`

### Step 4: Assign Riddles (5 min)

1. In Sphinx NPC component, expand "Available Riddles"
2. Add the 5 riddle assets created earlier:
   - `Riddle_DayAndNight.asset`
   - `Riddle_Footsteps.asset`
   - `Riddle_Hole.asset`
   - `Riddle_River.asset`
   - `Riddle_Darkness.asset`

### Step 5: Add Audio & VFX (10 min)

1. Assign audio clips:
   - Greeting Sound (mysterious tone)
   - Correct Answer Sound (magical chime)
   - Wrong Answer Sound (low hum)
2. Assign particle systems:
   - Correct Particles (sparkles, glowing effects)
   - Wrong Particles (dim flash)
   - Riddle Glow (mysterious aura when riddles available)

### Step 6: Link Response Dialogues to RiddleManager (5 min)

1. In RiddleManager component:
   - Assign `Sphinx_CorrectAnswer` to "On Correct Answer Dialogue"
   - Assign `Sphinx_WrongAnswer` to "On Wrong Answer Dialogue"

### Total Time: ~2.5 hours

---

## Testing Checklist

- [ ] Sphinx greets player properly on first interaction
- [ ] Sphinx presents riddles when available
- [ ] Riddle UI shows correctly with 4 answer choices
- [ ] Correct answers trigger celebration and rewards
- [ ] Wrong answers show explanation and consolation
- [ ] Cooldown prevents answering same riddle immediately
- [ ] All riddles complete triggers master dialogue
- [ ] Riddle glow effect shows when riddles available
- [ ] Audio plays at appropriate times
- [ ] Animations trigger correctly (awaken, bow, etc.)

---

## Tips

- **Dynamic Text:** Replace `[ANSWERED_COUNT]`, `[COOLDOWN_TIME]`, etc. with actual values
- **Voice Lines:** Use mysterious, wise-sounding voice clips
- **Portrait:** Create a mystical sphinx portrait (cat + wings + wise eyes)
- **Personality:** The Sphinx is wise, patient, philosophical, and respects cleverness
- **Visual Style:** Stone texture, glowing eyes, magical aura effects
