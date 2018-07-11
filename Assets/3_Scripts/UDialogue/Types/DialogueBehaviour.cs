using System.Collections;
using UnityEngine;

namespace UDialogue
{
	[System.Serializable]
	public struct DialogueBehaviour
	{
		#region Types

		public enum NullResponseAction
		{
			End,			// End current dialogue. (default behaviour)
			ReturnToRoot,	// Return to the dialogue's root node.
			None,			// Stay on the current node, don't do anything.
		}

		#endregion
		#region Fields

		public NullResponseAction onNullResponse;
		//...

		#endregion
		#region Properties

		public static DialogueBehaviour Default
		{
			get
			{
				DialogueBehaviour db = new DialogueBehaviour();

				db.onNullResponse = NullResponseAction.End;
				//...

				return db;
			}
		}

		#endregion
	}
}
