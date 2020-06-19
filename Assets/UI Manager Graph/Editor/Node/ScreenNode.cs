﻿using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

namespace Com.Github.Knose1.UiManagerGraph.Node {
	public class ScreenNode : UIManagerGraphNode
	{
		protected const string SCREEN = "Screen";

		private TextField nameField;
		public string ScreenName => nameField.value;

		public ScreenNode() : base()
		{
			title = SCREEN;
			
			nameField = new TextField("Name");
			nameField.value = SCREEN;
			nameField.labelElement.style.minWidth = 30;
			nameField.labelElement.style.unityTextAlign = TextAnchor.MiddleLeft;
			nameField.style.width = 125;

			nameField.RegisterValueChangedCallback(OnNameFieldChange);


			AddInspectorElement(nameField);
			entryPoint = true;

			capabilities |= Capabilities.Renamable;

			elementTypeColor = Color.cyan;

			Port output = GeneratePort(Direction.Output, Port.Capacity.Single);
			output.SetPortName(NEXT);
			AddOutputElement(output);

			Port input = GeneratePort(Direction.Input, Port.Capacity.Multi);
			input.SetPortName(PREVIOUS);
			AddInputElement(input);

			RefreshExpandedState();
			RefreshPorts();
		}

		private void OnNameFieldChange(ChangeEvent<string> evt)
		{
			title = evt.newValue;
		}
	}
}