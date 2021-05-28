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
    }
}
