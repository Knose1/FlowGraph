﻿using Com.Github.Knose1.Flow.Editor.Node;
using Com.Github.Knose1.Flow.Editor.WindowElements;
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow.Editor
{
	/// <summary>
	/// The window for the FlowGraph
	/// </summary>
	public class FlowWindow : EditorWindow
	{

		private const string MULTIPLE_EDIT_NOT_SUPPORTED = "Can't Edit Multiple objects";
		private const string ASSTET_NOT_SELECTED = "Can't Edit Multiple objects";
		private const string TITLE = nameof(FlowWindow);
		private const string STATE_NODE = "+State Node";
		private const string REROUTE = "+Reroute";
		private const string EXIT = "+Exit Node";
		private const string MINIMAP = "Toggle minimap";
		private const string SAVE = "Save";
		
		protected FlowGraph graph;
		protected FlowGraphManager manager;
		private bool isDirty = false;
		protected ToolbarButton save;
		private TriggerList listView;

		[MenuItem("Window/Game/Flow")]
		static public void Open()
		{
			FlowWindow window = GetWindow<FlowWindow>();
			window.Show();
		}

		public virtual void OnEnable()
		{
			OnDisable(); //Just In Case

			if (manager == null)
				manager = new FlowGraphManager();
			
			manager.OnSelectionStatusChange += Manager_OnSelectionStatusChange;
			
			GenerateGraph();
			//GenerateTriggerList();
			GenerateToolbar();

			manager.OnSaving += Manager_OnSaving;
			manager.Init();

			rootVisualElement.RegisterCallback<KeyDownEvent>(OnKeyDown);
		}


		private void OnKeyDown(KeyDownEvent evt)
		{
			if (evt.keyCode == KeyCode.S && evt.ctrlKey)
			{
				manager.Save();
			} 
		}

		protected virtual void GenerateGraph()
		{
			titleContent = new GUIContent(TITLE);
			graph = new FlowGraph(manager)
			{
				name = titleContent.text
			};

			rootVisualElement.Add(graph);
			graph.StretchToParentSize();
			graph.OnChange += Graph_OnGetDirty;
		}

		protected virtual void GenerateToolbar()
		{
			Toolbar toolbar = new Toolbar();
			toolbar.StretchToParentWidth();

			//Button Screen Node
			ToolbarButton createScreenNode = new ToolbarButton(CreateStateNode);
			createScreenNode.text = STATE_NODE;
			toolbar.Add(createScreenNode);

			//Button Reroute Node
			ToolbarButton createReroute = new ToolbarButton(CreateRetoute);
			createReroute.text = REROUTE;
			toolbar.Add(createReroute);

			//Button Exit Node
			ToolbarButton createExit = new ToolbarButton(CreateExit);
			createExit.text = EXIT;
			toolbar.Add(createExit);

			//Toggle Minimap
			ToolbarToggle minimapToggle = new ToolbarToggle();
			minimapToggle.RegisterValueChangedCallback(MinimapToggleChange);
			minimapToggle.text = MINIMAP;
			toolbar.Add(minimapToggle);

			//Middle
			VisualElement middle = new VisualElement();
			middle.name = "Middle";
			middle.style.flexGrow = 1;
			toolbar.Add(middle);

			//New Asset
			ToolbarButton newAsset = new ToolbarButton(manager.CreateAsset);
			newAsset.text = "New Graph";
			toolbar.Add(newAsset);

			//Generate
			ToolbarButton generate  = new ToolbarButton(manager.GenerateCode);
			generate.text = "Generate";
			toolbar.Add(generate);

			//Save
			save = new ToolbarButton(() => { manager.Save(); });
			save.text = SAVE;
			toolbar.Add(save);

			rootVisualElement.Add(toolbar);
		}

		protected virtual void GenerateTriggerList()
		{
			listView = new TriggerList();
			rootVisualElement.Add(listView);
		}

		private void MinimapToggleChange(ChangeEvent<bool> evt)
		{
			graph.ToggleMinimap(evt.newValue);
		}
		
		public void ShowAsDirty()
		{
			save.text = "*" + SAVE;
			titleContent.text = "*" + TITLE;
			manager.ShowAsDirty();
		}

		protected void Manager_OnSaving()
		{
			isDirty = false;

			Debug.Log("[" + nameof(FlowWindow) + "] Saving...");
			graph.SaveIn(manager.Target);
			save.text = SAVE;
			titleContent.text = TITLE;
			Debug.Log("[" + nameof(FlowWindow) + "] Saved !");
		}

		private void Manager_OnSelectionStatusChange(FlowGraphManager.Status status)
		{
			isDirty = false;
			save.text = SAVE;
			titleContent.text = TITLE;

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

		private void Graph_OnGetDirty()
		{
			if (isDirty) return;

			isDirty = true;
			ShowAsDirty();
		}

		#region Create
		private void CreateStateNode()
		{
			graph.AddNode(new StateNode());
		}

		private void CreateRetoute()
		{
			graph.CreateRetoute();
		}

		private void CreateExit()
		{
			graph.AddNode(new ExitNode());
		}

		private void CreateConditionNode()
		{
			//graph.AddNode(new ConditionNode());
		}
		#endregion

		public void OnDisable()
		{
			if (manager != null) manager.OnSelectionStatusChange -= Manager_OnSelectionStatusChange;

			if (graph != null) rootVisualElement.Remove(graph);
			
			if (graph != null) graph.Dispose();
			if (manager != null) manager.Dispose();


			rootVisualElement.UnregisterCallback<KeyDownEvent>(OnKeyDown);
		}
	}
}