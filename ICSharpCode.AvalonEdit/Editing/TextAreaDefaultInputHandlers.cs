﻿using ICSharpCode.AvalonEdit.Document;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace ICSharpCode.AvalonEdit.Editing
{
    /// <summary>
    /// Contains the predefined input handlers.
    /// </summary>
    public class TextAreaDefaultInputHandler : TextAreaInputHandler
    {
        /// <summary>
        /// Gets the caret navigation input handler.
        /// </summary>
        public TextAreaInputHandler CaretNavigation { get; private set; }

        /// <summary>
        /// Gets the editing input handler.
        /// </summary>
        public TextAreaInputHandler Editing { get; private set; }

        /// <summary>
        /// Gets the mouse selection input handler.
        /// </summary>
        public ITextAreaInputHandler MouseSelection { get; private set; }

        /// <summary>
        /// Creates a new TextAreaDefaultInputHandler instance.
        /// </summary>
        public TextAreaDefaultInputHandler(TextArea textArea)
            : base(textArea)
        {
            NestedInputHandlers.Add(CaretNavigation = CaretNavigationCommandHandler.Create(textArea));
            NestedInputHandlers.Add(Editing = EditingCommandHandler.Create(textArea));
            NestedInputHandlers.Add(MouseSelection = new SelectionMouseHandler(textArea));

            CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, ExecuteUndo, CanExecuteUndo));
            CommandBindings.Add(new CommandBinding(ApplicationCommands.Redo, ExecuteRedo, CanExecuteRedo));
        }

        internal static KeyBinding CreateFrozenKeyBinding(ICommand command, ModifierKeys modifiers, Key key)
        {
            KeyBinding keyBinding = new KeyBinding(command, key, modifiers);
            // Mark KeyBindings as frozen because they're shared between multiple editor instances.
            // KeyBinding derives from Freezable only in .NET 4, so we have to use this little trick:
            Freezable freezable = ((object)keyBinding) as Freezable;
            if (freezable != null)
                freezable.Freeze();
            return keyBinding;
        }

        internal static void WorkaroundWPFMemoryLeak(List<InputBinding> inputBindings)
        {
            // Work around WPF memory leak:
            // KeyBinding retains a reference to whichever UIElement it is used in first.
            // Using a dummy element for this purpose ensures that we don't leak
            // a real text editor (which a potentially large document).
            UIElement dummyElement = new UIElement();
            dummyElement.InputBindings.AddRange(inputBindings);
        }

        #region Undo / Redo

        UndoStack GetUndoStack()
        {
            TextDocument document = TextArea.Document;
            if (document != null)
                return document.UndoStack;
            else
                return null;
        }

        void ExecuteUndo(object sender, ExecutedRoutedEventArgs e)
        {
            var undoStack = GetUndoStack();
            if (undoStack != null)
            {
                if (undoStack.CanUndo)
                {
                    undoStack.Undo();
                    TextArea.Caret.BringCaretToView();
                }
                e.Handled = true;
            }
        }

        void CanExecuteUndo(object sender, CanExecuteRoutedEventArgs e)
        {
            var undoStack = GetUndoStack();
            if (undoStack != null)
            {
                e.Handled = true;
                e.CanExecute = undoStack.CanUndo;
            }
        }

        void ExecuteRedo(object sender, ExecutedRoutedEventArgs e)
        {
            var undoStack = GetUndoStack();
            if (undoStack != null)
            {
                if (undoStack.CanRedo)
                {
                    undoStack.Redo();
                    TextArea.Caret.BringCaretToView();
                }
                e.Handled = true;
            }
        }

        void CanExecuteRedo(object sender, CanExecuteRoutedEventArgs e)
        {
            var undoStack = GetUndoStack();
            if (undoStack != null)
            {
                e.Handled = true;
                e.CanExecute = undoStack.CanRedo;
            }
        }

        #endregion Undo / Redo
    }
}