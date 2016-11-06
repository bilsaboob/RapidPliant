using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Charts;
using Pliant.Forest;
using Pliant.Tree;

namespace RapidPliant.App.Msagl
{
    public class MsaglParseTreeGraph : MsaglGraph<ITreeNode, ITreeNode>
    {
        public MsaglParseTreeGraph()
        {
        }

        protected override IEnumerable<ITreeNode> GetStateTransitions(ITreeNode node)
        {
            var tokenNode = node as ITokenTreeNode;
            if (tokenNode != null)
            {
                return new ITreeNode[] {tokenNode};
            }

            var internalNode = node as IInternalTreeNode;
            if (internalNode != null && internalNode.Children != null)
            {
                return internalNode.Children.ToList();
            }
            
            return null;
        }

        protected override ITreeNode GetTransitionToState(ITreeNode transition)
        {
            return transition;
        }

        protected override string GetStateLabel(ITreeNode node)
        {
            return node.ToString();
        }

        protected override string GetTransitionLabel(ITreeNode trans)
        {
            return "";
        }
    }

    public class InternalForestAndNode : IForestNode
    {
        public InternalForestAndNode(IForestNode sourceNode, IAndForestNode andForestNode)
        {
            SourceNode = sourceNode;
            AndNode = andForestNode;

            Origin = sourceNode.Origin;
            Location = sourceNode.Location;
            NodeType = ForestNodeType.Intermediate;

            Children = andForestNode.Children.ToList();
        }

        public IForestNode SourceNode { get; set; }
        public IAndForestNode AndNode { get; set; }

        public int Origin { get; private set; }
        public int Location { get; private set; }
        public ForestNodeType NodeType { get; private set; }

        public IReadOnlyList<IForestNode> Children { get; private set; }

        public void Accept(IForestNodeVisitor visitor)
        {
        }

        public override string ToString()
        {
            return SourceNode.ToString();
        }
    }
}
