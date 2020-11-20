using Com.Github.Knose1.Flow.Engine.Machine;
using Com.Github.Knose1.Flow.Engine.Machine.Interfaces;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Example.SubClass
{

	public class DebugSubClass : IState, IStateStart
	{
		public void OnStart(Thread thread)
		{
			StateMachine.Machine machine = thread.Machine;
			StateMachine.Machine parent = machine.Parent;
			string name = machine.GetType().Name;
			if (parent != null)
			{
				name = parent.GetType().Name+" | "+name;
				Debug.Log(name + ": " + thread.CurrentState.name);
			}

		}
	}
}
