# 💻 Code Templates for Custom Scripts

## 🪄 Magic System Scripts

### ManaAttribute.cs
```csharp
using UnityEngine;
using SickscoreGames.HUDNavigationSystem;

// Extends Survival Engine's attribute system for mana regeneration
public class ManaAttribute : MonoBehaviour
{
    [Header("Mana Settings")]
    public float regenerationRate = 1f; // Mana per second
    public float regenerationDelay = 0f; // Delay before regeneration starts
    
    private AttributeData manaAttribute;
    private float currentMana;
    private float maxMana;
    private float timeSinceLastUse = 0f;
    
    void Start()
    {
        // Get mana attribute from Survival Engine's attribute system
        // This assumes Survival Engine has an AttributeManager
        // Adjust based on actual Survival Engine implementation
        InitializeMana();
    }
    
    void Update()
    {
        if (manaAttribute != null)
        {
            timeSinceLastUse += Time.deltaTime;
            
            // Start regenerating after delay
            if (timeSinceLastUse >= regenerationDelay && currentMana < maxMana)
            {
                currentMana += regenerationRate * Time.deltaTime;
                currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
                UpdateManaUI();
            }
        }
    }
    
    void InitializeMana()
    {
        // Initialize mana from Survival Engine's system
        // Implementation depends on Survival Engine's attribute system
    }
    
    public bool UseMana(float amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            timeSinceLastUse = 0f;
            UpdateManaUI();
            return true;
        }
        return false;
    }
    
    public float GetCurrentMana()
    {
        return currentMana;
    }
    
    void UpdateManaUI()
    {
        // Update UI if Survival Engine has mana display
        // Or implement custom UI update here
    }
}
```

### CastGrowthSpellAction.cs
```csharp
using UnityEngine;

// Custom action for casting growth spell on plants
public class CastGrowthSpellAction : SAction
{
    [Header("Spell Settings")]
    public float manaCost = 10f;
    public float range = 5f; // Range to target plants
    
    public override bool CanDoAction(PlayerCharacter character, Selectable select)
    {
        // Check if player has enough mana
        ManaAttribute mana = character.GetComponent<ManaAttribute>();
        if (mana == null || mana.GetCurrentMana() < manaCost)
        {
            return false;
        }
        
        // Check if target is a plant
        Plant plant = select.GetComponent<Plant>();
        if (plant == null)
        {
            return false;
        }
        
        // Check if plant is in range
        float distance = Vector3.Distance(character.transform.position, select.transform.position);
        if (distance > range)
        {
            return false;
        }
        
        return true;
    }
    
    public override void DoAction(PlayerCharacter character, Selectable select)
    {
        ManaAttribute mana = character.GetComponent<ManaAttribute>();
        if (mana == null || !mana.UseMana(manaCost))
        {
            Debug.Log("Not enough mana!");
            return;
        }
        
        Plant plant = select.GetComponent<Plant>();
        if (plant != null)
        {
            // Advance plant to next growth stage
            // This depends on Survival Engine's Plant implementation
            // Example: plant.GrowToNextStage();
            
            Debug.Log("Growth spell cast! Plant advanced to next stage.");
        }
    }
}
```

---

## 🐉 Dragon & Quest System Scripts

### QuestSystem.cs
```csharp
using UnityEngine;
using System.Collections.Generic;

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
    public bool isActive;
}

public class QuestSystem : MonoBehaviour
{
    [Header("Quest Settings")]
    public List<Quest> availableQuests = new List<Quest>();
    
    private Dictionary<string, Quest> activeQuests = new Dictionary<string, Quest>();
    
    public void StartQuest(Quest quest)
    {
        if (!activeQuests.ContainsKey(quest.questName))
        {
            quest.isActive = true;
            quest.isCompleted = false;
            activeQuests[quest.questName] = quest;
            Debug.Log($"Quest started: {quest.questName}");
        }
    }
    
    public bool CheckQuestCompletion(string questName, Inventory inventory)
    {
        if (!activeQuests.ContainsKey(questName))
        {
            return false;
        }
        
        Quest quest = activeQuests[questName];
        
        // Check if player has required items
        int itemCount = inventory.GetItemCount(quest.requiredItem);
        if (itemCount >= quest.requiredAmount)
        {
            return true;
        }
        
        return false;
    }
    
    public void CompleteQuest(string questName, Inventory inventory)
    {
        if (!activeQuests.ContainsKey(questName))
        {
            return;
        }
        
        Quest quest = activeQuests[questName];
        
        // Remove required items
        inventory.RemoveItem(quest.requiredItem, quest.requiredAmount);
        
        // Give reward
        inventory.AddItem(quest.rewardItem, quest.rewardAmount);
        
        quest.isCompleted = true;
        quest.isActive = false;
        
        Debug.Log($"Quest completed: {questName}! Received {quest.rewardAmount}x {quest.rewardItem.name}");
    }
    
    public Quest GetQuest(string questName)
    {
        if (activeQuests.ContainsKey(questName))
        {
            return activeQuests[questName];
        }
        return null;
    }
}
```

