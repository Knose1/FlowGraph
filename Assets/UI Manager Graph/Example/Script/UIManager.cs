//Intended result


using System;
using UnityEngine;
using Com.Github.Knose1.Flow.Engine.Machine;
using Com.Github.Knose1.Flow.Engine.Machine.State;

namespace Com.Github.Knose1.Flow.Example
{
	/// <summary>
	/// GENERATED CLASS
	/// </summary>
	public class UIManager : StateMachine
	{
		public event Action OnMenu;
		public event Action OnStartGame;

		[SerializeField] protected GameObject titlecard;
		
		protected Com.Github.Knose1.Flow.Example.Menu menu = new Com.Github.Knose1.Flow.Example.Menu();

		protected GameObjectMachineState titlecardState;
		protected ClassMachineState menuState;
		protected MachineState startGameState;

		protected override void SetupMachine()
		{
			base.SetupMachine();

			titlecardState = new GameObjectMachineState(titlecard);
			menuState = new ClassMachineState(menu);
			startGameState = new MachineState();

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
	}
}