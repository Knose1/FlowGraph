using Com.Github.Knose1.Flow.Editor.Node;
using Com.Github.Knose1.Flow.Engine.Settings;
using Com.Github.Knose1.Flow.Engine.Settings.NodeData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow.Editor
{
	public static class FlowGraphAssetDatabase
	{
		public const string ASSET_FOLDER = "Assets/UI Manager Graph/Editor/Asset/";

		/// <summary>
		/// The stylesheet name
		/// </summary>
		public const string RESSOURCE_STYLESHEET = ASSET_FOLDER+"Graph.uss";
		/// <summary>
		/// The stylesheet black name
		/// </summary>
		public const string RESSOURCE_STYLESHEET_BLACK = ASSET_FOLDER+"GraphBlack.uss";
		/// <summary>
		/// The stylesheet white name
		/// </summary>
		public const string RESSOURCE_STYLESHEET_WHITE = ASSET_FOLDER+"GraphWhite.uss";

		/// <summary>
		/// Arguments : <br/> 
		/// - #{NAMESPACE}#: The class's namespace
		/// - #{CLASS}#: The class's name
		/// - #{EVENTS}#: The event fields
		/// - #{GO_FIELDS}#: The game object fields
		/// - #{CLASS_FIELDS}#: The class object fields
		/// - #{STATES}#: The MachineState fields
		/// - #{CREATE_STATES}#: Where to create the states
		/// - #{ALLOW_TRIGGERS}# : Where to allow triggers
		/// - #{ADD_TRIGGERS}# : Where to add triggers to states
		/// - #{ADD_EVENTS}# : Where to add events to
		/// - #{ENTRY_STATE}# : The first state to be executed
		/// </summary>
		public const string CLASS_TEMPLATE = ASSET_FOLDER+"Class_template.cs.txt";
		public const string SUBSTATE_CLASS_TEMPLATE = ASSET_FOLDER+"SubStateClass_template.cs.txt";

		/// <summary>
		/// <see cref="Generate.TemplateJsonData"/>
		/// </summary>
		public const string ARGS_TEMPLATE = ASSET_FOLDER+"TemplateArgs.json";
	}

	/// <summary>
	/// Flow graph, you can place <see cref="FlowGraphNode"/>s on it, move the nodes, link them etc...<br/>
	/// <br/>
	/// There is also a <see cref="FlowGraph.Save()"/> method to save the graph
	/// </summary>
	public class FlowGraph : GraphView, IDisposable
	{
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

				return count > 0 && !(count == 1 && containsEntry); //The selection can't contain only entry node
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
		/*                Events                */
		/*                                      */
		/*//////////////////////////////////////*/

		public event Action OnChange;

		/*//////////////////////////////////////*/
		/*                                      */
		/*                Fields                */
		/*                                      */
		/*//////////////////////////////////////*/

		protected EntryNode entryNode = null;
		protected FlowGraphManager manager;
		private MiniMap miniMap;

		/// <summary>
		/// To know when the graph is copyPasting datas
		/// </summary>
		public bool isUnserializing;
		/// <summary>
		/// To know when the graph is initing a graph
		/// </summary>
		public bool isInit;
		
		public const string GRID_BACKGROUND_CLASS_NAME = "flowGridBackground";


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

			StateNode.StateOutputPort.OnDataChange += OnElementChange;
			FlowGraphNode.OnChange += OnElementChange;
			StateNode.OnSubstateChange += StateNode_OnSubstateChange;
			FlowGraphEdge.OnChange += OnElementChange;
			FlowGraphEdge.OnAddReroute += FlowGraphEdge_OnAddReroute;
			FlowGraphPort.OnConnection += FlowGraphPort_OnConnection;
			RerouteNode.OnChange += OnElementChange;
		}

		private bool StateNode_OnSubstateChange(FlowGraphScriptable newScriptable)
		{
			FlowGraphScriptable targetScriptable = manager.Target;
			if (newScriptable == null) return true;

			//Returne false when found
			return targetScriptable.ParentHierachyToList().Contains(newScriptable);
		}

		public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
		{
			base.BuildContextualMenu(evt);

			bool onlyRerouteSelected = true;
			bool inAndOutIsSame = true;

			List<ISelectable> selection = this.selection;
			List<RerouteNode> rerouteNodes = new List<RerouteNode>();

			//Scope to kill temp in and out port
			{
				Port inPort = null;
				Port outPort = null;
				foreach (var item in selection)
				{
					if (item is UnityEditor.Experimental.GraphView.Node)
					{
						bool isReroute = item is RerouteNode;
						//onlyRerouteSelected switch to false when the node is not a RerouteNode
						onlyRerouteSelected = isReroute && onlyRerouteSelected; //onlyRerouteSelected == true
					


						if (isReroute)
						{
							RerouteNode reroute = item as RerouteNode;
							rerouteNodes.Add(reroute);

							RerouteNode.GetReroute(reroute, out _, out _, out Port rerouteInPort, out Port rerouteOutPort);

							inPort = inPort ?? rerouteInPort;
							outPort = outPort ?? rerouteOutPort;

							inAndOutIsSame = inPort == rerouteInPort && outPort == rerouteOutPort;
						}

						if (!onlyRerouteSelected || !inAndOutIsSame) break;
					}
				}
			}

			if (inAndOutIsSame) inAndOutIsSame = RerouteNode.AreConsecutives(rerouteNodes);

			int nodesCount = rerouteNodes.Count;
			if (onlyRerouteSelected && inAndOutIsSame && nodesCount > 0)
			{
				if (nodesCount == 1)
				{
					evt.menu.AppendAction("Remove Reroute", LMenuRemoveReroute);
				}
				else
				{

					evt.menu.AppendAction("Remove Reroutes", LMenuRemoveReroute);
					//evt.menu.AppendAction("Colapse Reroutes", LMenuColapseReroute);
				}
			}

			void LMenuRemoveReroute(DropdownMenuAction obj)
			{
				this.selection = new List<ISelectable>(rerouteNodes);

				RerouteNode reroute = rerouteNodes.First();
				RerouteNode.GetReroute(reroute, out _, out _, out Port inPort, out Port outPort, rerouteNodes);

				foreach (var rerouteItem in rerouteNodes)
				{
					RemoveNode(rerouteItem);
				}
				
				AddElement(inPort.ConnectTo<FlowGraphEdge>(outPort));
				UpdateReroute();
			}

			/*void LMenuColapseReroute(DropdownMenuAction obj)
			{
				rerouteNodes.GetCenter();

			}*/
		}


		private void FlowGraphPort_OnConnection(FlowGraphPort inPort, FlowGraphPort outPort)
		{
			if (inPort != null && outPort != null && inPort.node is RerouteNode)
			{
				(inPort.node as RerouteNode).Color = outPort.portColor;
			}

			OnElementChange();
		}

		private void FlowGraphEdge_OnAddReroute(Vector2 arg1, Edge arg2)
		{
			RerouteNode reroute = CreateRetoute();

			RerouteNode.AddReroute(arg2, reroute, this);

			Rect pos = reroute.GetPosition();
			pos.position = contentViewContainer.WorldToLocal(arg1);
			reroute.SetPosition(pos);

			reroute.UpdateColors();
		}

		/*//////////////////////////////////////*/
		/*                                      */
		/*            Public Methods            */
		/*                                      */
		/*//////////////////////////////////////*/

		public RerouteNode CreateRetoute()
		{
			RerouteNode node = new RerouteNode();
			AddNode(node);
			node.UpdateColors();

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
			DeleteElements(node.GetConnections());

			if (node is IDisposable)
			{
				(node as IDisposable).Dispose();
			}

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
				else if (graphNode is RerouteNode)
				{
					nodeData = new RerouteData(graphNode.GetPosition().position);
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

						ConnectorData connection = target.AddConnector(inputNodeData, inputPortIndex, outputNodeData, outputPortIndex);

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
			AddNode(entryNode = GenerateEntryPointNode(), false);
		}

		/// <summary>
		/// Generate a graph from the scriptable's datas
		/// </summary>
		/// <param name="nodes"></param>
		protected List<ISelectable> GenerateGraphFromDatas(NodeDataList target, Vector2 offsetPosition = default)
		{
			if (target == null)
			{
				Debug.LogWarning("["+nameof(FlowGraph)+"] Can't parse data");
				return new List<ISelectable>();
			}

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
					nodeData = target.exitNode[nodeAndIndex.index];
					node = ExitNode.FromData(nodeData as ExitNodeData);
				}

				/*else if (nodeAndIndex.type == typeof(ConditionNodeData)) 
				{
					nodeData = target.conditionNodes[nodeAndIndex.index];
					node = ConditionNode.FromData(nodeData as ConditionNodeData);
				}*/

				else if (nodeAndIndex.type == typeof(RerouteData))
				{
					nodeData = target.reroutes[nodeAndIndex.index];
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

				ConnectedPortData input = connection.input;
				TempDataAndVisualLinkage inputNode = tempDataAndVisualLinkage[input.nodeId];

				ConnectedPortData output = connection.output;
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
					Debug.LogWarning($"Port not found:\ninput : {input}\noutput: {output}");
					continue;
				}

				Edge edge = inputPort.ConnectTo<FlowGraphEdge>(outputPort);
				AddElement(edge);
				selectables.Add(edge);
			}

			UpdateReroute();

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
			List<FlowGraphScriptable> parents = manager.Target.parents;
			for (int i = parents.Count - 1; i >= 0; i--)
			{
				if (parents[i] is null) parents.Remove(null);
			}

			isInit = true;
			isUnserializing = true;

			//Remove old data
			entryNode = null;
			DestroyGraphVisual();
			StateNode.StateOutputPort.DisposeAll();

			//If there is no target, there is no graph to load
			if (!manager.Target) return;
			manager.Target.GetNodes(out List<NodeDataList.NodeAndIndex> nodes);

			try
			{
				//Create New Graph
				if (nodes.Count < 1)
				{
					//Create New Graph
					GenerateNewGraph();
					return;
				}

				//Generate Graph From Datas
				GenerateGraphFromDatas(manager.Target.nodes, nodes);
			}
			catch (Exception err)
			{
				Debug.LogError(err);
				Debug.LogWarning("[" + nameof(FlowGraph) + "] An error occured when Unserializing the graph");
			}

			isUnserializing = false;
			isInit = false;
		}

		private void OnElementChange()
		{
			if (!isInit)
				OnChange?.Invoke();
	
			UpdateReroute();
		}

		private void UpdateReroute()
		{
			List<UnityEditor.Experimental.GraphView.Node> nodes = this.nodes.ToList();
			foreach (var item in nodes)
			{
				(item as RerouteNode)?.UpdateColors();
			}


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
			SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

			this.AddManipulator(new ContentDragger());
			this.AddManipulator(new SelectionDragger());
			this.AddManipulator(new RectangleSelector());

			GridBackground gridBackground = new GridBackground();
			gridBackground.AddToClassList(GRID_BACKGROUND_CLASS_NAME);
			Insert(0, gridBackground);
			gridBackground.StretchToParentSize();
			
			serializeGraphElements -= SerializeGraphElements;
			unserializeAndPaste -= UnserializeAndPasteOperation;
			deleteSelection -= DeleteSelectionOperation;

			serializeGraphElements += SerializeGraphElementsAsNodeDataList;
			unserializeAndPaste += UnserializeAndPasteNodeDataList;
			deleteSelection += DeleteSelectionNodeDataList;
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
			isUnserializing = true;

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

			isUnserializing = false;
		}

		private void DeleteSelectionNodeDataList(string operationName, AskUser askUser)
		{
			List<ISelectable> selection1 = new List<ISelectable>(selection);
			foreach (var item in selection1)
			{
				if (item is UnityEditor.Experimental.GraphView.Node)
				{
					RemoveNode(item as UnityEditor.Experimental.GraphView.Node);
				}

				if (item is GraphElement) RemoveElement(item as GraphElement);
			}

			OnElementChange();
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

			var startNode = startPort.node;

			foreach (Port port in ports)
			{
				if (port.direction == startPort.direction) continue;
				if (port == startPort) continue;

				List<Edge> connections = port.connections.ToList();

				bool isAlreadyConnected = false;
				

				bool isReroute = startNode is RerouteNode;
				bool isSamePort = startPort != port;

				bool isSameNode = startNode == port.node && isReroute;

				if (isSamePort && !isSameNode && !isAlreadyConnected)
					compatiblePorts.Add(port);
			}

			Edge[] startPortEdges = startPort.connections.ToArray();
			for (int i = startPortEdges.Length - 1; i >= 0; i--)
			{
				Port otherPort = null;
				switch (startPort.direction)
				{
					case Direction.Input:
						otherPort = startPortEdges[i].output;
						break;
					case Direction.Output:
						otherPort = startPortEdges[i].input;
						break;
				}

				if (otherPort == null) continue;

				UnityEditor.Experimental.GraphView.Node otherNode = otherPort.node;

				if (otherNode is RerouteNode)
				{
					RerouteNode.GetReroute(otherNode as RerouteNode, out FlowGraphNode inNode, out FlowGraphNode outNode, out Port inPort, out Port outPort);
					
					compatiblePorts.Remove(inPort);
					compatiblePorts.Remove(outPort);
				}
			}

			return compatiblePorts;
		}
		
		/*//////////////////////////////////////*/
		/*                                      */
		/*               Dispose                */
		/*                                      */
		/*//////////////////////////////////////*/

		/// <summary>
		/// Destroy the instance and Unregister all callbacks
		/// </summary>
		public void Dispose()
		{
			manager.OnDataChange -= Manager_OnDataChange;

			FlowGraphNode.OnChange -= OnElementChange;
			FlowGraphEdge.OnChange -= OnElementChange;
			StateNode.OnSubstateChange -= StateNode_OnSubstateChange;
			FlowGraphEdge.OnAddReroute -= FlowGraphEdge_OnAddReroute;
			FlowGraphPort.OnConnection -= FlowGraphPort_OnConnection;
			RerouteNode.OnChange -= OnElementChange;

			OnChange = null;
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

		public static List<Edge> GetConnections(this UnityEditor.Experimental.GraphView.Node node)
		{
			List<Edge> toReturn = new List<Edge>();
			List<Port> ports = node.GetPorts();

			foreach (var port in ports)
			{
				toReturn.AddRange(port.connections);
			}

			return toReturn;
		}

		public static T GetFirstChildOfType<T>(this VisualElement elm) where T : VisualElement {
			IEnumerable<VisualElement> enumerable = elm.Children();
			IEnumerator<VisualElement> enumerator = enumerable.GetEnumerator();

			int length = enumerable.Count();
			if (length == 0) return null;

			while (enumerator.MoveNext())
			{
				if (enumerator.Current is T)
					return (T)enumerator.Current;
			}

			enumerator = enumerable.GetEnumerator();
			while (enumerator.MoveNext())
			{
				T child = enumerator.Current.GetFirstChildOfType<T>();
				if (child != null) 
					return child;
			}

			return null;
		}

		public static Vector2 GetCenter(this IEnumerable<VisualElement> selection)
		{
			bool isToRturnSet = false;
			Vector2 toReturn = default;
			foreach (VisualElement item in selection)
			{
				Vector2 pos = item.worldBound.center;

				if (!isToRturnSet)
				{
					toReturn = pos;
					isToRturnSet = true;
					continue;
				}

				toReturn = (toReturn + pos) / 2;
			}

			return toReturn;
		}

		public static Vector2 GetCenter(this IEnumerable<UnityEditor.Experimental.GraphView.Node> selection)
		{
			bool isToRturnSet = false;
			Vector2 toReturn = default;
			foreach (var item in selection)
			{
				Vector2 pos = item.GetPosition().center;

				if (!isToRturnSet)
				{
					toReturn = pos;
					isToRturnSet = true;
					continue;
				}

				toReturn = (toReturn + pos) / 2;
			}

			return toReturn;
		}
	}
}