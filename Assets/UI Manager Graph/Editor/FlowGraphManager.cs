using System;
using UnityEditor;

namespace Com.Github.Knose1.Flow
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
			int length = Selection.assetGUIDs.Length;
			if (length > 0)
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
			if (!(obj is /*To complete*/))
			{
				OnSelectionStatusChange?.Invoke(Status.NotSelected);
				OnDataChange?.Invoke();
				return;
			}

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
