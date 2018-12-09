#if UNITY_EDITOR

using CommonCS.IEnumerables;
using CommonCS.Types;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using CommonUnity.Deprecated.Editor;

namespace CommonUnity.Events.Editor
{
	[CustomPropertyDrawer(typeof(Event))]
	public class EventDrawer : CustomDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, k_FieldHeight), property.FindPropertyRelative("enabled"), new GUIContent(" "));

			// put help hint here

			bool fold = property.FindPropertyRelative("fold").boolValue;
			string name = property.FindPropertyRelative("name").stringValue;
			if (EditorGUI.Foldout(new Rect(position.x, position.y, 16, 16), fold, name))
			{
				// property was folded
				property.FindPropertyRelative("fold").boolValue = true;

				// prepare rect
				Rect rect = new Rect(position.x, position.y + k_FieldHeight, position.width, k_LineHeight);

				// ask for name and trigger type
				Display(ref rect, Style.FieldIsTitle, property, "name");

				EditorGUI.indentLevel++;
				if (EditorGUI.Foldout(new Rect(rect.x, rect.y, 64, 16), property.FindPropertyRelative("triggerFold").boolValue, "Trigger"))
				{
					EditorGUI.indentLevel--;

					property.FindPropertyRelative("triggerFold").boolValue = true;

					Display(ref rect, Style.BlankTitle, property, "trigger");

					// display options depending on trigger type
					switch (property.FindPropertyRelative("trigger").enumValueIndex)
					{
						case 0:
							Display(ref rect, Style.BlankTitle, property, "keyCode");
							Display(ref rect, Style.BlankTitle, property, "keyPressType");
							break;
						case 1: Display(ref rect, Style.Vertical | Style.BlankTitle, property, "collider", "colliderEvent"); break;
						case 2: Display(ref rect, Style.BlankTitle, property, "time"); break;
						case 3: DisplayUnityEvent(ref rect, property); break;
						case 4: DisplayCSharpEvent(ref rect, property); break;
						case 5: Display(ref rect, Style.Vertical | Style.BlankTitle, property, "monoBehaviourTarget", "monoBehaviourEvent"); break;
						case 6:
							DisplayPipedAction(ref rect, property);
							break;
					}

					EditorGUI.indentLevel--;
				}
				else
				{
					EditorGUI.indentLevel -= 2;
					property.FindPropertyRelative("triggerFold").boolValue = false;

					DisplayTriggerLabel(ref rect, property);

					rect.y += k_FieldHeight;
				}

				EditorGUI.indentLevel = 2;
				EditorGUI.PropertyField(rect, property.FindPropertyRelative("actions"), true);

				if (!property.FindPropertyRelative("actions").isExpanded)
				{
					EditorGUI.LabelField(rect, " ", property.FindPropertyRelative("actions").arraySize.ToString());
				}

			}
			else
			{
				// property was not folded
				property.FindPropertyRelative("fold").boolValue = false;

				string topLabel = "Actions: " + property.FindPropertyRelative("actions").arraySize.ToString() + ", Trigger: " + GetTriggerLabel(property);

				EditorGUI.LabelField(new Rect(position.x + 16, position.y, position.width, k_FieldHeight), " ", topLabel);
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float retval = 0;

			//Actions title field
			retval += k_FieldHeight;

			if (property.FindPropertyRelative("actions").isExpanded)
			{
				retval += k_FieldHeight;
				retval += k_FieldHeight * 4 * property.FindPropertyRelative("actions").arraySize;
			}

			// trigger (1 to 3 lines)
			retval += k_FieldHeight * 2;

			if (property.FindPropertyRelative("triggerFold").boolValue)
			{
				retval += k_FieldHeight * 2;
			}

			// name
			retval += k_FieldHeight;

			// reset if collapsed at top
			if (!property.FindPropertyRelative("fold").boolValue)
			{
				retval = k_LineHeight;
			}

			return retval;
		}

		private void DisplayUnityEvent(ref Rect rect, SerializedProperty property)
		{
			int unityIndex = property.FindPropertyRelative("unityIndex").intValue;
			int unityEventIndex = property.FindPropertyRelative("unityEventIndex").intValue;

			SerializedProperty unityEventTarget = property.FindPropertyRelative("unityEventTarget");

			Display(ref rect, Style.None, " ", unityEventTarget);

			GameObject gameObject = unityEventTarget.objectReferenceValue as GameObject;
			if (gameObject != null)
			{
				EditorGUI.indentLevel++;
				Rect rect2 = EditorGUI.PrefixLabel(rect, new GUIContent(" "));

				rect2.width /= 2;

				string[] components = gameObject.GetComponents<Component>().ToStringArray((c) => { return c.GetType().Name; });
				EditorGUI.indentLevel--;
				EditorGUI.indentLevel--;
				unityIndex = EditorGUI.Popup(rect2, unityIndex, components);
				rect.y += k_FieldHeight;

				Component component = null;
				if (unityIndex < gameObject.GetComponents<Component>().Length)
				{
					component = gameObject.GetComponents<Component>()[unityIndex];
				}
				else
				{
					return;
				}

				string[] unityEvents = component.GetType().GetFields().GetEach((fi) =>
				{
					return fi.FieldType.IsOrInheritsFrom<UnityEvent>() || fi.FieldType.IsOrInheritsFrom<UnityEvent<bool>>()
					|| fi.FieldType.IsOrInheritsFrom<UnityEvent<float>>() || fi.FieldType.IsOrInheritsFrom<UnityEvent<int>>()
					|| fi.FieldType.IsOrInheritsFrom<UnityEvent<string>>() || fi.FieldType.IsOrInheritsFrom<UnityEvent<UnityEngine.Object>>();
				}).ToStringArray((fi) =>
				{
					return fi.Name;
				});

				rect2.x += rect2.width;

				unityEventIndex = EditorGUI.Popup(rect2, unityEventIndex, unityEvents);

				FieldInfo unityEventInfo = unityEvents.Length > 0 ? component.GetType().GetField(unityEvents[unityEventIndex]) : null;
				if (unityEventInfo != null)
				{
					property.FindPropertyRelative("unityEventComponent").objectReferenceValue = component;
					property.FindPropertyRelative("unityEventName").stringValue = unityEvents[unityEventIndex];
				}
			}

			property.FindPropertyRelative("unityIndex").intValue = unityIndex;
			property.FindPropertyRelative("unityEventIndex").intValue = unityEventIndex;
		}

		private void DisplayCSharpEvent(ref Rect rect, SerializedProperty property)
		{
			int csharpIndex = property.FindPropertyRelative("csharpIndex").intValue;
			int csharpEventIndex = property.FindPropertyRelative("csharpEventIndex").intValue;

			SerializedProperty csharpEventTarget = property.FindPropertyRelative("csharpEventTarget");

			Display(ref rect, Style.None, " ", csharpEventTarget);

			GameObject gameObject = csharpEventTarget.objectReferenceValue as GameObject;
			if (gameObject != null)
			{
				EditorGUI.indentLevel++;
				Rect rect2 = EditorGUI.PrefixLabel(rect, new GUIContent(" "));

				rect2.width /= 2;

				string[] components = gameObject.GetComponents<Component>().ToStringArray((c) => { return c.GetType().Name; });
				EditorGUI.indentLevel--;
				EditorGUI.indentLevel--;
				csharpIndex = EditorGUI.Popup(rect2, csharpIndex, components);
				rect.y += k_FieldHeight;

				Component component = gameObject.GetComponents<Component>()[csharpIndex];
				string[] csharpEvents = component.GetType().GetEvents().ToStringArray((ei) =>
				{
					return ei.Name;
				});

				rect2.x += rect2.width;

				csharpEventIndex = EditorGUI.Popup(rect2, csharpEventIndex, csharpEvents);
				EditorGUI.indentLevel++;

				if (csharpEvents.Length > 0 && component.GetType().GetEvent(csharpEvents[csharpEventIndex]) != null)
				{
					property.FindPropertyRelative("csharpEventComponent").objectReferenceValue = component;
					property.FindPropertyRelative("csharpEventName").stringValue = csharpEvents[csharpEventIndex];
				}
			}

			property.FindPropertyRelative("csharpIndex").intValue = csharpIndex;
			property.FindPropertyRelative("csharpEventIndex").intValue = csharpEventIndex;
		}

		private void DisplayTriggerLabel(ref Rect rect, SerializedProperty property)
		{
			EditorGUI.LabelField(rect, " ", GetTriggerLabel(property));
		}

		private string GetTriggerLabel(SerializedProperty property)
		{
			string label = "";
			switch (property.FindPropertyRelative("trigger").enumValueIndex)
			{
				case 0:
					int kc = property.FindPropertyRelative("keyCode").enumValueIndex;
					int pt = property.FindPropertyRelative("keyPressType").enumValueIndex;
					label = property.FindPropertyRelative("keyPressType").enumDisplayNames[pt] + " " +
						property.FindPropertyRelative("keyCode").enumDisplayNames[kc] + " key";
					break;
				case 1:
					int ce = property.FindPropertyRelative("colliderEvent").enumValueIndex;
					Collider collider = property.FindPropertyRelative("collider").objectReferenceValue as Collider;

					if (collider != null)
						label = property.FindPropertyRelative("colliderEvent").enumDisplayNames[ce].ToString() + " " + collider.name;
					break;
				case 2:
					label = property.FindPropertyRelative("time").floatValue.ToString() + " seconds";
					break;
				case 3:
					GameObject target = property.FindPropertyRelative("unityEventTarget").objectReferenceValue as GameObject;
					Component component = property.FindPropertyRelative("unityEventComponent").objectReferenceValue as Component;
					string eventName = property.FindPropertyRelative("unityEventName").stringValue;

					if (target != null && component != null)
					{
						label = target.name + ":" + component.GetType().Name + "." + eventName;
					}
					break;
				case 4:
					GameObject target2 = property.FindPropertyRelative("csharpEventTarget").objectReferenceValue as GameObject;
					Component component2 = property.FindPropertyRelative("csharpEventComponent").objectReferenceValue as Component;
					string eventName2 = property.FindPropertyRelative("csharpEventName").stringValue;

					if (target2 != null && component2 != null)
					{
						label = target2.name + ":" + component2.GetType().Name + "." + eventName2;
					}
					break;
				case 5:
					GameObject target3 = property.FindPropertyRelative("monoBehaviourTarget").objectReferenceValue as GameObject;
					int mbe = property.FindPropertyRelative("monoBehaviourEvent").enumValueIndex;
					if (target3 != null)
					{
						label = target3.name + ":." + property.FindPropertyRelative("monoBehaviourEvent").enumNames[mbe];
					}
					break;
				case 6:
					label = property.FindPropertyRelative("pipeEvent").stringValue + ", Action " + (property.FindPropertyRelative("pipeAction").intValue + 1).ToString();
					break;
			}

			return label;
		}

		private void DisplayPipedAction(ref Rect rect, SerializedProperty property)
		{
			SerializedProperty pipeTarget = property.FindPropertyRelative("pipeTarget");

			Display(ref rect, Style.BlankTitle, pipeTarget);

			EventHandler controller = pipeTarget.objectReferenceValue as EventHandler;

			// stop if gameobject hasn't been set
			if (controller == null)
				return;

			EditorGUI.indentLevel--;

			Rect rect2 = EditorGUI.PrefixLabel(rect, new GUIContent(" "));
			rect2.width -= 18;
			rect2.width /= 2;

			int pipeEventIndex = property.FindPropertyRelative("pipeEventIndex").intValue;
			string[] eventNames = controller.events.ToStringArray((e) =>
			{
				return e.name;
			});

			pipeEventIndex = EditorGUI.Popup(rect2, pipeEventIndex, eventNames);

			rect.y += k_FieldHeight;
			property.FindPropertyRelative("pipeEventIndex").intValue = pipeEventIndex;

			property.FindPropertyRelative("pipeEvent").stringValue = eventNames[pipeEventIndex];

			rect2.x += rect2.width;

			string[] actionNames = controller.events[pipeEventIndex].actions.ToStringArray((x, i) =>
			{
				return (i + 1).ToString();
			});

			int pipeActionIndex = property.FindPropertyRelative("pipeActionIndex").intValue;
			pipeActionIndex = EditorGUI.Popup(rect2, pipeActionIndex, actionNames);

			property.FindPropertyRelative("pipeActionIndex").intValue = pipeActionIndex;

			property.FindPropertyRelative("pipeAction").intValue = pipeActionIndex;

			EditorGUI.indentLevel++;
		}
	}
}

#endif