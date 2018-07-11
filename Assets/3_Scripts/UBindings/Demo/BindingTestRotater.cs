using UnityEngine;
using System.Collections;

namespace UBindings.Test
{
	/// <summary>
	/// Binding testing scenario, used to demonstrate method binding.
	/// </summary>
	public class BindingTestRotater : MonoBehaviour, IBindingCore
	{
		#region Types

		[System.Serializable]
		public class Rotator
		{
			public float rotSpeed;
			public Transform transform;

			// Target method to call via binding:
			public void setRotationSpeed(ref Binding binding)
			{
				this.rotSpeed = (float)binding.eventObject;
				//Debug.Log("TEST: Setting " + transform.name + "'s speed to " + rotSpeed + " deg/s");
			}
		}

		#endregion
		#region Fields

		// Target member of the binding, also responsible for rotation speed:
		public Rotator rotator = new Rotator() { rotSpeed=30.0f, transform=null };

		// Structure instance holding the binding data:
		public Binding binding = new Binding() { path="rotator/:setRotationSpeed" };
		private BindingExecutor exec = null;

		public bool outputDebugLogs = false;

		#endregion
		#region Methods
		
		void Update()
		{
			rotator.transform.Rotate(Vector3.forward * rotator.rotSpeed * Time.deltaTime);
		}
		
		public BindingResult executeBinding(ref Binding inBinding)
		{
			// Make sure there is a binding executor instance at hand, to resolve the binding:
			if(exec == null) exec = new BindingExecutor();
			if(outputDebugLogs)
			{
				Debug.Log("Binding Core: Forwarding binding to executor.");
			}

			// Execute the binding and return result:
			BindingResult result = exec.executeBinding(ref inBinding, this);
			if(outputDebugLogs)
			{
				Debug.Log("Test Rotater: Binding execution returned with error code: " + result.error.ToString());
			}
			return result;
		}
		
		public void uiInputChanged(string txt)
		{
			try
			{
				if(outputDebugLogs)
				{
					Debug.Log("Test Rotater: Received text input '" + txt + "'.");
				}

				// Try casting input string to a float value, then set it as binding content:
				binding.eventObject = (float)System.Convert.ToDouble(txt);
				// Tell the binding core to execute it:
				executeBinding(ref binding);
			}
			catch (System.Exception ex)
			{
				if(outputDebugLogs)
				{
					Debug.Log("Test Rotater: Cannot parse string input.\nException message: " + ex.Message);
				}
				return;
			}
		}
		
		#endregion
	}
}
