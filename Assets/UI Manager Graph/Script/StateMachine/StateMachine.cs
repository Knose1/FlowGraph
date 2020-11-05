using Com.Github.Knose1.Flow.Engine.Utils;
using Com.Github.Knose1.Flow.Engine.Machine.State;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Com.Github.Knose1.Flow.Engine.Machine
{
	public abstract class StateMachine : MonoBehaviour
	{
		public const string END_STATE = nameof(endState);
		public const string STOP_STATE = nameof(stopState);

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

		public Thread GetThreadById(int id)
		{
			for (int i = threads.Count - 1; i >= 0; i--)
			{
				Thread thread = threads[i];
				if (thread.Id == id) 
					return thread;
			}

			return null;
		}

		private string _debugTag = null;
		public string DebugTag
		{
			get
			{
				if (_debugTag != null) return _debugTag;

				return _debugTag = "[" + GetType().Name + "]";
			}
		}

		/// <summary>
		/// A state that calls <see cref="Thread.Die"/> on start
		/// </summary>
		protected MachineState endState;

		/// <summary>
		/// A state that calls <see cref="StopMachine()"/> on start
		/// </summary>
		protected MachineState stopState;

		/// <summary>
		/// Allow logs
		/// </summary>
		[SerializeField] private bool _debug = false;
		public bool IsDebug => _debug;

		[SerializeField] public bool startOnAwake = true;

		protected virtual void Awake()
		{
			allowedTriggers = new List<string>();
			endState = new MachineState("end");
			endState.OnStart += EndState_OnStart;

			stopState = new MachineState("stop");
			stopState.OnStart += StopState_OnStart;

			SetupMachine();

			if (startOnAwake) StartMachine();
		}

		private void StopState_OnStart(Thread obj) => StopMachine();

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
#if UNITY_EDITOR || DEVELOPEMENT_BUILD
			if (IsDebug) Debug.Log(DebugTag + " Created thread, id : " + thread.Id);
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
		/// <param name="keep">If true, the trigger will be kept for a future state</param>
		public void SetTrigger(string trigger, bool keep = true)
		{
			if (!allowedTriggers.Contains(trigger))
			{
				Debug.LogWarning(DebugTag + " \""+trigger+"\" is not an allowed trigger");
				return;
			}
			if (triggers.Contains(trigger)) return;

#if UNITY_EDITOR || DEVELOPEMENT_BUILD
			if (IsDebug) Debug.Log(DebugTag + " Set Trigger : " + trigger);
#endif
			bool hasTrigger = false;

			var lList = threads.ToList();
			int count = lList.Count;
			for (int i = 0; i < count; i++)
			{
				ExecuteTrigger(lList[i], trigger, out bool hasTrigger2);
				hasTrigger = hasTrigger2 || hasTrigger;
			}

			if (!hasTrigger && keep) triggers.Add(trigger);
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
				MachineState state = thread.GetNextState(trigger, out createThread, alreadySeen);
				if (state != null)
				{
					hasTrigger = true;

					Thread threadToSet = thread;
					if (createThread)
					{
						threadToSet = CreateThread();
					}
					alreadySeen.Add(state);
					threadToSet.SetState(state, !createThread && iterationCount != 0);
				}
				else
				{
					break;
				}

				if (++iterationCount > 100)
				{
					Debug.LogError(DebugTag +nameof(iterationCount)+ " > 100");
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

#if UNITY_EDITOR || DEVELOPEMENT_BUILD
			if (IsDebug) Debug.Log(DebugTag + " Remove Trigger : " + trigger);
#endif
			if (triggers.Contains(trigger)) triggers.Remove(trigger);
		}

		private void Update()
		{
			var lList = threads.ToList();
			int count = lList.Count;
			for (int i = 0; i < count; i++)
			{
				lList[i].Update();
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
#if UNITY_EDITOR || DEVELOPEMENT_BUILD
			if (IsDebug) Debug.Log(DebugTag + " Ended thread, id : " + thread.Id);
#endif
		}
	}

}
