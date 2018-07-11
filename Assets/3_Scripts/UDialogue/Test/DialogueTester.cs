using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UBindings;

namespace UDialogue.Test
{
	public class DialogueTester : MonoBehaviour, IBindingCore, IDialogueTrigger
	{
		#region Types

		[System.Serializable]
		public struct Flag
		{
			public string name;
			public int state;
		}

		#endregion
		#region Fields

		public Dialogue dialogue = null;

		private DialogueController dialogueController = null;
		private BindingExecutor bindingExecutor = null;

		private bool isActive = true;

		private Flag[] flags = new Flag[5]
		{
			new Flag() { name="Romance", state=0 },
			new Flag() { name="Hostile", state=0 },
			new Flag() { name="Romance2", state=0 },
			new Flag() { name="Quest", state=0 },
			new Flag() { name="SpecialGoods", state=0 }
		};

		private float uiQuestPopupTime = 0.0f;

		#endregion
		#region Methods

		void Start()
		{
			isActive = dialogue != null;
			dialogueController = new DialogueController();

			dialogueController.loadDialogue(dialogue);
			dialogueController.startDialogue(this, this);

			uiQuestPopupTime = -10.0f;
		}

		void OnGUI()
		{
			if (dialogue == null) return;
			if(!isActive)
			{
				if(GUI.Button(new Rect(Screen.width*.5f, Screen.height*.5f, 200, 22), "Start Dialogue"))
				{
					isActive = true;
					dialogueController.startDialogue(this, this);
				}

				return;
			}

			const float uiRespHeight = 22;
			const float uiRespBlockHeight = uiRespHeight + 3;
			const float uiContentHeaderHeight = 20;
			const float uiContentOffset = uiContentHeaderHeight + 3;
			const float uiContentHeight = 105;
			const float uiResponseHeight = 6 * uiRespHeight + 8;
			const float uiFullHeight = uiContentHeight + uiResponseHeight;
			const float m = 4;

			float sw = Screen.width - 2 * m;
			float sh = Screen.height - m;

			Rect fullRect = new Rect(m, sh - uiFullHeight, sw, uiFullHeight);

			DialogueContent content = dialogueController.getCurrentContent();
			DialogueResponse[] responses = dialogueController.getCurrentResponses();
			string contentTxt = content.text;
			string charNameTxt = dialogue.characters[content.speakerId].name;

			GUI.BeginGroup(fullRect);
			GUI.Box(new Rect(0, 0, fullRect.width, fullRect.height), "");

			// Content block:
			Rect contRect = new Rect(m, m + uiContentOffset, fullRect.width - 2 * m, uiContentHeight);
			Rect headerRect = new Rect(contRect.x, 0, contRect.width, uiContentHeaderHeight);

			GUI.Label(headerRect, string.Format("<b>{0}:</b>", charNameTxt));
			GUI.Label(contRect, contentTxt);

			// Response block:
			Rect respRect = new Rect(contRect.x, fullRect.height-uiResponseHeight-m, contRect.width, uiResponseHeight);
			GUI.BeginGroup(respRect);

			// Show all available response options:
			for(int i = 0; i < responses.Length; ++i)
			{
				Rect iRespRect = new Rect(0, i * uiRespBlockHeight, respRect.width, uiRespHeight);
				DialogueResponse response = responses[i];
				string iRespTxt = response.responseText;

				// Display each response as a simple button:
				if (GUI.Button(iRespRect, iRespTxt))
				{
					// Call onto dialogue controller to select this response:
					dialogueController.selectResponse(i);
				}
			}

			GUI.EndGroup();
			GUI.EndGroup();

			// Quest popup:
			float questPopupDelta = Time.time - uiQuestPopupTime;
			const float questPopupMaxTime = 3.0f;
			if (questPopupDelta < questPopupMaxTime)
			{
				Color prevCol = GUI.color;

				float alpha = 1 - Mathf.Clamp01((questPopupDelta - 1.0f) / (questPopupMaxTime - 1.0f));
				GUI.color = new Color(1, 0.75f, 0, alpha);
				GUI.Box(new Rect(Screen.width*.5f-180, 128, 360, 60), "New Quest:\n<b>Saving the princess.</b>");

				GUI.color = prevCol;
			}
		}

