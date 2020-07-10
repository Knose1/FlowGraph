namespace Com.Github.Knose1.Flow.Editor.Generate
{
	[System.Serializable]
	public struct GeneratePosition
	{
		public string EVENTS;
		public string GO_FIELDS;
		public string CLASS_FIELDS;
		public string STATES;
		public string CREATE_STATES;
	}

	[System.Serializable]
	public struct TemplateJsonData
	{
		public GeneratePosition EventState;
		public GeneratePosition GoState;
		public GeneratePosition ClassMachineState;
		public string ALLOW_TRIGGERS;
		public string ADD_TRIGGERS;
		public string ADD_EVENTS;
		public string ENTRY_STATE;
	}
}
