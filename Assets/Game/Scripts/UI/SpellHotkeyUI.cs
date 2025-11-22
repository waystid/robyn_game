using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CozyGame.Magic;

namespace CozyGame.UI
{
    /// <summary>
    /// Individual spell hotkey slot UI.
    /// Displays spell icon, cooldown, and hotkey number.
    /// </summary>
    public class SpellHotkeySlot : MonoBehaviour
    {
        [Header("UI Elements")]
        [Tooltip("Spell icon image")]
        public Image spellIcon;

        [Tooltip("Cooldown overlay image")]
        public Image cooldownOverlay;

        [Tooltip("Hotkey text (1-5)")]
        public TextMeshProUGUI hotkeyText;

        [Tooltip("Spell name text")]
        public TextMeshProUGUI spellNameText;

        [Tooltip("Cooldown timer text")]
        public TextMeshProUGUI cooldownTimerText;

        [Tooltip("Mana cost text")]
        public TextMeshProUGUI manaCostText;

        [Header("Colors")]
        [Tooltip("Available spell color")]
        public Color availableColor = Color.white;

        [Tooltip("Cooldown color")]
        public Color cooldownColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);

        [Tooltip("Not enough mana color")]
        public Color noManaColor = new Color(1f, 0.3f, 0.3f, 0.8f);

        private SpellData assignedSpell;
        private int hotkeyNumber;

        /// <summary>
        /// Assign spell to this slot
        /// </summary>
        public void AssignSpell(SpellData spell, int hotkey)
        {
            assignedSpell = spell;
            hotkeyNumber = hotkey;

            if (spell != null)
            {
                // Update UI
                if (spellIcon != null && spell.spellEffectPrefab != null)
                {
                    // TODO: Extract icon from prefab or use spell color
                    spellIcon.color = spell.spellColor;
                    spellIcon.enabled = true;
                }

                if (spellNameText != null)
                {
                    spellNameText.text = spell.spellName;
                }

                if (manaCostText != null)
                {
                    manaCostText.text = $"{spell.manaCost} MP";
                }
            }
            else
            {
                // Clear slot
                if (spellIcon != null)
                {
                    spellIcon.enabled = false;
                }

                if (spellNameText != null)
                {
                    spellNameText.text = "";
                }

                if (manaCostText != null)
                {
                    manaCostText.text = "";
                }
            }

            if (hotkeyText != null)
            {
                hotkeyText.text = hotkey.ToString();
            }
        }

        /// <summary>
        /// Update slot display based on spell state
        /// </summary>
        public void UpdateSlot()
        {
            if (assignedSpell == null)
                return;

            float cooldownRemaining = assignedSpell.GetRemainingCooldown();
            bool isOnCooldown = cooldownRemaining > 0f;
            bool hasEnoughMana = MagicSystem.Instance != null &&
                                 MagicSystem.Instance.currentMana >= assignedSpell.manaCost;

            // Update cooldown overlay
            if (cooldownOverlay != null)
            {
                if (isOnCooldown)
                {
                    float cooldownProgress = cooldownRemaining / assignedSpell.cooldown;
                    cooldownOverlay.fillAmount = cooldownProgress;
                    cooldownOverlay.enabled = true;
                    cooldownOverlay.color = cooldownColor;
                }
                else if (!hasEnoughMana)
                {
                    cooldownOverlay.fillAmount = 1f;
                    cooldownOverlay.enabled = true;
                    cooldownOverlay.color = noManaColor;
                }
                else
                {
                    cooldownOverlay.enabled = false;
                }
            }

            // Update cooldown timer text
            if (cooldownTimerText != null)
            {
                if (isOnCooldown)
                {
                    cooldownTimerText.text = $"{cooldownRemaining:F1}s";
                    cooldownTimerText.enabled = true;
                }
                else
                {
                    cooldownTimerText.enabled = false;
                }
            }

            // Update icon color
            if (spellIcon != null)
            {
                if (!hasEnoughMana)
                {
                    spellIcon.color = noManaColor;
                }
                else if (isOnCooldown)
                {
                    spellIcon.color = cooldownColor;
                }
                else
                {
                    spellIcon.color = availableColor;
                }
            }
        }

        /// <summary>
        /// Get assigned spell
        /// </summary>
        public SpellData GetAssignedSpell()
        {
            return assignedSpell;
        }

