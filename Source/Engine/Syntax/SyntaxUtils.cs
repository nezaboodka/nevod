using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Nezaboodka.Nevod
{
    internal static class SyntaxUtils
    {
        internal static void CreateChildrenForElements<T>(ReadOnlyCollection<T> elements, List<Syntax> children, 
            Scanner scanner) where T : Syntax
        {
            if (elements.Count != 0)
                children.Add(elements[0]);
            for (int i = 0; i < elements.Count - 1; i++)
            {
                Syntax firstSyntax = elements[i];
                Syntax secondSyntax = elements[i + 1];
                CreateChildrenForRange(firstSyntax.TextRange.End, secondSyntax.TextRange.Start, children, scanner);
                children.Add(secondSyntax);
            }
        }

        internal static void CreateChildrenForRange(int rangeStart, int rangeEnd, List<Syntax> children, Scanner scanner)
        {
            if (rangeStart == rangeEnd)
                return;
            scanner.SetPosition(rangeStart);
            NextNonTriviaToken(scanner);
            while (scanner.CurrentToken.TextSlice.Position < rangeEnd)
            {
                LexicalToken token = scanner.CurrentToken;
                NextNonTriviaToken(scanner);
                TerminalSyntax terminal = Syntax.Terminal(token.Id);
                int terminalStart = token.TextSlice.Position;
                int terminalEnd = scanner.CurrentToken.TextSlice.Position;
                terminal.TextRange = new TextRange(terminalStart, terminalEnd);
                children.Add(terminal);
            }
        }

        internal static void CreateChildrenForRange(in TextRange textRange, List<Syntax> children, Scanner scanner) =>
            CreateChildrenForRange(textRange.Start, textRange.End, children, scanner);

        internal static List<T> MergeSyntaxListsByTextRange<T>(ReadOnlyCollection<T> firstList, 
            ReadOnlyCollection<T> secondList) where T : Syntax
        {
            var mergedList = new List<T>(firstList.Count + secondList.Count);
            int firstIndex = 0;
            int secondIndex = 0;
            while (firstIndex < firstList.Count && secondIndex < secondList.Count)
            {
                if (firstList[firstIndex].TextRange.Start < secondList[secondIndex].TextRange.Start)
                    mergedList.Add(firstList[firstIndex++]);
                else
                    mergedList.Add(secondList[secondIndex++]);
            }
            if (firstIndex < firstList.Count)
                for (int i = firstIndex; i < firstList.Count; i++)
                    mergedList.Add(firstList[i]);
            else if (secondIndex < secondList.Count)
                for (int i = secondIndex; i < secondList.Count; i++)
                    mergedList.Add(secondList[i]);
            return mergedList;
        }

        private static void NextNonTriviaToken(Scanner scanner)
        {
            do
            {
                scanner.NextTokenOrComment();
            } while (scanner.CurrentToken.Id == TokenId.Comment || 
                     scanner.CurrentToken.Id == TokenId.UnterminatedComment);
        }
    }
}
