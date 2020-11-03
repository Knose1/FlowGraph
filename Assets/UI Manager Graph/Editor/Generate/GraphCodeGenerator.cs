﻿using Com.Github.Knose1.Flow.Engine.Machine;
using Com.Github.Knose1.Flow.Engine.Settings;
using Com.Github.Knose1.Flow.Engine.Settings.NodeData;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Com.Github.Knose1.Flow.Editor.Generate
{
	public static class GraphCodeGenerator
	{
		private const string END_STATE         = "END_STATE";

		private const string EVENTS         = "EVENTS";
		private const string GO_FIELDS      = "GO_FIELDS";
		private const string CLASS_FIELDS   = "CLASS_FIELDS";
		private const string STATES         = "STATES";
		private const string CREATE_STATES  = "CREATE_STATES";
		private const string ALLOW_TRIGGERS = "ALLOW_TRIGGERS";
		private const string ADD_TRIGGERS   = "ADD_TRIGGERS";
		private const string ADD_EVENTS     = "ADD_EVENTS";
		private const string ENTRY_STATE	= "ENTRY_STATE";

		private const string NAMESPACE		= "NAMESPACE";
		private const string CLASS			= "CLASS";
		private const string H_STATE_NAME	= "H_STATE_NAME";
		private const string L_STATE_NAME	= "L_STATE_NAME";
		private const string TRIGGER		= "TRIGGER";
		private const string CREATE_THREAD	= "CREATE_THREAD";

		private const string PREFIX = "#{";
		private const string SUFFIX = "}#";

		//use of const because true.toString() returns "True"
		private const string TRUE  = "true";
		private const string FALSE = "false";
		private const string NULL = "null";

		private const string UNDEFINED = "[UNDEFINED_REPLACER]";
		private const string DEBUG_PREFIX = "[" + nameof(GraphCodeGenerator) + "]";
		private readonly static Regex END_STATE_REGEX       = new Regex(Regex.Escape(PREFIX+ END_STATE      +SUFFIX));

		private readonly static Regex EVENTS_REGEX          = new Regex(Regex.Escape(PREFIX+ EVENTS			+SUFFIX));
		private readonly static Regex GO_FIELDS_REGEX       = new Regex(Regex.Escape(PREFIX+ GO_FIELDS		+SUFFIX));
		private readonly static Regex CLASS_FIELDS_REGEX    = new Regex(Regex.Escape(PREFIX+ CLASS_FIELDS	+SUFFIX));
		private readonly static Regex STATES_REGEX          = new Regex(Regex.Escape(PREFIX+ STATES			+SUFFIX));
		private readonly static Regex CREATE_STATES_REGEX   = new Regex(Regex.Escape(PREFIX+ CREATE_STATES	+SUFFIX));
		private readonly static Regex ALLOW_TRIGGERS_REGEX  = new Regex(Regex.Escape(PREFIX+ ALLOW_TRIGGERS +SUFFIX));
		private readonly static Regex ADD_TRIGGERS_REGEX    = new Regex(Regex.Escape(PREFIX+ ADD_TRIGGERS	+SUFFIX));
		private readonly static Regex ADD_EVENTS_REGEX      = new Regex(Regex.Escape(PREFIX+ ADD_EVENTS		+SUFFIX));
		private readonly static Regex ENTRY_STATE_REGEX     = new Regex(Regex.Escape(PREFIX+ ENTRY_STATE	+SUFFIX));

		private readonly static Regex NAMESPACE_REGEX       = new Regex(Regex.Escape(PREFIX+ NAMESPACE		+SUFFIX));
		private readonly static Regex CLASS_REGEX           = new Regex(Regex.Escape(PREFIX+ CLASS			+SUFFIX));
		private readonly static Regex H_STATE_NAME_REGEX    = new Regex(Regex.Escape(PREFIX+ H_STATE_NAME	+SUFFIX));
		private readonly static Regex L_STATE_NAME_REGEX    = new Regex(Regex.Escape(PREFIX+ L_STATE_NAME	+SUFFIX));
		private readonly static Regex TRIGGER_REGEX         = new Regex(Regex.Escape(PREFIX+ TRIGGER		+SUFFIX));
		private readonly static Regex CREATE_THREAD_REGEX   = new Regex(Regex.Escape(PREFIX+ CREATE_THREAD	+SUFFIX));
		
		private readonly static Regex NAMESPACE_0_REGEX       = new Regex(Regex.Escape(PREFIX+ NAMESPACE	 + "_0"	+SUFFIX));
		private readonly static Regex CLASS_0_REGEX           = new Regex(Regex.Escape(PREFIX+ CLASS		 + "_0"	+SUFFIX));
		private readonly static Regex H_STATE_NAME_0_REGEX    = new Regex(Regex.Escape(PREFIX+ H_STATE_NAME  + "_0" +SUFFIX));
		private readonly static Regex L_STATE_NAME_0_REGEX    = new Regex(Regex.Escape(PREFIX+ L_STATE_NAME  + "_0" +SUFFIX));

		private readonly static Regex NAMESPACE_1_REGEX       = new Regex(Regex.Escape(PREFIX+ NAMESPACE     + "_1" +SUFFIX));
		private readonly static Regex CLASS_1_REGEX           = new Regex(Regex.Escape(PREFIX+ CLASS         + "_1" +SUFFIX));
		private readonly static Regex H_STATE_NAME_1_REGEX    = new Regex(Regex.Escape(PREFIX+ H_STATE_NAME  + "_1" +SUFFIX));
		private readonly static Regex L_STATE_NAME_1_REGEX    = new Regex(Regex.Escape(PREFIX+ L_STATE_NAME  + "_1" +SUFFIX));

		private readonly static Regex CONTAINS_UNDEFINED   = new Regex(Regex.Escape(UNDEFINED));

		private static string ReplaceJsonDataTemplate(string input, string @namespace, string @class, string hStateName, string lStateName, string trigger, string createThread)
		{
			MatchCollection namespaceMatch = NAMESPACE_REGEX.Matches(input);
			MatchCollection classMatch = CLASS_REGEX.Matches(input);
			MatchCollection hStateMatch = H_STATE_NAME_REGEX.Matches(input);
			MatchCollection lStateMatch = L_STATE_NAME_REGEX.Matches(input);
			MatchCollection triggerMatch = TRIGGER_REGEX.Matches(input);
			MatchCollection createThreadMatch = CREATE_THREAD_REGEX.Matches(input);
			MatchCollection endStateMatch = END_STATE_REGEX.Matches(input);

			List<MatchAndReplacer> replacers = MatchAndReplacer.JoinMultiple(
				MatchAndReplacer.GetMatchAndReplacers(endStateMatch, StateMachine.END_STATE),
				MatchAndReplacer.GetMatchAndReplacers(namespaceMatch, @namespace),
				MatchAndReplacer.GetMatchAndReplacers(classMatch, @class),
				MatchAndReplacer.GetMatchAndReplacers(hStateMatch, hStateName),
				MatchAndReplacer.GetMatchAndReplacers(lStateMatch, lStateName),
				MatchAndReplacer.GetMatchAndReplacers(triggerMatch, trigger),
				MatchAndReplacer.GetMatchAndReplacers(createThreadMatch, createThread)
			);

			replacers.Sort((MatchAndReplacer a, MatchAndReplacer b) => {
				return a.match.Index - b.match.Index;
			});

			input = input.Replace(replacers);

			return input;
		}
		
		private static string ReplaceJsonDataTemplateMultiple(string input, 
			string @namespace_0, string @class_0, string hStateName_0, string lStateName_0, string trigger, string createThread,
			string @namespace_1, string @class_1, string hStateName_1, string lStateName_1
		)
		{
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

			replacers.Sort((MatchAndReplacer a, MatchAndReplacer b) => {
				return a.match.Index - b.match.Index;
			});

			input = input.Replace(replacers);

			return input;
		}

		public static string Generate(string template, TemplateJsonData dataTemplate, NodeDataList data)
		{
			List<string> states			= new List<string>();
			List<string> createStates	= new List<string>();

			List<string> events			= new List<string>();
			
			List<string> goFields		= new List<string>();

			List<string> classFields	= new List<string>();
			
			List<string> allowTriggers	= new List<string>();
			List<string> addTriggers    = new List<string>();
			List<string> addEvents		= new List<string>();

			List<StateNodeData> stateNodes = data.stateNodes;
			data.GetNodes(out List<NodeData> nodes);
			int stateNodesCount = stateNodes.Count;

			void LGetStateData(StateNodeData state, out string @namespace, out string @class, out string hStateName, out string lStateName)
			{
				//Higher CamelCase
				hStateName = state.name;
				hStateName = hStateName.Remove(0, 1).Insert(0, hStateName.Substring(0, 1).ToUpper()); // set the 1st character to Upper

				//Lower camelCase
				lStateName = state.name;
				lStateName = lStateName.Remove(0, 1).Insert(0, lStateName.Substring(0, 1).ToLower()); //set the 1st character to lower

				@namespace = state.@namespace;
				@class = state.@class;
			}

			void LGetPortDatas(StateNodeData.StateNodePort port, out string trigger, out string createThread)
			{
				trigger = port.trigger;
				createThread = BoolToString(port.createThread);
			}

			for (int i = 0; i < stateNodesCount; i++)
			{
				StateNodeData state = stateNodes[i];
				int portCount = state.ports.Count;

				LGetStateData(state, out string @namespace, out string @class, out string hStateName, out string lStateName);

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
				}

				for (int j = 0; j < portCount; j++)
				{
					StateNodeData.StateNodePort stateNodeOutputPort = state.ports[j];
					
					LGetPortDatas(stateNodeOutputPort, out string trigger, out string createThread);
					allowTriggers.Add(ReplaceJsonDataTemplate(dataTemplate.ALLOW_TRIGGERS, @namespace, @class, hStateName, lStateName, trigger, createThread));
					
					List<ConnectorData> connections = data.FindOutputNode(state, stateNodeOutputPort);
					int connectionCount = connections.Count;

					for (int k = 0; k < connectionCount; k++)
					{
						ConnectorData connection = connections[k];
						ConnectedPortData nodePortData = connection.input;

						NodeData otherNode = nodePortData.GetNode(nodes);
						if (otherNode is RerouteData)
						{
							NodeData node = (otherNode as RerouteData).GetNextEffectNode(data);

							otherNode = node;
						}

						if (otherNode is ExitNodeData)
						{
							addTriggers.Add(ReplaceJsonDataTemplate(dataTemplate.END_STATE, @namespace, @class, hStateName, lStateName, trigger, FALSE));
							continue;
						}

						if (otherNode is StateNodeData)
						{
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
					entryState = ReplaceJsonDataTemplate(dataTemplate.ENTRY_STATE, UNDEFINED, UNDEFINED, StateMachine.END_STATE, StateMachine.END_STATE, UNDEFINED, FALSE);
			}


			MatchCollection namespaceMatch      = NAMESPACE_REGEX.Matches(template);
			MatchCollection classMatch			= CLASS_REGEX.Matches(template);
			MatchCollection eventsMatch         = EVENTS_REGEX.Matches(template);
			MatchCollection goFieldsMatch       = GO_FIELDS_REGEX.Matches(template);
			MatchCollection classFieldsMatch    = CLASS_FIELDS_REGEX.Matches(template);
			MatchCollection statesMatch         = STATES_REGEX.Matches(template);
			MatchCollection createStatesMatch   = CREATE_STATES_REGEX.Matches(template);
			MatchCollection allowTriggersMatch  = ALLOW_TRIGGERS_REGEX.Matches(template);
			MatchCollection addTriggersMatch    = ADD_TRIGGERS_REGEX.Matches(template);
			MatchCollection addEventsMatch      = ADD_EVENTS_REGEX.Matches(template);
			MatchCollection entryStateMatch     = ENTRY_STATE_REGEX.Matches(template);

			string LJoin(IList<string> str, string join = "") {
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
					MatchAndReplacer.GetMatchAndReplacers(eventsMatch, LJoin(events, "\r\n")),
					MatchAndReplacer.GetMatchAndReplacers(goFieldsMatch, LJoin(goFields, "\r\n")),
					MatchAndReplacer.GetMatchAndReplacers(classFieldsMatch, LJoin(classFields, "\r\n")),
					MatchAndReplacer.GetMatchAndReplacers(statesMatch, LJoin(states, "\r\n")),
					MatchAndReplacer.GetMatchAndReplacers(createStatesMatch, LJoin(createStates, "\r\n")),
					MatchAndReplacer.GetMatchAndReplacers(allowTriggersMatch, LJoin(allowTriggers, "\r\n")),
					MatchAndReplacer.GetMatchAndReplacers(addTriggersMatch, LJoin(addTriggers, "\r\n")),
					MatchAndReplacer.GetMatchAndReplacers(addEventsMatch, LJoin(addEvents, "\r\n")),
					MatchAndReplacer.GetMatchAndReplacers(entryStateMatch, entryState)
				);

			replacers.Sort((MatchAndReplacer a, MatchAndReplacer b) => {
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
			int indexDifference = 0;

			foreach (MatchAndReplacer item in matchAndReplacers)
			{
				Match match = item.match;
				string replacer = item.replacer;

				input = input.Remove(indexDifference + match.Index, match.Length).Insert(indexDifference + match.Index, replacer);
				indexDifference += replacer.Length - match.Length;
			}

			return input;
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

			public static List<MatchAndReplacer> JoinMultiple(List<MatchAndReplacer>first, params List<MatchAndReplacer>[] matchAndReplacers)
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
