using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Utml
{
	[RequireComponent(typeof(RectTransform))]
	public class UtmlImage : UtmlElement
	{
		#region Fields

		[SerializeField]
		protected Image uiImage = null;

		#endregion
		#region Methods

		public override void setStyle (UtmlElementStyle style)
		{
			// Update style for base UI members:
			base.setStyle (style);

			// Set image contents, display style and coloring:
			if(uiImage != null)
			{
				uiImage.color = style.imageColor;
				uiImage.overrideSprite = style.imageOverride;
				uiImage.type = style.imageStyle;

				if(style.imageStyle == Image.Type.Filled)
				{
					uiImage.fillMethod = style.imageFill;
					uiImage.fillAmount = style.imageFillAmount;
				}
			}

			// TODO: Adjust element size to match image size and aspect ratio.
		}

		public void setImage(Sprite newSprite, float fillAmount = 1.0f)
		{
			if(uiImage != null)
			{
				uiImage.overrideSprite = newSprite;
				uiImage.fillAmount = Mathf.Clamp01(fillAmount);

				// TODO: Update element size to match image size and aspect ratio.
			}
		}

		#endregion
	}
}
