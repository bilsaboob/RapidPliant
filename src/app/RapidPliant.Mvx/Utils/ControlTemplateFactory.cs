using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace RapidPliant.Mvx.Utils
{
    public class CustomFactory : FrameworkElementFactory
    {
        
    }

    public static class ControlTemplateFactory
    {
        public static DataTemplate CreateDataTemplate(TemplateFactoryControlDelegate factory)
        {
            if (factory == null)
                throw new ArgumentNullException("factory");

            var frameworkElementFactory = new FrameworkElementFactory(typeof(TemplateFactoryControl));
            frameworkElementFactory.SetValue(TemplateFactoryControl.FactoryProperty, factory);

            var dataTemplate = new DataTemplate(typeof(DependencyObject));
            dataTemplate.VisualTree = frameworkElementFactory;
            return dataTemplate;
        }
        
        public static ControlTemplate CreateControlTemplate(Type controlType, TemplateFactoryControlDelegate factory)
        {
            if (controlType == null)
                throw new ArgumentNullException("controlType");

            if (factory == null)
                throw new ArgumentNullException("factory");

            var frameworkElementFactory = new FrameworkElementFactory(typeof(TemplateFactoryControl));
            frameworkElementFactory.SetValue(TemplateFactoryControl.FactoryProperty, factory);

            var controlTemplate = new ControlTemplate(controlType);
            controlTemplate.VisualTree = frameworkElementFactory;
            return controlTemplate;
        }
    }

    public delegate object TemplateFactoryControlDelegate();

    public class TemplateFactoryControl : ContentControl
    {
        /*internal static readonly DependencyProperty CustomContentProperty = DependencyProperty.Register("CustomContent", typeof(object), typeof(TemplateFactoryControl), new FrameworkPropertyMetadata(null, _CustomContentChanged));
        private static void _CustomContentChanged(DependencyObject instance, DependencyPropertyChangedEventArgs args)
        {
            var control = (TemplateFactoryControl)instance;
            var content = args.NewValue;
            control.CustomContent = content;
        }*/

        internal static readonly DependencyProperty FactoryProperty = DependencyProperty.Register("Factory", typeof(TemplateFactoryControlDelegate), typeof(TemplateFactoryControl), new PropertyMetadata(null, _FactoryChanged));
        private static void _FactoryChanged(DependencyObject instance, DependencyPropertyChangedEventArgs args)
        {
            var control = (TemplateFactoryControl)instance;
            var factory = (TemplateFactoryControlDelegate)args.NewValue;
            
            var contentControl = factory();

            var frameworkElement = contentControl as FrameworkElement;
            if (frameworkElement != null)
            {
                var rapidViewRoot = frameworkElement.ThisOrFindParent<RapidView>();
                if (rapidViewRoot == null)
                    rapidViewRoot = frameworkElement.FindChildren<RapidView>().FirstOrDefault();

                if (rapidViewRoot != null)
                {
                    //Make sure to load the view!
                    RapidMvxContext parentMvxContext = null;
                    var parentView = control.FindParent<RapidView>();
                    if (parentView != null)
                    {
                        parentMvxContext = parentView.Context;
                    }
                    RapidMvx.LoadView(rapidViewRoot, control.DataContext as RapidViewModel, parentMvxContext);
                }
                else
                {
                    frameworkElement.DataContext = control.DataContext;
                }
            }
            
            //control.CustomContent = contentControl;
            control.Content = contentControl;
        }
        
        /*public object CustomContent
        {
            get
            {
                var val = GetValue(CustomContentProperty);
                if (val == null)
                {
                    var baseVal = base.Content;
                    if (baseVal != null)
                    {
                        SetValue(CustomContentProperty, baseVal);
                        val = baseVal;
                    }
                }
                return val;
            }
            set
            {
                SetValue(CustomContentProperty, value);
                base.Content = value;
            }
        }*/

        protected override bool ShouldSerializeProperty(DependencyProperty dp)
        {
            return base.ShouldSerializeProperty(dp);
        }

        public override bool ShouldSerializeContent()
        {
            return base.ShouldSerializeContent();
        }

        [XmlIgnore]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TemplateFactoryControlDelegate Factory
        {
            get { return GetValue(FactoryProperty) as TemplateFactoryControlDelegate; }
            set { SetValue(FactoryProperty, value); }
        }
    }
}
