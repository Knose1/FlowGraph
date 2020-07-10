namespace Com.Github.Knose1.Flow.Engine.Machine.State
{
	public class TriggerData
	{
		public MachineState state;
		public string trigger;
		public bool createThread;

		public TriggerData(MachineState state, string trigger, bool createThread)
		{
			this.state = state;
			this.trigger = trigger;
			this.createThread = createThread;
		}
	}

}
