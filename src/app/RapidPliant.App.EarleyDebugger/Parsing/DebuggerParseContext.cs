using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Charts;
using Pliant.Runtime;
using Pliant.Tokens;
using RapidPliant.App.ViewModels.Earley;

namespace RapidPliant.App.EarleyDebugger.Parsing
{
    public class DebuggerParseContext : ParseContext
    {
        protected EarleyChartViewModel EarleyChart { get; set; }

        public DebuggerParseContext(EarleyChartViewModel earleyChart)
        {
            EarleyChart = earleyChart;
        }
    
        public override void ReadCharacter(int position, char character)
        {
            base.ReadCharacter(position, character);
        }

        public override void Started(int origin, IState startState)
        {
            var set = EarleyChart.GetEarleySet(origin);

            base.Started(origin, startState);
        }

        public override void Scanned(int origin, IState scanState, IState nextState, IToken scannedToken)
        {
            base.Scanned(origin, scanState, nextState, scannedToken);
        }

        public override void Predicted(PredictionMode mode, int origin, IState predictState, IState nextState)
        {
            base.Predicted(mode, origin, predictState, nextState);
        }

        public override void Completed(CompletionMode mode, int origin, IState completedState, IState nextState)
        {
            base.Completed(mode, origin, completedState, nextState);
        }

        public override void Transitioned(int origin, ITransitionState transitionState)
        {
            base.Transitioned(origin, transitionState);
        }
    }
}
