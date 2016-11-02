using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RapidPliant.App.Utils
{
    public abstract class ReusableNamesCollection
    {
        public ReusableNamesCollection()
        {
            AvailableNames = new List<NameEntry>();
        }
        
        protected List<NameEntry> AvailableNames { get; set; }

        protected void AddAvailableName(string name)
        {
            AvailableNames.Add(new NameEntry(AvailableNames.Count, name));
        }

        public NameEntry AquireNextName(string name = null)
        {
            NameEntry nameEntry = null;
            if (name != null)
            {
                nameEntry = AvailableNames.FirstOrDefault(n => !n.IsAquired && string.Equals(n.Name, name, StringComparison.CurrentCultureIgnoreCase));
            }
            else
            {
                nameEntry = AvailableNames.FirstOrDefault(n => !n.IsAquired);
            }
            
            if (nameEntry == null)
                return null;

            nameEntry.IsAquired = true;

            return nameEntry;
        }

        public void ReleaseName(NameEntry nameEntry)
        {
            if(nameEntry == null)
                return;

            nameEntry.IsAquired = false;
        }
        
        public class NameEntry
        {
            public NameEntry(int index, string name)
            {
                Name = name;
            }

            internal int Index { get; set; }

            internal bool IsAquired { get; set; }

            public string Name { get; protected set; }
        }
    }

    public class ReusableAlphabetNamesCollection : ReusableNamesCollection
    {
        public ReusableAlphabetNamesCollection()
        {
            InitAvailableAlphabetNames();
        }

        private void InitAvailableAlphabetNames()
        {
            for (var i = 'A'; i < 'Z'; ++i)
            {
                AddAvailableName(i.ToString());
            }
        }
    }
}
