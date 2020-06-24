﻿using Com.Github.Knose1.Flow.Engine.Settings.NodeData;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow.Editor.Node
{
	public class StateNode : FlowGraphNode
	{
		protected const string STATE = "State";

		private TextField nameField;
		public string StateName
		{
			get => nameField.value;
			set => title = nameField.value = value;
		}

		public StateNode() : base()
		{
			capabilities |= Capabilities.Renamable;
			elementTypeColor = Color.cyan;

			//Ports
			Port output = GeneratePort(Direction.Output, Port.Capacity.Single);
			output.SetPortName(NEXT);
			AddOutputElement(output);

			Port input = GeneratePort(Direction.Input, Port.Capacity.Multi);
			input.SetPortName(PREVIOUS);
			AddInputElement(input);

			//Fields
			nameField = new TextField("Name");
			StateName = STATE; //This also sets the title
			CorrectLabel(nameField.labelElement);
			nameField.style.width = 125;

			nameField.RegisterValueChangedCallback(OnNameFieldChange);
			AddInspectorElement(nameField);


			//Refresh
			RefreshExpandedState();
			RefreshPorts();
		}

		private void OnNameFieldChange(ChangeEvent<string> evt)
		{
			title = evt.newValue;
		}

		public override NodeData Serialize()
		{
			return new StateNodeData(GetPosition().position, StateName);
		}

		public static StateNode FromData(StateNodeData data)
		{
			StateNode toReturn = new StateNode();
			toReturn.StateName = data.name;

			return toReturn;
		}
	}
}