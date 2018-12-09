using System;
using System.Reflection;
using UnityEngine;

namespace Jake.RL
{
	using ArrayExtensions;
	using System.Threading;
	using Threading;

	[Serializable]
	public class Data
	{
		/*
		 *	Public fields
		 */

		/*
		 *	Static
		 */

		public static BindingFlags MemberFlags
		{
			get
			{
				return BindingFlags.Instance | BindingFlags.Public;
			}
		}

		/*
		 *	Instance
		 */

		public DataType type;
		public Source source;

		// source: field, method and property
		public GameObject gameObject;
		public Component component;

		// source: field
		public FieldInfo Field
		{
			get
			{
				return component.GetType().GetFields(MemberFlags).First((f) => f.ToString() == fieldSignature);
			}
		}

		// source: inspector
		public bool			boo;
		public Camera		cam;
		public float		flt;
		public int			i;
		public string		str;
		public Quaternion	qtn;
		public Vector3		vec;

		// source: method
		public MethodInfo Method
		{
			get
			{
				return component.GetType().GetMethods(MemberFlags).First((m) => m.ToString() == methodSignature);
			}
		}

		// source: property
		public PropertyInfo Property
		{
			get
			{
				return component.GetType().GetProperties(MemberFlags).First((p) => p.ToString() == propertySignature);
			}
		}

		// Drawer necessary fields
		public string fieldSignature, methodSignature, propertySignature;
		public bool folded;

		/*
		 *	Public methods
		 */

		/*
		 *	Static
		 */

		public static Type GetDataType(DataType type)
		{
			switch (type)
			{
				case DataType.None:			return null;
				case DataType.Bool:			return typeof(bool);
				case DataType.Camera:		return typeof(Camera);
				case DataType.Float:		return typeof(float);
				case DataType.Int:			return typeof(int);
				case DataType.Quaternion:	return typeof(Quaternion);
				case DataType.String:		return typeof(string);
				case DataType.Vector3:		return typeof(Vector3);
				default:
					Debug.LogError		("Data type not supported.");
					throw new Exception	("Data type not supported.");
			}
		}

		/*
		 *	Instance
		 */

		/// <summary>
		/// Get value
		/// </summary>
		public object GetValue()
		{
			switch (source)
			{
				case Source.Field:		return GetFieldValue();
				case Source.Inspector:	return GetInspectorValue();
				case Source.Method:		return GetMethodValue();
				case Source.Property:	return GetPropertyValue();
				default:
					Debug.LogError		("Source not supported.");
					throw new Exception	("Source not supported.");
			}
		}

		/// <summary>
		/// Set value
		/// </summary>
		public void SetValue(object value)
		{
			switch (source)
			{
				case Source.Field:		SetFieldValue(value);		break;
				case Source.Inspector:	SetInspectorValue(value);	break;
				case Source.Method:		SetMethodValue(value);		break;
				case Source.Property:	SetPropertyValue(value);	break;
				default:
					Debug.LogError		("Source not supported.");
					throw new Exception	("Source not supported.");
			}
		}

		/// <summary>
		/// Get value type
		/// </summary>
		public Type GetDataType()
		{
			return GetDataType(type);
		}
		
		/*
		 *	Private methods
		 */

		private object GetFieldValue()
		{
			return Field.GetValue(component);
		}

		private object GetInspectorValue()
		{
			switch (type)
			{
				case DataType.None:			return null;
				case DataType.Bool:			return boo;
				case DataType.Camera:		return cam;
				case DataType.Float:		return flt;
				case DataType.Int:			return i;
				case DataType.Quaternion:	return qtn;
				case DataType.String:		return str;
				case DataType.Vector3:		return vec;
				default:
					Debug.LogError		("Data type not supported.");
					throw new Exception	("Data type not supported.");
			}
		}

		private object GetMethodValue()
		{
			//if (Dispatcher.unityThreadOnlyTypes.Contains(component.GetType()))
			//{
			//	var value = DefaultValue();
			//	Dispatcher.AddJob(() =>
			//	{
			//		value = Method.Invoke(component, null);
			//	});

			//	return value;
			//}
			//else
			//{
				return Method.Invoke(component, null);
			//}
		}

		private object GetPropertyValue()
		{
			//if (Dispatcher.unityThreadOnlyTypes.Contains(component.GetType()))
			//{
			//	var value = DefaultValue();
			//	Dispatcher.AddJob(() =>
			//	{
			//		value = Property.GetValue(component, null);
			//	});

			//	return value;
			//}
			//else
			//{
				return Property.GetValue(component, null);
			//}
		}

		private void SetFieldValue(object value)
		{
			Field.SetValue(component, value);
		}

		private void SetInspectorValue(object value)
		{
			switch (type)
			{
				case DataType.None:										break;
				case DataType.Bool:			boo = (bool)		value;	break;
				case DataType.Camera:		cam = (Camera)		value;	break;
				case DataType.Float:		flt = (float)		value;	break;
				case DataType.Int:			i	= (int)			value;	break;
				case DataType.Quaternion:	qtn = (Quaternion)	value;	break;
				case DataType.String:		str = (string)		value;	break;
				case DataType.Vector3:		vec = (Vector3)		value;	break;
				default:
					Debug.LogError		("Data type not supported.");
					throw new Exception	("Data type not supported.");
			}
		}

		private void SetMethodValue(object value)
		{
			//if (Dispatcher.unityThreadOnlyTypes.Contains(component.GetType()))
			//{
			//	var parameters = Method.GetParameters();
			//	var requiresBlocker = parameters.Length == 1 && parameters[0].ParameterType == typeof(AutoResetEvent);
			//	if (requiresBlocker)
			//	{
			//		Dispatcher.AddJob((b) =>
			//		{
			//			Method.Invoke(component, new object[] { b });
			//		});
			//	}
			//	else
			//	{
			//		Dispatcher.AddJob(() =>
			//		{
			//			Method.Invoke(component, type == DataType.None ? null : new object[] { value });
			//		});
			//	}
			//}
			//else
			//{
				Method.Invoke(component, type == DataType.None ? null : new object[] { value });
			//}
		}

		private void SetPropertyValue(object value)
		{
			//if (Dispatcher.unityThreadOnlyTypes.Contains(component.GetType()))
			//{
			//	Dispatcher.AddJob(() =>
			//	{
			//		Property.SetValue(component, value, null);
			//	});
			//}
			//else
			//{
				Property.SetValue(component, value, null);
			//}
		}

		private object DefaultValue()
		{
			switch (type)
			{
				case DataType.None:			return null;
				case DataType.Bool:			return default(bool);
				case DataType.Camera:		return default(Camera);
				case DataType.Float:		return default(float);
				case DataType.Int:			return default(int);
				case DataType.Quaternion:	return default(Quaternion);
				case DataType.String:		return default(string);
				case DataType.Vector3:		return default(Vector3);
				default:
					Debug.LogError		("Data type not supported.");
					throw new Exception	("Data type not supported.");
			}
		}
	}
}
