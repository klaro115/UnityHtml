using UnityEngine;
using System.Collections;

namespace UDialogue
{
	[System.Serializable]
	public struct DialogueCharacter
	{
		#region Fields

		public string name;
		//...

		#endregion
		#region Properties

		public static DialogueCharacter Default
		{
			get
			{
				DialogueCharacter dc = new DialogueCharacter();

				dc.name = "Character";
				//...

				return dc;
			}
		}

		#endregion
	}
}
