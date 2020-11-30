using Com.Github.Knose1.Flow.Engine.Machine;
using Com.Github.Knose1.Flow.Engine.Machine.Interfaces;
using Com.Github.Knose1.Flow.Engine.Machine.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Example.BulletExample
{
	public class Timer : IState, IStateStart, IStateUpdate, IStateEnd
	{
		float time = 0;
		int modulo = 0;

		Thread paternThread;
		MachineState state = null;

		public void OnStart(Thread thread)
		{
			paternThread = thread.StateMachine.GetThreadById(2);
			paternThread.OnChange += PaternThread_OnChange;
		}

		private void PaternThread_OnChange(Thread arg1, MachineState arg2)
		{
			if (arg2 != state)
			{
				modulo = 0;
				time = 0;
			}

			state = arg2;
		}

		public void OnUpdate(Thread thread)
		{
			int oldModulo = modulo;
			time += Time.deltaTime;
			modulo = Mathf.FloorToInt(time / 1);

			if (modulo > oldModulo && modulo > 0)
			{
				for (int i = 1; i <= modulo && i < 100; i++)
				{
					thread.StateMachine.SetTrigger("Time" + (i == 1 ? "" : i.ToString()), false);
				}
			}
		}

		public void OnEnd(Thread thread)
		{
			paternThread.OnChange -= PaternThread_OnChange;
		}
	}
}
