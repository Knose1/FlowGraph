
//Generated class
//If you need to modify the class, override it


using System;
using UnityEngine;
using Com.Github.Knose1.Flow.Engine.Machine;
using Com.Github.Knose1.Flow.Engine.Machine.State;

namespace Com.Github.Knose1.Flow.Example
{
	/// <summary>
	/// GENERATED CLASS
	/// </summary>
	public class UIManagerGenerated : StateMachine
	{
		public event Action OnStartGame;
		public event Action OnMenu;

		[SerializeField] protected GameObject titlecard;
		
		protected Com.Github.Knose1.Flow.Example.Menu menu = new Com.Github.Knose1.Flow.Example.Menu();
		
		protected MachineState startGameState;
		protected ClassMachineState menuState;
		protected GameObjectMachineState titlecardState;
		
		protected override void SetupMachine()
		{
			base.SetupMachine();

			startGameState = new MachineState("StartGame");
			menuState = new ClassMachineState("Menu",menu);
			titlecardState = new GameObjectMachineState("Titlecard",titlecard);

			AllowTrigger("GameEnd");
			AllowTrigger("StartGame");
			AllowTrigger("Exit");
			AllowTrigger("");

			startGameState.AddTrigger("GameEnd",menuState, false);
			menuState.AddTrigger("StartGame",startGameState, false);
			menuState.AddTrigger("Exit", endState);
			titlecardState.AddTrigger("",menuState, false);

			startGameState.OnStart += (Thread thread) => { OnStartGame?.Invoke(); };
			menuState.OnStart += (Thread thread) => { OnMenu?.Invoke(); };
		}

		protected override void EntryPoint(Thread mainThread)
		{
			mainThread.SetState(titlecardState);
		}
	}
}