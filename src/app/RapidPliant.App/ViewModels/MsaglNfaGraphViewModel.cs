using System;
using System.Collections.Generic;
using Microsoft.Msagl.Drawing;
using Pliant.Automata;
using RapidPliant.App.Msagl;

namespace RapidPliant.App.ViewModels
{
    public abstract class MsaglNfaGraphViewModel : MsaglGraphModel<INfaState, INfaTransition>
    {
        public MsaglNfaGraphViewModel()
        {
        }
        
        protected override int GetStateId(INfaState state)
        {
            throw new NotImplementedException();
            //return state.Id;
        }

        protected override IEnumerable<INfaTransition> GetStateTransitions(INfaState state)
        {
            return state.Transitions;
        }

        protected override INfaState GetTransitionToState(INfaTransition transition)
        {
            return transition.Target;
        }

        protected override string GetStateLabel(INfaState state)
        {
            throw new NotImplementedException();
            //return state.Id.ToString();
        }
    }
}
