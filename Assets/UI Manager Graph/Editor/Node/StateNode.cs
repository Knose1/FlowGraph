﻿using Com.Github.Knose1.Flow.Engine.Settings;
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
	public class StateNode : FlowGraphNode, IDisposable
	{

		//*/////////////////////////////////////*//
		//                                       //
		//              Local Class              //
		//                                       //
		//*/////////////////////////////////////*//
		
		public class StateOutputPort : GraphElement, IDisposable
		{
			private static List<StateOutputPort> _list = new List<StateOutputPort>();
			public static List<StateOutputPort> List => _list;
			public static void DisposeAll()
			{
				List<StateOutputPort> _list1 = new List<StateOutputPort>(_list);
				foreach (var item in _list1)
				{
					item.Dispose();
				}
			}

			public delegate void DelegateTriggerChange(List<string> triggers, DelegateTriggerSelected callback);
			public delegate void DelegateTriggerSelected(string selectedTrigger);
			public static event DelegateTriggerChange OnTriggerChange;
			public static event Action OnDataChange;

			public event Action<StateOutputPort> OnDestroy;

			private Port _port;
			public Port Port => _port;

			private VisualElement rootContainer;
			private TextField triggerField;
			private Toggle createThreadField;
			private VisualElement portContainer;
			private Button portRemoveButton;
			private TriggerSelectionBorder triggerSelectionBorder;

			public string Trigger
			{
				get => triggerField.value;
				set 
				{ 
					SetPortName(triggerField.value = value);
					ComputeTriggersAndSentEvent(); 
				}
			}
			public bool CreateThread
			{
				get => createThreadField.value;
				set 
				{
					createThreadField.value = value;
				}
			}

			private void SetPortName(string value)
			{
				if (value == "")
				{
					value = NEXT;
				}

				_port.portName = value;
			}

			public class TriggerSelectionBorder : VisualElement
			{
				private const string SHOW_CLASS = "show";
				private const float HEX_MAX_VALUE = 0xFF;
				
				private bool m_show;
				public bool Show 
				{
					get => m_show;
					set
					{
						m_show = value;
						if (!m_show) RemoveFromClassList(SHOW_CLASS);
						else AddToClassList(SHOW_CLASS);
					}
				}

				private static readonly Color defaultColor = new Color(0xFF / HEX_MAX_VALUE, 0x98 / HEX_MAX_VALUE, 0x44 / HEX_MAX_VALUE);
				private Color m_color = defaultColor;
				public Color Color 
				{
					get => m_color;
					set
					{
						m_color = value;

						style.borderBottomColor =
						style.borderLeftColor =
						style.borderRightColor =
						style.borderTopColor = m_color;
					}
				}

				public TriggerSelectionBorder()
				{
					Color = m_color;
					focusable = false;
					this.pickingMode = PickingMode.Ignore;
					name = "selection-border";
					AddToClassList("trigger-border");
				}
			}

			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="port">Port child</param>
			public StateOutputPort(Port port) : base()
			{
				this.elementTypeColor = new Color(0, 0, 0, 0);

				_list.Add(this);

				rootContainer = new VisualElement();
				rootContainer.name = nameof(rootContainer);
				Add(rootContainer);

				
				triggerSelectionBorder = new TriggerSelectionBorder();
				Add(triggerSelectionBorder);

				UIManagerGraphNodeExtend.Indent(this);
				style.marginTop = 10;
				style.paddingLeft = 5;
				style.paddingRight = 5;
				style.paddingTop = 5;
				
				const float BG_COLOR = 0.1037736f;
				const float BG_ALPHA = 0.6784314f;
				style.backgroundColor = new Color(BG_COLOR, BG_COLOR, BG_COLOR, BG_ALPHA);

				//Trigger Field
				triggerField = new TextField("Trigger");
				triggerField.style.width = 120;
				triggerField.RegisterValueChangedCallback(TriggerField_OnValueChanged);
				RegisterField(triggerField, VarCorrector, true, true);
				UIManagerGraphNodeExtend.CorrectLabel(triggerField.labelElement);
				rootContainer.Add(triggerField);

				//Create Thread Field
				createThreadField = new Toggle("Create Thread");
				createThreadField.style.width = 120;
				createThreadField.RegisterValueChangedCallback(ThreadField_OnValueChanged);
				UIManagerGraphNodeExtend.CorrectLabel(createThreadField.labelElement);
				UIManagerGraphNodeExtend.CorrectToggle(createThreadField);
				rootContainer.Add(createThreadField);


				//Port Container
				portContainer = new VisualElement();
				portContainer.style.flexDirection = FlexDirection.Row;
				portContainer.name = nameof(portContainer);
				rootContainer.Add(portContainer);

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

				_port.UpdatePresenterPosition();
				UpdatePresenterPosition();

				//triggerField.style.width = 300;
				//triggerField.style.height = 200;

				ComputeTriggersAndSentEvent();
			}

			private void ThreadField_OnValueChanged(ChangeEvent<bool> evt)
			{
				OnDataChange?.Invoke();
			}

			private void TriggerField_OnValueChanged(ChangeEvent<string> evt)
			{
				SetPortName(evt.newValue);
				ComputeTriggersAndSentEvent();
			}

			public StateNodeData.StateNodePort GetData()
			{
				StateNodeData.StateNodePort data = new StateNodeData.StateNodePort
				{
					trigger = Trigger,
					createThread = CreateThread
				};

				return data;
			}

			public void FromData(StateNodeData.StateNodePort data)
			{
				Trigger = data.trigger;
				CreateThread = data.createThread;
			}

			public void Dispose()
			{
				if (parent != null) parent.Remove(this);
				
				_list.Remove(this);

				ComputeTriggersAndSentEvent();

				OnDestroy?.Invoke(this);
				OnDestroy = null;
			}

			protected static void ComputeTriggersAndSentEvent()
			{
				List<string> triggers = new List<string>();
				for (int i = _list.Count - 1; i >= 0; i--)
				{
					string trigger = _list[i].Trigger;
					if (trigger != "" && triggers.Contains(trigger)) continue;

					triggers.Add(trigger);
				}

				OnTriggerChange?.Invoke(triggers, TriggerSelecedCallback);
				OnDataChange?.Invoke();
			}

			private static void TriggerSelecedCallback(string selectedTrigger)
			{
				List<StateOutputPort> _list1 = new List<StateOutputPort>(_list);
				foreach (var item in _list1)
				{
					item.ShowTrigger(item.Trigger == selectedTrigger);
				}
			}

			public void SetColor(Color color)
			{
				triggerSelectionBorder.Color = color;
				_port.portColor = color;
			}

			public void ShowTrigger(bool selected)
			{
				/*if (selected)
					Add(triggerSelectionBorder);
				else if (triggerSelectionBorder.parent == this)
					Remove(triggerSelectionBorder);*/
				triggerSelectionBorder.Show = selected;
			}
		}


		//*/////////////////////////////////////*//
		//                                       //
		//                 Const                 //
		//                                       //
		//*/////////////////////////////////////*//
		protected const string SUB_STATE_END = "SubStateEnd";
		protected const string STATE = "State";
		protected Color INSTANTIATE_COLOR = Color.cyan;
		protected Color CONSTRUCTOR_COLOR = new Color(0.6f, 1f,0.6f);
		protected Color EVENT_COLOR = new Color(177/255f, 160/255f, 246/255f);
		protected Color EMPTY_COLOR = Color.white * 4 / 5;
		protected Color SUBSTATE_COLOR = new Color(255/255f, 150/255f, 200/255f);

		//*/////////////////////////////////////*//
		//                                       //
		//                Events                 //
		//                                       //
		//*/////////////////////////////////////*//

		/// <summary>
		/// Return true if the scriptable is allowed
		/// </summary>
		/// <param name="newScriptable"></param>
		/// <param name="targetScriptable">The current graph</param>
		/// <returns></returns>
		public delegate bool DelegateSubstateChange(FlowGraphScriptable newScriptable);
		public static event DelegateSubstateChange OnSubstateChange;

		//*/////////////////////////////////////*//
		//                                       //
		//             Output Ports              //
		//                                       //
		//*/////////////////////////////////////*//
		private List<StateOutputPort> stateOutputPorts = new List<StateOutputPort>();
		private Port subMachineEndPort;

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
		
		/*private ObjectField prefabField;
		public GameObject Prefab
		{
			get => (GameObject)prefabField.value;
			set => prefabField.value = value;
		}*/
		
		private TextElement eventTextElement;

		private Toggle generateEventField;
		public bool GenerateEvent
		{
			get => generateEventField.value;
			set => generateEventField.value = value;
		}

		private ObjectField flowGraphScriptableField;
		public FlowGraphScriptable SubState
		{
			get => flowGraphScriptableField.value as FlowGraphScriptable;
			protected set => flowGraphScriptableField.value = value;
		}


		//*/////////////////////////////////////*//
		//                                       //
		//              Constructor              //
		//                                       //
		//*/////////////////////////////////////*//
		public StateNode() : base()
		{
			capabilities |= Capabilities.Renamable;
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

			subMachineEndPort = GeneratePort(Direction.Output, Port.Capacity.Single);
			subMachineEndPort.SetPortName(SUB_STATE_END);
			subMachineEndPort.portColor = SUBSTATE_COLOR;
		}

		protected override void SetupFields()
		{
			//Port Button
			portAddButton = new Button(AddStateOutputPort);
			portAddButton.text = "Add Port";
			titleContainer.Add(portAddButton);
			RegisterButton(portAddButton);

			//Name field
			nameField = new TextField("Name");
			UIManagerGraphNodeExtend.CorrectLabel(nameField.labelElement); //Correct the label style of the field
			nameField.style.width = 125; //Set its width
			nameField.RegisterValueChangedCallback(OnNameFieldChange); //Register onValueChanged
			RegisterField(nameField, VarCorrector, true, true);
			AddInspectorElement(nameField); //Add to inspector

			//Creation Mode field
			executionModeField = new EnumField("Execution Mode", StateNodeData.Execution.Instantiate);
			UIManagerGraphNodeExtend.CorrectLabel(executionModeField.labelElement);
			executionModeField.style.width = 160;
			executionModeField.RegisterValueChangedCallback(OnCreationModeFieldChange); //Register onValueChanged
			RegisterField(executionModeField);
			AddInspectorElement(executionModeField);

			//Namespace field
			namespaceField = new TextField("Namespace");
			namespaceField.tooltip = "The namespace of the class";
			UIManagerGraphNodeExtend.CorrectLabel(namespaceField.labelElement);
			UIManagerGraphNodeExtend.Indent(namespaceField, 1);
			namespaceField.style.width = 250;
			RegisterField(namespaceField, VarCorrector);

			//Class field
			classField = new TextField("Class");
			classField.tooltip = "The name of the class to use ( new MyNamespace.MyClass() )";
			UIManagerGraphNodeExtend.CorrectLabel(classField.labelElement);
			UIManagerGraphNodeExtend.Indent(classField, 1);
			classField.style.width = 160;
			RegisterField(classField, VarCorrector);

			//Substate Field
			flowGraphScriptableField = new ObjectField("Graph");
			flowGraphScriptableField.allowSceneObjects = false;
			flowGraphScriptableField.objectType = typeof(FlowGraphScriptable);
			flowGraphScriptableField.tooltip = "The subgraph";
			UIManagerGraphNodeExtend.CorrectLabel(flowGraphScriptableField.labelElement);
			flowGraphScriptableField.style.width = 180;
			flowGraphScriptableField.RegisterValueChangedCallback(OnFlowGraphScriptableFieldChange);
			UIManagerGraphNodeExtend.Indent(flowGraphScriptableField, 1);


			//Prefab field
			//prefabField = new ObjectField("Prefab");
			//prefabField.tooltip = "The prefab object to create";
			//prefabField.allowSceneObjects = false;
			//prefabField.objectType = typeof(GameObject);
			//UIManagerGraphNodeExtend.CorrectLabel(prefabField.labelElement);
			//UIManagerGraphNodeExtend.Indent(prefabField, 1);
			//prefabField.style.width = 160;
			//RegisterField(prefabField);

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
			RegisterField(generateEventField);

			StateName = STATE;	//This also sets the title of the node
			
			OnCreationModeFieldChange();
		}
		protected void AddStateOutputPort() => AddStateOutputPort(null);
		protected void AddStateOutputPort(StateNodeData.StateNodePort data, bool autoRefresh = true)
		{
			StateOutputPort output = new StateOutputPort(GeneratePort(Direction.Output, Port.Capacity.Single));
			output.SetColor(GetNodeColor());

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
			RemovePort(obj.Port);
			stateOutputPorts.Remove(obj);
			RemoveOutputElement(obj);
		}

		private void OnNameFieldChange(ChangeEvent<string> evt)
		{
			title = evt.newValue;
			UpdateEventText();
		}

		private void OnCreationModeFieldChange() => OnCreationModeFieldChange(ExecutionMode);
		private void OnCreationModeFieldChange(ChangeEvent<Enum> evt)
		{
			if (evt.newValue != evt.previousValue)
				OnCreationModeFieldChange((StateNodeData.Execution)evt.newValue);
		}

		private void OnCreationModeFieldChange(StateNodeData.Execution creationMode)
		{
			RemoveInspectorElement(namespaceField);
			RemoveInspectorElement(classField);
			//RemoveInspectorElement(prefabField);
			RemoveInspectorElement(flowGraphScriptableField);
			RemoveInspectorElement(eventTextElement);
			RemoveInspectorElement(generateEventField);

			RemovePort(subMachineEndPort);
			RemoveOutputElement(subMachineEndPort);

			switch (creationMode)
			{
				case StateNodeData.Execution.Constructor:
					UIManagerGraphNodeExtend.CorrectText(eventTextElement);
					UIManagerGraphNodeExtend.Indent(eventTextElement, 2);
					SetNodeColor(CONSTRUCTOR_COLOR);

					AddInspectorElement(namespaceField);
					AddInspectorElement(classField);
					AddInspectorElement(generateEventField);
					break;
				case StateNodeData.Execution.Instantiate:
					UIManagerGraphNodeExtend.CorrectText(eventTextElement);
					UIManagerGraphNodeExtend.Indent(eventTextElement, 2);
					SetNodeColor(INSTANTIATE_COLOR);

					//AddInspectorElement(prefabField);
					AddInspectorElement(generateEventField);
					break;
				case StateNodeData.Execution.Event:
					UIManagerGraphNodeExtend.CorrectText(eventTextElement);
					UIManagerGraphNodeExtend.Indent(eventTextElement, 1);
					SetNodeColor(EVENT_COLOR);

					AddInspectorElement(eventTextElement);
					return;
				case StateNodeData.Execution.Empty:
					SetNodeColor(EMPTY_COLOR);
					return;
				case StateNodeData.Execution.SubState:
					AddInspectorElement(flowGraphScriptableField);
					SetNodeColor(SUBSTATE_COLOR);

					InsertPort(subMachineEndPort, 1);
					InsertOutputElement(subMachineEndPort, 0);
					RefreshExpandedState();
					RefreshPorts();

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
		
		private void OnFlowGraphScriptableFieldChange(ChangeEvent<UnityEngine.Object> evt)
		{
			FlowGraphScriptable previousScriptable = evt.previousValue as FlowGraphScriptable;
			FlowGraphScriptable newScriptable = evt.newValue as FlowGraphScriptable;
			if (OnSubstateChange.Invoke(newScriptable))
			{
				flowGraphScriptableField.SetValueWithoutNotify(newScriptable);
				CallOnChange();
			}
			else 
				flowGraphScriptableField.SetValueWithoutNotify(previousScriptable);


		}



		//*/////////////////////////////////////*//
		//                                       //
		//             Serialization             //
		//                                       //
		//*/////////////////////////////////////*//
		public override NodeData Serialize()
		{
			return new StateNodeData(GetPosition().position, StateName, ExecutionMode, Namespace, Class, GenerateEvent, GetPortsData(), SubState);
		}

		public static StateNode FromData(StateNodeData data)
		{
			StateNode toReturn = new StateNode();
			toReturn.StateName = data.name;
			toReturn.ExecutionMode = data.executionMode;
			toReturn.Namespace = data.stateNamespace;
			toReturn.Class = data.stateClass;
			toReturn.GenerateEvent = data.generateEvent;
			toReturn.SubState = data.subState;

			toReturn.GeneratePortsFromData(data.ports);
			
			toReturn.OnCreationModeFieldChange();


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
			List<Port> nodesPorts = NodeHelper.GetPorts(this);
			for (int i = 0; i < count; i++)
			{
				StateOutputPort port = stateOutputPorts[i];
				StateNodeData.StateNodePort item = port.GetData();
				item.id = nodesPorts.IndexOf(port.Port);
				toReturn.Add(item);
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

		protected override void SetNodeColor(Color color)
		{
			base.SetNodeColor(color);
			foreach (StateOutputPort item in stateOutputPorts)
			{
				item.SetColor(color);
			}
		}

		public void Dispose()
		{
			for (int i = stateOutputPorts.Count - 1; i >= 0; i--)
			{
				stateOutputPorts[i].Dispose();

			}

			SubState = null;
		}
	}
}