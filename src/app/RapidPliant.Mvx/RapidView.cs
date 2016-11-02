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

        RapidMvxContext Context { get; }
        void BindContext(RapidMvxContext context);
    }

    public interface IRapidView<TViewModel> : IRapidView
    {
    }

    public class RapidView : UserControl, IRapidView
    {
        private Type _viewModelType;

        public RapidView()
        {
        }

        /// <summary>
        /// The mvx context associated with this view, holds dependency information regarding the expected viewmodel / view combination
        /// </summary>
        public RapidMvxContext Context { get; private set; }

        /// <summary>
        /// The view model type expected for this view
        /// </summary>
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
        
        /// <summary>
        /// Specified wether the view has a view model instance or not, accessing the ViewModel property may resolve the actual view model instance.
        /// </summary>
        public bool HasViewModel { get; protected set; }

        /// <summary>
        /// The view model instance
        /// </summary>
        public RapidViewModel ViewModel { get; protected set; }
        
        /// <summary>
        /// Binds the specified mvx context to this view, configuring the view model from the context as the view mdoel for this view
        /// </summary>
        /// <param name="context"></param>
        public void BindContext(RapidMvxContext context)
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

        /// <summary>
        /// Binds the specified view model to this view, sets the view model as the "DataContext" 
        /// </summary>
        /// <param name="viewModel"></param>
        public void BindViewModel(RapidViewModel viewModel)
        {
            ViewModel = viewModel;
            HasViewModel = viewModel != null;

            //Set the datacontext!
            DataContext = ViewModel;
        }
    }
}
