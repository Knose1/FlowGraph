using Com.Github.Knose1.Common;
using Com.Github.Knose1.Flow.Engine.Machine;
using Com.Github.Knose1.Flow.Engine.Settings;
using Com.Github.Knose1.Flow.Engine.Settings.NodeData;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Com.Github.Knose1.Flow.Editor.Generate
{
	using static GraphCodeGenerator.Library;
	/// <summary>
	/// A class that compile a NodeDataList into C# script (string) using a template
	/// </summary>
	public static class GraphCodeGenerator
	{
		public const string CRLF = "\r\n";

		public static class Library
		{
			public const string END_STATE		= "END_STATE";
			public const string STOP_STATE      = "STOP_STATE";

			public const string EVENTS          = "EVENTS";
			public const string GO_FIELDS       = "GO_FIELDS";
			public const string CLASS_FIELDS    = "CLASS_FIELDS";
			public const string SUBSTATE_FIELDS = "SUBSTATE_FIELDS";
			public const string STATES          = "STATES";
			public const string CREATE_STATES   = "CREATE_STATES";
			public const string ALLOW_TRIGGERS  = "ALLOW_TRIGGERS";
			public const string ADD_TRIGGERS    = "ADD_TRIGGERS";
			public const string ADD_EVENTS      = "ADD_EVENTS";
			public const string ENTRY_STATE     = "ENTRY_STATE";
			public const string SUBSTATE_CLASS  = "SUBSTATE_CLASS";

			public const string NAMESPACE       = "NAMESPACE";
			public const string CLASS           = "CLASS";
			public const string H_STATE_NAME    = "H_STATE_NAME";
			public const string L_STATE_NAME    = "L_STATE_NAME";
			public const string TRIGGER         = "TRIGGER";
			public const string CREATE_THREAD   = "CREATE_THREAD";

			public const string PREFIX = "#{";
			public const string SUFFIX = "}#";

			//use of const because true.toString() returns "True"
			public const string TRUE  = "true";
			public const string FALSE = "false";
			private const string NULL = "null";

			public const string UNDEFINED = "/*[UNDEFINED_REPLACER]*/";
			public const string DEBUG_PREFIX = "[" + nameof(GraphCodeGenerator) + "]";

			#region REGEX
			public readonly static Regex END_STATE_REGEX		= new Regex(Regex.Escape(PREFIX+ END_STATE      +SUFFIX));
			public readonly static Regex STOP_STATE_REGEX       = new Regex(Regex.Escape(PREFIX+ STOP_STATE      +SUFFIX));

			public readonly static Regex EVENTS_REGEX           = new Regex(Regex.Escape(PREFIX+ EVENTS         +SUFFIX));
			public readonly static Regex GO_FIELDS_REGEX        = new Regex(Regex.Escape(PREFIX+ GO_FIELDS      +SUFFIX));
			public readonly static Regex CLASS_FIELDS_REGEX     = new Regex(Regex.Escape(PREFIX+ CLASS_FIELDS   +SUFFIX));
			public readonly static Regex SUBSTATE_FIELDS_REGEX  = new Regex(Regex.Escape(PREFIX+ SUBSTATE_FIELDS +SUFFIX));
			public readonly static Regex STATES_REGEX           = new Regex(Regex.Escape(PREFIX+ STATES         +SUFFIX));
			public readonly static Regex CREATE_STATES_REGEX    = new Regex(Regex.Escape(PREFIX+ CREATE_STATES  +SUFFIX));
			public readonly static Regex ALLOW_TRIGGERS_REGEX   = new Regex(Regex.Escape(PREFIX+ ALLOW_TRIGGERS +SUFFIX));
			public readonly static Regex ADD_TRIGGERS_REGEX     = new Regex(Regex.Escape(PREFIX+ ADD_TRIGGERS   +SUFFIX));
			public readonly static Regex ADD_EVENTS_REGEX       = new Regex(Regex.Escape(PREFIX+ ADD_EVENTS     +SUFFIX));
			public readonly static Regex ENTRY_STATE_REGEX      = new Regex(Regex.Escape(PREFIX+ ENTRY_STATE    +SUFFIX));
			public readonly static Regex SUBSTATE_CLASS_REGEX   = new Regex(Regex.Escape(PREFIX+ SUBSTATE_CLASS +SUFFIX));

			public readonly static Regex NAMESPACE_REGEX        = new Regex(Regex.Escape(PREFIX+ NAMESPACE      +SUFFIX));
			public readonly static Regex CLASS_REGEX            = new Regex(Regex.Escape(PREFIX+ CLASS          +SUFFIX));
			public readonly static Regex H_STATE_NAME_REGEX     = new Regex(Regex.Escape(PREFIX+ H_STATE_NAME   +SUFFIX));
			public readonly static Regex L_STATE_NAME_REGEX     = new Regex(Regex.Escape(PREFIX+ L_STATE_NAME   +SUFFIX));
			public readonly static Regex TRIGGER_REGEX          = new Regex(Regex.Escape(PREFIX+ TRIGGER        +SUFFIX));
			public readonly static Regex CREATE_THREAD_REGEX    = new Regex(Regex.Escape(PREFIX+ CREATE_THREAD  +SUFFIX));

			public readonly static Regex NAMESPACE_0_REGEX      = new Regex(Regex.Escape(PREFIX+ NAMESPACE     + "_0" +SUFFIX));
			public readonly static Regex CLASS_0_REGEX          = new Regex(Regex.Escape(PREFIX+ CLASS         + "_0" +SUFFIX));
			public readonly static Regex H_STATE_NAME_0_REGEX   = new Regex(Regex.Escape(PREFIX+ H_STATE_NAME  + "_0" +SUFFIX));
			public readonly static Regex L_STATE_NAME_0_REGEX   = new Regex(Regex.Escape(PREFIX+ L_STATE_NAME  + "_0" +SUFFIX));

			public readonly static Regex NAMESPACE_1_REGEX      = new Regex(Regex.Escape(PREFIX+ NAMESPACE     + "_1" +SUFFIX));
			public readonly static Regex CLASS_1_REGEX          = new Regex(Regex.Escape(PREFIX+ CLASS         + "_1" +SUFFIX));
			public readonly static Regex H_STATE_NAME_1_REGEX   = new Regex(Regex.Escape(PREFIX+ H_STATE_NAME  + "_1" +SUFFIX));
			public readonly static Regex L_STATE_NAME_1_REGEX	= new Regex(Regex.Escape(PREFIX+ L_STATE_NAME  + "_1" +SUFFIX));

			public readonly static Regex CONTAINS_UNDEFINED   = new Regex(Regex.Escape(UNDEFINED));
			#endregion REGEX
		}
		
		/// <summary>
		/// Function used to replace a template (input) by some values
		/// </summary>
		/// <param name="input"></param>
		/// <param name="namespace"></param>
		/// <param name="class"></param>
		/// <param name="hStateName"></param>
		/// <param name="lStateName"></param>
		/// <param name="trigger"></param>
		/// <param name="createThread"></param>
		/// <returns></returns>
		private static string ReplaceJsonDataTemplate(string input, string @namespace, string @class, string hStateName, string lStateName, string trigger, string createThread)
		{
			MatchCollection endStateMatch = END_STATE_REGEX.Matches(input);
			MatchCollection stopStateMatch = STOP_STATE_REGEX.Matches(input);

			MatchCollection namespaceMatch = NAMESPACE_REGEX.Matches(input);
			MatchCollection classMatch = CLASS_REGEX.Matches(input);
			MatchCollection hStateMatch = H_STATE_NAME_REGEX.Matches(input);
			MatchCollection lStateMatch = L_STATE_NAME_REGEX.Matches(input);
			MatchCollection triggerMatch = TRIGGER_REGEX.Matches(input);
			MatchCollection createThreadMatch = CREATE_THREAD_REGEX.Matches(input);

			List<MatchAndReplacer> replacers = MatchAndReplacer.JoinMultiple(
				MatchAndReplacer.GetMatchAndReplacers(endStateMatch, StateMachine.Machine.END_STATE),
				MatchAndReplacer.GetMatchAndReplacers(stopStateMatch, StateMachine.Machine.STOP_STATE),
				MatchAndReplacer.GetMatchAndReplacers(namespaceMatch, @namespace),
				MatchAndReplacer.GetMatchAndReplacers(classMatch, @class),
				MatchAndReplacer.GetMatchAndReplacers(hStateMatch, hStateName),
				MatchAndReplacer.GetMatchAndReplacers(lStateMatch, lStateName),
				MatchAndReplacer.GetMatchAndReplacers(triggerMatch, trigger),
				MatchAndReplacer.GetMatchAndReplacers(createThreadMatch, createThread)
			);

			replacers.Sort((MatchAndReplacer a, MatchAndReplacer b) =>
			{
				return a.match.Index - b.match.Index;
			});

			input = input.Replace(replacers);

			return input;
		}

		/// <summary>
		/// Function used to replace a template (input) by some values
		/// </summary>
		/// <param name="input"></param>
		/// <param name="namespace_0"></param>
		/// <param name="class_0"></param>
		/// <param name="hStateName_0"></param>
		/// <param name="lStateName_0"></param>
		/// <param name="trigger"></param>
		/// <param name="createThread"></param>
		/// <param name="namespace_1"></param>
		/// <param name="class_1"></param>
		/// <param name="hStateName_1"></param>
		/// <param name="lStateName_1"></param>
		/// <returns></returns>
		private static string ReplaceJsonDataTemplateMultiple(string input,
			string @namespace_0, string @class_0, string hStateName_0, string lStateName_0, string trigger, string createThread,
			string @namespace_1, string @class_1, string hStateName_1, string lStateName_1
		)
		{
			MatchCollection endStateMatch = END_STATE_REGEX.Matches(input);
			MatchCollection stopStateMatch = STOP_STATE_REGEX.Matches(input);

			MatchCollection namespaceMatch_0 = NAMESPACE_0_REGEX.Matches(input);
			MatchCollection classMatch_0 = CLASS_0_REGEX.Matches(input);
			MatchCollection hStateMatch_0 = H_STATE_NAME_0_REGEX.Matches(input);
			MatchCollection lStateMatch_0 = L_STATE_NAME_0_REGEX.Matches(input);
			MatchCollection triggerMatch = TRIGGER_REGEX.Matches(input);
			MatchCollection createThreadMatch = CREATE_THREAD_REGEX.Matches(input);

			MatchCollection namespaceMatch_1 = NAMESPACE_1_REGEX.Matches(input);
			MatchCollection classMatch_1 = CLASS_1_REGEX.Matches(input);
			MatchCollection hStateMatch_1 = H_STATE_NAME_1_REGEX.Matches(input);
			MatchCollection lStateMatch_1 = L_STATE_NAME_1_REGEX.Matches(input);

			List<MatchAndReplacer> replacers = MatchAndReplacer.JoinMultiple(
				MatchAndReplacer.GetMatchAndReplacers(endStateMatch, StateMachine.Machine.END_STATE),
				MatchAndReplacer.GetMatchAndReplacers(stopStateMatch, StateMachine.Machine.STOP_STATE),

				MatchAndReplacer.GetMatchAndReplacers(namespaceMatch_0, @namespace_0),
				MatchAndReplacer.GetMatchAndReplacers(classMatch_0, @class_0),
				MatchAndReplacer.GetMatchAndReplacers(hStateMatch_0, hStateName_0),
				MatchAndReplacer.GetMatchAndReplacers(lStateMatch_0, lStateName_0),
				MatchAndReplacer.GetMatchAndReplacers(triggerMatch, trigger),
				MatchAndReplacer.GetMatchAndReplacers(createThreadMatch, createThread),

				MatchAndReplacer.GetMatchAndReplacers(namespaceMatch_1, @namespace_1),
				MatchAndReplacer.GetMatchAndReplacers(classMatch_1, @class_1),
				MatchAndReplacer.GetMatchAndReplacers(hStateMatch_1, hStateName_1),
				MatchAndReplacer.GetMatchAndReplacers(lStateMatch_1, lStateName_1)
			);

			replacers.Sort((MatchAndReplacer a, MatchAndReplacer b) =>
			{
				return a.match.Index - b.match.Index;
			});

			input = input.Replace(replacers);

			return input;
		}

		/// <summary>
		/// Generate the C# code
		/// </summary>
		/// <param name="template">The templates used to generate the whole class</param>
		/// <param name="template">The templates used to generate the substate class</param>
		/// <param name="dataTemplate">The templates for specific parts of the code</param>
		/// <param name="data">The data to translate into C# code</param>
		/// <returns></returns>
		public static string Generate(string template, string substateTemplate, TemplateJsonData dataTemplate, NodeDataList data)
		{
			/****************************************************/
			/* Lists for specific parts of the code to generate */
			List<string> states         = new List<string>();
			List<string> createStates   = new List<string>();
			List<string> events         = new List<string>();
			List<string> goFields       = new List<string>();
			List<string> classFields    = new List<string>();
			List<string> subStateFields = new List<string>();
			List<string> allowTriggers  = new List<string>();
			List<string> addTriggers    = new List<string>();
			List<string> addEvents      = new List<string>();
			List<string> subStateClass  = new List<string>();
			/*                                                  */
			/****************************************************/

			List<StateNodeData> stateNodes = data.stateNodes;
			data.GetNodes(out List<NodeData> nodes);
			int stateNodesCount = stateNodes.Count;

			void LGetStateData(StateNodeData state, out string @namespace, out string @class, out string hStateName, out string lStateName)
			{
				//Higher CamelCase
				hStateName = state.name;
				hStateName = hStateName.ToUpperCamelCase();

				//Lower camelCase
				lStateName = state.name;
				lStateName = lStateName.ToLowerCamelCase();

				@namespace = state.@namespace;
				@class = state.@class;
			}

			void LGetPortDatas(StateNodeData.StateNodePort port, out string trigger, out string createThread)
			{
				trigger = port.trigger;
				createThread = BoolToString(port.createThread);
			}

			//Iterate on each node to generate the node
			for (int i = 0; i < stateNodesCount; i++)
			{
				StateNodeData state = stateNodes[i];
				int portCount = state.ports.Count;

				LGetStateData(state, out string @namespace, out string @class, out string hStateName, out string lStateName);

				/// <summary>
				/// Add states in the different List<string> (see at top of the function)
				/// </summary>
				void LAddState(GeneratePosition generatePosition, StateNodeData.Execution executionType, bool generateEvent = false)
				{
					if (executionType == StateNodeData.Execution.Event) generateEvent = true;

					states.Add(ReplaceJsonDataTemplate(generatePosition.STATES, @namespace, @class, hStateName, lStateName, UNDEFINED, UNDEFINED));
					createStates.Add(ReplaceJsonDataTemplate(generatePosition.CREATE_STATES, @namespace, @class, hStateName, lStateName, UNDEFINED, UNDEFINED));

					if (generateEvent)
					{
						events.Add(ReplaceJsonDataTemplate(generatePosition.EVENTS, @namespace, @class, hStateName, lStateName, UNDEFINED, UNDEFINED));
						addEvents.Add(ReplaceJsonDataTemplate(dataTemplate.ADD_EVENTS, @namespace, @class, hStateName, lStateName, UNDEFINED, UNDEFINED));
					}

					switch (executionType)
					{
						case StateNodeData.Execution.Instantiate:
							goFields.Add(ReplaceJsonDataTemplate(generatePosition.GO_FIELDS, @namespace, @class, hStateName, lStateName, UNDEFINED, UNDEFINED));
							break;
						case StateNodeData.Execution.Constructor:
							classFields.Add(ReplaceJsonDataTemplate(generatePosition.CLASS_FIELDS, @namespace, @class, hStateName, lStateName, UNDEFINED, UNDEFINED));
							break;
						case StateNodeData.Execution.SubState:
							subStateFields.Add(ReplaceJsonDataTemplate(generatePosition.SUBSTATE_FIELDS, @namespace, @class, hStateName, lStateName, UNDEFINED, UNDEFINED));
							break;
					}
				}

				switch (state.executionMode)
				{
					case StateNodeData.Execution.Instantiate:
						LAddState(dataTemplate.GoState, state.executionMode, state.generateEvent);
						break;
					case StateNodeData.Execution.Constructor:
						LAddState(dataTemplate.ClassMachineState, state.executionMode, state.generateEvent);
						break;
					case StateNodeData.Execution.Event:
						LAddState(dataTemplate.EventState, state.executionMode, true);
						break;
					case StateNodeData.Execution.Empty:
						LAddState(dataTemplate.EmptyState, state.executionMode, false);
						break;
					case StateNodeData.Execution.SubState:
						LAddState(dataTemplate.SubstateMachine, state.executionMode, false);
						subStateClass.Add(Generate(substateTemplate, substateTemplate, dataTemplate, state.subState.nodes));
						break;
				}

				//Iterate on each output port of the nodes to generate the connections
				//
				//Connections goes from output to input
				//
				for (int j = 0; j < portCount; j++)
				{
					StateNodeData.StateNodePort stateNodeOutputPort = state.ports[j];

					LGetPortDatas(stateNodeOutputPort, out string trigger, out string createThread);
					allowTriggers.Add(ReplaceJsonDataTemplate(dataTemplate.ALLOW_TRIGGERS, @namespace, @class, hStateName, lStateName, trigger, createThread));

					List<ConnectorData> connections = data.FindOutputNode(state, stateNodeOutputPort); //Find each connection linked to the output port
					int connectionCount = connections.Count;

					//For each connection
					for (int k = 0; k < connectionCount; k++)
					{
						ConnectorData connection = connections[k];
						ConnectedPortData nodePortData = connection.input;

						NodeData otherNode = nodePortData.GetNode(nodes);
						if (otherNode is RerouteData)
						{
							//If next node is a reroute, find where it goes
							NodeData node = (otherNode as RerouteData).GetNextEffectNode(data);

							otherNode = node;
						}

						if (otherNode is ExitNodeData)
						{
							//If exit, generate exit
							string jsonDataTemplate = "";
							switch ((otherNode as ExitNodeData).exitType)
							{
								case ExitNodeData.ExitType.StopThread:
									jsonDataTemplate = dataTemplate.END_STATE;
									break;
								case ExitNodeData.ExitType.StopMachine:
									jsonDataTemplate = dataTemplate.STOP_STATE;
									break;
								default:
									break;
							}

							addTriggers.Add(ReplaceJsonDataTemplate(jsonDataTemplate, @namespace, @class, hStateName, lStateName, trigger, FALSE));
							continue;
						}

						if (otherNode is StateNodeData)
						{
							//If state, generate state
							StateNodeData state_1 = otherNode as StateNodeData;

							LGetStateData(state_1, out string @namespace_1, out string @class_1, out string hStateName_1, out string lStateName_1);

							addTriggers.Add(
								ReplaceJsonDataTemplateMultiple(dataTemplate.ADD_TRIGGERS,
									@namespace, @class, hStateName, lStateName, trigger, createThread,
									@namespace_1, @class_1, hStateName_1, lStateName_1
								)
							);
						}

						if (otherNode == null) continue;


					}
				}

			}

			string entryNamespace = data.entryNode.@namespace;
			string entryClass = data.entryNode.@class;

			NodeData output = data.entryNode.GetFirstNode(data);
			string entryState = "";

			{
				if (output is StateNodeData)
				{
					LGetStateData(output as StateNodeData, out string @namespace_1, out string @class_1, out string hStateName_1, out string lStateName_1);
					entryState = ReplaceJsonDataTemplate(dataTemplate.ENTRY_STATE, UNDEFINED, UNDEFINED, hStateName_1, lStateName_1, UNDEFINED, FALSE);
				}
				else if (output is ExitNodeData)
					entryState = ReplaceJsonDataTemplate(dataTemplate.ENTRY_STATE, UNDEFINED, UNDEFINED, StateMachine.Machine.END_STATE, StateMachine.Machine.END_STATE, UNDEFINED, FALSE);
			}


			MatchCollection namespaceMatch      = NAMESPACE_REGEX.Matches(template);
			MatchCollection classMatch          = CLASS_REGEX.Matches(template);
			MatchCollection eventsMatch         = EVENTS_REGEX.Matches(template);
			MatchCollection goFieldsMatch       = GO_FIELDS_REGEX.Matches(template);
			MatchCollection classFieldsMatch    = CLASS_FIELDS_REGEX.Matches(template);
			MatchCollection substateFieldsMatch = SUBSTATE_FIELDS_REGEX.Matches(template);
			MatchCollection statesMatch         = STATES_REGEX.Matches(template);
			MatchCollection createStatesMatch   = CREATE_STATES_REGEX.Matches(template);
			MatchCollection allowTriggersMatch  = ALLOW_TRIGGERS_REGEX.Matches(template);
			MatchCollection addTriggersMatch    = ADD_TRIGGERS_REGEX.Matches(template);
			MatchCollection addEventsMatch      = ADD_EVENTS_REGEX.Matches(template);
			MatchCollection entryStateMatch     = ENTRY_STATE_REGEX.Matches(template);
			MatchCollection substateClassMatch  = SUBSTATE_CLASS_REGEX.Matches(template);

			string LJoin(IList<string> str, string join = "")
			{
				if (str.Count == 0) return "";
				if (str.Count == 1) return str[0];

				string toRet = "";
				int count = str.Count;

				for (int i = 0; i < count; i++)
				{
					toRet += str[i] + join;
				}

				toRet = toRet.Remove(toRet.Length - join.Length, join.Length);

				return toRet;
			}

			List<MatchAndReplacer> replacers = MatchAndReplacer.JoinMultiple(
					MatchAndReplacer.GetMatchAndReplacers(namespaceMatch, entryNamespace),
					MatchAndReplacer.GetMatchAndReplacers(classMatch, entryClass),
					MatchAndReplacer.GetMatchAndReplacers(eventsMatch, LJoin(events, CRLF)),
					MatchAndReplacer.GetMatchAndReplacers(goFieldsMatch, LJoin(goFields, CRLF)),
					MatchAndReplacer.GetMatchAndReplacers(classFieldsMatch, LJoin(classFields, CRLF)),
					MatchAndReplacer.GetMatchAndReplacers(substateFieldsMatch, LJoin(subStateFields, CRLF)),
					MatchAndReplacer.GetMatchAndReplacers(statesMatch, LJoin(states, CRLF)),
					MatchAndReplacer.GetMatchAndReplacers(createStatesMatch, LJoin(createStates, CRLF)),
					MatchAndReplacer.GetMatchAndReplacers(allowTriggersMatch, LJoin(allowTriggers, CRLF)),
					MatchAndReplacer.GetMatchAndReplacers(addTriggersMatch, LJoin(addTriggers, CRLF)),
					MatchAndReplacer.GetMatchAndReplacers(addEventsMatch, LJoin(addEvents, CRLF)),
					MatchAndReplacer.GetMatchAndReplacers(entryStateMatch, entryState),
					MatchAndReplacer.GetMatchAndReplacers(substateClassMatch, LJoin(subStateClass, CRLF))
				);

			replacers.Sort((MatchAndReplacer a, MatchAndReplacer b) =>
			{
				return a.match.Index - b.match.Index;
			});

			string toReturn = template.Replace(replacers);
			return toReturn;
		}

		private static string BoolToString(bool createThread)
		{
			return createThread ? TRUE : FALSE;
		}

		private static string Replace(this string input, List<MatchAndReplacer> matchAndReplacers)
		{
			const string NEXT_LINE = "\r\n|\n";
			Regex newLine = new Regex(NEXT_LINE);
			Regex tabCounter = new Regex("^\t|(?<=\t)\t");
			int indexDifference = 0;

			foreach (MatchAndReplacer item in matchAndReplacers)
			{
				MatchCollection lines = newLine.Matches(input);
				Match match = item.match;
				string replacer = item.replacer;

				int startIndex = indexDifference + match.Index;
				int numberTab = 0;

				{
					int firstBefore = GetFirstIndexBefore(lines, startIndex);
					int firstAfter = GetFirstIndexAfter(lines, startIndex, input.Length);

					numberTab = tabCounter.Matches(input.Substring(firstBefore, firstAfter - firstBefore)).Count;
				}

				replacer = newLine.Replace(replacer, "$&" + MultiplyString("\t", numberTab));

				input = input.Remove(startIndex, match.Length).Insert(startIndex, replacer);
				indexDifference += replacer.Length - match.Length;
			}

			return input;
		}

		private static string MultiplyString(string toMultiply, int count)
		{
			string toReturn = "";
			for (int i = 0; i < count; i++)
			{
				toReturn += toMultiply;
			}

			return toReturn;
		}

		private static int GetFirstIndexBefore(MatchCollection matchCollection, int maxIndexInString)
		{
			int matchIndex = 0;
			for (int i = 0; i < matchCollection.Count; i++)
			{
				Match m = matchCollection[i];
				int index = m.Index + m.Length;
				if (index < maxIndexInString && index > matchIndex)
				{
					matchIndex = index;
				}
			}

			return matchIndex;
		}

		private static int GetFirstIndexAfter(MatchCollection matchCollection, int minIndexInString, int stringLength)
		{
			int matchIndex = stringLength;
			for (int i = 0; i < matchCollection.Count; i++)
			{
				Match m = matchCollection[i];
				int index = m.Index;
				if (index > minIndexInString && index < matchIndex)
				{
					matchIndex = index;
				}
			}

			return matchIndex;
		}

		private struct MatchAndReplacer
		{
			public Match match;
			public string replacer;

			public MatchAndReplacer(Match match, string replacer)
			{
				this.match = match;
				this.replacer = replacer;
			}

			public static List<MatchAndReplacer> GetMatchAndReplacers(MatchCollection collection, string replacer)
			{
				List<MatchAndReplacer> toRet = new List<MatchAndReplacer>();

				foreach (Match item in collection)
				{
					toRet.Add(new MatchAndReplacer(item, replacer));
				}

				return toRet;
			}

			public static List<MatchAndReplacer> JoinMultiple(List<MatchAndReplacer> first, params List<MatchAndReplacer>[] matchAndReplacers)
			{
				foreach (List<MatchAndReplacer> item in matchAndReplacers)
				{
					first.AddRange(item);
				}

				return first;
			}
		}
	}
}