### DeliverItemsAction.cs
```csharp
using UnityEngine;

// Action for delivering items to NPCs (like Dragon)
public class DeliverItemsAction : SAction
{
    [Header("Quest Settings")]
    public Quest quest;
    
    public override bool CanDoAction(PlayerCharacter character, Selectable select)
    {
        if (quest == null || quest.isCompleted)
        {
            return false;
        }
        
        Inventory inventory = character.GetComponent<Inventory>();
        if (inventory == null)
        {
            return false;
        }
        
        QuestSystem questSystem = FindObjectOfType<QuestSystem>();
        if (questSystem == null)
        {
            return false;
        }
        
        // Check if quest is active and can be completed
        if (!quest.isActive)
        {
            // Start the quest
            return true;
        }
        
        // Check if player has required items
        return questSystem.CheckQuestCompletion(quest.questName, inventory);
    }
    
    public override void DoAction(PlayerCharacter character, Selectable select)
    {
        Inventory inventory = character.GetComponent<Inventory>();
        QuestSystem questSystem = FindObjectOfType<QuestSystem>();
        
        if (inventory == null || questSystem == null)
        {
            return;
        }
        
        if (!quest.isActive)
        {
            // Start the quest
            questSystem.StartQuest(quest);
            Debug.Log($"Quest started: {quest.description}");
        }
        else if (questSystem.CheckQuestCompletion(quest.questName, inventory))
        {
            // Complete the quest
            questSystem.CompleteQuest(quest.questName, inventory);
            Debug.Log($"Quest completed! Received reward.");
        }
        else
        {
            Debug.Log($"You need {quest.requiredAmount}x {quest.requiredItem.name} to complete this quest.");
        }
    }
}
```

---

## 🦁 Sphinx & Riddle System Scripts

### RiddleData.cs
```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "New Riddle", menuName = "Game/Riddle Data")]
public class RiddleData : ScriptableObject
{
    [Header("Riddle")]
    public string question;
    
    [Header("Answers")]
    public string[] answers = new string[3]; // 3 answer choices
    
    [Header("Correct Answer")]
    [Range(0, 2)]
    public int correctAnswerIndex = 0;
    
    [Header("Reward")]
    public ItemData rewardItem;
    public int rewardAmount = 1;
}
```

### AskRiddleAction.cs
```csharp
using UnityEngine;
using System.Collections.Generic;

// Action for Sphinx to ask riddles
public class AskRiddleAction : SAction
{
    [Header("Riddle Settings")]
    public List<RiddleData> riddles = new List<RiddleData>();
    public float cooldownTime = 300f; // 5 minutes cooldown
    
    private Dictionary<PlayerCharacter, float> lastRiddleTime = new Dictionary<PlayerCharacter, float>();
    private Dictionary<PlayerCharacter, RiddleData> currentRiddles = new Dictionary<PlayerCharacter, RiddleData>();
    
    public override bool CanDoAction(PlayerCharacter character, Selectable select)
    {
        // Check cooldown
        if (lastRiddleTime.ContainsKey(character))
        {
            float timeSinceLastRiddle = Time.time - lastRiddleTime[character];
            if (timeSinceLastRiddle < cooldownTime)
            {
                return false;
            }
        }
        
        return riddles.Count > 0;
    }
    
    public override void DoAction(PlayerCharacter character, Selectable select)
    {
        if (riddles.Count == 0)
        {
            return;
        }
        
        // Select random riddle
        RiddleData riddle = riddles[Random.Range(0, riddles.Count)];
        currentRiddles[character] = riddle;
        
        // Show riddle UI
        RiddleUI riddleUI = FindObjectOfType<RiddleUI>();
        if (riddleUI != null)
        {
            riddleUI.ShowRiddle(riddle, this, character);
        }
        else
        {
            Debug.LogWarning("RiddleUI not found! Create one in the scene.");
        }
    }
    
    public void OnRiddleAnswered(PlayerCharacter character, int selectedAnswer)
    {
        if (!currentRiddles.ContainsKey(character))
        {
            return;
        }
        
        RiddleData riddle = currentRiddles[character];
        lastRiddleTime[character] = Time.time;
        
        if (selectedAnswer == riddle.correctAnswerIndex)
        {
            // Correct answer - give reward
            Inventory inventory = character.GetComponent<Inventory>();
            if (inventory != null && riddle.rewardItem != null)
            {
                inventory.AddItem(riddle.rewardItem, riddle.rewardAmount);
                Debug.Log($"Correct! You received {riddle.rewardAmount}x {riddle.rewardItem.name}");
            }
        }
        else
        {
            Debug.Log("Incorrect answer. Try again later!");
        }
        
        currentRiddles.Remove(character);
    }
}
```

