using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Runtime;
using RapidPliant.App.ViewModels.Earley;
using RapidPliant.Mvx;

namespace RapidPliant.App.ViewModels.Grammar
{
    public class ParseRunnerViewModel : RapidViewModel
    {
        protected IParseRunner _parseRunner;
        
        public ParseRunnerViewModel()
        {
        }
        
        public ParseEngineViewModel ParseEngine { get { return get(() => ParseEngine); } set { set(() => ParseEngine, value); } }

        public EarleyChartViewModel EarleyChart { get { return get(() => EarleyChart); } set { set(() => EarleyChart, value); } }

        public bool CanScanNext { get { return get(() => CanScanNext); } set { set(() => CanScanNext, value); } }
        public bool CanLexNext { get { return get(() => CanLexNext); } set { set(() => CanLexNext, value); } }

        public void ScanNext()
        {
        }

        public void LexNext()
        {
            if (_parseRunner.EndOfStream())
            {
                CanScanNext = false;
                CanLexNext = false;
                return;
            }
            
            var readResult = _parseRunner.Read();
            
            if (!readResult)
            {
                CanScanNext = false;
                CanLexNext = false;
            }

            EarleyChart.RefreshFromChart();
        }

        public void LoadParseRunner(IParseRunner parseRunner)
        {
            _parseRunner = parseRunner;
        }
    }
}
