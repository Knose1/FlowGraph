using Com.Github.Knose1.Flow.Engine.Settings.NodeData;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow.Editor.Node
{
	public class EntryNode : FlowGraphNode, IUnique
	{
		protected const string ENTRY = "Entry";

		private EnumField startField;
		public EntryNodeData.StartMode Start
		{
			get => (EntryNodeData.StartMode)startField.value;
			set => startField.value = value;
		}

		private Toggle generateCallbackField;
		public bool GenerateCallback
		{
			get => generateCallbackField.value;
			set => generateCallbackField.value = value;
		}

		public EntryNode() : base()
		{
			title = ENTRY;
			capabilities ^= Capabilities.Deletable;
			elementTypeColor = Color.red;

			//Port
			Port port = GeneratePort(Direction.Output);
			port.SetPortName(NEXT);

			AddOutputElement(port);

			//Fields
			startField = new EnumField("Start", EntryNodeData.StartMode.StartOnAwake); 
			CorrectLabel(startField.labelElement);
			startField.style.width = 125;
			AddInspectorElement(startField);

			generateCallbackField = new Toggle("Generate Callback"); 
			CorrectLabel(generateCallbackField.labelElement);
			generateCallbackField.style.width = 125;
			AddInspectorElement(generateCallbackField);

			//Refresh
			RefreshExpandedState();
			RefreshPorts();
		}

		public override NodeData Serialize()
		{
			return new EntryNodeData(GetPosition().position, Start, GenerateCallback);
		}

		public static EntryNode FromData(EntryNodeData data)
		{
			EntryNode node = new EntryNode();
			node.Start = data.startMode;
			node.GenerateCallback = data.generateCallback;
			return node;
		}
	}
}