using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Msagl.Drawing;
using Pliant.Automata;
using Pliant.RegularExpressions;
using RapidPliant.App.LexDebugger.Msagl;
using RapidPliant.App.ViewModels;
using RapidPliant.Automata;

namespace RapidPliant.App.LexDebugger.ViewModels
{
    public class LexMsaglNfaDfaGraphViewModel : MsaglNfaGraphViewModel
    {
        protected RegexParser RegexParser { get; set; }
        protected IRegexToNfa RegexToNfa { get; set; }
        protected INfaToDfa NfaToDfa { get; set; }

        public LexMsaglNfaDfaGraphViewModel()
        {
            RegexParser = new RegexParser();
            RegexToNfa = new ThompsonConstructionAlgorithm();
            NfaToDfa = new SubsetConstructionAlgorithm();
        }

        public Graph NfaGraph
        {
            get { return get(() => NfaGraph); }
            set { set(() => NfaGraph, value); }
        }

        public Graph DfaGraph
        {
            get { return get(() => DfaGraph); }
            set { set(() => DfaGraph, value); }
        }

        public ObservableCollection<LexPatternViewModel> LexPatterns
        {
            get { return get(() => LexPatterns); }
            set { set(() => LexPatterns, value); }
        }

        public void RefreshLexPatterns(IEnumerable<LexPatternViewModel> lexPatterns)
        {
            LexPatterns = new ObservableCollection<LexPatternViewModel>(lexPatterns);

            var patternsNfa = CreateMergedNfa(lexPatterns);
            NfaGraph = BuildNfaGraph(patternsNfa);

            var patternsDfa = CreateDfa(patternsNfa);
            DfaGraph = BuildDfaGraph(patternsDfa);
        }
        
        private Graph BuildNfaGraph(INfa patternsNfa)
        {
            var nfaGraph = new LexMsaglNfaGraph();
            nfaGraph.Build(patternsNfa.GetAllStates());
            return nfaGraph.Graph;
        }

        private Graph BuildDfaGraph(IDfaState patternsDfa)
        {
            var dfaGraph = new LexMsaglDfaGraph();
            dfaGraph.Build(patternsDfa.GetAllStates());
            return dfaGraph.Graph;
        }

        private IDfaState CreateDfa(INfa nfa)
        {
            return NfaToDfa.Transform(nfa);
        }

        private INfa CreateMergedNfa(IEnumerable<LexPatternViewModel> lexPatterns)
        {
            var patternNfas = new List<INfa>();

            foreach (var lexPattern in lexPatterns)
            {
                var regex = ParseRegEx(lexPattern.Pattern);
                if (regex == null)
                    continue;

                var regexNfa = CreateNfaForRegEx(regex);
                if (regexNfa == null)
                    continue;

                patternNfas.Add(regexNfa);
            }

            return patternNfas.UnionAll();
        }

        protected Regex ParseRegEx(string pattern)
        {
            return RegexParser.Parse(pattern);
        }

        private INfa CreateNfaForRegEx(Regex regex)
        {
            return RegexToNfa.Transform(regex);
        }
    }
}
