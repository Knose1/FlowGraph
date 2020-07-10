using Com.Github.Knose1.Flow.Engine;

namespace Com.Github.Knose1.Flow.Engine.Machine.Interfaces
{
	public interface IState { }

	public interface IStateStart : IState
	{
		void OnStart(Thread thread);
	}

	public interface IStateEnd : IState
	{
		void OnEnd(Thread thread);
	}

	public interface IStateUpdate : IState
	{
		void OnUpdate(Thread thread);
	}

}
