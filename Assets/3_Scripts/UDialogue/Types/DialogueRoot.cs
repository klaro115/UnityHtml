using UnityEngine;
using System.Collections;

namespace UDialogue
{
	[System.Serializable]
	public struct DialogueRoot
	{
		#region Fields

		public DialogueConditions conditions;	// Conditions that need to be met before unlocking this node.
		public DialogueNode node;				// The potential starting node of the dialogue.

		#endregion
		#region Properties

		public static DialogueRoot Blank
		{
			get
			{
				DialogueRoot dr = new DialogueRoot();

				dr.conditions = DialogueConditions.None;
				dr.node = null;

				return dr;
			}
		}

		#endregion
	}
}
