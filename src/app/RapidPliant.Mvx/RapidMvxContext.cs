using System;
using System.Collections.Generic;
using System.Linq;
using RapidPliant.Mvx.Utils;

namespace RapidPliant.Mvx
{
    /// <summary>
    /// Context for a View/ViewModel configuration
    /// </summary>
    public class RapidMvxContext
    {
        private RapidView _view;
        private RapidViewModel _viewModel;
        private List<RapidViewModel> _viewModels;
        private List<RapidMvxContext> _childContexts;

        private bool _hasInitializedForView;
        private bool _hasInitializedForViewModel;
        
        public RapidMvxContext(Type viewModelType, Type viewType)
        {
            ViewModelType = viewModelType;
            ViewType = viewType;

            _viewModels = new List<RapidViewModel>();
            _childContexts = new List<RapidMvxContext>();
        }

        /// <summary>
        /// The parent context - usually the context of the closest "parent view"
        /// </summary>
        public RapidMvxContext ParentContext { get; private set; }

        /// <summary>
        /// The child contexts of this context, usually the closest "view" children.
        /// </summary>
        public IEnumerable<RapidMvxContext> ChildContexts
        {
            get { return _childContexts; }
        }

        /// <summary>
        /// The view model type expected for this context
        /// </summary>
        public Type ViewModelType { get; private set; }

        /// <summary>
        /// Gets/Sets the view model instance for this context. Setting the view model will unload any previous view model instance and rebind the view to this new view model.
        /// </summary>
        public RapidViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                var prevViewModel = _viewModel;

                //Set the new viewmodel
                _viewModel = value;

                if (prevViewModel != null && prevViewModel.IsLoaded)
                {
                    if (prevViewModel != null)
                    {
                        //Unload previous viewmodel
                        prevViewModel.Unload();
                    }
                }

                if (!_hasInitializedForViewModel)
                    return;

