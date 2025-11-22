using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CozyGame.Magic;
using CozyGame.Economy;
using CozyGame.Dialogue;

namespace CozyGame.NPCs
{
    /// <summary>
    /// Trainable spell data
    /// </summary>
    [System.Serializable]
    public class TrainableSpell
    {
        [Tooltip("Spell to teach")]
        public Spell spell;

        [Tooltip("Cost to learn this spell")]
        public int cost = 100;

        [Tooltip("Currency type")]
        public CurrencyType currencyType = CurrencyType.Gold;

        [Tooltip("Required player level")]
        public int requiredLevel = 1;

        [Tooltip("Required friendship level")]
        public int requiredFriendship = 0;

        [Tooltip("Required magic level")]
        public int requiredMagicLevel = 1;

        [Tooltip("Prerequisite spells (must know these first)")]
        public List<Spell> prerequisiteSpells = new List<Spell>();

        [Tooltip("Is this spell available for teaching?")]
        public bool isAvailable = true;

        [Tooltip("Custom unlock dialogue")]
        public DialogueData unlockDialogue;

        /// <summary>
        /// Check if player meets requirements
        /// </summary>
        public bool MeetsRequirements(out string reason)
        {
            reason = "";

            // Check player level
            if (PlayerStats.Instance != null && PlayerStats.Instance.level < requiredLevel)
            {
                reason = $"Requires player level {requiredLevel}";
                return false;
            }

            // Check magic level
            if (MagicSystem.Instance != null && MagicSystem.Instance.magicLevel < requiredMagicLevel)
            {
                reason = $"Requires magic level {requiredMagicLevel}";
                return false;
            }

            // Check prerequisites
            if (MagicSystem.Instance != null)
            {
                foreach (var prereq in prerequisiteSpells)
                {
                    if (!MagicSystem.Instance.KnowsSpell(prereq))
                    {
                        reason = $"Must learn {prereq.spellName} first";
                        return false;
                    }
                }
            }

            // Check friendship (done by caller since it needs NPC name)

            return true;
        }
    }

    /// <summary>
    /// Trainer NPC that teaches spells and abilities.
    /// Extends NPCInteractable with training functionality.
    /// </summary>
    public class TrainerNPC : NPCInteractable
    {
        [Header("Trainer Settings")]
        [Tooltip("Spells this trainer can teach")]
        public List<TrainableSpell> teachableSpells = new List<TrainableSpell>();

        [Tooltip("Trainer specialization (for display)")]
        public string specialization = "Magic Trainer";

        [Tooltip("Training greeting dialogue")]
        public DialogueData trainingGreeting;

        [Tooltip("Dialogue when requirements not met")]
        public DialogueData requirementsNotMetDialogue;

        [Tooltip("Dialogue when spell learned successfully")]
        public DialogueData spellLearnedDialogue;

        [Tooltip("Dialogue when already knows spell")]
        public DialogueData alreadyKnowsDialogue;

        [Header("Training Bonuses")]
        [Tooltip("Discount percentage per friendship level")]
        [Range(0f, 0.1f)]
        public float discountPerLevel = 0.03f; // 3% per level

        [Tooltip("Max discount percentage")]
        [Range(0f, 0.5f)]
        public float maxDiscount = 0.4f; // 40% max

        [Tooltip("Grant bonus spell power based on friendship")]
        public bool grantBonusSpellPower = true;

        [Tooltip("Spell power bonus per friendship level (%)")]
        [Range(0f, 0.05f)]
        public float spellPowerBonusPerLevel = 0.01f; // 1% per level

        // Events
        public UnityEvent<Spell> OnSpellTaught;
        public UnityEvent OnTrainingStarted;
        public UnityEvent OnTrainingEnded;

        /// <summary>
        /// Override interact to open training UI
        /// </summary>
        public override void Interact()
        {
            // Play greeting dialogue if available
            if (trainingGreeting != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(trainingGreeting);
            }

            // Open training UI
            OpenTraining();
        }

        /// <summary>
        /// Open training UI
        /// </summary>
        public virtual void OpenTraining()
        {
            if (UI.TrainingUI.Instance != null)
            {
                UI.TrainingUI.Instance.OpenTraining(this);
                OnTrainingStarted?.Invoke();
            }
            else
            {
                Debug.LogWarning("[TrainerNPC] TrainingUI not found!");
            }
        }

        /// <summary>
        /// Close training UI
        /// </summary>
        public virtual void CloseTraining()
        {
            if (UI.TrainingUI.Instance != null)
            {
                UI.TrainingUI.Instance.CloseTraining();
                OnTrainingEnded?.Invoke();
            }
        }

        /// <summary>
        /// Teach spell to player
        /// </summary>
        public bool TeachSpell(TrainableSpell trainableSpell)
        {
            if (trainableSpell == null || trainableSpell.spell == null)
            {
                Debug.LogWarning("[TrainerNPC] Invalid trainable spell!");
                return false;
            }

            if (MagicSystem.Instance == null)
            {
                Debug.LogWarning("[TrainerNPC] MagicSystem not found!");
                return false;
            }

            // Check if already knows spell
            if (MagicSystem.Instance.KnowsSpell(trainableSpell.spell))
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        "You already know this spell!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.yellow
                    );
                }

