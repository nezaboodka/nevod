using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Nezaboodka.Nevod.Engine.Tests.TestHelper;

namespace Nezaboodka.Nevod.Engine.Tests
{
    [TestClass]
    [TestCategory("Syntax"), TestCategory("Syntax children")]
    public class SyntaxChildrenTests
    {
        [TestMethod]
        public void AnySpanSyntaxChildren()
        {
            void AnySpanWithoutExtraction()
            {
                string pattern = "HtmlTitle = '<title>' ... '</title>';";
                PackageSyntax package = ParsePatterns(pattern);
                var anySpanSyntax = (AnySpanSyntax)((PatternSyntax)package.Patterns[0]).Body;
                anySpanSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = anySpanSyntax.Children;
                Assert.AreEqual(children.Count, 3);
                Assert.AreEqual(anySpanSyntax.Left, children[0]);
                TestTokenIdAndTextRange(pattern, children[1], TokenId.Ellipsis, "... ");
                Assert.AreEqual(anySpanSyntax.Right, children[2]);
            }
            void AnySpanWithExtraction()
            {
                string pattern = "HtmlTitle(Title) = '<title>' .. Title .. '</title>';";
                PackageSyntax package = ParsePatterns(pattern);
                var anySpanSyntax = (AnySpanSyntax)((PatternSyntax)package.Patterns[0]).Body;
                anySpanSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = anySpanSyntax.Children;
                Assert.AreEqual(children.Count, 5);
                Assert.AreEqual(anySpanSyntax.Left, children[0]);
                TestTokenIdAndTextRange(pattern, children[1], TokenId.DoublePeriod, ".. ");
                Assert.AreEqual(anySpanSyntax.ExtractionOfSpan, children[2]);
                TestTokenIdAndTextRange(pattern, children[3], TokenId.DoublePeriod, ".. ");
                Assert.AreEqual(anySpanSyntax.Right, children[4]);
            }
            void AnySpanWithMissingRightPart()
            {
                string pattern = "HtmlTitle = '<title>' ... ;";
                PackageSyntax package = ParsePatterns(pattern);
                var anySpanSyntax = (AnySpanSyntax)((PatternSyntax)package.Patterns[0]).Body;
                anySpanSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = anySpanSyntax.Children;
                Assert.AreEqual(children.Count, 2);
                Assert.AreEqual(anySpanSyntax.Left, children[0]);
                TestTokenIdAndTextRange(pattern, children[1], TokenId.Ellipsis, "... ");
            }
            AnySpanWithoutExtraction();
            AnySpanWithExtraction();
            AnySpanWithMissingRightPart();
        }

        [TestMethod]
        public void ConjunctionSyntaxChildren()
        {
            string pattern = "Pattern = Alpha & Num & AlphaNum;";
            PackageSyntax package = ParsePatterns(pattern);
            var conjunctionSyntax = (ConjunctionSyntax)((PatternSyntax)package.Patterns[0]).Body;
            conjunctionSyntax.CreateChildren(pattern);
            ReadOnlyCollection<Syntax> children = conjunctionSyntax.Children;
            Assert.AreEqual(children.Count, 5);
            Assert.AreEqual(conjunctionSyntax.Elements[0], children[0]);
            TestTokenIdAndTextRange(pattern, children[1], TokenId.Amphersand, "& ");
            Assert.AreEqual(conjunctionSyntax.Elements[1], children[2]);
            TestTokenIdAndTextRange(pattern, children[3], TokenId.Amphersand, "& ");
            Assert.AreEqual(conjunctionSyntax.Elements[2], children[4]);
        }

        [TestMethod]
        public void ExceptionSyntaxChildren()
        {
            string pattern = "Pattern = { Word, ~'Hello' };";
            PackageSyntax package = ParsePatterns(pattern);
            var exceptionSyntax = (ExceptionSyntax)((VariationSyntax)((PatternSyntax)package.Patterns[0]).Body).Elements[1];
            exceptionSyntax.CreateChildren(pattern);
            ReadOnlyCollection<Syntax> children = exceptionSyntax.Children;
            Assert.AreEqual(children.Count, 2);
            TestTokenIdAndTextRange(pattern, children[0], TokenId.Tilde, "~");
            Assert.AreEqual(exceptionSyntax.Body, children[1]);
        }

