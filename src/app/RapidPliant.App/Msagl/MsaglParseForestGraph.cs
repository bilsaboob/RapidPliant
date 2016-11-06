using System.Collections.Generic;
using System.Linq;
using Pliant.Forest;

namespace RapidPliant.App.Msagl
{
    public class MsaglParseForestGraph : MsaglGraph<IForestNode, IForestNode>
    {
        public MsaglParseForestGraph()
        {
        }

        protected override IEnumerable<IForestNode> GetStateTransitions(IForestNode node)
        {
            var tokenNode = node as ITokenForestNode;
            if (tokenNode != null)
            {
                return new [] { tokenNode };
            }

            var tmpInternalNode = node as InternalForestAndNode;
            if (tmpInternalNode != null)
            {
                return tmpInternalNode.Children.ToList();
            }

            var internalNode = node as IInternalForestNode;
            if (internalNode != null && internalNode.Children != null)
            {
                return internalNode.Children.Select(n => new InternalForestAndNode(node, n)).ToList();
            }
            
            return null;
        }

        protected override IForestNode GetTransitionToState(IForestNode transition)
        {
            return transition;
        }

        protected override string GetStateLabel(IForestNode node)
        {
            return node.ToString();
        }

        protected override string GetTransitionLabel(IForestNode trans)
        {
            return "";
        }
    }
}