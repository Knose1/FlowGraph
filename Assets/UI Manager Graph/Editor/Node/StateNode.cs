using Com.Github.Knose1.Flow.Engine.Settings.NodeData;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow.Editor.Node
{
	public class StateNode : FlowGraphNode
	{
		//*/////////////////////////////////////*//
		//                                       //
		//              Local Class              //
		//                                       //
		//*/////////////////////////////////////*//
		private class StateOutputPort : VisualElement, IDisposable
		{
			public event Action<StateOutputPort> OnDestroy;

			private Port _port;
			public Port Port => _port;

			private TextField triggerField;
			private VisualElement portContainer;
			private Button portRemoveButton;

			public string Trigger
			{
				get => triggerField.value;
				set => SetPortName(triggerField.value = value);
			}

			private void SetPortName(string value)
			{
				if (value == "")
				{
					value = NEXT;
				}

				_port.portName = value;
			}

			public StateOutputPort(Port port)
			{
				UIManagerGraphNodeExtend.Indent(this);
				style.marginTop = 10;


				//Trigger Field
				triggerField = new TextField("Trigger");
				triggerField.style.width = 120;
				triggerField.RegisterValueChangedCallback(TriggerField_OnValueChanged);
				UIManagerGraphNodeExtend.CorrectLabel(triggerField.labelElement);
				Add(triggerField);


				//Port Container
				portContainer = new VisualElement();
				portContainer.style.flexDirection = FlexDirection.Row;
				portContainer.name = nameof(portContainer);
				Add(portContainer);

				//Remove port button
				portRemoveButton = new Button(Dispose);
				portRemoveButton.style.height = 16;
				portRemoveButton.style.width = 50;
				portRemoveButton.text = "Remove";
				portContainer.Add(portRemoveButton);

				//Port
				_port = port;
				SetPortName("");
				portContainer.Add(_port);


				//triggerField.style.width = 300;
				//triggerField.style.height = 200;
			}

			private void TriggerField_OnValueChanged(ChangeEvent<string> evt)
			{
				SetPortName(evt.newValue);
			}

			public StateNodeData.StateNodePort GetData()
			{
				StateNodeData.StateNodePort data = new StateNodeData.StateNodePort
				{
					trigger = Trigger
				};

				return data;
			}

			public void FromData(StateNodeData.StateNodePort data)
			{
				Trigger = data.trigger;
			}

			public void Dispose()
			{
				if (parent != null) parent.Remove(this);

				OnDestroy?.Invoke(this);
				OnDestroy = null;
			}
		}


		//*/////////////////////////////////////*//
		//                                       //
		//                 Const                 //
		//                                       //
		//*/////////////////////////////////////*//
		protected const string STATE = "State";


		//*/////////////////////////////////////*//
		//                                       //
		//                 Field                 //
		//                                       //
		//*/////////////////////////////////////*//
		private List<StateOutputPort> stateOutputPorts = new List<StateOutputPort>();


		//*/////////////////////////////////////*//
		//                                       //
		//      UIFields and their property      //
		//                                       //
		//*/////////////////////////////////////*//
		private Button portAddButton;

		private TextField nameField;
		public string StateName
		{
			get => nameField.value;
			set {
				title = nameField.value = value;
				UpdateEventText();
			}
		}


		private EnumField executionModeField;
		public StateNodeData.Execution ExecutionMode
		{
			get => (StateNodeData.Execution)executionModeField.value;
			set => executionModeField.value = value;
		}
		
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
		
		private ObjectField prefabField;
		public GameObject Prefab
		{
			get => (GameObject)prefabField.value;
			set => prefabField.value = value;
		
		}
		
		private TextElement eventTextElement;

		private Toggle generateEventField;
		public bool GenerateEvent
		{
			get => generateEventField.value;
			set => generateEventField.value = value;
		}


		//*/////////////////////////////////////*//
		//                                       //
		//              Constructor              //
		//                                       //
		//*/////////////////////////////////////*//
		public StateNode() : base()
		{
			capabilities |= Capabilities.Renamable;
			SetTopColor(elementTypeColor = Color.cyan);
		}


		//*/////////////////////////////////////*//
		//                                       //
		//        Setup (port and fields)        //
		//                                       //
		//*/////////////////////////////////////*//
		protected override void SetupPorts()
		{
			Port input = GeneratePort(Direction.Input, Port.Capacity.Multi);
			input.SetPortName(PREVIOUS);
			AddInputElement(input);
		}

		protected override void SetupFields()
		{
			//Port Button
			portAddButton = new Button(AddStateOutputPort);
			portAddButton.text = "Add Port";
			titleContainer.Add(portAddButton);

			//Name field
			nameField = new TextField("Name");
			UIManagerGraphNodeExtend.CorrectLabel(nameField.labelElement); //Correct the label style of the field
			nameField.style.width = 125; //Set its width
			nameField.RegisterValueChangedCallback(OnNameFieldChange); //Register onValueChanged
			AddInspectorElement(nameField); //Add to inspector

			//CreationMode field
			executionModeField = new EnumField("Execution Mode", StateNodeData.Execution.Instantiate);
			UIManagerGraphNodeExtend.CorrectLabel(executionModeField.labelElement);
			executionModeField.style.width = 160;
			executionModeField.RegisterValueChangedCallback(OnCreationModeFieldChange); //Register onValueChanged
			AddInspectorElement(executionModeField);

			//Namespace field
			namespaceField = new TextField("Namespace");
			namespaceField.tooltip = "The namespace of the class";
			UIManagerGraphNodeExtend.CorrectLabel(namespaceField.labelElement);
			UIManagerGraphNodeExtend.Indent(namespaceField, 1);
			namespaceField.style.width = 250;

			//Class field
			classField = new TextField("Class");
			classField.tooltip = "The name of the class to use ( new MyNamespace.MyClass() )";
			UIManagerGraphNodeExtend.CorrectLabel(classField.labelElement);
			UIManagerGraphNodeExtend.Indent(classField, 1);
			classField.style.width = 160;

			//Prefab field
			prefabField = new ObjectField("Prefab");
			prefabField.tooltip = "The prefab object to create";
			prefabField.allowSceneObjects = false;
			prefabField.objectType = typeof(GameObject);
			UIManagerGraphNodeExtend.CorrectLabel(prefabField.labelElement);
			UIManagerGraphNodeExtend.Indent(prefabField, 1);
			prefabField.style.width = 160;
			
			//Event text
			eventTextElement = new TextElement();
			eventTextElement.tooltip = "The name of the event";
			eventTextElement.style.width = 160;
			UIManagerGraphNodeExtend.CorrectText(eventTextElement);

			//Generate event toggle
			generateEventField = new Toggle("Generate Event");
			generateEventField.tooltip = "When checked, generate an event";
			UIManagerGraphNodeExtend.CorrectLabel(generateEventField.labelElement);
			generateEventField.style.width = 160;
			generateEventField.RegisterValueChangedCallback(OnGenerateEventFieldChange);
			UIManagerGraphNodeExtend.CorrectToggle(generateEventField);
			UIManagerGraphNodeExtend.Indent(generateEventField, 1);

			StateName = STATE;	//This also sets the title of the node
			
			OnCreationModeFieldChange();
		}

		protected void AddStateOutputPort() => AddStateOutputPort(null);
		protected void AddStateOutputPort(StateNodeData.StateNodePort data, bool autoRefresh = true)
		{
			StateOutputPort output = new StateOutputPort(GeneratePort(Direction.Output, Port.Capacity.Multi));
			AddOutputElement(output);
			output.OnDestroy += StateOutput_OnDestroy;
			stateOutputPorts.Add(output);

			if (data != null) output.FromData(data);

			if (autoRefresh)
			{
				RefreshExpandedState();
				RefreshPorts();
			}
		}


		//*/////////////////////////////////////*//
		//                                       //
		//            Event handelers            //
		//                                       //
		//*/////////////////////////////////////*//
		private void StateOutput_OnDestroy(StateOutputPort obj)
		{
			Ports.Remove(obj.Port);
			stateOutputPorts.Remove(obj);
			RemoveOutputElement(obj);
		}

		private void OnNameFieldChange(ChangeEvent<string> evt)
		{
			title = evt.newValue;
			UpdateEventText();
		}

		private void OnCreationModeFieldChange() => OnCreationModeFieldChange(ExecutionMode);
		private void OnCreationModeFieldChange(ChangeEvent<Enum> evt) => OnCreationModeFieldChange((StateNodeData.Execution)evt.newValue);
		private void OnCreationModeFieldChange(StateNodeData.Execution creationMode)
		{
			RemoveInspectorElement(namespaceField);
			RemoveInspectorElement(classField);
			RemoveInspectorElement(prefabField);
			RemoveInspectorElement(eventTextElement);
			RemoveInspectorElement(generateEventField);

			switch (creationMode)
			{
				case StateNodeData.Execution.Constructor:
					UIManagerGraphNodeExtend.CorrectText(eventTextElement);
					UIManagerGraphNodeExtend.Indent(eventTextElement, 2);

					AddInspectorElement(namespaceField);
					AddInspectorElement(classField);
					AddInspectorElement(generateEventField);
					break;
				case StateNodeData.Execution.Instantiate:
					UIManagerGraphNodeExtend.CorrectText(eventTextElement);
					UIManagerGraphNodeExtend.Indent(eventTextElement, 2);

					AddInspectorElement(prefabField);
					AddInspectorElement(generateEventField);
					break;
				case StateNodeData.Execution.Event:
					UIManagerGraphNodeExtend.CorrectText(eventTextElement);
					UIManagerGraphNodeExtend.Indent(eventTextElement, 1);
					AddInspectorElement(eventTextElement);
					return;

				default: 
					return;
			}

			OnGenerateEventFieldChange();
		}

		private void OnGenerateEventFieldChange() => OnGenerateEventFieldChange(GenerateEvent);
		private void OnGenerateEventFieldChange(ChangeEvent<bool> evt) => OnGenerateEventFieldChange(evt.newValue);
		private void OnGenerateEventFieldChange(bool toggled) 
		{
			if (toggled)
				AddInspectorElement(eventTextElement);
			else
				RemoveInspectorElement(eventTextElement);
		}


		//*/////////////////////////////////////*//
		//                                       //
		//             Serialization             //
		//                                       //
		//*/////////////////////////////////////*//
		public override NodeData Serialize()
		{
			return new StateNodeData(GetPosition().position, StateName, ExecutionMode, Namespace, Class, Prefab, GenerateEvent, GetPortsData());
		}

		public static StateNode FromData(StateNodeData data)
		{
			StateNode toReturn = new StateNode();
			toReturn.StateName = data.name;
			toReturn.ExecutionMode = data.executionMode;
			toReturn.Namespace = data.@namespace;
			toReturn.Class = data.@class;
			toReturn.Prefab = data.prefab;
			toReturn.GenerateEvent = data.generateEvent;
			toReturn.OnCreationModeFieldChange();

			toReturn.GeneratePortsFromData(data.ports);

			toReturn.RefreshExpandedState();
			toReturn.RefreshPorts();


			return toReturn;
		}

		protected void GeneratePortsFromData(List<StateNodeData.StateNodePort> ports)
		{
			int count = ports.Count;
			for (int i = 0; i < count; i++)
			{
				AddStateOutputPort(ports[i], false);
			}
		}

		protected List<StateNodeData.StateNodePort> GetPortsData()
		{
			List<StateNodeData.StateNodePort> toReturn = new List<StateNodeData.StateNodePort>();

			int count = stateOutputPorts.Count;
			for (int i = 0; i < count; i++)
			{
				StateOutputPort port = stateOutputPorts[i];
				toReturn.Add(port.GetData());
			}

			return toReturn;
		}
		

		//*/////////////////////////////////////*//
		//                                       //
		//                 Other                 //
		//                                       //
		//*/////////////////////////////////////*//
		private void UpdateEventText()
		{
			eventTextElement.text = "Event : "+StateNodeData.GetEventName(StateName);
		}
	}
}