using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using UBindings;

namespace Utml
{
	[RequireComponent(typeof(RectTransform))]
	public class UtmlElement : MonoBehaviour
	{
		#region Fields

		protected IBindingCore bindingCore = null;

		[SerializeField]
		protected Text uiLabel = null;
		[SerializeField]
		protected Image uiBackground = null;

		#endregion
		#region Methods

		public virtual bool setLabel(string labelText)
		{
			// If a label object is assigned, change its contents:
			if(labelText != null && uiLabel != null)
			{
				uiLabel.text = labelText;

				// Automatically resize the UI element based on label content for multi-line text:
				if(labelText.Contains("\n"))
				{
					setElementSize(RectTransform.Axis.Vertical, uiLabel.preferredHeight);
				}

				return true;
			}
			return false;
		}
		public virtual void setStyle(UtmlElementStyle style)
		{
			if(uiLabel != null)
			{
				uiLabel.color = style.labelColor;
				uiLabel.fontSize = style.labelFontSize;

				// Automatically resize the UI element based on label content for multi-line text:
				if(uiLabel.text.Contains("\n"))
				{
					setElementSize(RectTransform.Axis.Vertical, uiLabel.preferredHeight);
				}
			}
			if(uiBackground != null)
			{
				uiBackground.color = style.imageColor;
			}
		}

		public virtual void setElementSize(RectTransform.Axis axis, float newHeight)
		{
			// TODO: UNTESTED! Necessary for proper text paragraph scaling!

			RectTransform rTrans = transform as RectTransform;
			rTrans.SetSizeWithCurrentAnchors(axis, uiLabel.preferredHeight);
		}

		protected virtual Selectable getUiControl()
		{
			return GetComponent<Selectable>();
		}

		protected virtual bool executeBinding(ref Binding inBinding)
		{
			if(bindingCore != null)
			{
				bindingCore.executeBinding(ref inBinding);

				// React to the outcome of the event:
				return executeBindingResponseCode(ref inBinding);
			}
			// Binding can not be executed, return false:
			return false;
		}
		protected virtual bool executeBindingResponseCode(ref Binding inBinding)
		{
			bool success = true;

			switch (inBinding.responseCode) {
			case BindingResponse.OK:
				/*
				if(inBinding.disableSelf)
				{
					Selectable selectable = getUiControl();
					if(selectable != null) selectable.interactable = false;
				}
				*/
				break;
			case BindingResponse.Error:
				Debug.LogError("[UtmlElement] Error! Binding execution for '" + inBinding.path +
					"' encountered an error!");
				success = false;
				break;
			default:
				success = false;
				break;
			}

			// Reset binding's response code:
			inBinding.responseCode = BindingResponse.None;

			return success;
		}

		#endregion
	}
}