                if (alreadyKnowsDialogue != null && DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(alreadyKnowsDialogue);
                }

                return false;
            }

            // Check requirements
            string reason;
            if (!trainableSpell.MeetsRequirements(out reason))
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        reason,
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.red
                    );
                }

                if (requirementsNotMetDialogue != null && DialogueManager.Instance != null)
                {
                    DialogueManager.Instance.StartDialogue(requirementsNotMetDialogue);
                }

                return false;
            }

            // Check friendship requirement
            int friendshipLevel = 0;
            if (Social.RelationshipSystem.Instance != null)
            {
                Social.NPCRelationship relationship = Social.RelationshipSystem.Instance.GetRelationship(npcName);
                if (relationship != null)
                {
                    friendshipLevel = relationship.friendshipLevel;
                }

                if (friendshipLevel < trainableSpell.requiredFriendship)
                {
                    if (FloatingTextManager.Instance != null && Camera.main != null)
                    {
                        FloatingTextManager.Instance.Show(
                            $"Need friendship level {trainableSpell.requiredFriendship}!",
                            Camera.main.transform.position + Camera.main.transform.forward * 3f,
                            Color.red
                        );
                    }
                    return false;
                }
            }

            // Calculate cost with discount
            int finalCost = CalculateTrainingCost(trainableSpell);

            // Check if can afford
            if (!CurrencyManager.Instance.HasCurrency(trainableSpell.currencyType, finalCost))
            {
                if (FloatingTextManager.Instance != null && Camera.main != null)
                {
                    FloatingTextManager.Instance.Show(
                        $"Need {finalCost} {trainableSpell.currencyType}!",
                        Camera.main.transform.position + Camera.main.transform.forward * 3f,
                        Color.red
                    );
                }
                return false;
            }

            // Pay for training
            CurrencyManager.Instance.RemoveCurrency(trainableSpell.currencyType, finalCost);

            // Learn spell
            MagicSystem.Instance.LearnSpell(trainableSpell.spell);

            // Apply friendship bonus if enabled
            if (grantBonusSpellPower && Social.RelationshipSystem.Instance != null)
            {
                float powerBonus = friendshipLevel * spellPowerBonusPerLevel;
                // Note: Spell power bonus would need to be stored somewhere
                // For now, just log it
                Debug.Log($"[TrainerNPC] Granted {powerBonus * 100}% spell power bonus from friendship");
            }

            // Show success message
            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show(
                    $"Learned {trainableSpell.spell.spellName}!",
                    Camera.main.transform.position + Camera.main.transform.forward * 3f,
                    Color.cyan
                );
            }

            // Play learn sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound("spell_learn");
            }

            // Play custom unlock dialogue
            if (trainableSpell.unlockDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(trainableSpell.unlockDialogue);
            }
            else if (spellLearnedDialogue != null && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartDialogue(spellLearnedDialogue);
            }

            OnSpellTaught?.Invoke(trainableSpell.spell);

            // Increase friendship from training
            if (Social.RelationshipSystem.Instance != null)
            {
                Social.RelationshipSystem.Instance.ModifyFriendship(npcName, 5);
            }

            return true;
        }

        /// <summary>
        /// Calculate training cost with discounts
        /// </summary>
        public int CalculateTrainingCost(TrainableSpell trainableSpell)
        {
            float baseCost = trainableSpell.cost;

            // Apply friendship discount
            float discount = GetRelationshipDiscount();
            float finalCost = baseCost * (1f - discount);

            return Mathf.RoundToInt(finalCost);
        }

        /// <summary>
        /// Get relationship discount percentage
        /// </summary>
        public float GetRelationshipDiscount()
        {
            if (Social.RelationshipSystem.Instance == null)
                return 0f;

            Social.NPCRelationship relationship = Social.RelationshipSystem.Instance.GetRelationship(npcName);
            if (relationship == null)
                return 0f;

            float discount = relationship.friendshipLevel * discountPerLevel;
            return Mathf.Min(discount, maxDiscount);
        }

        /// <summary>
        /// Get available spells for current level and friendship
        /// </summary>
        public List<TrainableSpell> GetAvailableSpells()
        {
            List<TrainableSpell> available = new List<TrainableSpell>();

            int friendshipLevel = 0;
            if (Social.RelationshipSystem.Instance != null)
            {
                Social.NPCRelationship relationship = Social.RelationshipSystem.Instance.GetRelationship(npcName);
                if (relationship != null)
                {
                    friendshipLevel = relationship.friendshipLevel;
                }
            }

            foreach (var spell in teachableSpells)
            {
                if (spell.isAvailable && friendshipLevel >= spell.requiredFriendship)
                {
                    available.Add(spell);
                }
            }

            return available;
        }

        /// <summary>
        /// Get spells player can learn right now
        /// </summary>
        public List<TrainableSpell> GetLearnableSpells()
        {
            List<TrainableSpell> learnable = new List<TrainableSpell>();
            List<TrainableSpell> available = GetAvailableSpells();

            foreach (var spell in available)
            {
                // Skip if already knows
                if (MagicSystem.Instance != null && MagicSystem.Instance.KnowsSpell(spell.spell))
                    continue;

                // Check requirements
                string reason;
                if (spell.MeetsRequirements(out reason))
                {
                    learnable.Add(spell);
                }
            }

            return learnable;
        }
    }
}
