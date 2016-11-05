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
        private static readonly MemberEntry _nullMemberEntry = new NullPropertyEntry();

        /// <summary>
        /// The properties contained within this view model
        /// </summary>
        private Dictionary<string, MemberEntry> _memberEntries;
        
        public RapidViewModel()
        {
            _memberEntries = new Dictionary<string, MemberEntry>();
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
            var prop = GetOrCreateMemberEntry(memberExpression);
            prop.SetValueAndNotify(this, value);
        }

        internal void set(string memberName, object value)
        {
            var prop = GetOrCreateMemberEntry(memberName);
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
            var prop = GetOrCreateMemberEntry(memberExpression);
            return prop.GetValue<TValue>();
        }

        internal object get(string memberName)
        {
            var prop = GetOrCreateMemberEntry(memberName);
            return prop.GetValue<object>();
        }
        #endregion

        #region helpers
        private MemberEntry GetOrCreateMemberEntry<TValue>(Expression<Func<TValue>> memberExpression)
        {
            var memberPath = this.GetMemberInfoPath(memberExpression);
            if (memberPath == null || memberPath.MemberInfos.Count == 0)
                return _nullMemberEntry;

            return GetOrCreateMemberEntry(memberPath);
        }

        private MemberEntry GetOrCreateMemberEntry(string memberName)
        {
            var member = this.GetType().GetMember(memberName).FirstOrDefault();
            if (member == null)
                return _nullMemberEntry;
            
            return GetOrCreateMemberEntry(new MemberInfoPath(new [] {member}));
        }

        private MemberEntry GetOrCreateMemberEntry(MemberInfoPath memberPath)
        {
            var fullPathName = memberPath.FullPathName;
            MemberEntry memberEntry;
            if (!_memberEntries.TryGetValue(fullPathName, out memberEntry))
            {
                memberEntry = CreateMemberEntry(memberPath);
                _memberEntries[fullPathName] = memberEntry;
            }
            return memberEntry;
        }

        private MemberEntry CreateMemberEntry(MemberInfoPath memberPath)
        {
            
            if (memberPath.MemberInfos.Count == 1)
            {
                //It's a simple member path entry
                return new MemberEntry(memberPath);
            }
            else
            {
                //For sub entries we need to listen for any path changes!
                var memberEntry = new SubMemberEntry(memberPath);
                var pathChangeListener = new MemberPathChangeListener(this, memberPath, memberEntry);
                memberEntry.PathChangeListener = pathChangeListener;
                pathChangeListener.StartListening();
                return memberEntry;
            }
        }

        protected T GetOrCreate<T>()
        {
            return default(T);
        }

        class MemberEntry
        {
            public MemberEntry(MemberInfoPath memberPath)
            {
                if (memberPath != null)
                {
                    MemberPath = memberPath;
                    Member = new QualifiedMember(MemberPath);
                }
            }

            public QualifiedMember Member { get; private set; }
            public MemberInfoPath MemberPath { get; private set; }

            public virtual string Name { get { return Member.Path.FullPathName; } }
            public virtual object Value { get; protected set; }

            public MemberPathChangeListener PathChangeListener { get; set; }

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

        class SubMemberEntry : MemberEntry
        {
            public SubMemberEntry(MemberInfoPath memberPath)
                : base(memberPath)
            {
            }

            public override void SetValueAndNotify(RapidViewModel source, object value)
            {
                if (source == null)
                    return;

                Value = value;
                
                //Try setting the value at the end of the path
                Member.SetValueForRoot(source, value);

                //Notify about the first path name...
                source.OnPropertyChanged(MemberPath.FullPathName);
            }
        }

        class MemberPathChangeListener
        {
            private List<MemberChangeListener> MemberChangeListeners { get; set; }
            private List<MemberInfo> RemainingMemberInfosToInitialize { get; set; }
            private MemberEntry MemberEntry { get; set; }
            private RapidViewModel Root { get; set; }

            public MemberPathChangeListener(RapidViewModel root, MemberInfoPath memberPath, MemberEntry memberEntry)
            {
                Root = root;
                MemberEntry = memberEntry;

                MemberChangeListeners = new List<MemberChangeListener>();

                RemainingMemberInfosToInitialize = memberPath.MemberInfos.ToList();

                InitializeMemberInfos(root);
            }

            public void EnsureMemberInfosInitialized(MemberChangeListener fromMemberChangeListener, object target)
            {
                fromMemberChangeListener.PathChangeListenerPendingInitialize = null;

                if (RemainingMemberInfosToInitialize.Count == 0)
                    return;

                InitializeMemberInfos(target);
            }
            
            private void InitializeMemberInfos(object target)
            {
                var remainingMemberInfos = RemainingMemberInfosToInitialize.ToList();

                MemberChangeListener prevListener = null;
                while (remainingMemberInfos.Count > 0)
                {
                    var memberInfo = remainingMemberInfos[0];
                    remainingMemberInfos.RemoveAt(0);

                    var memberChangeListener = new MemberChangeListener(Root, target, memberInfo, MemberEntry);
                    memberChangeListener.PathChangeListenerPendingInitialize = this;
                    MemberChangeListeners.Add(memberChangeListener);

                    if (prevListener != null)
                        prevListener.NextMemberListener = memberChangeListener;

                    //Get the new target
                    target = QualifiedMember.GetValue(memberInfo, target);
                    if (target == null)
                        break;

                    prevListener = memberChangeListener;
                }

                RemainingMemberInfosToInitialize = remainingMemberInfos.ToList();
            }

            public void StartListening()
            {
                //Start listening for changes... in reversed order
                var listenersReversed = MemberChangeListeners.ToList();
                listenersReversed.Reverse();
                foreach (var memberChangeListener in listenersReversed)
                {
                    memberChangeListener.StartListening();
                }
            }

            public void StopListening()
            {
                foreach (var memberChangeListener in MemberChangeListeners)
                {
                    memberChangeListener.StopListening();
                }
            }
        }

        class MemberChangeListener
        {
            public bool IsListening { get; private set; }
            public RapidViewModel Root { get; set; }
            public object Target { get; private set; }
            public MemberInfo MemberInfo { get; private set; }
            
            public object MemberValue { get; private set; }
            public object MemberEntryValue { get; private set; }

            public MemberEntry MemberEntry { get; private set; }

            public MemberChangeListener(RapidViewModel root, object target, MemberInfo memberInfo, MemberEntry memberEntry)
            {
                Root = root;
                Target = target;
                MemberInfo = memberInfo;
                MemberEntry = memberEntry;

                MemberValue = QualifiedMember.GetValue(MemberInfo, Target);
                MemberEntryValue = memberEntry.Member.GetValueForRoot(Root);
            }

            public MemberChangeListener NextMemberListener { get; set; }
            public MemberPathChangeListener PathChangeListenerPendingInitialize { get; set; }



            public void ResetForTarget(object newTarget)
            {
                StopListening();
                Target = newTarget;
                StartListening();                
            }

            public void StartListening()
            {
                StartListening_(Target);
            }

            private void StartListening_(object target)
            {
                if(IsListening)
                    return;

                var notifyChange = target as INotifyPropertyChanged;
                if (notifyChange != null)
                {
                    IsListening = true;
                    notifyChange.PropertyChanged += MemberChanged;
                }
            }

            public void StopListening()
            {
                StopListening_(Target);
            }

            private void StopListening_(object target)
            {
                var notifyChange = target as INotifyPropertyChanged;
                if (notifyChange != null)
                {
                    notifyChange.PropertyChanged -= MemberChanged;
                }

                IsListening = false;
            }

            private void MemberChanged(object sender, PropertyChangedEventArgs e)
            {
                if(!string.Equals(e.PropertyName, MemberInfo.Name))
                    return;

                //The member changed
                var memberValue = QualifiedMember.GetValue(MemberInfo, sender);
                if (MemberValue == null && memberValue != null)
                {
                    if(PathChangeListenerPendingInitialize != null)
                        PathChangeListenerPendingInitialize.EnsureMemberInfosInitialized(this, memberValue);
                }

                if (!ValueEquals(MemberValue, memberValue))
                {
                    MemberValue = memberValue;
                    
                    if (NextMemberListener != null)
                        NextMemberListener.ResetForTarget(memberValue);
                }
            }

            private bool ValueEquals(object v1, object v2)
            {
                if (v1 == null && v2 == null)
                    return true;

                if (ReferenceEquals(v1, v2))
                    return true;

                if (v1 != null)
                {
                    return v1.Equals(v2);
                }

                if (v2 != null)
                {
                    return v2.Equals(v1);
                }

                return false;
            }
        }

        class NullPropertyEntry : MemberEntry
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
