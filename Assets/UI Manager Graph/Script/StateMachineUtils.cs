using Com.Github.Knose1.Flow.Engine.Machine.State;
using System;
using System.Collections.Generic;

namespace Com.Github.Knose1.Flow.Engine.Utils
{
	public static class StateMachineUtils
	{

		public static TriggerData GetFirstCallbackByTrigger(this List<TriggerData> triggerCallbacks, string trigger, List<MachineState> negativeFilter = null)
		{
			int count = triggerCallbacks.Count;
			for (int i = 0; i < count; i++)
			{
				TriggerData callback = triggerCallbacks[i];
				if (negativeFilter != null && negativeFilter.Contains(callback.state)) continue;

				if (callback.trigger == "" || callback.trigger == trigger)
					return callback;
			}

			return null;
		}
	}

}
