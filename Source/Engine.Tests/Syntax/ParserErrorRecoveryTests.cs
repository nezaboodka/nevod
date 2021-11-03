using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Nezaboodka.Nevod.Engine.Tests.ErrorRecoveryTestHelper;

namespace Nezaboodka.Nevod.Engine.Tests
{
    [TestClass]
    [TestCategory("Syntax"), TestCategory("SyntaxParser"), TestCategory("Parser error recovery"), TestCategory("Error recovery")]
    public class ParserErrorRecoveryTests
    {
        [TestMethod]
        public void MissingPatternParts()
        {
            ParseAndCompareErrors(
                patterns: "= Word;",
                CreateExpectedError(invalidToken: "=", TextResource.PatternNameExpected));
            ParseAndCompareErrors(
                patterns: "P1 Num;",
                CreateExpectedError(invalidToken: "Num", TextResource.EqualSignExpectedInPatternDefinition));
            ParseAndCompareErrors(
                patterns: "P2 = ;",
                CreateExpectedError(invalidToken: ";", TextResource.ExpressionExpected));
            ParseAndCompareErrors(
                patterns: "P3 = Symbol ",
                // Space after Symbol
                CreateExpectedError(errorStart: 11, errorLength: 1, TextResource.PatternShouldEndWithSemicolon));
        }

        [TestMethod]
        public void MisplacedSemicolonInPattern()
        {
            // Parsing of P1 should be terminated by semicolon as it is at the top nesting level.
            // In P2, P3 and P4 semicolon is nested in variation, parenthesized expression and span respectively,
            // so it's more likely that it has been placed by accident and can be skipped.
            ParseAndCompareErrors(
                patterns: "P1 = Word + ; Num;",
                new List<ExpectedError>
                {
                    CreateExpectedError(invalidToken: ";", TextResource.ExpressionExpected),
                    CreateExpectedError(invalidToken: ";", TextResource.EqualSignExpectedInPatternDefinition)
                },
                additionalChecks: package =>
                {
                    Assert.AreEqual(expected: 2, package.Patterns.Count, message: "Actual number of patterns is not equal to the expected one.");
                    Assert.AreEqual(expected: "Num", ((PatternSyntax)package.Patterns[1]).Name);
                });
            ParseAndCompareErrors(
                patterns: "P2 = {Word + ; Num};",
                CreateExpectedError(invalidToken: ";", TextResource.ExpressionExpected));
            ParseAndCompareErrors(
                patterns: "P3 = (Word + ; Num);",
                CreateExpectedError(invalidToken: ";", TextResource.ExpressionExpected));
            ParseAndCompareErrors(
                patterns: "P4 = [0+ Word + ; Num];",
                CreateExpectedError(invalidToken: ";", TextResource.ExpressionExpected));
        }

        [TestMethod]
        public void MissingCloseBrace()
        {
            ParseAndCompareErrors(
                patterns: "P1 = {(Word + Num} + Symbol;",
                CreateExpectedError(invalidToken: "}", TextResource.CloseParenthesisExpected));
            ParseAndCompareErrors(
                patterns: "P2 = ({Word, Num) + Space;",
                CreateExpectedError(invalidToken: ")", TextResource.CloseCurlyBraceExpected));
            ParseAndCompareErrors(
                patterns: "P3 = (Word + (Num;",
                CreateExpectedError(invalidToken: ";", TextResource.CloseParenthesisExpected));
            ParseAndCompareErrors(
                patterns: "P4 = (Alpha + [1+ Word, 0+ Num); ",
                CreateExpectedError(invalidToken: ")", TextResource.CloseSquareBracketExpected));
        }

