namespace Com.Github.Knose1.Flow.Editor
{
	public static class FlowGraphAssetDatabase
	{
		public const string ASSET_ROOT_NAME = "Assets/";
		public const string ASSET_FOLDER = ASSET_ROOT_NAME+"UI Manager Graph/Editor/Asset/";

		/// <summary>
		/// The GUID save text file
		/// </summary>
		public const string GUID_TEXT_FILE = ASSET_FOLDER+"SettingLastSelectedGuid.txt";
		/// <summary>
		/// The stylesheet name
		/// </summary>
		public const string RESSOURCE_STYLESHEET = ASSET_FOLDER+"Graph.uss";
		/// <summary>
		/// The stylesheet black name
		/// </summary>
		public const string RESSOURCE_STYLESHEET_BLACK = ASSET_FOLDER+"GraphBlack.uss";
		/// <summary>
		/// The stylesheet white name
		/// </summary>
		public const string RESSOURCE_STYLESHEET_WHITE = ASSET_FOLDER+"GraphWhite.uss";

		/// <summary>
		/// Arguments : <br/> 
		/// - #{NAMESPACE}#: The class's namespace
		/// - #{CLASS}#: The class's name
		/// - #{EVENTS}#: The event fields
		/// - #{GO_FIELDS}#: The game object fields
		/// - #{CLASS_FIELDS}#: The class object fields
		/// - #{STATES}#: The MachineState fields
		/// - #{CREATE_STATES}#: Where to create the states
		/// - #{ALLOW_TRIGGERS}# : Where to allow triggers
		/// - #{ADD_TRIGGERS}# : Where to add triggers to states
		/// - #{ADD_EVENTS}# : Where to add events to
		/// - #{ENTRY_STATE}# : The first state to be executed
		/// </summary>
		public const string CLASS_TEMPLATE = ASSET_FOLDER+"Class_template.cs.txt";
		public const string SUBSTATE_CLASS_TEMPLATE = ASSET_FOLDER+"SubStateClass_templace.cs.txt";

		/// <summary>
		/// <see cref="Generate.TemplateJsonData"/>
		/// </summary>
		public const string ARGS_TEMPLATE = ASSET_FOLDER+"TemplateArgs.json";
	}
}