        [TestMethod]
        public void ExtractionFromFieldSyntaxChildren()
        {
            string pattern = @"Pattern(X) = P2(X: Q);";
            PackageSyntax package = ParsePatterns(pattern);
            var extractionFromFieldSyntax =
                (ExtractionFromFieldSyntax)((PatternReferenceSyntax)((PatternSyntax)package.Patterns[0]).Body)
                .ExtractionFromFields[0];
            extractionFromFieldSyntax.CreateChildren(pattern);
            ReadOnlyCollection<Syntax> children = extractionFromFieldSyntax.Children;
            Assert.AreEqual(children.Count, 3);
            TestTokenIdAndTextRange(pattern, children[0], TokenId.Identifier, "X");
            TestTokenIdAndTextRange(pattern, children[1], TokenId.Colon, ": ");
            TestTokenIdAndTextRange(pattern, children[2], TokenId.Identifier, "Q");
        }

        [TestMethod]
        public void ExtractionSyntaxChildren()
        {
            string pattern = @"Pattern(X) = X: Word;";
            PackageSyntax package = ParsePatterns(pattern);
            var extractionSyntax = (ExtractionSyntax)((PatternSyntax)package.Patterns[0]).Body;
            extractionSyntax.CreateChildren(pattern);
            ReadOnlyCollection<Syntax> children = extractionSyntax.Children;
            Assert.AreEqual(children.Count, 3);
            TestTokenIdAndTextRange(pattern, children[0], TokenId.Identifier, "X");
            TestTokenIdAndTextRange(pattern, children[1], TokenId.Colon, ": ");
            Assert.AreEqual(extractionSyntax.Body, children[2]);
        }

        [TestMethod]
        public void FieldReferenceSyntaxChildren()
        {
            string pattern = @"Pattern(X) = X: Word + X;";
            PackageSyntax package = ParsePatterns(pattern);
            var fieldReferenceSyntax = (FieldReferenceSyntax)((SequenceSyntax)((PatternSyntax)package.Patterns[0]).Body).Elements[1];
            fieldReferenceSyntax.CreateChildren(pattern);
            ReadOnlyCollection<Syntax> children = fieldReferenceSyntax.Children;
            Assert.AreEqual(children.Count, 1);
            TestTokenIdAndTextRange(pattern, children[0], TokenId.Identifier, "X");
        }

        [TestMethod]
        public void FieldSyntaxChildren()
        {
            string pattern = @"Pattern( ~Field ) = Field: Word;";
            PackageSyntax package = ParsePatterns(pattern);
            FieldSyntax fieldSyntax = ((PatternSyntax)package.Patterns[0]).Fields[0];
            fieldSyntax.CreateChildren(pattern);
            ReadOnlyCollection<Syntax> children = fieldSyntax.Children;
            Assert.AreEqual(children.Count, 2);
            TestTokenIdAndTextRange(pattern, children[0], TokenId.Tilde, "~");
            TestTokenIdAndTextRange(pattern, children[1], TokenId.Identifier, "Field ");
        }

        [TestMethod]
        public void HavingSyntaxChildren()
        {
            void HavingWithInnerAndOuter()
            {
                string pattern = @"#AboutNezaboodka = Sentence @having 'Nezaboodka';";
                PackageSyntax package = ParsePatterns(pattern);
                var havingSyntax = (HavingSyntax)((PatternSyntax)package.Patterns[0]).Body;
                havingSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = havingSyntax.Children;
                Assert.AreEqual(children.Count, 3);
                Assert.AreEqual(havingSyntax.Outer, children[0]);
                TestTokenIdAndTextRange(pattern, children[1], TokenId.HavingKeyword, "@having ");
                Assert.AreEqual(havingSyntax.Inner, children[2]);
            }
            void HavingWithoutInner()
            {
                string pattern = @"#AboutNezaboodka = Sentence @having;";
                PackageSyntax package = ParsePatterns(pattern);
                var havingSyntax = (HavingSyntax)((PatternSyntax)package.Patterns[0]).Body;
                havingSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = havingSyntax.Children;
                Assert.AreEqual(children.Count, 2);
                Assert.AreEqual(havingSyntax.Outer, children[0]);
                TestTokenIdAndTextRange(pattern, children[1], TokenId.HavingKeyword, "@having");
            }
            HavingWithInnerAndOuter();
            HavingWithoutInner();
        }

