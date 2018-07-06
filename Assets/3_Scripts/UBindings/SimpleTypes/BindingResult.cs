using UnityEngine;
using System.Collections;

namespace UBindings
{
	public struct BindingResult
	{
		#region Constructors

		public BindingResult(bool inValue)
		{
			value = inValue;
			error = BindingError.Success;
		}
		public BindingResult(BindingError inError)
		{
			value = false;
			error = inError;
		}

		#endregion
		#region Fields

		public bool value;			// Return value of the binding call.
		public BindingError error;	// Type of any error encountered while resolving the binding.

		#endregion
	}
}
