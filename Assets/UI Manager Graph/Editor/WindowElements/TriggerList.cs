using Com.Github.Knose1.Flow.Editor.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow.Editor.WindowElements
{
	public class TriggerList : VisualElement, IDisposable
	{
		ListView listView;

		List<string> labels;
		private StateNode.StateOutputPort.DelegateTriggerSelected callback;

		public TriggerList()
		{
			this.name = nameof(TriggerList);
			this.AddToClassList(nameof(TriggerList));

			StateNode.StateOutputPort.OnTriggerChange += StateOutputPort_OnTriggerChange;
			name = nameof(TriggerList);

			TextElement title = new TextElement();
			title.text = "Trigger List";
			Add(title);

			GenerateListView();
		}

		public void UpdateSize()
		{
			this.StretchToParentSize();
		}

		private void GenerateListView()
		{
			// The "makeItem" function will be called as needed
			// when the ListView needs more items to render
			Func<VisualElement> makeItem = () => new Label();

			// As the user scrolls through the list, the ListView object
			// will recycle elements created by the "makeItem"
			// and invoke the "bindItem" callback to associate
			// the element with the matching data item (specified as an index in the list)
			Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = "\'"+labels[i]+"\'";

			const int itemHeight = 16;

			listView = new ListView(labels, itemHeight, makeItem, bindItem);
			listView.selectionType = SelectionType.Single;
			listView.onSelectionChanged += ListView_onSelectionChanged; ;
			listView.onItemChosen += ListView_onItemChosen;
			listView.style.flexGrow = 1.0f;

			Add(listView);
		}

		private void ListView_onSelectionChanged(List<object> obj)
		{
			if (obj.Count > 0) ListView_onItemChosen(obj[0]);
			else ListView_onItemChosen(null);
		}

		private void ListView_onItemChosen(object obj)
		{
			string label = (obj as string);
			callback(label);
		}

		private void StateOutputPort_OnTriggerChange(List<string> triggers, StateNode.StateOutputPort.DelegateTriggerSelected callback)
		{
			this.callback = callback;
			List<string> labels = new List<string>();
			foreach (var item in triggers)
			{
				if (labels.Contains(item)) continue;

				labels.Add(item);
			}

			labels.Sort((string a, string b) => a.Length - b.Length);

			this.labels = labels;
			listView.itemsSource = this.labels;
			ListView_onItemChosen(listView.selectedItem);
		}
		public void UnSelectItems()
		{
			listView.selectedIndex = -1;
		}

		public void Dispose()
		{
			StateNode.StateOutputPort.OnTriggerChange -= StateOutputPort_OnTriggerChange;
		}

	}
}
