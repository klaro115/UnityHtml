using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Utml
{
	[System.Serializable]
	public struct UtmlElementStyle
	{
		#region Fields

		public Color labelColor;			// 'labelColor'		Descriptive text label color.
		public int labelFontSize;			// 'labelSize'		Descriptive text font size.

		public Color textColor;				// 'textColor'		Text content color.
		public int textFontSize;			// 'textSize'		Text content font size.

		public Color backgroundColor;		// 'bgColor'		Color of background graphics.
		public Sprite backgroundOverride;	// 'bgImage'		Replacement sprite for background graphics.

		public Color imageColor;			// 'imgColor'		Color of image content.
		public Sprite imageOverride;		// 'imgImage'		Replacement sprite for image content.
		public Image.Type imageStyle;		// 'imgStyle'		Display style for image content.
		public Image.FillMethod imageFill;	// 'imgFill'		Fill method used on fill-type image elements.
		public float imageFillAmount;		// 'imgFillAmount'	Fill amount used on fill-type image elements.

		#endregion
		#region Properties

		public static UtmlElementStyle Default
		{
			get
			{
				UtmlElementStyle es = new UtmlElementStyle();

				es.labelColor = Color.white;
				es.labelFontSize = 14;

				es.textColor = Color.black;
				es.textFontSize = 14;

				es.backgroundColor = Color.white;
				es.backgroundOverride = null;

				es.imageColor = Color.white;
				es.imageOverride = null;
				es.imageStyle = Image.Type.Simple;
				es.imageFill = Image.FillMethod.Horizontal;
				es.imageFillAmount = 1.0f;

				return es;
			}
		}

		#endregion
	}
}
