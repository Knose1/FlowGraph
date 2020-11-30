using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow.Editor.Node
{
	public class RerouteNode : TokenNode
	{
		private const float FF = 0xFF;
		private static readonly Color COLOR_WHITE = new Color(0x29 / FF, 0x29 / FF, 0x29 / FF);
		private static readonly Color COLOR_BLACK = new Color(0.784f, 0.784f, 0.784f);

		private bool hasColorBeingSet;

		private Color _color;
		public Color Color
		{
			get
			{
				return _color;
			}

			set
			{
				if (hasColorBeingSet) return;
				hasColorBeingSet = true;

				_color = value;
				SetPortColor(value);

				Edge inputEdge = output.connections.FirstOrDefault();
				Port inp;
				if (inputEdge == null || (inp = inputEdge.input) == null)
				{
					hasColorBeingSet = false;
					return;
				}
				if (inp.node is RerouteNode) 
					(inp.node as RerouteNode).Color = _color;
				
				hasColorBeingSet = false;
			}
		}

		public static Action OnChange { get; internal set; }

		private void SetPortColor(Color color)
		{
			(input as FlowGraphPort).portColor = color;
			(output as FlowGraphPort).portColor = color;
		}

		public RerouteNode() :
		base(
			FlowGraphPort.Create(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, null),
			FlowGraphPort.Create(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, null)
		)
		{
			this.elementTypeColor = new Color(0, 0, 0, 0);

			/*this.GetFirstChildOfType<Label>()?.RemoveFromHierarchy();
			this.GetFirstChildOfType<Image>()?.RemoveFromHierarchy();

			input.GetFirstChildOfType<Label>()?.RemoveFromHierarchy();
			output.GetFirstChildOfType<Label>()?.RemoveFromHierarchy();*/


			input.style.height = output.style.height = 15;
			input.style.paddingBottom = output.style.paddingBottom =
			input.style.paddingLeft = output.style.paddingLeft =
			input.style.paddingRight = output.style.paddingRight =
			input.style.paddingTop = output.style.paddingTop = 0;

			//input.parent.style.position = new StyleEnum<Position>(Position.Absolute);
			//input.parent.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);

			SetDefaultColor();
			RegisterCallback<MouseDownEvent>(OnMouseDown);
		}

		private void SetDefaultColor() => SetPortColor(_color = EditorGUIUtility.isProSkin ? COLOR_BLACK : COLOR_WHITE);
		private void SetDefaultColorPropagate() => Color = EditorGUIUtility.isProSkin ? COLOR_BLACK : COLOR_WHITE;
		private void OnMouseDown(MouseDownEvent evt)
		{
			OnChange?.Invoke();
		}

		public void UpdateColors()
		{
			BringToFront();

			Edge outputEdge;
			Port output;

			if ((outputEdge = input.connections.FirstOrDefault()) == null || (output = outputEdge.output) == null)
			{
				SetDefaultColorPropagate();
				return;
			}
			
			Color = output.portColor;

		}

		public static void AddReroute(Edge edge, RerouteNode reroute, FlowGraph flowGraph) => AddReroute(edge, reroute, flowGraph, out Edge outPut, out Edge inpPut);
		public static void AddReroute(Edge edge, RerouteNode reroute, FlowGraph flowGraph, out Edge outEdge, out Edge inpEdge)
		{
			Port inp = edge.input;
			Port outp = edge.output;

			flowGraph.DeleteElements(new GraphElement[] { edge });

			outEdge = reroute.output.ConnectTo<FlowGraphEdge>(inp);
			inpEdge = reroute.input.ConnectTo<FlowGraphEdge>(outp);

			flowGraph.AddElement(outEdge);
			flowGraph.AddElement(inpEdge);
		}

		public static void GetReroute(RerouteNode reroute, out FlowGraphNode inNode, out FlowGraphNode outNode) => GetReroute(reroute, out inNode, out outNode, out _, out _);
		public static void GetReroute(RerouteNode reroute, out FlowGraphNode inNode, out FlowGraphNode outNode, out Port inPort, out Port outPort)
		{
			GetReroute(reroute, out var inNodeNode, out var outNodeNode, out inPort, out outPort, null);

			inNode = inNodeNode as FlowGraphNode;
			outNode = outNodeNode as FlowGraphNode;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reroute"></param>
		/// <param name="inNode"></param>
		/// <param name="outNode"></param>
		/// <param name="inPort"></param>
		/// <param name="outPort"></param>
		/// <param name="filter">Nodes that can only be iterated, set to null for none</param>
		public static void GetReroute(RerouteNode reroute, out UnityEditor.Experimental.GraphView.Node inNode, out UnityEditor.Experimental.GraphView.Node outNode, out Port inPort, out Port outPort, List<RerouteNode> filter)
		{
			if (reroute is null)
			{
				throw new ArgumentNullException(nameof(reroute));
			}

			inNode = null;
			outNode = null;

			List<RerouteNode> alreadySeen = new List<RerouteNode>();

			UnityEditor.Experimental.GraphView.Node currentIn = reroute;
			UnityEditor.Experimental.GraphView.Node currentOut = reroute;

			bool filterIsNull = filter == null;
			bool isInFilter = false;
			
			do
			{
				alreadySeen.Add(currentIn as RerouteNode);
				currentIn = GetFirstInReroute(currentIn as RerouteNode, out inPort);
				isInFilter = filterIsNull || currentIn != null && filter.Contains(currentIn);
			} while (isInFilter && currentIn != null && currentIn is RerouteNode && !alreadySeen.Contains(currentIn));
			//Exit if : notInFilter, Null, not reroute or alreadySeen

			if (isInFilter && (currentIn == null || currentIn is RerouteNode))
				inNode = null;
			else
				inNode = currentIn;

			//--------------------------------------------//

			alreadySeen.Remove(reroute);

			//--------------------------------------------//

			isInFilter = false;
			do
			{
				alreadySeen.Add(currentOut as RerouteNode);
				currentOut = GetFirstOutReroute(currentOut as RerouteNode, out outPort);
				isInFilter = filterIsNull || currentOut != null && filter.Contains(currentOut);
			} while (isInFilter && currentOut != null && currentOut is RerouteNode && !alreadySeen.Contains(currentOut));
			//Exit if : notInFilter, Null, not reroute or alreadySeen

			if (isInFilter && (currentOut == null || currentOut is RerouteNode))
				outNode = null;
			else
				outNode = currentOut;
		}

		public static bool AreConsecutives(List<RerouteNode> toCheck)
		{
			if (toCheck is null)
			{
				throw new ArgumentNullException(nameof(toCheck));
			}

			if (toCheck.Count <= 1)
			{
				return true;
			}

			RerouteNode reroute = toCheck[0];

			if (reroute is null)
			{
				throw new ArgumentNullException(nameof(reroute));
			}

			List<RerouteNode> alreadySeen = new List<RerouteNode>();

			UnityEditor.Experimental.GraphView.Node currentIn = reroute;
			UnityEditor.Experimental.GraphView.Node currentOut = reroute;

			do
			{
				alreadySeen.Add(currentIn as RerouteNode);
				currentIn = GetFirstInReroute(currentIn as RerouteNode, out _);

			} while (toCheck.Contains(currentIn) && currentIn != null && currentIn is RerouteNode && !alreadySeen.Contains(currentIn));
			//Exit if : not contained in list, Null, not reroute or alreadySeen

			//--------------------------------------------//

			alreadySeen.Remove(reroute);

			//--------------------------------------------//

			do
			{
				alreadySeen.Add(currentOut as RerouteNode);
				currentOut = GetFirstOutReroute(currentOut as RerouteNode, out _);

			} while (toCheck.Contains(currentOut) && currentOut != null && currentOut is RerouteNode && !alreadySeen.Contains(currentOut));
			//Exit if : not contained in list, Null, not reroute or alreadySeen


			if (alreadySeen.Count != toCheck.Count)
			{
				foreach (var item in toCheck)
				{
					if (!alreadySeen.Contains(item)) return false;
				}
			}

			return true;
		}

		private static UnityEditor.Experimental.GraphView.Node GetFirstOutReroute(RerouteNode reroute, out Port port)
		{
			Edge e = reroute.output.connections.FirstOrDefault();
			port = null;

			if (e == null) return null;
			if ((port = e.input) == null) return null;

			return port.node;
		}

		private static UnityEditor.Experimental.GraphView.Node GetFirstInReroute(RerouteNode reroute, out Port port)
		{
			Edge e = reroute.input.connections.FirstOrDefault();
			port = null;

			if (e == null) return null;
			if ((port = e.output) == null) return null;

			return port.node;
		}
	}
}
