using Com.Github.Knose1.Flow.Engine.Machine;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Github.Knose1.Flow.Example
{
	public class TriggerSetter : MonoBehaviour
	{
		public InputField field;
		public Button btn;
		public StateMachine stateMachine;

		protected string trigger;

		private void Awake()
		{
			field.onValueChanged.AddListener(OnValueChange);
			btn.onClick.AddListener(OnSubmit);
		}

		private void OnValueChange(string arg0)
		{
			trigger = arg0;
		}

		private void OnSubmit()
		{
			stateMachine.SetTrigger(trigger);
			field.text = "";
			trigger = "";
		}
	}
}