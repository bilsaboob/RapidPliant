using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.Mvx;

namespace RapidPliant.App.EarleyDebugger.ViewModels
{
    public interface IParserViewModel : IRapidViewModel
    {
        bool CanParseNext { get; }

        bool ParseNext();
    }
}
