namespace ICSharpCode.AvalonEdit.Search
{
    /// <summary>
    /// Holds default texts for buttons and labels in the SearchPanel. Override properties to add other languages.
    /// </summary>
    public class Localization
    {
        /// <summary>
        /// Default: 'Match case'
        /// </summary>
        public virtual string MatchCaseText => "Match case";

        /// <summary>
        /// Default: 'Match whole words'
        /// </summary>
        public virtual string MatchWholeWordsText => "Match whole words";

        /// <summary>
        /// Default: 'Use regular expressions'
        /// </summary>
        public virtual string UseRegexText => "Use regular expressions";

        /// <summary>
        /// Default: 'Find next (F3)'
        /// </summary>
        public virtual string FindNextText => "Find next (F3)";

        /// <summary>
        /// Default: 'Find previous (Shift+F3)'
        /// </summary>
        public virtual string FindPreviousText => "Find previous (Shift+F3)";

        /// <summary>
        /// Default: 'Error: '
        /// </summary>
        public virtual string ErrorText => "Error: ";

        /// <summary>
        /// Default: 'No matches found!'
        /// </summary>
        public virtual string NoMatchesFoundText => "No matches found!";
    }
}