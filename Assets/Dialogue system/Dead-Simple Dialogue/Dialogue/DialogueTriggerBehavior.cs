using UnityEngine;
using Dossamer.Dialogue.Schema;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace Dossamer.Dialogue
{
    public class DialogueTriggerBehavior : MonoBehaviour
    {
        public Cutscene dialogueToTrigger;
        [SerializeField]
        bool _onlyTriggerOnce = true;

        // Static dictionary to track triggered dialogues across scene reloads
        private static Dictionary<string, bool> triggeredDialogues = new Dictionary<string, bool>();

        // Unique identifier for this trigger
        private string triggerID;

        void Awake()
        {
            // Create unique ID based on scene and object
            triggerID = SceneManager.GetActiveScene().name + "_" + gameObject.name;

            // Subscribe to scene change event to clear tracking
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Clear all triggered dialogues when changing to a different scene
            if (scene.name != SceneManager.GetActiveScene().name)
            {
                triggeredDialogues.Clear();
            }
        }

        public void TriggerDialogue()
        {
            // Check if this specific trigger was already triggered
            if (_onlyTriggerOnce && triggeredDialogues.ContainsKey(triggerID) && triggeredDialogues[triggerID])
            {
                Debug.Log("Dialogue already triggered, skipping");
                return;
            }

            Debug.Log("triggering dialogue");

            // Mark this trigger as triggered
            if (_onlyTriggerOnce)
            {
                triggeredDialogues[triggerID] = true;
            }

            DialogueManager.Instance.StartNewDialogue(dialogueToTrigger);
        }
    }
}