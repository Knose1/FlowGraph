﻿using Com.Github.Knose1.Flow.Engine.Settings.NodeData;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Engine.Settings
{
	/// <summary>
	/// Class that register nodes and their connection
	/// </summary>
	[Serializable]
	public class NodeDataList
	{
		/// <summary>
		/// Struct that register the type of a Node and its index in the list (see: <see cref="GetNodes(out List{NodeAndIndex})"/>
		/// </summary>
		public struct NodeAndIndex
		{
			public int index;
			public Type type;
			public Vector2 position;

			public NodeAndIndex(int index, Type type, Vector2 position)
			{
				this.index = index;
				this.type = type;
				this.position = position;
			}
		}

		public const string DEBUG_PREFIX = "["+nameof(NodeDataList)+"]";
		private const string CHECK_NAME_REGEX = "(^\\d|\\n| |\\W)";

		public EntryNodeData entryNode;
		public List<ExitNodeData> exitNode;
		public List<StateNodeData> stateNodes;
		public List<RerouteData> reroute;
		/* Condition Nodes are deprecated */
		[NonSerialized] public List<ConditionNodeData> conditionNodes;

		/// <summary>
		/// Connections between the Nodes
		/// </summary>
		public List<ConnectorData> connections;
		
		private bool isDataAdded = true;
		private List<NodeData.NodeData> lastNodes;

		public NodeDataList()
		{
			ClearAllDatas();
		}

		public NodeDataList(EntryNodeData entryNode, List<ExitNodeData> exitNode, List<StateNodeData> stateNodes, List<ConditionNodeData> conditionNodes, List<RerouteData> reroute, List<ConnectorData> connections)
		{
			this.entryNode = entryNode;
			this.exitNode = exitNode;
			this.stateNodes = stateNodes;
			this.conditionNodes = conditionNodes;
			this.reroute = reroute;

			this.connections = connections;
		}

		/// <summary>
		/// Return true if there is an error
		/// </summary>
		/// <returns></returns>
		public bool GetErrors()
		{
			bool hasError = false;

			List<string> namespaceToCheck = new List<string>();
			List<string> classToCheck = new List<string>();
			
			namespaceToCheck.Add(entryNode.@namespace);
			classToCheck.Add(entryNode.@class);
			
			for (int i = stateNodes.Count - 1; i >= 0; i--)
			{
				StateNodeData stateNode = stateNodes[i];
				if (stateNode.executionMode == StateNodeData.Execution.Constructor)
				{
					namespaceToCheck.Add(stateNode.@namespace);
					classToCheck.Add(stateNode.@class);
				}
			}

			//Check class
			for (int i = classToCheck.Count - 1; i >= 0; i--)
			{
				string className = classToCheck[i];
				if (!CheckName(className)) 
				{
					Debug.LogError(DEBUG_PREFIX + $" the class \"{className}\" is in wrong format");
					hasError = true;
				};
			}

			//Check namespaces
			for (int i = namespaceToCheck.Count - 1; i >= 0; i--)
			{
				string @namespace = namespaceToCheck[i];
				if (!CheckName(@namespace.Split('.')))
				{
					Debug.LogError(DEBUG_PREFIX + $" the namespace \"{@namespace}\" is in wrong format");
					hasError = true;
				};
			}

			return hasError;
		}

		/// <summary>
		/// Return true if there is no problem
		/// </summary>
		/// <param name="names"></param>
		/// <returns></returns>
		protected bool CheckName(IList<string> names)
		{
			for (int i = names.Count - 1; i >= 0; i--)
			{
				if (!CheckName(names[i])) return false;
			}

			return true;
		}

		/// <summary>
		/// Return true if there is no problem
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		protected bool CheckName(string name)
		{
			if (name.Length == 0) return false;

			Regex checkName = new Regex(CHECK_NAME_REGEX);

			return !checkName.IsMatch(name);
		}

		/// <summary>
		/// Clear all datas
		/// </summary>
		public void ClearAllDatas()
		{
			entryNode = null;
			exitNode = new List<ExitNodeData>();
			stateNodes = new List<StateNodeData>();
			conditionNodes = new List<ConditionNodeData>();
			reroute = new List<RerouteData>();

			connections = new List<ConnectorData>();
		}

		public List<ConnectorData> FindInputNode(NodeData.NodeData nodeData)
		{
			List<ConnectorData> connections = this.connections;
			GetNodes(out List<NodeData.NodeData> nodes);
			List<ConnectorData> toRet = new List<ConnectorData>();
			for (int i = connections.Count - 1; i >= 0; i--)
			{
				ConnectorData connector = connections[i];
				if (connector.input.GetNode(nodes) == nodeData) toRet.Add(connector);
			}

			return toRet;
		}

		public List<ConnectorData> FindOutputNode(NodeData.NodeData nodeData)
		{
			List<ConnectorData> connections = this.connections;
			GetNodes(out List<NodeData.NodeData> nodes);
			List<ConnectorData> toRet = new List<ConnectorData>();
			for (int i = connections.Count - 1; i >= 0; i--)
			{
				ConnectorData connector = connections[i];
				if (connector.output.GetNode(nodes) == nodeData) toRet.Add(connector);
			}

			return toRet;
		}

		public List<ConnectorData> FindOutputNode(NodeData.StateNodeData nodeData, StateNodeData.StateNodePort port)
		{
			List<ConnectorData> connections = this.connections;
			GetNodes(out List<NodeData.NodeData> nodes);
			List<ConnectorData> toRet = new List<ConnectorData>();
			for (int i = connections.Count - 1; i >= 0; i--)
			{
				ConnectorData connector = connections[i];
				if (
					connector.output.GetNode(nodes) == nodeData && //Same node
					port.id == connector.output.portId // Same id
				) toRet.Add(connector);
			}

			return toRet;
		}

		/// <summary>
		/// Get the <see cref="NodeAndIndex"/>.<br/>
		/// From index 0 to nodes.count :<br/>
		/// <br/>
		/// <see cref="entryNode"/>, <see cref="exitNode"/>, <see cref="stateNodes"/>, <see cref="conditionNodes"/>
		/// </summary>
		/// <param name="nodes"></param>
		public void GetNodes(out List<NodeAndIndex> nodes)
		{

			nodes = new List<NodeAndIndex>();

			if (entryNode != null && entryNode.IsNotNull) nodes.Add(new NodeAndIndex(0, entryNode.GetType(), entryNode.position));

			for (int i = 0; i < exitNode.Count; i++)
			{
				nodes.Add(new NodeAndIndex(i, exitNode[i].GetType(), exitNode[i].position));
			}

			for (int i = 0; i < stateNodes.Count; i++)
			{
				nodes.Add(new NodeAndIndex(i, stateNodes[i].GetType(), stateNodes[i].position));
			}

			for (int i = 0; i < conditionNodes.Count; i++)
			{
				nodes.Add(new NodeAndIndex(i, conditionNodes[i].GetType(), conditionNodes[i].position));
			}

			for (int i = 0; i < reroute.Count; i++)
			{
				nodes.Add(new NodeAndIndex(i, reroute[i].GetType(), reroute[i].position));
			}
		}

		/// <summary>
		/// Get a list of nodes <see cref="NodeData"/>.<br/>
		/// <br/>
		/// <see cref="entryNode"/>, <see cref="exitNode"/>, <see cref="stateNodes"/>, <see cref="conditionNodes"/>
		/// </summary>
		/// <param name="nodes"></param>
		public void GetNodes(out List<NodeData.NodeData> nodes)
		{
			if (!isDataAdded)
			{
				nodes = lastNodes;
				return;
			}

			nodes = new List<NodeData.NodeData>();
			nodes.Add(entryNode);
			nodes.AddRange(exitNode);
			nodes.AddRange(stateNodes);
			nodes.AddRange(conditionNodes);
			nodes.AddRange(reroute);

			lastNodes = nodes = nodes.FindAll((NodeData.NodeData d) => { return d != null && d.IsNotNull; });

			isDataAdded = false;
		}

		/// <summary>
		/// Adds the <see cref="NodeData.NodeData"/> at the end of any list
		/// </summary>
		/// <param name="nodeData">The <see cref="NodeData.NodeData"/> to add</param>
		public void AddNode(NodeData.NodeData nodeData)
		{
			if (nodeData is EntryNodeData)
				entryNode = nodeData as EntryNodeData;

			else if (nodeData is ExitNodeData)
				exitNode.Add(nodeData as ExitNodeData);

			else if (nodeData is StateNodeData)
				stateNodes.Add(nodeData as StateNodeData);

			else if (nodeData is ConditionNodeData)
				conditionNodes.Add(nodeData as ConditionNodeData);

			else if (nodeData is RerouteData)
				reroute.Add(nodeData as RerouteData);

			isDataAdded = true;
		}

		/// <summary>
		/// Adds the <see cref="NodeData.NodeData"/> at index 0 of any list
		/// </summary>
		/// <param name="nodeData">The <see cref="NodeData.NodeData"/> to add</param>
		public void UnshiftNode(NodeData.NodeData nodeData)
		{
			if (nodeData is EntryNodeData)
				entryNode = nodeData as EntryNodeData;

			else if (nodeData is ExitNodeData)
				exitNode.Insert(0, nodeData as ExitNodeData);

			else if (nodeData is StateNodeData)
				stateNodes.Insert(0, nodeData as StateNodeData);

			else if (nodeData is ConditionNodeData)
				conditionNodes.Insert(0, nodeData as ConditionNodeData);

			else if (nodeData is RerouteData)
				reroute.Insert(0, nodeData as RerouteData);
		}

		/// <summary>
		/// Check if the connection is registered in <see cref="connections"/>
		/// </summary>
		/// <returns>Returns true when the connection is in <see cref="connections"/></returns>
		public bool IsConnectionRegistered(ConnectorData connectorData)
		{
			for (int i = connections.Count - 1; i >= 0; i--)
			{
				ConnectorData connectionOther = connections[i];

				if (connectorData == connectionOther) return true;
			}

			return false;
		}

		/// <summary>
		/// Create and get the <see cref="ConnectorData"/> from the NodeData (input and output) and their portId<br/>
		/// <br/>
		/// ps : those 2 lines are equivalents / are the same 
		/// <code>
		///		GetConnectorData(a,0,b,1) //Create a connction between a (port 0) and b (port 1)<br/>
		///		GetConnectorData(b,1,a,0) //Create a connction between a (port 0) and b (port 1)
		///	</code>
		/// </summary>
		/// <param name="input">The input node</param>
		/// <param name="inputPortId">The input node's port</param>
		/// <param name="output">The output node</param>
		/// <param name="outputPortId">The output node's port</param>
		/// <returns></returns>
		public ConnectorData AddConnector(NodeData.NodeData input, int inputPortId, NodeData.NodeData output, int outputPortId)
		{
			GetNodes(out List<NodeData.NodeData> nodes);

			return new ConnectorData(new ConnectedPortData(nodes.IndexOf(input), inputPortId), new ConnectedPortData(nodes.IndexOf(output), outputPortId));
		}

	}

	[CreateAssetMenu(
		menuName = "FlowGraph/" + nameof(FlowGraphScriptable),
		fileName = nameof(FlowGraphScriptable),
		order = 0
	)]
	public class FlowGraphScriptable : ScriptableObject
	{
		[SerializeField] public NodeDataList nodes = new NodeDataList();

		public EntryNodeData EntryNode
		{
			get => nodes.entryNode;
			set => nodes.entryNode = value;
		}

		public List<ExitNodeData> ExitNode
		{
			get => nodes.exitNode;
			set => nodes.exitNode = value;
		}

		public List<StateNodeData> StateNodes
		{
			get => nodes.stateNodes;
			set => nodes.stateNodes = value;
		}

		public List<ConnectorData> Connections
		{
			get => nodes.connections;
			set => nodes.connections = value;
		}

		public void ClearAllDatas() => nodes.ClearAllDatas();
		public void GetNodes(out List<NodeDataList.NodeAndIndex> nodes) => this.nodes.GetNodes(out nodes);

		public void AddNode(NodeData.NodeData nodeData) => nodes.AddNode(nodeData);
		public void UnshiftNode(NodeData.NodeData nodeData) => nodes.UnshiftNode(nodeData);
		public bool IsConnectionRegistered(ConnectorData connectorData) => nodes.IsConnectionRegistered(connectorData);
		public ConnectorData GetConnectorData(NodeData.NodeData input, int inputPortId, NodeData.NodeData output, int outputPortId) => nodes.AddConnector(input, inputPortId, output, outputPortId);
	}
}

