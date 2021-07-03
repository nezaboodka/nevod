//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using static Nezaboodka.Nevod.Engine.Tests.TestHelper;

namespace Nezaboodka.Nevod.Engine.Tests
{
    // For convinience all text strings in tests are written in single quotes ('), not double quotes (").
    // The ParseAndToString function replaces double quotes with single quotes before comparison of results.
    [TestClass]
    [TestCategory("Syntax"), TestCategory("SyntaxParser")]
    public class SyntaxParserTests
    {
        [TestMethod]
        public void SimplestPatternParsing()
        {
            string pattern = "TheWord = Word;";
            var parser = new SyntaxParser();
            PackageSyntax package = parser.ParsePackageText(pattern);
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void SimplestPatternWithMultipartNameParsing()
        {
            string pattern = "The.Word = Word;";
            var parser = new SyntaxParser();
            PackageSyntax package = parser.ParsePackageText(pattern);
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void TargetPatternParsing()
        {
            string pattern = "#TheWord = Word;";
            var parser = new SyntaxParser();
            PackageSyntax package = parser.ParsePackageText(pattern);
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void SingleLineCommentSyntaxParsing()
        {
            string pattern = "#TheWord = Word;";
            string patternWithComment = pattern + "//The rest is comment";
            var parser = new SyntaxParser();
            PackageSyntax package = parser.ParsePackageText(patternWithComment);
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void MultiLineCommentSyntaxParsing()
        {
            string pattern = "#TheWord = Word;";
            string patternWithComments = "/*TheSpace = Space; //slash */" + pattern + "/**/\n" + "\n/*\n*/";
            var parser = new SyntaxParser();
            PackageSyntax package = parser.ParsePackageText(patternWithComments);
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void WordAttributesParsing()
        {
            string pattern = "TheWord = Word(3-5, Uppercase);";
            var parser = new SyntaxParser();
            PackageSyntax package = parser.ParsePackageText(pattern);
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void WordEmptyAttributesParsing()
        {
            string pattern = "TheWord = Word();";
            var parser = new SyntaxParser();
            PackageSyntax package = parser.ParsePackageText(pattern);
            string text = package.ToString();
            TestPackageIs(pattern.Replace("Word()", "Word"), text);
        }

        [TestMethod]
        public void WordAlphaAttributesParsing()
        {
            string pattern = "TheWord = Alpha(3-5, Uppercase);";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void WordNumAttributesParsing()
        {
            string pattern = "TheWord = Num(3-5, Uppercase);";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void WordAlphaNumAttributesParsing()
        {
            string pattern = "TheWord = AlphaNum(3-5, Uppercase);";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void WordNumAlphaAttributesParsing()
        {
            string pattern = "TheWord = NumAlpha(3-5, Uppercase);";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void TextPrefixParsing()
        {
            string pattern = "TheWord = 'Neza'!*;";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void TextPrefixAttributesParsing()
        {
            string pattern = "TheWord = 'Neza'!*(Alpha, 3-6, Uppercase);";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void TextPrefixWithSuffixLengthParsing()
        {
            string pattern = "TheWord = 'Neza'!*(3-6);";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void TextSymbolParsing()
        {
            string pattern = "Name = '#' + Symbol + '@' + '.';";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void SequenceSyntaxParsing()
        {
            string pattern = "Name = Alpha + Space + Alpha;";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void WordSequenceSyntaxParsing()
        {
            string pattern = "NameSurname = Word _ Word;";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void WordSequenceOfSequencesParsing()
        {
            string pattern = "PostalAddress = Num + '-' + Word _ Word + Space + 'street';";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void AnySpanParsing()
        {
            string pattern = "Html = '<html>' ... '</html>';";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void AnySpanWithExtractionParsing()
        {
            string pattern = "Html(X) = '<html>' .. X .. '</html>';";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void WordsAtZeroDistanceSyntaxParsing()
        {
            string pattern = "Name = Alpha ... Num ... AlphaNum;";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void WordsAtZeroDistanceSyntaxPriorityParsing()
        {
            string pattern = "Name = Alpha ... (Num ... AlphaNum);";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void WordsAtVariableDistanceSyntaxParsing()
        {
            string pattern = "Name = Alpha .. [0-5] .. Num .. [1+] .. AlphaNum;";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void WordsAtFixedDistanceSyntaxParsing()
        {
            string pattern = "Name = Alpha .. [5] .. Num;";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void WordSpanWithExclusiveElementParsing()
        {
            string pattern = "GoogleCorp = 'Google' .. [0-5] ~{'Drive', 'Mail', 'AdWords'} .. 'Corp';";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void WordSpanWithExtractionParsing()
        {
            string pattern = "GoogleCorp(S) = 'Google' .. S:[0-5] .. 'Corp';";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void WordSpanWithExclusiveElementAndExtractionParsing()
        {
            string pattern = "GoogleCorp(S) = 'Google' .. S:[0-5] ~{'Drive', 'Mail', 'AdWords'} .. 'Corp';";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void ConjunctionSyntaxParsing()
        {
            string pattern = "Pattern = Alpha & Num & AlphaNum;";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void VariationSyntaxParsing()
        {
            string pattern = "Cipher = {Alpha, AlphaNum, NumAlpha, Num, ~Symbol};";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void EndlessFromZeroRepetitionSyntaxParsing()
        {
            string pattern = "TheBlanks = [0+ {Space, LineBreak}];";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void OptionalRepetitionSyntaxParsing()
        {
            string pattern = "TheBlanks = ? {Space, LineBreak};";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void ExactRepetitionSyntaxParsing()
        {
            string pattern = "TheBlanks = [3 {Space, LineBreak}];";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void EndlessFromOneRepetitionSyntaxParsing()
        {
            string pattern = "TheBlanks = [1+ {Space, LineBreak}];";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void TokenSyntaxParsing()
        {
            string pattern = "Nezaboodka = 'Nezaboodka'! ... 'Software';";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void PatternWithFieldsSyntaxParsing()
        {
            string pattern = "Relation(Subj, Verb, Obj) = Subj: Word ... Verb: Word ... Obj: Word;";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void PatternReferenceSyntaxParsing()
        {
            string pattern = "Acquisition(Who, Whom) = Who: Subject ... 'acquire' ... Whom: Object;";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void FieldReferenceParsing()
        {
            string pattern = "#Pattern(X, Y, Z) = X: Word _ Y: [0+ Z: Word _ Z] _ X _ Y;";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void FieldReferenceInRepetitionParsing()
        {
            string pattern = "#Pattern(X) = X: Word _ [0+ Word _ X];";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void InsideSyntaxParsing()
        {
            string pattern = "PrimaryKeyword = Keyword @inside Sentence @inside Paragraph;";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void ParenthesisSyntaxParsing()
        {
            string pattern = "Name = [1-5 Word ... Alpha] + Space + (Alpha ... Word);";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void InsideInVariationSyntaxParsing()
        {
            string pattern = "PrimaryKeyword = {Keyword @inside Sentence, AnotherKeyword @inside AnotherPattern};";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void ExclusiveInsideInVariationSyntaxParsing()
        {
            string pattern = "PrimaryKeyword = {Keyword @inside Sentence, ~AnotherKeyword @inside AnotherPattern};";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void OperationPriorityParsing()
        {
            string pattern = "#Pattern = P1 @inside P2 ... P3 + P4 & ? {P5, P6} @inside P7;";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void IdentifierPatternParsing()
        {
            string pattern = "Identifier = {Alpha, AlphaNum, '_'} + [0+ {Word, '_'}];";
            TestParseAndToString(pattern);
        }

        [TestMethod]
        public void EmailPatternParsing()
        {
            string patterns = "#Email = Word + [0+ {Word, '.', '_', '+'}] + '@' + Word + [0+ {Word, '.', '_'}];";
            TestParseAndToString(patterns);
        }

        [TestMethod]
        public void UrlPatternParsing()
        {
            string patterns =
@"#Url = {'http', 'https'} + '://' + Domain + ? Path + ? Query;
Domain = Word + [1+ '.' + Word + [0+ {Word, '_', '-'}]];
Path = '/' + [0+ {Word, '/', '_', '+', '-', '%'}];
Query = '?' + ? (QueryParam + [0+ '&' + QueryParam]);
QueryParam = Identifier + '=' + Identifier;
Identifier = {Alpha, AlphaNum, '_'} + [0+ {Word, '_'}];";
            TestParseAndToString(patterns);
        }

        [TestMethod]
        public void UrlFieldsPatternParsing()
        {
            string patterns =
@"#Url(Domain, Path) = {'http', 'https'} + '://' + Domain: Url-Domain + Path: ? Url-Path + ? Query;
Url-Domain = Word + [1+ '.' + Word + [0+ {Word, '_', '-'}]];
Url-Path = '/' + [0+ {Word, '/', '_', '+', '-', '%'}];
Query = '?' + ? (QueryParam + [0+ '&' + QueryParam]);
QueryParam = Identifier + '=' + Identifier;
Identifier = {Alpha, AlphaNum, '_'} + [0+ {Word, '_'}];";
            TestParseAndToString(patterns);
        }

        [TestMethod]
        public void NamespaceWithPatternParsing()
        {
            string patterns =
@"@namespace Package
{
    @pattern #Pattern1 = Alpha;
    @pattern #Pattern2 = AlphaNum;
}
";
            TestParseAndToString(patterns);
        }

        [TestMethod]
        public void NamespacesWithPatternsParsing()
        {
            string patterns =
@"@namespace Namespace1
{
    @pattern #Pattern1 = Alpha;
    @pattern #Pattern2 = AlphaNum;
}

@namespace Namespace2
{
    @pattern #Pattern1 = Alpha;
    @pattern #Pattern2 = AlphaNum;
}

@pattern #Pattern1 = Alpha;
@pattern #Pattern2 = AlphaNum;
";
            TestParseAndToString(patterns);
        }

        [TestMethod]
        public void PatternWithNetsedPatternsParsing()
        {
            string patterns =
@"@pattern #Pattern1 = Pattern1-1
@where
{
    @pattern #Pattern1-1 = Pattern1-2-1
    @where
    {
        @pattern Pattern1-2-1 = Alpha;
    };
};
";
            TestParseAndToString(patterns);
        }

        [TestMethod]
        public void PatternWithNetsedPatternsWithMultipartNameParsing()
        {
            string patterns =
@"#P = The.Pattern
@where
{
    #The.Pattern = 'test';
};
";
            TestParseAndToString(patterns);
        }

        [TestMethod]
        public void NamespaceWithNetsedPatternsParsing()
        {
            string patterns =
@"@namespace Namespace1
{
    @pattern #Pattern1 = Pattern1-1 + Pattern1-2
    @where
    {
        @pattern #Pattern1-1 = Alpha + Pattern1-2;
        @pattern #Pattern1-2 = Pattern1-2-1
        @where
        {
            @pattern Pattern1-2-1 = Space;
        };
    };
}
";
            TestParseAndToString(patterns);
        }

        [TestMethod]
        public void PatternSyntaxInNamespace()
        {
            string patterns =
@"@namespace Test
{
    P = 'pattern';
    @pattern T = 'test';
}";
            TestParseAndToString(patterns);
        }

        [TestMethod]
        public void ParseAsPatternBody()
        {
            string pattern = "Start + Word";
            var parser = new SyntaxParser();
            PackageSyntax package = parser.ParseExpressionText(pattern);
            string text = package.ToString();
            TestPackageIs("#Pattern = " + pattern + ";", text);
        }

        [TestMethod]
        public void PackageSourceTextInformation()
        {
            string patterns =
@"@namespace Basic {
    Pattern1 = A + B @where {
        A = Word + {'Foo', ~'Bar'};
        B = [1+ Num(2-4) & 'Nezaboodka'];
    };
    
    Pattern2 = A _ B @where {
        A = 'Company' @outside Pattern1.A;
        B = Pattern1.B @having Alpha(2-10, TitleCase);
    };

    Pattern3 = 'Nezaboodka' + ? 'Company';
    @search @pattern Pattern4 = 'Fo'!* .. [0-2] .. 'Ba'!*(Alpha, 1, Lowercase);
    #Pattern5 = 'Hello' @inside ('Hello' + 'world');
    Pattern6(X, Y) = X: 'Nezaboodka' ... Y: 'Company';
    Pattern7(Q, S) = Pattern6(Q: X, S: Y);
    Pattern8(X) = [1+ X: Symbol + X];
    Pattern9(X, ~Y) = {X: Punct + X, Y: Symbol + Y};
}";
            var parser = new SyntaxParser();
            PackageSyntax package = parser.ParsePackageText(patterns);
            void Pattern1()
            {
                var pattern = (PatternSyntax) package.Patterns[0];
                TestSourceTextInformation(pattern, 1, 4, 4, 5,
@"Pattern1 = A + B @where {
        A = Word + {'Foo', ~'Bar'};
        B = [1+ Num(2-4) & 'Nezaboodka'];
    };");
                var sequence = (SequenceSyntax) pattern.Body;
                TestSourceTextInformation(sequence.Elements[0], 1, 15, 1, 15, "A");
                TestSourceTextInformation(sequence.Elements[1], 1, 19, 1, 19, "B");
                void PatternA()
                {
                    PatternSyntax patternA = pattern.NestedPatterns[0];
                    TestSourceTextInformation(patternA, 2, 8, 2, 34, "A = Word + {'Foo', ~'Bar'};");
                    var sequence = (SequenceSyntax) patternA.Body;
                    TestSourceTextInformation(sequence.Elements[0], 2, 12, 2, 15, "Word");
                    var variation = (VariationSyntax) sequence.Elements[1];
                    TestSourceTextInformation(variation, 2, 19, 2, 33, "{'Foo', ~'Bar'}");
                    TestSourceTextInformation(variation.Elements[0], 2, 20, 2, 24, "'Foo'");
                    var exception = (ExceptionSyntax) variation.Elements[1];
                    TestSourceTextInformation(exception, 2, 27, 2, 32, "~'Bar'");
                    TestSourceTextInformation(exception.Body, 2, 28, 2, 32, "'Bar'");
                }
                void PatternB()
                {
                    PatternSyntax patternB = pattern.NestedPatterns[1];
                    TestSourceTextInformation(patternB, 3, 8, 3, 40, "B = [1+ Num(2-4) & 'Nezaboodka'];");
                    var span = (SpanSyntax) patternB.Body;
                    TestSourceTextInformation(span, 3, 12, 3, 39, "[1+ Num(2-4) & 'Nezaboodka']");
                    var repetition = (RepetitionSyntax) span.Elements[0];
                    TestSourceTextInformation(repetition, 3, 13, 3, 38, "1+ Num(2-4) & 'Nezaboodka'");
                    var conjunction = (ConjunctionSyntax) repetition.Body;
                    TestSourceTextInformation(conjunction.Elements[0], 3, 16, 3, 23, "Num(2-4)");
                    TestSourceTextInformation(conjunction.Elements[1], 3, 27, 3, 38, "'Nezaboodka'");
                }
                PatternA();
                PatternB();
            }
            void Pattern2()
            {
                var pattern2 = (PatternSyntax) package.Patterns[1];
                TestSourceTextInformation(pattern2, 6, 4, 9, 5, 
@"Pattern2 = A _ B @where {
        A = 'Company' @outside Pattern1.A;
        B = Pattern1.B @having Alpha(2-10, TitleCase);
    };");
                var wordSequence = (WordSequenceSyntax) pattern2.Body;
                TestSourceTextInformation(wordSequence.Elements[0], 6, 15, 6, 15, "A");
                TestSourceTextInformation(wordSequence.Elements[1], 6, 19, 6, 19, "B");
                void PatternA()
                {
                    PatternSyntax patternA = pattern2.NestedPatterns[0];
                    TestSourceTextInformation(patternA, 7, 8, 7, 41, "A = 'Company' @outside Pattern1.A;");
                    var outside = (OutsideSyntax) patternA.Body;
                    TestSourceTextInformation(outside, 7, 12, 7, 40, "'Company' @outside Pattern1.A");
                    TestSourceTextInformation(outside.Body, 7, 12, 7, 20, "'Company'");
                    TestSourceTextInformation(outside.Exception, 7, 31, 7, 40, "Pattern1.A");
                }
                void PatternB()
                {
                    PatternSyntax patternB = pattern2.NestedPatterns[1];
                    TestSourceTextInformation(patternB, 8, 8, 8, 53, "B = Pattern1.B @having Alpha(2-10, TitleCase);");
                    var having = (HavingSyntax)patternB.Body;
                    TestSourceTextInformation(having, 8, 12, 8, 52, "Pattern1.B @having Alpha(2-10, TitleCase)");
                    TestSourceTextInformation(having.Outer, 8, 12, 8, 21, "Pattern1.B");
                    TestSourceTextInformation(having.Inner, 8, 31, 8, 52, "Alpha(2-10, TitleCase)");
                }
                PatternA();
                PatternB();
            }
            void Pattern3()
            {
                var pattern3 = (PatternSyntax) package.Patterns[2];
                TestSourceTextInformation(pattern3, 11, 4, 11, 41, "Pattern3 = 'Nezaboodka' + ? 'Company';");
                var sequence = (SequenceSyntax) pattern3.Body;
                TestSourceTextInformation(sequence, 11, 15, 11, 40, "'Nezaboodka' + ? 'Company'");
                TestSourceTextInformation(sequence.Elements[0], 11, 15, 11, 26, "'Nezaboodka'");
                var optionality = (OptionalitySyntax) sequence.Elements[1];
                TestSourceTextInformation(optionality, 11, 30, 11, 40, "? 'Company'");
                TestSourceTextInformation(optionality.Body, 11, 32, 11, 40, "'Company'");
            }
            void Pattern4()
            {
                var pattern4 = (PatternSyntax) package.Patterns[3];
                TestSourceTextInformation(pattern4, 12, 4, 12, 78, "@search @pattern Pattern4 = 'Fo'!* .. [0-2] .. 'Ba'!*(Alpha, 1, Lowercase);");
                var wordSpan = (WordSpanSyntax) pattern4.Body;
                TestSourceTextInformation(wordSpan, 12, 32, 12, 77, "'Fo'!* .. [0-2] .. 'Ba'!*(Alpha, 1, Lowercase)");
                TestSourceTextInformation(wordSpan.Left, 12, 32, 12, 37, "'Fo'!*");
                TestSourceTextInformation(wordSpan.Right, 12, 51, 12, 77, "'Ba'!*(Alpha, 1, Lowercase)");
            }
            void Pattern5()
            {
                var pattern5 = (PatternSyntax) package.Patterns[4];
                TestSourceTextInformation(pattern5, 13, 4, 13, 51, "#Pattern5 = 'Hello' @inside ('Hello' + 'world');");
                var inside = (InsideSyntax) pattern5.Body;
                TestSourceTextInformation(inside.Inner, 13, 16, 13, 22, "'Hello'");
                TestSourceTextInformation(inside.Outer, 13, 33, 13, 49, "'Hello' + 'world'");
                var sequence = (SequenceSyntax) inside.Outer;
                TestSourceTextInformation(sequence.Elements[0], 13, 33, 13, 39, "'Hello'");
                TestSourceTextInformation(sequence.Elements[1], 13, 43, 13, 49, "'world'");   
            }
            void Pattern6()
            {
                var pattern6 = (PatternSyntax) package.Patterns[5];
                TestSourceTextInformation(pattern6, 14, 4, 14, 53, "Pattern6(X, Y) = X: 'Nezaboodka' ... Y: 'Company';");
                TestSourceTextInformation(pattern6.Fields[0], 14, 13, 14, 13, "X");
                TestSourceTextInformation(pattern6.Fields[1], 14, 16, 14, 16, "Y");
                var anySpan = (AnySpanSyntax) pattern6.Body;
                TestSourceTextInformation(anySpan, 14, 21, 14, 52, "X: 'Nezaboodka' ... Y: 'Company'");
                var xExtraction = (ExtractionSyntax) anySpan.Left;
                TestSourceTextInformation(xExtraction, 14, 21, 14, 35, "X: 'Nezaboodka'");
                TestSourceTextInformation(xExtraction.Body, 14, 24, 14, 35, "'Nezaboodka'");
                var yExtraction = (ExtractionSyntax) anySpan.Right;
                TestSourceTextInformation(yExtraction, 14, 41, 14, 52, "Y: 'Company'");
                TestSourceTextInformation(yExtraction.Body, 14, 44, 14, 52, "'Company'");
            }
            void Pattern7()
            {
                var pattern7 = (PatternSyntax) package.Patterns[6];
                TestSourceTextInformation(pattern7, 15, 4, 15, 41, "Pattern7(Q, S) = Pattern6(Q: X, S: Y);");
                TestSourceTextInformation(pattern7.Fields[0], 15, 13, 15, 13, "Q");
                TestSourceTextInformation(pattern7.Fields[1], 15, 16, 15, 16, "S");
                var patternReference = (PatternReferenceSyntax) pattern7.Body;
                TestSourceTextInformation(patternReference, 15, 21, 15, 40, "Pattern6(Q: X, S: Y)");
                var extractionQ = (ExtractionFromFieldSyntax) patternReference.ExtractionFromFields[0];
                TestSourceTextInformation(extractionQ, 15, 30, 15, 33, "Q: X");
                var extractionS = (ExtractionFromFieldSyntax) patternReference.ExtractionFromFields[1];
                TestSourceTextInformation(extractionS, 15, 36, 15, 39, "S: Y");
            }
            void Pattern8()
            {
                var pattern8 = (PatternSyntax) package.Patterns[7];
                TestSourceTextInformation(pattern8, 16, 4, 16, 36, "Pattern8(X) = [1+ X: Symbol + X];");
                TestSourceTextInformation(pattern8.Fields[0], 16, 13, 16, 13, "X");
                var span = (SpanSyntax) pattern8.Body;
                TestSourceTextInformation(span, 16, 18, 16, 35, "[1+ X: Symbol + X]");
                var repetition = (RepetitionSyntax) span.Elements[0];
                TestSourceTextInformation(repetition, 16, 19, 16, 34, "1+ X: Symbol + X");
                var sequence = (SequenceSyntax) repetition.Body;
                TestSourceTextInformation(sequence, 16, 22, 16, 34, "X: Symbol + X");
                var extraction = (ExtractionSyntax) sequence.Elements[0];
                TestSourceTextInformation(extraction, 16, 22, 16, 30, "X: Symbol");
                TestSourceTextInformation(extraction.Body, 16, 25, 16, 30, "Symbol");
                Assert.AreEqual("X", extraction.FieldName);
                TestSourceTextInformation(sequence.Elements[1], 16, 34, 16, 34, "X");
            }
            void Pattern9()
            {
                var pattern9 = (PatternSyntax) package.Patterns[8];
                TestSourceTextInformation(pattern9, 17, 4, 17, 51, "Pattern9(X, ~Y) = {X: Punct + X, Y: Symbol + Y};");
                TestSourceTextInformation(pattern9.Fields[0], 17, 13, 17, 13, "X");
                TestSourceTextInformation(pattern9.Fields[1], 17, 16, 17, 17, "~Y");
                var variation = (VariationSyntax) pattern9.Body;
                TestSourceTextInformation(variation, 17, 22, 17, 50, "{X: Punct + X, Y: Symbol + Y}");
                var sequence1 = (SequenceSyntax) variation.Elements[0];
                TestSourceTextInformation(sequence1, 17, 23, 17, 34, "X: Punct + X");
                var extractionX = (ExtractionSyntax) sequence1.Elements[0];
                TestSourceTextInformation(extractionX, 17, 23, 17, 30, "X: Punct");
                TestSourceTextInformation(extractionX.Body, 17, 26, 17, 30, "Punct");
                TestSourceTextInformation(sequence1.Elements[1], 17, 34, 17, 34, "X");
                var sequence2 = (SequenceSyntax) variation.Elements[1];
                TestSourceTextInformation(sequence2, 17, 37, 17, 49, "Y: Symbol + Y");
                var extractionY = (ExtractionSyntax) sequence2.Elements[0];
                TestSourceTextInformation(extractionY, 17, 37, 17, 45, "Y: Symbol");
                TestSourceTextInformation(extractionY.Body, 17, 40, 17, 45, "Symbol");
                TestSourceTextInformation(sequence2.Elements[1], 17, 49, 17, 49, "Y");
            }
            Pattern1();
            Pattern2();
            Pattern3();
            Pattern4();
            Pattern5();
            Pattern6();
            Pattern7();
            Pattern8();
            Pattern9();
        }
    }
}
