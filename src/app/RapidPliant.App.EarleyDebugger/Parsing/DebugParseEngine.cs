using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Forest;
using Pliant.Grammars;
using Pliant.Runtime;
using Pliant.Tokens;

namespace RapidPliant.App.EarleyDebugger.Parsing
{
    public class DebugParseEngine : IParseEngine
    {
        public DebugParseEngine(IParseEngine parseEngineToDebug)
        {
            TargetParseEngine = parseEngineToDebug;
        }

        public IParseEngine TargetParseEngine { get; protected set; }

        public IToken LastPulsedToken { get; protected set; }

        public bool LastPulsedTokenSuccess { get; set; }

        public void StartNewPulsePass()
        {
            LastPulsedToken = null;
            LastPulsedTokenSuccess = false;
        }

        #region IParseEngine
        public IGrammar Grammar { get { return TargetParseEngine.Grammar; } }

        public int Location { get { return TargetParseEngine.Location; } }
        
        public void Reset()
        {
            TargetParseEngine.Reset();
        }

        public bool Pulse(IToken token)
        {
            LastPulsedToken = token;
            var result = TargetParseEngine.Pulse(token);
            LastPulsedTokenSuccess = result;
            return result;
        }

        public List<ILexerRule> GetExpectedLexerRules()
        {
            return TargetParseEngine.GetExpectedLexerRules();
        }

        public bool IsAccepted()
        {
            return TargetParseEngine.IsAccepted();
        }

        public IInternalForestNode GetParseForestRootNode()
        {
            return TargetParseEngine.GetParseForestRootNode();
        }
        #endregion
    }
}
