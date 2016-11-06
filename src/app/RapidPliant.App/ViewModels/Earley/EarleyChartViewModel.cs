using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Pliant.Charts;
using RapidPliant.Mvx;

namespace RapidPliant.App.ViewModels.Earley
{
    public class EarleyChartViewModel : RapidViewModel
    {
        public IReadOnlyChart EarleyChart { get; protected set; }

        public EarleyChartViewModel()
        {
            EarleySets = new ObservableCollection<EarleySetViewModel>();
        }

        public EarleyChartViewModel LoadFromChart(IReadOnlyChart chart)
        {
            EarleyChart = chart;

            EarleySets = new ObservableCollection<EarleySetViewModel>(EarleyChart.EarleySets.Select(set=>new EarleySetViewModel().LoadFromEarleySet(set)));

            RefreshActiveSet();

            return this;
        }
        
        public ObservableCollection<EarleySetViewModel> EarleySets { get { return get(() => EarleySets); } set { set(() => EarleySets, value); } }

        public EarleySetViewModel ActiveEarleySet { get { return get(() => ActiveEarleySet); } set { set(() => ActiveEarleySet, value); } }

        public bool ActiveEarleySetChanged { get { return get(() => ActiveEarleySetChanged); } set { set(() => ActiveEarleySetChanged, value); } }

        public EarleySetViewModel GetEarleySet(int location)
        {
            while (location >= EarleySets.Count)
            {
                var set = CreateEarleySet(location);
                EarleySets.Add(set);
            }

            return EarleySets[location];
        }

        public EarleySetViewModel GetLastEarleySet()
        {
            return EarleySets.LastOrDefault();
        }

        public void RefreshFromChart()
        {
            //Add any new earley sets!
            if (EarleyChart != null)
            {
                var count = EarleyChart.Count;
                for (var i = EarleySets.Count; i < count; ++i)
                {
                    var earleySet = EarleyChart.EarleySets[i];
                    var earleySetVm = new EarleySetViewModel().LoadFromEarleySet(earleySet);
                    EarleySets.Add(earleySetVm);

                    RefreshActiveSet();
                }
            }
        }

        private void RefreshActiveSet()
        {
            ActiveEarleySetChanged = false;

            var newActive = EarleySets.LastOrDefault();
            if(newActive == null)
                return;
            
            if (newActive != ActiveEarleySet)
            {
                if(ActiveEarleySet != null)
                    ActiveEarleySet.IsCurrent = false;

                ActiveEarleySet = newActive;

                if (ActiveEarleySet != null)
                {
                    ActiveEarleySet.IsCurrent = true;
                    ActiveEarleySetChanged = true;
                }
            }
        }

        #region helpers
        protected EarleySetViewModel CreateEarleySet(int location)
        {
            var set = new EarleySetViewModel();
            set.Location = location;

            return set;
        }
        #endregion
        
    }
}
