using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RapidPliant.Mvx.Utils;

namespace RapidPliant.Mvx.Binding
{
    public static class RapidBindingHelpers
    {
        public static object FindDataContexts(FrameworkElement frameworkElem, PathIterator path, out object rootDataContext, out object thisDataContext)
        {
            rootDataContext = null;
            thisDataContext = null;
            object target = null;

            var parentWithDataContext = frameworkElem.FindParentWithDataContext();
            if (parentWithDataContext != null && parentWithDataContext.DataContext != null)
            {
                thisDataContext = parentWithDataContext.DataContext;
                rootDataContext = thisDataContext;
                target = thisDataContext;
            }

            if (path.MoveNext())
            {
                if (path.Current.Name == "root")
                {
                    var view = frameworkElem.FindParent<RapidView>();
                    rootDataContext = view.ViewModel;
                    target = rootDataContext;
                }
            }

            return target;
        }
    }
}
