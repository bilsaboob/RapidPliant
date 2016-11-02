using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Grammars;
using RapidPliant.App.Services;
using RapidPliant.Mvx;

namespace RapidPliant.App.ViewModels.Grammar
{
    public class GrammarViewModel : RapidViewModel
    {
        public IGrammar Grammar { get; protected set; }
        
        public GrammarViewModel()
        {
        }

        public string Name { get { return get(() => Name); } set {set(()=>Name, value);} }
        public string Description { get { return get(() => Description); } set { set(() => Description, value); } }
        public Type GrammarType { get { return get(() => GrammarType); } set { set(() => GrammarType, value); } }

        public void LoadGrammar(IGrammar grammar)
        {
            Grammar = grammar;
        }

        private void LoadGrammarInfo(GrammarInfo grammarInfo)
        {
            Name = grammarInfo.Name;
            Description = grammarInfo.Description;
            GrammarType = grammarInfo.GrammarType;
        }

        public static GrammarViewModel CreateFromGrammarInfo(GrammarInfo grammarInfo)
        {
            var g = new GrammarViewModel();
            g.LoadGrammarInfo(grammarInfo);
            return g;
        }
    }
}
