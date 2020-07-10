using Com.Github.Knose1.Flow.Engine.Settings.NodeData;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow.Editor
{
	/// <summary>
	/// Mark a node as Unique
	/// </summary>
	public interface IUnique {}

	/// <summary>
	/// Base Node class for <see cref="FlowGraph"/>
	/// </summary>
	public abstract class FlowGraphNode : UnityEditor.Experimental.GraphView.Node
	{
		public static event Action OnChange;

		private const int TITLE_BORDER_TOP_WIDTH = 2;

		protected const string NEXT = "Next";
		protected const string OUTPUT = "Output";
		protected const string INPUT = "Input";
		protected const string PREVIOUS = "Previous";
		protected VisualElement inspectorElement;
		private List<Port> _ports = new List<Port>();

		/// <summary>
		/// Node's ports
		/// </summary>
		public List<Port> Ports => _ports;

		protected FlowGraphNode() : this(new Vector2(100, 150)) { }
		protected FlowGraphNode(Vector2 startSize) : base()
		{
			SetPosition(new Rect(Vector2.zero, startSize));
			
			capabilities ^= Capabilities.Collapsible;

			inspectorElement = new VisualElement();
			inspectorElement.name = nameof(inspectorElement);
			float inspectorColor = 0x28/(float)0xff;
			inspectorElement.style.backgroundColor = new Color(inspectorColor, inspectorColor, inspectorColor, 0.8f);
			topContainer.Insert(1, inspectorElement);

			titleButtonContainer.parent.Remove(titleButtonContainer);

			SetupPorts();
			SetupFields();

			RefreshExpandedState();
			RefreshPorts();

			RegisterCallback<MouseDownEvent>(OnMouseDown);
			
		}

		private void OnMouseDown(MouseDownEvent evt)
		{
			OnChange?.Invoke();
		}

		protected void RemoveTopColor()
		{
			titleContainer.style.borderTopWidth = 0;
		}

		protected void SetNodeColor(Color color)
		{
			titleContainer.style.borderColor = color;
			titleContainer.style.borderTopWidth = TITLE_BORDER_TOP_WIDTH;
			elementTypeColor = color;
		}

		public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
		{
			return Port.Create<FlowGraphEdge>(orientation, direction, capacity, type);
		}

		protected Port GeneratePort(Direction direction, Port.Capacity capacity = Port.Capacity.Single)
		{
			Port port = InstantiatePort(Orientation.Horizontal, direction, capacity, null);
			port.RegisterCallback<MouseDownEvent>(OnMouseDown);
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

		protected void RemoveOutputElement(VisualElement elm)
		{
			if (!outputContainer.Contains(elm)) return;
			outputContainer.Remove(elm);
		}

		protected void AddInputElement(VisualElement elm)
		{
			inputContainer.Add(elm);
		}

		protected void RemoveInputElement(VisualElement elm)
		{
			inputContainer.Remove(elm);
		}

		protected void RemovePort(Port port)
		{
			_ports.Remove(port);
			port.UnregisterCallback<MouseDownEvent>(OnMouseDown);

			List<Edge> connections = port.connections.ToList();
			
			for (int i = connections.Count - 1; i >= 0; i--)
			{
				Edge edge = connections[i];
				port.Disconnect(edge);
				edge.RemoveFromHierarchy();
			}

			OnChange?.Invoke();
		}

		protected void AddInspectorElement(VisualElement elm)
		{
			inspectorElement.Add(elm);

			inspectorElement.style.paddingLeft = 3;
			inspectorElement.style.paddingRight = 3;
			inspectorElement.style.paddingTop = 3;
			inspectorElement.style.paddingBottom = 3;
		}

		protected void RemoveInspectorElement(VisualElement elm)
		{
			if (!inspectorElement.Contains(elm)) return;

			inspectorElement.Remove(elm);

			if (inspectorElement.childCount != 0) return;

			inspectorElement.style.paddingLeft = 0;
			inspectorElement.style.paddingRight = 0;
			inspectorElement.style.paddingTop = 0;
			inspectorElement.style.paddingBottom = 0;
		}

		protected virtual void SetupPorts()	 { }
		protected virtual void SetupFields() { }

		protected void RegisterField<T>(BaseField<T> field)
		{
			field.RegisterValueChangedCallback(Field_OnValueChanged);
		}

		private void Field_OnValueChanged<T>(ChangeEvent<T> evt)
		{
			OnChange?.Invoke();
		}

		protected void RegisterButton(Button button)
		{
			button.clickable.clicked += Button_OnClick;
		}

		private void Button_OnClick()
		{
			OnChange?.Invoke();
		}

		public abstract NodeData Serialize();
	}

	public static class UIManagerGraphNodeExtend
	{
		public const int INDENTATION_SIZE = 10;

		public static T SetPositionFromData<T>(this T node, Vector2 position) where T : UnityEditor.Experimental.GraphView.Node
		{
			Rect pos = node.GetPosition();
			pos.position = position;
			node.SetPosition(pos);
			return node;
		}

		public static void SetPortName(this Port port, string name)
		{
			port.portName = name;
		}

		public static void CorrectLabel(Label labelElement)
		{
			labelElement.style.minWidth = 30;
			labelElement.style.unityTextAlign = TextAnchor.MiddleLeft;
		}

		public static void CorrectText(TextElement txt)
		{
			txt.style.height = 16;
			txt.style.marginTop = 0;
			txt.style.marginBottom = 2;
			txt.style.marginLeft = 2;
		}

		public static void CorrectToggle(VisualElement elm)
		{
			elm.style.marginBottom = 2;
		}

		/// <summary>
		/// Warning : This methode modify the value of <see cref="VisualElement.style.marginLeft"/>
		/// </summary>
		/// <param name="elm"></param>
		/// <param name="indentationCount"></param>
		public static void Indent(VisualElement elm, int indentationCount = 1, bool relatif = true)
		{
			elm.style.marginLeft = (relatif ? elm.style.marginLeft.value.value : 0) + indentationCount * INDENTATION_SIZE;
		}
	}
}