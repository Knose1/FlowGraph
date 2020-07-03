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
					if (selection[i] is UnityEditor.Experimental.GraphView.Node) ++count;
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
					UnityEditor.Experimental.GraphView.Node node = selection[i] as UnityEditor.Experimental.GraphView.Node;

					if (node == null || node.capabilities != Capabilities.Deletable) return false;
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
		protected FlowGraphManager manager;
		private MiniMap miniMap;
		private StyleSheet styleSheet;


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


		public TokenNode CreateRetoute()
		{
			TokenNode node = new TokenNode(
				Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, null),
				Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, null)
			);
			AddNode(node);

			return node;
		}

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

		/// <summary>
		/// Adds a node on the grid
		/// </summary>
		/// <param name="node"></param>
		/// <param name="relativePosition">If false, the position is absolute on the graph</param>
		/// <returns></returns>
		public UnityEditor.Experimental.GraphView.Node AddNode(UnityEditor.Experimental.GraphView.Node node, bool relativePosition = true)
		{
			if (relativePosition) 
			{
				Vector3 viewScale = viewTransform.scale;
				Vector3 viewPosition = viewTransform.position;
				Vector2 viewUpperLeftPosition = new Vector2(viewPosition.x / viewScale.x, viewPosition.y / viewScale.y);
				Vector2 parentHalfSize = parent.contentRect.center;

				Rect pos = node.GetPosition();
				pos.position -= viewUpperLeftPosition - parentHalfSize / viewScale;
				node.SetPosition(pos); 
			}
			AddElement(node);

			return node;
		}

		/// <summary>
		/// Remove a node from the grid
		/// </summary>
		/// <returns></returns>
		public void RemoveNode(UnityEditor.Experimental.GraphView.Node node)
		{
			RemoveElement(node);
		}

		/// <summary>
		/// Show the minimap or not
		/// </summary>
		/// <param name="show">If true, show the minimap</param>
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
				UnityEditor.Experimental.GraphView.Node graphNode = nodes[i];
				if (graphNode == null) continue;

				//Check if the node is on the canvas
				if (!nodes.Contains(graphNode))
				{
					nodes.Remove(graphNode);
					continue;
				}

				NodeData nodeData = null;

				if (graphNode is FlowGraphNode)
				{
					nodeData = (graphNode as FlowGraphNode).Serialize();
				}
				else if (graphNode is TokenNode)
				{
					nodeData = new  RerouteData(graphNode.GetPosition().position);
				}

				tempDataAndVisualLinkage.Insert(0, new TempDataAndVisualLinkage(nodeData, graphNode));

				target.UnshiftNode(nodeData);
			}

			///Save connections
			for (int i = 0; i < tempDataAndVisualLinkage.Count; i++)
			{
				UnityEditor.Experimental.GraphView.Node inputGraphNode = tempDataAndVisualLinkage[i].graphNode;

				List<Port> ports = inputGraphNode.GetPorts();

				for (int j = 0; j < ports.Count; j++)
				{
					IEnumerator<Edge> inputConnections = ports[j].connections.GetEnumerator();
					while (inputConnections.MoveNext())
					{
						Edge edge = inputConnections.Current;
						if (edges != null && !edges.Contains(edge)) continue;

						Port inputPort = edge.input;
						Port outputPort = edge.output;

						UnityEditor.Experimental.GraphView.Node inputNode = inputPort.node;
						UnityEditor.Experimental.GraphView.Node outputNode = outputPort.node;

						int tempDataInputIndex = tempDataAndVisualLinkage.IndexOf(inputNode);
						int tempDataOutputIndex = tempDataAndVisualLinkage.IndexOf(outputNode);

						if (tempDataInputIndex == -1 || tempDataOutputIndex == -1) continue;

						NodeData inputNodeData = tempDataAndVisualLinkage[tempDataInputIndex].nodeData;
						NodeData outputNodeData = tempDataAndVisualLinkage[tempDataOutputIndex].nodeData;

						int inputPortIndex = inputNode.GetPorts().IndexOf(inputPort);
						int outputPortIndex = outputNode.GetPorts().IndexOf(outputPort);

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
				AddNode(entryNode = GenerateEntryPointNode(), false);
				AddNode(exitNode = GenerateExitPointNode(), false);
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
				UnityEditor.Experimental.GraphView.Node node = null;

				if (nodeAndIndex.type == typeof(EntryNodeData)) { 
					nodeData = target.entryNode;
					node = entryNode = EntryNode.FromData(nodeData as EntryNodeData);
				}

				else if (nodeAndIndex.type == typeof(StateNodeData))
				{
					nodeData = target.stateNodes[nodeAndIndex.index];
					node = StateNode.FromData(nodeData as StateNodeData);
				}

				else if (nodeAndIndex.type == typeof(ExitNodeData)) 
				{
					nodeData = target.exitNode;
					node = exitNode = ExitNode.FromData(nodeData as ExitNodeData);
				}

				else if (nodeAndIndex.type == typeof(ConditionNodeData)) 
				{
					nodeData = target.conditionNodes[nodeAndIndex.index];
					node = ConditionNode.FromData(nodeData as ConditionNodeData);
				}

				else if (nodeAndIndex.type == typeof(RerouteData))
				{
					nodeData = target.reroute[nodeAndIndex.index];
					node = CreateRetoute();
				}

				if (node != null)
				{
					node.SetPositionFromData(nodeAndIndex.position + offsetPosition);
					AddNode(node, false);

					selectables.Add(node);
				}

				tempDataAndVisualLinkage.Add(new TempDataAndVisualLinkage(nodeData, node));
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

				Port inputPort;
				Port outputPort;
				try
				{
					inputPort = inputNode.graphNode.GetPorts()[input.portId];
					outputPort = outputNode.graphNode.GetPorts()[output.portId];
				}
				catch (Exception)
				{
					Debug.LogWarning("Port not found");
					continue;
				}

				Edge edge = inputPort.ConnectTo<Edge>(outputPort);
				AddElement(edge);
				selectables.Add(edge);
			}

			return selectables;
		}

		/*//////////////////////////////////////*/
		/*                                      */
		/*           Event Handelers            */
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

		/*//////////////////////////////////////*/
		/*                                      */
		/*           Private Methods            */
		/*                                      */
		/*//////////////////////////////////////*/

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
			unserializeAndPaste += UnserializeAndPasteNodeDataList;
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

		/// <summary>
		/// Method for serializing graph elements for copy/paste and other actions.<br/><br/>
		/// See : <see cref="GraphView.serializeGraphElements"/>
		/// </summary>
		/// <param name="elements"></param>
		/// <returns></returns>
		protected string SerializeGraphElementsAsNodeDataList(IEnumerable<GraphElement> elements)
		{
			List<Edge> edges = new List<Edge>();
			List<UnityEditor.Experimental.GraphView.Node> nodes = new List<UnityEditor.Experimental.GraphView.Node>();

			IEnumerator<GraphElement> enumerator = elements.GetEnumerator();

			while (enumerator.MoveNext())
			{
				if (enumerator.Current is Edge) edges.Add(enumerator.Current as Edge);
				else if (enumerator.Current is FlowGraphNode && !(enumerator.Current is IUnique)) nodes.Add(enumerator.Current as FlowGraphNode);
				else if (enumerator.Current is TokenNode)
				{
					nodes.Add(enumerator.Current as TokenNode);
				}
			}

			NodeDataList datas = GenerateNodeDataList(nodes, edges);
			return JsonUtility.ToJson(datas);
		}

		/// <summary>
		/// Method for unserializing graph elements for copy/paste and other actions.<br/><br/>
		/// See : <see cref="GraphView.unserializeAndPaste"/>
		/// </summary>
		/// <param name="elements"></param>
		/// <returns></returns>
		protected void UnserializeAndPasteNodeDataList(string operationName, string data)
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
		/// Get all ports compatible with given port.
		/// </summary>
		/// <param name="startPort">Start port to validate against.</param>
		/// <param name="nodeAdapter">Node adapter.</param>
		/// <returns>List of compatible ports.</returns>
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

		/// <summary>
		/// Destroy the instance and Unregister all calback
		/// </summary>
		public void Dispose()
		{
			Resources.UnloadAsset(styleSheet);
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
		public UnityEditor.Experimental.GraphView.Node graphNode;

		public TempDataAndVisualLinkage(NodeData nodeData, UnityEditor.Experimental.GraphView.Node graphNode)
		{
			this.nodeData = nodeData;
			this.graphNode = graphNode;
		}

		public static implicit operator bool(TempDataAndVisualLinkage d) => d.graphNode != null && d.nodeData != null;
	}

	internal static class HelperTempDataAndVisualLinkage
	{
		/// <summary>
		/// Get the index of a <see cref="TempDataAndVisualLinkage"/> in a List by its <see cref="NodeData"/>.
		/// </summary>
		/// <param name="t">This</param>
		/// <param name="nodeData">The <see cref="NodeData"/> to find in the list</param>
		/// <returns></returns>
		public static int IndexOf(this List<TempDataAndVisualLinkage> t, NodeData nodeData)
		{
			int count = t.Count;
			for (int i = 0; i < count; i++)
			{
				if (t[i].nodeData == nodeData) return i;
			}


			return -1;
		}

		/// <summary>
		/// Get the index of a <see cref="TempDataAndVisualLinkage"/> in a List by its <see cref="UnityEditor.Experimental.GraphView.Node"/>.
		/// </summary>
		/// <param name="t">This</param>
		/// <param name="graphNode">The <see cref="UnityEditor.Experimental.GraphView.Node"/> to find in the list</param>
		/// <returns></returns>
		public static int IndexOf(this List<TempDataAndVisualLinkage> t, UnityEditor.Experimental.GraphView.Node graphNode)
		{
			int count = t.Count;
			for (int i = 0; i < count; i++)
			{
				if (t[i].graphNode == graphNode) return i;
			}

			return -1;
		}
	}

	public static class NodeHelper
	{
		/// <summary >
		/// Get all the <see cref="Port"/>s in a <see cref="UnityEditor.Experimental.GraphView.Node"/><br/>
		/// All the <see cref="Port"/>s returned are children of those containers :<br/>
		/// - <see cref="UnityEditor.Experimental.GraphView.Node.inputContainer"/><br/>
		/// - <see cref="UnityEditor.Experimental.GraphView.Node.outputContainer"/><br/>
		/// <br/>
		/// If the <see cref="UnityEditor.Experimental.GraphView.Node"/> is a <see cref="FlowGraphNode"/>, returns <see cref="FlowGraphNode.Ports"/><br/>
		/// </summary>
		/// <param name="node">This</param>
		/// <returns></returns>
		public static List<Port> GetPorts(this UnityEditor.Experimental.GraphView.Node node) 
		{
			if (node is FlowGraphNode) return (node as FlowGraphNode).Ports;

			List<Port> ports = new List<Port>();

			IEnumerator<VisualElement> inputEnumerable = node.inputContainer.Children().GetEnumerator();
			IEnumerator<VisualElement> outputEnumerable = node.outputContainer.Children().GetEnumerator();

			while (inputEnumerable.MoveNext())
			{
				if (inputEnumerable.Current != null && inputEnumerable.Current is Port) ports.Add(inputEnumerable.Current as Port);
			}

			while (outputEnumerable.MoveNext())
			{
				if (outputEnumerable.Current != null && outputEnumerable.Current is Port) ports.Add(outputEnumerable.Current as Port);
			}

			return ports;
		}
	}
}