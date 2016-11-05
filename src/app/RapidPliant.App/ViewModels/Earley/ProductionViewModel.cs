using System.Collections.ObjectModel;
using System.Linq;
using Pliant.Grammars;
using RapidPliant.Mvx;

namespace RapidPliant.App.ViewModels.Earley
{
    public class ProductionViewModel : RapidViewModel
    {
        public IProduction Production { get; protected set; }

        public ProductionViewModel()
        {
            RightHandSide = new ObservableCollection<SymbolViewModel>();
        }

        public ProductionViewModel LoadFromProduction(IProduction production)
        {
            Production = production;
            if (Production == null)
                return this;

            LeftHandSideName = Production.LeftHandSide.Name;
            RightHandSide = new ObservableCollection<SymbolViewModel>(Production.RightHandSide.Select(symbol => new SymbolViewModel().LoadFromSymbol(symbol)));
            DisplayLabel = Production.ToString();

            return this;
        }

        public string DisplayLabel { get { return get(() => DisplayLabel); } set { set(() => DisplayLabel, value); } }

        public string LeftHandSideName { get { return get(() => LeftHandSideName); } set { set(() => LeftHandSideName, value); } }

        public ObservableCollection<SymbolViewModel> RightHandSide { get { return get(() => RightHandSide); } set { set(()=>RightHandSide, value);} }
    }
}