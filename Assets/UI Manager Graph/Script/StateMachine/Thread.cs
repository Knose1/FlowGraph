using Com.Github.Knose1.Flow.Engine.Machine.State;
using Com.Github.Knose1.Flow.Engine.Utils;
using System;
using System.Collections.Generic;

namespace Com.Github.Knose1.Flow.Engine.Machine
{
	/// <summary>
	/// A thread know what is the current <see cref="MachineState"/> and can access the trigger conditions of this state.<br/>
	/// It has a hard dependence with its <see cref="Machine.StateMachine"/>
	/// </summary>
	public class Thread
	{
		protected static int staticId = 0;

		private int _id = 0;
		public int Id => _id;

		private StateMachine _stateMachine;
		public StateMachine StateMachine => _stateMachine;

		public event Action<Thread> OnDie;
		protected List<TriggerData> Triggers => currentState.triggers;
		protected MachineState currentState;
		protected MachineState nextState;
		private bool isDead = false;

		public Thread(StateMachine stateMachine)
		{
			_id = ++staticId;
			_stateMachine = stateMachine;
		}

		public MachineState GetState(string trigger, out bool createThread, List<MachineState> negativeFilter = null)
		{
			createThread = false;

			TriggerData triggerState = Triggers.GetFirstCallbackByTrigger(trigger, negativeFilter);
			if (triggerState != null)
			{
				createThread = triggerState.createThread;
				return triggerState.state;
			}

			return null;
		}

		public void CheckForTriggersInCurrentState()
		{
			_stateMachine.CheckForTrigger(this);
		}

		public void Update()
		{
			if (nextState != null)
			{
				currentState?.End(this);
				currentState = nextState;
				currentState.Start(this);
				nextState = null;
				if (isDead) return;
				CheckForTriggersInCurrentState();
			}
			currentState?.Update(this);
		}


		public void Die()
		{
			isDead = true;
			currentState?.End(this);
			currentState = null;
			nextState = null;
			OnDie?.Invoke(this);
			OnDie = null;
		}

		/// <summary>
		/// Asyncronously set a state
		/// </summary>
		public void SetState(MachineState state)
		{
			nextState = state;
		}
	}
}
