﻿using Com.Github.Knose1.Flow.Engine.Settings.NodeData;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Editor.Node
{
	[Obsolete]
	public class ConditionNode : FlowGraphNode
	{
		protected const string CONDITION = "Condition";
		protected const string IF = "If";
		protected const string ELSE = "Else";

		public ConditionNode() : base()
		{
			title = CONDITION;

			elementTypeColor = Color.green;

			//Generate Ports
			Port output = GeneratePort(Direction.Output, Port.Capacity.Single);
			output.SetPortName(IF);
			AddOutputElement(output);

			Port @else = GeneratePort(Direction.Output, Port.Capacity.Single);
			@else.SetPortName(ELSE);
			AddOutputElement(@else);

			Port input = GeneratePort(Direction.Input, Port.Capacity.Multi);
			input.SetPortName(PREVIOUS);
			AddInputElement(input);

			//Generate Properties


			RefreshExpandedState();
			RefreshPorts();
		}

		public override NodeData Serialize()
		{
			return new ConditionNodeData(GetPosition().position);
		}

		public static ConditionNode FromData(ConditionNodeData data)
		{
			return new ConditionNode();
		}
	}
}