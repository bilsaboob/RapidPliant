using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using RapidPliant.App.EarleyDebugger.ViewModels;
using RapidPliant.Mvx;

namespace RapidPliant.App.EarleyDebugger.Parsing
{
    public class RunParseTask
    {
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;

        public RunParseTask(IParserViewModel parserViewModel)
        {
            ParserViewModel = parserViewModel;
        }

        public IParserViewModel ParserViewModel { get; set; }

        public bool CanContinue { get; set; }

        public int ParseNextDelayMs { get; set; }

        public void Start()
        {
            if (_task == null)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _cancellationToken = _cancellationTokenSource.Token;
                CanContinue = true;
                //Start running the parse task
                _task = Task.Factory.StartNew(() => {
                    try
                    {
                        Execute();
                    }
                    finally
                    {
                        _task = null;
                        CanContinue = false;
                    }
                });
            }
        }
    
        public void Stop()
        {
            CanContinue = false;

            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        private void Execute()
        {
            var canContinue = CanContinue;
            while (true)
            {
                DelayParseNext();

                canContinue = CanContinue;
                if (!canContinue)
                    break;

                ParserViewModel.Invoke(() => {
                    if (CanContinue)
                    {
                        var parsed = ParserViewModel.ParseNext();
                        canContinue = parsed && ParserViewModel.CanParseNext;
                    }
                });

                if (!CanContinue)
                    canContinue = false;

                DelayParseNext();
            }
        }

        private void DelayParseNext()
        {
            if (ParseNextDelayMs > 0)
            {
                Thread.Sleep(ParseNextDelayMs);
            }
        }
    }
}
