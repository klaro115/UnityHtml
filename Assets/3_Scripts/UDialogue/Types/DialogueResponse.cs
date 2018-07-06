using UnityEngine;
using System.Collections;

namespace UDialogue
{
	[System.Serializable]
	public struct DialogueResponse
	{
		#region Fields

		public string responseText;
		public DialogueConditions conditions;
		public DialogueNode nextNode;

		#endregion
		#region Properties

		public static DialogueResponse Blank
		{
			get
			{
				DialogueResponse dr = new DialogueResponse();

				dr.responseText = "[...]";
				dr.conditions = DialogueConditions.None;
				dr.nextNode = null;

				return dr;
			}
		}

		#endregion
	}
}
