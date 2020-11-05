using Com.Github.Knose1.Flow.Engine.Machine.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Github.Knose1.Flow.Engine.Machine.State
{
	/// <summary>
	/// A state in the <see cref="StateMachine"/>.<br/>
	/// It defines how the target is called, the triggers to go to another <see cref="MachineState"/> and has an event <see cref="MachineState.OnStart"/><br/>
	/// <br/>
	/// Child classes : <br/>
	///	 - <see cref="ClassMachineState"/><br/>
	///  - <see cref="GameObjectMachineState"/>
	/// </summary>
	public class MachineState
	{
		public readonly string name;
		public List<TriggerData> triggers = new List<TriggerData>();

		public event Action<Thread> OnStart;

		public MachineState(string name) {
			this.name = name;
		}

		public virtual void Start(Thread thread)
		{
			OnStart?.Invoke(thread);
		}
		public virtual void Update(Thread thread)
		{

		}
		public virtual void End(Thread thread)
		{

		}

		public void AddTrigger(string trigger, MachineState nextState, bool createThread = false) => AddTrigger(new TriggerData(nextState, trigger, createThread));
		public void AddTrigger(TriggerData triggerState)
		{
			triggers.Add(triggerState);
		}

	}

	/// <summary>
	/// A state for <see cref="IState"/> classes
	/// </summary>
	public class ClassMachineState : MachineState
	{
		public IState target;
		public ClassMachineState(string name, IState target) : base(name)
		{
			this.target = target;
		}


		public override void Start(Thread thread)
		{
			base.Start(thread);
			(target as IStateStart)?.OnStart(thread);
		}

		public override void Update(Thread thread)
		{
			base.Update(thread);
			(target as IStateUpdate)?.OnUpdate(thread);
		}

		public override void End(Thread thread)
		{
			base.End(thread);
			(target as IStateEnd)?.OnEnd(thread);
		}

	}

	public class GameObjectMachineState : MachineState
	{
		public GameObject target;
		public GameObjectMachineState(string name, GameObject target) : base(name)
		{
			this.target = target;
		}


		public override void Start(Thread thread)
		{
			base.Start(thread);
			target.SetActive(true);

			foreach (IStateStart item in target.GetComponentsInChildren<IStateStart>())
			{
				item.OnStart(thread);
			}

		}

		public override void Update(Thread thread)
		{
			base.Update(thread);

			foreach (IStateUpdate item in target.GetComponentsInChildren<IStateUpdate>())
			{
				item.OnUpdate(thread);
			}
		}

		public override void End(Thread thread)
		{
			base.End(thread);
			target.SetActive(false);

			foreach (IStateEnd item in target.GetComponentsInChildren<IStateEnd>())
			{
				item.OnEnd(thread);
			}
		}
	}
}
