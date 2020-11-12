using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow.Editor
{
	public class FlowGraphEdge : Edge
	{
		private const string CLASS_NAME = nameof(FlowGraphEdge);

		public static event Action OnChange;
		public static event Action OnChangePortChange;
		public static event Action<Vector2, Edge> OnAddReroute;

		public FlowGraphEdge() : base()
		{
			//RegisterCallback<MouseDownEvent>(OnMouseDown);
			this.AddManipulator(new ContextualMenuManipulator(BuildMenu));

			OnChange?.Invoke();
			OnChangePortChange?.Invoke();

			AddToClassList(CLASS_NAME);
		}

		private Vector2 lastMenuPosition;
		private void BuildMenu(ContextualMenuPopulateEvent obj)
		{
			lastMenuPosition = obj.mousePosition;
			obj.menu.AppendAction("Add Reroute", MenuAddReroute);
		}

		private void MenuAddReroute(DropdownMenuAction obj)
		{
			OnAddReroute?.Invoke(lastMenuPosition, this);
		}

		public override void OnUnselected()
		{
			OnChange?.Invoke();
			OnChangePortChange?.Invoke();
			
			base.OnUnselected();
		}

		protected override EdgeControl CreateEdgeControl()
		{
			return new FlowGraphEdgeControl();
		}

		

		private void OnMouseDown(MouseDownEvent evt)
		{
			OnChange?.Invoke();
		}
	}
}
