using Com.Github.Knose1.Flow.Engine.Utils;
using Com.Github.Knose1.Flow.Engine.Machine.State;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Engine.Machine
{
	public abstract class StateMachine : MonoBehaviour
	{
		/// <summary>
		/// List of allowed triggers
		/// </summary>
		private List<string> allowedTriggers;

		/// <summary>
		/// List of triggers
		/// </summary>
		private List<string> triggers;

		/// <summary>
		/// List of threads
		/// </summary>
		private List<Thread> threads;

		protected const string DEBUG_TAG = "["+nameof(StateMachine)+"]";

		/// <summary>
		/// A state that calls <see cref="Thread.Die"/> on start
		/// </summary>
		protected MachineState endState;

		[SerializeField] bool debug = false;

		protected virtual void Awake()
		{
			allowedTriggers = new List<string>();
			endState = new MachineState();
			endState.OnStart += EndState_OnStart;

			SetupMachine();

			StartMachine();
		}

		protected virtual void SetupMachine()
		{
			AllowTrigger("");

		}

		public virtual void StartMachine()
		{
			ResetLists();
			SetTrigger("");
			Thread mainThread = CreateThread();
			EntryPoint(mainThread);
		}

		protected abstract void EntryPoint(Thread mainThread);

		public virtual void StopMachine()
		{
			ResetLists();
		}

		public Thread CreateThread()
		{
			Thread thread = new Thread(this);
			thread.OnDie += Thread_OnDie;
			threads.Add(thread);
#if UNITY_EDITOR
			if (debug) Debug.Log(DEBUG_TAG + " Created thread, id : " + thread.Id);
#endif
			return thread;
		}

		/// <summary>
		/// Reset <see cref="triggers"/> and <see cref="triggerCallback"/>
		/// </summary>
		protected virtual void ResetLists()
		{
			triggers = new List<string>();
			threads = new List<Thread>();
		}

		public void AllowTrigger(string trigger)
		{
			if (allowedTriggers.Contains(trigger)) return;
			allowedTriggers.Add(trigger);
		}

		public void RemoveAllowedTrigger(string trigger)
		{
			if (allowedTriggers.Contains(trigger)) 
				allowedTriggers.Remove(trigger);
		}

		/// <summary>
		/// Set a trigger. If the trigger doesn't trigger anything, it's stocked in <see cref="triggers"/>.<br/>
		/// To remove a trigger, use <see cref="RemoveTrigger"/>
		/// </summary>
		/// <param name="trigger">The trigger to set</param>
		public void SetTrigger(string trigger)
		{
			if (!allowedTriggers.Contains(trigger))
			{
				Debug.LogWarning(DEBUG_TAG + " \""+trigger+"\" is not an allowed trigger");
				return;
			}
			if (triggers.Contains(trigger)) return;

#if UNITY_EDITOR
			if (debug) Debug.Log(DEBUG_TAG + " Set Trigger : " + trigger);
#endif
			bool hasTrigger = false;


			for (int i = 0; i < threads.Count; i++)
			{
				ExecuteTrigger(threads[i], trigger, out bool hasTrigger2);
				hasTrigger = hasTrigger2 || hasTrigger;
			}

			if (!hasTrigger) triggers.Add(trigger);
		}

		/// <summary>
		/// Execute a trugger :
		/// - Create a thread if <see cref="TriggerData.createThread"/> is true
		/// - Set the state of the thread
		/// - Sometimes 
		/// </summary>
		/// <param name="thread"></param>
		/// <param name="trigger"></param>
		/// <param name="hasTrigger"></param>
		/// <returns>True if the last state has created a new thread</returns>
		private bool ExecuteTrigger(Thread thread, string trigger, out bool hasTrigger)
		{
			int iterationCount = 0;
			bool createThread = false;
			List<MachineState> alreadySeen = new List<MachineState>();
			hasTrigger = false;

			do
			{
				MachineState state = thread.GetState(trigger, out createThread, alreadySeen);
				if (state != null)
				{
					hasTrigger = true;
					if (createThread)
					{
						thread = CreateThread();
					}
					alreadySeen.Add(state);
					thread.SetState(state);
#if UNITY_EDITOR
					if (debug) Debug.Log(DEBUG_TAG + " New state on thread, id : " + thread.Id);
#endif
				}
				else
				{
					break;
				}

				if (++iterationCount > 100)
				{
					Debug.LogError("++x > 100");
					break;
				}
			}
			while (createThread == true);

			return createThread;
		}

		/// <summary>
		/// Iterates on every triggers in <see cref="triggers"/> and check
		/// </summary>
		/// <param name="thread"></param>
		public void CheckForTrigger(Thread thread)
		{
			List<string> triggered = new List<string>();
			int count = triggers.Count;
			int i = 0;

			bool createThread = false;

			do
			{
				string trigger = triggers[i++];
				createThread = ExecuteTrigger(thread, trigger, out bool hasTrigger);
				if (hasTrigger) triggered.Add(trigger);

			} while (createThread == false && i < count);

			for (int f = triggered.Count - 1; f >= 0; f--)
			{
				RemoveTrigger(triggered[f]);
			}
		}

		/// <summary>
		/// Remove a trigger.<br/>
		/// To add a trigger, use <see cref="SetTrigger"/>
		/// </summary>
		/// <param name="trigger">The trigger to remove</param>
		public void RemoveTrigger(string trigger)
		{
			if (trigger == "") return; //Yep we can't remove "" triger

#if UNITY_EDITOR
			if (debug) Debug.Log(DEBUG_TAG + " Remove Trigger : " + trigger);
#endif
			if (triggers.Contains(trigger)) triggers.Remove(trigger);
		}

		private void Update()
		{
			for (int i = threads.Count - 1; i >= 0; i--)
			{
				threads[i].Update();
			}
		}

		private void EndState_OnStart(Thread obj)
		{
			obj.Die();
		}

		private void Thread_OnDie(Thread thread)
		{
			threads.Remove(thread);
			thread.OnDie -= Thread_OnDie;
#if UNITY_EDITOR
			if (debug) Debug.Log(DEBUG_TAG + " Ended thread, id : " + thread.Id);
#endif
		}
	}

}
