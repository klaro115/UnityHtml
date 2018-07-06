using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UBindings;

namespace Utml
{
	public class UtmlDemo1Canvas : UtmlCanvas
	{
		#region Fields

		public UtmlCanvasBuilder builder = new UtmlCanvasBuilder();

		#endregion
		#region Methods

		void Start ()
		{
			// Make sure the canvas builder instance is not null:
			if(builder == null)
			{
				builder = new UtmlCanvasBuilder();
				builder.style = ScriptableObject.CreateInstance<UtmlStyle>();
				builder.style.baseStyle = UtmlElementStyle.Default;
			}

			// Load Utml source file and parse UI elements from text:
			string filePath = Application.dataPath + "/1_Prefabs/Utml/Resources/Demo/UtmlDemo1.txt";
			if(!builder.parse(filePath))
			{
				Debug.LogError("[UtmlDemo1Canvas] Error! Failed to load and build UI elements from Utml file!");
				gameObject.SetActive(false);
			}

			// Actually build UI elements from parsed data and spawn them in scene:
			builder.build(this);
		}

		// Display text entered by the user in a paragraph element on the canvas.
		private void displayInputString(ref Binding binding)
		{
			// 1. Retrieve a Utml canvas element via its tag:
			UtmlElement outputElement = getElementByTag("outputParagraph");

			// 2. Replace text contents on the UI element by the content supplied by the binding:
			outputElement.setLabel(binding.eventString);

			// 3. Set binding response code to OK, so the calling element may react accordingly:
			binding.responseCode = BindingResponse.OK;

			// NOTE: The response code is a direct feedback signal used by the binding system.
			// It is the major reason why 'binding' is passed by reference rather than by value, despite
			// it being a struct. The response code is reset to 'None' by the caller after processing it.
		}

		// Print out the iconical 'Hello World!' when the button is pressed.
		private void printHelloWorld(ref Binding binding)
		{
			// 1. Print out whatever the binding tells us to:
			Debug.Log("Hello World!");

			// 2. Set binding response code to OK, so the calling element may react accordingly:
			binding.responseCode = BindingResponse.OK;
		}

		#endregion
	}
}
