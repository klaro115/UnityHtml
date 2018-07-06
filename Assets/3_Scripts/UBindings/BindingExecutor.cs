using UnityEngine;
using System.Collections;
using System.Text;
using System.Reflection;

namespace UBindings
{
	public class BindingExecutor
	{
		#region Types

		// The type of member a path is ultimately adressing:
		private enum MemberType
		{
			Field,
			Method,

			None
		}

		#endregion
		#region Fields Static

		// Constants for command characters in relative binding paths: (use as prefix to target member name)
		private static readonly char pathBufferFillChar = '$';
		private static readonly char pathMethodChar = ':';
		private static readonly char pathFieldChar = '=';

		// Initialization string for the string builder/buffer used in path resolution: (128 placeholder chars)
		private static readonly string pathBufferBlankString =
			"$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$" +
			"$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$";
		// Binding flags used for reflection when resolving fields in path:
		private static System.Reflection.BindingFlags bindingFieldFlags =
			BindingFlags.DeclaredOnly |
			BindingFlags.NonPublic |
			BindingFlags.Public |
			BindingFlags.Instance;

		#endregion
		#region Methods Binding

		/// <summary>
		/// Perform a basic check to see if there are any inconsitencies within a binding:
		/// </summary>
		/// <returns><c>true</c>, if binding was found valid, <c>false</c> otherwise.</returns>
		/// <param name="binding">The binding you wish to verify.</param>
		public static  bool verifyBinding(ref Binding binding)
		{
			// To verify the validity of a binding call, first check its path:
			if(string.IsNullOrEmpty(binding.path))
			{
				return false;
			}
			return true;
		}

		private BindingResult resolveBindingPath(ref Binding binding, IBindingCore core)
		{
			if(core == null)
			{
				Debug.LogError("[BindingExecutor] Error! Unable to resolve path from null binding core!");
				return new BindingResult(BindingError.NullReference);
			}
			// NOTE: When calling this, it is assumed that the binding was verified beforehand.
			// NOTE2: This method resolves the path, finds the event call method, then checks parameters.

			StringBuilder pathBuffer = new StringBuilder(pathBufferBlankString);
			// Reset path buffer:
			for(int j = 0; j < pathBuffer.Length; ++j)
				pathBuffer[j] = pathBufferFillChar;
			
			/* NOTE3: Was I trying to use a char array because strings are immutable and heap allocations are slow? Yep.
			* For the most part, I'm trying to resolve the path on stack only, with very few runtime allocations.
			* Had to use a string builder instead, because C# can't just use pointer style strings like in C++. */
			
			// Get the path from binding for easier access:
			string path = binding.path;
			
			// Declare some 'navigation' and reference variables:
			int bufferIndex = 0;
			MemberType memberType = MemberType.None;
			
			object current = core;
			System.Type currentType = core.GetType();
			
			try
			{
				// Iterate through the path characters and evaluate parts of it as you get to them:
				for(int i = 0; i < path.Length; ++i)
				{
					char c = path[i];
					
					// A new path segment commences, evaluate the previous section:
					if(c == '/')
					{
						string fBufferString = pathBuffer.ToString().Substring(0, bufferIndex);
						FieldInfo fInfo = currentType.GetField(fBufferString, bindingFieldFlags);
						if(fInfo == null)
						{
							// No matching field was found, abort query:
							Debug.LogError("[BindingExecutor] Error! Field '" + fBufferString +
								"' not found in type '" + currentType.Name + "'! Aborting path resolution!");
							return new BindingResult(BindingError.NotFound);
						}
						// Get the object instance referenced by the field's value and determine its type:
						current = fInfo.GetValue(current);
						if(current == null)
						{
							Debug.LogError("Error! For some obnoxious reason, '" + fInfo.Name + "' was null on object.");
							return new BindingResult(BindingError.NullReference);
						}
						currentType = current.GetType();

						// Reset path buffer and index:
						for(int j = 0; j < pathBuffer.Length; ++j)
							pathBuffer[j] = pathBufferFillChar;
						bufferIndex = 0;
					}
					// The method name at the end of a path is designated by the ':' prefix:
					else if(c == pathMethodChar)
					{
						memberType = MemberType.Method;
					}
					else if(c == pathFieldChar)
					{
						memberType = MemberType.Field;
					}
					else if(c != '\0')
					{
						// Write path characters into buffer one after the other:
						pathBuffer[bufferIndex++] = c;
					}

					if(c == '\0' || i == path.Length - 1)
					{
						// Check if a method or type name was found/declared within the preceeding path:
						switch (memberType)
						{
						case MemberType.Method:
							return useBindingMethod(current, pathBuffer, bufferIndex, ref binding);
						case MemberType.Field:
							return useBindingField(current, pathBuffer, bufferIndex, ref binding);
						default:
							Debug.LogError("[BindingExecutor] Error! No member declaration found in path '" + path +
								"'! Are you missing a '" + pathMethodChar + "' or '" + pathFieldChar + "' prefix?");
							break;
						}
						return new BindingResult(BindingError.InvalidBinding);
					}
				}
			}
			// Catch any exceptions in the process:
			catch (System.Exception ex)
			{
				Debug.LogError("[BindingExecutor] ERROR! An exception was caught while resolving binding path '" +
					path + "'!\nException message: '" + ex.Message + "'\nAborting path resolution!");
			}

			return new BindingResult(BindingError.Failure);
		}

