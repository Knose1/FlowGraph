
//Generated class
//If you need to modify the class, override it


using System;
using UnityEngine;
using Com.Github.Knose1.Flow.Engine.Machine;
using Com.Github.Knose1.Flow.Engine.Machine.State;

namespace Com.Github.Knose1.Flow.Example.BulletExample
{
	/// <summary>
	/// GENERATED CLASS
	/// </summary>
	public class BulletTestAbstract : StateMachine
	{
		public event Action OnStart;

		
		
		protected Com.Github.Knose1.Flow.Example.BulletExample.Two two = new Com.Github.Knose1.Flow.Example.BulletExample.Two();
		protected Com.Github.Knose1.Flow.Example.BulletExample.One one1 = new Com.Github.Knose1.Flow.Example.BulletExample.One();
		protected Com.Github.Knose1.Flow.Example.BulletExample.Timer timer = new Com.Github.Knose1.Flow.Example.BulletExample.Timer();
		protected Com.Github.Knose1.Flow.Example.BulletExample.Two two1 = new Com.Github.Knose1.Flow.Example.BulletExample.Two();
		protected Com.Github.Knose1.Flow.Example.BulletExample.One one = new Com.Github.Knose1.Flow.Example.BulletExample.One();
		
		protected ClassMachineState twoState;
		protected ClassMachineState one1State;
		protected ClassMachineState timerState;
		protected MachineState empty1State;
		protected ClassMachineState two1State;
		protected MachineState startState;
		protected ClassMachineState oneState;
		
		protected override void SetupMachine()
		{
			base.SetupMachine();

			twoState = new ClassMachineState("Two",two);
			one1State = new ClassMachineState("One1",one1);
			timerState = new ClassMachineState("Timer",timer);
			empty1State = new MachineState("Empty1");
			two1State = new ClassMachineState("Two1",two1);
			startState = new MachineState("Start");
			oneState = new ClassMachineState("One",one);

			AllowTrigger("Time");
			AllowTrigger("Time");
			AllowTrigger("Exit");
			AllowTrigger("");
			AllowTrigger("");
			AllowTrigger("Time2");
			AllowTrigger("");
			AllowTrigger("");
			AllowTrigger("Time");
			AllowTrigger("Time2");

			twoState.AddTrigger("Time",empty1State, false);
			one1State.AddTrigger("Time", endState);
			timerState.AddTrigger("Exit", stopState);
			empty1State.AddTrigger("",one1State, true);
			empty1State.AddTrigger("",two1State, false);
			two1State.AddTrigger("Time2",oneState, false);
			startState.AddTrigger("",oneState, true);
			startState.AddTrigger("",timerState, false);
			oneState.AddTrigger("Time",oneState, false);
			oneState.AddTrigger("Time2",twoState, false);

			startState.OnStart += (Thread thread) => { OnStart?.Invoke(); };
		}

		protected override void EntryPoint(Thread mainThread)
		{
			mainThread.SetState(startState);
		}
	}
}