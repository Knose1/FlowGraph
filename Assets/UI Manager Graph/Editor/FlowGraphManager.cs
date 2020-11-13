using Com.Github.Knose1.Flow.Editor.Generate;
using Com.Github.Knose1.Flow.Engine.Settings;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Editor
{
	/// <summary>
	/// A class that check when the selection change and that regroup some toolbar functions (for example GenerateCode)
	/// </summary>
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

		/// <summary>
		/// Create scriptable object
		/// </summary>
		public void CreateAsset()
		{
			string path = EditorUtility.SaveFilePanel(
				title:"Create Asset", "", nameof(FlowGraphScriptable), "asset"
			);
			string assetPath = "Assets"+path.Replace(Application.dataPath, "");

			FlowGraphScriptable asset = ScriptableObject.CreateInstance<FlowGraphScriptable>();
			AssetDatabase.CreateAsset(asset, assetPath);

			EditorGUIUtility.PingObject(asset);
			Selection.activeObject = asset;
			_target = asset;

			OnSelectionStatusChange?.Invoke(Status.NoProblem);
			OnDataChange?.Invoke();
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
			string assetPath = "Assets"+path.Replace(Application.dataPath, "");

			if (path == "")
			{
				Debug.Log(DEBUG_PREFIX + " Canceled");
				return;
			}

			string classTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(FlowGraphAssetDatabase.CLASS_TEMPLATE).text;
			string substateClassTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(FlowGraphAssetDatabase.SUBSTATE_CLASS_TEMPLATE).text;
			string argsTemplate = AssetDatabase.LoadAssetAtPath<TextAsset>(FlowGraphAssetDatabase.ARGS_TEMPLATE).text;
			string code = GraphCodeGenerator.Generate(classTemplate, substateClassTemplate, JsonUtility.FromJson<TemplateJsonData>(argsTemplate), _target.nodes);

			if (File.Exists(path))
			{
				AssetDatabase.DeleteAsset(assetPath);
				AssetDatabase.Refresh();
			}

			StreamWriter streamWriter = File.CreateText(path);
			streamWriter.Write(code);
			streamWriter.Close();

			Debug.Log(assetPath);

			AssetDatabase.Refresh();
			EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath));

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
