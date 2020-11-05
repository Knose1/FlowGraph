using Com.Github.Knose1.Flow.Engine.Settings.NodeData;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow.Editor.Node
{
	public class ExitNode : FlowGraphNode
	{
		protected readonly Color STOP_THRED_COLOR = Color.yellow;
		protected readonly Color STOP_MACHINE_COLOR = Color.white;

		protected const string EXIT = "Exit";

		private EnumField exitTypeField;
		public ExitNodeData.ExitType ExitType
		{
			get => (ExitNodeData.ExitType)exitTypeField.value;
			set => exitTypeField.value = value;
		}

		public ExitNode() : base()
		{
			title = EXIT;

			//AddInspectorElement

			SetNodeColor(STOP_THRED_COLOR);

			//Creation Mode field
			exitTypeField = new EnumField("Exit Type", ExitNodeData.ExitType.StopThread);
			UIManagerGraphNodeExtend.CorrectLabel(exitTypeField.labelElement);
			exitTypeField.style.width = 160;
			exitTypeField.RegisterValueChangedCallback(OnExitTypeFieldChange); //Register onValueChanged
			RegisterField(exitTypeField);
			AddInspectorElement(exitTypeField);

			//Setup port
			Port port = GeneratePort(Direction.Input, Port.Capacity.Multi);
			port.SetPortName(PREVIOUS);

			AddInputElement(port);

			RefreshExpandedState();
			RefreshPorts();
		}

		private void OnExitTypeFieldChange() => OnExitTypeFieldChange(ExitType);
		private void OnExitTypeFieldChange(ChangeEvent<Enum> evt) => OnExitTypeFieldChange((ExitNodeData.ExitType)evt.newValue);
		private void OnExitTypeFieldChange(ExitNodeData.ExitType exitTypeField)
		{
			switch (exitTypeField)
			{
				case ExitNodeData.ExitType.StopThread:
					SetNodeColor(STOP_THRED_COLOR);
					break;
				case ExitNodeData.ExitType.StopMachine:
					SetNodeColor(STOP_MACHINE_COLOR);
					break;
			}
		}

		public override NodeData Serialize()
		{
			return new ExitNodeData(GetPosition().position, ExitType);
		}

		public static ExitNode FromData(ExitNodeData data)
		{
			ExitNode exitNode = new ExitNode();
			exitNode.ExitType = data.exitType;
			exitNode.OnExitTypeFieldChange();

			return exitNode;
		}
	}
}