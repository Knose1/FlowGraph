using Com.Github.Knose1.Flow.Editor.Node;
using Com.Github.Knose1.Flow.Engine.Settings;
using Com.Github.Knose1.Flow.Engine.Settings.NodeData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow.Editor
{
	/// <summary>
	/// Flow graph, you can place <see cref="FlowGraphNode"/>s on it, move the nodes, link them etc...<br/>
	/// <br/>
	/// There is also a <see cref="FlowGraph.Save()"/> method to save the graph
	/// </summary>
	public class FlowGraph : GraphView, IDisposable
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
		protected List<FlowGraphNode> graphNodes = new List<FlowGraphNode>();
		protected FlowGraphManager manager;


		/*//////////////////////////////////////*/
		/*                                      */
		/*             Constructor              */
		/*                                      */
		/*//////////////////////////////////////*/
		public FlowGraph(FlowGraphManager manager) : base()
		{
			SetupGraph();

			this.manager = manager;
			manager.OnDataChange += Manager_OnDataChange;

		}

		/*//////////////////////////////////////*/
		/*                                      */
		/*               Methods                */
		/*                                      */
		/*//////////////////////////////////////*/

		/// <summary>
		/// Save the graph in a <see cref="FlowGraphScriptable"/>
		/// </summary>
		/// <param name="target"></param>
		public void SaveIn(FlowGraphScriptable target)
		{
			List<UnityEditor.Experimental.GraphView.Node> nodes = this.nodes.ToList();

			//Empty override the save
			target.EmptyNodes();
			target.connections = new List<ConnectorData>();

			List<TempDataAndVisualLinkage> tempDataAndVisualLinkage = new List<TempDataAndVisualLinkage>();

			///Save nodes
			for (int i = graphNodes.Count - 1; i >= 0; i--)
			{
				FlowGraphNode graphNode = graphNodes[i];

				//Check if the node is on the canvas
				if (!nodes.Contains(graphNode))
				{
					graphNodes.Remove(graphNode);
					continue;
				}

				NodeData nodeData = null;

				nodeData = graphNode.Serialize();

				tempDataAndVisualLinkage.Insert(0, new TempDataAndVisualLinkage(nodeData, graphNode));

				target.UnshiftNode(nodeData);
			}

			///Save connections
			for (int i = 0; i < tempDataAndVisualLinkage.Count; i++)
			{
				FlowGraphNode inputGraphNode = tempDataAndVisualLinkage[i].graphNode;

				List<Port> ports = inputGraphNode.Ports;
				for (int j = 0; j < ports.Count; j++)
				{
					IEnumerator<Edge> inputConnections = ports[j].connections.GetEnumerator();
					while (inputConnections.MoveNext())
					{
						Edge edge = inputConnections.Current;

						Port inputPort = edge.input;
						Port outputPort = edge.output;

						FlowGraphNode inputNode = inputPort.node as FlowGraphNode;
						FlowGraphNode outputNode = outputPort.node as FlowGraphNode;

						NodeData inputNodeData = tempDataAndVisualLinkage[tempDataAndVisualLinkage.IndexOf(inputNode)].nodeData;
						NodeData outputNodeData = tempDataAndVisualLinkage[tempDataAndVisualLinkage.IndexOf(outputNode)].nodeData;

						int inputPortIndex = inputNode.Ports.IndexOf(inputPort);
						int outputPortIndex = outputNode.Ports.IndexOf(outputPort);

						ConnectorData connection = target.GetConnectorData(inputNodeData, inputPortIndex, outputNodeData, outputPortIndex);

						if (!target.IsConnectionRegistered(connection))
							target.connections.Add(connection);
					}
				}


			}
		}

		public FlowGraphNode CreateNode(FlowGraphNode node, bool relativePosition = true)
		{
			graphNodes.Add(node);
			AddElement(node);

			return node;
		}

		public void RemoveNode(FlowGraphNode node)
		{
			RemoveElement(node);
			graphNodes.Remove(node);
		}

		/// <summary>
		/// On Data change, we must load the graph
		/// </summary>
		private void Manager_OnDataChange()
		{
			entryNode = null;
			exitNode = null;
			DestroyGraphVisual();

			if (!manager.Target) return;
			manager.Target.GetNodes(out List<FlowGraphScriptable.NodeAndIndex> nodes);

			//Create New Graph
			if (nodes.Count < 2)
			{
				//Create New Graph
				GenerateNewGraph();
				return;
			}

			//Generate Graph From Datas
			GenerateGraphFromDatas(nodes);
		}

		private void DestroyGraphVisual()
		{
			for (int i = graphNodes.Count - 1; i >= 0; i--)
			{
				RemoveNode(graphNodes[i]);
			}
			
			//Remove all the other elements
			graphElements.ForEach(RemoveElement);

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

		private void GenerateNewGraph()
		{
			try
			{
				CreateNode(entryNode = GenerateEntryPointNode(), false);
				CreateNode(exitNode = GenerateExitPointNode(), false);
			}
			catch (Exception err)
			{
				Debug.LogError(err);
				Debug.LogWarning("[" + nameof(FlowGraph) + "] An error occured when Generating the graph");
			}
		}

		private void GenerateGraphFromDatas(List<FlowGraphScriptable.NodeAndIndex> nodes)
		{
			FlowGraphScriptable target = manager.Target;

			List<TempDataAndVisualLinkage> tempDataAndVisualLinkage = new List<TempDataAndVisualLinkage>();
			for (int i = 0; i < nodes.Count; i++)
			{
				FlowGraphScriptable.NodeAndIndex nodeAndIndex = nodes[i];

				NodeData node = null;

				FlowGraphNode flowGraphNode = null;
				switch (nodeAndIndex.type)
				{
					case NodeData.NodeType.Entry:

						node = target.entryNode[nodeAndIndex.index];

						if (entryNode == null)
						{
							flowGraphNode = entryNode = new EntryNode();
						}
						else
						{
							Debug.LogWarning("Two Entry nodes has been found");
							continue;
						}
						break;

					case NodeData.NodeType.State:

						node = target.stateNodes[nodeAndIndex.index];

						flowGraphNode = new StateNode();
						(flowGraphNode as StateNode).StateName = (node as StateNodeData).name;
						break;

					case NodeData.NodeType.Exit:

						node = target.exitNode[nodeAndIndex.index];

						if (exitNode == null)
						{
							flowGraphNode = exitNode = new ExitNode();
						}
						else
						{
							Debug.LogWarning("Two Exit nodes has been found");
							continue;
						}
						break;

					case NodeData.NodeType.Condition:

						node = target.conditionNodes[nodeAndIndex.index];

						flowGraphNode = new ConditionNode();
						break;

				}

				if (flowGraphNode != null)
				{
					Rect pos = flowGraphNode.GetPosition();
					pos.position = nodeAndIndex.position;
					flowGraphNode.SetPosition(pos);
					CreateNode(flowGraphNode);
				}

				tempDataAndVisualLinkage.Add(new TempDataAndVisualLinkage(node, flowGraphNode));
			}

			List<ConnectorData> connectorDatas = manager.Target.connections;
			for (int g = connectorDatas.Count - 1; g >= 0; g--)
			{
				ConnectorData connection = connectorDatas[g];

				PortData input = connection.input;
				TempDataAndVisualLinkage inputNode = tempDataAndVisualLinkage[input.nodeId];

				PortData output = connection.output;
				TempDataAndVisualLinkage outputNode = tempDataAndVisualLinkage[output.nodeId];

				if (!inputNode || !outputNode) continue;

				Port inputPort = inputNode.graphNode.Ports[input.portId];
				Port outputPort = outputNode.graphNode.Ports[output.portId];

				AddElement(inputPort.ConnectTo(outputPort));
			}
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

		public void Dispose()
		{
			manager.OnDataChange -= Manager_OnDataChange;
		}
	}

	/// <summary>
	/// Temp class to link visual and data when generating graph from datas
	/// </summary>
	internal class TempDataAndVisualLinkage
	{
		/// <summary>
		/// The data of the node
		/// </summary>
		public NodeData nodeData;

		/// <summary>
		/// The visual for the node
		/// </summary>
		public FlowGraphNode graphNode;

		public TempDataAndVisualLinkage(NodeData nodeData, FlowGraphNode graphNode)
		{
			this.nodeData = nodeData;
			this.graphNode = graphNode;
		}

		public static implicit operator bool(TempDataAndVisualLinkage d) => d.graphNode != null && d.nodeData != null;
	}

	internal static class HelperTempDataAndVisualLinkage
	{
		public static int IndexOf(this List<TempDataAndVisualLinkage> t, NodeData nodeData)
		{
			int count = t.Count;
			for (int i = 0; i < count; i++)
			{
				if (t[i].nodeData == nodeData) return i;
			}


			return -1;
		}

		public static int IndexOf(this List<TempDataAndVisualLinkage> t, FlowGraphNode graphNode)
		{
			int count = t.Count;
			for (int i = 0; i < count; i++)
			{
				if (t[i].graphNode == graphNode) return i;
			}

			return -1;
		}
	}
}