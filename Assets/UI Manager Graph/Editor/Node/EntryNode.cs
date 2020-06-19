using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Com.Github.Knose1.UiManagerGraph.Node
{
	public class EntryNode : UIManagerGraphNode
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
	}
}