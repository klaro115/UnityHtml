using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Utml
{
	[System.Serializable]
	public class UtmlCanvasBuilder
	{
		#region Types

		public enum Type
		{
			Element,
			Button,
			Toggle,
			Input,
			Image,
			//...

			None
		}
		public enum StyleType
		{
			LabelColor,		// 'labelColor'
			LabelSize,		// 'labelSize'
			TextColor,		// 'textColor'
			TextSize,		// 'textSize'
			BgColor,		// 'bgColor'
			BgImage,		// 'bgImage'
			ImgColor,		// 'imgColor'
			ImgImage,		// 'imgImage'
			ImgStyle,		// 'imgStyle'
			ImgFillAmount,	// 'imgFillAmount'
			ImgFill,		// 'imgFill'
			//...

			None
		}
		public enum BindingType
		{
			Path,			// 'path'
			Type,			// 'type'
			//...

			String,			// 'string'
			DisableSelf,	// 'disable'
			//...

			None
		}

		public enum ObjectType
		{
			Paragraph,	// 'p'
			Button,		// 'button'
			Toggle,		// 'toggle'
			Input,		// 'input'
			Image,		// 'img'
			//...

			Div,		// 'div'
			//...

			Unknown
		}

		#endregion
		#region Fields

		public UtmlStyle style = null;
//		private UtmlElementStyle elementStyle = UtmlElementStyle.Default;
//		private List<UtmlParsedElement> pElements = new List<UtmlParsedElement>();

		#endregion
		#region Fields Static

		// Object type parsing stuff:
		public static readonly string[] parsingObjectKeys = new string[6]
		{
			"p", "button", "toggle", "input", "img", "div"
		};
		public static readonly ObjectType[] parsingObjectTypes = new ObjectType[6]
		{
			ObjectType.Paragraph, ObjectType.Button, ObjectType.Toggle, ObjectType.Input, ObjectType.Image, ObjectType.Div
		};

		// Element type parsing stuff:
		public static readonly string[] parsingTypeStrings =
			new string[5] { "p", "button", "toggle", "input", "img" };
		public static readonly Type[] parsingTypes =
			new Type[5] { Type.Element, Type.Button, Type.Toggle, Type.Input, Type.Image };

		// Element style parsing stuff:
		public static readonly string[] parsingStyleStrings = new string[11]
		{
			"labelColor", "labelSize", "textColor", "textSize", "bgColor", "bgImage",
			"imgColor", "imgImage", "imgStyle", "imgFillAmount", "imgFill"
		};
		public static readonly StyleType[] parsingStyles = new StyleType[11]
		{
			StyleType.LabelColor, StyleType.LabelSize, StyleType.TextColor, StyleType.TextSize,
			StyleType.BgColor, StyleType.BgImage, StyleType.ImgColor, StyleType.ImgImage,
			StyleType.ImgStyle, StyleType.ImgFillAmount, StyleType.ImgFill
		};

		public static readonly string parsingBindingKey = "binding";
		public static readonly string[] parsingBindingStrings =
			new string[4] { "path", "type", "string", "disable" };
		public static readonly BindingType[] parsingBindings =
			new BindingType[4] { BindingType.Path, BindingType.Type, BindingType.String, BindingType.DisableSelf };
		
		#endregion
		#region Methods Parsing

		public bool parse(string utmlFilePath)
		{
			// Verify input parameters:
			if(string.IsNullOrEmpty(utmlFilePath))
			{
				Debug.LogError("[UtmlCanvasBuilder] Error! Please provide a valid path to a Utml source file!");
				return false;
			}

			// Use the basic element style from asset:
//			elementStyle = style.baseStyle;

			// Prepare an string array containing all lines within the Utml file:
			string text = "";

			// Try reading the contents from file:
			StreamReader reader = null;
			try
			{
				reader = new StreamReader(utmlFilePath);
				if(reader == null)
				{
					Debug.LogError("[UtmlCanvasBuilder] Error! Couldn't find or open file '" + utmlFilePath + "'!");
					return false;
				}

				// Read all file contents to string:
				string fileTxt = reader.ReadToEnd();
				reader.Close();

				if(string.IsNullOrEmpty(fileTxt))
				{
					Debug.LogError("[UtmlCanvasBuilder] Error! Utml file was empty, unable to parse.");
					return false;
				}

				// Do some preliminary string formatting right away:
				string[] lines = fileTxt.Split(new char[1] { '\n' });
				for(int i = 0; i < lines.Length; ++i)
				{
					string line = lines[i];
					// Skip empty lines:
					if(string.IsNullOrEmpty(line)) continue;

					// Sort out comments:
					int commentIndex = line.IndexOf('#');
					if(commentIndex == 0)
						continue;
					else if(commentIndex > 0)
						line = line.Substring(0, commentIndex);

					// Assemble coding bits to one string:
					text += line;
				}

				// Get rid of some undesirable characters:
				text = text.Replace('\n', ' ');
				text = text.Replace('\0', ' ');
				text = text.Replace('\t', ' ');
			}
			// Catch any exceptions and deal with the aftermath:
			catch (System.Exception ex)
			{
				Debug.LogError("[UtmlCanvasBuilder] ERROR! An exception was encountered while trying to read" +
					" Utml file!\nException message: '" + ex.Message + "'.");
				// TODO: Clean up the mess?

				if(reader != null) reader.Close();
				return false;
			}

			// Parse file header first:
			string prefixHeader = "<head>";
			string postfixHeader = "</head>";
			if(text.Contains(prefixHeader) && text.Contains(postfixHeader))
			{
				int headerIndex = text.IndexOf(prefixHeader) + prefixHeader.Length;
				int headerSize = text.IndexOf(postfixHeader) - headerIndex - postfixHeader.Length;

				parseHeader(text.Substring(headerIndex, headerSize));
			}

			// Start parsing file body next:
			string prefixBody = "<body>";
			string postfixBody = "</body>";
			if(!text.Contains(prefixHeader) || !text.Contains(postfixHeader))
			{
				Debug.LogError("[UtmlCanvasBuilder] Error! Utml file must always contain at least a body!");
				return false;
			}
			// Parse file body:
			{
				int bodyIndex = text.IndexOf(prefixBody) + prefixBody.Length;
				int bodySize = text.IndexOf(postfixBody) - bodyIndex - postfixBody.Length;

				parseBody(text.Substring(bodyIndex, bodySize));
			}

			// Return success:
			return true;
		}

		private void parseHeader(string text)
		{
			if(string.IsNullOrEmpty(text)) return;

			// TODO
		}

		private void parseBody(string text)
		{
			if(string.IsNullOrEmpty(text)) return;

			char[] breakChars = new char[5] { ' ', ',', '/', '>', '=' };
			for(int i = 0; i < text.Length; ++i)
			{
				char c = text[i];
				char c2 = text[i + 1];

				if(c == '<')
				{
					i++;
					// 1. Identify which word this is:
					int wordEndIndex = text.IndexOfAny(breakChars, i);
					string word = text.Substring(i, wordEndIndex - i);

					int bracketEndIndex = text.IndexOf('>', wordEndIndex);
					bool isSingleBracket = text[bracketEndIndex - 1] == '/';

					// Figure out the type of object the word introduces:
//					ObjectType wordType = ObjectType.Unknown;
					for(int j = 0; j < parsingObjectKeys.Length; ++j)
					{
						if(parsingObjectKeys[i] == word)
						{
//							wordType = parsingObjectTypes[i];
							break;
						}
					}

					// 2.a) The object is contained within a single bracket body:
					if(isSingleBracket)
					{
						
					}
					// 2.b) The objects has a body as well, possibly with additional objects inside:
					else
					{
						
					}

					//parseBodyWord(text, i, wordEndIndex);
				}
			}

			// TODO
		}





		#endregion
		#region Parsing Helpers

		// Parse text to UI element type:
		private static Type parseTypeString(string txt, ref int index)
		{
			for(int i = 0; i < parsingTypeStrings.Length; ++i)
			{
				if(compareString(txt, parsingTypeStrings[i], ref index))
				{
					return parsingTypes[i];
				}
			}
			return Type.None;
		}
		// Parse text to UI style:
		private static StyleType parseStyleString(string txt, ref int index)
		{
			for(int i = 0; i < parsingStyleStrings.Length; ++i)
			{
				if(compareString(txt, parsingStyleStrings[i], ref index))
				{
					return parsingStyles[i];
				}
			}
			return StyleType.None;
		}
		// Parse text to binding parameter:
		private static BindingType parseBindingString(string txt, ref int index)
		{
			for(int i = 0; i < parsingBindingStrings.Length; ++i)
			{
				if(compareString(txt, parsingBindingStrings[i], ref index))
				{
					return parsingBindings[i];
				}
			}
			return BindingType.None;
		}

		// Directly compare part of a text line (starting at index) to a keyword:
		private static bool compareString(string txt, string comp, ref int index)
		{
			int minLength = Mathf.Min(txt.Length, comp.Length);
			for(int i = 0; i < minLength; ++i)
			{
				if(txt[index + i] != comp[i]) return false;
			}
			index += comp.Length;
			return true;
		}

		// Retrieve a prefab corresponding to a given UI element type:
		private UtmlElement getTypePrefab(Type type)
		{
			switch (type)
			{
			case Type.Element:
				return style.paragraphPrefab;
			case Type.Button:
				return style.buttonPrefab;
			case Type.Toggle:
				return style.togglePrefab;
			case Type.Input:
				return style.inputPrefab;
			case Type.Image:
				return style.imagePrefab;
			default:
				break;
			}
			return null;
		}

		// Read and parse style parameters, then apply values to current style:
		private static void applyStyleString(string txt, ref int index, StyleType type, ref UtmlElementStyle style)
		{
			// TODO: Read and parse style parameters, then apply values to current style.
		}
		// Revert a designated style setting to the base style value:
		private static void endStyleString(StyleType type, ref UtmlElementStyle style, UtmlElementStyle baseStyle)
		{
			switch (type)
			{
			case StyleType.LabelColor:
				style.labelColor = baseStyle.labelColor;
				break;
			case StyleType.LabelSize:
				style.labelFontSize = baseStyle.labelFontSize;
				break;
			case StyleType.TextColor:
				style.textColor = baseStyle.textColor;
				break;
			case StyleType.TextSize:
				style.textFontSize = baseStyle.textFontSize;
				break;
			case StyleType.BgColor:
				style.backgroundColor = baseStyle.backgroundColor;
				break;
			case StyleType.BgImage:
				style.backgroundOverride = baseStyle.backgroundOverride;
				break;
			case StyleType.ImgColor:
				style.imageColor = baseStyle.imageColor;
				break;
			case StyleType.ImgImage:
				style.imageOverride = baseStyle.imageOverride;
				break;
			case StyleType.ImgStyle:
				style.imageStyle = baseStyle.imageStyle;
				break;
			case StyleType.ImgFillAmount:
				style.imageFillAmount = baseStyle.imageFillAmount;
				break;
			case StyleType.ImgFill:
				style.imageFill = baseStyle.imageFill;
				break;
			default:
				break;
			}
		}

		#endregion
		#region Methods Build
	
		public bool build(UtmlCanvas canvas)
		{
			// Verify input parameters:
			if(canvas == null)
			{
				Debug.LogError("[UtmlCanvasBuilder] Error! An instance of type UtmlCanvas is required as" +
					" parent object and interface to all newly created UI elements!");
				return false;
			}

			// TODO: Check if any Utml data has peviously been parsed. (stored in 'pElements' list)

			/* TODO:
			 * 1. Spawn UI elements from prefabs, parent to canvas. ([later] parent according to hierarchy).
			 * 2. Set contents from styles.
			 * 3. Position and scale UI elements correctly.
			 * 4. Assign bindings.
			 * 5. Initialize instances and set references.
			 */

			Debug.LogError("[UtmlCanvasBuilder] NOT IMPLEMENTED!");
			return false;
		}

		#endregion
	}
}
