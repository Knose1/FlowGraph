using Com.Github.Knose1.UiManagerGraph.Node;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.UiManagerGraph
{
	public class UIManagerGraph : GraphView
	{
		private const string GRAPH = "Graph";

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
					UIManagerGraphNode node = selection[i] as UIManagerGraphNode;

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

		protected EntryNode entryNode = null;
		protected ExitNode exitNode = null;

		public UIManagerGraph() : base()
		{
			styleSheets.Add(Resources.Load<StyleSheet>(GRAPH));
			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());

			GridBackground gridBackground = new GridBackground();
			Insert(0, gridBackground);
			gridBackground.StretchToParentSize();

			try
			{
				AddElement(entryNode = GenerateEntryPointNode());
				AddElement(exitNode  = GenerateExitPointNode());
			}
			catch (Exception err)
			{
				Debug.LogError(err);
				Debug.LogWarning("An error occured when Generating the graph");
			}
		}

		public UIManagerGraphNode CreateNode(UIManagerGraphNode node)
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

		public void Save()
		{
		}
	}
}