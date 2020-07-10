using Com.Github.Knose1.Flow.Editor.Generate;
using Com.Github.Knose1.Flow.Engine.Settings;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Editor
{
	public class FlowGraphManager : IDisposable
	{
		public enum Status
		{
			NoProblem,
			MultipleEdit,
			NotSelected
		}

		private const string DEBUG_PREFIX = "[" + nameof(FlowGraphManager) + "]";

		public event Action OnDataChange;
		public event Action<Status> OnSelectionStatusChange;
		public event Action OnSaving;

		protected FlowGraphScriptable _target;
		public FlowGraphScriptable Target => _target;


		public void Init()
		{
			Selection.selectionChanged += Selection_SelectionChanged;
			
			Selection_SelectionChanged();
		}

		public void ShowAsDirty()
		{
			EditorUtility.SetDirty(_target);
		}
		
		public bool Save()
		{
			if (!_target)
			{
				Debug.LogWarning(DEBUG_PREFIX+" There is no openned graph");
				return false;
			}
			OnSaving?.Invoke();
			AssetDatabase.SaveAssets();

			return true;
		}

		public void CreateAsset()
		{
			throw new NotImplementedException();
		}

		public void GenerateCode()
		{
			if (!Save()) 
			{
				Debug.Log(DEBUG_PREFIX+" Can't generate the code");
				return;
			}

			Debug.Log(DEBUG_PREFIX+" Start Generating...");
			
			if (_target.nodes.GetErrors())
			{
				Debug.Log(DEBUG_PREFIX + " Can't generate the code");
				return;
			}

			string folderArg = AssetDatabase.GetAssetPath(_target);
			folderArg = Path.GetDirectoryName(folderArg);

			string path = EditorUtility.SaveFilePanel(
				title:"Generate Script", folderArg, _target.EntryNode.@class, "cs"
			);

			if (path == "")
			{
				Debug.Log(DEBUG_PREFIX + " Canceled");
				return;
			}

			string classTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(FlowGraphAssetDatabase.CLASS_TEMPLATE).text;
			string argsTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(FlowGraphAssetDatabase.ARGS_TEMPLATE).text;
			string code = GraphCodeGenerator.Generate(classTemplate, JsonUtility.FromJson<TemplateJsonData>(argsTemplate), _target);

			Debug.Log(DEBUG_PREFIX + " Code has been generated");
		}

		private void Selection_SelectionChanged()
		{
			int length = Selection.assetGUIDs.Length;
			if (length > 1)
			{
				_target = null;

				OnSelectionStatusChange?.Invoke(Status.MultipleEdit);
				OnDataChange?.Invoke();
				return;
			}

			if (length == 0)
			{
				if (_target != null) return;

				OnSelectionStatusChange?.Invoke(Status.NotSelected);
				OnDataChange?.Invoke();
				return;
			}

			UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]));
			if (!(obj is FlowGraphScriptable))
			{
				if (_target != null) return;

				OnSelectionStatusChange?.Invoke(Status.NotSelected);
				OnDataChange?.Invoke();
				return;
			}

			_target = obj as FlowGraphScriptable;

			OnSelectionStatusChange?.Invoke(Status.NoProblem);

			//Load asset

			OnDataChange?.Invoke();
		}

		public void Dispose()
		{
			Selection.selectionChanged -= Selection_SelectionChanged;
			OnDataChange = null;
		}
	}
}
