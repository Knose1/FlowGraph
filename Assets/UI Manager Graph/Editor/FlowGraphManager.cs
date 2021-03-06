﻿using Com.Github.Knose1.Flow.Editor.Generate;
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
		protected Status _currentStatus;

		public FlowGraphScriptable Target => _target;
		public Status CurrentStatus => _currentStatus;


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
				title:"Generate Script", folderArg, _target.EntryNode.stateClass, "cs"
			);
			string assetPath = "Assets"+path.Replace(Application.dataPath, "");

			if (path == "")
			{
				Debug.Log(DEBUG_PREFIX + " Canceled");
				return;
			}

			_target.nodes.AskForReloadList();
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
			string path = Path.Combine(Application.dataPath, FlowGraphAssetDatabase.GUID_TEXT_FILE.Substring(FlowGraphAssetDatabase.ASSET_ROOT_NAME.Length));
			string GUID = null;
			if (Selection.assetGUIDs.Length == 0)
			{
				if (_target != null) return;

				StreamReader streamReader = new StreamReader(path);
				GUID = streamReader.ReadToEnd();
				streamReader.Close();
			}
			else
			{
				GUID = Selection.assetGUIDs[0];
			}

			UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(GUID));
			if (!(obj is FlowGraphScriptable))
			{
				if (_target != null) return;

				_currentStatus = Status.NotSelected;
				OnSelectionStatusChange?.Invoke(_currentStatus);
				OnDataChange?.Invoke();
				return;
			}


			StreamWriter streamWriter = null;
			if (File.Exists(path))
			{
				streamWriter = new StreamWriter(path, false);
			}
			else
			{
				streamWriter = File.CreateText(path);
			}

			streamWriter.Write(GUID);
			streamWriter.Close();

			_target = obj as FlowGraphScriptable;
			OnSelectionStatusChange?.Invoke(_currentStatus);
			OnDataChange?.Invoke();
		}

		[Obsolete]
		private void Selection_SelectionChanged_Old()
		{
			int length = Selection.assetGUIDs.Length;
			if (length > 1)
			{
				_target = null;

				_currentStatus = Status.MultipleEdit;
				OnSelectionStatusChange?.Invoke(_currentStatus);
				OnDataChange?.Invoke();
				return;
			}

			if (length == 0)
			{
				if (_target != null) return;

				_currentStatus = Status.NotSelected;
				OnSelectionStatusChange?.Invoke(_currentStatus);
				OnDataChange?.Invoke();
				return;
			}

			UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]));
			if (!(obj is FlowGraphScriptable))
			{
				if (_target != null) return;

				_currentStatus = Status.NotSelected;
				OnSelectionStatusChange?.Invoke(_currentStatus);
				OnDataChange?.Invoke();
				return;
			}

			_target = obj as FlowGraphScriptable;

			_currentStatus = Status.NoProblem;
			OnSelectionStatusChange?.Invoke(_currentStatus);

			//Load asset

			OnDataChange?.Invoke();
		}

		public void Dispose()
		{
			Selection.selectionChanged -= Selection_SelectionChanged;
			OnDataChange = null;
			OnSelectionStatusChange = null;
			OnSaving = null;
		}
	}
}