        /// <summary>
        /// Get hotkey number
        /// </summary>
        public int GetHotkeyNumber()
        {
            return hotkeyNumber;
        }
    }

    /// <summary>
    /// Spell hotkey UI manager.
    /// Manages 1-5 spell hotkey slots with input handling and cooldown visualization.
    /// </summary>
    public class SpellHotkeyUI : MonoBehaviour
    {
        public static SpellHotkeyUI Instance { get; private set; }

        [Header("UI References")]
        [Tooltip("Spell hotkey slot prefab")]
        public GameObject slotPrefab;

        [Tooltip("Container for slots")]
        public Transform slotContainer;

        [Tooltip("Number of hotkey slots (1-5)")]
        [Range(1, 5)]
        public int numberOfSlots = 5;

        [Header("Spell Assignments")]
        [Tooltip("Spells assigned to hotkeys 1-5")]
        public SpellData[] assignedSpells = new SpellData[5];

        [Header("Input")]
        [Tooltip("Enable hotkey input (1-5 keys)")]
        public bool enableHotkeyInput = true;

        [Tooltip("Require key hold for spell cast")]
        public bool requireKeyHold = false;

        private List<SpellHotkeySlot> slots = new List<SpellHotkeySlot>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            InitializeSlots();
        }

        private void Update()
        {
            // Update all slot displays
            foreach (var slot in slots)
            {
                slot.UpdateSlot();
            }

            // Handle hotkey input
            if (enableHotkeyInput)
            {
                HandleHotkeyInput();
            }
        }

        /// <summary>
        /// Initialize hotkey slots
        /// </summary>
        private void InitializeSlots()
        {
            // Clear existing slots
            foreach (var slot in slots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            slots.Clear();

            // Create slots
            for (int i = 0; i < numberOfSlots; i++)
            {
                CreateSlot(i);
            }
        }

        /// <summary>
        /// Create a hotkey slot
        /// </summary>
        private void CreateSlot(int index)
        {
            GameObject slotObj = null;

            if (slotPrefab != null && slotContainer != null)
            {
                slotObj = Instantiate(slotPrefab, slotContainer);
            }
            else
            {
                // Create default slot
                slotObj = CreateDefaultSlot();
                if (slotContainer != null)
                {
                    slotObj.transform.SetParent(slotContainer);
                }
            }

            SpellHotkeySlot slot = slotObj.GetComponent<SpellHotkeySlot>();
            if (slot == null)
            {
                slot = slotObj.AddComponent<SpellHotkeySlot>();
            }

            // Assign spell if available
            SpellData spell = index < assignedSpells.Length ? assignedSpells[index] : null;
            slot.AssignSpell(spell, index + 1);

            slots.Add(slot);
        }

        /// <summary>
        /// Create default slot UI (fallback)
        /// </summary>
        private GameObject CreateDefaultSlot()
        {
            GameObject slotObj = new GameObject("Spell Slot");

            // Add image component
            Image bg = slotObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Size
            RectTransform rect = slotObj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(64, 64);

            return slotObj;
        }

        /// <summary>
        /// Handle hotkey input (1-5 keys)
        /// </summary>
        private void HandleHotkeyInput()
        {
            for (int i = 0; i < numberOfSlots && i < slots.Count; i++)
            {
                KeyCode key = KeyCode.Alpha1 + i;

                bool shouldCast = requireKeyHold ? Input.GetKey(key) : Input.GetKeyDown(key);

                if (shouldCast)
                {
                    CastSpellByHotkey(i + 1);
                }
            }
        }

        /// <summary>
        /// Cast spell by hotkey number (1-5)
        /// </summary>
        public void CastSpellByHotkey(int hotkeyNumber)
        {
            int index = hotkeyNumber - 1;

            if (index < 0 || index >= slots.Count)
            {
                Debug.LogWarning($"Invalid hotkey number: {hotkeyNumber}");
                return;
            }

            SpellHotkeySlot slot = slots[index];
            SpellData spell = slot.GetAssignedSpell();

            if (spell == null)
            {
                Debug.Log($"No spell assigned to hotkey {hotkeyNumber}");
                return;
            }

            // Check cooldown
            if (!spell.IsOffCooldown())
            {
                Debug.Log($"{spell.spellName} is on cooldown!");
                return;
            }

            // Check mana
            if (MagicSystem.Instance != null && MagicSystem.Instance.currentMana < spell.manaCost)
            {
                Debug.Log($"Not enough mana for {spell.spellName}!");
                return;
            }

            // Cast spell through magic system
            if (MagicSystem.Instance != null)
            {
                Vector3 targetPosition = GetTargetPosition();
                MagicSystem.Instance.CastSpell(spell, targetPosition);
                Debug.Log($"Cast {spell.spellName} via hotkey {hotkeyNumber}");
            }
        }

        /// <summary>
        /// Get target position from mouse/input
        /// </summary>
        private Vector3 GetTargetPosition()
        {
            if (InputManager.Instance != null)
            {
                return InputManager.Instance.GetPointerWorldPosition();
            }

            // Fallback: use mouse raycast
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                return hit.point;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Assign spell to hotkey slot
        /// </summary>
        public void AssignSpellToSlot(int slotIndex, SpellData spell)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count)
            {
                Debug.LogWarning($"Invalid slot index: {slotIndex}");
                return;
            }

            // Update array
            if (slotIndex < assignedSpells.Length)
            {
                assignedSpells[slotIndex] = spell;
            }

            // Update slot
            slots[slotIndex].AssignSpell(spell, slotIndex + 1);

            Debug.Log($"Assigned {(spell != null ? spell.spellName : "empty")} to slot {slotIndex + 1}");
        }

        /// <summary>
        /// Clear all spell assignments
        /// </summary>
        public void ClearAllSlots()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                AssignSpellToSlot(i, null);
            }
        }

        /// <summary>
        /// Get spell in slot
        /// </summary>
        public SpellData GetSpellInSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count)
                return null;

            return slots[slotIndex].GetAssignedSpell();
        }

        /// <summary>
        /// Refresh all slots
        /// </summary>
        public void RefreshSlots()
        {
            InitializeSlots();
        }
    }
}
