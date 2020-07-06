using Com.Github.Knose1.Flow.Editor.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow.Editor.WindowElements
{
	public class TriggerList : VisualElement, IDisposable
	{
		ListView listView;

		public TriggerList()
		{
			this.StretchToParentSize();
			style.width = 100;
			style.backgroundColor = Color.red;

			StateNode.StateOutputPort.OnTriggerChange += StateOutputPort_OnTriggerChange;
			name = nameof(TriggerList);
			listView = new ListView();
			Add(listView);
		}

		private void StateOutputPort_OnTriggerChange(List<string> obj)
		{
			listView.itemsSource = obj;
		}

		public void Dispose()
		{
			StateNode.StateOutputPort.OnTriggerChange -= StateOutputPort_OnTriggerChange;
		}
	}
}
