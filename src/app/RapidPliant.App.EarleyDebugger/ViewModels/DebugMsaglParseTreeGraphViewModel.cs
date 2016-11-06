using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Msagl.Drawing;
using Pliant.Forest;
using Pliant.Tree;
using RapidPliant.App.Msagl;
using RapidPliant.App.ViewModels.Msagl;

namespace RapidPliant.App.EarleyDebugger.Msagl
{
    public class DebugMsaglParseTreeGraphViewModel : MsaglParseTreeGraphViewModel
    {
        public DebugMsaglParseTreeGraphViewModel()
        {
        }

        public Graph Graph
        {
            get { return get(() => Graph); }
            set { set(() => Graph, value); }
        }

        public void LoadForParseForest(IInternalForestNode parseRoot)
        {
            if(parseRoot == null)
                return;

            //Build and set the graph
            var graph = BuildParseTreeGraph(parseRoot);
            Graph = graph;
        }

        private Graph BuildParseTreeGraph(IInternalForestNode parseRoot)
        {
            var parseTreeEnumerable = new ParseTreeEnumerable(parseRoot);
            var allTreeNodes = parseTreeEnumerable.ToList();

            var parseTreeGraph = new DebugMsaglParseTreeGraph();
            parseTreeGraph.Build(allTreeNodes);
            return parseTreeGraph.Graph;
        }
    }
}
