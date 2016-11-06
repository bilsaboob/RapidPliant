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
}