### RiddleUI.cs
```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// UI for displaying and answering riddles
public class RiddleUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject riddlePanel;
    public TextMeshProUGUI questionText;
    public Button[] answerButtons = new Button[3];
    public TextMeshProUGUI[] answerTexts = new TextMeshProUGUI[3];
    public Button closeButton;
    
    private RiddleData currentRiddle;
    private AskRiddleAction riddleAction;
    private PlayerCharacter currentPlayer;
    
    void Start()
    {
        // Hide panel initially
        if (riddlePanel != null)
        {
            riddlePanel.SetActive(false);
        }
        
        // Setup button listeners
        for (int i = 0; i < answerButtons.Length; i++)
        {
            int index = i; // Capture for closure
            answerButtons[i].onClick.AddListener(() => OnAnswerSelected(index));
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseRiddle);
        }
    }
    
    public void ShowRiddle(RiddleData riddle, AskRiddleAction action, PlayerCharacter player)
    {
        currentRiddle = riddle;
        riddleAction = action;
        currentPlayer = player;
        
        if (riddlePanel != null)
        {
            riddlePanel.SetActive(true);
        }
        
        // Set question text
        if (questionText != null)
        {
            questionText.text = riddle.question;
        }
        
        // Set answer texts
        for (int i = 0; i < answerTexts.Length && i < riddle.answers.Length; i++)
        {
            if (answerTexts[i] != null)
            {
                answerTexts[i].text = riddle.answers[i];
            }
        }
    }
    
    void OnAnswerSelected(int answerIndex)
    {
        if (riddleAction != null && currentPlayer != null)
        {
            riddleAction.OnRiddleAnswered(currentPlayer, answerIndex);
        }
        
        CloseRiddle();
    }
    
    void CloseRiddle()
    {
        if (riddlePanel != null)
        {
            riddlePanel.SetActive(false);
        }
        
        currentRiddle = null;
        riddleAction = null;
        currentPlayer = null;
    }
}
```

---

## 📝 Notes on Implementation

### Survival Engine Integration
These scripts assume certain Survival Engine components exist:
- `PlayerCharacter` - Main player controller
- `Selectable` - Component for interactable objects
- `SAction` - Base class for actions (check Survival Engine docs)
- `Inventory` - Inventory system
- `Plant` - Plant system
- `ItemData` - ScriptableObject for items

**Important:** Adjust these scripts based on your actual Survival Engine implementation. The class names and methods may differ.

### Testing Tips
1. Start with simple test cases
2. Add Debug.Log statements to track flow
3. Test each system independently before integrating
4. Use Unity's Inspector to verify ScriptableObject references

### Customization
- Adjust mana costs, regeneration rates
- Modify quest rewards and requirements
- Add more riddles to the list
- Customize UI appearance
- Add sound effects and animations

---

## 🔧 Quick Setup Checklist

When implementing each script:
- [ ] Create script file in appropriate folder
- [ ] Add to GameObject or create prefab
- [ ] Assign ScriptableObject references
- [ ] Test in Play mode
- [ ] Debug any errors
- [ ] Integrate with other systems

