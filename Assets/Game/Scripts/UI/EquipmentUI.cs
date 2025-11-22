using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using CozyGame.Inventory;

namespace CozyGame.UI
{
    /// <summary>
    /// Equipment panel UI controller.
    /// Shows equipped items in all equipment slots.
    /// </summary>
    public class EquipmentUI : MonoBehaviour
    {
        [System.Serializable]
        public class EquipmentSlotUI
        {
            public EquipmentSlot slotType;
            public Image itemIcon;
            public GameObject emptyIndicator;
            public Button unequipButton;
        }

        [Header("Equipment Slots")]
        public EquipmentSlotUI[] equipmentSlots;

        [Header("Stats Display")]
        public TextMeshProUGUI totalArmorText;
        public TextMeshProUGUI totalDamageText;

        [Header("Panel")]
        public GameObject equipmentPanel;

        private void Start()
        {
            // Setup unequip buttons
            foreach (var slot in equipmentSlots)
            {
                if (slot.unequipButton != null)
                {
                    EquipmentSlot slotType = slot.slotType;
                    slot.unequipButton.onClick.AddListener(() => UnequipSlot(slotType));
                }
            }

            // Subscribe to equipment events
            if (EquipmentSystem.Instance != null)
            {
                EquipmentSystem.Instance.OnEquipmentChanged.AddListener(RefreshDisplay);
            }

            RefreshDisplay();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (EquipmentSystem.Instance != null)
            {
                EquipmentSystem.Instance.OnEquipmentChanged.RemoveListener(RefreshDisplay);
            }
        }

        /// <summary>
        /// Refresh all equipment slots display
        /// </summary>
        public void RefreshDisplay()
        {
            if (EquipmentSystem.Instance == null)
                return;

            foreach (var slotUI in equipmentSlots)
            {
                Item equippedItem = EquipmentSystem.Instance.GetEquippedItem(slotUI.slotType);
                bool hasItem = (equippedItem != null);

                // Update icon
                if (slotUI.itemIcon != null)
                {
                    slotUI.itemIcon.enabled = hasItem;
                    if (hasItem && equippedItem.icon != null)
                    {
                        slotUI.itemIcon.sprite = equippedItem.icon;
                    }
                }

                // Update empty indicator
                if (slotUI.emptyIndicator != null)
                {
                    slotUI.emptyIndicator.SetActive(!hasItem);
                }

                // Update unequip button
                if (slotUI.unequipButton != null)
                {
                    slotUI.unequipButton.gameObject.SetActive(hasItem);
                }
            }

            // Update stats
            if (totalArmorText != null)
            {
                int armor = EquipmentSystem.Instance.GetTotalArmor();
                totalArmorText.text = $"Armor: {armor}";
            }

            if (totalDamageText != null)
            {
                int damage = EquipmentSystem.Instance.GetTotalDamage();
                totalDamageText.text = $"Damage: {damage}";
            }
        }

        /// <summary>
        /// Unequip item from slot
        /// </summary>
        private void UnequipSlot(EquipmentSlot slot)
        {
            if (EquipmentSystem.Instance != null)
            {
                EquipmentSystem.Instance.UnequipItem(slot);
            }
        }

        /// <summary>
        /// Show equipment panel
        /// </summary>
        public void Show()
        {
            if (equipmentPanel != null)
            {
                equipmentPanel.SetActive(true);
                RefreshDisplay();
            }
        }

        /// <summary>
        /// Hide equipment panel
        /// </summary>
        public void Hide()
        {
            if (equipmentPanel != null)
            {
                equipmentPanel.SetActive(false);
            }
        }
    }
}
