using System;
using System.Collections.Generic;
using Microsoft.Msagl.Drawing;
using Pliant.Automata;
using RapidPliant.App.Msagl;

namespace RapidPliant.App.ViewModels
{
    public class MsaglDfaGraph : MsaglGraph<IDfaState, IDfaTransition>
    {
        public MsaglDfaGraph()
        {
        }
        
        protected override IEnumerable<IDfaTransition> GetStateTransitions(IDfaState state)
        {
            return state.Transitions;
        }

        protected override IDfaState GetTransitionToState(IDfaTransition transition)
        {
            return transition.Target;
        }

        protected override bool IsFinalState(IDfaState state)
        {
            var isFinal = base.IsFinalState(state);
            if (isFinal)
                return true;

            return state.IsFinal;
        }
    }
}
