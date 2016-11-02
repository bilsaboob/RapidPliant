using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using RapidPliant.Mvx.Utils;

namespace RapidPliant.Mvx
{
    public static class RapidMvx
    {
        public static void Init()
        {
        }

        /// <summary>
        /// Loads/Initializes the specified control/view.
        /// * Builds the mvx context hierarchy by traversing the child views of the control.
        /// * Resolves the expected view models of the generated mvx context hierarchy
        /// * Creates instances of the view model properties of view models that are resolved
        /// </summary>
        /// <param name="viewControl"></param>
        public static void LoadView(Control viewControl)
        {
            var view = viewControl as IRapidView;
            if (view == null)
                return;

            //Build the mvx contexts!
            var rootContext = BuildMvxContextRecursive(viewControl, null);

            ResolveViewModelsRecursive(rootContext);

            InitializeContextsRecursive(rootContext);
        }

        /// <summary>
        /// Initialize the mvx context, indirectly initializing the view / view model combination
        /// </summary>
        /// <param name="context"></param>
        private static void InitializeContextsRecursive(RapidMvxContext context)
        {
            if (context == null)
                return;

            context.Initialize();

            foreach (var childContext in context.ChildContexts)
            {
                InitializeContextsRecursive(childContext);
            }
        }

        /// <summary>
        /// Builds the mvx context hierarchy recursively for the specified view control
        /// </summary>
        /// <param name="viewControl"></param>
        /// <param name="parentContext"></param>
        /// <returns></returns>
        private static RapidMvxContext BuildMvxContextRecursive(DependencyObject viewControl, RapidMvxContext parentContext)
        {
            if (viewControl == null)
                return parentContext;

            var returnContext = parentContext;

            //Build the context for the current view
            RapidMvxContext context = null;
            var view = viewControl as IRapidView;
            if (view != null)
            {
                context = view.Context;

                if (context == null)
                {
                    context = CreateContextForView(view);
                    context.BindParentContext(parentContext);
                    view.BindContext(context);
                    context.View = (RapidView)view;
                }

                if (returnContext == null)
                {
                    returnContext = context;
                }
            }

            //Build contexts for the children!
            var children = viewControl.GetAllChildren();
            if (children != null)
            {
                foreach (var child in children)
                {
                    var childParentContext = context;
                    if (childParentContext == null)
                    {
                        childParentContext = parentContext;
                    }

                    var childContext = BuildMvxContextRecursive(child, childParentContext);
                    if (returnContext == null)
                    {
                        returnContext = childContext;
                    }
                }
            }

            return returnContext;
        }

        /// <summary>
        /// Resolves the view models of the specified mvx context hierarchy, creating instances for the view model properties of any resolved view model.
        /// </summary>
        /// <param name="context"></param>
        private static void ResolveViewModelsRecursive(RapidMvxContext context)
        {
            if (context == null)
                return;

            var viewModel = context.ViewModel;
            if (viewModel == null)
            {
                viewModel = context.GetOrCreateViewModel();
                context.ViewModel = viewModel;
            }

            if (viewModel != null)
            {
                viewModel.LoadViewModels();
                ResolveViewModelMembers(context, viewModel);
            }

            foreach (var childContext in context.ChildContexts)
            {
                ResolveViewModelsRecursive(childContext);
            }
        }

        /// <summary>
        /// Resolves the view model properties of the specified view model, by querying the context for an appropriate view model
        /// </summary>
        /// <param name="context"></param>
        /// <param name="viewModel"></param>
        private static void ResolveViewModelMembers(RapidMvxContext context, RapidViewModel viewModel)
        {
            var properties = viewModel.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToList();
            var viewModelProperties = properties.Where(p => typeof(RapidViewModel).IsAssignableFrom(p.PropertyType)).ToList();
            foreach (var viewModelProperty in viewModelProperties)
            {
                var val = viewModelProperty.GetValue(viewModel);
                if (val == null)
                {
                    val = context.GetOrCreateViewModel(viewModelProperty.PropertyType);
                    if (val != null)
                    {
                        viewModelProperty.SetValue(viewModel, val);
                    }
                }
            }
        }

        /// <summary>
        /// Creates an mvx context for the specifed view
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        private static RapidMvxContext CreateContextForView(IRapidView view)
        {
            var viewModelType = view.ViewModelType;
            var viewType = view.GetType();

            var context = new RapidMvxContext(viewModelType, viewType);
            return context;
        }

        /// <summary>
        /// Evaluates the specified view type, trying to resolve the expected view model type for the generic IRapidView<TViewModel> interface
        /// </summary>
        /// <param name="viewType"></param>
        /// <returns></returns>
        public static Type GetViewModelTypeForView(Type viewType)
        {
            var viewModelType = viewType.GetInterfaces().Where(t => {
                if (!t.IsGenericType)
                    return false;
                if (t.GetGenericTypeDefinition() != typeof(IRapidView<>))
                    return false;
                if (t.GenericTypeArguments.Length == 0)
                    return false;
                if (!typeof(RapidViewModel).IsAssignableFrom(t.GenericTypeArguments[0]))
                    return false;
                return true;
            }).Select(t => t.GenericTypeArguments[0]).FirstOrDefault();
            return viewModelType;
        }

        /// <summary>
        /// Creates a view model of expected type for the specified view type
        /// </summary>
        /// <param name="viewType"></param>
        /// <returns></returns>
        public static RapidViewModel CreateViewModelForView(Type viewType)
        {
            var viewModelType = GetViewModelTypeForView(viewType);

            if (viewModelType == null)
                return null;

            return GetOrCreateViewModel(viewModelType);
        }

        /// <summary>
        /// Gets an existing view model of the specified type or creates/resolves a new instance.
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <returns></returns>
        public static RapidViewModel GetOrCreateViewModel(Type viewModelType)
        {
            var viewModel = Activator.CreateInstance(viewModelType);
            if (viewModel == null)
                return null;

            return (RapidViewModel)viewModel;
        }
    }
}
