using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UDialogue
{
	/// <summary>
	/// Listener interface used for receiving feedback about ongoing dialogue events.
	/// It also serves to query wether the conditions for responses and roots have been met.
	/// </summary>
	public interface IDialogueTrigger
	{
		/// <summary>
		/// Tell the trigger entity about events as they come up during a dialogue.
		/// </summary>
		/// <param name="eventType">Type of event.</param>
		void notifyDialogueEvent(DialogueEvent eventType);
		/// <summary>
		/// Check if a certain dialogue condition has been met.
		/// </summary>
		/// <returns><c>true</c>, if condition was met, <c>false</c> otherwise.</returns>
		/// <param name="condition">The condition in question.</param>
		bool checkDialogueCondition(ref DialogueConditions condition);
	}
}
