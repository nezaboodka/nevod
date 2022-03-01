using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nezaboodka.Nevod
{
    internal class ChildrenBuilder
    {
        private readonly List<Syntax> fChildren;
        private readonly Scanner fScanner;

        public ChildrenBuilder(string sourceText)
        {
            fChildren = new List<Syntax>();
            fScanner = new Scanner(sourceText);
        }

        public ReadOnlyCollection<Syntax> GetChildren() => fChildren.AsReadOnly();

        public void Add(Syntax child) => fChildren.Add(child);

        public void AddForElements<T>(ReadOnlyCollection<T> elements) where T : Syntax
        {
            if (elements.Count != 0)
                fChildren.Add(elements[0]);
            for (int i = 0; i < elements.Count - 1; i++)
            {
                Syntax firstSyntax = elements[i];
                Syntax secondSyntax = elements[i + 1];
                AddInsideRange(firstSyntax.TextRange.End, secondSyntax.TextRange.Start);
                fChildren.Add(secondSyntax);
            }
        }

        public void AddInsideRange(int rangeStart, int rangeEnd)
        {
            if (rangeStart != rangeEnd)
            {
                fScanner.SetPosition(rangeStart);
                NextNonTriviaToken(fScanner);
                while (fScanner.CurrentToken.TextSlice.Position < rangeEnd)
                {
                    LexicalToken token = fScanner.CurrentToken;
                    NextNonTriviaToken(fScanner);
                    Lexeme lexeme = Syntax.Lexeme(token.Id);
                    int lexemeStart = token.TextSlice.Position;
                    int lexemeEnd = fScanner.CurrentToken.TextSlice.Position;
                    lexeme.TextRange = new TextRange(lexemeStart, lexemeEnd);
                    fChildren.Add(lexeme);
                }
            }
        }

        public void AddInsideRange(in TextRange textRange) => AddInsideRange(textRange.Start, textRange.End);

        private void NextNonTriviaToken(Scanner scanner)
        {
            do
            {
                scanner.NextTokenOrComment();
            } while (scanner.CurrentToken.Id == TokenId.Comment || 
                     scanner.CurrentToken.Id == TokenId.UnterminatedComment);
        }
    }
}
