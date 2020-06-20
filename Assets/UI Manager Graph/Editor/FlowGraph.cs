using Com.Github.Knose1.Flow.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow
{
	/// <summary>
	/// Flow graph, you can place <see cref="FlowGraphNode"/>s on it, move the nodes, link them etc...<br/>
	/// <br/>
	/// There is also a <see cref="FlowGraph.Save()"/> method to save the graph
	/// </summary>
	public class FlowGraph : GraphView
	{
		/// <summary>
		/// The stylesheet name
		/// </summary>
		private const string RESSOURCE_STYLESHEET = "Graph";

		//Some overrides
		protected override bool canCopySelection
		{
			get
			{
				return selection.Count > 0 && !selection.Contains(entryNode) && !selection.Contains(exitNode);
			}
		}
		protected override bool canCutSelection
		{
			get
			{
				for (int i = selection.Count - 1; i >= 0; i--)
				{
					FlowGraphNode node = selection[i] as FlowGraphNode;

					if (node.capabilities != Capabilities.Deletable) return false;
				}

				return true;
			}
		}
		protected override bool canPaste
		{
			get
			{
				return true;
			}
		}
		protected override bool canDuplicateSelection
		{
			get
			{
				return canCopySelection;
			}
		}


		/*//////////////////////////////////////*/
		/*                                      */
		/*                Fields                */
		/*                                      */
		/*//////////////////////////////////////*/
		protected EntryNode entryNode = null;
		protected ExitNode exitNode = null;



		public FlowGraph(FlowGraphManager manager) : base()
		{
			SetupGraph();

			manager.OnDataChange += Manager_OnDataChange;

			/* Generate new graph
			try
			{
				AddElement(entryNode = GenerateEntryPointNode());
				AddElement(exitNode = GenerateExitPointNode());
			}
			catch (Exception err)
			{
				Debug.LogError(err);
				Debug.LogWarning("[" + nameof(FlowGraph) + "] An error occured when Generating the graph");
			}*/
		}

		private void Manager_OnDataChange()
		{
			//Generate Graph
			throw new NotImplementedException();
		}

		private void SetupGraph()
		{
			StyleSheet styleSheet = null;
			try
			{
				//Load
				styleSheet = Resources.Load<StyleSheet>(RESSOURCE_STYLESHEET);
			}
			catch (Exception e)
			{
				Debug.LogError(e);
				Debug.LogWarning("[" + nameof(FlowGraph) + "] error loading the Stylesheet, filename : Ressource/" + RESSOURCE_STYLESHEET);
			}

			try
			{
				if (styleSheet != null) styleSheets.Add(styleSheet);
			}
			catch (Exception e)
			{
				Debug.LogError(e);
				Debug.LogWarning("[" + nameof(FlowGraph) + "] error parsing the Stylesheet, filename : Ressource/" + RESSOURCE_STYLESHEET);

				styleSheets.Remove(styleSheet);
			}

			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());

			GridBackground gridBackground = new GridBackground();
			Insert(0, gridBackground);
			gridBackground.StretchToParentSize();
		}

		public FlowGraphNode CreateNode(FlowGraphNode node)
		{
			AddElement(node);

			return node;
		}

		private EntryNode GenerateEntryPointNode()
		{
			EntryNode node = new EntryNode();

			node.SetPosition(new Rect(100, 200, 100, 150));

			return node;
		}

		private ExitNode GenerateExitPointNode()
		{
			ExitNode node = new ExitNode();

			node.SetPosition(new Rect(500, 200, 100, 150));

			return node;
		}

		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			List<Port> compatiblePorts = new List<Port>();
			List<Port> ports = base.ports.ToList();
			
			foreach (Port port in ports)
			{
				List<Edge> connections = port.connections.ToList();
				bool isAlreadyConnected = connections.Contains(startPort.edgeConnector.edgeDragHelper.edgeCandidate);

				if (startPort != port && startPort.node != port.node && !isAlreadyConnected)
					compatiblePorts.Add(port);
			}

			return compatiblePorts;
		}
	}
}