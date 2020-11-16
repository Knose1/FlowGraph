using Com.Github.Knose1.Flow.Engine.Machine.State;
using Com.Github.Knose1.Flow.Engine.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

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

		private StateMachine.Machine _Machine;
		public StateMachine.Machine Machine => _Machine;
		public StateMachine StateMachine => _Machine.StateMachine;

		public event Action<Thread, MachineState> OnChange;
		public event Action<Thread> OnDie;
		protected List<TriggerData> Triggers
		{
			get
			{
				if (_currentState == null) return new List<TriggerData>();
				return _currentState.triggers;
			}
		}
		protected MachineState _currentState;
		public MachineState CurrentState => _currentState;
		protected MachineState nextState;
		private bool isDead = false;
		private bool isChecked = false;

		public Thread(StateMachine.Machine stateMachine)
		{
			_id = ++staticId;
			_Machine = stateMachine;
		}

		/// <summary>
		/// Get next state by how it's triggered
		/// </summary>
		/// <param name="trigger"></param>
		/// <param name="createThread">If the state create a thred</param>
		/// <param name="negativeFilter">A list of machine state to exclude from searching</param>
		/// <returns></returns>
		public State.MachineState GetNextState(string trigger, out bool createThread, List<State.MachineState> negativeFilter = null)
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
			isChecked = true;
			_Machine.CheckForTrigger(this);
		}

		public void Update()
		{
			if (isDead) return;

			if (nextState != null)
			{
				State.MachineState oldCurrentState = _currentState;
				OnNewState();
				if (isDead) return;

				if (oldCurrentState != null) CheckForTriggersInCurrentState();
			}
			else if (!isChecked && _currentState != null)
			{
				CheckForTriggersInCurrentState();
			}
			_currentState?.Update(this);
		}

		/// <summary>
		/// Called when a new state is added
		/// </summary>
		/// <param name="check">If true, CheckForTriggersInCurrentState is called</param>
		private void OnNewState(bool check = true)
		{
			isChecked = false;

			_currentState?.End(this);
			_currentState = nextState;

#if UNITY_EDITOR || DEVELOPEMENT_BUILD
			if (_Machine.IsDebug)
				Debug.Log(_Machine.DebugTag + " New state \"" + _currentState.name + "\" on thread, id : " + _id);
#endif

			_currentState.Start(this);
			nextState = null;

			OnChange?.Invoke(this, _currentState);

			if (isDead) return;

			if (check) CheckForTriggersInCurrentState();
		}

		public void Die()
		{
			isDead = true;
			_currentState?.End(this);
			_currentState = null;
			nextState = null;
			OnDie?.Invoke(this);
			OnDie = null;
			OnChange = null;
		}

		/// <summary>
		/// Asyncronously set a state
		/// </summary>
		/// <param name="state">The state to add</param>
		/// <param name="force">Force will directly add the state without waiting for the next update</param>
		public void SetState(State.MachineState state, bool force = false)
		{
			nextState = state;
			if (force) OnNewState(false);
		}
	}
}
