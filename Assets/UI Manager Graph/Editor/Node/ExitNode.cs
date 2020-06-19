using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Com.Github.Knose1.UiManagerGraph.Node
{
	public class ExitNode : UIManagerGraphNode
	{
		protected const string EXIT = "Exit Game";

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
	}
}