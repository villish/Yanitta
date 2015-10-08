﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ICSharpCode.AvalonEdit
{
    public class DraggedAdorner : Adorner
    {
        readonly ContentPresenter _contentPresenter;
        double _left;
        double _top;
        readonly AdornerLayer _adornerLayer;

        public DraggedAdorner(object dragDropData, DataTemplate dragDropTemplate, UIElement adornedElement, AdornerLayer adornerLayer)
            : base(adornedElement)
        {
            _adornerLayer = adornerLayer;

            _contentPresenter = new ContentPresenter
            {
                Content = dragDropData,
                ContentTemplate = dragDropTemplate,
                Opacity = 0.7
            };

            _adornerLayer.Add(this);
        }

        public void SetPosition(double left, double top)
        {
            // -1 and +13 align the dragged adorner with the dashed rectangle that shows up
            // near the mouse cursor when dragging.
            _left = left - 1;
            _top = top + 13;
            if (_adornerLayer != null)
            {
                try
                {
                    _adornerLayer.Update(AdornedElement);
                }
                catch { }
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _contentPresenter.Measure(constraint);
            return _contentPresenter.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _contentPresenter.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _contentPresenter;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            var result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(_left, _top));

            return result;
        }

        public void Detach()
        {
            _adornerLayer.Remove(this);
        }
    }
}
