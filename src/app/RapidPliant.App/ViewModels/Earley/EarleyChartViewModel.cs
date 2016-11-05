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

            return this;
        }

        public ObservableCollection<EarleySetViewModel> EarleySets { get { return get(() => EarleySets); } set { set(() => EarleySets, value); } }

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
            var count = EarleyChart.Count;
            for (var i = EarleySets.Count - 1; i < count; ++i)
            {
                var earleySet = EarleyChart.EarleySets[i];
                EarleySets.Add(new EarleySetViewModel().LoadFromEarleySet(earleySet));
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
