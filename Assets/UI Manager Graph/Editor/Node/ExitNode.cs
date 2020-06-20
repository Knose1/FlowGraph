﻿using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Node
{
	public class ExitNode : FlowGraphNode
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
	}
}