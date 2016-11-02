using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Automata;
using RapidPliant.App.ViewModels;

namespace RapidPliant.App.LexDebugger.Msagl
{
    public class LexMsaglNfaGraph : MsaglNfaGraph
    {
        public LexMsaglNfaGraph()
        {
        }

        protected override string GetTransitionLabel(INfaTransition transition)
        {
            var nullTransition = transition as NullNfaTransition;
            if (nullTransition != null)
            {
                return "";
            }

            var termTransition = transition as TerminalNfaTransition;
            if (termTransition != null)
            {
                return termTransition.Terminal.ToString();
            }

            return "<???>";
        }
    }
}
