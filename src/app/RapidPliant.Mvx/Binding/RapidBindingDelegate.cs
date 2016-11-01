using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RapidPliant.Mvx.Binding
{
    public abstract class RapidBindingDelegateBase
    {
        public RapidBindingDelegateBase(BindTo binding, FrameworkElement frameworkElement)
        {
            FrameworkElement = frameworkElement;
            Binding = binding;
        }

        public FrameworkElement FrameworkElement { get; protected set; }

        public BindTo Binding { get; protected set; }

        public string BoundMemberName { get; protected set; }

        public virtual void EnsureSerialization()
        {
            RapidBindingSerializationProperty.SetRapidBinding(FrameworkElement, BoundMemberName + "=\"" + Binding.ToSerializationString(this) + "\"");
        }

        public abstract object ProvideValue(IServiceProvider provider);
    }

    public abstract class RapidBindingDelegate : RapidBindingDelegateBase
    {
        public RapidBindingDelegate(BindTo binding, Type delegateType, FrameworkElement frameworkElement)
            : base(binding, frameworkElement)
        {
            DelegateType = delegateType;
            EventArgsType = DelegateType.GetMethod("Invoke").GetParameters()[1].ParameterType;
            Delegate = Delegate.CreateDelegate(delegateType, this, "OnEvent");
        }

        public Type EventArgsType { get; protected set; }

        public Type DelegateType { get; protected set; }

        public Delegate Delegate { get; protected set; }

        protected virtual void OnEvent(object sender, RoutedEventArgs args)
        {
            var frameworkElem = sender as FrameworkElement;
            CallMethodForPath(frameworkElem);
        }

        private void CallMethodForPath(FrameworkElement frameworkElement)
        {
            new ActionMethodWithPath(frameworkElement, Binding.Path.Path).Invoke();
        }

        public override object ProvideValue(IServiceProvider provider)
        {
            return Delegate;
        }
    }

    public class RapidBindingMethodActionDelegate : RapidBindingDelegate
    {
        public RapidBindingMethodActionDelegate(BindTo rapidBinding, FrameworkElement frameworkElement, MethodInfo methodInfo)
            : base(rapidBinding, methodInfo.GetParameters()[1].ParameterType, frameworkElement)
        {
            MethodInfo = methodInfo;
            BoundMemberName = MethodInfo.Name;
        }

        public MethodInfo MethodInfo { get; protected set; }
    }

    public class RapidBindingEventDelegate : RapidBindingDelegate
    {
        public RapidBindingEventDelegate(BindTo rapidBinding, FrameworkElement frameworkElement, EventInfo eventInfo)
            : base(rapidBinding, eventInfo.EventHandlerType, frameworkElement)
        {
            EventInfo = eventInfo;
            BoundMemberName = EventInfo.Name;
        }

        public EventInfo EventInfo { get; protected set; }
    }

    public class RapidBindingPropertyDelegate : RapidBindingDelegateBase
    {
        private RapidBindingPropertyValueProviderConverter _converter;

        public RapidBindingPropertyDelegate(BindTo binding, FrameworkElement frameworkElement, DependencyProperty dependencyProperty)
            : base(binding, frameworkElement)
        {
            DependencyProperty = dependencyProperty;
            BoundMemberName = DependencyProperty.Name;

            SetupBinding();
        }

        public DependencyProperty DependencyProperty { get; protected set; }

        private void SetupBinding()
        {
            _converter = new RapidBindingPropertyValueProviderConverter(this);
        }

        public override object ProvideValue(IServiceProvider provider)
        {
            _converter.SetupBinding();
            return _converter.ProvideValue(provider);
        }

        public override void EnsureSerialization()
        {
            //Don't serialize... we use the bindingexpression converter!
        }
    }
}
