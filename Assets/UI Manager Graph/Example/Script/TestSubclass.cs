//Generated class
//If you need to modify the class, override it


using System;
using UnityEngine;
using Com.Github.Knose1.Flow.Engine.Machine;
using Com.Github.Knose1.Flow.Engine.Machine.State;

namespace Com.Github.Knose1.Flow.Example.SubClass
{
	/// <summary>
	/// GENERATED CLASS
	/// </summary>
	public class TestSubclass : StateMachine
	{
		public event Action OnExit;

		
		
		
		
		protected Hi.MySubClassMachine helloSubMachine;
		protected Hi.MySubClassMachine owOSubMachine;
		
		protected SubstateMachine helloSubState;
		protected MachineState exitState;
		protected SubstateMachine owOSubState;
		
		protected override void SetupMachine()
		{
			base.SetupMachine();

			helloSubState = new SubstateMachine("HelloSub", helloSubMachine = new Hi.MySubClassMachine(this));
			exitState = new MachineState("Exit");
			owOSubState = new SubstateMachine("OwOSub", owOSubMachine = new Hi.MySubClassMachine(this));

			AllowTrigger("OwO");
			AllowTrigger("");
			AllowTrigger("Test");

			helloSubState.AddTrigger("OwO",exitState, false);
			exitState.AddTrigger("", endState);
			owOSubState.AddTrigger("Test",helloSubState, false);

			exitState.OnStart += (Thread thread) => { OnExit?.Invoke(); };
		}
		

		protected override void EntryPoint(Thread mainThread)
		{
			mainThread.SetState(owOSubState);
		}
		
		
	}
	
	namespace Hi
	{
		using static Com.Github.Knose1.Flow.Engine.Machine.StateMachine;
		
		/// <summary>
		/// GENERATED SUBSTATE CLASS
		/// </summary>
		public class MySubClassMachine : Machine
		{
			
	
			
			
			
			
			
			
			protected MachineState state1State;
			
			public MySubClassMachine(StateMachine stateMachine) : base(stateMachine){}
	
			protected override void SetupMachine()
			{
				base.SetupMachine();
	
				state1State = new MachineState("State1");
	
				AllowTrigger("Exit");
	
				state1State.AddTrigger("Exit", stopState);
	
				
			}
	
			protected override void EntryPoint(Thread mainThread)
			{
				mainThread.SetState(state1State);
			}
			
			
		}
		
		
	}
}