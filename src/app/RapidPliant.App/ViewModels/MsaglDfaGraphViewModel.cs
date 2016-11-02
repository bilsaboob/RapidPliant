using System;
using System.Collections.Generic;
using Microsoft.Msagl.Drawing;
using Pliant.Automata;
using RapidPliant.App.Msagl;

namespace RapidPliant.App.ViewModels
{
    public abstract class MsaglDfaGraphViewModel : MsaglGraphModel<IDfaState, IDfaTransition>
    {
        public MsaglDfaGraphViewModel()
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