		private BindingResult useBindingMethod(object current, StringBuilder pathBuffer, int bufferIndex, ref Binding binding)
		{
			System.Type currentType = current.GetType();

			// Use reflection to find the named method within the current path object:
			string mBufferString = pathBuffer.ToString().Substring(0, bufferIndex);
			MethodInfo mInfo = currentType.GetMethod(mBufferString);
			if(mInfo == null)
			{
				// No matching method was found, abort query:
				Debug.LogError("[BindingExecutor] Error! Method '" + mBufferString +
					"' not found in type '" + currentType.Name + "'! Aborting path resolution!");
				return new BindingResult(BindingError.NotFound);
			}

			// Try and create a usable delegate from the method info object, then return that:
			System.Delegate listener = BindingListener.CreateDelegate(typeof(BindingListener), current, mInfo);
			if(listener == null)
			{
				Debug.LogError("[BindingExecutor] Error! Failed to create delegate from method '" +
					mInfo.Name + "'! Target method must match the 'BindingListener' specification!");
				return new BindingResult(BindingError.InvalidType);
			}

			// Call the method in question and return successful event resolution:
			BindingListener listenerMethod = (BindingListener)listener;
			listenerMethod(ref binding);
			return new BindingResult(true);
		}

		private BindingResult useBindingField(object current, StringBuilder pathBuffer, int bufferIndex, ref Binding binding)
		{
			System.Type currentType = current.GetType();

			string fBufferString = pathBuffer.ToString().Substring(0, bufferIndex);

			// Check for any fitting properties first:
			PropertyInfo pInfo = currentType.GetProperty(fBufferString);
			if(pInfo != null)
			{
				System.Type pType = pInfo.PropertyType;
				if(pType == binding.eventValue.GetType())
				{
					pInfo.SetValue(current, binding.eventValue, null);
					return new BindingResult(true);
				}
				else if(pType == binding.eventString.GetType())
				{
					pInfo.SetValue(current, binding.eventString, null);
					return new BindingResult(true);
				}

				if(pType != binding.eventObject.GetType())
				{
					Debug.LogError("[BindingExecutor] Error! Property '" + fBufferString + "' type mismatch with" +
						"provided value's type: " + pType.ToString()+" vs. "+binding.eventObject.GetType().ToString());
					return new BindingResult(BindingError.InvalidType);
				}

				pInfo.SetValue(current, binding.eventObject, null);
				return new BindingResult(true);
			}

			// If no property was found, search for matching fields next:
			FieldInfo fInfo = currentType.GetField(fBufferString, bindingFieldFlags);
			if(fInfo == null)
			{
				// No matching field was found, abort query:
				Debug.LogError("[BindingExecutor] Error! Field '" + fBufferString +
					"' not found in type '" + currentType.Name + "'! Aborting path resolution!");
				return new BindingResult(BindingError.NotFound);
			}

			System.Type fType = fInfo.FieldType;
			if(fType == binding.eventValue.GetType())
			{
				fInfo.SetValue(current, binding.eventValue);
				return new BindingResult(true);
			}
			else if(fType == binding.eventString.GetType())
			{
				fInfo.SetValue(current, binding.eventString);
				return new BindingResult(true);
			}

			if(fType != binding.eventObject.GetType())
			{
				Debug.LogError("[BindingExecutor] Error! Field '" + fBufferString + "' type mismatch with" +
					"provided value's type: " + fType.ToString()+" vs. "+binding.eventObject.GetType().ToString());
				return new BindingResult(BindingError.InvalidType);
			}

			// Set the field on the object at hand:
			fInfo.SetValue(current, binding.eventObject);
			return new BindingResult(true);
		}

		/// <summary>
		/// Call to execute a binding. This will first verify the binding, then resolve its path and
		/// finally try to execute the binding by either assigning the value of a field, by setting
		/// a property or by calling a method on the target designated by the binding's path.
		/// </summary>
		/// <returns>A result containing an error code and success feedback.</returns>
		/// <param name="binding">The binding you wish to take effect.</param>
		/// <param name="core">The binding core object relative to which the path should be resolved.</param>
		public BindingResult executeBinding(ref Binding binding, IBindingCore core)
		{
			// Check the binding's parameter set:
			if(!verifyBinding(ref binding))
			{
				// Tell the author of this binding request that the operation was aborted:
				binding.responseCode = BindingResponse.Error;

				return new BindingResult(BindingError.InvalidBinding);
			}
			
			// NOTE: All methods callable via binding must match the 'UtmlBindingListener' delegate!
			
			// Try and find some dependency via reflection using the hierarchy given by the path:
			BindingResult result = resolveBindingPath(ref binding, core);
			if(result.error != BindingError.Success)
			{
				// Tell the author of this binding request that an error occurred:
				binding.responseCode = BindingResponse.Error;

				// Invalid or unknown binding, abort event call:
				Debug.LogError("[BindingExecutor] Error! Path resolution for binding '" + binding.path + "' failed!");
			}

			return result;
		}
		
		#endregion
	}
}
