using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Msagl.Drawing;


namespace RapidPliant.App.Msagl
{
    public abstract class MsaglGraph<TState, TTransition, TNode, TEdge>
        where TNode : MsaglGraphNode<TState, TTransition, TNode, TEdge>, new()
        where TEdge : MsaglGraphNodeEdge<TState, TTransition, TNode, TEdge>, new()
    {
        protected Graph _graph;
        protected Dictionary<int, NodeEntry> _graphNodesById;
        protected Dictionary<TState, NodeEntry> _grahpNodesByState;
        protected int _nextStateId;

        public MsaglGraph()
        {
        }

        public Graph Graph { get { return _graph; } }

        public void Build(IEnumerable<TState> states)
        {
            _graph = CreateGraph();
            _graphNodesById = new Dictionary<int, NodeEntry>();
            _grahpNodesByState = new Dictionary<TState, NodeEntry>();
            _nextStateId = 1;

            CreateStateEntries(states);
            
            CreateTransitions(states);

            ConfigureNodes();
        }

        private void CreateStateEntries(IEnumerable<TState> states)
        {
            foreach (var state in states)
            {
                GetOrCreateNodeEntry(state);
            }
        }

        private NodeEntry GetOrCreateNodeEntry(TState state)
        {
            NodeEntry nodeEntry;
            if (!_grahpNodesByState.TryGetValue(state, out nodeEntry))
            {
                var nodeId = GetStateId(state);
                if (nodeId < 0)
                    nodeId = GenerateNodeId(state);

                nodeEntry = new NodeEntry(nodeId);
                nodeEntry.State = state;

                _grahpNodesByState[state] = nodeEntry;
                _graphNodesById[nodeId] = nodeEntry;
            }
            return nodeEntry;
        }

        protected virtual Graph CreateGraph()
        {
            return new Graph();
        }

        private void ConfigureNodes()
        {
            foreach (var nodeEntry in _graphNodesById.Values)
            {
                ConfigureNode(nodeEntry.Node);
            }
        }

        private void ConfigureNode(TNode graphNode)
        {
            var node = _graph.FindNode(graphNode.NodeId);
            graphNode.Node = node;

            node.Attr.Shape = Shape.Circle;
            node.Attr.XRadius = 1;
            node.Attr.YRadius = 1;
            node.Attr.LineWidth = 1;

            var state = graphNode.State;
            var isFinal = IsFinalState(state);
            if (isFinal)
            {
                node.Attr.Shape = Shape.DoubleCircle;
            }

            node.LabelText = GetStateLabel(state);

            PopulateGraphNode(graphNode);
        }

        protected virtual string GetStateLabel(TState state)
        {
            var nodeEntry = GetOrCreateNodeEntry(state);
            if(nodeEntry == null)
                return state.ToString();

            return nodeEntry.NodeId.ToString();
        }

        protected virtual bool IsFinalState(TState state)
        {
            var transitions = GetStateTransitions(state);
            var isFinal = !transitions.Any();
            return isFinal;
        }

        protected virtual void PopulateGraphNode(TNode graphNode)
        {
        }

        private void CreateTransitions(IEnumerable<TState> states)
        {
            foreach (var fromState in states)
            {
                var transitions = GetStateTransitions(fromState);
                if (transitions != null)
                {
                    foreach (var transition in transitions)
                    {
                        CreateGraphEdge(fromState, transition);
                    }
                }
            }
        }

        protected virtual void CreateGraphEdge(TState fromState, TTransition transition)
        {
            CreateTransitionGraphEdge(fromState, transition);
        }

        protected abstract IEnumerable<TTransition> GetStateTransitions(TState state);

        protected TNode GetOrCreateGraphNode(TState state)
        {
            var nodeEntry = GetOrCreateNodeEntry(state);
            var node = nodeEntry.Node;

            if (node == null)
            {
                node = CreateGraphNode(state);
                node.State = state;
                node.StateId = nodeEntry.NodeId;

                nodeEntry.Node = node;
            }

            return node;
        }

        protected virtual TNode CreateGraphNode(TState state)
        {
            return new TNode();
        }

        protected virtual int GenerateNodeId(TState state)
        {
            return _nextStateId++;
        }

        protected virtual int GetStateId(TState state)
        {
            return -1;
        }

        protected void CreateTransitionGraphEdge(TState fromState, TTransition transition)
        {
            var fromGraphNode = GetOrCreateGraphNode(fromState);
            if (fromGraphNode == null)
                return;

            var toState = GetTransitionToState(transition);
            if (toState == null)
                return;

            var toGraphNode = GetOrCreateGraphNode(toState);
            if (toGraphNode == null)
                return;

            var graphEdge = CreateTransitionGraphEdge(fromGraphNode, toGraphNode, transition);

            graphEdge.Edge.LabelText = GetTransitionLabel(graphEdge.Transition);

            PopulateGraphEdge(graphEdge);
        }

        protected virtual void PopulateGraphEdge(TEdge graphEdge)
        {
        }

        protected TEdge CreateTransitionGraphEdge(TNode fromNode, TNode toNode, TTransition transition)
        {
            var edge = _graph.AddEdge(fromNode.NodeId, toNode.NodeId);
            var transEdge = CreateGraphEdge(fromNode, toNode, edge);
            transEdge.Transition = transition;
            fromNode.AddOutEdge(transEdge);
            toNode.AddInEdge(transEdge);
            return transEdge;
        }

        protected virtual TEdge CreateGraphEdge(TNode fromNode, TNode toNode, Edge edge)
        {
            var e = new TEdge();
            e.FromNode = fromNode;
            e.ToNode = toNode;
            e.Edge = edge;
            return e;
        }

        protected abstract TState GetTransitionToState(TTransition transition);

        protected virtual string GetTransitionLabel(TTransition trans)
        {
            if (trans == null)
                return null;

            return trans.ToString();
        }

        #region helper classes

        protected class NodeEntry
        {
            public NodeEntry(int nodeId)
            {
                NodeId = nodeId;
            }

            public int NodeId { get; set; }
            public TNode Node { get; set; }
            public TState State { get; set; }
        }

        #endregion

    }

