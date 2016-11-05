using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Pliant.Charts;
using Pliant.Forest;
using Pliant.Grammars;
using RapidPliant.Mvx;

namespace RapidPliant.App.ViewModels.Earley
{
    public class EarleyStateViewModel : RapidViewModel
    {
        public IState State { get; protected set; }

        public EarleyStateViewModel()
        {
        }

        public EarleyStateViewModel LoadFromState(IState state)
        {
            State = state;
            if (State == null)
                return this;

            Position = State.Position;

            IsComplete = State.IsComplete;

            StateType = State.StateType;

            Production = new ProductionViewModel().LoadFromProduction(State.Production);

            PreDotSymbol = new SymbolViewModel().LoadFromSymbol(State.PreDotSymbol);

            PostDotSymbol = new SymbolViewModel().LoadFromSymbol(State.PostDotSymbol);

            DisplayLabel = State.ToString();

            return this;
        }

        public string DisplayLabel { get { return get(() => DisplayLabel); } set { set(() => DisplayLabel, value); } }

        public int Position { get { return get(() => Position); } set { set(() => Position, value); } }

        public bool IsComplete { get { return get(() => IsComplete); } set { set(() => IsComplete, value); } }

        public StateType StateType { get { return get(() => StateType); } set { set(() => StateType, value); } }

        public ProductionViewModel Production { get { return get(() => Production); } set { set(() => Production, value); } }

        public SymbolViewModel PreDotSymbol { get { return get(() => PreDotSymbol); } set { set(() => PreDotSymbol, value); } }

        public SymbolViewModel PostDotSymbol { get { return get(() => PostDotSymbol); } set { set(() => PostDotSymbol, value); } }
        
    }
}
