//Intended result


using System;
using UnityEngine;
using Com.Github.Knose1.Flow.Engine.Machine;
using Com.Github.Knose1.Flow.Engine.Machine.State;

namespace Com.Github.Knose1.Flow.Example
{
	/// <summary>
	/// GENERATED CLASS (intended result)
	/// </summary>
	public class UIManager : StateMachine
	{
		public event Action OnMenu;
		public event Action OnStartGame;

		[SerializeField] protected GameObject titlecard;
		
		protected Com.Github.Knose1.Flow.Example.Menu menu = new Com.Github.Knose1.Flow.Example.Menu();

		protected TestMachine testSubMachine;

		protected GameObjectMachineState titlecardState;
		protected ClassMachineState menuState;
		protected MachineState startGameState;
		
		protected SubstateMachine testSubstate;

		protected override void SetupMachine()
		{
			base.SetupMachine();

			titlecardState = new GameObjectMachineState("Titlecard",titlecard);
			menuState = new ClassMachineState("Menu", menu);
			startGameState = new MachineState("StartGame");
			testSubstate = new SubstateMachine("Test", testSubMachine = new TestMachine((StateMachine)this, (Machine)this));
			testSubstate.nextMachine = startGameState;

			AllowTrigger("CountDown");
			AllowTrigger("StartGame");

			titlecardState.AddTrigger("CountDown", menuState);
			menuState.AddTrigger("StartGame", startGameState);
			startGameState.AddTrigger("", endState);

			menuState.OnStart += (Thread thread) => { OnMenu?.Invoke(); };
			startGameState.OnStart += (Thread thread) => { OnStartGame?.Invoke(); };
		}

		protected override void EntryPoint(Thread mainThread)
		{
			mainThread.SetState(titlecardState);
		}

		public class TestMachine : Machine
		{
			public TestMachine(StateMachine stateMachine, Machine machine) : base(stateMachine, machine){}

			protected override void EntryPoint(Thread mainThread) => throw new NotImplementedException();
		}
	}
}