using UnityEngine;
using System.Collections;

namespace UBindings
{
	public interface IBindingCore
	{
		BindingResult executeBinding(ref Binding binding);
	}
}
