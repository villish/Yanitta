﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Bracket;
using ICSharpCode.AvalonEdit.Rendering;
using ICSharpCode.AvalonEdit.Search;
using ICSharpCode.AvalonEdit.Utils;
using System.Windows.Automation.Peers;

namespace ICSharpCode.AvalonEdit
{
    /// <summary>
    /// The text editor control.
    /// Contains a scrollable TextArea.
    /// </summary>
    [Localizability(LocalizationCategory.Text), ContentProperty("Text")]
    public class TextEditor : Control, ITextEditorComponent, IServiceProvider, IWeakEventListener, IDisposable
    {
        bool isUpdated;
        CompletionWindow mIntelliSeinceWindow;
        ToolTip mToolTip;
        FoldingManager foldingManager;
        AbstractFoldingStrategy foldingStrategy = new RegexFoldingStrategy();
        BracketHighlightRenderer bracketRenderer;
        DispatcherTimer foldingUpdateTimer;
        BracketSearcher bracketSearcher = new BracketSearcher();

        #region Constructors

        static TextEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextEditor), new FrameworkPropertyMetadata(typeof(TextEditor)));
            FocusableProperty.OverrideMetadata(typeof(TextEditor), new FrameworkPropertyMetadata(true));
        }

        /// <summary>
        /// Creates a new TextEditor instance.
        /// </summary>
        public TextEditor()
            : this(new TextArea())
        {
            Options.ShowTabs = true;

            ShowLineNumbers = true;
            SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Resources.LUA;
            TextArea.DefaultInputHandler.NestedInputHandlers.Add(new SearchInputHandler(TextArea));
            TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.LuaIndentationStrategy(this);

            foldingManager = FoldingManager.Install(TextArea);
            foldingStrategy.UpdateFoldings(foldingManager, Document);

            foldingUpdateTimer = new DispatcherTimer();
            foldingUpdateTimer.Interval = TimeSpan.FromSeconds(1);
            foldingUpdateTimer.Tick += (o, e) => {
                if (isUpdated)
                {
                    foldingStrategy.UpdateFoldings(foldingManager, Document);
                    isUpdated = false;
                }
            };
            foldingUpdateTimer.Start();

            bracketRenderer = new BracketHighlightRenderer(TextArea.TextView);
            TextArea.Caret.PositionChanged += HighlightBrackets;

            TextArea.TextEntering += TextEditorTextAreaTextEntering;
            TextArea.PreviewKeyDown += TextArea_PreviewKeyDown;

            TextArea.TextView.MouseHover += TextViewMouseHover;
            TextArea.TextView.MouseHoverStopped += TextViewMouseHoverStopped;
        }

        /// <summary>
        /// Creates a new TextEditor instance.
        /// </summary>
        protected TextEditor(TextArea textArea)
        {
            if (textArea == null)
                throw new ArgumentNullException(nameof(textArea));

            this.textArea = textArea;

            textArea.TextView.Services.AddService(typeof(TextEditor), this);
            SetCurrentValue(OptionsProperty, textArea.Options);
            SetCurrentValue(DocumentProperty, new TextDocument());
        }

        ~TextEditor()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (foldingUpdateTimer != null)
            {
                foldingUpdateTimer.Stop();
                foldingUpdateTimer.IsEnabled = false;
                foldingUpdateTimer = null;
            }
            if (mIntelliSeinceWindow != null)
            {
                mIntelliSeinceWindow.Close();
                mIntelliSeinceWindow = null;
            }
        }

        #endregion Constructors

        /// <inheritdoc/>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new TextEditorAutomationPeer(this);
        }

        /// Forward focus to TextArea.
        /// <inheritdoc/>
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            if (e.NewFocus == this)
            {
                Keyboard.Focus(TextArea);
                e.Handled = true;
            }
        }

        #region ToolTip
        void TextViewMouseHoverStopped(object sender, MouseEventArgs e)
        {
            if (mToolTip != null && mToolTip.IsOpen)
                mToolTip.IsOpen = false;
        }

        void TextViewMouseHover(object sender, MouseEventArgs e)
        {
            var position = TextArea.TextView.GetPosition(
                                        e.GetPosition(TextArea.TextView)
                                        + TextArea.TextView.ScrollOffset
                                        );
            if (position == null)
                return;

            var offset = Document.GetOffset(position.Value.Location);
            var hovered_word = GetWord(offset);
            if (hovered_word == null)
                return;
            var keyWordInfo = IntelliSienceManager.IntelliSienceCollection.FirstOrDefault(n => n.Name == hovered_word);
            if (mToolTip != null && mToolTip.IsOpen)
                mToolTip.IsOpen = false;
            if (keyWordInfo != null)
            {
                mToolTip = new ToolTip
                {
                    Content = keyWordInfo.ToString(),
                    IsOpen = true,
                    PlacementTarget = this
                };
            }
        }
        #endregion

        #region Document property

        /// <summary>
        /// Document property.
        /// </summary>
        public static readonly DependencyProperty DocumentProperty
            = TextView.DocumentProperty.AddOwner(
                typeof(TextEditor), new FrameworkPropertyMetadata(OnDocumentChanged));

        /// <summary>
        /// Gets/Sets the document displayed by the text editor.
        /// This is a dependency property.
        /// </summary>
        public TextDocument Document
        {
            get { return (TextDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        /// <summary>
        /// Occurs when the document property has changed.
        /// </summary>
        public event EventHandler DocumentChanged;

        /// <summary>
        /// Raises the <see cref="DocumentChanged"/> event.
        /// </summary>
        protected virtual void OnDocumentChanged(EventArgs e)
        {
            if (DocumentChanged != null)
            {
                DocumentChanged(this, e);
            }
        }

        static void OnDocumentChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            ((TextEditor)dp).OnDocumentChanged((TextDocument)e.OldValue, (TextDocument)e.NewValue);
        }

        void OnDocumentChanged(TextDocument oldValue, TextDocument newValue)
        {
            if (oldValue != null)
            {
                TextDocumentWeakEventManager.TextChanged.RemoveListener(oldValue, this);
                PropertyChangedEventManager.RemoveListener(oldValue.UndoStack, this, "IsOriginalFile");
            }
            textArea.Document = newValue;
            if (newValue != null)
            {
                TextDocumentWeakEventManager.TextChanged.AddListener(newValue, this);
                PropertyChangedEventManager.AddListener(newValue.UndoStack, this, "IsOriginalFile");
            }
            OnDocumentChanged(EventArgs.Empty);
            OnTextChanged(EventArgs.Empty);
        }

        #endregion Document property

        #region Options property

        /// <summary>
        /// Options property.
        /// </summary>
        public static readonly DependencyProperty OptionsProperty
            = TextView.OptionsProperty.AddOwner(typeof(TextEditor), new FrameworkPropertyMetadata(OnOptionsChanged));

        /// <summary>
        /// Gets/Sets the options currently used by the text editor.
        /// </summary>
        public TextEditorOptions Options
        {
            get { return (TextEditorOptions)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }

        /// <summary>
        /// Occurs when a text editor option has changed.
        /// </summary>
        public event PropertyChangedEventHandler OptionChanged;

        /// <summary>
        /// Raises the <see cref="OptionChanged"/> event.
        /// </summary>
        protected virtual void OnOptionChanged(PropertyChangedEventArgs e)
        {
            if (OptionChanged != null)
            {
                OptionChanged(this, e);
            }
        }

        private static void OnOptionsChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            ((TextEditor)dp).OnOptionsChanged((TextEditorOptions)e.OldValue, (TextEditorOptions)e.NewValue);
        }

        void OnOptionsChanged(TextEditorOptions oldValue, TextEditorOptions newValue)
        {
            if (oldValue != null)
            {
                PropertyChangedWeakEventManager.RemoveListener(oldValue, this);
            }
            textArea.Options = newValue;
            if (newValue != null)
            {
                PropertyChangedWeakEventManager.AddListener(newValue, this);
            }
            OnOptionChanged(new PropertyChangedEventArgs(null));
        }

        /// <inheritdoc cref="IWeakEventListener.ReceiveWeakEvent"/>
        protected virtual bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(PropertyChangedWeakEventManager))
            {
                OnOptionChanged((PropertyChangedEventArgs)e);
                return true;
            }
            else if (managerType == typeof(TextDocumentWeakEventManager.TextChanged))
            {
                OnTextChanged(e);
                return true;
            }
            else if (managerType == typeof(PropertyChangedEventManager))
            {
                return HandleIsOriginalChanged((PropertyChangedEventArgs)e);
            }
            return false;
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            return ReceiveWeakEvent(managerType, sender, e);
        }

        #endregion Options property

        #region Text property

        /// <summary>
        ///
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TextEditor),
            new PropertyMetadata(string.Empty, OnTextPropertyTextChanged));

        private static void OnTextPropertyTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                var editor = (d as TextEditor);
                if (editor.Document.Text != (string)e.NewValue)
                {
                    editor.Document.Text = (string)e.NewValue;
                    editor.Document.UndoStack.ClearAll();
                    editor.isUpdated = true;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="TextChanged"/> event.
        /// </summary>
        protected virtual void OnTextChanged(EventArgs e)
        {
            var old = (string)GetValue(TextProperty);
            if (old != Document.Text)
            {
                SetValue(TextProperty, Document.Text);
                isUpdated = true;
                if (TextChanged != null)
                    TextChanged(this, e);
            }
        }

        /// <summary>
        /// Gets/Sets the text of the current document.
        /// </summary>
        [Localizability(LocalizationCategory.Text), DefaultValue("")]
        public string Text
        {
            get { return Document.Text;  }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Occurs when the Text property changes.
        /// </summary>
        public event EventHandler TextChanged;

        #endregion Text property

        #region TextArea / ScrollViewer properties

        readonly TextArea textArea;
        ScrollViewer scrollViewer;

        /// <summary>
        /// Is called after the template was applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            scrollViewer = (ScrollViewer)Template.FindName("PART_ScrollViewer", this);
        }

        /// <summary>
        /// Gets the text area.
        /// </summary>
        public TextArea TextArea
        {
            get { return textArea; }
        }

        /// <summary>
        /// Gets the scroll viewer used by the text editor.
        /// This property can return null if the template has not been applied / does not contain a scroll viewer.
        /// </summary>
        internal ScrollViewer ScrollViewer
        {
            get { return scrollViewer; }
        }

        bool CanExecute(RoutedUICommand command)
        {
            TextArea m_textArea = TextArea;
            if (m_textArea == null)
                return false;
            return command.CanExecute(null, m_textArea);
        }

        void Execute(RoutedUICommand command)
        {
            TextArea m_textArea = TextArea;
            if (m_textArea != null)
                command.Execute(null, m_textArea);
        }

        #endregion TextArea / ScrollViewer properties

        #region Syntax highlighting

        /// <summary>
        /// The <see cref="SyntaxHighlighting"/> property.
        /// </summary>
        public static readonly DependencyProperty SyntaxHighlightingProperty =
            DependencyProperty.Register("SyntaxHighlighting", typeof(IHighlightingDefinition), typeof(TextEditor),
                                        new FrameworkPropertyMetadata(OnSyntaxHighlightingChanged));

        /// <summary>
        /// Gets/sets the syntax highlighting definition used to colorize the text.
        /// </summary>
        public IHighlightingDefinition SyntaxHighlighting
        {
            get { return (IHighlightingDefinition)GetValue(SyntaxHighlightingProperty); }
            set { SetValue(SyntaxHighlightingProperty, value); }
        }

        IVisualLineTransformer colorizer;

        private static void OnSyntaxHighlightingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TextEditor)d).OnSyntaxHighlightingChanged(e.NewValue as IHighlightingDefinition);
        }

        void OnSyntaxHighlightingChanged(IHighlightingDefinition newValue)
        {
            if (colorizer != null)
            {
                TextArea.TextView.LineTransformers.Remove(colorizer);
                colorizer = null;
            }
            if (newValue != null)
            {
                colorizer = CreateColorizer(newValue);
                TextArea.TextView.LineTransformers.Insert(0, colorizer);
            }
        }

        /// <summary>
        /// Creates the highlighting colorizer for the specified highlighting definition.
        /// Allows derived classes to provide custom colorizer implementations for special highlighting definitions.
        /// </summary>
        /// <returns></returns>
        protected virtual IVisualLineTransformer CreateColorizer(IHighlightingDefinition highlightingDefinition)
        {
            if (highlightingDefinition == null)
                throw new ArgumentNullException(nameof(highlightingDefinition));
            return new HighlightingColorizer(highlightingDefinition.MainRuleSet);
        }

        #endregion Syntax highlighting

        #region CaretPositionChanged - Bracket Highlighting

        /// <summary>
        /// Highlights matching brackets.
        /// </summary>
        void HighlightBrackets(object sender, EventArgs e)
        {
            var result = bracketSearcher.SearchBracket(Document, TextArea.Caret.Offset);
            bracketRenderer.SetHighlight(result);
        }

        #endregion

        #region WordWrap

        /// <summary>
        /// Word wrap dependency property.
        /// </summary>
        public static readonly DependencyProperty WordWrapProperty =
            DependencyProperty.Register("WordWrap", typeof(bool), typeof(TextEditor),
                                        new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Specifies whether the text editor uses word wrapping.
        /// </summary>
        /// <remarks>
        /// Setting WordWrap=true has the same effect as setting HorizontalScrollBarVisibility=Disabled and will override the
        /// HorizontalScrollBarVisibility setting.
        /// </remarks>
        public bool WordWrap
        {
            get { return (bool)GetValue(WordWrapProperty); }
            set { SetValue(WordWrapProperty, value); }
        }

        #endregion WordWrap

        #region IsReadOnly

        /// <summary>
        /// IsReadOnly dependency property.
        /// </summary>
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(TextEditor),
                                        new FrameworkPropertyMetadata(false, OnIsReadOnlyChanged));

        /// <summary>
        /// Specifies whether the user can change the text editor content.
        /// Setting this property will replace the
        /// <see cref="Editing.TextArea.ReadOnlySectionProvider">TextArea.ReadOnlySectionProvider</see>.
        /// </summary>
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = d as TextEditor;
            if (editor != null)
            {
                if ((bool)e.NewValue)
                    editor.TextArea.ReadOnlySectionProvider = ReadOnlyDocument.Instance;
                else
                    editor.TextArea.ReadOnlySectionProvider = NoReadOnlySections.Instance;

                var peer = UIElementAutomationPeer.FromElement(editor) as TextEditorAutomationPeer;
                if (peer != null)
                {
                    peer.RaiseIsReadOnlyChanged((bool)e.OldValue, (bool)e.NewValue);
                }
            }
        }

        #endregion IsReadOnly

        #region IsModified

        /// <summary>
        /// Dependency property for <see cref="IsModified"/>
        /// </summary>
        public static readonly DependencyProperty IsModifiedProperty =
            DependencyProperty.Register("IsModified", typeof(bool), typeof(TextEditor),
                                        new FrameworkPropertyMetadata(false, OnIsModifiedChanged));

        /// <summary>
        /// Gets/Sets the 'modified' flag.
        /// </summary>
        public bool IsModified
        {
            get { return (bool)GetValue(IsModifiedProperty); }
            set { SetValue(IsModifiedProperty, value); }
        }

        static void OnIsModifiedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = d as TextEditor;
            if (editor != null)
            {
                TextDocument document = editor.Document;
                if (document != null)
                {
                    UndoStack undoStack = document.UndoStack;
                    if ((bool)e.NewValue)
                    {
                        if (undoStack.IsOriginalFile)
                            undoStack.DiscardOriginalFileMarker();
                    }
                    else
                    {
                        undoStack.MarkAsOriginalFile();
                    }
                }
            }
        }

        bool HandleIsOriginalChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsOriginalFile")
            {
                TextDocument document = Document;
                if (document != null)
                {
                    SetCurrentValue(IsModifiedProperty, !document.UndoStack.IsOriginalFile);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion IsModified

        #region ShowLineNumbers

        /// <summary>
        /// ShowLineNumbers dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowLineNumbersProperty =
            DependencyProperty.Register("ShowLineNumbers", typeof(bool), typeof(TextEditor),
                                        new FrameworkPropertyMetadata(false, OnShowLineNumbersChanged));

        /// <summary>
        /// Specifies whether line numbers are shown on the left to the text view.
        /// </summary>
        public bool ShowLineNumbers
        {
            get { return (bool)GetValue(ShowLineNumbersProperty); }
            set { SetValue(ShowLineNumbersProperty, value); }
        }

        static void OnShowLineNumbersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (TextEditor)d;
            var leftMargins = editor.TextArea.LeftMargins;
            if ((bool)e.NewValue)
            {
                var lineNumbers = new LineNumberMargin();
                var line = (Line)DottedLineMargin.Create();
                leftMargins.Insert(0, lineNumbers);
                leftMargins.Insert(1, line);
                var lineNumbersForeground = new Binding("LineNumbersForeground") { Source = editor };
                line.SetBinding(Shape.StrokeProperty, lineNumbersForeground);
                lineNumbers.SetBinding(Control.ForegroundProperty, lineNumbersForeground);
            }
            else
            {
                for (int i = 0; i < leftMargins.Count; i++)
                {
                    if (leftMargins[i] is LineNumberMargin)
                    {
                        leftMargins.RemoveAt(i);
                        if (i < leftMargins.Count && DottedLineMargin.IsDottedLineMargin(leftMargins[i]))
                        {
                            leftMargins.RemoveAt(i);
                        }
                        break;
                    }
                }
            }
        }

        #endregion ShowLineNumbers

        #region LineNumbersForeground

        /// <summary>
        /// LineNumbersForeground dependency property.
        /// </summary>
        public static readonly DependencyProperty LineNumbersForegroundProperty =
            DependencyProperty.Register("LineNumbersForeground", typeof(Brush), typeof(TextEditor),
                                        new FrameworkPropertyMetadata(Brushes.Gray, OnLineNumbersForegroundChanged));

        /// <summary>
        /// Gets/sets the Brush used for displaying the foreground color of line numbers.
        /// </summary>
        public Brush LineNumbersForeground
        {
            get { return (Brush)GetValue(LineNumbersForegroundProperty); }
            set { SetValue(LineNumbersForegroundProperty, value); }
        }

        static void OnLineNumbersForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (TextEditor)d;
            var lineNumberMargin = editor.TextArea.LeftMargins.FirstOrDefault(margin => margin is LineNumberMargin) as LineNumberMargin;

            if (lineNumberMargin != null)
            {
                lineNumberMargin.SetValue(ForegroundProperty, e.NewValue);
            }
        }

        #endregion LineNumbersForeground

        #region TextBoxBase-like methods

        /// <summary>
        /// Appends text to the end of the document.
        /// </summary>
        public void AppendText(string textData)
        {
            Document.Insert(Document.TextLength, textData);
        }

        /// <summary>
        /// Begins a group of document changes.
        /// </summary>
        public void BeginChange()
        {
            Document.BeginUpdate();
        }

        /// <summary>
        /// Copies the current selection to the clipboard.
        /// </summary>
        public void Copy()
        {
            Execute(ApplicationCommands.Copy);
        }

        /// <summary>
        /// Removes the current selection and copies it to the clipboard.
        /// </summary>
        public void Cut()
        {
            Execute(ApplicationCommands.Cut);
        }

        /// <summary>
        /// Begins a group of document changes and returns an object that ends the group of document
        /// changes when it is disposed.
        /// </summary>
        public IDisposable DeclareChangeBlock()
        {
            return Document.RunUpdate();
        }

        /// <summary>
        /// Ends the current group of document changes.
        /// </summary>
        public void EndChange()
        {
            Document.EndUpdate();
        }

        /// <summary>
        /// Scrolls one line down.
        /// </summary>
        public void LineDown()
        {
            if (scrollViewer != null)
                scrollViewer.LineDown();
        }

        /// <summary>
        /// Scrolls to the left.
        /// </summary>
        public void LineLeft()
        {
            if (scrollViewer != null)
                scrollViewer.LineLeft();
        }

        /// <summary>
        /// Scrolls to the right.
        /// </summary>
        public void LineRight()
        {
            if (scrollViewer != null)
                scrollViewer.LineRight();
        }

        /// <summary>
        /// Scrolls one line up.
        /// </summary>
        public void LineUp()
        {
            if (scrollViewer != null)
                scrollViewer.LineUp();
        }

        /// <summary>
        /// Scrolls one page down.
        /// </summary>
        public void PageDown()
        {
            if (scrollViewer != null)
                scrollViewer.PageDown();
        }

        /// <summary>
        /// Scrolls one page up.
        /// </summary>
        public void PageUp()
        {
            if (scrollViewer != null)
                scrollViewer.PageUp();
        }

        /// <summary>
        /// Scrolls one page left.
        /// </summary>
        public void PageLeft()
        {
            if (scrollViewer != null)
                scrollViewer.PageLeft();
        }

        /// <summary>
        /// Scrolls one page right.
        /// </summary>
        public void PageRight()
        {
            if (scrollViewer != null)
                scrollViewer.PageRight();
        }

        /// <summary>
        /// Pastes the clipboard content.
        /// </summary>
        public void Paste()
        {
            Execute(ApplicationCommands.Paste);
        }

        /// <summary>
        /// Redoes the most recent undone command.
        /// </summary>
        /// <returns>True is the redo operation was successful, false is the redo stack is empty.</returns>
        public bool Redo()
        {
            if (CanExecute(ApplicationCommands.Redo))
            {
                Execute(ApplicationCommands.Redo);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Scrolls to the end of the document.
        /// </summary>
        public void ScrollToEnd()
        {
            ApplyTemplate(); // ensure scrollViewer is created
            if (scrollViewer != null)
                scrollViewer.ScrollToEnd();
        }

        /// <summary>
        /// Scrolls to the start of the document.
        /// </summary>
        public void ScrollToHome()
        {
            ApplyTemplate(); // ensure scrollViewer is created
            if (scrollViewer != null)
                scrollViewer.ScrollToHome();
        }

        /// <summary>
        /// Scrolls to the specified position in the document.
        /// </summary>
        public void ScrollToHorizontalOffset(double offset)
        {
            ApplyTemplate(); // ensure scrollViewer is created
            if (scrollViewer != null)
                scrollViewer.ScrollToHorizontalOffset(offset);
        }

        /// <summary>
        /// Scrolls to the specified position in the document.
        /// </summary>
        public void ScrollToVerticalOffset(double offset)
        {
            ApplyTemplate(); // ensure scrollViewer is created
            if (scrollViewer != null)
                scrollViewer.ScrollToVerticalOffset(offset);
        }

        /// <summary>
        /// Selects the entire text.
        /// </summary>
        public void SelectAll()
        {
            Execute(ApplicationCommands.SelectAll);
        }

        /// <summary>
        /// Undoes the most recent command.
        /// </summary>
        /// <returns>True is the undo operation was successful, false is the undo stack is empty.</returns>
        public bool Undo()
        {
            if (CanExecute(ApplicationCommands.Undo))
            {
                Execute(ApplicationCommands.Undo);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets if the most recent undone command can be redone.
        /// </summary>
        public bool CanRedo
        {
            get { return CanExecute(ApplicationCommands.Redo); }
        }

        /// <summary>
        /// Gets if the most recent command can be undone.
        /// </summary>
        public bool CanUndo
        {
            get { return CanExecute(ApplicationCommands.Undo); }
        }

        /// <summary>
        /// Gets the vertical size of the document.
        /// </summary>
        public double ExtentHeight
        {
            get
            {
                return scrollViewer != null ? scrollViewer.ExtentHeight : 0;
            }
        }

        /// <summary>
        /// Gets the horizontal size of the current document region.
        /// </summary>
        public double ExtentWidth
        {
            get
            {
                return scrollViewer != null ? scrollViewer.ExtentWidth : 0;
            }
        }

        /// <summary>
        /// Gets the horizontal size of the viewport.
        /// </summary>
        public double ViewportHeight
        {
            get
            {
                return scrollViewer != null ? scrollViewer.ViewportHeight : 0;
            }
        }

        /// <summary>
        /// Gets the horizontal size of the viewport.
        /// </summary>
        public double ViewportWidth
        {
            get
            {
                return scrollViewer != null ? scrollViewer.ViewportWidth : 0;
            }
        }

        /// <summary>
        /// Gets the vertical scroll position.
        /// </summary>
        public double VerticalOffset
        {
            get
            {
                return scrollViewer != null ? scrollViewer.VerticalOffset : 0;
            }
        }

        /// <summary>
        /// Gets the horizontal scroll position.
        /// </summary>
        public double HorizontalOffset
        {
            get
            {
                return scrollViewer != null ? scrollViewer.HorizontalOffset : 0;
            }
        }

        #endregion TextBoxBase-like methods

        #region TextBox methods

        /// <summary>
        /// Gets/Sets the selected text.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SelectedText
        {
            get
            {
                TextArea m_textArea = TextArea;
                // We'll get the text from the whole surrounding segment.
                // This is done to ensure that SelectedText.Length == SelectionLength.
                if (m_textArea != null && m_textArea.Document != null && !m_textArea.Selection.IsEmpty)
                    return m_textArea.Document.GetText(m_textArea.Selection.SurroundingSegment);
                return string.Empty;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                TextArea m_textArea = TextArea;
                if (m_textArea != null && m_textArea.Document != null)
                {
                    int offset = SelectionStart;
                    int length = SelectionLength;
                    m_textArea.Document.Replace(offset, length, value);
                    // keep inserted text selected
                    m_textArea.Selection = Selection.Create(m_textArea, offset, offset + value.Length);
                }
            }
        }

        /// <summary>
        /// Gets/sets the caret position.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CaretOffset
        {
            get
            {
                TextArea m_textArea = TextArea;
                if (m_textArea != null)
                    return m_textArea.Caret.Offset;
                return 0;
            }
            set
            {
                TextArea m_textArea = TextArea;
                if (m_textArea != null)
                    m_textArea.Caret.Offset = value;
            }
        }

        /// <summary>
        /// Gets/sets the start position of the selection.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionStart
        {
            get
            {
                TextArea m_textArea = TextArea;
                if (m_textArea != null)
                {
                    if (m_textArea.Selection.IsEmpty)
                        return m_textArea.Caret.Offset;
                    return m_textArea.Selection.SurroundingSegment.Offset;
                }
                return 0;
            }
            set
            {
                Select(value, SelectionLength);
            }
        }

        /// <summary>
        /// Gets/sets the length of the selection.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectionLength
        {
            get
            {
                TextArea m_textArea = TextArea;
                if (m_textArea != null && !m_textArea.Selection.IsEmpty)
                    return m_textArea.Selection.SurroundingSegment.Length;
                return 0;
            }
            set
            {
                Select(SelectionStart, value);
            }
        }

        /// <summary>
        /// Selects the specified text section.
        /// </summary>
        public void Select(int start, int length)
        {
            int documentLength = Document != null ? Document.TextLength : 0;
            if (start < 0 || start > documentLength)
                throw new ArgumentOutOfRangeException(nameof(start), start, "Value must be between 0 and " + documentLength);
            if (length < 0 || start + length > documentLength)
                throw new ArgumentOutOfRangeException(nameof(length), length, "Value must be between 0 and " + (documentLength - length));
            textArea.Selection = Selection.Create(textArea, start, start + length);
            textArea.Caret.Offset = start + length;
        }

        /// <summary>
        /// Gets the number of lines in the document.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int LineCount
        {
            get
            {
                if (Document != null)
                    return Document.LineCount;
                else
                    return 1;
            }
        }

        /// <summary>
        /// Clears the text.
        /// </summary>
        public void Clear()
        {
            Text = string.Empty;
        }

        #endregion TextBox methods

        #region Loading from stream

        /// <summary>
        /// Loads the text from the stream, auto-detecting the encoding.
        /// </summary>
        /// <remarks>
        /// This method sets <see cref="IsModified"/> to false.
        /// </remarks>
        public void Load(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding ?? Encoding.UTF8))
            {
                Text = reader.ReadToEnd();
                Encoding = reader.CurrentEncoding; // assign encoding after ReadToEnd() so that the StreamReader can autodetect the encoding
            }
            IsModified = false;
        }

        /// <summary>
        /// Loads the text from the stream, auto-detecting the encoding.
        /// </summary>
        public void Load(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Load(fs);
            }
        }

        /// <summary>
        /// Gets/sets the encoding used when the file is saved.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Encoding Encoding { get; set; }

        /// <summary>
        /// Saves the text to the stream.
        /// </summary>
        /// <remarks>
        /// This method sets <see cref="IsModified"/> to false.
        /// </remarks>
        public void Save(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            var writer = new StreamWriter(stream, Encoding ?? Encoding.UTF8);
            writer.Write(Text);
            writer.Flush();
            // do not close the stream
            IsModified = false;
        }

        /// <summary>
        /// Saves the text to the file.
        /// </summary>
        public void Save(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Save(fs);
            }
        }

        #endregion Loading from stream

        #region MouseHover events

        /// <summary>
        /// The PreviewMouseHover event.
        /// </summary>
        public static readonly RoutedEvent PreviewMouseHoverEvent =
            TextView.PreviewMouseHoverEvent.AddOwner(typeof(TextEditor));

        /// <summary>
        /// The MouseHover event.
        /// </summary>
        public static readonly RoutedEvent MouseHoverEvent =
            TextView.MouseHoverEvent.AddOwner(typeof(TextEditor));

        /// <summary>
        /// The PreviewMouseHoverStopped event.
        /// </summary>
        public static readonly RoutedEvent PreviewMouseHoverStoppedEvent =
            TextView.PreviewMouseHoverStoppedEvent.AddOwner(typeof(TextEditor));

        /// <summary>
        /// The MouseHoverStopped event.
        /// </summary>
        public static readonly RoutedEvent MouseHoverStoppedEvent =
            TextView.MouseHoverStoppedEvent.AddOwner(typeof(TextEditor));

        /// <summary>
        /// Occurs when the mouse has hovered over a fixed location for some time.
        /// </summary>
        public event MouseEventHandler PreviewMouseHover
        {
            add { AddHandler(PreviewMouseHoverEvent, value); }
            remove { RemoveHandler(PreviewMouseHoverEvent, value); }
        }

        /// <summary>
        /// Occurs when the mouse has hovered over a fixed location for some time.
        /// </summary>
        public event MouseEventHandler MouseHover
        {
            add { AddHandler(MouseHoverEvent, value); }
            remove { RemoveHandler(MouseHoverEvent, value); }
        }

        /// <summary>
        /// Occurs when the mouse had previously hovered but now started moving again.
        /// </summary>
        public event MouseEventHandler PreviewMouseHoverStopped
        {
            add { AddHandler(PreviewMouseHoverStoppedEvent, value); }
            remove { RemoveHandler(PreviewMouseHoverStoppedEvent, value); }
        }

        /// <summary>
        /// Occurs when the mouse had previously hovered but now started moving again.
        /// </summary>
        public event MouseEventHandler MouseHoverStopped
        {
            add { AddHandler(MouseHoverStoppedEvent, value); }
            remove { RemoveHandler(MouseHoverStoppedEvent, value); }
        }

        #endregion MouseHover events

        #region ScrollBarVisibility

        /// <summary>
        /// Dependency property for <see cref="HorizontalScrollBarVisibility"/>
        /// </summary>
        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = ScrollViewer.HorizontalScrollBarVisibilityProperty.AddOwner(typeof(TextEditor), new FrameworkPropertyMetadata(ScrollBarVisibility.Visible));

        /// <summary>
        /// Gets/Sets the horizontal scroll bar visibility.
        /// </summary>
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
            set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
        }

        /// <summary>
        /// Dependency property for <see cref="VerticalScrollBarVisibility"/>
        /// </summary>
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = ScrollViewer.VerticalScrollBarVisibilityProperty.AddOwner(typeof(TextEditor), new FrameworkPropertyMetadata(ScrollBarVisibility.Visible));

        /// <summary>
        /// Gets/Sets the vertical scroll bar visibility.
        /// </summary>
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
            set { SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        #endregion ScrollBarVisibility

        #region Code Completion

        public string GetWord(int offset = -1)
        {
            if (offset == -1)
                offset = SelectionStart;

            if (offset < 0 || offset >= Text.Length)
                return null;

            int start = 0, len = 0;

            for (start = offset - 1; start >= 0; start--)
            {
                var c = Text[start];
                if (!((c >= 'A' && c <= 'z') || (c >= '0' && c <= '9') || c == '_' || c == ':' || c == '.'))
                {
                    ++start;
                    break;
                }
            }
            start = Math.Max(start, 0);
            for (int j = start; j < Text.Length; ++j, ++len)
            {
                var c = Text[j];
                if (!((c >= 'A' && c <= 'z') || (c >= '0' && c <= '9') || c == '_' || c == ':' || c == '.'))
                    break;
            }
            var word = Text.Substring(start, len);
            return string.IsNullOrWhiteSpace(word) ? null : word.Trim();
        }

        public int StartCurrentWord
        {
            get
            {
                int i = 0;
                for (i = SelectionStart - 1; i >= 0; i--)
                {
                    var c = Text[i];
                    // допустимые символы в именах...
                    if (!((c >= 'A' && c <= 'z') || (c >= '0' && c <= '9') || c == '_' || c == ':' || c == '.'))
                    {
                        ++i;
                        break;
                    }
                }
                return Math.Max(i, 0);
            }
        }

        public string CurrentWordToCursor
        {
            get
            {
                var i = StartCurrentWord;
                var word = Text.Substring(i, SelectionStart - i);
                return string.IsNullOrWhiteSpace(word) ? "" : word.Trim();
            }
        }

        void TextArea_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Space) // show intelli seince
                {
                    e.Handled = true;

                    mIntelliSeinceWindow = new CompletionWindow(this);
                    mIntelliSeinceWindow.CompletionList.CompletionData.Clear();
                    mIntelliSeinceWindow.CompletionList.CompletionData.AddRange(IntelliSienceManager.IntelliSienceCollection);
                    mIntelliSeinceWindow.Show();
                    mIntelliSeinceWindow.Closed += (se, ee) => mIntelliSeinceWindow = null;

                    var filter = CurrentWordToCursor;
                    if (filter != "")
                    {
                        mIntelliSeinceWindow.CompletionList.IsFiltering = true;
                        mIntelliSeinceWindow.CompletionList.SelectItem(filter);
                    }
                }
            }

            if (e.Key == Key.Escape) // cencel
            {
                if (mToolTip != null && mToolTip.IsOpen)
                {
                    mToolTip.IsOpen = false;
                    mToolTip = null;
                }
            }

            if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                if (e.Key == Key.D9)
                {
                    var keyWordInfo = IntelliSienceManager.IntelliSienceCollection.FirstOrDefault(n => n.Name == CurrentWordToCursor);
                    if (mToolTip != null && mToolTip.IsOpen)
                        mToolTip.IsOpen = false;
                    if (keyWordInfo != null)
                    {
                        mToolTip = new ToolTip
                        {
                            Content = keyWordInfo.ToString(),
                            IsOpen = true,
                            PlacementTarget = this
                        };
                    }
                }
                else if (e.Key == Key.D0)
                {
                    if (mToolTip != null && mToolTip.IsOpen)
                    {
                        mToolTip.IsOpen = false;
                        mToolTip = null;
                    }
                }
            }
        }

        void TextEditorTextAreaTextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && mIntelliSeinceWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    mIntelliSeinceWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        #endregion Code Completion

        object IServiceProvider.GetService(Type serviceType)
        {
            return textArea.GetService(serviceType);
        }

        /// <summary>
        /// Gets the text view position from a point inside the editor.
        /// </summary>
        /// <param name="point">The position, relative to top left
        /// corner of TextEditor control</param>
        /// <returns>The text view position, or null if the point is outside the document.</returns>
        public TextViewPosition? GetPositionFromPoint(Point point)
        {
            if (Document == null)
                return null;
            TextView textView = TextArea.TextView;
            return textView.GetPosition(TranslatePoint(point, textView) + textView.ScrollOffset);
        }

        /// <summary>
        /// Scrolls to the specified line.
        /// This method requires that the TextEditor was already assigned a size (WPF layout must have run prior).
        /// </summary>
        public void ScrollToLine(int line)
        {
            ScrollTo(line, -1);
        }

        /// <summary>
        /// Scrolls to the specified line/column.
        /// This method requires that the TextEditor was already assigned a size (WPF layout must have run prior).
        /// </summary>
        public void ScrollTo(int line, int column)
        {
            const double MinimumScrollPercentage = 0.3;

            TextView textView = textArea.TextView;
            TextDocument document = textView.Document;
            if (scrollViewer != null && document != null)
            {
                if (line < 1)
                    line = 1;
                if (line > document.LineCount)
                    line = document.LineCount;

                IScrollInfo scrollInfo = textView;
                if (!scrollInfo.CanHorizontallyScroll)
                {
                    // Word wrap is enabled. Ensure that we have up-to-date info about line height so that we scroll
                    // to the correct position.
                    // This avoids that the user has to repeat the ScrollTo() call several times when there are very long lines.
                    var vl = textView.GetOrConstructVisualLine(document.GetLineByNumber(line));
                    double remainingHeight = scrollViewer.ViewportHeight / 2;
                    while (remainingHeight > 0)
                    {
                        DocumentLine prevLine = vl.FirstDocumentLine.PreviousLine;
                        if (prevLine == null)
                            break;
                        vl = textView.GetOrConstructVisualLine(prevLine);
                        remainingHeight -= vl.Height;
                    }
                }

                var p = textArea.TextView.GetVisualPosition(new TextViewPosition(line, Math.Max(1, column)), VisualYPosition.LineMiddle);
                double verticalPos = p.Y - scrollViewer.ViewportHeight / 2;
                if (Math.Abs(verticalPos - scrollViewer.VerticalOffset) > MinimumScrollPercentage * scrollViewer.ViewportHeight)
                {
                    scrollViewer.ScrollToVerticalOffset(Math.Max(0, verticalPos));
                }
                if (column > 0)
                {
                    if (p.X > scrollViewer.ViewportWidth - Caret.MinimumDistanceToViewBorder * 2)
                    {
                        var horizontalPos = Math.Max(0, p.X - scrollViewer.ViewportWidth / 2);
                        if (Math.Abs(horizontalPos - scrollViewer.HorizontalOffset) > MinimumScrollPercentage * scrollViewer.ViewportWidth)
                        {
                            scrollViewer.ScrollToHorizontalOffset(horizontalPos);
                        }
                    }
                    else
                    {
                        scrollViewer.ScrollToHorizontalOffset(0);
                    }
                }
            }
        }
    }
}