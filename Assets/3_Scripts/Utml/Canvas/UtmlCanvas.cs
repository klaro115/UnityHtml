using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

using UBindings;

namespace Utml
{
	public class UtmlCanvas : MonoBehaviour, IBindingCore
	{
		#region Types

		public delegate void BindingListener(ref Binding binding);

		#endregion
		#region Fields

		private Dictionary<string, UtmlElement> elements = new Dictionary<string, UtmlElement>();

		#endregion
		#region Fields Static
		
		private static BindingExecutor exec = null;

		#endregion
		#region Properties

		public Dictionary<string, UtmlElement> Elements
		{
			get { return elements; }
		}

		#endregion
		#region Methods Elements

		public UtmlElement getElementByTag(string inTag)
		{
			if(string.IsNullOrEmpty(inTag))
			{
				Debug.LogError("[UtmlCanvas] Error! Unable to get UI element using null or empty tag!");
				return null;
			}
			if(elements == null || elements.Count == 0)
			{
				Debug.LogError("[UtmlCanvas] Error! No elements were registered in canvas, cannot retrieve by tag!");
				return null;
			}

			// Try to find the element by its tag, then return result:
			UtmlElement element = null;
			elements.TryGetValue(inTag, out element);
			if(element == null)
			{
				Debug.LogError("[UtmlCanvas] Error! No element with tag '" + inTag + "' could be found!");
			}
			return element;
		}

		#endregion
		#region Methods Binding

		public BindingResult executeBinding(ref Binding binding)
		{
			if(exec == null) exec = new BindingExecutor();

			return exec.executeBinding(ref binding, this);
		}

		#endregion
	}
}
