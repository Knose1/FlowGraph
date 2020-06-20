using Com.Github.Knose1.Flow.Node;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow {

	public class FlowWindow : EditorWindow {

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
			manager = new FlowGraphManager();

			manager.OnSelectionStatusChange += Manager_OnSelectionStatusChange;
			manager.Init();

			GenerateGraph();
			GenerateToolbar();
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

			//Screen Node
			Button createScreenNode = new Button(CreateStateNode);
			createScreenNode.text = "+State Node";
			toolbar.Add(createScreenNode);
			
			rootVisualElement.Add(toolbar);
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
			rootVisualElement.Remove(graph);
			manager.Dispose();
		}
	}
}