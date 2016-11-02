using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Automata;
using Pliant.Collections;

namespace RapidPliant.Automata
{
    public static class DfaExtensions
    {
        public static IEnumerable<IDfaState> GetAllStates(this IDfaState fromDfaState)
        {
            var visitedStates = new ProcessOnceQueue<IDfaState>();
            CollectDfaStates(fromDfaState, visitedStates);
            return visitedStates;
        }
        
        private static void CollectDfaStates(IDfaState fromState, ProcessOnceQueue<IDfaState> visitedStates)
        {
            if (!visitedStates.Enqueue(fromState))
                return;

            foreach (var transition in fromState.Transitions)
            {
                CollectDfaStates(transition.Target, visitedStates);
            }
        }
    }
}
