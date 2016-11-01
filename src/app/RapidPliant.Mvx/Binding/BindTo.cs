using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RapidPliant.Mvx.Binding
{
    public class BindTo : BindingMarkupExtensionBase
    {
        public BindTo()
        {
        }

        public BindTo(string path)
        {
            Path = new PropertyPath(path);
        }

        public override object ProvideValue(IServiceProvider provider)
        {
            RapidBindingDelegateBase bindingDelegateBase = null;

            FrameworkElement targetFrameworkElement;
            DependencyProperty targetDepProp;
            if (bindingDelegateBase == null && TryGetTargetItems(provider, out targetFrameworkElement, out targetDepProp))
            {
                //This is a normal property binding... so just return that!
                bindingDelegateBase = new RapidBindingPropertyDelegate(this, targetFrameworkElement, targetDepProp);
            }

            MethodInfo targetDepPropMethod;
            EventInfo targetDepPropEvent;
            if (bindingDelegateBase == null && TryGetTargetItems(provider, out targetFrameworkElement, out targetDepPropEvent))
            {
                bindingDelegateBase = new RapidBindingEventDelegate(this, targetFrameworkElement, targetDepPropEvent);
            }

            if (bindingDelegateBase == null && TryGetTargetItems(provider, out targetFrameworkElement, out targetDepPropMethod))
            {
                bindingDelegateBase = new RapidBindingMethodActionDelegate(this, targetFrameworkElement, targetDepPropMethod);
            }

            if (bindingDelegateBase != null)
            {
                bindingDelegateBase.EnsureSerialization();

                return bindingDelegateBase.ProvideValue(provider);
            }

            throw new Exception("Not supported property type for RapidBinding!");
        }

        public string ToSerializationString(RapidBindingDelegateBase bindingDelegate)
        {
            var sb = new StringBuilder();
            sb.Append("{BindTo");

            if (Binding.Path != null)
            {
                sb.AppendFormat(" Path={0}", Binding.Path.Path);
            }

            sb.Append("}");
            return sb.ToString();
        }

        public BindTo CloneForBindingExpressionMarkupSerializationObject()
        {
            var b = new BindTo();

            b.Path = ActualPath;
            b.Converter = ActualConverter;
            b.ConverterCulture = ConverterCulture;
            b.ConverterParameter = ConverterParameter;
            b.Mode = Mode;
            b.ElementName = ElementName;
            b.BindsDirectlyToSource = BindsDirectlyToSource;
            b.NotifyOnSourceUpdated = NotifyOnSourceUpdated;
            b.NotifyOnTargetUpdated = NotifyOnTargetUpdated;
            b.NotifyOnValidationError = NotifyOnValidationError;
            b.IsAsync = IsAsync;

            return b;
        }
    }
}
