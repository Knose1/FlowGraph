using Com.Github.Knose1.Flow.Engine.Machine.Interfaces;
using Com.Github.Knose1.Flow.Engine.Machine.State;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Engine.Machine
{
	public abstract class StateMachine : MonoBehaviour
	{
		public static explicit operator Machine(StateMachine stateMachine) => stateMachine._mainMachine;

		private LMainMachine _mainMachine;
		public Machine MainMachine => _mainMachine;

		public Thread GetThreadById(int id)
		{
			return _mainMachine.GetThreadById(id);
		}

		public string MachineDebugTag => _mainMachine.DebugTag;

		/// <summary>
		/// A state that calls <see cref="Thread.Die"/> on start
		/// </summary>
		protected State.MachineState endState => _mainMachine.EndState;

		/// <summary>
		/// A state that calls <see cref="Machine.StopMachine()"/> on start
		/// </summary>
		protected State.MachineState stopState => _mainMachine.StopState;

		/// <summary>
		/// Allow logs
		/// </summary>
		[SerializeField] private bool m_debug = false;
		public bool IsDebug => m_debug;

		[SerializeField] public bool m_startOnAwake = true;

		protected virtual void Awake()
		{
			_mainMachine = new LMainMachine(this);
			_mainMachine.OnStart(null);
		}

		protected virtual void SetupMachine() {}
		public virtual void StartMachine() => _mainMachine.StartMachine();
		protected abstract void EntryPoint(Thread mainThread);
		public virtual void StopMachine() => _mainMachine.StopMachine();
		public Thread CreateThread() => _mainMachine.CreateThread();
		/// <summary>
		/// Reset <see cref="Machine.triggers"/> and <see cref="Machine.threads"/>
		/// </summary>
		protected virtual void ResetLists() {}
		public void AllowTrigger(string trigger) => _mainMachine.AllowTrigger(trigger);
		public void RemoveAllowedTrigger(string trigger) => _mainMachine.RemoveAllowedTrigger(trigger);
		/// <summary>
		/// Set a trigger. If the trigger doesn't trigger anything, it's stocked in <see cref="triggers"/>.<br/>
		/// To remove a trigger, use <see cref="RemoveTrigger"/>
		/// </summary>
		/// <param name="trigger">The trigger to set</param>
		/// <param name="keep">If true, the trigger will be kept for a future state</param>
		public void SetTrigger(string trigger, bool keep = true) => _mainMachine.SetTrigger(trigger, keep);
		/// <summary>
		/// Remove a trigger.<br/>
		/// To add a trigger, use <see cref="SetTrigger"/>
		/// </summary>
		/// <param name="trigger">The trigger to remove</param>
		public void RemoveTrigger(string trigger) => _mainMachine.RemoveTrigger(trigger);
		protected virtual void Update() => MainMachine.OnUpdate(null);

		/// <summary>
		/// A machine. Override it to create a substate machine
		/// </summary>
		public abstract class Machine : IState, IStateEnd, IStateStart, IStateUpdate
		{
			public static explicit operator StateMachine(Machine machine) => machine.StateMachine;

			/// <summary>
			/// Called before the first thread is created
			/// </summary>
			public event Action OnMachineStart;

			/// <summary>
			/// Called after all the thread has been disposed
			/// </summary>
			public event Action OnMachineStop;

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

			private StateMachine _stateMachine;
			public StateMachine StateMachine => _stateMachine;

			protected Machine(StateMachine stateMachine, Machine parent) => _stateMachine = stateMachine;

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
			protected State.MachineState endState;

			/// <summary>
			/// A state that calls <see cref="StopMachine()"/> on start
			/// </summary>
			protected State.MachineState stopState;
			
			public bool IsDebug => StateMachine.IsDebug;
			public bool m_startOnAwake => StateMachine.m_startOnAwake;

			/// <summary>
			/// Called on state start
			/// </summary>
			/// <param name="thread"></param>
			public virtual void OnStart(Thread thread)
			{
				allowedTriggers = new List<string>();
				endState = new State.MachineState("end");
				endState.OnStart += EndState_OnStart;

				stopState = new State.MachineState("stop");
				stopState.OnStart += StopState_OnStart;

				SetupMachine();

				if (m_startOnAwake) StartMachine();
			}

			protected virtual void SetupMachine()
			{
				AllowTrigger("");
			}

			public virtual void StartMachine()
			{
				ResetLists();
				SetTrigger("");
				OnMachineStart?.Invoke();
				Thread mainThread = CreateThread();
				EntryPoint(mainThread);
			}

			protected abstract void EntryPoint(Thread mainThread);

			/// <summary>
			/// Called on state end
			/// </summary>
			/// <param name="thread"></param>
			public virtual void OnEnd(Thread thread)
			{
				StopMachine();
			}
			public virtual void StopMachine()
			{
				ResetLists();
				OnMachineStop?.Invoke();
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
			/// Reset <see cref="triggers"/> and <see cref="threads"/>
			/// </summary>
			protected virtual void ResetLists()
			{
				triggers = new List<string>();
				threads = new List<Thread>();
			}

			/// <summary>
			/// A trigger that can be set (see <see cref="SetTrigger(string, bool)"/>
			/// </summary>
			/// <param name="trigger"></param>
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
					Debug.LogWarning(DebugTag + " \"" + trigger + "\" is not an allowed trigger");
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
				List<State.MachineState> alreadySeen = new List<State.MachineState>();
				hasTrigger = false;

				do
				{
					State.MachineState state = thread.GetNextState(trigger, out createThread, alreadySeen);
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
						Debug.LogError(DebugTag + nameof(iterationCount) + " > 100");
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
				List<string> triggers = new List<string>(this.triggers);
				int count = this.triggers.Count;
				int i = 0;

				bool createThread = false;

				do
				{
					if (i >= count) continue;

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

			public virtual void OnUpdate(Thread thread)
			{
				var lList = threads.ToList();
				int count = lList.Count;
				for (int i = 0; i < count; i++)
				{
					lList[i].Update();
				}
			}

			private void EndState_OnStart(Thread obj) => obj.Die();
			private void StopState_OnStart(Thread obj) => StopMachine();
			private void Thread_OnDie(Thread thread)
			{
				threads.Remove(thread);
				thread.OnDie -= Thread_OnDie;
#if UNITY_EDITOR || DEVELOPEMENT_BUILD
				if (IsDebug) Debug.Log(DebugTag + " Ended thread, id : " + thread.Id);
#endif
			}
		}


		internal class LMainMachine : Machine
		{
			public LMainMachine(StateMachine stateMachine) : base(stateMachine, null) { }

			public State.MachineState EndState => endState;
			public State.MachineState StopState => stopState;

			protected override void SetupMachine()
			{
				base.SetupMachine();
				StateMachine.SetupMachine();
			}

			protected override void EntryPoint(Thread mainThread)
			{
				StateMachine.EntryPoint(mainThread);
			}

			protected override void ResetLists()
			{
				base.ResetLists();
				StateMachine.ResetLists();
			}
		}
	}
}
