using System.Windows;
using System.Windows.Controls;

namespace ICSharpCode.AvalonEdit
{
    public class DragDropHelperControl : Control
    {
        static DragDropHelperControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragDropHelperControl),
                new FrameworkPropertyMetadata(typeof(DragDropHelperControl)));
        }
    }
}
