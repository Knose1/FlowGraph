using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Editor
{
	public class FlowGraphEdgeControl : EdgeControl
	{
		[Obsolete]
		public static event Action OnChange;


		public FlowGraphEdgeControl() : base() {}

		protected override void PointsChanged()
		{
			base.PointsChanged();
			OnChange?.Invoke();
		}
	}
}
