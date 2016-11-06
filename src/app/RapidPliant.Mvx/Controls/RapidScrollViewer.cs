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
        public static readonly DependencyProperty ReScrollToRightEndProperty = DependencyProperty.RegisterAttached(
            "ReScrollToRightEnd",
            typeof(bool),
            typeof(RapidScrollViewer),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ReScrollToEndChanged)
        );

        private static void ReScrollToEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var scrollViewer = d as RapidScrollViewer;
            if(scrollViewer == null)
                return;

            scrollViewer.ReScrollToRightEnd = (bool) e.NewValue;
        }

        public RapidScrollViewer()
        {
            InitializedChildren = new HashSet<FrameworkElement>();
        }

        private HashSet<FrameworkElement> InitializedChildren { get; set; }

        public bool ReScrollToRightEnd
        {
            get { return (bool)GetValue(ReScrollToRightEndProperty); }
            set
            {
                SetValue(ReScrollToRightEndProperty, value);

                if (value)
                {
                    ScrollToRightEnd();
                }
            }
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
