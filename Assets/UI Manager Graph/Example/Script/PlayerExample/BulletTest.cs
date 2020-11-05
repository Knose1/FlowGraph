
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
	public class BulletTest : StateMachine
	{
		public event Action OnStart;

		
		
		protected Com.Github.Knose1.Flow.Example.BulletExample.Two two = new Com.Github.Knose1.Flow.Example.BulletExample.Two();
		protected Com.Github.Knose1.Flow.Example.BulletExample.Two two1 = new Com.Github.Knose1.Flow.Example.BulletExample.Two();
		protected Com.Github.Knose1.Flow.Example.BulletExample.One one1 = new Com.Github.Knose1.Flow.Example.BulletExample.One();
		protected Com.Github.Knose1.Flow.Example.BulletExample.One one = new Com.Github.Knose1.Flow.Example.BulletExample.One();
		protected Com.Github.Knose1.Flow.Example.BulletExample.Timer timer = new Com.Github.Knose1.Flow.Example.BulletExample.Timer();
		
		protected MachineState empty1State;
		protected ClassMachineState twoState;
		protected ClassMachineState two1State;
		protected ClassMachineState one1State;
		protected ClassMachineState oneState;
		protected ClassMachineState timerState;
		protected MachineState startState;
		
		protected override void SetupMachine()
		{
			base.SetupMachine();

			empty1State = new MachineState("Empty1");
			twoState = new ClassMachineState("Two",two);
			two1State = new ClassMachineState("Two1",two1);
			one1State = new ClassMachineState("One1",one1);
			oneState = new ClassMachineState("One",one);
			timerState = new ClassMachineState("Timer",timer);
			startState = new MachineState("Start");

			AllowTrigger("");
			AllowTrigger("");
			AllowTrigger("Time");
			AllowTrigger("Time");
			AllowTrigger("Time");
			AllowTrigger("Time");
			AllowTrigger("Time2");
			AllowTrigger("End");
			AllowTrigger("");
			AllowTrigger("");

			empty1State.AddTrigger("",one1State, true);
			empty1State.AddTrigger("",two1State, false);
			twoState.AddTrigger("Time",empty1State, false);
			two1State.AddTrigger("Time",oneState, false);
			one1State.AddTrigger("Time", endState);
			oneState.AddTrigger("Time",oneState, false);
			oneState.AddTrigger("Time2",twoState, false);
			timerState.AddTrigger("End", stopState);
			startState.AddTrigger("",oneState, true);
			startState.AddTrigger("",timerState, false);

			startState.OnStart += (Thread thread) => { OnStart?.Invoke(); };
		}

		protected override void EntryPoint(Thread mainThread)
		{
			mainThread.SetState(startState);
		}
	}
}