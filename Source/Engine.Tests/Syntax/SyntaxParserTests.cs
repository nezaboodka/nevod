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
    Pattern10 = 'Hello' + WordBreak + 'World' + WordBreak + '!';
}".Replace("\r\n", "\n"); // Use \n as line feeds to make positions platform-independent
            var parser = new SyntaxParser();
            PackageSyntax package = parser.ParsePackageText(patterns);
            TestSourceTextInformation(package, 0, 706);
            void Pattern1()
            {
                var pattern = (PatternSyntax) package.Patterns[0];
                TestSourceTextInformation(pattern, 23, 133);
                var sequence = (SequenceSyntax) pattern.Body;
                TestSourceTextInformation(sequence, 34, 39);
                TestSourceTextInformation(sequence.Elements[0], 34, 35);
                TestSourceTextInformation(sequence.Elements[1], 38, 39);
                void PatternA()
                {
                    PatternSyntax patternA = pattern.NestedPatterns[0];
                    TestSourceTextInformation(patternA, 57, 84);
                    var sequence = (SequenceSyntax) patternA.Body;
                    TestSourceTextInformation(sequence, 61, 83);
                    TestSourceTextInformation(sequence.Elements[0], 61, 65);
                    var variation = (VariationSyntax) sequence.Elements[1];
                    TestSourceTextInformation(variation, 68, 83);
                    TestSourceTextInformation(variation.Elements[0], 69, 74);
                    var exception = (ExceptionSyntax) variation.Elements[1];
                    TestSourceTextInformation(exception, 76, 82);
                    TestSourceTextInformation(exception.Body, 77, 82);
                }
                void PatternB()
                {
                    PatternSyntax patternB = pattern.NestedPatterns[1];
                    TestSourceTextInformation(patternB, 93, 126);
                    var span = (SpanSyntax) patternB.Body;
                    TestSourceTextInformation(span, 97, 125);
                    var repetition = (RepetitionSyntax) span.Elements[0];
                    TestSourceTextInformation(repetition, 98, 124);
                    var conjunction = (ConjunctionSyntax) repetition.Body;
                    TestSourceTextInformation(conjunction.Elements[0], 101, 109);
                    TestSourceTextInformation(conjunction.Elements[1], 112, 124);
                }
                PatternA();
                PatternB();
            }
            void Pattern2()
            {
                var pattern2 = (PatternSyntax) package.Patterns[1];
                TestSourceTextInformation(pattern2, 143, 273);
                var wordSequence = (WordSequenceSyntax) pattern2.Body;
                TestSourceTextInformation(wordSequence.Elements[0], 154, 155);
                TestSourceTextInformation(wordSequence.Elements[1], 158, 159);
                void PatternA()
                {
                    PatternSyntax patternA = pattern2.NestedPatterns[0];
                    TestSourceTextInformation(patternA, 177,  211);
                    var outside = (OutsideSyntax) patternA.Body;
                    TestSourceTextInformation(outside, 181, 210);
                    TestSourceTextInformation(outside.Body, 181, 190);
                    TestSourceTextInformation(outside.Exception, 200, 210);
                }
                void PatternB()
                {
                    PatternSyntax patternB = pattern2.NestedPatterns[1];
                    TestSourceTextInformation(patternB, 220, 266);
                    var having = (HavingSyntax)patternB.Body;
                    TestSourceTextInformation(having, 224, 265);
                    TestSourceTextInformation(having.Outer, 224, 234);
                    TestSourceTextInformation(having.Inner, 243, 265);
                }
                PatternA();
                PatternB();
            }
            void Pattern3()
            {
                var pattern3 = (PatternSyntax) package.Patterns[2];
                TestSourceTextInformation(pattern3, 279, 317);
                var sequence = (SequenceSyntax) pattern3.Body;
                TestSourceTextInformation(sequence, 290, 316);
                TestSourceTextInformation(sequence.Elements[0], 290, 302);
                var optionality = (OptionalitySyntax) sequence.Elements[1];
                TestSourceTextInformation(optionality, 305, 316);
                TestSourceTextInformation(optionality.Body, 307, 316);
            }
            void Pattern4()
            {
                var pattern4 = (PatternSyntax) package.Patterns[3];
                TestSourceTextInformation(pattern4, 322, 397);
                var wordSpan = (WordSpanSyntax) pattern4.Body;
                TestSourceTextInformation(wordSpan, 350, 396);
                TestSourceTextInformation(wordSpan.Left, 350, 356);
                TestSourceTextInformation(wordSpan.Right, 369, 396);
            }
            void Pattern5()
            {
                var pattern5 = (PatternSyntax) package.Patterns[4];
                TestSourceTextInformation(pattern5, 402, 450);
                var inside = (InsideSyntax) pattern5.Body;
                TestSourceTextInformation(inside.Inner, 414, 421);
                TestSourceTextInformation(inside.Outer, 431, 448);
                var sequence = (SequenceSyntax) inside.Outer;
                TestSourceTextInformation(sequence.Elements[0], 431, 438);
                TestSourceTextInformation(sequence.Elements[1], 441, 448);   
            }
            void Pattern6()
            {
                var pattern6 = (PatternSyntax) package.Patterns[5];
                TestSourceTextInformation(pattern6, 455, 505);
                TestSourceTextInformation(pattern6.Fields[0], 464, 465);
                TestSourceTextInformation(pattern6.Fields[1], 467, 468);
                var anySpan = (AnySpanSyntax) pattern6.Body;
                TestSourceTextInformation(anySpan, 472, 504);
                var xExtraction = (ExtractionSyntax) anySpan.Left;
                TestSourceTextInformation(xExtraction, 472, 487);
                TestSourceTextInformation(xExtraction.Body, 475, 487);
                var yExtraction = (ExtractionSyntax) anySpan.Right;
                TestSourceTextInformation(yExtraction, 492, 504);
                TestSourceTextInformation(yExtraction.Body, 495, 504);
            }
            void Pattern7()
            {
                var pattern7 = (PatternSyntax) package.Patterns[6];
                TestSourceTextInformation(pattern7, 510, 548);
                TestSourceTextInformation(pattern7.Fields[0], 519, 520);
                TestSourceTextInformation(pattern7.Fields[1], 522, 523);
                var patternReference = (PatternReferenceSyntax) pattern7.Body;
                TestSourceTextInformation(patternReference, 527, 547);
                TestSourceTextInformation(patternReference.ExtractionFromFields[0], 536, 540);
                TestSourceTextInformation(patternReference.ExtractionFromFields[1], 542, 546);
            }
            void Pattern8()
            {
                var pattern8 = (PatternSyntax) package.Patterns[7];
                TestSourceTextInformation(pattern8, 553, 586);
                TestSourceTextInformation(pattern8.Fields[0], 562, 563);
                var span = (SpanSyntax) pattern8.Body;
                TestSourceTextInformation(span, 567, 585);
                var repetition = (RepetitionSyntax) span.Elements[0];
                TestSourceTextInformation(repetition, 568, 584);
                var sequence = (SequenceSyntax) repetition.Body;
                TestSourceTextInformation(sequence, 571, 584);
                var extraction = (ExtractionSyntax) sequence.Elements[0];
                TestSourceTextInformation(extraction, 571, 580);
                TestSourceTextInformation(extraction.Body, 574, 580);
                Assert.AreEqual("X", extraction.FieldName);
                TestSourceTextInformation(sequence.Elements[1], 583, 584);
            }
            void Pattern9()
            {
                var pattern9 = (PatternSyntax) package.Patterns[8];
                TestSourceTextInformation(pattern9, 591, 639);
                TestSourceTextInformation(pattern9.Fields[0], 600, 601);
                TestSourceTextInformation(pattern9.Fields[1], 603, 605);
                var variation = (VariationSyntax) pattern9.Body;
                TestSourceTextInformation(variation, 609, 638);
                var sequence1 = (SequenceSyntax) variation.Elements[0];
                TestSourceTextInformation(sequence1, 610, 622);
                var extractionX = (ExtractionSyntax) sequence1.Elements[0];
                TestSourceTextInformation(extractionX, 610, 618);
                TestSourceTextInformation(extractionX.Body, 613, 618);
                TestSourceTextInformation(sequence1.Elements[1], 621, 622);
                var sequence2 = (SequenceSyntax) variation.Elements[1];
                TestSourceTextInformation(sequence2, 624, 637);
                var extractionY = (ExtractionSyntax) sequence2.Elements[0];
                TestSourceTextInformation(extractionY, 624, 633);
                TestSourceTextInformation(extractionY.Body, 627, 633);
                TestSourceTextInformation(sequence2.Elements[1], 636, 637);
            }
            void Pattern10()
            {
                var pattern10 = (PatternSyntax) package.Patterns[9];
                TestSourceTextInformation(pattern10, 644, 704);
                var sequence = (SequenceSyntax) pattern10.Body;
                TestSourceTextInformation(sequence, 656, 703);
                TestSourceTextInformation(sequence.Elements[0], 656, 663);
                TestSourceTextInformation(sequence.Elements[1], 666, 675);
                TestSourceTextInformation(sequence.Elements[2], 678, 685);
                TestSourceTextInformation(sequence.Elements[3], 688, 697);
                TestSourceTextInformation(sequence.Elements[4], 700, 703);
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
            Pattern10();
        }
    }
}
