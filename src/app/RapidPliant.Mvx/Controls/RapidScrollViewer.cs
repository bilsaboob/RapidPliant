using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using RapidPliant.Mvx.Utils;

namespace RapidPliant.Mvx.Controls
{
    public class RapidScrollViewer : ScrollViewer
    {
        public static readonly DependencyProperty AutoScrollToRightEndProperty = DependencyProperty.RegisterAttached(
            "AutoScrollToRightEnd",
            typeof(bool),
            typeof(RapidScrollViewer),
            new PropertyMetadata(false, AutoScrollToEndChanged)
        );

        private static void AutoScrollToEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = d as RapidScrollViewer;
            if(scrollViewer == null)
                return;

            scrollViewer.AutoScrollToRightEnd = (bool) e.NewValue;
        }

        public RapidScrollViewer()
        {
            InitializedChildren = new HashSet<FrameworkElement>();
        }

        private HashSet<FrameworkElement> InitializedChildren { get; set; }

        public bool AutoScrollToRightEnd
        {
            get { return (bool)GetValue(AutoScrollToRightEndProperty); }
            set
            {
                SetValue(AutoScrollToRightEndProperty, value);

                if (value)
                {
                    EnableScrollToRightEnd();
                }
                else
                {
                    DisableScrollToRightEnd();
                }
            }
        }

        private void DisableScrollToRightEnd()
        {
            ScrollChanged -= OnScrollChanged;
        }

        private void EnableScrollToRightEnd()
        {
            ScrollChanged += OnScrollChanged;
            ScrollToRightEnd();
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs scrollChangedEventArgs)
        {
            var scrollViewer = (ScrollViewer)sender;
            if (scrollViewer.HorizontalOffset == scrollViewer.ScrollableWidth)
                ScrollToRightEnd();
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            var newChildren = new HashSet<FrameworkElement>();
            foreach (var child in this.GetImmediateVisualChildren().CastEachAs<FrameworkElement>())
            {
                newChildren.Add(child);
            }

            var childrenToRemove = new HashSet<FrameworkElement>(InitializedChildren);
            childrenToRemove.ExceptWith(newChildren);

            foreach (var child in childrenToRemove)
            {
                child.SizeChanged -= ChildOnSizeChanged;
            }

            InitializedChildren = newChildren;

            foreach (var child in newChildren)
            {
                child.SizeChanged += ChildOnSizeChanged;
            }
        }

        private void ChildOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Do any potential customized caluclations here!
            var children = InitializedChildren.ToList();

            var height = Height;
            var actualHeigh = ActualHeight;
            var totalHeight = children.Sum(elem => elem.ActualHeight);
            //Height = totalHeight;

            var width = Width;
            var actualWidth = ActualWidth;
            var totalWidth = children.Sum(elem => elem.ActualWidth);
            //Width = totalWidth;
        }
    }
}
