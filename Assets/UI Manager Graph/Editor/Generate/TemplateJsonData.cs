namespace Com.Github.Knose1.Flow.Editor.Generate
{
	[System.Serializable]
	public struct GeneratePosition
	{
		public string EVENTS;
		public string GO_FIELDS;
		public string CLASS_FIELDS;
		public string SUBSTATE_FIELDS;
		public string STATES;
		public string CREATE_STATES;
		public string STATE_NAME_SUFFIX;
		public string FIELD_NAME_SUFFIX;

	}

	[System.Serializable]
	public struct TemplateJsonData
	{
		public GeneratePosition EmptyState;
		public GeneratePosition EventState;
		public GeneratePosition GoState;
		public GeneratePosition ClassMachineState;
		public GeneratePosition SubstateMachine;
		public string SUB_STATE_NEXT;
		public string ALLOW_TRIGGERS;
		public string ADD_TRIGGERS;
		public string ADD_EVENTS;
		public string ENTRY_STATE;
		public string END_STATE;
		public string STOP_STATE;
	}
}
