using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using UBindings;

namespace Utml
{
	[RequireComponent(typeof(Button))]
	public class UtmlButton : UtmlElement
	{
		#region Fields

		[SerializeField]
		private Button uiButton = null;
		public Binding binding = Binding.Blank;

		#endregion
		#region Methods

		protected override Selectable getUiControl ()
		{
			return uiButton;
		}

		public void uiButtonPressed()
		{
			// Call the binding event whenever the button is clicked:
			executeBinding(ref binding);
		}

		#endregion
	}
}
