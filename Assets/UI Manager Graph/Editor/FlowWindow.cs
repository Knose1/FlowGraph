using Com.Github.Knose1.Flow.Editor.Node;
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow.Editor
{

	public class FlowWindow : EditorWindow
	{

		private const string MULTIPLE_EDIT_NOT_SUPPORTED = "Can't Edit Multiple objects";
		private const string ASSTET_NOT_SELECTED = "Can't Edit Multiple objects";

		protected FlowGraph graph;
		protected FlowGraphManager manager;

		[MenuItem("Window/Game/Flow")]
		static private void Init()
		{
			FlowWindow window = GetWindow<FlowWindow>();
			window.Show();
		}

		public virtual void OnEnable()
		{
			OnDisable(); //Just In Case

			manager = new FlowGraphManager();
			manager.OnSelectionStatusChange += Manager_OnSelectionStatusChange;

			GenerateGraph();
			GenerateToolbar();

			manager.Init();
		}

		private void Manager_OnSelectionStatusChange(FlowGraphManager.Status status)
		{
			switch (status)
			{
				case FlowGraphManager.Status.NoProblem:
					break;
				case FlowGraphManager.Status.MultipleEdit:
					break;
				case FlowGraphManager.Status.NotSelected:
					break;
			}
		}

		protected virtual void GenerateGraph()
		{
			titleContent = new GUIContent(nameof(FlowWindow));
			graph = new FlowGraph(manager)
			{
				name = titleContent.text
			};

			rootVisualElement.Add(graph);
			graph.StretchToParentSize();
		}

		protected virtual void GenerateToolbar()
		{
			Toolbar toolbar = new Toolbar();
			toolbar.StretchToParentWidth();

			//Screen Node
			ToolbarButton createScreenNode = new ToolbarButton(CreateStateNode);
			createScreenNode.text = "+State Node";
			toolbar.Add(createScreenNode);

			//Middle
			VisualElement middle = new VisualElement();
			middle.name = "Middle";
			toolbar.Add(middle);

			middle.style.flexGrow = 1;

			//Minimap
			ToolbarToggle minimapToggle = new ToolbarToggle();
			minimapToggle.RegisterValueChangedCallback(MinimapToggleChange);
			minimapToggle.text = "Toggle minimap";
			toolbar.Add(minimapToggle);

			middle.style.flexGrow = 1;

			//Save
			ToolbarButton save = new ToolbarButton(Save);
			save.text = "Save";
			toolbar.Add(save);


			rootVisualElement.Add(toolbar);
		}

		private void MinimapToggleChange(ChangeEvent<bool> evt)
		{
			graph.ToggleMinimap(evt.newValue);
		}

		protected void Save()
		{
			Debug.Log("["+nameof(FlowWindow)+"] Saving...");
			graph.SaveIn(manager.Target);
			Debug.Log("[" + nameof(FlowWindow) + "] Saved !");
		}

		#region Create
		void CreateStateNode()
		{
			graph.CreateNode(new StateNode());
		}

		void CreateConditionNode()
		{
			graph.CreateNode(new ConditionNode());
		}
		#endregion

		public void OnDisable()
		{
			if (manager != null) manager.OnSelectionStatusChange -= Manager_OnSelectionStatusChange;

			if (graph != null) rootVisualElement.Remove(graph);
			
			if (graph != null) graph.Dispose();
			if (manager != null) manager.Dispose();

		}
	}
}