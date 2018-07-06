using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UBindings
{
	[System.Serializable]
	public struct Binding
	{
		#region Fields

		// EXECUTION & BEHAVIOUR:
		public string path;						// 'path'		Call and reference path relative to binding core.
		public BindingType type;				// 'type'		Type of event this binds to.
		public BindingResponse responseCode;	// 	-			Response code container after calling an event.

		// STATUS PARAMETERS:
		public int eventValue;					// 	-			Value serving as status parameter when calling events.
		public string eventString;				// 'string'		String value parameter when calling events.
		public object eventObject;				//  -			Any other type of data linked to this binding.
		//...

		#endregion
		#region Properties

		public static Binding Blank
		{
			get
			{
				Binding b = new Binding();

				b.path = null;
				b.type = BindingType.SingleEvent;
				b.responseCode = BindingResponse.None;

				b.eventValue = 0;
				b.eventString = null;
				b.eventObject = null;
				//...

				return b;
			}
		}

		#endregion
	}
}
