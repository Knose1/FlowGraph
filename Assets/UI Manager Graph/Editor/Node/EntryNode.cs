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

		private TextElement generatedClassTextElement;

		private TextField namespaceField;
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
			title = ENTRY;
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
			namespaceField = new TextField("Namespace");
			namespaceField.tooltip = "The namespace of the class";
			UIManagerGraphNodeExtend.CorrectLabel(namespaceField.labelElement);
			namespaceField.style.width = 250;
			UIManagerGraphNodeExtend.Indent(namespaceField);
			AddInspectorElement(namespaceField);

			//Class field
			classField = new TextField("Class");
			classField.tooltip = "The name of the class to use ( new MyNamespace.MyClass() )";
			UIManagerGraphNodeExtend.CorrectLabel(classField.labelElement);
			classField.style.width = 160;
			UIManagerGraphNodeExtend.Indent(classField);
			AddInspectorElement(classField);
		}

		public override NodeData Serialize()
		{
			return new EntryNodeData(GetPosition().position, Namespace, Class);
		}

		public static EntryNode FromData(EntryNodeData data)
		{
			EntryNode node = new EntryNode();
			node.Class = data.@class;
			node.Namespace = data.@namespace;
			return node;
		}
	}
}