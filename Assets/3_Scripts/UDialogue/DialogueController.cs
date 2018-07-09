using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UBindings;

namespace UDialogue
{
	[System.Serializable]
	public class DialogueController
	{
		#region Fields

		private Dialogue dialogue = null;
		private IDialogueTrigger trigger = null;
		private IBindingCore bindingCore = null;

		private DialogueNode currentNode = null;
		private int currentContentIndex = 0;


		private static readonly DialogueResponse[] fallbackResponses = new DialogueResponse[1] { DialogueResponse.Blank };
		private static readonly DialogueResponse[] nextContentResponses = new DialogueResponse[1]
		{
			new DialogueResponse() { nextNode=null, responseText="...", conditions=DialogueConditions.None }
		};

		#endregion
		#region Properties

		public DialogueNode CurrentNode
		{
			get { return currentNode; }
		}

		#endregion
		#region Methods

		public bool loadDialogue(Dialogue asset, IDialogueTrigger inTrigger, IBindingCore inBindingCore)
		{
			// Verify parameters and log warnings as needed:
			if(asset == null)
			{
				Debug.LogError("[DialogueController] Error! Dialogue asset may not be null!");
				return false;
			}
			if(inBindingCore == null)
			{
				Debug.LogWarning("[DialogueController] Error! Null binding core may result in dialogue bindings not being resolved!");
			}

			// Instantiate a copy of the dialogue asset such as to not accidentially modify it runtime:
			dialogue = Object.Instantiate<Dialogue>(asset);
			trigger = inTrigger;
			bindingCore = inBindingCore;

			// Reset all flags, counters and references:
			reset();

			// Return success:
			return true;
		}

		private void reset()
		{
			currentNode = null;
			currentContentIndex = 0;
		}

		public DialogueContent getCurrentContent()
		{
			if (currentNode == null) return DialogueContent.Blank;

			currentContentIndex = Mathf.Clamp(currentContentIndex, 0, currentNode.content.Length);
			return currentNode.content[currentContentIndex];
		}
		public DialogueResponse[] getCurrentResponses()
		{
			if (currentNode == null) return fallbackResponses;

			// Only offer to proceed to the next content, if there are 1 or more content items remaining:
			if(currentContentIndex < currentNode.content.Length - 1)
			{
				return nextContentResponses;
			}
			// If there's only one content item or we've reached to final one, present actual response options:
			else
			{
				return currentNode.responses;
			}

			// TODO: Only return responses whose conditions have been met!!!
		}

		public bool startDialogue()
		{
			// NOTE: Returns false if the conditions for none of the root nodes were met!

			if (dialogue == null) return false;

			// Reset all flags, counters and references:
			reset();

			// Execute start binding right away:
			if(bindingCore != null && !string.IsNullOrEmpty(dialogue.startBinding.path))
			{
				BindingResult result = bindingCore.executeBinding(ref dialogue.startBinding);
				if (result.error != BindingError.Success)
				{
					Debug.LogError("[DialogueController] An error was encountered while trying to resolve the start binding!\nError type: " + result.error.ToString());
				}
			}

			DialogueRoot root = dialogue.rootNodes[0];
			for(int i = 0; i < dialogue.rootNodes.Length; ++i)
			{
				DialogueRoot curRoot = dialogue.rootNodes[i];
				// Ignore roots that (for whatever reason) don't have a node assigned:
				if(curRoot.node == null) continue;

				// Either chose a root with zero conditions:
				if(string.IsNullOrEmpty(curRoot.conditions.keyword))
				{
					root = curRoot;
					continue;
				}
				// Or pick a root where all conditions have been cleared:
				else if(trigger != null && trigger.checkDialogueCondition(ref curRoot.conditions))
				{
					root = curRoot;
					continue;
				}
				// NOTE: It's always the last matching root in the array that will be chosen as starting point!
			}

			bool started = selectNode(root.node);

			// Notify the dialogue trigger of the start of a new dialogue:
			if(started && trigger != null)
			{
				trigger.notifyDialogueEvent(DialogueEvent.Start);
			}
			return started;
		}

