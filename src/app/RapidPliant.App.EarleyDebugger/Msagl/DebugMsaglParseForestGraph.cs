using Pliant.Forest;
using Pliant.Tree;
using RapidPliant.App.Msagl;

namespace RapidPliant.App.EarleyDebugger.Msagl
{
    public class DebugMsaglParseForestGraph : MsaglParseForestGraph
    {
        public DebugMsaglParseForestGraph()
        {
        }

        protected override string GetStateLabel(IForestNode node)
        {
            var tokenNode = node as ITokenForestNode;
            if (tokenNode != null)
            {
                return $"T:{tokenNode.Token.TokenType.Id}({tokenNode.Origin}, {tokenNode.Location}) = {tokenNode.Token.Value}";
            }

            var symbolNode = node as SymbolForestNode;
            if (symbolNode != null)
            {
                return $"S:({symbolNode.Symbol}, {symbolNode.Origin}, {symbolNode.Location})";
            }

            var virtNode = node as VirtualForestNode;
            if (virtNode != null)
            {
                return $"V:({virtNode.Symbol}, {virtNode.Origin}, {virtNode.Location})";
            }

            var interNode = node as IntermediateForestNode;
            if (interNode != null)
            {
                return $"(IM:{interNode.NodeType}, {interNode.Origin}, {interNode.Location})";
            }

            var internalNode = node as IInternalForestNode;
            if (internalNode != null)
            {
                return $"I:({internalNode.NodeType}, {internalNode.Origin}, {internalNode.Location})";
            }

            return node.ToString();
        }

        protected override string GetTransitionLabel(IForestNode trans)
        {
            return "";
        }
    }
}