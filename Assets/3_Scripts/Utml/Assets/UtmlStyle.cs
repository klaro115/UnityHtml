using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utml
{
	[CreateAssetMenu(menuName="Utml/Create Style", fileName="new Utml Style")]
	[System.Serializable]
	public class UtmlStyle : ScriptableObject
	{
		#region Fields

		public UtmlButton buttonPrefab;		// A regular clickable button.
		public UtmlToggle togglePrefab;		// A plain old checkbox.
		public UtmlInput inputPrefab;		// An input field for entering text.
		public UtmlElement paragraphPrefab;	// A text box, or multi-line label.
		public UtmlImage imagePrefab;		// An image container.

		public UtmlElementStyle baseStyle;	// Default base element style to use.

		#endregion
	}
}
