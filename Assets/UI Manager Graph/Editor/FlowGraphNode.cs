using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow.Editor
{

	/// <summary>
	/// Base Node class for <see cref="FlowGraph"/>
	/// </summary>
	public abstract class FlowGraphNode : UnityEditor.Experimental.GraphView.Node
	{
		protected const string NEXT = "Next";
		protected const string OUTPUT = "Output";
		protected const string INPUT = "Input";
		protected const string PREVIOUS = "Previous";

		public string GUID;
		public bool entryPoint;

		protected VisualElement inspectorElement;
		protected List<Port> _ports = new List<Port>();

		/// <summary>
		/// Node's ports
		/// </summary>
		public List<Port> Ports => _ports;

		protected FlowGraphNode() : this(new Vector2(100, 150)) { }
		protected FlowGraphNode(Vector2 startSize) : base()
		{
			SetPosition(new Rect(Vector2.zero, startSize));
			GUID = Guid.NewGuid().ToString();

			capabilities ^= Capabilities.Collapsible;

			inspectorElement = new VisualElement();
			inspectorElement.name = nameof(inspectorElement);
			topContainer.Insert(1, inspectorElement);

			titleButtonContainer.parent.Remove(titleButtonContainer);

			RefreshExpandedState();
			RefreshPorts();
		}

		protected Port GeneratePort(Direction direction, Port.Capacity capacity = Port.Capacity.Single)
		{
			Port port = InstantiatePort(Orientation.Horizontal, direction, capacity, null);
			_ports.Add(port);
			return port;
		}

		protected Port GeneratePort<T>(Direction direction, Port.Capacity capacity = Port.Capacity.Single)
		{
			Port port = InstantiatePort(Orientation.Horizontal, direction, capacity, typeof(T));
			_ports.Add(port);
			return port;
		}

		protected void AddOutputElement(VisualElement elm)
		{
			outputContainer.Add(elm);
		}

		protected void AddInputElement(VisualElement elm)
		{
			inputContainer.Add(elm);
		}

		protected void AddInspectorElement(VisualElement elm)
		{
			inspectorElement.Add(elm);

			inspectorElement.style.paddingLeft = 3;
			inspectorElement.style.paddingRight = 3;
			inspectorElement.style.paddingTop = 3;
			inspectorElement.style.paddingBottom = 3;
		}
	}

	public static class UIManagerGraphNodeExtend
	{
		public static void SetPortName(this Port port, string name)
		{
			port.portName = name;
		}
	}
}