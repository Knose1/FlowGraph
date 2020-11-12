using Com.Github.Knose1.Flow.Editor.Node;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Com.Github.Knose1.Flow.Editor
{
	public class FlowGraphPort : Port
	{
		new public virtual Color portColor
		{
			get => base.portColor;
			set
			{
				base.portColor = value;

				m_ConnectorBox.style.borderBottomColor =
				m_ConnectorBox.style.borderTopColor =
				m_ConnectorBox.style.borderLeftColor =
				m_ConnectorBox.style.borderRightColor = value;

				if (connections.Count() > 0)
				{
					m_ConnectorBoxCap.style.backgroundColor = value;
				}

				foreach (var conn in connections)
				{
					conn.MarkDirtyRepaint();
					conn.OnPortChanged(direction == Direction.Input);
				}
			}
		}

		private DefaultEdgeConnectorListener connectorListener;

		public static event Action<FlowGraphPort, FlowGraphPort> OnConnection;

		protected FlowGraphPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type) {}

		//https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/GraphViewEditor/Elements/Port.cs#L294
		private class DefaultEdgeConnectorListener : IEdgeConnectorListener
		{
			private const int OFFSET_REROUTE = 50;
			private GraphViewChange m_GraphViewChange;
			private List<Edge> m_EdgesToCreate;
			private List<GraphElement> m_EdgesToDelete;
			private FlowGraphPort m_port;
			private GraphView m_GraphView;

			public DefaultEdgeConnectorListener(FlowGraphPort port)
			{
				m_EdgesToCreate = new List<Edge>();
				m_EdgesToDelete = new List<GraphElement>();
				m_port = port;
				m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
			}

			public void OnDropOutsidePort(Edge edge, Vector2 position)
			{
				if (m_GraphView == null) m_GraphView = m_port.GetFirstAncestorOfType<GraphView>();

 				List<UnityEditor.Experimental.GraphView.Node> query = m_GraphView.nodes.ToList();
				int count = query.Count;
				for (int i = 0; i < count; i++)
				{
					var node = query[i];
					if (node.ContainsPoint(position))
					{
						if (node is RerouteNode)
						{
							RerouteNode reroute = (RerouteNode)node;

							if (edge.input  == null) reroute.output.Connect(edge);
							if (edge.output == null) reroute.input.Connect(edge);
						}
						break;
					}
				}
			}
			public void OnDrop(GraphView graphView, Edge edge)
			{
				m_GraphView = graphView;
				//
				m_EdgesToCreate.Clear();
				m_EdgesToCreate.Add(edge);

				// We can't just add these edges to delete to the m_GraphViewChange
				// because we want the proper deletion code in GraphView to also
				// be called. Of course, that code (in DeleteElements) also
				// sends a GraphViewChange.

				m_EdgesToDelete.Clear();
				if (edge.input != null && edge.input.capacity == Capacity.Single)
					foreach (Edge edgeToDelete in edge.input.connections)
						if (edgeToDelete != edge)
							m_EdgesToDelete.Add(edgeToDelete);

				if (edge.output != null && edge.output.capacity == Capacity.Single)
					foreach (Edge edgeToDelete in edge.output.connections)
						if (edgeToDelete != edge)
							m_EdgesToDelete.Add(edgeToDelete);

				if (m_EdgesToDelete.Count > 0)
					graphView.DeleteElements(m_EdgesToDelete);

				var edgesToCreate = m_EdgesToCreate;
				if (graphView.graphViewChanged != null)
				{
					edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;
				}

				foreach (Edge e in edgesToCreate)
				{
					graphView.AddElement(e);
					edge.input?.Connect(e);
					edge.output?.Connect(e);

					if (e.input == null || e.output == null || !(graphView is FlowGraph) || (graphView as FlowGraph).isUnserializing) continue;

					UnityEditor.Experimental.GraphView.Node node1 = e.input.node;
					if (node1 == e.output.node)
					{
						FlowGraph flowGraph = (graphView as FlowGraph);

						//Reroute 1
						Rect bound = node1.localBound;
						RerouteNode reroute = flowGraph.CreateRetoute();
						
							//Add at top right of the node
						Rect pos = reroute.GetPosition();
						pos.position = bound.position + (OFFSET_REROUTE/2+bound.width) * Vector2.right + (OFFSET_REROUTE * UnityEngine.Random.value) * Vector2.down;
						reroute.SetPosition(pos);

						RerouteNode.AddReroute(e, reroute, flowGraph, out Edge outEdge, out Edge inpEdge);


						//Reroute 2
						RerouteNode reroute2 = flowGraph.CreateRetoute();
						
							//Add at top left of the node
						Rect pos2 = reroute2.GetPosition();
						pos2.position = bound.position - OFFSET_REROUTE*1.5f * Vector2.right + (OFFSET_REROUTE * UnityEngine.Random.value) * Vector2.down;
						reroute2.SetPosition(pos2);
						
						RerouteNode.AddReroute(outEdge, reroute2, flowGraph);
					}
				}

				if (!(graphView as FlowGraph).isInit)
				{
					switch (m_port.direction)
					{
						case Direction.Input:
							OnConnection?.Invoke(m_port as FlowGraphPort, edge.output as FlowGraphPort);
							break;
						case Direction.Output:
							OnConnection?.Invoke(edge.input as FlowGraphPort, m_port as FlowGraphPort);
							break;
					}
				}

			}
		}

		public static FlowGraphPort Create(Orientation orientation, Direction direction, Capacity capacity, Type type)
		{
			var port = new FlowGraphPort(orientation, direction, capacity, type);
			var connectorListener = new DefaultEdgeConnectorListener(port);

			port.m_EdgeConnector = new EdgeConnector<FlowGraphEdge>(connectorListener);

			port.connectorListener = connectorListener;
			
			port.AddManipulator(port.m_EdgeConnector);
			return port;
		}
	}
}