namespace Com.Github.Knose1.Flow.Engine.Settings.NodeData
{
	
	[Serializable]
	public class ConnectorData
	{
		[SerializeField] public ConnectedPortData input;
		[SerializeField] public ConnectedPortData output;

		public ConnectorData(ConnectedPortData input, ConnectedPortData output)
		{
			this.input = input;
			this.output = output;
		}

		public void GetNodes(List<NodeData> nodes, out NodeData input, out NodeData output)
		{
			input  = this.input.GetNode(nodes);
			output = this.output.GetNode(nodes);
		}

		public override bool Equals(object obj) => obj is ConnectorData data && EqualityComparer<ConnectedPortData>.Default.Equals(input, data.input) && EqualityComparer<ConnectedPortData>.Default.Equals(output, data.output);

		public override int GetHashCode()
		{
			var hashCode = -1523711817;
			hashCode = hashCode * -1521134295 + EqualityComparer<ConnectedPortData>.Default.GetHashCode(input);
			hashCode = hashCode * -1521134295 + EqualityComparer<ConnectedPortData>.Default.GetHashCode(output);
			return hashCode;
		}

		public static bool operator ==(ConnectorData a, ConnectorData b)
		{
			return (a.input == b.input && a.output == b.output) || (a.input == b.output && a.output == b.input);
		}

