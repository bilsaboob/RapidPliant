using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace RapidPliant.Mvx
{
    public interface IRapidView
    {
        Type ViewModelType { get; }
        RapidViewModel ViewModel { get; }

        MvxContext Context { get; }
        void BindContext(MvxContext context);
    }

    public interface IRapidView<TViewModel> : IRapidView
    {
    }

    public class RapidView : UserControl, IRapidView
    {
        private List<DependencyObject> _relatedObjects;

        public RapidView()
        {
            _relatedObjects = new List<DependencyObject>();
        }

        public void AddRelatedObject(DependencyObject depObj)
        {
            if (!_relatedObjects.Contains(depObj))
                _relatedObjects.Add(depObj);
        }

        private Type _viewModelType;
        public virtual Type ViewModelType
        {
            get
            {
                if (_viewModelType == null)
                {
                    _viewModelType = RapidMvx.GetViewModelTypeForView(GetType());
                }
                return _viewModelType;
            }
        }

        public bool HasViewModel { get; protected set; }
        public RapidViewModel ViewModel { get; protected set; }

        public MvxContext Context { get; private set; }

        public void BindContext(MvxContext context)
        {
            if (Context == context)
                return;

            Context = context;
            RapidViewModel viewModel = null;

            if (Context != null)
            {
                viewModel = Context.ViewModel;
            }

            BindViewModel(viewModel);
        }

        public void BindViewModel(RapidViewModel viewModel)
        {
            ViewModel = viewModel;
            HasViewModel = viewModel != null;

            //Set the datacontext!
            DataContext = ViewModel;
        }
    }
}