        [TestMethod]
        public void InsideSyntaxChildren()
        {
            string pattern = @"#AboutNezaboodka = 'Nezaboodka' @inside Sentence;";
            PackageSyntax package = ParsePatterns(pattern);
            var insideSyntax = (InsideSyntax)((PatternSyntax)package.Patterns[0]).Body;
            insideSyntax.CreateChildren(pattern);
            ReadOnlyCollection<Syntax> children = insideSyntax.Children;
            Assert.AreEqual(children.Count, 3);
            Assert.AreEqual(insideSyntax.Inner, children[0]);
            TestTokenIdAndTextRange(pattern, children[1], TokenId.InsideKeyword, "@inside ");
            Assert.AreEqual(insideSyntax.Outer, children[2]);
        }

        [TestMethod]
        public void OptionalitySyntaxChildren()
        {
            string pattern = @"#Company = 'Nezaboodka ' + ?'Software';";
            PackageSyntax package = ParsePatterns(pattern);
            var optionalitySyntax = (OptionalitySyntax)((SequenceSyntax)((PatternSyntax)package.Patterns[0]).Body).Elements[1];
            optionalitySyntax.CreateChildren(pattern);
            ReadOnlyCollection<Syntax> children = optionalitySyntax.Children;
            Assert.AreEqual(children.Count, 2);
            TestTokenIdAndTextRange(pattern, children[0], TokenId.Question, "?");
            Assert.AreEqual(optionalitySyntax.Body, children[1]);
        }

        [TestMethod]
        public void OutsideSyntaxChildren()
        {
            string pattern = @"#AboutNezaboodka = 'Nezaboodka' @outside Sentence;";
            PackageSyntax package = ParsePatterns(pattern);
            var outsideSyntax = (OutsideSyntax)((PatternSyntax)package.Patterns[0]).Body;
            outsideSyntax.CreateChildren(pattern);
            ReadOnlyCollection<Syntax> children = outsideSyntax.Children;
            Assert.AreEqual(children.Count, 3);
            Assert.AreEqual(outsideSyntax.Body, children[0]);
            TestTokenIdAndTextRange(pattern, children[1], TokenId.OutsideKeyword, "@outside ");
            Assert.AreEqual(outsideSyntax.Exception, children[2]);
        }