		public static bool operator !=(ConnectorData a, ConnectorData b)
		{
			return !(a == b);
		}
	}

	[Serializable]
	public class ConnectedPortData
	{
		[SerializeField] public int nodeId;
		[SerializeField] public int portId;

		public ConnectedPortData(int nodeId, int portId)
		{
			this.nodeId = nodeId;
			this.portId = portId;
		}

		public override bool Equals(object obj) => obj is ConnectedPortData data && nodeId == data.nodeId && portId == data.portId;

		public override int GetHashCode()
		{
			var hashCode = -443552365;
			hashCode = hashCode * -1521134295 + nodeId.GetHashCode();
			hashCode = hashCode * -1521134295 + portId.GetHashCode();
			return hashCode;
		}

		public override string ToString()
		{
			return JsonUtility.ToJson(this);
		}

		public static bool operator ==(ConnectedPortData a, ConnectedPortData b)
		{
			return a.nodeId == b.nodeId && a.portId == b.portId;
		}

		public static bool operator !=(ConnectedPortData a, ConnectedPortData b)
		{
			return !(a == b);
		}

		public NodeData GetNode(List<NodeData> nodes) => nodes[nodeId];
	}

	[Serializable]
	public class NodeData
	{
		/// <summary>
		/// The position of the node
		/// </summary>
		[SerializeField] public Vector2 position;
		[SerializeField, HideInInspector] private bool _isNotNull = false;
		public bool IsNotNull => _isNotNull;

