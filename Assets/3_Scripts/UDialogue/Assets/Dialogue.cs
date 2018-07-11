using System.Collections;
using UnityEngine;

using UBindings;

namespace UDialogue
{
	[CreateAssetMenu(menuName="UDialogue/Create Dialogue", fileName="new Dialogue")]
	[System.Serializable]
	public class Dialogue : ScriptableObject
	{
		#region Fields

		public DialogueCharacter[] characters = new DialogueCharacter[1] { DialogueCharacter.Default };

		public DialogueBehaviour behaviour = DialogueBehaviour.Default;

		public Binding startBinding = Binding.Blank;	// Event triggered at the start of the dialogue.
		public Binding endBinding = Binding.Blank;		// Event triggered when exiting the dialogue.

		public DialogueRoot[] rootNodes = null;			// All possible root starting nodes of the dialogue.

		#endregion
		#region Methods
		
		//...
		
		#endregion
	}
}