		public bool endDialogue(bool unloadDialogueAsset = false)
		{
			// Reset all flags, counters and references:
			reset();
			currentNode = null;
			bindingCore = null;

			if(dialogue == null)
			{
				Debug.LogError("[DialogueController] Error! Dialogue is null yet your are still trying to end it.");
				return false;
			}

			// Execute end binding right away:
			if (bindingCore != null && !string.IsNullOrEmpty(dialogue.endBinding.path))
			{
				BindingResult result = bindingCore.executeBinding(ref dialogue.endBinding);
				if (result.error != BindingError.Success)
				{
					Debug.LogError("[DialogueController] An error was encountered while trying to resolve the start binding!\nError type: " + result.error.ToString());
				}
			}

			// End the dialogue and unload asset as required:
			if(unloadDialogueAsset)
			{
				Dialogue prevDialogue = dialogue;
				dialogue = null;

				Object.Destroy(prevDialogue);
				Resources.UnloadAsset(prevDialogue);
			}

			// Notify the dialogue trigger of the end of a new dialogue:
			if (trigger != null)
			{
				trigger.notifyDialogueEvent(DialogueEvent.End);
			}

			return true;
		}

		public bool selectNode(DialogueNode newNode)
		{
			if(newNode == null)
			{
				Debug.LogError("[DialogueController] Error! Unable to select null dialogue node! Aborting selection.");
				return false;
			}
			if(newNode.content == null || newNode.content.Length == 0)
			{
				Debug.LogError("[DialogueController] Error! Dialogue nodes must have at least 1 content item! Aborting selection.");
				return false;
			}

			// Assign new node as current:
			currentNode = newNode;

			// Reset flags and indices:
			currentContentIndex = 0;

			// Execute the new node's first content's binding:
			if(bindingCore != null && !string.IsNullOrEmpty(currentNode.content[0].eventBinding.path))
			{
				BindingResult result = bindingCore.executeBinding(ref currentNode.content[0].eventBinding);
				if(result.error != BindingError.Success)
				{
					Debug.LogError("[DialogueController] An error was encountered while trying to resolve a node's first binding!\nError type: " + result.error.ToString());
				}
			}

			return true;
		}

		public bool selectResponse(int responseIndex)
		{
			// Make sure there currently is an active node:
			if(currentNode == null)
			{
				Debug.LogError("[DialogueController] Error! Unable to select response from null node! Aborting response.");
				return false;
			}

			// Ignore response index and show next content item instead, if multiple contents exist for the current node:
			if(currentNode.content != null && currentNode.content.Length > 1 && currentContentIndex < currentNode.content.Length - 1)
			{
				// Notify the dialogue trigger that a next content will now be displayed:
				if (trigger != null)
				{
					trigger.notifyDialogueEvent(DialogueEvent.ContentChanged);
				}

				// Increment content index:
				currentContentIndex++;
				return true;
			}

			// Verify index and currently active selection:
			DialogueResponse[] responses = getCurrentResponses();
			if(responses == null || responseIndex < 0 || responseIndex >= responses.Length)
			{
				Debug.LogError("[DialogueController] Error! Invalid response index " + responseIndex + " selected! Aborting response.");
				return false;
			}

			// If a followup node was provided, select it right away:
			DialogueResponse selected = responses[responseIndex];
			if(selected.nextNode != null)
			{
				// Notify the dialogue trigger that a response was selected:
				if (trigger != null)
				{
					trigger.notifyDialogueEvent(DialogueEvent.PlayerResponse);
				}

				// Select followup node:
				return selectNode(selected.nextNode);
			}

			// TODO: End dialogue or whatever. (todo: maybe add further behaviour settings to dialogue asset?)

			//TEMP
			endDialogue();
			return true;
		}


		#endregion
	}
}
