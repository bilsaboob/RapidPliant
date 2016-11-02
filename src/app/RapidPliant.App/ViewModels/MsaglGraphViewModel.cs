using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Msagl.Drawing;
using RapidPliant.Mvx;
using RapidPliant.Mvx.Controls.Extensions;

namespace RapidPliant.App.ViewModels
{
    public class MsaglGraphViewModel : RapidViewModel
    {
        public MsaglGraphViewModel()
        {
        }

        public Graph Graph
        {
            get { return get(() => Graph); }
            set { set(() => Graph, value); }
        }
    }
}
