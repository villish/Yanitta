﻿using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Diagnostics;

namespace ICSharpCode.AvalonEdit.Folding
{
    /// <summary>
    /// A section that can be folded.
    /// </summary>
    public sealed class FoldingSection : TextSegment
    {
        readonly FoldingManager manager;
        bool isFolded;
        internal CollapsedLineSection[] collapsedSections;
        string title;

        /// <summary>
        /// Gets/sets if the section is folded.
        /// </summary>
        public bool IsFolded
        {
            get { return isFolded; }
            set
            {
                if (isFolded != value)
                {
                    isFolded = value;
                    if (value)
                    {
                        // Create collapsed sections
                        if (manager != null)
                        {
                            DocumentLine startLine = manager.document.GetLineByOffset(StartOffset);
                            DocumentLine endLine = manager.document.GetLineByOffset(EndOffset);
                            if (startLine != endLine)
                            {
                                DocumentLine startLinePlusOne = startLine.NextLine;
                                collapsedSections = new CollapsedLineSection[manager.textViews.Count];
                                for (int i = 0; i < collapsedSections.Length; i++)
                                {
                                    collapsedSections[i] = manager.textViews[i].CollapseLines(startLinePlusOne, endLine);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Destroy collapsed sections
                        RemoveCollapsedLineSection();
                    }
                    if (manager != null)
                        manager.Redraw(this);
                }
            }
        }

        /// <summary>
        /// Creates new collapsed section when a text view is added to the folding manager.
        /// </summary>
        internal CollapsedLineSection CollapseSection(TextView textView)
        {
            DocumentLine startLine = manager.document.GetLineByOffset(StartOffset);
            DocumentLine endLine = manager.document.GetLineByOffset(EndOffset);
            if (startLine != endLine)
            {
                DocumentLine startLinePlusOne = startLine.NextLine;
                return textView.CollapseLines(startLinePlusOne, endLine);
            }
            return null;
        }

        internal void ValidateCollapsedLineSections()
        {
            if (!isFolded)
            {
                RemoveCollapsedLineSection();
                return;
            }
            DocumentLine startLine = manager.document.GetLineByOffset(StartOffset);
            DocumentLine endLine = manager.document.GetLineByOffset(EndOffset);
            if (startLine == endLine)
            {
                RemoveCollapsedLineSection();
            }
            else
            {
                if (collapsedSections == null)
                    collapsedSections = new CollapsedLineSection[manager.textViews.Count];
                // Validate collapsed line sections
                DocumentLine startLinePlusOne = startLine.NextLine;
                for (int i = 0; i < collapsedSections.Length; i++)
                {
                    var collapsedSection = collapsedSections[i];
                    if (collapsedSection == null || collapsedSection.Start != startLinePlusOne || collapsedSection.End != endLine)
                    {
                        // recreate this collapsed section
                        Debug.WriteLine("CollapsedLineSection validation - recreate collapsed section from " + startLinePlusOne + " to " + endLine);
                        if (collapsedSection != null)
                            collapsedSection.Uncollapse();
                        collapsedSections[i] = manager.textViews[i].CollapseLines(startLinePlusOne, endLine);
                    }
                }
            }
        }

        /// <summary>
        /// Gets/Sets the text used to display the collapsed version of the folding section.
        /// </summary>
        public string Title
        {
            get { return title; }
            set
            {
                if (title != value)
                {
                    title = value;
                    if (IsFolded && manager != null)
                        manager.Redraw(this);
                }
            }
        }

        /// <summary>
        /// Gets the content of the collapsed lines as text.
        /// </summary>
        public string TextContent
        {
            get { return manager.document.GetText(StartOffset, EndOffset - StartOffset); }
        }

        /// <summary>
        /// Gets/Sets an additional object associated with this folding section.
        /// </summary>
        public object Tag { get; set; }

        internal FoldingSection(FoldingManager manager, int startOffset, int endOffset)
        {
            this.manager = manager;
            StartOffset = startOffset;
            Length = endOffset - startOffset;
        }

        void RemoveCollapsedLineSection()
        {
            if (collapsedSections != null)
            {
                foreach (var collapsedSection in collapsedSections)
                {
                    if (collapsedSection != null && collapsedSection.Start != null)
                        collapsedSection.Uncollapse();
                }
                collapsedSections = null;
            }
        }
    }
}