using Com.Github.Knose1.Flow.Engine.Settings.NodeData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Engine.Settings
{
	[CreateAssetMenu(
		menuName = "FlowGraph/" + nameof(FlowGraphScriptable),
		fileName = nameof(FlowGraphScriptable),
		order = 0
	)]
	public class FlowGraphScriptable : ScriptableObject
	{

		public struct NodeAndIndex
		{
			public int index;
			public NodeData.NodeData.NodeType type;
			public Vector2 position;

			public NodeAndIndex(int index, NodeData.NodeData.NodeType type, Vector2 position)
			{
				this.index = index;
				this.type = type;
				this.position = position;
			}
		}

		public void EmptyNodes()
		{
			entryNode = new List<EntryNodeData>();
			exitNode = new List<ExitNodeData>();
			stateNodes = new List<StateNodeData>();
			conditionNodes = new List<ConditionNodeData>();
		}

		public void GetNodes(out List<NodeAndIndex> nodes)
		{

			nodes = new List<NodeAndIndex>();

			for (int i = 0; i < entryNode.Count; i++)
			{
				nodes.Add(new NodeAndIndex(i, entryNode[i].type, entryNode[i].position));
			}

			for (int i = 0; i < exitNode.Count; i++)
			{
				nodes.Add(new NodeAndIndex(i, exitNode[i].type, exitNode[i].position));
			}


			for (int i = 0; i < stateNodes.Count; i++)
			{
				nodes.Add(new NodeAndIndex(i, stateNodes[i].type, stateNodes[i].position));
			}

			for (int i = 0; i < conditionNodes.Count; i++)
			{
				nodes.Add(new NodeAndIndex(i, conditionNodes[i].type, conditionNodes[i].position));
			}
		}


		[SerializeField] public List<EntryNodeData> entryNode = new List<EntryNodeData>();
		[SerializeField] public List<ExitNodeData> exitNode = new List<ExitNodeData>();
		[SerializeField] public List<StateNodeData> stateNodes = new List<StateNodeData>();
		[SerializeField] public List<ConditionNodeData> conditionNodes = new List<ConditionNodeData>();

		[SerializeField] public List<ConnectorData> connections;


		public void AddNode(NodeData.NodeData nodeData)
		{
			switch (nodeData.type)
			{
				case NodeData.NodeData.NodeType.Unknown:
					break;
				case NodeData.NodeData.NodeType.Entry:
					entryNode.Add(nodeData as EntryNodeData);
					break;
				case NodeData.NodeData.NodeType.State:
					stateNodes.Add(nodeData as StateNodeData);
					break;
				case NodeData.NodeData.NodeType.Exit:
					exitNode.Add(nodeData as ExitNodeData);
					break;
				case NodeData.NodeData.NodeType.Condition:
					conditionNodes.Add(nodeData as ConditionNodeData);
					break;
			}
		}

		public void UnshiftNode(NodeData.NodeData nodeData)
		{
			switch (nodeData.type)
			{
				case NodeData.NodeData.NodeType.Unknown:
					break;
				case NodeData.NodeData.NodeType.Entry:
					entryNode.Insert(0, nodeData as EntryNodeData);
					break;
				case NodeData.NodeData.NodeType.State:
					stateNodes.Insert(0, nodeData as StateNodeData);
					break;
				case NodeData.NodeData.NodeType.Exit:
					exitNode.Insert(0, nodeData as ExitNodeData);
					break;
				case NodeData.NodeData.NodeType.Condition:
					conditionNodes.Insert(0, nodeData as ConditionNodeData);
					break;
			}
		}

		/// <summary>
		/// Check if the connection is registered in <see cref="connections"/>
		/// </summary>
		/// <returns></returns>
		public bool IsConnectionRegistered(ConnectorData connectorData)
		{
			for (int i = connections.Count - 1; i >= 0; i--)
			{
				ConnectorData connectionOther = connections[i];

				if (connectorData == connectionOther) return true;
			}

			return false;
		}



		public ConnectorData GetConnectorData(NodeData.NodeData input, int inputPortId, NodeData.NodeData output, int outputPortId)
		{
			int inputIndex = -1;
			int outputIndex = -1;

			int entryNodeCount = entryNode.Count;
			int exitNodeCount = exitNode.Count;
			int stateNodesCount = stateNodes.Count;

			switch (input.type)
			{
				case NodeData.NodeData.NodeType.Entry:
					inputIndex = entryNode.IndexOf(input as EntryNodeData);
					break;
				case NodeData.NodeData.NodeType.Exit:
					inputIndex = entryNodeCount + exitNode.IndexOf(input as ExitNodeData);
					break;
				case NodeData.NodeData.NodeType.State:
					inputIndex = entryNodeCount + exitNodeCount + stateNodes.IndexOf(input as StateNodeData);
					break;
				case NodeData.NodeData.NodeType.Condition:
					inputIndex = entryNodeCount + exitNodeCount + stateNodesCount + conditionNodes.IndexOf(input as ConditionNodeData);
					break;
			}

			switch (output.type)
			{
				case NodeData.NodeData.NodeType.Entry:
					outputIndex = entryNode.IndexOf(output as EntryNodeData);
					break;
				case NodeData.NodeData.NodeType.Exit:
					outputIndex = entryNodeCount + exitNode.IndexOf(output as ExitNodeData);
					break;
				case NodeData.NodeData.NodeType.State:
					outputIndex = entryNodeCount + exitNodeCount + stateNodes.IndexOf(output as StateNodeData);
					break;
				case NodeData.NodeData.NodeType.Condition:
					outputIndex = entryNodeCount + exitNodeCount + stateNodesCount + conditionNodes.IndexOf(output as ConditionNodeData);
					break;
			}

			return new ConnectorData(new PortData(inputIndex, inputPortId), new PortData(outputIndex, outputPortId));
		}
	}
}

