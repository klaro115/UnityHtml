using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UDialogue
{
	/// <summary>
	/// Listener interface used for receiving feedback about ongoing dialogue events.
	/// </summary>
	public interface IDialogueTrigger
	{
		void notifyDialogueEvent(DialogueEvent eventType);
	}
}
