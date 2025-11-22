using System.Collections.Generic;
using UnityEngine;

namespace CozyGame.Dialogue
{
    /// <summary>
    /// ScriptableObject containing a complete dialogue tree for an NPC.
    /// Can be used for quest dialogues, riddle conversations, or general chat.
    /// </summary>
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Cozy Game/Dialogue/Dialogue Tree", order = 1)]
    public class DialogueData : ScriptableObject
    {
        [Header("Dialogue Info")]
        [Tooltip("Name of this dialogue (for editor reference)")]
        public string dialogueName = "My Dialogue";

        [Tooltip("Description of when this dialogue is used")]
        [TextArea(2, 4)]
        public string description = "Describe when this dialogue plays...";

        [Header("Nodes")]
        [Tooltip("All dialogue nodes in this tree")]
        public List<DialogueNode> nodes = new List<DialogueNode>();

        [Tooltip("ID of the starting node (usually 'start' or 'node_001')")]
        public string startNodeID = "start";

        [Header("Settings")]
        [Tooltip("Can this dialogue be replayed?")]
        public bool isRepeatable = true;

        [Tooltip("Cooldown in minutes before dialogue can be replayed")]
        public float cooldownMinutes = 0f;

        [Tooltip("Play this dialogue once per game session only")]
        public bool oncePerSession = false;

        [Header("Audio")]
        [Tooltip("Background music to play during this dialogue")]
        public AudioClip backgroundMusic;

        [Tooltip("Ambient sound during dialogue")]
        public AudioClip ambientSound;

        [Header("Camera")]
        [Tooltip("Camera position preset for this dialogue (close-up, wide, etc.)")]
        public DialogueCameraMode cameraMode = DialogueCameraMode.Default;

        [Tooltip("Custom camera zoom distance (if using Custom mode)")]
        public float customCameraDistance = 3f;

        // Runtime tracking
        [System.NonSerialized]
        private float lastPlayedTime = -999999f;

        [System.NonSerialized]
        private bool playedThisSession = false;

        /// <summary>
        /// Get a specific node by ID
        /// </summary>
        public DialogueNode GetNode(string nodeID)
        {
            foreach (DialogueNode node in nodes)
            {
                if (node.nodeID == nodeID)
                    return node;
            }

            Debug.LogWarning($"[DialogueData] Node '{nodeID}' not found in dialogue '{dialogueName}'");
            return null;
        }

        /// <summary>
        /// Get the starting node for this dialogue
        /// </summary>
        public DialogueNode GetStartNode()
        {
            return GetNode(startNodeID);
        }

        /// <summary>
        /// Check if this dialogue is available to play
        /// </summary>
        public bool IsAvailable()
        {
            // Check if already played this session
            if (oncePerSession && playedThisSession)
                return false;

            // Check cooldown
            if (!isRepeatable && lastPlayedTime > 0)
                return false;

            if (cooldownMinutes > 0)
            {
                float timeSinceLastPlay = Time.time - lastPlayedTime;
                float cooldownSeconds = cooldownMinutes * 60f;

                if (timeSinceLastPlay < cooldownSeconds)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get remaining cooldown time in seconds
        /// </summary>
        public float GetRemainingCooldown()
        {
            if (cooldownMinutes <= 0)
                return 0f;

            float timeSinceLastPlay = Time.time - lastPlayedTime;
            float cooldownSeconds = cooldownMinutes * 60f;

            return Mathf.Max(0f, cooldownSeconds - timeSinceLastPlay);
        }

        /// <summary>
        /// Mark this dialogue as played
        /// </summary>
        public void MarkAsPlayed()
        {
            lastPlayedTime = Time.time;
            playedThisSession = true;
        }

        /// <summary>
        /// Reset the dialogue (for testing or special events)
        /// </summary>
        public void Reset()
        {
            lastPlayedTime = -999999f;
            playedThisSession = false;
        }

        /// <summary>
        /// Validate dialogue tree structure (call from editor)
        /// </summary>
        public List<string> ValidateDialogue()
        {
            List<string> errors = new List<string>();

            // Check if start node exists
            if (GetStartNode() == null)
            {
                errors.Add($"Start node '{startNodeID}' not found!");
            }

            // Check for duplicate node IDs
            HashSet<string> nodeIDs = new HashSet<string>();
            foreach (DialogueNode node in nodes)
            {
                if (nodeIDs.Contains(node.nodeID))
                {
                    errors.Add($"Duplicate node ID: '{node.nodeID}'");
                }
                else
                {
                    nodeIDs.Add(node.nodeID);
                }
            }

            // Check if all referenced nodes exist
            foreach (DialogueNode node in nodes)
            {
                // Check next node
                if (!string.IsNullOrEmpty(node.nextNodeID))
                {
                    if (GetNode(node.nextNodeID) == null)
                    {
                        errors.Add($"Node '{node.nodeID}' references missing node '{node.nextNodeID}'");
                    }
                }

                // Check choice targets
                foreach (DialogueChoice choice in node.choices)
                {
                    if (GetNode(choice.targetNodeID) == null)
                    {
                        errors.Add($"Node '{node.nodeID}' choice '{choice.choiceText}' references missing node '{choice.targetNodeID}'");
                    }
                }
            }

            // Check for unreachable nodes (nodes not referenced by any other node)
            HashSet<string> reachableNodes = new HashSet<string>();
            reachableNodes.Add(startNodeID);

            bool foundNewNodes = true;
            while (foundNewNodes)
            {
                foundNewNodes = false;
                foreach (DialogueNode node in nodes)
                {
                    if (!reachableNodes.Contains(node.nodeID))
                        continue;

                    // Add next node
                    if (!string.IsNullOrEmpty(node.nextNodeID) && !reachableNodes.Contains(node.nextNodeID))
                    {
                        reachableNodes.Add(node.nextNodeID);
                        foundNewNodes = true;
                    }

                    // Add choice targets
                    foreach (DialogueChoice choice in node.choices)
                    {
                        if (!reachableNodes.Contains(choice.targetNodeID))
                        {
                            reachableNodes.Add(choice.targetNodeID);
                            foundNewNodes = true;
                        }
                    }
                }
            }

            // Report unreachable nodes
            foreach (DialogueNode node in nodes)
            {
                if (!reachableNodes.Contains(node.nodeID))
                {
                    errors.Add($"Node '{node.nodeID}' is unreachable from start node");
                }
            }

            return errors;
        }
    }

    /// <summary>
    /// Camera positioning modes for dialogue
    /// </summary>
    public enum DialogueCameraMode
    {
        Default,      // No camera change
        CloseUp,      // Close to NPC face
        MediumShot,   // Waist-up view
        WideShot,     // Full body + environment
        Custom        // Use customCameraDistance
    }
}
