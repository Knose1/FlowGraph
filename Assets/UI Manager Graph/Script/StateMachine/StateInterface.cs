using Com.Github.Knose1.Flow.Engine;

namespace Com.Github.Knose1.Flow.Engine.Machine.Interfaces
{
	public interface IState { }

	/// <summary>
	/// Called when state starts
	/// </summary>
	public interface IStateStart : IState
	{
		void OnStart(Thread thread);
	}

	/// <summary>
	/// Called when state end
	/// </summary>
	public interface IStateEnd : IState
	{
		void OnEnd(Thread thread);
	}

	/// <summary>
	/// Called when state update
	/// </summary>
	public interface IStateUpdate : IState
	{
		void OnUpdate(Thread thread);
	}

}
