#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace CommonUnity.Deprecated.Editor
{
	public class CustomDrawer : PropertyDrawer
	{
		/*
			Enums
		*/

		protected enum Style { None, Horizontal = 1, Vertical = 2, FieldIsTitle = 4, BlankTitle = 8 };

		/*
			Constant members
		*/

		protected const int k_LineHeight = 16;
		protected const int k_LinePadding = 2;
		protected const int k_FieldHeight = k_LineHeight + k_LinePadding;
		protected const int k_BreakHeight = k_FieldHeight / 2;

		/*
			Inherited properties
		*/

		private int m_TitleIndent = 1;
		protected int TitleIndent { get { return m_TitleIndent; } set { m_TitleIndent = value; } }

		private int m_FieldIndent = -1;
		protected int FieldIndent { get { return m_FieldIndent; } set { m_FieldIndent = value; } }

		/*
			Inherited methods
		*/

		protected bool Display(ref Rect line, Style style, string title, SerializedProperty property, params string[] fields)
		{
			if (fields.Length == 0)
			{
				return BaseDisplay(ref line, style, title, property);
			}
			else if (fields.Length == 1)
			{
				return BaseDisplay(ref line, style, title, property.FindPropertyRelative(fields[0]));
			}

			if ((style & Style.Horizontal) == Style.Horizontal)
			{
				bool retval = true;

				// horizontal
				Rect rect = new Rect(line.x, line.y, line.width / fields.Length, line.height);
				if (title != null)
				{
					rect = EditorGUI.PrefixLabel(line, new GUIContent(title));
					rect.width /= fields.Length;
				}

				foreach (string field in fields)
				{
					retval &= Display(rect, Style.None, property, field);
					rect.x += line.width;
				}

				line.y += k_FieldHeight;

				return retval;
			}
			else if ((style & Style.Vertical) == Style.Vertical)
			{
				bool retval = true;

				// vertical 
				foreach (string field in fields)
				{
					retval &= Display(ref line, style, title, property, field);
				}

				return retval;
			}

			return false;
		}

		protected bool Display(ref Rect line, Style style, SerializedProperty property, params string[] fields)
		{
			return Display(ref line, style, title: null, property: property, fields: fields);
		}

		protected bool Display2(ref Rect line, Style style, string[] titles, SerializedProperty property, params string[] fields)
		{
			bool retval = true;

			// vertical 
			int i = 0;
			foreach (string field in fields)
			{
				retval &= Display(ref line, style, titles[i++], property, field);
			}

			return retval;
		}

		/*
			Pass rect by value
		*/

		protected bool Display(Rect line, Style style, SerializedProperty property, params string[] fields)
		{
			return Display(ref line, style, property, fields);
		}

		protected bool Display(Rect line, Style style, string title, SerializedProperty property, params string[] fields)
		{
			return Display(ref line, style, title, property, fields);
		}

		protected bool Display(Rect line, Style style, string[] titles, SerializedProperty property, params string[] fields)
		{
			return Display2(ref line, style, titles, property, fields);
		}

		/*
			Private methods
		*/

		private bool BaseDisplay(ref Rect line, Style style, string title, SerializedProperty property)
		{
			EditorGUI.indentLevel += TitleIndent;
			Rect rect = line;
			if ((style & Style.FieldIsTitle) == Style.FieldIsTitle)
			{
				rect = EditorGUI.PrefixLabel(line, new GUIContent(Nice(property.name)));
			}
			else if ((style & Style.BlankTitle) == Style.BlankTitle)
			{
				rect = EditorGUI.PrefixLabel(line, new GUIContent(" "));
			}
			else if (title != null)
			{
				rect = EditorGUI.PrefixLabel(line, new GUIContent(title));
			}

			EditorGUI.indentLevel -= TitleIndent;
			EditorGUI.indentLevel += FieldIndent;
			bool success = EditorGUI.PropertyField(rect, property, GUIContent.none);
			EditorGUI.indentLevel -= FieldIndent;

			line.y += k_FieldHeight;

			return success;
		}

		private string Nice(string text)
		{
			for (int i = 1; i < text.Length; i++)
			{
				if (char.IsUpper(text[i]))
				{
					text = text.Insert(i++, " ");
				}
			}

			return char.ToUpper(text[0]) + text.Substring(1);
		}
	}
}

#endif