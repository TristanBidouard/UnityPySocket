using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Jake.Tcp
{
	[CustomEditor(typeof(TcpConnection))]
	public class TcpConnectionEditor : Editor
	{
		TcpConnection tcpConnection;
		SerializedProperty autoConnect;
		SerializedProperty isServer;
		SerializedProperty ipAddress;
		SerializedProperty port;
		SerializedProperty useJson;
		SerializedProperty jsonFile;

		bool jsonLoaded;

		void OnEnable()
		{
			tcpConnection = target as TcpConnection;
			autoConnect		= serializedObject.FindProperty("autoConnect");
			isServer		= serializedObject.FindProperty("isServer");
			ipAddress		= serializedObject.FindProperty("ipAddress");
			port			= serializedObject.FindProperty("port");
			useJson			= serializedObject.FindProperty("useJson");
			jsonFile		= serializedObject.FindProperty("jsonFile");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			GUI.enabled = false;
			EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(target as TcpConnection), typeof(TcpConnection), false);
			GUI.enabled = true;

			EditorGUILayout.PropertyField(autoConnect);
			EditorGUILayout.PropertyField(isServer);

			if (tcpConnection.useJson)
			{
				if (!jsonLoaded)
				{
					try
					{
						tcpConnection.LoadJson();
						jsonLoaded = true;
					}
					catch
					{

					}
				}

				GUI.enabled = false;
			}
			else
			{
				jsonLoaded = false;
			}

			EditorGUILayout.PropertyField(ipAddress);
			EditorGUILayout.PropertyField(port);

			if (tcpConnection.useJson)
			{
				GUI.enabled = true;
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(useJson);

			if (tcpConnection.useJson)
			{
				GUILayout.FlexibleSpace();
				EditorGUILayout.PropertyField(jsonFile, GUIContent.none);
			}

			EditorGUILayout.EndHorizontal();

			serializedObject.ApplyModifiedProperties();
		}
	}
}
