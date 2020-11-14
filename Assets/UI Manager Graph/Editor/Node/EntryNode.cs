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
		private const string ENTRY = "Entry";
		protected const string TITLE_STATE_MACHINE = "State Machine "+ENTRY;
		private const string TITLE_SUBSTATE_MACHINE = "SubState Machine "+ENTRY;
		private const string NAMESPACE = "Namespace";
		private const string RELATIVE_NAMESPACE = "Relative "+NAMESPACE;
		private TextElement generatedClassTextElement;

		private TextField namespaceField;

		public void SetSubState()
		{
			title = TITLE_SUBSTATE_MACHINE;
			string value = Namespace;
			namespaceField.label = RELATIVE_NAMESPACE;
			Namespace = value;
		}

		public string Namespace
		{
			get => namespaceField.value;
			set => namespaceField.value = value;
		}

		private TextField classField;
		public string Class
		{
			get => classField.value;
			set => classField.value = value;
		}

		public EntryNode() : base()
		{
			title = TITLE_STATE_MACHINE;
			capabilities ^= Capabilities.Deletable;
			SetNodeColor(Color.red);
		}

		protected override void SetupPorts()
		{
			Port port = GeneratePort(Direction.Output);
			port.SetPortName(NEXT);
			
			AddOutputElement(port);
		}

		protected override void SetupFields()
		{
			generatedClassTextElement = new TextElement();
			generatedClassTextElement.tooltip = "The class that will be generated";
			generatedClassTextElement.text = "Generated class";
			UIManagerGraphNodeExtend.CorrectText(generatedClassTextElement);
			AddInspectorElement(generatedClassTextElement);

			//Namespace field
			namespaceField = new TextField(NAMESPACE);
			namespaceField.tooltip = "The namespace of the class";
			UIManagerGraphNodeExtend.CorrectLabel(namespaceField.labelElement);
			namespaceField.style.width = 250;
			UIManagerGraphNodeExtend.Indent(namespaceField);
			RegisterField(namespaceField, VarCorrector);
			AddInspectorElement(namespaceField);

			//Class field
			classField = new TextField("Class");
			classField.tooltip = "The name of the class to use ( new MyNamespace.MyClass() )";
			UIManagerGraphNodeExtend.CorrectLabel(classField.labelElement);
			classField.style.width = 160;
			UIManagerGraphNodeExtend.Indent(classField);
			RegisterField(classField, VarCorrector);
			AddInspectorElement(classField);
		}

		public override NodeData Serialize()
		{
			return new EntryNodeData(GetPosition().position, Namespace, Class);
		}

		public static EntryNode FromData(EntryNodeData data)
		{
			EntryNode node = new EntryNode();
			node.Class = data.stateClass;
			node.Namespace = data.stateNamespace;
			return node;
		}
	}
}