        [TestMethod]
        public void MissingPrimaryExpression()
        {
            // In patterns P1, P2 and P3, close brackets have corresponding open ones, so primary expression parsing
            // method should not skip them
            // In patterns P4, P5 and P6 close brackets have no corresponding open ones, so they can be skipped to
            // proceed parsing of primary expression
            ParseAndCompareErrors(
                patterns: "P1 = (Word + ) Num;",
                new List<ExpectedError>
                {
                    CreateExpectedError(invalidToken: ")", TextResource.ExpressionExpected),
                    // Space between close parenthesis and Num
                    CreateExpectedError(errorStart: 14, errorLength: 1, TextResource.OperatorExpected)
                });
            ParseAndCompareErrors(
                patterns: "P2 = [0+ Word + ] Num;",
                new List<ExpectedError>
                {
                    CreateExpectedError(invalidToken: "]", TextResource.ExpressionExpected),
                    // Space between close square bracket and Num
                    CreateExpectedError(errorStart: 17, errorLength: 1, TextResource.OperatorExpected)
                });
            ParseAndCompareErrors(
                patterns: "P3 = {Word + } Num;",
                new List<ExpectedError>
                {
                    CreateExpectedError(invalidToken: "}", TextResource.ExpressionExpected),
                    // Space between close curly brace and Num
                    CreateExpectedError(errorStart: 14, errorLength: 1, TextResource.OperatorExpected)
                });
            ParseAndCompareErrors(
                patterns: "P4 = Word + ) Num;",
                CreateExpectedError(invalidToken: ")", TextResource.ExpressionExpected));
            ParseAndCompareErrors(
                patterns: "P5 = Word + ] Num;",
                CreateExpectedError(invalidToken: "]", TextResource.ExpressionExpected));  
            ParseAndCompareErrors(
                patterns: "P6 = Word + } Num;",
                CreateExpectedError(invalidToken: "}", TextResource.ExpressionExpected));
        }

        [TestMethod]
        public void MissingCommaOrOperator()
        {
            ParseAndCompareErrors(
                patterns: "P1 = Word Num;",
                // Space between Word and Num
                CreateExpectedError(errorStart: 9, errorLength: 1, TextResource.OperatorExpected));
            ParseAndCompareErrors(
                patterns: "P2 = Word {AlphaNum, Num};",
                // Space between Word and open curly brace
                CreateExpectedError(errorStart: 9, errorLength: 1, TextResource.OperatorExpected));
            ParseAndCompareErrors(
                patterns: "P3 = [0+ Word Num];",
                // Space between Word and Num
                CreateExpectedError(errorStart: 13, errorLength: 1, TextResource.OperatorExpected));
            ParseAndCompareErrors(
                patterns: "P4 = {Word Num};",
                CreateExpectedError(invalidToken: "Num", TextResource.CloseCurlyBraceOrCommaExpected));
        }

        [TestMethod]
        public void MisplacedComma()
        {
            ParseAndCompareErrors(
                patterns: "P1 = Word + , Num;",
                CreateExpectedError(invalidToken: ",", TextResource.ExpressionExpected));
            ParseAndCompareErrors(
                patterns: "P1 = {Word + , Num};",
                CreateExpectedError(invalidToken: ",", TextResource.ExpressionExpected),
                additionalChecks: package =>
                {
                    var variation = (VariationSyntax)((PatternSyntax)package.Patterns[0]).Body;
                    Assert.AreEqual(expected: 2, variation.Elements.Count, message: "Actual number of variation elements is not equal to the expected one.");
                });
            ParseAndCompareErrors(
                patterns: "P1 = [0+ Word + , 0+ Num];",
                CreateExpectedError(invalidToken: ",", TextResource.ExpressionExpected),
                additionalChecks: package =>
                {
                    var span = (SpanSyntax)((PatternSyntax)package.Patterns[0]).Body;
                    Assert.AreEqual(expected: 2, span.Elements.Count, message: "Actual number of span elements is not equal to the expected one.");
                });
        }

        [TestMethod]
        public void ParsingTerminatedByStartOfPattern()
        {
            void AssertTwoPatterns(PackageSyntax package)
            {
                Assert.AreEqual(expected: 2, package.Patterns.Count, message: "Actual number of patterns is not equal to the expected one.");
            }
            
            ParseAndCompareErrors(
                patterns: "P1 = Word P2 = Num;",
                // Space between Word and P2
                CreateExpectedError(errorStart: 9, errorLength: 1, TextResource.PatternShouldEndWithSemicolon),
                AssertTwoPatterns);
            ParseAndCompareErrors(
                patterns: "P1 = P2 = Num;",
                CreateExpectedError(invalidToken: "P2", TextResource.ExpressionExpected),
                AssertTwoPatterns);
            ParseAndCompareErrors(
                patterns: "P1 = Word + P2 = Num;",
                CreateExpectedError(invalidToken: "P2", TextResource.ExpressionExpected),
                AssertTwoPatterns);
            ParseAndCompareErrors(
                patterns: "P1 = Word + #P2 = Num;",
                CreateExpectedError(invalidToken: "#", TextResource.ExpressionExpected),
                AssertTwoPatterns);
        }

