using UnityEngine;
using UnityEngine.Events;
using CozyGame.Dialogue;
using CozyGame.UI;

namespace CozyGame
{
    /// <summary>
    /// Base class for all interactive NPCs.
    /// Handles player interaction detection and dialogue triggering.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class NPCInteractable : MonoBehaviour
    {
        [Header("NPC Info")]
        [Tooltip("Display name for this NPC")]
        public string npcName = "Friendly NPC";

        [Tooltip("Default dialogue to play when interacted with")]
        public DialogueData defaultDialogue;

        [Header("Interaction Settings")]
        [Tooltip("Interaction range (triggers when player is within this distance)")]
        public float interactionRange = 3f;

        [Tooltip("Interaction key (PC)")]
        public KeyCode interactionKey = KeyCode.E;

        [Tooltip("Layer mask for player detection")]
        public LayerMask playerLayer = 1; // Default layer

        [Tooltip("Require player to look at NPC to interact")]
        public bool requireLookAt = false;

        [Tooltip("Max angle for look-at requirement (degrees)")]
        public float lookAtAngle = 45f;

        [Header("References")]
        [Tooltip("Nameplate UI component")]
        public NPCNameplate nameplate;

        [Tooltip("Animator for this NPC")]
        public Animator npcAnimator;

        [Header("Animation Triggers")]
        [Tooltip("Animation trigger when player starts talking")]
        public string talkAnimationTrigger = "Talk";

        [Tooltip("Animation trigger when conversation ends")]
        public string idleAnimationTrigger = "Idle";

        // Events
        public UnityEvent OnInteracted;
        public UnityEvent OnPlayerEnterRange;
        public UnityEvent OnPlayerExitRange;

        // State
        protected Transform playerTransform;
        protected bool isPlayerInRange = false;
        protected bool isInteracting = false;

        protected virtual void Start()
        {
            // Find player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }

            // Setup nameplate
            if (nameplate != null)
            {
                nameplate.SetName(npcName);
            }

            // Subscribe to dialogue events
            DialogueManager.OnDialogueStarted += HandleDialogueStarted;
            DialogueManager.OnDialogueEnded += HandleDialogueEnded;
        }

        protected virtual void OnDestroy()
        {
            // Unsubscribe from dialogue events
            DialogueManager.OnDialogueStarted -= HandleDialogueStarted;
            DialogueManager.OnDialogueEnded -= HandleDialogueEnded;
        }

        protected virtual void Update()
        {
            if (playerTransform == null)
                return;

            // Check if player is in range
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool inRange = distance <= interactionRange;

            if (inRange != isPlayerInRange)
            {
                isPlayerInRange = inRange;

                if (isPlayerInRange)
                {
                    OnPlayerEnterRange?.Invoke();
                }
                else
                {
                    OnPlayerExitRange?.Invoke();
                }
            }

            // Check for interaction input
            if (isPlayerInRange && !isInteracting)
            {
                bool interactPressed = false;

                // Check PC input
                if (Input.GetKeyDown(interactionKey))
                {
                    interactPressed = true;
                }

                // Check mobile input
                if (InputManager.Instance != null && InputManager.Instance.IsMobile())
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        // Check if tapping on NPC
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
                        {
                            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                            {
                                interactPressed = true;
                            }
                        }
                    }
                }

                if (interactPressed)
                {
                    // Check look-at requirement
                    if (requireLookAt && !IsPlayerLookingAt())
                    {
                        return;
                    }

                    Interact();
                }
            }
        }

        /// <summary>
        /// Called when player interacts with this NPC
        /// </summary>
        public virtual void Interact()
        {
            if (isInteracting)
                return;

            OnInteracted?.Invoke();

            // Start default dialogue if available
            if (defaultDialogue != null && DialogueManager.Instance != null)
            {
                bool started = DialogueManager.Instance.StartDialogue(defaultDialogue);
                if (started)
                {
                    isInteracting = true;
                    PlayTalkAnimation();
                }
            }
            else
            {
                Debug.LogWarning($"[NPCInteractable] {npcName} has no default dialogue assigned");
            }
        }

        /// <summary>
        /// Start a specific dialogue
        /// </summary>
        public virtual bool StartDialogue(DialogueData dialogue)
        {
            if (dialogue == null || DialogueManager.Instance == null)
                return false;

            bool started = DialogueManager.Instance.StartDialogue(dialogue);
            if (started)
            {
                isInteracting = true;
                PlayTalkAnimation();
            }

            return started;
        }

        /// <summary>
        /// Check if player is looking at this NPC
        /// </summary>
        protected bool IsPlayerLookingAt()
        {
            if (playerTransform == null || Camera.main == null)
                return false;

            Vector3 directionToNPC = (transform.position - Camera.main.transform.position).normalized;
            Vector3 cameraForward = Camera.main.transform.forward;

            float angle = Vector3.Angle(cameraForward, directionToNPC);
            return angle <= lookAtAngle;
        }

        /// <summary>
        /// Play talk animation
        /// </summary>
        protected void PlayTalkAnimation()
        {
            if (npcAnimator != null && !string.IsNullOrEmpty(talkAnimationTrigger))
            {
                npcAnimator.SetTrigger(talkAnimationTrigger);
            }
        }

        /// <summary>
        /// Play idle animation
        /// </summary>
        protected void PlayIdleAnimation()
        {
            if (npcAnimator != null && !string.IsNullOrEmpty(idleAnimationTrigger))
            {
                npcAnimator.SetTrigger(idleAnimationTrigger);
            }
        }

        /// <summary>
        /// Called when any dialogue starts
        /// </summary>
        protected virtual void HandleDialogueStarted(DialogueData dialogue)
        {
            // Override in subclasses if needed
        }

        /// <summary>
        /// Called when any dialogue ends
        /// </summary>
        protected virtual void HandleDialogueEnded(DialogueData dialogue)
        {
            isInteracting = false;
            PlayIdleAnimation();
        }

        /// <summary>
        /// Draw interaction range gizmo
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);

            if (requireLookAt)
            {
                Gizmos.color = Color.blue;
                // Draw look-at cone (simplified as a line)
                Vector3 forward = transform.forward * interactionRange;
                Gizmos.DrawLine(transform.position, transform.position + forward);
            }
        }
    }
}
