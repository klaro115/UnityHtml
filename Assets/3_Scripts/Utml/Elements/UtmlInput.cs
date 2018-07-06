using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using UBindings;

namespace Utml
{
	//[RequireComponent(typeof(InputField))]
	public class UtmlInput : UtmlElement
	{
		#region Types

		public enum BindingEvent
		{
			OnInputEnd,
			OnInputChanged,
		}

		#endregion
		#region Fields

		[SerializeField]
		private InputField uiInput = null;
		public Binding binding = Binding.Blank;

		public BindingEvent bindingEvent = BindingEvent.OnInputEnd;

		#endregion
		#region Methods

		protected override Selectable getUiControl ()
		{
			return uiInput;
		}

		public override void setStyle (UtmlElementStyle style)
		{
			// Update style for base UI members:
			base.setStyle(style);

			// Set text and placeholder color and font size according to the style:
			if(uiInput != null)
			{
				if(uiInput.placeholder != null)
				{
					Text uiPHTxt = uiInput.placeholder.GetComponent<Text>();
					uiPHTxt.color = style.textColor;
					uiPHTxt.fontSize = style.textFontSize;
				}
				uiInput.textComponent.color = style.textColor;
				uiInput.textComponent.fontSize = style.textFontSize;
			}
		}

		public void uiInputChanged()
		{
			binding.eventString = uiInput != null ? uiInput.text : null;

			// Call binding event once the text input was changed:
			if(bindingEvent == BindingEvent.OnInputChanged)
			{
				executeBinding(ref binding);
			}
		}
		public void uiInputEnded()
		{
			binding.eventString = uiInput != null ? uiInput.text : null;
			
			// Call binding event once text input has ended or focus on UI input field was lost:
			if(bindingEvent == BindingEvent.OnInputEnd)
			{
				executeBinding(ref binding);
			}
		}

		#endregion
	}
}
