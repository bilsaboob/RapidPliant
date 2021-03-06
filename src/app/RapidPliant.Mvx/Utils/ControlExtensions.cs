﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using RapidPliant.Mvx.Binding;

namespace RapidPliant.Mvx.Utils
{
    public static class ControlExtensions
    {
        public static Rect GetAbsolutePlacement(this FrameworkElement element, bool relativeToScreen = false)
        {
            var absolutePos = element.PointToScreen(new System.Windows.Point(0, 0));
            if (relativeToScreen)
            {
                return new Rect(absolutePos.X, absolutePos.Y, element.ActualWidth, element.ActualHeight);
            }
            var posMW = Application.Current.MainWindow.PointToScreen(new System.Windows.Point(0, 0));
            absolutePos = new System.Windows.Point(absolutePos.X - posMW.X, absolutePos.Y - posMW.Y);
            return new Rect(absolutePos.X, absolutePos.Y, element.ActualWidth, element.ActualHeight);
        }

        public static void ClearChildren(this Panel panel)
        {
            panel.Children.Clear();
        }

        public static List<UIElement> GetChildren(this Panel panel)
        {
            var children = new List<UIElement>();

            foreach (UIElement child in panel.Children)
            {
                children.Add(child);
            }

            return children;
        }

        public static List<UIElement> ClearAndGetChildren(this Panel panel)
        {
            var children = new List<UIElement>();

            foreach (UIElement child in panel.Children)
            {
                children.Add(child);
            }

            panel.Children.Clear();

            return children;
        }

        public static List<UIElement> GetChildrenAfter(this Panel panel, UIElement afterChild)
        {
            var children = new List<UIElement>();

            var addChildren = false;
            foreach (UIElement child in panel.Children)
            {
                if (afterChild == child)
                    addChildren = true;

                if (addChildren)
                {
                    children.Add(child);
                }
            }

            return children;
        }

        public static Nullable<TValue> GetPropertyValue<TValue>(this UIElement elem, DependencyProperty prop)
            where TValue : struct
        {
            var val = elem.GetValue(prop);
            if (val == null)
            {
                return null;
            }
            else
            {
                if (val == DependencyProperty.UnsetValue)
                {
                    return null;
                }
                return (TValue)val;
            }
        }

        public static List<DependencyObject> GetImmediateVisualChildren(this DependencyObject parent)
        {
            var children = new List<DependencyObject>();
            if (parent == null)
                return children;

            var parentVisual = parent as Visual;
            if (parentVisual != null)
            {
                var childCount = VisualTreeHelper.GetChildrenCount(parent);
                for (var i = 0; i < childCount; ++i)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child == null)
                        continue;

                    if (!children.Contains(child))
                        children.Add(child);
                }
            }

            return children;
        }

        public static IEnumerable<T> FindChildren<T>(this DependencyObject parent)
            where T : class
        {
            foreach (var child in parent.GetAllChildren())
            {
                var other = child as T;
                if (other != null)
                    yield return other;
            }
        }

        public static List<DependencyObject> GetAllChildren(this DependencyObject parent)
        {
            var children = new List<DependencyObject>();
            if (parent == null)
                return children;

            var parentVisual = parent as Visual;
            if (parentVisual != null)
            {
                var childCount = VisualTreeHelper.GetChildrenCount(parent);
                for (var i = 0; i < childCount; ++i)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);
                    if (child == null)
                        continue;

                    if (!children.Contains(child))
                        children.Add(child);
                }
            }

            foreach (var childElem in LogicalTreeHelper.GetChildren(parent))
            {
                var child = childElem as DependencyObject;
                if (child == null)
                    continue;

                if (!children.Contains(child))
                    children.Add(child);
            }

            return children;
        }

        public static FrameworkElement ThisOrFindParentWithDataContext(this DependencyObject child)
        {
            var frameworkElem = child as FrameworkElement;
            if (frameworkElem != null)
            {
                if (frameworkElem.DataContext != null)
                {
                    return frameworkElem;
                }
            }

            return child.FindParentWithDataContext();
        }

        public static FrameworkElement FindParentWithDataContext(this DependencyObject child)
        {
            if (child == null)
                return null;

            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
            {
                parentObject = LogicalTreeHelper.GetParent(child);
            }

            if (parentObject == null)
                return null;

            var parentFrameworkElem = parentObject as FrameworkElement;
            if (parentFrameworkElem != null)
            {
                if (parentFrameworkElem.DataContext != null)
                {
                    return parentFrameworkElem;
                }
            }

            return FindParentWithDataContext(parentObject);
        }

        public static T ThisOrFindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            if (child == null)
                return default(T);

            var typedChild = child as T;
            if (typedChild != null)
                return typedChild;

            return child.FindParent<T>();
        }

        public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            if (child == null)
                return default(T);

            var parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
            {
                parentObject = LogicalTreeHelper.GetParent(child);
            }

            var parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                return FindParent<T>(parentObject);
            }
        }

        public static UIElement CloneUIElement(this UIElement value)
        {
            if (value == null)
            {
                return null;
            }

            var sb = new StringBuilder();
            var writer = XmlWriter.Create(sb, CloneUIElementXmlWriterSettings);

            var mgr = new XamlDesignerSerializationManager(writer);
            mgr.XamlWriterMode = XamlWriterMode.Expression;

            XamlWriter.Save(value, mgr);
            var xamlStr = sb.ToString();

            xamlStr = RapidBindingSerializationProperty.UnwrapXaml(xamlStr);

            xamlStr = xamlStr.Replace("&quot;", "\"").Replace("\"\"", "\"");

            var stringReader = new StringReader(xamlStr);
            var xmlReader = XmlTextReader.Create(stringReader, new XmlReaderSettings());
            var elem = (UIElement)XamlReader.Load(xmlReader);

            return elem;
        }

        private static XmlWriterSettings _cloneUIElementXmlWriterSettings;
        private static XmlWriterSettings CloneUIElementXmlWriterSettings
        {
            get
            {
                if (_cloneUIElementXmlWriterSettings == null)
                {
                    _cloneUIElementXmlWriterSettings = new XmlWriterSettings {
                        Indent = true,
                        ConformanceLevel = ConformanceLevel.Fragment,
                        OmitXmlDeclaration = true,
                        NamespaceHandling = NamespaceHandling.OmitDuplicates
                    };
                    TypeDescriptor.AddAttributes(typeof(BindingExpression), new TypeConverterAttribute(typeof(BindingExpressionConverter)));
                }

                return _cloneUIElementXmlWriterSettings;
            }
        }
    }
}
