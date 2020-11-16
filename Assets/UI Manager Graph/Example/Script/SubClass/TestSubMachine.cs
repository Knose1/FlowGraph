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
	public class TestSubMachine : StateMachine
	{
		

		
		
		
		
		protected Sub.MySubClassMachine OwOSubMachine;
		protected Sub.OtherSubMachine HelloSubMachine;
		
		protected SubstateMachine owOSubState;
		protected SubstateMachine helloSubState;
		
		protected override void SetupMachine()
		{
			base.SetupMachine();

			owOSubState = new SubstateMachine("OwOSubState", OwOSubMachine = new Sub.MySubClassMachine((StateMachine)this, (Machine)this));
			helloSubState = new SubstateMachine("HelloSubState", HelloSubMachine = new Sub.OtherSubMachine((StateMachine)this, (Machine)this));

			

			

			owOSubState.nextMachine = helloSubState;
			helloSubState.nextMachine = endState;
		}
		

		protected override void EntryPoint(Thread mainThread)
		{
			mainThread.SetState(owOSubState);
		}
		
		
	}
	
	namespace Sub
	{
		using static Com.Github.Knose1.Flow.Engine.Machine.StateMachine;
		
		/// <summary>
		/// GENERATED SUBSTATE CLASS
		/// </summary>
		public class OtherSubMachine : Machine
		{
			
	
			
			
			protected Com.Github.Knose1.Flow.Example.SubClass.DebugSubClass Debug = new Com.Github.Knose1.Flow.Example.SubClass.DebugSubClass();
			
			
			
			protected ClassMachineState debugState;
			
			public OtherSubMachine(StateMachine stateMachine, Machine machine) : base(stateMachine, machine){}
	
			protected override void SetupMachine()
			{
				base.SetupMachine();
	
				debugState = new ClassMachineState("DebugState",Debug);
	
				AllowTrigger("");
	
				debugState.AddTrigger("", stopState);
	
				
			}
	
			protected override void EntryPoint(Thread mainThread)
			{
				mainThread.SetState(debugState);
			}
			
			
		}
	}
	namespace Sub
	{
		using static Com.Github.Knose1.Flow.Engine.Machine.StateMachine;
		
		/// <summary>
		/// GENERATED SUBSTATE CLASS
		/// </summary>
		public class MySubClassMachine : Machine
		{
			
	
			
			
			protected Com.Github.Knose1.Flow.Example.SubClass.DebugSubClass Debug = new Com.Github.Knose1.Flow.Example.SubClass.DebugSubClass();
			
			protected Com.Github.Knose1.Flow.Example.SubClass.Sub.OtherSubMachine YepAnotherSubSubMachine;
			
			protected ClassMachineState debugState;
			protected SubstateMachine yepAnotherSubSubState;
			
			public MySubClassMachine(StateMachine stateMachine, Machine machine) : base(stateMachine, machine){}
	
			protected override void SetupMachine()
			{
				base.SetupMachine();
	
				debugState = new ClassMachineState("DebugState",Debug);
				yepAnotherSubSubState = new SubstateMachine("YepAnotherSubSubState", YepAnotherSubSubMachine = new Com.Github.Knose1.Flow.Example.SubClass.Sub.OtherSubMachine((StateMachine)this, (Machine)this));
	
				AllowTrigger("");
	
				debugState.AddTrigger("",yepAnotherSubSubState, false);
	
				yepAnotherSubSubState.nextMachine = stopState;
			}
	
			protected override void EntryPoint(Thread mainThread)
			{
				mainThread.SetState(debugState);
			}
			
			
		}
	}
}