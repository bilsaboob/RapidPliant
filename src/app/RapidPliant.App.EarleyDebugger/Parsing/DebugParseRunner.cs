using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Runtime;
using Pliant.Tokens;

namespace RapidPliant.App.EarleyDebugger.Parsing
{
    public class DebugParseRunner : IParseRunner
    {
        public DebugParseRunner(IParseRunner parseRunnerToDebug)
        {
            TargetParseRunner = parseRunnerToDebug;
        }
        
        public IParseRunner TargetParseRunner { get; protected set; }

        public int Position { get { return TargetParseRunner.Position; } }

        public IParseEngine ParseEngine { get { return TargetParseRunner.ParseEngine; } }

        public bool EndOfStream()
        {
            return TargetParseRunner.EndOfStream();
        }

        public IEnumerable<ILexeme> ActiveLexemes { get { return TargetParseRunner.ActiveLexemes; } }

        public bool Read()
        {
            return TargetParseRunner.Read();
        }
    }
}
