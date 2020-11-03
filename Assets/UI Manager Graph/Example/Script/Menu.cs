using Com.Github.Knose1.Flow.Engine.Machine.Interfaces;
using Com.Github.Knose1.Flow.Engine.Machine;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Example
{
	public class Menu : IState, IStateStart, IStateEnd
	{
		public void OnStart(Thread thread)
		{
			Debug.Log("Menu start");
		}

		public void OnEnd(Thread thread)
		{
			Debug.Log("Menu end");
		}
	}
}