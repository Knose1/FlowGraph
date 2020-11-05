﻿using Com.Github.Knose1.Flow.Engine.Machine.State;
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

		private StateMachine _stateMachine;
		public StateMachine StateMachine => _stateMachine;

		public event Action<Thread, MachineState> OnChange;
		public event Action<Thread> OnDie;
		protected List<TriggerData> Triggers
		{
			get
			{
				if (currentState == null) return new List<TriggerData>();
				return currentState.triggers;
			}
		}
		protected MachineState currentState;
		protected MachineState nextState;
		private bool isDead = false;
		private bool isChecked = false;

		public Thread(StateMachine stateMachine)
		{
			_id = ++staticId;
			_stateMachine = stateMachine;
		}

		/// <summary>
		/// Get next state by how it's triggered
		/// </summary>
		/// <param name="trigger"></param>
		/// <param name="createThread">If the state create a thred</param>
		/// <param name="negativeFilter">A list of machine state to exclude from searching</param>
		/// <returns></returns>
		public MachineState GetNextState(string trigger, out bool createThread, List<MachineState> negativeFilter = null)
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
			_stateMachine.CheckForTrigger(this);
		}

		public void Update()
		{
			if (isDead) return;

			if (nextState != null)
			{
				MachineState oldCurrentState = currentState;
				OnNewState();
				if (isDead) return;

				if (oldCurrentState != null) CheckForTriggersInCurrentState();
			}
			else if (!isChecked && currentState != null)
			{
				CheckForTriggersInCurrentState();
			}
			currentState?.Update(this);
		}

		/// <summary>
		/// Called when a new state is added
		/// </summary>
		/// <param name="check">If true, CheckForTriggersInCurrentState is called</param>
		private void OnNewState(bool check = true)
		{
			isChecked = false;

			currentState?.End(this);
			currentState = nextState;

#if UNITY_EDITOR || DEVELOPEMENT_BUILD
			if (_stateMachine.IsDebug)
				Debug.Log(_stateMachine.DebugTag + " New state \"" + currentState.name + "\" on thread, id : " + _id);
#endif

			currentState.Start(this);
			nextState = null;

			OnChange?.Invoke(this, currentState);

			if (isDead) return;

			if (check) CheckForTriggersInCurrentState();
		}

		public void Die()
		{
			isDead = true;
			currentState?.End(this);
			currentState = null;
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
		public void SetState(MachineState state, bool force = false)
		{
			nextState = state;
			if (force) OnNewState(false);
		}
	}
}
