﻿using ICSharpCode.AvalonEdit.Document;
using System;
using System.Text.RegularExpressions;

namespace ICSharpCode.AvalonEdit.Indentation
{
    public class LuaIndentationStrategy : DefaultIndentationStrategy
    {
        const int indent_space_count = 4;
        const string patternFull  = @"\b(?<start>function|while(.+)do|if(.+)then|elseif(.+)then|for(.+)do|{{|end|}})\b";
        const string patternStart = @"\b(?<start>function|while(.+)do|if(.+)then|elseif(.+)then|for(.+)do|{{)\b";
        const string patternEnd   = @"\b(?<start>end)\b";

        TextEditor textEditor;

        public LuaIndentationStrategy(TextEditor textEditor)
        {
            this.textEditor = textEditor;
        }

        int CalcSpace(string str)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                if (!char.IsWhiteSpace(str[i]))
                    return i;
                if (i == str.Length - 1)
                    return str.Length;
            }
            return 0;
        }

        /// <inheritdoc cref="IIndentationStrategy.IndentLine"/>
        public override void IndentLine(TextDocument document, DocumentLine line)
        {
            if (line.PreviousLine == null)
                return;

            int prev = CalcSpace(line.PreviousLine.Text);

            var previousIsComment = line.PreviousLine.Text.TrimStart().StartsWith("--");

            if (Regex.IsMatch(line.Text, patternFull) && !previousIsComment)
            {
                var ind = new string(' ', prev);
                document.Insert(line.Offset, ind);
            }
            else if (Regex.IsMatch(line.PreviousLine.Text, patternStart) && !previousIsComment)
            {
                var ind = new string(' ', prev + indent_space_count);
                document.Insert(line.Offset, ind);

                var found = false;
                for (int i = line.LineNumber; i < document.LineCount; ++i)
                {
                    var text = document.Lines[i].Text;

                    if (string.IsNullOrWhiteSpace(text) || text.TrimStart().StartsWith("--"))
                        continue;

                    var sps = CalcSpace(text);

                    if (sps == prev && Regex.IsMatch(text, patternEnd))
                        found = true;
                }

                if (!found)
                {
                    var ntext = Environment.NewLine + new string(' ', prev) + "end";
                    var point = textEditor.SelectionStart;
                    document.Insert(line.Offset + ind.Length, ntext);
                    textEditor.SelectionStart = point;
                }
            }
            else
            {
                var ind = new string(' ', prev);
                if (line != null)
                    document.Insert(line.Offset, ind);
            }
        }
    }
}