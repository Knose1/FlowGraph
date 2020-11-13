using Com.Github.Knose1.Flow.Engine.Machine;
using Com.Github.Knose1.Flow.Engine.Machine.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Example.BulletExample
{
	public class One : IState, IStateStart
	{
		public void OnStart(Thread thread)
		{
			Debug.Log("One");
			(thread.StateMachine as BulletTest).CreateOne();
		}
	}
}
