using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Automata;
using Pliant.RegularExpressions;
using RapidPliant.App.LexDebugger.Msagl;
using RapidPliant.App.ViewModels;
using RapidPliant.Automata;

namespace RapidPliant.App.LexDebugger.ViewModels
{
    public class LexMsaglNfaGraphViewModel : MsaglNfaGraphViewModel
    {
        protected RegexParser RegexParser { get; set; }
        protected IRegexToNfa RegexToNfa { get; set; }

        public LexMsaglNfaGraphViewModel()
        {
            RegexParser = new RegexParser();
            RegexToNfa = new ThompsonConstructionAlgorithm();
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
            
            var nfaGraph = new LexMsaglNfaGraph();
            nfaGraph.Build(patternsNfa.GetAllStates());

            Graph = nfaGraph.Graph;
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
