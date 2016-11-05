using Pliant.Grammars;
using RapidPliant.Mvx;

namespace RapidPliant.App.ViewModels.Earley
{
    public class SymbolViewModel : RapidViewModel
    {
        public ISymbol Symbol { get; protected  set; }

        public SymbolViewModel()
        {
        }

        public SymbolViewModel LoadFromSymbol(ISymbol symbol)
        {
            Symbol = symbol;
            if (Symbol == null)
                return this;

            SymbolType = Symbol.SymbolType.ToString();
            DisplayLabel = Symbol.ToString();

            return this;
        }

        public string DisplayLabel { get { return get(() => DisplayLabel); } set { set(() => DisplayLabel, value); } }

        public string SymbolType { get { return get(() => SymbolType); } set { set(() => SymbolType, value); } }
    }
}