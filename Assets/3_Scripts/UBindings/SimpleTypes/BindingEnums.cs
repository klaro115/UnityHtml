using UnityEngine;
using System.Collections;

namespace UBindings
{
	public enum BindingType
	{
		SingleEvent,		// 'single'	Trigger a unique event.
		StartProcess,		// 'start'	Start a continuous event or process.
		EndProcess,			// 'end'	Request a running process or long event to end.
	}

	public enum BindingResponse
	{
		None,				// No response registered or response was reset after processing.

		OK,					// Positive response, denoting successful execution of the binding.
		Fail,				// Negative response, denoting a failed call of the binding.

		Error,				// An error was encountered during execution of the binding.
		//...
	}
}