        [TestMethod]
        public void PackageSyntaxChildren()
        {
            string patterns = @"
@require 'Basic.np';
@namespace Namespace {
    Pattern1 = Word;
    @search Pattern1;
    Pattern2 = Num;
}
@search Namespace.Pattern2;
@search Basic.*;
";
            PackageSyntax package = ParsePatterns(patterns);
            package.CreateChildren(patterns);
            ReadOnlyCollection<Syntax> children = package.Children;
            Assert.AreEqual(children.Count, 10);
            Assert.AreEqual(package.RequiredPackages[0], children[0]);
            TestTokenIdAndTextRange(patterns, children[1], TokenId.NamespaceKeyword, "@namespace ");
            TestTokenIdAndTextRange(patterns, children[2], TokenId.Identifier, "Namespace ");
            TestTokenIdAndTextRange(patterns, children[3], TokenId.OpenCurlyBrace, @"{
    ");
            Assert.AreEqual(package.Patterns[0], children[4]);
            Assert.AreEqual(package.SearchTargets[0], children[5]);
            Assert.AreEqual(package.Patterns[1], children[6]);
            TestTokenIdAndTextRange(patterns, children[7], TokenId.CloseCurlyBrace, @"}
");
            Assert.AreEqual(package.SearchTargets[1], children[8]);
            Assert.AreEqual(package.SearchTargets[2], children[9]);
        }

        [TestMethod]
        public void PatternReferenceSyntaxChildren()
        {
            void PatternReferenceWithEmptyExtractionList()
            {
                string pattern = @"Pattern(X) = P2();";
                PackageSyntax package = ParsePatterns(pattern);
                var patternReferenceSyntax = (PatternReferenceSyntax)((PatternSyntax)package.Patterns[0]).Body;
                patternReferenceSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = patternReferenceSyntax.Children;
                Assert.AreEqual(children.Count, 3);
                TestTokenIdAndTextRange(pattern, children[0], TokenId.Identifier, "P2");
                TestTokenIdAndTextRange(pattern, children[1], TokenId.OpenParenthesis, "(");
                TestTokenIdAndTextRange(pattern, children[2], TokenId.CloseParenthesis, ")");
            }
            void PatternReferenceWithExtraction()
            {
                string pattern = @"Pattern(X) = P2(X: Q);";
                PackageSyntax package = ParsePatterns(pattern);
                var patternReferenceSyntax = (PatternReferenceSyntax)((PatternSyntax)package.Patterns[0]).Body;
                patternReferenceSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = patternReferenceSyntax.Children;
                Assert.AreEqual(children.Count, 4);
                TestTokenIdAndTextRange(pattern, children[0], TokenId.Identifier, "P2");
                TestTokenIdAndTextRange(pattern, children[1], TokenId.OpenParenthesis, "(");
                Assert.AreEqual(patternReferenceSyntax.ExtractionFromFields[0], children[2]);
                TestTokenIdAndTextRange(pattern, children[3], TokenId.CloseParenthesis, ")");
            }
            PatternReferenceWithEmptyExtractionList();
            PatternReferenceWithExtraction();
        }

        [TestMethod]
        public void PatternSyntaxChildren()
        {
            void SimplePattern()
            {
                string pattern = @"#Pattern = Word;";
                PackageSyntax package = ParsePatterns(pattern);
                var patternSyntax = (PatternSyntax)package.Patterns[0];
                patternSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = patternSyntax.Children;
                Assert.AreEqual(children.Count, 5);
                TestTokenIdAndTextRange(pattern, children[0], TokenId.HashSign, "#");
                TestTokenIdAndTextRange(pattern, children[1], TokenId.Identifier, "Pattern ");
                TestTokenIdAndTextRange(pattern, children[2], TokenId.Equal, "= ");
                Assert.AreEqual(patternSyntax.Body, children[3]);
                TestTokenIdAndTextRange(pattern, children[4], TokenId.Semicolon, ";");
            }
            void PatternWithFieldsAndWithoutBody()
            {
                string pattern = @"Pattern(F1, F2) = ;";
                PackageSyntax package = ParsePatterns(pattern);
                var patternSyntax = (PatternSyntax)package.Patterns[0];
                patternSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = patternSyntax.Children;
                Assert.AreEqual(children.Count, 8);
                TestTokenIdAndTextRange(pattern, children[0], TokenId.Identifier, "Pattern");
                TestTokenIdAndTextRange(pattern, children[1], TokenId.OpenParenthesis, "(");
                Assert.AreEqual(patternSyntax.Fields[0], children[2]);
                TestTokenIdAndTextRange(pattern, children[3], TokenId.Comma, ", ");
                Assert.AreEqual(patternSyntax.Fields[1], children[4]);
                TestTokenIdAndTextRange(pattern, children[5], TokenId.CloseParenthesis, ") ");
                TestTokenIdAndTextRange(pattern, children[6], TokenId.Equal, "= ");
                TestTokenIdAndTextRange(pattern, children[7], TokenId.Semicolon, ";");
            }
            void PatternWithNestedPatternsAndWithoutSemicolon()
            {
                string pattern = @"
Pattern = Nested1 + Nested2 @where {
    Nested1 = Word;
    Nested2 = Num;
}";
                PackageSyntax package = ParsePatterns(pattern);
                var patternSyntax = (PatternSyntax)package.Patterns[0];
                patternSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = patternSyntax.Children;
                Assert.AreEqual(children.Count, 8);
                TestTokenIdAndTextRange(pattern, children[0], TokenId.Identifier, "Pattern ");
                TestTokenIdAndTextRange(pattern, children[1], TokenId.Equal, "= ");
                Assert.AreEqual(patternSyntax.Body, children[2]);
                TestTokenIdAndTextRange(pattern, children[3], TokenId.WhereKeyword, "@where ");
                TestTokenIdAndTextRange(pattern, children[4], TokenId.OpenCurlyBrace, @"{
    ");
                Assert.AreEqual(patternSyntax.NestedPatterns[0], children[5]);
                Assert.AreEqual(patternSyntax.NestedPatterns[1], children[6]);
                TestTokenIdAndTextRange(pattern, children[7], TokenId.CloseCurlyBrace, "}");
            }
            SimplePattern();
            PatternWithFieldsAndWithoutBody();
            PatternWithNestedPatternsAndWithoutSemicolon();
        }

        [TestMethod]
        public void RepetitionSyntaxChildren()
        {
            string pattern = @"#Pattern = [1-9 WordBreak];";
            PackageSyntax package = ParsePatterns(pattern);
            var repetitionSyntax = (RepetitionSyntax)((SpanSyntax)((PatternSyntax)package.Patterns[0]).Body).Elements[0];
            repetitionSyntax.CreateChildren(pattern);
            ReadOnlyCollection<Syntax> children = repetitionSyntax.Children;
            Assert.AreEqual(children.Count, 4);
            TestTokenIdAndTextRange(pattern, children[0], TokenId.IntegerLiteral, "1");
            TestTokenIdAndTextRange(pattern, children[1], TokenId.Minus, "-");
            TestTokenIdAndTextRange(pattern, children[2], TokenId.IntegerLiteral, "9 ");
            Assert.AreEqual(repetitionSyntax.Body, children[3]);
        }

        [TestMethod]
        public void RequiredPackageSyntaxChildren()
        {
            string pattern = @"@require 'Basic.np';";
            PackageSyntax package = ParsePatterns(pattern);
            RequiredPackageSyntax requiredPackageSyntax = package.RequiredPackages[0];
            requiredPackageSyntax.CreateChildren(pattern);
            ReadOnlyCollection<Syntax> children = requiredPackageSyntax.Children;
            Assert.AreEqual(children.Count, 3);
            TestTokenIdAndTextRange(pattern, children[0], TokenId.RequireKeyword, "@require ");
            TestTokenIdAndTextRange(pattern, children[1], TokenId.StringLiteral, "'Basic.np'");
            TestTokenIdAndTextRange(pattern, children[2], TokenId.Semicolon, ";");
        }

        [TestMethod]
        public void PatternSearchTargetSyntaxChildren()
        {
            string pattern = @"@search Basic.Time;";
            PackageSyntax package = ParsePatterns(pattern);
            var searchTarget = (PatternSearchTargetSyntax)package.SearchTargets[0];
            searchTarget.CreateChildren(pattern);
            ReadOnlyCollection<Syntax> children = searchTarget.Children;
            Assert.AreEqual(children.Count, 3);
            TestTokenIdAndTextRange(pattern, children[0], TokenId.SearchKeyword, "@search ");
            Assert.AreEqual(searchTarget.PatternReference, children[1]);
            TestTokenIdAndTextRange(pattern, children[2], TokenId.Semicolon, ";");
        }

        [TestMethod]
        public void NamespaceSearchTargetSyntaxChildren()
        {
            string pattern = @"@search Basic.*;";
            PackageSyntax package = ParsePatterns(pattern);
            var searchTarget = (NamespaceSearchTargetSyntax)package.SearchTargets[0];
            searchTarget.CreateChildren(pattern);
            ReadOnlyCollection<Syntax> children = searchTarget.Children;
            Assert.AreEqual(children.Count, 5);
            TestTokenIdAndTextRange(pattern, children[0], TokenId.SearchKeyword, "@search ");
            TestTokenIdAndTextRange(pattern, children[1], TokenId.Identifier, "Basic");
            TestTokenIdAndTextRange(pattern, children[2], TokenId.Period, ".");
            TestTokenIdAndTextRange(pattern, children[3], TokenId.Asterisk, "*");
            TestTokenIdAndTextRange(pattern, children[4], TokenId.Semicolon, ";");
        }

        [TestMethod]
        public void SequenceSyntaxChildren()
        {
            void SequenceWithAllElements()
            {
                string pattern = "Pattern = Alpha + Num;";
                PackageSyntax package = ParsePatterns(pattern);
                var sequenceSyntax = (SequenceSyntax)((PatternSyntax)package.Patterns[0]).Body;
                sequenceSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = sequenceSyntax.Children;
                Assert.AreEqual(children.Count, 3);
                Assert.AreEqual(sequenceSyntax.Elements[0], children[0]);
                TestTokenIdAndTextRange(pattern, children[1], TokenId.Plus, "+ ");
                Assert.AreEqual(sequenceSyntax.Elements[1], children[2]);
            }
            void SequenceWithoutLastElement()
            {
                string pattern = "Pattern = Alpha + ;";
                PackageSyntax package = ParsePatterns(pattern);
                var sequenceSyntax = (SequenceSyntax)((PatternSyntax)package.Patterns[0]).Body;
                sequenceSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = sequenceSyntax.Children;
                Assert.AreEqual(children.Count, 2);
                Assert.AreEqual(sequenceSyntax.Elements[0], children[0]);
                TestTokenIdAndTextRange(pattern, children[1], TokenId.Plus, "+ ");
            }
            SequenceWithAllElements();
            SequenceWithoutLastElement();
        }

        [TestMethod]
        public void SpanSyntaxChildren()
        {
            void SpanWithElements()
            {
                string pattern = @"#Pattern = [1-9 WordBreak, 1-9 Space];";
                PackageSyntax package = ParsePatterns(pattern);
                var spanSyntax = (SpanSyntax)((PatternSyntax)package.Patterns[0]).Body;
                spanSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = spanSyntax.Children;
                Assert.AreEqual(children.Count, 5);
                TestTokenIdAndTextRange(pattern, children[0], TokenId.OpenSquareBracket, "[");
                Assert.AreEqual(spanSyntax.Elements[0], children[1]);
                TestTokenIdAndTextRange(pattern, children[2], TokenId.Comma, ", ");
                Assert.AreEqual(spanSyntax.Elements[1], children[3]);
                TestTokenIdAndTextRange(pattern, children[4], TokenId.CloseSquareBracket, "]");
            }
            void EmptySpan()
            {
                string pattern = @"#Pattern = [];";
                PackageSyntax package = ParsePatterns(pattern);
                var spanSyntax = (SpanSyntax)((PatternSyntax)package.Patterns[0]).Body;
                spanSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = spanSyntax.Children;
                Assert.AreEqual(children.Count, 2);
                TestTokenIdAndTextRange(pattern, children[0], TokenId.OpenSquareBracket, "[");
                TestTokenIdAndTextRange(pattern, children[1], TokenId.CloseSquareBracket, "]");
            }
            SpanWithElements();
            EmptySpan();
        }

        [TestMethod]
        public void TextSyntaxChildren()
        {
            string pattern = @"Pattern = 'text'*(Alpha, 3-6, Lowercase);";
            PackageSyntax package = ParsePatterns(pattern);
            var textSyntax = (TextSyntax)((PatternSyntax)package.Patterns[0]).Body;
            textSyntax.CreateChildren(pattern);
            ReadOnlyCollection<Syntax> children = textSyntax.Children;
            Assert.AreEqual(children.Count, 10);
            TestTokenIdAndTextRange(pattern, children[0], TokenId.StringLiteral, "'text'*");
            TestTokenIdAndTextRange(pattern, children[1], TokenId.OpenParenthesis, "(");
            TestTokenIdAndTextRange(pattern, children[2], TokenId.Identifier, "Alpha");
            TestTokenIdAndTextRange(pattern, children[3], TokenId.Comma, ", ");
            TestTokenIdAndTextRange(pattern, children[4], TokenId.IntegerLiteral, "3");
            TestTokenIdAndTextRange(pattern, children[5], TokenId.Minus, "-");
            TestTokenIdAndTextRange(pattern, children[6], TokenId.IntegerLiteral, "6");
            TestTokenIdAndTextRange(pattern, children[7], TokenId.Comma, ", ");
            TestTokenIdAndTextRange(pattern, children[8], TokenId.Identifier, "Lowercase");
            TestTokenIdAndTextRange(pattern, children[9], TokenId.CloseParenthesis, ")");
        }

        [TestMethod]
        public void TokenSyntaxChildren()
        {
            string pattern = @"Pattern = Alpha(2-10, TitleCase);";
            PackageSyntax package = ParsePatterns(pattern);
            var tokenSyntax = (TokenSyntax)((PatternSyntax)package.Patterns[0]).Body;
            tokenSyntax.CreateChildren(pattern);
            ReadOnlyCollection<Syntax> children = tokenSyntax.Children;
            Assert.AreEqual(children.Count, 8);
            TestTokenIdAndTextRange(pattern, children[0], TokenId.Identifier, "Alpha");
            TestTokenIdAndTextRange(pattern, children[1], TokenId.OpenParenthesis, "(");
            TestTokenIdAndTextRange(pattern, children[2], TokenId.IntegerLiteral, "2");
            TestTokenIdAndTextRange(pattern, children[3], TokenId.Minus, "-");
            TestTokenIdAndTextRange(pattern, children[4], TokenId.IntegerLiteral, "10");
            TestTokenIdAndTextRange(pattern, children[5], TokenId.Comma, ", ");
            TestTokenIdAndTextRange(pattern, children[6], TokenId.Identifier, "TitleCase");
            TestTokenIdAndTextRange(pattern, children[7], TokenId.CloseParenthesis, ")");
        }

        [TestMethod]
        public void VariationSyntaxChildren()
        {
            void VariationWithElements()
            {
                string pattern = @"#Pattern = {Word, Num};";
                PackageSyntax package = ParsePatterns(pattern);
                var variationSyntax = (VariationSyntax)((PatternSyntax)package.Patterns[0]).Body;
                variationSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = variationSyntax.Children;
                Assert.AreEqual(children.Count, 5);
                TestTokenIdAndTextRange(pattern, children[0], TokenId.OpenCurlyBrace, "{");
                Assert.AreEqual(variationSyntax.Elements[0], children[1]);
                TestTokenIdAndTextRange(pattern, children[2], TokenId.Comma, ", ");
                Assert.AreEqual(variationSyntax.Elements[1], children[3]);
                TestTokenIdAndTextRange(pattern, children[4], TokenId.CloseCurlyBrace, "}");
            }
            void EmptyVariation()
            {
                string pattern = @"#Pattern = {};";
                PackageSyntax package = ParsePatterns(pattern);
                var variationSyntax = (VariationSyntax)((PatternSyntax)package.Patterns[0]).Body;
                variationSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = variationSyntax.Children;
                Assert.AreEqual(children.Count, 2);
                TestTokenIdAndTextRange(pattern, children[0], TokenId.OpenCurlyBrace, "{");
                TestTokenIdAndTextRange(pattern, children[1], TokenId.CloseCurlyBrace, "}");
            }
            VariationWithElements();
            EmptyVariation();
        }

        [TestMethod]
        public void WordSequenceSyntaxChildren()
        {
            string pattern = @"#Company = 'Nezaboodka' _* 'Software' _ 'LLC';";
            PackageSyntax package = ParsePatterns(pattern);
            var wordSequenceSyntax = (WordSequenceSyntax)((PatternSyntax)package.Patterns[0]).Body;
            wordSequenceSyntax.CreateChildren(pattern);
            ReadOnlyCollection<Syntax> children = wordSequenceSyntax.Children;
            Assert.AreEqual(children.Count, 5);
            Assert.AreEqual(wordSequenceSyntax.Elements[0], children[0]);
            TestSourceTextInformation(pattern, children[1], "_* ");
            Assert.AreEqual(wordSequenceSyntax.Elements[2], children[2]);
            TestSourceTextInformation(pattern, children[3], "_ ");
            Assert.AreEqual(wordSequenceSyntax.Elements[4], children[4]);
        }

        [TestMethod]
        public void WordSpanSyntaxChildren()
        {
            void WordSpanWithoutExtraction()
            {
                string pattern = "HtmlTitle = '<title>' .. [1-20] .. '</title>';";
                PackageSyntax package = ParsePatterns(pattern);
                var wordSpanSyntax = (WordSpanSyntax)((PatternSyntax)package.Patterns[0]).Body;
                wordSpanSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = wordSpanSyntax.Children;
                Assert.AreEqual(children.Count, 9);
                Assert.AreEqual(wordSpanSyntax.Left, children[0]);
                TestTokenIdAndTextRange(pattern, children[1], TokenId.DoublePeriod, ".. ");
                TestTokenIdAndTextRange(pattern, children[2], TokenId.OpenSquareBracket, "[");
                TestTokenIdAndTextRange(pattern, children[3], TokenId.IntegerLiteral, "1");
                TestTokenIdAndTextRange(pattern, children[4], TokenId.Minus, "-");
                TestTokenIdAndTextRange(pattern, children[5], TokenId.IntegerLiteral, "20");
                TestTokenIdAndTextRange(pattern, children[6], TokenId.CloseSquareBracket, "] ");
                TestTokenIdAndTextRange(pattern, children[7], TokenId.DoublePeriod, ".. ");
                Assert.AreEqual(wordSpanSyntax.Right, children[8]);
            }
            void WordSpanWithExtractionAndExclusion()
            {
                string pattern = "HtmlTitle(Title) = '<title>' .. Title: [1-20] ~'Exclusion' .. '</title>';";
                PackageSyntax package = ParsePatterns(pattern);
                var wordSpanSyntax = (WordSpanSyntax)((PatternSyntax)package.Patterns[0]).Body;
                wordSpanSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = wordSpanSyntax.Children;
                Assert.AreEqual(children.Count, 13);
                Assert.AreEqual(wordSpanSyntax.Left, children[0]);
                TestTokenIdAndTextRange(pattern, children[1], TokenId.DoublePeriod, ".. ");
                Assert.AreEqual(wordSpanSyntax.ExtractionOfSpan, children[2]);
                TestTokenIdAndTextRange(pattern, children[3], TokenId.Colon, ": ");
                TestTokenIdAndTextRange(pattern, children[4], TokenId.OpenSquareBracket, "[");
                TestTokenIdAndTextRange(pattern, children[5], TokenId.IntegerLiteral, "1");
                TestTokenIdAndTextRange(pattern, children[6], TokenId.Minus, "-");
                TestTokenIdAndTextRange(pattern, children[7], TokenId.IntegerLiteral, "20");
                TestTokenIdAndTextRange(pattern, children[8], TokenId.CloseSquareBracket, "] ");
                TestTokenIdAndTextRange(pattern, children[9], TokenId.Tilde, "~");
                Assert.AreEqual(wordSpanSyntax.Exclusion, children[10]);
                TestTokenIdAndTextRange(pattern, children[11], TokenId.DoublePeriod, ".. ");
                Assert.AreEqual(wordSpanSyntax.Right, children[12]);
            }
            WordSpanWithoutExtraction();
            WordSpanWithExtractionAndExclusion();
        }

        [TestMethod]
        public void ParenthesizedExpressionChildren()
        {
            void ParenthesizedExpressionInOptionality()
            {
                string pattern = @"#Company = ?('Optionality' + Space)";
                PackageSyntax package = ParsePatterns(pattern);
                var optionalitySyntax = (OptionalitySyntax)((PatternSyntax)package.Patterns[0]).Body;
                optionalitySyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = optionalitySyntax.Children;
                Assert.AreEqual(children.Count, 4);
                TestTokenIdAndTextRange(pattern, children[0], TokenId.Question, "?");
                TestTokenIdAndTextRange(pattern, children[1], TokenId.OpenParenthesis, "(");
                Assert.AreEqual(optionalitySyntax.Body, children[2]);
                TestTokenIdAndTextRange(pattern, children[3], TokenId.CloseParenthesis, ")");
            }
            void ParenthesizedExpressionInSequence()
            {
                string pattern = @"Pattern = (Word) + Num;";
                PackageSyntax package = ParsePatterns(pattern);
                var sequenceSyntax = (SequenceSyntax)((PatternSyntax)package.Patterns[0]).Body;
                sequenceSyntax.CreateChildren(pattern);
                ReadOnlyCollection<Syntax> children = sequenceSyntax.Children;
                Assert.AreEqual(children.Count, 5);
                TestTokenIdAndTextRange(pattern, children[0], TokenId.OpenParenthesis, "(");
                Assert.AreEqual(sequenceSyntax.Elements[0], children[1]);
                TestTokenIdAndTextRange(pattern, children[2], TokenId.CloseParenthesis, ") ");
                TestTokenIdAndTextRange(pattern, children[3], TokenId.Plus, "+ ");
                Assert.AreEqual(sequenceSyntax.Elements[1], children[4]);
            }
            ParenthesizedExpressionInOptionality();
            ParenthesizedExpressionInSequence();
        }
    }
}
