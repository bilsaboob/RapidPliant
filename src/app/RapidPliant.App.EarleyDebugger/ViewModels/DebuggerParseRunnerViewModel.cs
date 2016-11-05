using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Runtime;
using RapidPliant.App.EarleyDebugger.Parsing;
using RapidPliant.App.ViewModels.Earley;
using RapidPliant.App.ViewModels.Grammar;

namespace RapidPliant.App.EarleyDebugger.ViewModels
{
    public class DebuggerParseRunnerViewModel : ParseRunnerViewModel
    {
        public DebuggerParseRunnerViewModel()
        {
        }

        protected override ParseContext CreateParseContext()
        {
            return new DebuggerParseContext(EarleyChart);
        }
    }
}