namespace Com.Github.Knose1.Flow.Engine.Settings.NodeData
{
	[Serializable]
	public class ConnectorData
	{
		[SerializeField] public PortData input;
		[SerializeField] public PortData output;

		public ConnectorData(PortData input, PortData output)
		{
			this.input = input;
			this.output = output;
		}

		public override bool Equals(object obj) => obj is ConnectorData data && EqualityComparer<PortData>.Default.Equals(input, data.input) && EqualityComparer<PortData>.Default.Equals(output, data.output);

		public override int GetHashCode()
		{
			var hashCode = -1523711817;
			hashCode = hashCode * -1521134295 + EqualityComparer<PortData>.Default.GetHashCode(input);
			hashCode = hashCode * -1521134295 + EqualityComparer<PortData>.Default.GetHashCode(output);
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
	public class PortData
	{
		[SerializeField] public int nodeId;
		[SerializeField] public int portId;

		public PortData(int nodeId, int portId)
		{
			this.nodeId = nodeId;
			this.portId = portId;
		}

		public override bool Equals(object obj) => obj is PortData data && nodeId == data.nodeId && portId == data.portId;

		public override int GetHashCode()
		{
			var hashCode = -443552365;
			hashCode = hashCode * -1521134295 + nodeId.GetHashCode();
			hashCode = hashCode * -1521134295 + portId.GetHashCode();
			return hashCode;
		}

		public static bool operator ==(PortData a, PortData b)
		{
			return a.nodeId == b.nodeId && a.portId == b.portId;
		}

		public static bool operator !=(PortData a, PortData b)
		{
			return !(a == b);
		}
	}

	[Serializable]
	public class NodeData
	{
		public enum NodeType
		{
			Unknown,
			Entry,
			Exit,
			State,
			Condition
		}

		/// <summary>
		/// The type of node
		/// </summary>
		[SerializeField] public NodeType type;

		/// <summary>
		/// The position of the node
		/// </summary>
		[SerializeField] public Vector2 position;

		public NodeData(Vector2 position)
		{
			SetType();
			this.position = position;
		}

		protected virtual void SetType()
		{
			type = NodeType.Unknown;
		}
	}

	[Serializable]
	public class EntryNodeData : NodeData
	{
		public EntryNodeData(Vector2 position) : base(position)
		{
		}

		protected override void SetType()
		{
			type = NodeType.Entry;
		}
	}

	[Serializable]
	public class StateNodeData : NodeData
	{
		[SerializeField] public string name;

		public StateNodeData(Vector2 position, string name) : base(position)
		{
			this.name = name;
		}

		protected override void SetType()
		{
			type = NodeType.State;
		}
	}

	[Serializable]
	public class ExitNodeData : NodeData
	{
		public ExitNodeData(Vector2 position) : base(position)
		{
		}

		protected override void SetType()
		{
			type = NodeType.Exit;
		}
	}

	[Serializable]
	public class ConditionNodeData : NodeData
	{
		public ConditionNodeData(Vector2 position) : base(position)
		{
		}

		protected override void SetType()
		{
			type = NodeType.Condition;
		}
	}
}