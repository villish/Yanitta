﻿using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ICSharpCode.AvalonEdit.Highlighting
{
    /// <summary>
    /// Allows producing foldings from a document based on braces.
    /// </summary>
    public class RegexFoldingStrategy : AbstractFoldingStrategy
    {
        const RegexOptions PatternRegexOption = RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline;

        Regex startPattern;
        Regex endPattern;
        Regex commentPattern;

        public string StartPattern
        {
            get { return startPattern != null ? startPattern.ToString() : null; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    startPattern = new Regex(value, PatternRegexOption);
            }
        }

        public string EndPattern
        {
            get { return endPattern != null ? endPattern.ToString() : null; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    endPattern = new Regex(value, PatternRegexOption);
            }
        }

        public string CommentPattern
        {
            get { return commentPattern != null ? commentPattern.ToString() : null; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    commentPattern = new Regex(value, RegexOptions.Compiled | RegexOptions.Singleline);
            }
        }

        /// <summary>
        /// Creates a new BraceFoldingStrategy.
        /// </summary>
        public RegexFoldingStrategy()
        {
            StartPattern = @"(?<start>\b(function|while|if|for)\b|{|--\[\[)";
            EndPattern = @"(?<end>\b(end)\b|}|]])";
            CommentPattern = @"^\s*--[^\[]";
        }

        /// <summary>
        /// Create <see cref="NewFolding"/>s for the specified document.
        /// </summary>
        public override IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            firstErrorOffset = -1;

            var foldings = new List<NewFolding>();
            var stack = new Stack<int>();

            foreach (var line in document.Lines)
            {
                // комментарии пропускаем
                if (commentPattern.IsMatch(line.Text))
                    continue;

                foreach (Match match in startPattern.Matches(line.Text))
                {
                    var element = match.Groups["start"];
                    if (element.Success)
                    {
                        stack.Push(line.EndOffset);
                    }
                }

                foreach (Match match in endPattern.Matches(line.Text))
                {
                    var element = match.Groups["end"];
                    if (element.Success)
                    {
                        if (stack.Count > 0)
                        {
                            var first = stack.Pop();
                            var folding = new NewFolding(first, line.EndOffset);
                            foldings.Add(folding);
                        }
                        else
                        {
                            firstErrorOffset = line.Offset;
                        }
                    }
                }
            }

            if (stack.Count > 0)
            {
                firstErrorOffset = stack.Pop();
            }

            foldings.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return foldings;
        }
    }
}