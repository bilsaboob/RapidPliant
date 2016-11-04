using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;

namespace RapidPliant.App.Controls
{
    /// <summary>
    /// Class that inherits from the AvalonEdit TextEditor control to 
    /// enable MVVM interaction. 
    /// </summary>
    public class AvalonTextEditor : TextEditor, INotifyPropertyChanged
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(AvalonTextEditor),
            new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged)
        );

        public static readonly DependencyProperty LengthProperty = DependencyProperty.Register(
            "Length",
            typeof(int),
            typeof(AvalonTextEditor),
            new FrameworkPropertyMetadata(OnLengthChanged)
        );

        public static DependencyProperty CaretOffsetProperty = DependencyProperty.Register(
            "CaretOffset",
            typeof(int),
            typeof(AvalonTextEditor),
            new PropertyMetadata(OnCaretOffsetChanged)
        );

        public static readonly DependencyProperty TextLocationProperty = DependencyProperty.Register(
            "TextLocation", 
            typeof(TextLocation), 
            typeof(AvalonTextEditor),
            new PropertyMetadata(OnTextLocationChanged)
        );

        public static readonly DependencyProperty SelectionLengthProperty = DependencyProperty.Register(
            "SelectionLength",
            typeof(int),
            typeof(AvalonTextEditor), 
            new PropertyMetadata(OnSelectionLengthChanged)
        );

        private static void OnSelectionLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textEditor = d as AvalonTextEditor;
            if (textEditor == null)
                return;

            if (textEditor.SelectionLength != (int)e.NewValue)
            {
                textEditor.SelectionLength = (int)e.NewValue;
                textEditor.Select(textEditor.SelectionStart, (int)e.NewValue);
            }
        }

        /// <summary>
        /// DependencyProperty for the TextEditor SelectionStart property. 
        /// </summary>
        public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(
            "SelectionStart", 
            typeof(int), 
            typeof(AvalonTextEditor),
            new PropertyMetadata(OnSelectionStartChanged)
        );

        private static void OnSelectionStartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textEditor = d as AvalonTextEditor;
            if (textEditor == null)
                return;

            if (textEditor.SelectionStart != (int)e.NewValue)
            {
                textEditor.SelectionStart = (int)e.NewValue;
                textEditor.Select((int)e.NewValue, textEditor.SelectionLength);
            }
        }

        private static void OnTextLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textEditor = d as AvalonTextEditor;
            if (textEditor == null)
                return;

            var loc = (TextLocation)e.NewValue;
            if (_scrollEnabled)
                textEditor.ScrollTo(loc.Line, loc.Column);
        }

        private static void OnCaretOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textEditor = d as AvalonTextEditor;
            if (textEditor == null)
                return;

            if (textEditor.CaretOffset != (int)e.NewValue)
                textEditor.CaretOffset = (int)e.NewValue;
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //Is something like this needed?
            var textEditor = d as AvalonTextEditor;
            if (textEditor == null)
                return;

            //Set the new items source
            textEditor.Text = e.NewValue as string;
        }

        private static void OnLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private static bool _scrollEnabled = true;

        public AvalonTextEditor()
        {
            // Default options.
            FontSize = 12;
            FontFamily = new FontFamily("Consolas");
            Options = new TextEditorOptions {
                IndentationSize = 3,
                ConvertTabsToSpaces = true
            };
        }

        public TextLocation TextLocation
        {
            get { return base.Document.GetLocation(SelectionStart); }
            set { SetValue(TextLocationProperty, value); }
        }

        public new string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public int Length
        {
            get { return base.Text.Length; }
            set { }
        }

        public new int CaretOffset
        {
            get { return base.CaretOffset; }
            set { SetValue(CaretOffsetProperty, value); }
        }
        
        public new int SelectionLength
        {
            get { return base.SelectionLength; }
            set { SetValue(SelectionLengthProperty, value); }
        }

        public new int SelectionStart
        {
            get { return base.SelectionStart; }
            set { SetValue(SelectionStartProperty, value); }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            RaisePropertyChanged("Length");
            Text = base.Text;
            base.OnTextChanged(e);
        }

        void TextArea_SelectionChanged(object sender, EventArgs e)
        {
            SelectionStart = SelectionStart;
            SelectionLength = SelectionLength;
        }

        void TextArea_CaretPositionChanged(object sender, EventArgs e)
        {
            try
            {
                _scrollEnabled = false;
                TextLocation = TextLocation;
            }
            finally
            {
                _scrollEnabled = true;
            }
        }
        
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged([CallerMemberName] string memberName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
                PropertyChanged(this, new PropertyChangedEventArgs(memberName));
        }
        #endregion
    }
}
