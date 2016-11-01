using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace RapidPliant.Mvx.Binding
{
    public class BindingExpressionConverter : ExpressionConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(MarkupExtension))
                return true;

            return false;
        }
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(MarkupExtension))
            {
                var bindingExpression = value as BindingExpression;
                if (bindingExpression == null)
                    throw new Exception("Invalid binding expression!");

                var converter = (bindingExpression.ParentBinding.Source as RapidBindingPropertyValueProviderConverter);
                return converter.RapidBinding.CloneForBindingExpressionMarkupSerializationObject();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
