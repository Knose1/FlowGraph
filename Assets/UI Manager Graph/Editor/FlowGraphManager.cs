using Com.Github.Knose1.Flow.Engine.Settings;
using System;
using UnityEditor;

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

		public void Init()
		{
			Selection.selectionChanged += Selection_SelectionChanged;

			Selection_SelectionChanged();
		}

		public void CreateAsset()
		{
			throw new NotImplementedException();
		}

		private void Selection_SelectionChanged()
		{
			_target = null;

			int length = Selection.assetGUIDs.Length;
			if (length > 1)
			{
				OnSelectionStatusChange?.Invoke(Status.MultipleEdit);
				OnDataChange?.Invoke();
				return;
			}
			if (length == 0)
			{
				OnSelectionStatusChange?.Invoke(Status.NotSelected);
				OnDataChange?.Invoke();
				return;
			}

			UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]));
			if (!(obj is FlowGraphScriptable))
			{
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
