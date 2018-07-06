using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UBindings.Test
{
	public class BindingTestSetter : MonoBehaviour, IBindingCore
	{
		#region Types

		[System.Serializable]
		public class Data2
		{
			public string text;
		}

		[System.Serializable]
		public class Data
		{
			public Data2 data2;
		}

		#endregion
		#region Fields

		public InputField uiInputField = null;
		public Text uiOutputField = null;

		public Data data = new Data() { data2=new Data2() { text="This is a test." } };

		// Structure instance holding the binding data:
		public Binding binding = new Binding() { path="data/data2/=text", type=BindingType.SingleEvent };
		public Binding binding2 = new Binding() { path="uiOutputField/=text", type=BindingType.SingleEvent };
		private BindingExecutor exec = null;

		#endregion
		#region Methods

		public BindingResult executeBinding(ref Binding inBinding)
		{
			// Make sure there is a binding executor instance at hand, to resolve the binding:
			if(exec == null) exec = new BindingExecutor();

			return exec.executeBinding(ref inBinding, this);
		}

		public void uiInputChanged(string txt)
		{
			// Use a binding to set the data's data's text field:
			binding.eventString = txt;
			executeBinding(ref binding);
			// Use another binding to set the UI text objects text property:
			binding2.eventString = data.data2.text;
			executeBinding(ref binding2);
		}

		#endregion
	}
}
