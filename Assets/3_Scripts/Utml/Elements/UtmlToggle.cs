using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using UBindings;

namespace Utml
{
	//[RequireComponent(typeof(Toggle))]
	public class UtmlToggle : UtmlElement
	{
		#region Fields

		[SerializeField]
		private Toggle uiToggle = null;
		public Binding binding = Binding.Blank;

		#endregion
		#region Methods

		protected override Selectable getUiControl ()
		{
			return uiToggle;
		}

		public void uiValueChanged()
		{
			// Update binding paramters from currently set toggle value:
			if(uiToggle != null)
			{
				binding.eventValue = uiToggle.isOn ? 1 : 0;
			}

			// Call the binding event whenever the button is clicked:
			executeBinding(ref binding);
		}

		#endregion
	}
}