                InitializedForViewModel();
            }
        }

        /// <summary>
        /// The expected view type
        /// </summary>
        public Type ViewType { get; private set; }

        /// <summary>
        /// Gets/Sets the view instance for this context. Setting the view will bind the view to the current view model and potentially also load the viewmodel if not loaded
        /// </summary>
        public RapidView View
        {
            get { return _view; }
            set
            {
                _view = value;

                if (!_hasInitializedForView)
                    return;

                InitializeForView();
            }
        }

        /// <summary>
        /// Initializes the context by initializing the view model and the view parts
        /// </summary>
        public void Initialize()
        {
            InitializedForViewModel();
            InitializeForView();
        }

        /// <summary>
        /// Set the view model and rebind with the view
        /// </summary>
        private void InitializedForViewModel()
        {
            if (_viewModel != null)
            {
                //Bind the viewmodel context to this!
                _viewModel.BindContext(this);
            }

            if (_view != null)
            {
                _view.BindViewModel(_viewModel);
            }
        }

        /// <summary>
        /// Sets the view and binds it to the associated view model and possibly loads the view model
        /// </summary>
        private void InitializeForView()
        {
            //Bind the view to the viewmodel
            if (_viewModel != null)
            {
                _viewModel.BindView(_view);
            }

            if (_view != null)
            {
                //Bind the context to this!
                _view.BindContext(this);

                //Bind viewmodel if exists
                if (_viewModel != null)
                {
                    //Make sure the viewmodel is loaded!
                    if (!_viewModel.IsLoaded)
                    {
                        _viewModel.Load();
                    }
                }
            }
        }

        /// <summary>
        /// Binds the specified parent context to this, removing this from the previous parent and adding as a new child to the new parent.
        /// </summary>
        /// <param name="parentContext"></param>
        public void BindParentContext(RapidMvxContext parentContext)
        {
            var prevParentContext = ParentContext;
            ParentContext = parentContext;

            if (prevParentContext != null)
            {
                prevParentContext.RemoveChildContext(this);
            }

            if (ParentContext != null)
            {
                ParentContext.AddChildContext(this);
            }
        }

        /// <summary>
        /// Removes the specified child context
        /// </summary>
        /// <param name="mvxContext"></param>
        private void RemoveChildContext(RapidMvxContext mvxContext)
        {
            _childContexts.Remove(mvxContext);
        }

        /// <summary>
        /// Adds the specified child context
        /// </summary>
        /// <param name="mvxContext"></param>
        private void AddChildContext(RapidMvxContext mvxContext)
        {
            if (!_childContexts.Contains(mvxContext))
                _childContexts.Add(mvxContext);
        }

        /// <summary>
        /// Gets an existing view model instance of the type expected by this context, or tries to create a new instance of that type.
        /// </summary>
        /// <returns></returns>
        public RapidViewModel GetOrCreateViewModel()
        {
            return GetOrCreateViewModel(ViewModelType);
        }

        /// <summary>
        /// Gets an existing view model instance of the specified type or creates a new one if specified. Tries to find the "best matching" view model by calculating the "interface type inheritance distance".
        /// If no view model is found in this context, then we ask the parent context to give us the view model instead.
        /// 
        /// Example, given the following types and inheritance
        /// - A : B
        /// - B : C
        /// - C : D
        /// - I : D
        /// 
        /// Given there is an existing view model of type A, and another of type B, and the requested view model type is C, then B is has the closest inheritance distance to C, and is treated as the best match.
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <param name="create"></param>
        /// <returns></returns>
        public RapidViewModel GetOrCreateViewModel(Type viewModelType, bool create = true)
        {
            RapidViewModel viewModel = null;

            var matchingViewModels = FindMatchingViewModels(viewModelType);
            if (matchingViewModels != null && matchingViewModels.Count > 0)
            {
                //We have a viewmodel 
                viewModel = ResolveBestViewModel(viewModelType, matchingViewModels);
                return viewModel;
            }

            //Try getting from the parent!
            if (ParentContext != null)
            {
                viewModel = ParentContext.GetOrCreateViewModel(viewModelType, false);
            }

            if (viewModel == null && create)
            {
                //No existing viewmodels to get - create a new one!
                viewModel = CreateViewModel(viewModelType);
                if (viewModel != null)
                {
                    AddViewModel(viewModel);
                }
            }

            return viewModel;
        }

        /// <summary>
        /// Adds the specified view model to this context
        /// </summary>
        /// <param name="viewModel"></param>
        private void AddViewModel(RapidViewModel viewModel)
        {
            _viewModels.Add(viewModel);
        }

        /// <summary>
        /// Creates an instance of the specified view model type
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <returns></returns>
        private RapidViewModel CreateViewModel(Type viewModelType)
        {
            var viewModel = Activator.CreateInstance(viewModelType);
            return viewModel as RapidViewModel;
        }

        /// <summary>
        /// Tries to resolve which of the specified view models best matches the specified view model type by calculating the "interface type inheritance distance".
        /// 
        /// Example, given the following types and inheritance
        /// - A : B
        /// - B : C
        /// - C : D
        /// - I : D
        /// 
        /// Given there is an existing view model of type A, and another of type B, and the requested view model type is C, then B is has the closest inheritance distance to C, and is treated as the best match.
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <param name="viewModels"></param>
        /// <returns></returns>
        private RapidViewModel ResolveBestViewModel(Type viewModelType, List<RapidViewModel> viewModels)
        {
            var viewModelsByRelevance = new SortedList<int, RapidViewModel>();

            foreach (var viewModel in viewModels)
            {
                var type = viewModel.GetType();
                if (type == viewModelType)
                {
                    viewModelsByRelevance.Add(0, viewModel);
                }
                else
                {
                    var typeDistance = type.GetTypeDistanceTo(viewModelType);
                    if (!viewModelsByRelevance.ContainsKey(typeDistance))
                    {
                        viewModelsByRelevance.Add(typeDistance, viewModel);
                    }
                }
            }

            //Get the first viewmodel - thats the one that matches best!
            return viewModelsByRelevance.Values.FirstOrDefault();
        }

        /// <summary>
        /// Gives a list of view model instances from this context that can be assigned to the specified view model type.
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <returns></returns>
        private List<RapidViewModel> FindMatchingViewModels(Type viewModelType)
        {
            if (_viewModels.Count == 0)
                return null;

            var matchingViewModels = new List<RapidViewModel>();

            foreach (var viewModel in _viewModels)
            {
                var type = viewModel.GetType();
                if (viewModelType.IsAssignableFrom(type))
                {
                    matchingViewModels.Add(viewModel);
                }
            }

            return matchingViewModels;
        }
    }
}