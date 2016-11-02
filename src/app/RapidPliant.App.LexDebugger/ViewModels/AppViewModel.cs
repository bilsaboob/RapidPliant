using System.Collections.ObjectModel;
using RapidPliant.App.Utils;
using RapidPliant.Mvx;

namespace RapidPliant.App.LexDebugger.ViewModels
{
    public class AppViewModel : RapidViewModel
    {
        public ReusableAlphabetNamesCollection PatternNames { get; set; }

        public ReusableNamesCollection.NameEntry NewLexPatternNameEntry { get; set; }

        public AppViewModel()
        {
            PatternNames = new ReusableAlphabetNamesCollection();
            LexPatterns = new ObservableCollection<LexPatternViewModel>();

            NewLexPatternNameEntry = PatternNames.AquireNextName();
            NewLexPatternName = NewLexPatternNameEntry.Name;
        }

        public LexMsaglNfaGraphViewModel LexNfaGraph
        {
            get { return get(() => LexNfaGraph); }
            set { set(()=> LexNfaGraph, value);}
        }

        public ObservableCollection<LexPatternViewModel> LexPatterns
        {
            get { return get(() => LexPatterns); }
            set { set(()=>LexPatterns, value); }
        }

        public string NewLexPatternName
        {
            get { return get(() => NewLexPatternName); }
            set { set(()=>NewLexPatternName, value); }
        }

        public string NewLexPatternPattern
        {
            get { return get(() => NewLexPatternPattern); }
            set { set(() => NewLexPatternPattern, value); }
        }

        public void AddPattern()
        {
            //Check if we can get the specified available name?
            PatternNames.ReleaseName(NewLexPatternNameEntry);
            NewLexPatternNameEntry = PatternNames.AquireNextName(NewLexPatternName);

            var newLexPattern = new LexPatternViewModel(NewLexPatternName, NewLexPatternPattern, NewLexPatternNameEntry);
            LexPatterns.Add(newLexPattern);

            NewLexPatternName = "";
            NewLexPatternPattern = "";

            //Get the next available name now
            if (NewLexPatternNameEntry != null)
                NewLexPatternNameEntry = PatternNames.AquireNextName();

            if (NewLexPatternNameEntry != null)
                NewLexPatternName = NewLexPatternNameEntry.Name;

            RefreshGraphs();
        }
        
        public void RemovePattern(LexPatternViewModel lexPattern)
        {
            LexPatterns.Remove(lexPattern);

            var nameEntry = lexPattern.NameEntry;
            if (nameEntry != null)
                PatternNames.ReleaseName(nameEntry);
        }

        public void RefreshPattern(LexPatternViewModel lexPattern)
        {
            RefreshGraphs();
        }

        private void RefreshGraphs()
        {
            LexNfaGraph.RefreshLexPatterns(LexPatterns);
        }
    }
}
