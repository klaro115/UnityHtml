using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UBindings;

namespace UDialogue.Test
{
	public class DialogueTester : MonoBehaviour, IBindingCore, IDialogueTrigger
	{
		#region Fields

		public Dialogue dialogue = null;

		private DialogueController dialogueController = null;
		private BindingExecutor bindingExecutor = null;

		private bool isActive = true;

		#endregion
		#region Methods

		void Start()
		{
			isActive = dialogue != null;
			dialogueController = new DialogueController();

			dialogueController.loadDialogue(dialogue, this, this);
			dialogueController.startDialogue();
		}

		void OnGUI()
		{
			if (!isActive || dialogue == null) return;

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

		#endregion
	}
}
