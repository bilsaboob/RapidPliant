using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RapidPliant.App.Utils;
using RapidPliant.Mvx;

namespace RapidPliant.App.LexDebugger.ViewModels
{
    public class LexPatternViewModel : RapidViewModel
    {
        public ReusableNamesCollection.NameEntry NameEntry { get; set; }

        public LexPatternViewModel(string name, string pattern, ReusableNamesCollection.NameEntry nameEntry)
        {
            Name = name;
            Pattern = pattern;
            NameEntry = nameEntry;
        }

        public string Name
        {
            get { return get(() => Name); }
            set { set(()=>Name, value); }
        }

        public string Pattern
        {
            get { return get(() => Pattern); }
            set { set(()=>Pattern, value); }
        }
    }
}
