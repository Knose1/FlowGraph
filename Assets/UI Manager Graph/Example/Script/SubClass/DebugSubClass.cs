using Com.Github.Knose1.Flow.Engine.Machine;
using Com.Github.Knose1.Flow.Engine.Machine.Interfaces;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Example.SubClass
{

	public class DebugSubClass : IState, IStateStart
	{
		public void OnStart(Thread thread)
		{
			Debug.Log(thread.Machine.GetType().Name + ": " + thread.CurrentState.name);
		}
	}
}
