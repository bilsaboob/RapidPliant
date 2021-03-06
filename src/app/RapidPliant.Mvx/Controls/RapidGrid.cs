﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Xml.Serialization;
using RapidPliant.Mvx.Controls.Extensions;
using RapidPliant.Mvx.Utils;

namespace RapidPliant.Mvx.Controls
{
    [ContentProperty("Contents")]
    public partial class RapidGrid : UserControl
    {
        public static readonly DependencyProperty SerializedContentProperty = DependencyProperty.Register(
            "SerializedContent",
            typeof(RapidGridSerializedContentsCollection),
            typeof(RapidGrid),
            new FrameworkPropertyMetadata(null, OnSerializedContentChanged)
        );
        
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation",
            typeof(Orientation),
            typeof(RapidGrid),
            new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure, OnOrientationChanged)
        );

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            "Items",
            typeof(IEnumerable),
            typeof(RapidGrid),
            new FrameworkPropertyMetadata(null, OnItemsChanged)
        );

        private static void OnSerializedContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as RapidGrid;
            if (grid == null)
                return;

            grid.SerializedContent = e.NewValue as RapidGridSerializedContentsCollection;
        }

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //Is something like this needed?
            var grid = d as RapidGrid;
            if (grid == null)
                return;

            var newValue = e.NewValue as IEnumerable;

            //Set the new items source
            grid.Items = newValue;
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateOrientation(d as RapidGrid);
        }

        private static void UpdateOrientation(RapidGrid grid)
        {
            grid.UpdateOrientation();
            grid.InvalidateMeasure();
        }

        [XmlIgnore]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataTemplate ItemsDataTemplate { get; set; }

        private ItemsControl _itemsControl;
        [XmlIgnore]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private ItemsControl ItemsControl
        {
            get
            {
                if (_itemsControl == null)
                {
                    _itemsControl = new ItemsControl();
                    _itemsControl.ItemsPanel = CreateItemsControlContainerTemplate();
                }
                return _itemsControl;
            }
        }

        private ItemsPanelTemplate CreateItemsControlContainerTemplate()
        {
            var factory = new FrameworkElementFactory(typeof(StackPanel));
            factory.SetValue(StackPanel.IsItemsHostProperty, true);
            factory.SetValue(StackPanel.OrientationProperty, Orientation);
            return new ItemsPanelTemplate(factory);
        }

        private Grid ContentGrid { get; set; }

        private Dictionary<object, ChildItemEntry> Entries { get; set; }

        public RapidGrid()
        {
            Contents = new RapidGridContentsCollection(this);
            Entries = new Dictionary<object, ChildItemEntry>();

            InitializeComponent();

            Orientation = Orientation.Horizontal;
        }

        private void InitializeComponent()
        {
            ContentGrid = new Grid();
            ContentGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            ContentGrid.VerticalAlignment = VerticalAlignment.Stretch;
            Content = ContentGrid;
            EnsureSerializedContent();
        }
        
        [Bindable(true)]
        //[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEnumerable Items
        {
            get
            {
                return ItemsControl.ItemsSource;
            }
            set
            {
                if (DesignerProperties.GetIsInDesignMode(this))
                    return;

                if (ItemsDataTemplate == null)
                {
                    ItemsDataTemplate = ControlTemplateFactory.CreateDataTemplate(() => {
                        //Set the content grid as the items template to be used
                        var contentGrid = ContentGrid.CloneUIElement();
                        return contentGrid;
                    });
                }

                ItemsControl.ItemTemplate = ItemsDataTemplate;
                ItemsControl.ItemsSource = value;

                //Set the content to match the items control instead of the ContentGrid, but make sure to save the initial content for serialization purposes... so it looks the same when serialized!
                EnsureSerializedContent();
                Content = ItemsControl;
            }
        }
        
        [XmlIgnore]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RapidGridContentsCollection Contents { get; private set; }

        private RapidGridSerializedContentsCollection EnsureSerializedContent()
        {
            SerializedContent = new RapidGridSerializedContentsCollection(Contents);
            return SerializedContent;
        }

        public RapidGridSerializedContentsCollection SerializedContent
        {
            get
            {
                return GetValue(SerializedContentProperty) as RapidGridSerializedContentsCollection;
            }
            set
            {
                SetValue(SerializedContentProperty, value);
                LoadContents(value);
                //Content = value;
            }
        }

        private void LoadContents(RapidGridSerializedContentsCollection contents)
        {
            if (contents != null)
            {
                foreach (var content in contents)
                {
                    if(!Contents.Contains(content))
                        Contents.Add(content);
                }
            }
        }

        public Orientation Orientation
        {
            get
            {
                return (Orientation)GetValue(OrientationProperty);
            }
            set
            {
                SetValue(OrientationProperty, value);
            }
        }

        protected override bool ShouldSerializeProperty(DependencyProperty dp)
        {
            if(dp == ContentProperty)
                return false;

            return base.ShouldSerializeProperty(dp);
        }

        private void UpdateOrientation()
        {
            var children = ContentGrid.ClearAndGetChildren();
            ContentGrid.RowDefinitions.Clear();
            ContentGrid.ColumnDefinitions.Clear();
            Entries.Clear();

            var anyWidthStar = false;
            double minWidth = 0;

            var anyHeightStar = false;
            double minHeight = 0;

            foreach (var child in children)
            {
                var childEntry = AddChild(child);

                if (childEntry.Width.IsAbsolute)
                {
                    if (childEntry.Width.Value > minWidth)
                    {
                        minWidth = childEntry.Width.Value;
                    }
                }
                else if (childEntry.Width.IsStar)
                {
                    anyWidthStar = true;
                }

                if (childEntry.Height.IsAbsolute)
                {
                    if (childEntry.Height.Value > minHeight)
                    {
                        minHeight = childEntry.Height.Value;
                    }
                }
                else if (childEntry.Height.IsStar)
                {
                    anyHeightStar = true;
                }
            }

            if (Orientation == Orientation.Horizontal)
            {
                if (ContentGrid.RowDefinitions.Count == 0)
                {
                    var rowDef = new RowDefinition() { Height = GridLength.Auto };

                    if (anyHeightStar)
                    {
                        rowDef.Height = new GridLength(1, GridUnitType.Star);
                    }
                    else if (minHeight > 0)
                    {
                        rowDef.MinHeight = minHeight;
                    }

                    ContentGrid.RowDefinitions.Add(rowDef);
                }
            }
            else
            {
                if (ContentGrid.ColumnDefinitions.Count == 0)
                {
                    var colDef = new ColumnDefinition() { Width = GridLength.Auto };

                    if (anyWidthStar)
                    {
                        colDef.Width = new GridLength(1, GridUnitType.Star);
                    }
                    else if (minWidth > 0)
                    {
                        colDef.MinWidth = minWidth;
                    }

                    ContentGrid.ColumnDefinitions.Add(colDef);
                }
            }
        }

        public void UpdateLayoutByElement(UIElement elem)
        {
            ChildItemEntry e;
            if (!Entries.TryGetValue(elem, out e))
                return;

            UpdateOrientation();
        }

        public void ChildAdded(int index, UIElement child)
        {
            if (!ContentGrid.Children.Contains(child))
            {
                if (!SerializedContent.Contains(child))
                    SerializedContent.Insert(index, child);

                ContentGrid.Children.Insert(index, child);
                UpdateOrientation();
            }
        }

        public void ChildSet(UIElement prevItem, UIElement item)
        {
            return;
        }

        public void ChildrenCleared()
        {
            ContentGrid.Children.Clear();
            UpdateOrientation();
        }

        public void ChildRemoved(UIElement child)
        {
            if (ContentGrid.Children.Contains(child))
            {
                ContentGrid.Children.Remove(child);
                if(SerializedContent.Contains(child))
                    SerializedContent.Remove(child);
                UpdateOrientation();
            }
        }

        private ChildItemEntry AddChild(UIElement child)
        {
            var e = new ChildItemEntry();
            e.Child = child;

            UpdateColumnRowDefinition(e);

            AddChildEntry(e);

            if (Orientation == Orientation.Horizontal)
            {
                ContentGrid.ColumnDefinitions.Add(e.ColumnDefinition);
            }
            else
            {
                ContentGrid.RowDefinitions.Add(e.RowDefinition);
            }

            Grid.SetRow(child, e.Row);
            Grid.SetColumn(child, e.Column);

            ((IAddChild)ContentGrid).AddChild(child);

            return e;
        }

        private void UpdateColumnRowDefinition(ChildItemEntry e)
        {
            e.Orientation = Orientation;

            e.Width = GridLength.Auto;
            e.Height = GridLength.Auto;

            if (e.ColumnDefinition == null)
            {
                e.ColumnDefinition = new ColumnDefinition() {
                    Width = e.Width
                };
                e.Column = ContentGrid.ColumnDefinitions.Count;
            }

            if (e.RowDefinition == null)
            {
                e.RowDefinition = new RowDefinition() {
                    Height = e.Height
                };
                e.Row = ContentGrid.RowDefinitions.Count;
            }

            var widthProperty = e.Child.GetPropertyValue<GridLength>(set.WidthProperty);
            if (widthProperty.HasValue && widthProperty.Value != set._unsetValue)
            {
                e.Width = widthProperty.Value;
                e.ColumnDefinition.Width = e.Width;
            }

            var heightProperty = e.Child.GetPropertyValue<GridLength>(set.HeightProperty);
            if (heightProperty.HasValue && heightProperty.Value != set._unsetValue)
            {
                e.Height = heightProperty.Value;
                e.RowDefinition.Height = e.Height;
            }

            if (Orientation == Orientation.Horizontal)
            {
                e.Row = 0;
            }
            else if (Orientation == Orientation.Vertical)
            {
                e.Column = 0;
            }
        }

        private void AddChildEntry(ChildItemEntry e)
        {
            Entries[e.Child] = e;
        }

        private class ChildItemEntry
        {
            public Orientation Orientation { get; set; }
            public UIElement Child { get; set; }
            public RowDefinition RowDefinition { get; set; }
            public ColumnDefinition ColumnDefinition { get; set; }
            public int Row { get; set; }
            public int Column { get; set; }
            public GridLength Width { get; set; }
            public GridLength Height { get; set; }
        }
    }

    public class RapidGridSerializedContentsCollection : Collection<UIElement>
    {
        public RapidGridSerializedContentsCollection()
        {
        }

        public RapidGridSerializedContentsCollection(RapidGridContentsCollection contents)
        {
            if (contents != null)
            {
                foreach (var content in contents)
                {
                    if(!Contains(content))
                        Add(content);
                }
            }
        }
    }

    public class RapidGridContentsCollection : Collection<UIElement>
    {
        private RapidGrid _grid;
        
        public RapidGridContentsCollection(RapidGrid grid)
        {
            _grid = grid;
        }

        protected override void InsertItem(int index, UIElement item)
        {
            base.InsertItem(index, item);
            _grid.ChildAdded(index, item);
        }

        protected override void SetItem(int index, UIElement item)
        {
            var prevItem = this[index];
            base.SetItem(index, item);
            _grid.ChildSet(prevItem, item);
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            base.RemoveItem(index);
            _grid.ChildRemoved(item);
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            _grid.ChildrenCleared();
        }
    }
}
