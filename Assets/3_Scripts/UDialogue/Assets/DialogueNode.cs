using UnityEngine;
using System.Collections;

namespace UDialogue
{
	[System.Serializable]
	public class DialogueNode : ScriptableObject
	{
		#region Fields

		// Different blocks of content that are being said in chronological order:
		public DialogueContent[] content = new DialogueContent[1];

		// Different responses available after passing this node's content:
		public DialogueResponse[] responses = new DialogueResponse[1] { DialogueResponse.Blank };

		#endregion
	}
}
