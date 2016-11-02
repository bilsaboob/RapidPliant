using Pliant.Automata;
using RapidPliant.App.ViewModels;

namespace RapidPliant.App.LexDebugger.Msagl
{
    public class LexMsaglDfaGraph : MsaglDfaGraph
    {
        public LexMsaglDfaGraph()
        {
        }

        protected override string GetTransitionLabel(IDfaTransition transition)
        {
            return transition.Terminal.ToString();
        }

        protected override bool IsFinalState(IDfaState state)
        {
            if (state.IsFinal)
                return true;

            return base.IsFinalState(state);
        }
    }
}