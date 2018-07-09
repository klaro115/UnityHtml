using UnityEngine;
using System.Collections;

namespace UDialogue
{
	/// <summary>
	/// Conditions for checking wether a dialogue event or element is currently available and valid.
	/// This structure serves as a condition indentifier rather than a universial abstract container
	/// for the infinte range of possible conditions you may need in your game scenario.
	/// (Check comments in struct declaration for concrete examples on how to use)
	/// </summary>
	[System.Serializable]
	public struct DialogueConditions
	{
		/* HOW TO USE:
		 * The following contains a set of instructions and examples on how to check a condition
		 * as it arises in the course of dialogue.
		 * 
		 * INSTRUCTIONS:
		 * - Conditions are sent to the dialogue trigger instance by a dialogue controller.
		 * - Trigger should then use the 'keyword' field to determine how to treat the condition.
		 * - 'Comparison' and 'targetValue' fields provide a way to specify how to perform the
		 *    check, in case multiple conditions treat a same state or flag.
		 * - 'relatedObject' field may serve as reference or database while checking condition.
		 * - Hint: You could call upon the binding system to query condition states directly.
		 * 
		 * EXAMPLE USES:
		 * - Check if a quest is active:
		 *     => 'keyword'="quest", 'comparison'="Contained", 'targetState'=[questId]
		 * - Player's score high enough:
		 *     => 'keyword'="score", 'comparison'="Greater", 'targetState'=65000
		 * - Check if a boolean flag is raised:
		 *     => 'keyword'="flag", 'comparison'="Equal", 'targetState'=1
		 */

		#region Types

		/// <summary>
		/// How to compare states and thus deduce whether a condition is met.
		/// </summary>
		public enum Comparison
		{
			Equal,						// Is the current state equal to the target state?
			Different,					// Is the current state clearly different from target state?
			Less,						// Current state less than or inferior to target one?
			Greater,					// Current state greater than or superior to target one?
			Contained,					// Is target state contained within the current state or vice-versa? 
		}

		#endregion
		#region Fields

		public string keyword;			// Keyword for dialogue trigger, used to identify the condition.

		public Comparison comparision;	// How to compare the current state of affairs to the target state.
		public int targetState;			// Integer abstraction of a target state. (ex.: 0=false, 1=true)

		public Object relatedObject;	// An asset or reference needed for checking the condition's state.

		#endregion
		#region Properties

		public static DialogueConditions None
		{
			get
			{
				DialogueConditions dc = new DialogueConditions();

				dc.keyword = null;
				dc.comparision = Comparison.Equal;
				dc.targetState = 0;
				dc.relatedObject = null;

				return dc;
			}
		}

		#endregion
	}
}
