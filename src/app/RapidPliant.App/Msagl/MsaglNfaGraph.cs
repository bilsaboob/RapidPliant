using System;
using System.Collections.Generic;
using Microsoft.Msagl.Drawing;
using Pliant.Automata;
using RapidPliant.App.Msagl;

namespace RapidPliant.App.ViewModels
{
    public class MsaglNfaGraph : MsaglGraph<INfaState, INfaTransition>
    {
        public MsaglNfaGraph()
        {
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
            return base.GetStateLabel(state);
        }
    }
}
