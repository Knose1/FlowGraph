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

		
		
		protected Com.Github.Knose1.Flow.Example.BulletExample.Timer timer = new Com.Github.Knose1.Flow.Example.BulletExample.Timer();
		protected Com.Github.Knose1.Flow.Example.BulletExample.One one = new Com.Github.Knose1.Flow.Example.BulletExample.One();
		protected Com.Github.Knose1.Flow.Example.BulletExample.Two two = new Com.Github.Knose1.Flow.Example.BulletExample.Two();
		protected Com.Github.Knose1.Flow.Example.BulletExample.One one1 = new Com.Github.Knose1.Flow.Example.BulletExample.One();
		protected Com.Github.Knose1.Flow.Example.BulletExample.Two two1 = new Com.Github.Knose1.Flow.Example.BulletExample.Two();
		
		
		
		protected ClassMachineState timerState;
		protected ClassMachineState oneState;
		protected MachineState startState;
		protected ClassMachineState twoState;
		protected MachineState empty1State;
		protected ClassMachineState one1State;
		protected ClassMachineState two1State;
		protected MachineState waitState;
		protected MachineState empty2State;
		
		protected override void SetupMachine()
		{
			base.SetupMachine();

			timerState = new ClassMachineState("Timer",timer);
			oneState = new ClassMachineState("One",one);
			startState = new MachineState("Start");
			twoState = new ClassMachineState("Two",two);
			empty1State = new MachineState("Empty1");
			one1State = new ClassMachineState("One1",one1);
			two1State = new ClassMachineState("Two1",two1);
			waitState = new MachineState("Wait");
			empty2State = new MachineState("Empty2");

			AllowTrigger("End");
			AllowTrigger("Time");
			AllowTrigger("Time2");
			AllowTrigger("");
			AllowTrigger("");
			AllowTrigger("Time");
			AllowTrigger("Time3");
			AllowTrigger("");
			AllowTrigger("");
			AllowTrigger("Time3");
			AllowTrigger("Time2");
			AllowTrigger("Time");
			AllowTrigger("Time5");

			timerState.AddTrigger("End", stopState);
			oneState.AddTrigger("Time",oneState, false);
			oneState.AddTrigger("Time2",twoState, false);
			startState.AddTrigger("",oneState, true);
			startState.AddTrigger("",timerState, false);
			twoState.AddTrigger("Time",empty1State, false);
			empty1State.AddTrigger("Time3",empty2State, false);
			one1State.AddTrigger("", endState);
			two1State.AddTrigger("", endState);
			waitState.AddTrigger("Time3",oneState, false);
			empty2State.AddTrigger("Time2",one1State, true);
			empty2State.AddTrigger("Time",two1State, true);
			empty2State.AddTrigger("Time5",waitState, false);

			startState.OnStart += (Thread thread) => { OnStart?.Invoke(); };
		}
		

		protected override void EntryPoint(Thread mainThread)
		{
			mainThread.SetState(startState);
		}
		
		
		
	}
}