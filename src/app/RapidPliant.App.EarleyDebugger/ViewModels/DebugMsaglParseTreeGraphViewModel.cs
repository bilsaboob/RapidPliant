using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Msagl.Drawing;
using Pliant.Charts;
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

        public Graph ParseForestGraph
        {
            get { return get(() => ParseForestGraph); }
            set { set(() => ParseForestGraph, value); }
        }

        public Graph ParseTreeGraph
        {
            get { return get(() => ParseTreeGraph); }
            set { set(() => ParseTreeGraph, value); }
        }

        public void LoadParseTree(IInternalForestNode parseRoot)
        {
            if(parseRoot == null)
                return;

            //Build and set the graph
            var graph = BuildParseTreeGraphForParseRoot(parseRoot);
            ParseTreeGraph = graph;
        }

        public void LoadParseForest(IReadOnlyChart earleyChart)
        {
            if (earleyChart == null)
                return;

            //Build and set the graph
            var graph = BuildParseForestGraphForChart(earleyChart);
            ParseForestGraph = graph;
        }
        
        public Graph BuildParseForestGraphForChart(IReadOnlyChart earleyChart)
        {
            var allParseForestNodes = new HashSet<IForestNode>();

            List<IForestNode> parseNodes;
            foreach (var earleySet in earleyChart.EarleySets)
            {
                parseNodes = earleySet.Scans.Select(c => c.ParseNode).Where(n => n != null).ToList();
                foreach (var parseNode in parseNodes)
                    allParseForestNodes.Add(parseNode);

                parseNodes = earleySet.Predictions.Select(c => c.ParseNode).Where(n => n != null).ToList();
                foreach (var parseNode in parseNodes)
                    allParseForestNodes.Add(parseNode);

                parseNodes = earleySet.Completions.Select(c => c.ParseNode).Where(n => n != null).ToList();
                foreach (var parseNode in parseNodes)
                    allParseForestNodes.Add(parseNode);
                
                parseNodes = earleySet.Transitions.Select(c => c.ParseNode).Where(n => n != null).ToList();
                foreach (var parseNode in parseNodes)
                    allParseForestNodes.Add(parseNode);
            }

            var parseForestGraph = new DebugMsaglParseForestGraph();
            parseForestGraph.Build(allParseForestNodes.ToList());

            return parseForestGraph.Graph;
        }

        private Graph BuildParseTreeGraphForParseRoot(IInternalForestNode parseRoot)
        {
            var parseTreeEnumerable = new ParseTreeEnumerable(parseRoot);
            var allTreeNodes = parseTreeEnumerable.ToList();

            var parseTreeGraph = new DebugMsaglParseTreeGraph();
            parseTreeGraph.Build(allTreeNodes);
            return parseTreeGraph.Graph;
        }

        public void ResetGraphs()
        {
            ParseForestGraph = null;
            ParseTreeGraph = null;
        }
    }
}
