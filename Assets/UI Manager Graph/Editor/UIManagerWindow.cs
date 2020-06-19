using Com.Github.Knose1.UiManagerGraph.Node;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.UiManagerGraph {

	public class UIManagerWindow : EditorWindow {
		protected UIManagerGraph graph;

		[MenuItem("Window/Game/UIManager")]
		static private void Init()
		{
			UIManagerWindow window = GetWindow<UIManagerWindow>();
			window.Show();
		}

		public virtual void OnEnable()
		{
			GenerateGraph();
			GenerateToolbar();
		}

		protected virtual void GenerateGraph()
		{
			titleContent = new GUIContent(nameof(UIManagerWindow));
			graph = new UIManagerGraph
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
			Button createScreenNode = new Button(CreateScreenNode);
			createScreenNode.text = "+Screen Node";
			toolbar.Add(createScreenNode);
			
			//Screen Node
			Button createConditionNode = new Button(CreateConditionNode);
			createConditionNode.text = "+Condition Node";
			toolbar.Add(createConditionNode);

			rootVisualElement.Add(toolbar);
		}

		#region
		void CreateScreenNode() 
		{ 
			graph.CreateNode(new ScreenNode()); 
		}

		void CreateConditionNode() 
		{ 
			graph.CreateNode(new ConditionNode()); 
		}
		#endregion
		
		public void OnDisable()
		{
			rootVisualElement.Remove(graph);
		}
	}
}