		public NodeData(Vector2 position)
		{
			this.position = position;
			_isNotNull = true;
		}
	}

	[Serializable]
	public class EntryNodeData : NodeData
	{
		[SerializeField] public string @namespace;
		[SerializeField] public string @class;

		public EntryNodeData(Vector2 position, string @namespace, string @class) : base(position)
		{
			this.@namespace = @namespace;
			this.@class = @class;
		}

		public NodeData GetFirstNode(NodeDataList list)
		{
			List<ConnectorData> connectors = list.FindOutputNode(this);
			if (connectors.Count == 0) return null;

			ConnectorData connectorData = connectors[0];

			list.GetNodes(out List<NodeData> nodes);

			return connectorData.input.GetNode(nodes);
		}
	}

	[Serializable]
	public class StateNodeData : NodeData
	{
		public static string GetEventName(string evtBase)
		{
			TextInfo txtInfo = new CultureInfo("en-us", false).TextInfo;

			Regex regex = new Regex("[A-Z]");
			evtBase = regex.Replace(evtBase, (e) => { return " " + e.Value; });

			evtBase = txtInfo.ToTitleCase(evtBase).Replace("_", string.Empty).Replace(" ", string.Empty);
			return "On" + evtBase;
		}

		public enum Execution
		{
			Instantiate,
			Constructor,
			Event
		}

		[SerializeField] public string name;
		[SerializeField] public Execution executionMode;
		[SerializeField] public string @namespace;
		[SerializeField] public string @class;
		[SerializeField] public bool generateEvent;
		[SerializeField] public List<StateNodePort> ports;

		public StateNodeData(Vector2 position, string name, Execution executionMode, string @namespace, string @class, bool generateEvent, List<StateNodePort> ports) : base(position)
		{
			this.name = name;
			this.executionMode = executionMode;
			this.@namespace = @namespace;
			this.@class = @class;
			this.generateEvent = generateEvent;
			this.ports = ports;
		}

		public StateNodePort ConnectionToStateNodePort(ConnectedPortData connectedPortData)
		{
			foreach (StateNodePort port in ports)
			{
				port.id = connectedPortData.portId;
				return port;
			}

			return null;
		}

		[Serializable]
		public class StateNodePort
		{
			[SerializeField] public string trigger = "";
			[SerializeField] public bool createThread = false;
			[SerializeField] public int id;
		}
	}

	[Serializable]
	public class ExitNodeData : NodeData
	{
		public ExitNodeData(Vector2 position) : base(position)
		{
		}
	}

	[Serializable]
	public class ConditionNodeData : NodeData
	{
		public ConditionNodeData(Vector2 position) : base(position)
		{
		}
	}

	[Serializable]
	public class RerouteData : NodeData
	{
		public RerouteData(Vector2 position) : base(position){}

		public NodeData GetRedirection(NodeDataList list)
		{
			List<ConnectorData> connectors = list.FindOutputNode(this);
			if (connectors.Count == 0) return null;

			ConnectorData connectorData = connectors[0];

			list.GetNodes(out List<NodeData> nodes);

			return connectorData.input.GetNode(nodes);
		}

		public NodeData GetNextEffectNode(NodeDataList list)
		{
			List<NodeData> seenList = new List<NodeData>()
			{
				this
			};

			NodeData currentNode = this;
			bool isReroute = false;
			bool alreadySeen = false;
			do
			{
				currentNode = (currentNode as RerouteData).GetRedirection(list);
				if (currentNode == null) return null;

				isReroute = currentNode is RerouteData;
				alreadySeen = seenList.Contains(currentNode);
			} while (alreadySeen || isReroute);

			if (alreadySeen) return null;
			return currentNode;
		}
	}
}