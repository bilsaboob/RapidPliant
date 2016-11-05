using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using RapidPliant.Mvx.Utils;

namespace RapidPliant.Mvx
{
    public interface IRapidViewModel : INotifyPropertyChanged
    {
        void Invoke(Action action);
    }

    public class RapidViewModel : IRapidViewModel
    {
        private static readonly PropertyEntry _nullPropertyEntry = new NullPropertyEntry();

        /// <summary>
        /// The properties contained within this view model
        /// </summary>
        private Dictionary<string, PropertyEntry> _propertyEntries;
        
        public RapidViewModel()
        {
            _propertyEntries = new Dictionary<string, PropertyEntry>();
        }

        /// <summary>
        /// The mvx context associated with this view, holds dependency information regarding the expected viewmodel / view combination
        /// </summary>
        public RapidMvxContext Context { get; private set; }

        /// <summary>
        /// Specifies wether the view model has been loaded or not
        /// </summary>
        public bool IsLoaded { get; protected set; }

        /// <summary>
        /// Specifies wether a view has been bound to the view model or not
        /// </summary>
        protected bool HasView { get; private set; }

        /// <summary>
        /// The view that is bound to the view model, only a single view can be bound at a time
        /// </summary>
        protected RapidView View { get; private set; }

        /// <summary>
        /// Loads the data for the view model, allows view models handle loading logic themselves
        /// </summary>
        public void Load()
        {
            IsLoaded = true;
            LoadData();
        }

        /// <summary>
        /// Loads / Instantiates any required "sub viewmodels" - allows view model to do custom instantiation beside any other "by convention" automagic loading
        /// </summary>
        public virtual void LoadViewModels()
        {
        }

        /// <summary>
        /// Override to load the data, is triggered during the Load phase - usually soon after the view model is created and before any view is displayed
        /// </summary>
        protected virtual void LoadData()
        {
        }

        /// <summary>
        /// Override to unload data, is triggered during Unload phase - when the view model goes "out of context", such as when the the view is "unbound"
        /// </summary>
        public virtual void Unload()
        {
        }

        /// <summary>
        /// Binds the specified mvx context, binding the view from the context to this view model
        /// </summary>
        /// <param name="context"></param>
        public void BindContext(RapidMvxContext context)
        {
            if (Context == context)
                return;

            Context = context;
            RapidView view = null;

            if (Context != null)
            {
                view = Context.View;
            }

            BindView(view);
        }

        /// <summary>
        /// Binds the specified view to this view model
        /// </summary>
        /// <param name="view"></param>
        public void BindView(RapidView view)
        {
            View = view;
            HasView = view != null;
        }

        public void Invoke(Action action)
        {
            //Dispatch using the dispatcher of the view
            Dispatcher dispatcher = null;
            var uiElem = View as UIElement;
            if (uiElem != null && uiElem.Dispatcher != null)
            {
                dispatcher = uiElem.Dispatcher;
            }
            else
            {
                dispatcher = Dispatcher.CurrentDispatcher;
            }

            dispatcher.Invoke(action);
        }

        #region INotifyPropertyChange
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region View model property get/set helpers
        /// <summary>
        /// Sets the value for the property and trigges PropertyChanged event
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="memberExpression"></param>
        /// <param name="value"></param>
        protected void set<TValue>(Expression<Func<TValue>> memberExpression, TValue value)
        {
            var prop = GetOrCreateProperyEntry(memberExpression);
            prop.SetValueAndNotify(this, value);
        }

        /// <summary>
        /// Gets the value of the specified property.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="memberExpression"></param>
        /// <returns></returns>
        protected TValue get<TValue>(Expression<Func<TValue>> memberExpression)
        {
            var prop = GetOrCreateProperyEntry(memberExpression);
            return prop.GetValue<TValue>();
        }
        #endregion

        #region helpers
        private PropertyEntry GetOrCreateProperyEntry<TValue>(Expression<Func<TValue>> memberExpression)
        {
            var propInfo = this.GetPropertyInfo(memberExpression);
            if (propInfo == null)
                return _nullPropertyEntry;

            PropertyEntry propEntry;
            if (!_propertyEntries.TryGetValue(propInfo.Name, out propEntry))
            {
                propEntry = new PropertyEntry(propInfo);
                _propertyEntries[propInfo.Name] = propEntry;
            }
            return propEntry;
        }

        protected T GetOrCreate<T>()
        {
            return default(T);
        }

        class PropertyEntry
        {
            public PropertyEntry(PropertyInfo propInfo)
            {
                PropertyInfo = propInfo;
            }

            public PropertyInfo PropertyInfo { get; set; }

            public virtual string Name { get { return PropertyInfo.Name; } }
            public virtual object Value { get; protected set; }

            public virtual void SetValueAndNotify(RapidViewModel source, object value)
            {
                if (source == null)
                    return;

                Value = value;

                source.OnPropertyChanged(Name);
            }

            public virtual TValue GetValue<TValue>()
            {
                if (Value == null)
                {
                    return default(TValue);
                }

                return (TValue)Value;
            }
        }

        class NullPropertyEntry : PropertyEntry
        {
            public NullPropertyEntry() : base(null)
            {
            }

            public override string Name { get { return null; } }

            public override object Value { get { return null; } protected set { } }
        }

        #endregion
    }
}
