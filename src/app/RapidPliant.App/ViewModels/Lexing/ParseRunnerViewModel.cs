using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pliant.Runtime;
using RapidPliant.Mvx;

namespace RapidPliant.App.ViewModels.Grammar
{
    public class ParseRunnerViewModel : RapidViewModel
    {
        protected IParseRunner _parseRunner;
        
        public ParseRunnerViewModel()
        {
        }

        public ParseEngineViewModel ParseEngine { get { return get(() => ParseEngine); } set { set(() => ParseEngine, value); } }

        public bool CanScanNext { get { return get(() => CanScanNext); } set { set(() => CanScanNext, value); } }
        public bool CanLexNext { get { return get(() => CanLexNext); } set { set(() => CanLexNext, value); } }

        public void ScanNext()
        {
            //Scan the next character
            if (_parseRunner.EndOfStream())
            {
                CanScanNext = false;
                CanLexNext = false;
            }
        }

        public void LexNext()
        {
            //Lex the next token
        }

        public void LoadParseRunner(IParseRunner parseRunner)
        {
            _parseRunner = parseRunner;
        }
    }
}
