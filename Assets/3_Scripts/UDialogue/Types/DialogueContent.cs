using UnityEngine;
using System.Collections;

using UBindings;

namespace UDialogue
{
	[System.Serializable]
	public struct DialogueContent
	{
		#region Fields

		public int speakerId;			// ID of the character doing the talking during this content block.
		public string text;				// Text to be displayed while this content block is active.
		public float timeout;			// Time delay before automatically skipping to next content. (none=-1)

		public Binding eventBinding;	// Binding triggering an event related to this content block.

		#endregion
		#region Properties

		public static DialogueContent Blank
		{
			get
			{
				DialogueContent dc = new DialogueContent();

				dc.speakerId = 0;
				dc.text = null;
				dc.timeout = -1.0f;		// Set -1 to indicate that no timer be used.

				dc.eventBinding = Binding.Blank;

				return dc;
			}
		}

		#endregion
	}
}
