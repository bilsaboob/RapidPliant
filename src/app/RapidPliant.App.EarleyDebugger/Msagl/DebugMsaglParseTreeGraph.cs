using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Tree;
using RapidPliant.App.Msagl;

namespace RapidPliant.App.EarleyDebugger.Msagl
{
    public class DebugMsaglParseTreeGraph : MsaglParseTreeGraph
    {
        public DebugMsaglParseTreeGraph()
        {
        }

        protected override string GetStateLabel(ITreeNode node)
        {
            var tokenNode = node as ITokenTreeNode;
            if (tokenNode != null)
            {
                return $"{tokenNode.Token.TokenType.Id}({tokenNode.Origin}, {tokenNode.Location}) = {tokenNode.Token.Value}";
            }

            var internalNode = node as IInternalTreeNode;
            if (internalNode != null && internalNode.Children != null)
            {
                return $"({internalNode.Symbol}, {internalNode.Origin}, {internalNode.Location})";
            }

            return node.ToString();
        }

        protected override string GetTransitionLabel(ITreeNode trans)
        {
            return "";
        }
    }
}
