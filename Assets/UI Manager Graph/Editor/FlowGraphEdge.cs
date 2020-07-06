using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow.Editor
{
	public class FlowGraphEdge : Edge
	{
		public static event Action OnChange;

		public FlowGraphEdge()
		{
			RegisterCallback<MouseDownEvent>(OnMouseDown);
		}

		private void OnMouseDown(MouseDownEvent evt)
		{
			OnChange?.Invoke();
		}
	}
}
