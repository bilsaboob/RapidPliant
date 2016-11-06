using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Runtime;
using RapidPliant.App.EarleyDebugger.Parsing;
using RapidPliant.App.ViewModels.Earley;
using RapidPliant.App.ViewModels.Grammar;
using RapidPliant.Mvx;

namespace RapidPliant.App.EarleyDebugger.ViewModels
{
    public class ParseEngineViewModel : RapidViewModel
    {
        public DebugParseEngine ParseEngine { get; protected set; }

        public ParseEngineViewModel()
        {
        }

        public EarleyChartViewModel EarleyChart { get { return get(() => EarleyChart); } set { set(() => EarleyChart, value); } }

        public virtual void LoadParseEngine(DebugParseEngine parseEngine)
        {
            ParseEngine = parseEngine;
        }

        public TokenViewModel LastPulsedToken { get { return get(() => LastPulsedToken); } set { set(() => LastPulsedToken, value); } }

        public bool LastPulsedTokenSuccess { get { return get(() => LastPulsedTokenSuccess); } set { set(() => LastPulsedTokenSuccess, value); } }

        public bool HasPulsedForPulsePass
        {
            get { return get(() => HasPulsedForPulsePass); }
            set
            {
                set(() => HasPulsedForPulsePass, value);
            }
        }

        public void StartNewPulsePass()
        {
            ParseEngine.StartNewPulsePass();

            HasPulsedForPulsePass = false;
            LastPulsedToken = null;
            LastPulsedTokenSuccess = false;
        }

        public void RefreshForPulsePass()
        {
            HasPulsedForPulsePass = ParseEngine.LastPulsedToken != null;
            if (HasPulsedForPulsePass)
            {
                LastPulsedToken = new TokenViewModel().LoadForToken(ParseEngine.LastPulsedToken);
                LastPulsedTokenSuccess = ParseEngine.LastPulsedTokenSuccess;
            }
        }
    }
}
