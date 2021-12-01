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
        public void PatternSearchTarget()
        {
            string patterns =
@"@search Pattern;

Pattern = 'Pattern';
";

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
@"@require 'Patterns.np';
@namespace Basic {
    Pattern1 = A + B @where {
        A = Word + {'Foo', ~'Bar'};
        B = [1+ Num(2-4) & 'Nezaboodka'];
    };

    Pattern2 = A _ B @where {
        A = 'Company' @outside Pattern1.A;
        B = Pattern1.B @having Alpha(2-10, TitleCase);
    };

    Pattern3 = 'Nezaboodka' + ? 'Company';
    @search @pattern Pattern4(X) = 'Fo'!* .. X: [0-2] ~'Excluded' .. 'Ba'!*(Alpha, 1, Lowercase);
    #Pattern5 = 'Hello' @inside ('Hello' + 'world');
    Pattern6(X, Y) = X: 'Nezaboodka' ... Y: 'Company';
    Pattern7(Q, S) = Pattern6(Q: X, S: Y);
    Pattern8(X) = [1+ X: Symbol + X];
    Pattern9(X, ~Y) = {X: Punct + X, Y: Symbol + Y};
    Pattern10 = 'Hello' + WordBreak + 'World' + WordBreak + '!';
    @search Pattern10;
}";
            var parser = new SyntaxParser();
            PackageSyntax package = parser.ParsePackageText(patterns);
            TestSourceTextInformation(patterns, package, patterns);
            void Require()
            {
                RequiredPackageSyntax require = package.RequiredPackages[0];
                TestSourceTextInformation(patterns, require,
@"@require 'Patterns.np';
");
            }
            void Pattern1()
            {
                var pattern = (PatternSyntax) package.Patterns[0];
                TestSourceTextInformation(patterns, pattern,
@"Pattern1 = A + B @where {
        A = Word + {'Foo', ~'Bar'};
        B = [1+ Num(2-4) & 'Nezaboodka'];
    };

    ");
                var sequence = (SequenceSyntax) pattern.Body;
                TestSourceTextInformation(patterns, sequence, @"A + B ");
                TestSourceTextInformation(patterns, sequence.Elements[0], @"A ");
                TestSourceTextInformation(patterns, sequence.Elements[1], @"B ");
                void PatternA()
                {
                    PatternSyntax patternA = pattern.NestedPatterns[0];
                    TestSourceTextInformation(patterns, patternA,
@"A = Word + {'Foo', ~'Bar'};
        ");
                    var sequence = (SequenceSyntax) patternA.Body;
                    TestSourceTextInformation(patterns, sequence, @"Word + {'Foo', ~'Bar'}");
                    TestSourceTextInformation(patterns, sequence.Elements[0], @"Word ");
                    var variation = (VariationSyntax) sequence.Elements[1];
                    TestSourceTextInformation(patterns, variation, @"{'Foo', ~'Bar'}");
                    TestSourceTextInformation(patterns, variation.Elements[0], @"'Foo'");
                    var exception = (ExceptionSyntax) variation.Elements[1];
                    TestSourceTextInformation(patterns, exception, @"~'Bar'");
                    TestSourceTextInformation(patterns, exception.Body, @"'Bar'");
                }
                void PatternB()
                {
                    PatternSyntax patternB = pattern.NestedPatterns[1];
                    TestSourceTextInformation(patterns, patternB,
@"B = [1+ Num(2-4) & 'Nezaboodka'];
    ");
                    var span = (SpanSyntax) patternB.Body;
                    TestSourceTextInformation(patterns, span, @"[1+ Num(2-4) & 'Nezaboodka']");
                    var repetition = (RepetitionSyntax) span.Elements[0];
                    TestSourceTextInformation(patterns, repetition, @"1+ Num(2-4) & 'Nezaboodka'");
                    var conjunction = (ConjunctionSyntax) repetition.Body;
                    TestSourceTextInformation(patterns, conjunction.Elements[0], @"Num(2-4) ");
                    TestSourceTextInformation(patterns, conjunction.Elements[1], @"'Nezaboodka'");
                }
                PatternA();
                PatternB();
            }
            void Pattern2()
            {
                var pattern2 = (PatternSyntax) package.Patterns[1];
                TestSourceTextInformation(patterns, pattern2,
@"Pattern2 = A _ B @where {
        A = 'Company' @outside Pattern1.A;
        B = Pattern1.B @having Alpha(2-10, TitleCase);
    };

    ");
                var wordSequence = (WordSequenceSyntax) pattern2.Body;
                TestSourceTextInformation(patterns, wordSequence.Elements[0], @"A ");
                TestSourceTextInformation(patterns, wordSequence.Elements[1], @"B ");
                void PatternA()
                {
                    PatternSyntax patternA = pattern2.NestedPatterns[0];
                    TestSourceTextInformation(patterns, patternA,
@"A = 'Company' @outside Pattern1.A;
        ");
                    var outside = (OutsideSyntax) patternA.Body;
                    TestSourceTextInformation(patterns, outside, @"'Company' @outside Pattern1.A");
                    TestSourceTextInformation(patterns, outside.Body, @"'Company' ");
                    TestSourceTextInformation(patterns, outside.Exception, @"Pattern1.A");
                }
                void PatternB()
                {
                    PatternSyntax patternB = pattern2.NestedPatterns[1];
                    TestSourceTextInformation(patterns, patternB,
@"B = Pattern1.B @having Alpha(2-10, TitleCase);
    ");
                    var having = (HavingSyntax)patternB.Body;
                    TestSourceTextInformation(patterns, having, @"Pattern1.B @having Alpha(2-10, TitleCase)");
                    TestSourceTextInformation(patterns, having.Outer, @"Pattern1.B ");
                    TestSourceTextInformation(patterns, having.Inner, @"Alpha(2-10, TitleCase)");
                }
                PatternA();
                PatternB();
            }
            void Pattern3()
            {
                var pattern3 = (PatternSyntax) package.Patterns[2];
                TestSourceTextInformation(patterns, pattern3,
@"Pattern3 = 'Nezaboodka' + ? 'Company';
    ");
                var sequence = (SequenceSyntax) pattern3.Body;
                TestSourceTextInformation(patterns, sequence, @"'Nezaboodka' + ? 'Company'");
                TestSourceTextInformation(patterns, sequence.Elements[0], @"'Nezaboodka' ");
                var optionality = (OptionalitySyntax) sequence.Elements[1];
                TestSourceTextInformation(patterns, optionality, @"? 'Company'");
                TestSourceTextInformation(patterns, optionality.Body, @"'Company'");
            }
            void Pattern4()
            {
                var pattern4 = (PatternSyntax) package.Patterns[3];
                TestSourceTextInformation(patterns, pattern4,
@"@search @pattern Pattern4(X) = 'Fo'!* .. X: [0-2] ~'Excluded' .. 'Ba'!*(Alpha, 1, Lowercase);
    ");
                var wordSpan = (WordSpanSyntax) pattern4.Body;
                TestSourceTextInformation(patterns, wordSpan, @"'Fo'!* .. X: [0-2] ~'Excluded' .. 'Ba'!*(Alpha, 1, Lowercase)");
                TestSourceTextInformation(patterns, wordSpan.Left, @"'Fo'!* ");
                TestSourceTextInformation(patterns, wordSpan.Right, @"'Ba'!*(Alpha, 1, Lowercase)");
                TestSourceTextInformation(patterns, wordSpan.ExtractionOfSpan, @"X");
                TestSourceTextInformation(patterns, wordSpan.Exclusion, @"'Excluded' ");
            }
            void Pattern5()
            {
                var pattern5 = (PatternSyntax) package.Patterns[4];
                TestSourceTextInformation(patterns, pattern5,
@"#Pattern5 = 'Hello' @inside ('Hello' + 'world');
    ");
                var inside = (InsideSyntax) pattern5.Body;
                TestSourceTextInformation(patterns, inside.Inner, @"'Hello' ");
                TestSourceTextInformation(patterns, inside.Outer, @"'Hello' + 'world'");
                var sequence = (SequenceSyntax) inside.Outer;
                TestSourceTextInformation(patterns, sequence.Elements[0], @"'Hello' ");
                TestSourceTextInformation(patterns, sequence.Elements[1], @"'world'");
            }
            void Pattern6()
            {
                var pattern6 = (PatternSyntax) package.Patterns[5];
                TestSourceTextInformation(patterns, pattern6,
@"Pattern6(X, Y) = X: 'Nezaboodka' ... Y: 'Company';
    ");
                TestSourceTextInformation(patterns, pattern6.Fields[0], @"X");
                TestSourceTextInformation(patterns, pattern6.Fields[1], @"Y");
                var anySpan = (AnySpanSyntax) pattern6.Body;
                TestSourceTextInformation(patterns, anySpan, @"X: 'Nezaboodka' ... Y: 'Company'");
                var xExtraction = (ExtractionSyntax) anySpan.Left;
                TestSourceTextInformation(patterns, xExtraction, @"X: 'Nezaboodka' ");
                TestSourceTextInformation(patterns, xExtraction.Body, @"'Nezaboodka' ");
                var yExtraction = (ExtractionSyntax) anySpan.Right;
                TestSourceTextInformation(patterns, yExtraction, @"Y: 'Company'");
                TestSourceTextInformation(patterns, yExtraction.Body, @"'Company'");
            }
            void Pattern7()
            {
                var pattern7 = (PatternSyntax) package.Patterns[6];
                TestSourceTextInformation(patterns, pattern7,
@"Pattern7(Q, S) = Pattern6(Q: X, S: Y);
    ");
                TestSourceTextInformation(patterns, pattern7.Fields[0], @"Q");
                TestSourceTextInformation(patterns, pattern7.Fields[1], @"S");
                var patternReference = (PatternReferenceSyntax) pattern7.Body;
                TestSourceTextInformation(patterns, patternReference, @"Pattern6(Q: X, S: Y)");
                TestSourceTextInformation(patterns, patternReference.ExtractionFromFields[0], @"Q: X");
                TestSourceTextInformation(patterns, patternReference.ExtractionFromFields[1], @"S: Y");
            }
            void Pattern8()
            {
                var pattern8 = (PatternSyntax) package.Patterns[7];
                TestSourceTextInformation(patterns, pattern8,
@"Pattern8(X) = [1+ X: Symbol + X];
    ");
                TestSourceTextInformation(patterns, pattern8.Fields[0], @"X");
                var span = (SpanSyntax) pattern8.Body;
                TestSourceTextInformation(patterns, span, @"[1+ X: Symbol + X]");
                var repetition = (RepetitionSyntax) span.Elements[0];
                TestSourceTextInformation(patterns, repetition, @"1+ X: Symbol + X");
                var sequence = (SequenceSyntax) repetition.Body;
                TestSourceTextInformation(patterns, sequence, @"X: Symbol + X");
                var extraction = (ExtractionSyntax) sequence.Elements[0];
                TestSourceTextInformation(patterns, extraction, @"X: Symbol ");
                TestSourceTextInformation(patterns, extraction.Body, @"Symbol ");
                TestSourceTextInformation(patterns, sequence.Elements[1], @"X");
            }
            void Pattern9()
            {
                var pattern9 = (PatternSyntax) package.Patterns[8];
                TestSourceTextInformation(patterns, pattern9,
@"Pattern9(X, ~Y) = {X: Punct + X, Y: Symbol + Y};
    ");
                TestSourceTextInformation(patterns, pattern9.Fields[0], @"X");
                TestSourceTextInformation(patterns, pattern9.Fields[1], @"~Y");
                var variation = (VariationSyntax) pattern9.Body;
                TestSourceTextInformation(patterns, variation, @"{X: Punct + X, Y: Symbol + Y}");
                var sequence1 = (SequenceSyntax) variation.Elements[0];
                TestSourceTextInformation(patterns, sequence1, @"X: Punct + X");
                var extractionX = (ExtractionSyntax) sequence1.Elements[0];
                TestSourceTextInformation(patterns, extractionX, @"X: Punct ");
                TestSourceTextInformation(patterns, extractionX.Body, @"Punct ");
                TestSourceTextInformation(patterns, sequence1.Elements[1], @"X");
                var sequence2 = (SequenceSyntax) variation.Elements[1];
                TestSourceTextInformation(patterns, sequence2, @"Y: Symbol + Y");
                var extractionY = (ExtractionSyntax) sequence2.Elements[0];
                TestSourceTextInformation(patterns, extractionY, @"Y: Symbol ");
                TestSourceTextInformation(patterns, extractionY.Body, @"Symbol ");
                TestSourceTextInformation(patterns, sequence2.Elements[1], @"Y");
            }
            void Pattern10()
            {
                var pattern10 = (PatternSyntax) package.Patterns[9];
                TestSourceTextInformation(patterns, pattern10,
@"Pattern10 = 'Hello' + WordBreak + 'World' + WordBreak + '!';
    ");
                var sequence = (SequenceSyntax) pattern10.Body;
                TestSourceTextInformation(patterns, sequence, @"'Hello' + WordBreak + 'World' + WordBreak + '!'");
                TestSourceTextInformation(patterns, sequence.Elements[0], @"'Hello' ");
                TestSourceTextInformation(patterns, sequence.Elements[1], @"WordBreak ");
                TestSourceTextInformation(patterns, sequence.Elements[2], @"'World' ");
                TestSourceTextInformation(patterns, sequence.Elements[3], @"WordBreak ");
                TestSourceTextInformation(patterns, sequence.Elements[4], @"'!'");
            }
            void PatternSearchTarget()
            {
                var patternSearchTarget = (SearchTargetSyntax) package.SearchTargets[0];
                TestSourceTextInformation(patterns, patternSearchTarget, @"@search Pattern10;
");
            }
            Require();
            Pattern1();
            Pattern2();
            Pattern3();
            Pattern4();
            Pattern5();
            Pattern6();
            Pattern7();
            Pattern8();
            Pattern9();
            Pattern10();
            PatternSearchTarget();
        }
    }
}
