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
				int count = 0;
				for (int i = selection.Count - 1; i >= 0; i--)
				{
					if (selection[i] is FlowGraphNode) ++count;
				}

				bool containsEntry = selection.Contains(entryNode);
				bool containsExit = selection.Contains(exitNode);

				return count > 0 && !(count == 1 && (containsEntry || containsExit)) && !(count == 2 && containsEntry && containsExit); //The selection can't contain only entry / exit node
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
		//protected List<FlowGraphNode> graphNodes = new List<FlowGraphNode>();
		protected FlowGraphManager manager;
		private MiniMap miniMap;


		/*//////////////////////////////////////*/
		/*                                      */
		/*             Constructor              */
		/*                                      */
		/*//////////////////////////////////////*/


		public FlowGraph(FlowGraphManager manager) : base()
		{
			SetupGraph();

			miniMap = new MiniMap();
			this.manager = manager;
			manager.OnDataChange += Manager_OnDataChange;

		}


		/*//////////////////////////////////////*/
		/*                                      */
		/*            Public Methods            */
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
			target.ClearAllDatas();

			target.nodes = GenerateNodeDataList(nodes);
		}

		public FlowGraphNode CreateNode(FlowGraphNode node, bool relativePosition = true)
		{
			AddElement(node);

			return node;
		}

		public void RemoveNode(FlowGraphNode node)
		{
			RemoveElement(node);
		}

		public void ToggleMinimap(bool show)
		{
			if (show)
			{
				Add(miniMap);
				Vector2 mapSize = new Vector2(200, 180);
				Vector2 mapPos = new Vector2(parent.contentRect.width - mapSize.x, parent.contentRect.height - mapSize.y);
				miniMap.SetPosition(new Rect(mapPos, mapSize));
				miniMap.anchored = false;
				miniMap.elementTypeColor = new Color(1, 0, 1);
			}
			else Remove(miniMap);
		}

		/*//////////////////////////////////////*/
		/*                                      */
		/*          Protected Methods           */
		/*                                      */
		/*//////////////////////////////////////*/
		
		/// <summary>
		/// Get <see cref="NodeDataList"/> from a list of Nodes
		/// </summary>
		/// <param name="nodes">The nodes to register in the <see cref="NodeDataList"/></param>
		/// <returns></returns>
		protected NodeDataList GenerateNodeDataList(List<UnityEditor.Experimental.GraphView.Node> nodes) => GenerateNodeDataList(nodes, null);
		
		/// <summary>
		/// Get <see cref="NodeDataList"/> from a list of <see cref="UnityEditor.Experimental.GraphView.Node"/> and a list of <see cref="Edge"/>
		/// </summary>
		/// <param name="nodes">The nodes to register in the <see cref="NodeDataList"/></param>
		/// <param name="edges">The only edges that can be registered in the <see cref="NodeDataList"/></param>
		/// <returns></returns>
		protected NodeDataList GenerateNodeDataList(List<UnityEditor.Experimental.GraphView.Node> nodes, List<Edge> edges)
		{
			NodeDataList target = new NodeDataList();
			List<TempDataAndVisualLinkage> tempDataAndVisualLinkage = new List<TempDataAndVisualLinkage>();

			///Save nodes
			for (int i = nodes.Count - 1; i >= 0; i--)
			{
				FlowGraphNode graphNode = nodes[i] as FlowGraphNode;

				//Check if the node is on the canvas
				if (!nodes.Contains(graphNode))
				{
					nodes.Remove(graphNode);
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
						if (edges != null && !edges.Contains(edge)) continue;

						Port inputPort = edge.input;
						Port outputPort = edge.output;

						FlowGraphNode inputNode = inputPort.node as FlowGraphNode;
						FlowGraphNode outputNode = outputPort.node as FlowGraphNode;

						int tempDataInputIndex = tempDataAndVisualLinkage.IndexOf(inputNode);
						int tempDataOutputIndex = tempDataAndVisualLinkage.IndexOf(outputNode);

						if (tempDataInputIndex == -1 || tempDataOutputIndex == -1) continue;

						NodeData inputNodeData = tempDataAndVisualLinkage[tempDataInputIndex].nodeData;
						NodeData outputNodeData = tempDataAndVisualLinkage[tempDataOutputIndex].nodeData;

						int inputPortIndex = inputNode.Ports.IndexOf(inputPort);
						int outputPortIndex = outputNode.Ports.IndexOf(outputPort);

						ConnectorData connection = target.GetConnectorData(inputNodeData, inputPortIndex, outputNodeData, outputPortIndex);

						if (!target.IsConnectionRegistered(connection))
							target.connections.Add(connection);
					}
				}
			}

			return target;
		}
		
		/// <summary>
		/// Generate an entirely new Graph
		/// </summary>
		protected void GenerateNewGraph()
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

		/// <summary>
		/// Generate a graph from the scriptable's datas
		/// </summary>
		/// <param name="nodes"></param>
		protected List<ISelectable> GenerateGraphFromDatas(NodeDataList target, Vector2 offsetPosition = default)
		{
			target.GetNodes(out List<NodeDataList.NodeAndIndex> nodes);
			return GenerateGraphFromDatas(target, nodes, offsetPosition);
		}
		
		/// <summary>
		/// Generate a graph from the scriptable's datas
		/// </summary>
		/// <param name="nodes"></param>
		protected List<ISelectable> GenerateGraphFromDatas(NodeDataList target, List<NodeDataList.NodeAndIndex> nodes, Vector2 offsetPosition = default)
		{
			List<ISelectable> selectables = new List<ISelectable>();

			List<TempDataAndVisualLinkage> tempDataAndVisualLinkage = new List<TempDataAndVisualLinkage>();
			for (int i = 0; i < nodes.Count; i++)
			{
				NodeDataList.NodeAndIndex nodeAndIndex = nodes[i];

				NodeData nodeData = null;

				FlowGraphNode flowGraphNode = null;

				if (nodeAndIndex.type == typeof(EntryNodeData)) { 
					nodeData = target.entryNode;
					flowGraphNode = entryNode = EntryNode.FromData(nodeData as EntryNodeData);
				}

				else if (nodeAndIndex.type == typeof(StateNodeData))
				{
					nodeData = target.stateNodes[nodeAndIndex.index];
					flowGraphNode = StateNode.FromData(nodeData as StateNodeData);
				}

				else if (nodeAndIndex.type == typeof(ExitNodeData)) 
				{
					nodeData = target.exitNode;
					flowGraphNode = exitNode = ExitNode.FromData(nodeData as ExitNodeData);
				}

				else if (nodeAndIndex.type == typeof(ConditionNodeData)) 
				{
					nodeData = target.conditionNodes[nodeAndIndex.index];
					flowGraphNode = ConditionNode.FromData(nodeData as ConditionNodeData);
				}

				if (flowGraphNode != null)
				{
					flowGraphNode.SetPositionFromData(nodeAndIndex.position + offsetPosition);
					CreateNode(flowGraphNode);

					selectables.Add(flowGraphNode);
				}

				tempDataAndVisualLinkage.Add(new TempDataAndVisualLinkage(nodeData, flowGraphNode));
			}

			//Connect them
			List<ConnectorData> connectorDatas = target.connections;
			for (int g = connectorDatas.Count - 1; g >= 0; g--)
			{
				ConnectorData connection = connectorDatas[g];

				PortData input = connection.input;
				TempDataAndVisualLinkage inputNode = tempDataAndVisualLinkage[input.nodeId];

				PortData output = connection.output;
				TempDataAndVisualLinkage outputNode = tempDataAndVisualLinkage[output.nodeId];

				if (!inputNode || !outputNode) continue;

				Port inputPort = null;
				Port outputPort = null;
				try
				{
					inputPort = inputNode.graphNode.Ports[input.portId];
					outputPort = outputNode.graphNode.Ports[output.portId];
				}
				catch (Exception)
				{
					Debug.LogWarning("Port not found");
					continue;
				}

				Edge edge = inputPort.ConnectTo(outputPort);
				AddElement(edge);
				selectables.Add(edge);
			}

			return selectables;
		}

		/*//////////////////////////////////////*/
		/*                                      */
		/*           Private Methods            */
		/*                                      */
		/*//////////////////////////////////////*/
		
		/// <summary>
		/// On Data change, we must load the graph
		/// </summary>
		private void Manager_OnDataChange()
		{
			//Remove old data
			entryNode = null;
			exitNode = null;
			DestroyGraphVisual();

			//If there is no target, there is no graph to load
			if (!manager.Target) return;
			manager.Target.GetNodes(out List<NodeDataList.NodeAndIndex> nodes);

			//Create New Graph
			if (nodes.Count < 2)
			{
				//Create New Graph
				GenerateNewGraph();
				return;
			}

			//Generate Graph From Datas
			GenerateGraphFromDatas(manager.Target.nodes, nodes);
		}

		/// <summary>
		/// Remove all elements on the graph
		/// </summary>
		private void DestroyGraphVisual()
		{
			//Reset the nodes from the list
			//graphNodes = new List<FlowGraphNode>();

			//Remove all the elements
			graphElements.ForEach(RemoveElement);

		}

		/// <summary>
		/// Load StyleSheet, SetupZoom, AddManipulators, Add the GridBackground and Stretch to Parent Size,
		/// Register unserializeAndPaste and serializeGraphElements
		/// </summary>
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

			serializeGraphElements -= SerializeGraphElements;
			unserializeAndPaste -= UnserializeAndPasteOperation;

			serializeGraphElements += SerializeGraphElementsAsNodeDataList;
			unserializeAndPaste += UnserializeAndPastNodeDataList;
		}
		/// <summary>
		/// Util function when generating New Graph
		/// </summary>
		/// <returns></returns>
		private EntryNode GenerateEntryPointNode()
		{
			EntryNode node = new EntryNode();

			node.SetPosition(new Rect(100, 200, 100, 150));

			return node;
		}
		
		/// <summary>
		/// Util function when generating New Graph
		/// </summary>
		/// <returns></returns>
		private ExitNode GenerateExitPointNode()
		{
			ExitNode node = new ExitNode();

			node.SetPosition(new Rect(500, 200, 100, 150));

			return node;
		}
		
		/*//////////////////////////////////////*/
		/*                                      */
		/*  Serialize and Unserialize Methods   */
		/*                                      */
		/*//////////////////////////////////////*/

		protected string SerializeGraphElementsAsNodeDataList(IEnumerable<GraphElement> elements)
		{
			List<Edge> edges = new List<Edge>();
			List<UnityEditor.Experimental.GraphView.Node> nodes = new List<UnityEditor.Experimental.GraphView.Node>();

			IEnumerator<GraphElement> enumerator = elements.GetEnumerator();

			while (enumerator.MoveNext())
			{
				if (enumerator.Current is Edge) edges.Add(enumerator.Current as Edge);
				else if (enumerator.Current is FlowGraphNode && !(enumerator.Current is IUnique)) nodes.Add(enumerator.Current as FlowGraphNode);
			}

			NodeDataList datas = GenerateNodeDataList(nodes, edges);
			return JsonUtility.ToJson(datas);
		}

		protected void UnserializeAndPastNodeDataList(string operationName, string data)
		{
			List<ISelectable> selection = this.selection.ToList();
			foreach (ISelectable selectionItem in selection)
			{
				selectionItem.Unselect(this);
			}

			selection = GenerateGraphFromDatas(JsonUtility.FromJson<NodeDataList>(data), new Vector2(50,50));

			foreach (ISelectable selectionItem in selection)
			{
				selectionItem.Select(this, true);
			}
		}

		/*//////////////////////////////////////*/
		/*                                      */
		/*              Overrides               */
		/*                                      */
		/*//////////////////////////////////////*/

		/// <summary>
		/// 
		/// </summary>
		/// <param name="startPort"></param>
		/// <param name="nodeAdapter"></param>
		/// <returns></returns>
		public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
		{
			List<Port> compatiblePorts = new List<Port>();
			List<Port> ports = base.ports.ToList();

			foreach (Port port in ports)
			{
				List<Edge> connections = port.connections.ToList();

				bool isAlreadyConnected = false;
				for (int i = connections.Count - 1; i >= 0; i--)
				{
					Port other = null;


					if (other == port)
					{
						isAlreadyConnected = true;
						break;
					}
				}


				if (startPort != port && startPort.node != port.node && !isAlreadyConnected)
					compatiblePorts.Add(port);
			}

			return compatiblePorts;
		}
		
		/*//////////////////////////////////////*/
		/*                                      */
		/*               Dispose                */
		/*                                      */
		/*//////////////////////////////////////*/

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