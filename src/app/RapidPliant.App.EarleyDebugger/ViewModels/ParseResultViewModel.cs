using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Msagl.Drawing;
using Pliant.Automata;
using Pliant.Charts;
using Pliant.Forest;
using Pliant.Tree;
using RapidPliant.App.EarleyDebugger.Msagl;
using RapidPliant.App.Msagl;
using RapidPliant.App.ViewModels.Msagl;
using RapidPliant.Automata;
using RapidPliant.Mvx;

namespace RapidPliant.App.EarleyDebugger.ViewModels
{
    public class ParseResultViewModel : RapidViewModel
    {
        public ParseResultViewModel()
        {
        }

        public DebugMsaglParseTreeGraphViewModel ParseGraph
        {
            get { return get(() => ParseGraph); }
            set { set(() => ParseGraph, value); }
        }

        public void LoadParseResult(IInternalForestNode parseRoot)
        {
            ParseGraph.LoadParseTree(parseRoot);
        }

        public void LoadParseForest(IReadOnlyChart earleyChart)
        {
            ParseGraph.LoadParseForest(earleyChart);
        }

        public void Reset()
        {
            ParseGraph.ResetGraphs();
        }
    }
}
