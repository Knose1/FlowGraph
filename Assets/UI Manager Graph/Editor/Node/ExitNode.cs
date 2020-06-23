using Com.Github.Knose1.Flow.Engine.Settings.NodeData;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Editor.Node
{
	public class ExitNode : FlowGraphNode, IUnique
	{
		protected const string EXIT = "Exit";

		public ExitNode() : base()
		{
			title = EXIT;
			entryPoint = true;

			capabilities ^= Capabilities.Deletable;

			elementTypeColor = Color.red;

			Port port = GeneratePort(Direction.Input, Port.Capacity.Multi);
			port.SetPortName(PREVIOUS);

			AddInputElement(port);

			RefreshExpandedState();
			RefreshPorts();
		}

		public override NodeData Serialize()
		{
			return new ExitNodeData(GetPosition().position);
		}

		public static ExitNode FromData(ExitNodeData data)
		{
			return new ExitNode();
		}
	}
}