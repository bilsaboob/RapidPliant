using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Charts;
using RapidPliant.Mvx;

namespace RapidPliant.App.ViewModels.Earley
{
    public class EarleySetViewModel : RapidViewModel
    {
        public IEarleySet EarleySet { get; protected set; }

        public EarleySetViewModel()
        {
            Predictions = new ObservableCollection<EarleyStateViewModel>();
            Scans = new ObservableCollection<EarleyStateViewModel>();
            Completions = new ObservableCollection<EarleyStateViewModel>();
            Transitions = new ObservableCollection<EarleyStateViewModel>();
        }

        public EarleySetViewModel LoadFromEarleySet(IEarleySet set)
        {
            EarleySet = set;
            if (EarleySet == null)
                return this;

            Location = EarleySet.Location;

            Scans = new ObservableCollection<EarleyStateViewModel>(EarleySet.Scans.Select(state => new EarleyStateViewModel().LoadFromState(state)));
            Predictions = new ObservableCollection<EarleyStateViewModel>(EarleySet.Predictions.Select(state => new EarleyStateViewModel().LoadFromState(state)));
            Completions = new ObservableCollection<EarleyStateViewModel>(EarleySet.Completions.Select(state => new EarleyStateViewModel().LoadFromState(state)));
            Transitions = new ObservableCollection<EarleyStateViewModel>(EarleySet.Transitions.Select(state => new EarleyStateViewModel().LoadFromState(state)));

            return this;
        }

        public int Location { get { return get(() => Location); } set { set(() => Location, value); } }

        public ObservableCollection<EarleyStateViewModel> Predictions { get { return get(() => Predictions); } set { set(() => Predictions, value); } }
        public ObservableCollection<EarleyStateViewModel> Scans { get { return get(() => Scans); } set { set(() => Scans, value); } }
        public ObservableCollection<EarleyStateViewModel> Completions { get { return get(() => Completions); } set { set(() => Completions, value); } }
        public ObservableCollection<EarleyStateViewModel> Transitions { get { return get(() => Transitions); } set { set(() => Transitions, value); } }

        public void AddScan(EarleyStateViewModel state)
        {
            Scans.Add(state);
        }

        public void AddPrediction(EarleyStateViewModel state)
        {
            Predictions.Add(state);
        }

        public void AddCompletion(EarleyStateViewModel state)
        {
            Completions.Add(state);
        }

        public void AddTransition(EarleyStateViewModel state)
        {
            Transitions.Add(state);
        }
    }
}