    public class MsaglGraphNode<TState, TTransition, TNode, TEdge>
        where TEdge : MsaglGraphNodeEdge<TState, TTransition, TNode, TEdge>
        where TNode : MsaglGraphNode<TState, TTransition, TNode, TEdge>
    {
        private List<TEdge> _outEdges;
        private List<TEdge> _inEdges;

        public MsaglGraphNode()
        {
            _outEdges = new List<TEdge>();
            _inEdges = new List<TEdge>();
        }

        public object StateId { get; set; }
        public TState State { get; set; }

        public string NodeId { get { return StateId.ToString(); } }
        public Node Node { get; set; }

        public IReadOnlyList<TEdge> OutEdges { get { return _outEdges; } }
        public IReadOnlyList<TEdge> InEdges { get { return _inEdges; } }

        public void AddInEdge(TEdge edge)
        {
            if (!_inEdges.Contains(edge))
                _inEdges.Add(edge);
        }

        public void AddOutEdge(TEdge edge)
        {
            if (!_outEdges.Contains(edge))
                _outEdges.Add(edge);
        }
    }

    public class MsaglGraphNodeEdge<TState, TTransition, TNode, TEdge>
        where TEdge : MsaglGraphNodeEdge<TState, TTransition, TNode, TEdge>
        where TNode : MsaglGraphNode<TState, TTransition, TNode, TEdge>
    {
        public MsaglGraphNodeEdge()
        {
        }

        public TTransition Transition { get; set; }
        public TNode FromNode { get; set; }
        public TNode ToNode { get; set; }
        public Edge Edge { get; set; }
    }

    public abstract class MsaglGraph<TState, TTransition> : MsaglGraph<TState, TTransition, MsaglGraphNode<TState, TTransition>, MsaglGraphNodeEdge<TState, TTransition>>
    {
    }

    public class MsaglGraphNode<TState, TTransition> : MsaglGraphNode<TState, TTransition, MsaglGraphNode<TState, TTransition>, MsaglGraphNodeEdge<TState, TTransition>>
    {
    }

    public class MsaglGraphNodeEdge<TState, TTransition> : MsaglGraphNodeEdge<TState, TTransition, MsaglGraphNode<TState, TTransition>, MsaglGraphNodeEdge<TState, TTransition>>
    {
    }
}
