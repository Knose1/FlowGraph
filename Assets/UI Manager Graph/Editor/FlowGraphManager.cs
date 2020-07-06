using Com.Github.Knose1.Flow.Engine.Settings;
using System;
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

		public event Action OnDataChange;
		public event Action<Status> OnSelectionStatusChange;

		protected FlowGraphScriptable _target;
		public FlowGraphScriptable Target => _target;

		public event Action OnSaving;

		public void Init()
		{
			Selection.selectionChanged += Selection_SelectionChanged;
			
			Selection_SelectionChanged();
		}

		public void ShowAsDirty()
		{
			EditorUtility.SetDirty(_target);
		}
		
		public void Save()
		{
			OnSaving?.Invoke();
			AssetDatabase.SaveAssets();
		}

		public void CreateAsset()
		{
			throw new NotImplementedException();
		}

		public void GenerateCode()
		{
			EditorUtility.SaveFilePanel(
				"", "", "", ".cs"
			);
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
