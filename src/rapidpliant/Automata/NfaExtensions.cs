using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Automata;
using Pliant.Collections;

namespace RapidPliant.Automata
{
    public static class NfaExtensions
    {
        public static INfa UnionAll(this IEnumerable<INfa> nfas)
        {
            var start = new NfaState();
            var end = new NfaState();

            foreach (var nfa in nfas)
            {
                start.AddTransistion(new NullNfaTransition(nfa.Start));
                nfa.End.AddTransistion(new NullNfaTransition(end));
            }

            return new Nfa(start, end);
        }

        public static IEnumerable<INfaState> GetAllStates(this INfa nfa)
        {
            return nfa.Start.GetAllStates();
        }

        public static IEnumerable<INfaState> GetAllStates(this INfaState fromState)
        {
            var visitedStates = new ProcessOnceQueue<INfaState>();
            CollectNfaStates(fromState, visitedStates);
            return visitedStates;
        }

        private static void CollectNfaStates(INfaState fromState, ProcessOnceQueue<INfaState> visitedStates)
        {
            if(!visitedStates.Enqueue(fromState))
                return;

            foreach (var transition in fromState.Transitions)
            {
                CollectNfaStates(transition.Target, visitedStates);
            }
        }
    }
}