		public BindingResult executeBinding(ref Binding binding)
		{
			// NOTE: IBindingCore execution method implementation.
			if(bindingExecutor == null) bindingExecutor = new BindingExecutor();

			return bindingExecutor.executeBinding(ref binding, this);
		}

		public void notifyDialogueEvent(DialogueEvent eventType)
		{
			// NOTE: IDialogueTrigger listener method implementation.
			switch (eventType)
			{
				case DialogueEvent.End:
					isActive = false;
					break;
				default:
					break;
			}
		}

		public bool checkDialogueCondition(ref DialogueConditions condition)
		{
			// See if the condition requires a flag check:
			string flagKeyword = "flag:";
			if(!condition.keyword.Contains(flagKeyword))
			{
				if (condition.keyword.Contains("checkMoney")) return checkMoney(condition.targetState, condition.comparision);
				else if (condition.keyword.Contains("checkCharisma")) return checkCharisma(condition.targetState, condition.comparision);
				else return false;
			}

			// Retrieve the requested flag's name:
			int flagNameIndex = condition.keyword.IndexOf(':') + 1;
			string flagName = condition.keyword.Substring(flagNameIndex);

			// Iterate through all flags and find a matching one:checkMoney
			for(int i = 0; i < flags.Length; ++i)
			{
				if(string.Compare(flags[i].name, flagName) == 0)
				{
					// Compare current state to the condition's target state value:
					switch (condition.comparision)
					{
					case DialogueConditions.Comparison.Equal:
						return flags[i].state == condition.targetState;
					case DialogueConditions.Comparison.Different:
						return flags[i].state != condition.targetState;
					case DialogueConditions.Comparison.Greater:
						return flags[i].state > condition.targetState;
					case DialogueConditions.Comparison.Less:
						return flags[i].state < condition.targetState;
					default:
						break;
					}
				}
			}
			// No match or unsupported comparison type, fail check:
			return false;
		}

		private bool checkMoney(int amount, DialogueConditions.Comparison comparison)
		{
			// NOTE: Placeholder code/method for checking how the player's funds compare to the specified amount.

			Debug.Log("Condition check: Checking if the player has sufficient funds in their inventory." +
				"\nAmount required: " + amount + " - Comparison type: " + comparison.ToString());
			return true;
		}

		private bool checkCharisma(int score, DialogueConditions.Comparison comparison)
		{
			// NOTE: Placeholder code/method for checking how the player's charisma score compares to the specified value.

			Debug.Log("Condition check: Checking if the player has a sufficiently high charisma score." +
				"\nScore required: " + score + " - Comparison type: " + comparison.ToString());
			return true;
		}

		#endregion
		#region Methods BindingListeners

		public void raiseFlag(ref Binding binding)
		{
			// Find a flag with the name given in 'binding.eventString':
			for(int i = 0; i < flags.Length; ++i)
			{
				if(string.Compare(flags[i].name, binding.eventString) == 0)
				{
					// Change state value of the flag:
					flags[i].state = binding.eventValue;
					binding.responseCode = BindingResponse.OK;

					// Enable a popup if the quest flag was raised:
					if(i == 3 && flags[i].state != 0)
					{
						uiQuestPopupTime = Time.time;
					}

					return;
				}
			}

			// No flag with that name was found, report failure:
			binding.responseCode = BindingResponse.Fail;
		}

		public void startTrade(ref Binding binding)
		{
			// End ongoing dialogue:
			dialogueController.endDialogue();

			// See if black market should be shown as well:
			bool showBlackMarket = binding.eventValue != 0;

			// Open trade menu and filter goods:
			Debug.Log("[TRADE NOT IMPLEMENTED] (show black market items: " + showBlackMarket.ToString() + ")");	//TODO
		}

		public void spendMoney(ref Binding binding)
		{
			// NOTE: Placeholder code/method for withdrawing an amount of money from the player's inventory.

			int goldCoins = binding.eventValue;
			Debug.Log("Removing money from player's inventory: " + goldCoins + " gold");
		}

		#endregion
	}
}
