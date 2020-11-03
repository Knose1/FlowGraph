using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Com.Github.Knose1.Flow.Editor.Generate
{
	/// <summary>
	/// Custom inspector for FlowGraphScriptable
	/// </summary>
	[CustomEditor(typeof(Engine.Settings.FlowGraphScriptable))]
	public class FlowGraphScriptableEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Open graph"))
			{
				FlowWindow.Open();
			}
		}
	}
}
