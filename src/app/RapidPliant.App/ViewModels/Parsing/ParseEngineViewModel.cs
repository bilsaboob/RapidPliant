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
    public class ParseEngineViewModel : RapidViewModel
    {
        public IParseEngine ParseEngine { get; protected set; }
        
        public ParseEngineViewModel()
        {
        }

        public EarleyChartViewModel EarleyChart { get { return get(() => EarleyChart); } set { set(() => EarleyChart, value); } }

        public void LoadParseEngine(IParseEngine parseEngine)
        {
            ParseEngine = parseEngine;
        }
    }
}
