
//Generated class
//If you need to modify the class, override it


using System;
using UnityEngine;
using Com.Github.Knose1.Flow.Engine.Machine;
using Com.Github.Knose1.Flow.Engine.Machine.State;

namespace Com.Github.Knose1.Flow.Example.PlayerExample
{
	/// <summary>
	/// GENERATED CLASS
	/// </summary>
	public class BulletTestAbstract : StateMachine
	{
		public event Action OnStart;

		
		
		protected Com.Github.Knose1.Flow.Example.BulletExample.One one = new Com.Github.Knose1.Flow.Example.BulletExample.One();
		protected Com.Github.Knose1.Flow.Example.BulletExample.Two two = new Com.Github.Knose1.Flow.Example.BulletExample.Two();
		protected Com.Github.Knose1.Flow.Example.BulletExample.One one1 = new Com.Github.Knose1.Flow.Example.BulletExample.One();
		protected Com.Github.Knose1.Flow.Example.BulletExample.Timer timer = new Com.Github.Knose1.Flow.Example.BulletExample.Timer();
		protected Com.Github.Knose1.Flow.Example.BulletExample.Two two1 = new Com.Github.Knose1.Flow.Example.BulletExample.Two();
		
		protected ClassMachineState oneState;
		protected ClassMachineState twoState;
		protected ClassMachineState one1State;
		protected ClassMachineState timerState;
		protected MachineState empty1State;
		protected ClassMachineState two1State;
		protected MachineState startState;
		
		protected override void SetupMachine()
		{
			base.SetupMachine();

			oneState = new ClassMachineState("One",one);
			twoState = new ClassMachineState("Two",two);
			one1State = new ClassMachineState("One1",one1);
			timerState = new ClassMachineState("Timer",timer);
			empty1State = new MachineState("Empty1");
			two1State = new ClassMachineState("Two1",two1);
			startState = new MachineState("Start");

			

			

			startState.OnStart += (Thread thread) => { OnStart?.Invoke(); };
		}

		protected override void EntryPoint(Thread mainThread)
		{
			mainThread.SetState(startState);
		}
	}
}