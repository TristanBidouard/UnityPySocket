#if UNITY_EDITOR

using CommonCS.IEnumerables;
using CommonCS.Types;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using CommonUnity.Deprecated.Editor;

namespace CommonUnity.Events.Editor
{
	[CustomPropertyDrawer(typeof(HandlerAction))]
	public class HandlerActionDrawer : CustomDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.indentLevel = 1;

			int componentIndex = property.FindPropertyRelative("componentIndex").intValue;
			int methodIndex = property.FindPropertyRelative("methodIndex").intValue;

			Rect rect = new Rect(position.x, position.y, position.width, k_LineHeight);

			TitleIndent = 2;
			Display(ref rect, Style.None, property.FindPropertyRelative("method").stringValue + " ", property.FindPropertyRelative("target"));
			TitleIndent = 0;

			GameObject gameObject = property.FindPropertyRelative("target").objectReferenceValue as GameObject;
			if (gameObject != null)
			{
				property.FindPropertyRelative("target").objectReferenceValue = gameObject;

				EditorGUI.indentLevel++;
				Rect rect2 = EditorGUI.PrefixLabel(rect, new GUIContent(" "));

				rect2.width -= 18;
				rect2.width /= 2;

				string[] components = gameObject.GetComponents<Component>().ToStringArray((c) => { return c.GetType().Name; });
				EditorGUI.indentLevel--;
				EditorGUI.indentLevel--;
				componentIndex = EditorGUI.Popup(rect2, componentIndex, components);
				rect.y += k_FieldHeight;

				Component component = null;

				if (componentIndex < gameObject.GetComponents<Component>().Length)
				{
					component = gameObject.GetComponents<Component>()[componentIndex];
				}
				else
				{
					EditorGUI.indentLevel++;
					return;
				}

				property.FindPropertyRelative("component").objectReferenceValue = component;

				MethodInfo[] methodInfos = component.GetType().GetMethods().GetEach((mi) =>
				{
					bool retval = char.IsUpper(mi.Name, 0) && mi.GetParameters().Length < 2;

					if (mi.GetParameters().Length == 1)
					{
						ParameterInfo pi = mi.GetParameters()[0];
						if (pi.ParameterType == typeof(bool) ||
							pi.ParameterType == typeof(float) ||
							pi.ParameterType == typeof(int) ||
							pi.ParameterType == typeof(string) ||
							pi.ParameterType.IsOrInheritsFrom<UnityEngine.Object>())
						{
							retval &= true;
						}
						else
						{
							retval &= false;
						}
					}

					retval &= !mi.IsGenericMethod;

					return retval;
				});

				string[] methods = methodInfos.ToStringArray();

				rect2.x += rect2.width;

				methodIndex = GetFakeMethodIndex(component, methodIndex);
				methodIndex = EditorGUI.Popup(rect2, methodIndex, methods);
				EditorGUI.indentLevel++;

				// parameter stuff
				if (methodInfos.Length > 0 && methodIndex <= methodInfos.Length)
				{
					MethodInfo methodInfo = methodInfos[methodIndex];

					property.FindPropertyRelative("method").stringValue = methodInfo.Name;

					rect.width -= 18;

					if (methodInfo.ReturnParameter.ParameterType == typeof(IEnumerator))
					{
						property.FindPropertyRelative("isIEnumerator").boolValue = true;
						Display(ref rect, Style.BlankTitle, property.FindPropertyRelative("methodType"));
					}
					else
					{
						property.FindPropertyRelative("isIEnumerator").boolValue = false;
					}

					var parameters = methodInfo.GetParameters();
					if (parameters.Length == 0)
					{
						property.FindPropertyRelative("argumentType").enumValueIndex = 0;
					}
					else if (parameters.Length == 1 && property.FindPropertyRelative("methodType").enumValueIndex != 2)
					{
						bool useArg = property.FindPropertyRelative("useArg").boolValue;

						rect.x += 18;
						rect.width -= 18;

						var paramInfo = parameters[0];
						if (paramInfo.ParameterType == typeof(bool))
						{
							property.FindPropertyRelative("argumentType").enumValueIndex = 1;
							if (useArg) Display(ref rect, Style.None, " ", property, "argBool");
						}
						else if (paramInfo.ParameterType == typeof(float))
						{
							property.FindPropertyRelative("argumentType").enumValueIndex = 2;
							if (useArg) Display(ref rect, Style.None, " ", property, "argFloat");
						}
						else if (paramInfo.ParameterType == typeof(int))
						{
							property.FindPropertyRelative("argumentType").enumValueIndex = 3;
							if (useArg) Display(ref rect, Style.None, " ", property, "argInt");
						}
						else if (paramInfo.ParameterType == typeof(string))
						{
							property.FindPropertyRelative("argumentType").enumValueIndex = 4;
							if (useArg) Display(ref rect, Style.None, " ", property, "argString");
						}
						else if (paramInfo.ParameterType.IsOrInheritsFrom<UnityEngine.Object>())
						{
							property.FindPropertyRelative("argumentType").enumValueIndex = 5;

							if (useArg)
							{
								rect.width += 18;

								var obj = property.FindPropertyRelative("argObject").objectReferenceValue;
								obj = EditorGUI.ObjectField(rect, " ", obj, paramInfo.ParameterType, true);
								property.FindPropertyRelative("argObject").objectReferenceValue = obj;
								rect.y += k_FieldHeight;
							}
						}

						if (!useArg)
						{
							rect.y += k_FieldHeight;
						}

						Display(new Rect(rect.x - 18, rect.y - 18, rect.width, rect.height), Style.BlankTitle, property, "useArg");
					}
				}

				property.FindPropertyRelative("componentIndex").intValue = componentIndex;
				property.FindPropertyRelative("methodIndex").intValue = GetRealMethodIndex(component, methodIndex);
			}

			EditorGUI.indentLevel += 2;
		}

		private int GetFakeMethodIndex(Component component, int realMethodIndex)
		{
			var result = 0;
			var methods = component.GetType().GetMethods();
			for (int i = 0; i < realMethodIndex; ++i)
			{

				if (UsableMethod(methods[i]))
					result++;
			}
			
			return result;
		}

		private int GetRealMethodIndex(Component component, int fakeMethodIndex)
		{
			var methods = component.GetType().GetMethods();
			for (int i = 0; i <= fakeMethodIndex; ++i)
			{
				if (!UsableMethod(methods[i]))
					fakeMethodIndex++;
			}
			
			return fakeMethodIndex;
		}

		private bool UsableMethod(MethodInfo method)
		{
			bool usableMethod = char.IsUpper(method.Name, 0) && method.GetParameters().Length < 2;

			if (method.GetParameters().Length == 1)
			{
				ParameterInfo pi = method.GetParameters()[0];
				if (pi.ParameterType == typeof(bool)	||
					pi.ParameterType == typeof(float)	||
					pi.ParameterType == typeof(int)		||
					pi.ParameterType == typeof(string)	||
					pi.ParameterType.IsOrInheritsFrom<UnityEngine.Object>())
				{
					usableMethod &= true;
				}
				else
				{
					usableMethod &= false;
				}
			}

			return usableMethod;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float retval = k_FieldHeight * 2;

			if (property.FindPropertyRelative("argumentType").enumValueIndex > 0)
			{
				retval += k_FieldHeight;
			}

			if (property.FindPropertyRelative("isIEnumerator").boolValue)
			{
				retval += k_FieldHeight;
			}

			return retval;
		}
	}
}

#endif