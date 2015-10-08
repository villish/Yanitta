
namespace ICSharpCode.AvalonEdit.Highlighting.Bracket
{
    public class BracketSearchResult
    {
        public int OpeningOffset { get; private set; }

        public int ClosingOffset { get; private set; }

        public int DefinitionHeaderOffset { get; set; }

        public int DefinitionHeaderLength { get; set; }

        public BracketSearchResult(int openingOffset, int closingOffset)
        {
            OpeningOffset = openingOffset;
            ClosingOffset = closingOffset;
        }
    }
}