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
        
        protected override int GetStateId(IDfaState state)
        {
            throw new NotImplementedException();
            //return state.Id;
        }

        protected override IEnumerable<IDfaTransition> GetStateTransitions(IDfaState state)
        {
            return state.Transitions;
        }

        protected override IDfaState GetTransitionToState(IDfaTransition transition)
        {
            return transition.Target;
        }

        protected override string GetStateLabel(IDfaState state)
        {
            throw new NotImplementedException();
            //return state.Id.ToString();
        }
    }
}
