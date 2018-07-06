using UnityEngine;
using System.Collections;

namespace UBindings
{
	public enum BindingError
	{
		Success,
		Failure,

		InvalidBinding,
		InvalidType,

		NotAssigned,
		NotFound,
		NotImplemented,

		NullReference,
	}
}
