using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UBindings;

namespace Utml
{
	[System.Serializable]
	public struct UtmlParsedElement
	{
		#region Fields

		//public int id;					// Unique identification number of the element.
		//public int parentId;				// ID number of the parent UI element.

		public string tag;					// Unique object indentifier tag (can be left blank).
		public string label;				// Current label text contents.
		public Rect localRect;				// Local position and scale on UI.

		public UtmlCanvasBuilder.Type type;	// What type of UI element this represents.
		public UtmlElement prefab;			// The prefab to spawn this from in scene.

		public UtmlElementStyle style;		// What style to apply to this element when spawning it.
		public Binding binding;				// Event and method binding of the UI element.

		#endregion
	}
}