        [TestMethod]
        public void RequireInTheMiddleOfTheFile()
        {
            string patterns = 
@"Pattern = Word;
@require 'Basic.np';
@search Basic.Date;
";
            ParseAndCompareErrors(patterns,
                CreateExpectedError(invalidToken: "@require 'Basic.np';", TextResource.RequireKeywordsAreOnlyAllowedInTheBeginning));
        }

        [TestMethod]
        public void ExtractionErrors()
        {
            // Field reference to repetition
            ParseAndCompareErrors(
                patterns: "#Pattern(X) = [0+ X: Word] _ X;",
                CreateExpectedError(invalidToken: "X", TextResource.ValueOfFieldShouldBeExtractedFromTextBeforeUse));
            // Field reference to variation
            ParseAndCompareErrors(
                patterns: "#Pattern(X) = {X: Word, Alpha, Num} _ X;",
                CreateExpectedError(invalidToken: "X", TextResource.ValueOfFieldShouldBeExtractedFromTextBeforeUse));
            // Field reference to variation from another alternative
            ParseAndCompareErrors(
                patterns: "#Pattern(X) = {X: Word, Alpha _ X, Num};",
                CreateExpectedError(invalidToken: "X", TextResource.ValueOfFieldShouldBeExtractedFromTextBeforeUse));
            ParseAndCompareErrors(
                patterns: "#Pattern(X) = [0+ X: Word] _ X: Word;",
                CreateExpectedError(invalidToken: "X", TextResource.FieldAlreadyUsedForTextExtraction));
            ParseAndCompareErrors(
                patterns: "#Pattern() = UndeclaredField: Word;",
                CreateExpectedError(invalidToken: "UndeclaredField", TextResource.UndeclaredField));
        }

        [TestMethod]
        public void NumericRangeErrors()
        {
            ParseAndCompareErrors(
                patterns: "Pattern = [10-5 AlphaNum];",
                CreateExpectedError(invalidToken: "10", TextResource.NumericRangeLowBoundCannotBeGreaterThanHighBound));
            ParseAndCompareErrors(
                patterns: "Pattern = [-5 AlphaNum];",
                new List<ExpectedError>
                {
                    CreateExpectedError(invalidToken: "-", TextResource.NumericRangeExpected),
                    CreateExpectedError(invalidToken: "5", TextResource.ExpressionExpected),
                });
            ParseAndCompareErrors(
                patterns: $"Pattern = [0-{int.MaxValue} AlphaNum];",
                CreateExpectedError(invalidToken: int.MaxValue.ToString(), TextResource.InvalidValueOfNumericRangeBound));
        }

        [TestMethod]
        public void FieldsErrors()
        {
            ParseAndCompareErrors(
                patterns: "Pattern(Method, Domain = Method: Pattern.Method + Domain: Pattern.Domain;",
                CreateExpectedError(invalidToken: "=", TextResource.CloseParenthesisExpected));
            ParseAndCompareErrors(
                patterns: "Pattern(Method, Domain, = Method: Pattern.Method + Domain: Pattern.Domain;",
                CreateExpectedError(invalidToken: "=", TextResource.FieldNameExpected));
            ParseAndCompareErrors(
                patterns: "Pattern(Method, Method) = Method: Pattern.Method;",
                CreateExpectedError(invalidToken: "Method", TextResource.DuplicatedField));
        }

        [TestMethod]
        public void TextErrors()
        {
            ParseAndCompareErrors(
                patterns: "P1 = '';",
                CreateExpectedError(invalidToken: "''", TextResource.NonEmptyStringLiteralExpected));            
            ParseAndCompareErrors(
                patterns: "P2 = 'text'(Alpha, 3-6, Lowercase);",
                CreateExpectedError(invalidToken: "(Alpha, 3-6, Lowercase)", TextResource.TextAttributesAreAllowedOnlyForTextPrefixLiterals));
        }

        [TestMethod]
        public void UnterminatedComment()
        {
            string patterns = 
                @"Pattern = Word;
/* This is an
unterminated comment";
            ParseAndCompareErrors(
                patterns,
                // Last symbol of "comment" word
                CreateExpectedError(errorStart: 49, errorLength: 1, TextResource.UnterminatedComment));
        }
        
        [TestMethod]
        public void UnterminatedStringLiteral()
        {
            ParseAndCompareErrors(
                "Pattern = 'This is an unterminated string literal",
                // Last symbol of "literal" word
                CreateExpectedError(errorStart: 48, errorLength: 1, TextResource.UnterminatedStringLiteral));
        }
    }
}
