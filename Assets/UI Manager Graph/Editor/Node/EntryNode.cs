using Com.Github.Knose1.Flow.Engine.Settings.NodeData;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Editor.Node
{
	public class EntryNode : FlowGraphNode, IUnique
	{
		protected const string ENTRY = "Entry";

		public EntryNode() : base()
		{
			title = ENTRY;
			entryPoint = true;

			capabilities ^= Capabilities.Deletable;

			elementTypeColor = Color.red;

			Port port = GeneratePort(Direction.Output);
			port.SetPortName(NEXT);

			AddOutputElement(port);

			RefreshExpandedState();
			RefreshPorts();
		}

		public override NodeData Serialize()
		{
			return new EntryNodeData(GetPosition().position);
		}

		public static EntryNode FromData(EntryNodeData data)
		{
			return new EntryNode();
		}
	}
}