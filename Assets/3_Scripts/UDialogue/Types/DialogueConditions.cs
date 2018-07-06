using UnityEngine;
using System.Collections;

namespace UDialogue
{
	[System.Serializable]
	public struct DialogueConditions
	{
		#region Fields



		#endregion
		#region Properties

		public static DialogueConditions None
		{
			get
			{
				DialogueConditions dc = new DialogueConditions();

				//...

				return dc;
			}
		}

		#endregion
		#region Methods


		#endregion
	}
}
