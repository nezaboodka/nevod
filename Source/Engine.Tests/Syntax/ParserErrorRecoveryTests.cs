using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public void RequireErrors()
        {
            ParseAndCompareErrors(patterns: @"
Pattern = Word;
@require 'Basic.np';
@search Basic.Date;
",
                CreateExpectedError(invalidToken: "@require 'Basic.np';", TextResource.RequireKeywordsAreOnlyAllowedInTheBeginning));
            ParseAndCompareErrors(patterns: @"
@require 'Basic.np'*;
@search Basic.Date;
",
                CreateExpectedError(invalidToken: "'Basic.np'*", TextResource.InvalidSpecifierAfterStringLiteral));
            ParseAndCompareErrors(patterns: @"
@require ;
@search Basic.Date;
",
                CreateExpectedError(invalidToken: ";", TextResource.FilePathAsStringLiteralExpected));
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
        public void ExtractionFromFieldErrors()
        {
            ParseAndCompareErrors(
                patterns: @"
Pattern(X, Y) = X: Word + Y: Num;
Reference(A) = Pattern(A: X, A: Y); 
",
                CreateExpectedError(invalidToken: "A", TextResource.FieldAlreadyUsedForTextExtraction));
            ParseAndCompareErrors(
                patterns: @"
Pattern(X) = X: Word;
Reference() = Pattern(UndeclaredField: X); 
",
                CreateExpectedError(invalidToken: "UndeclaredField", TextResource.UndeclaredField));
            ParseAndCompareErrors(
                patterns: @"
Pattern(X) = X: Word;
Reference(A) = Pattern(A X); 
",
                CreateExpectedError(invalidToken: "X", TextResource.ColonExpected));
            ParseAndCompareErrors(
                patterns: @"
Pattern(X) = X: Word;
Reference(A) = Pattern(A: ); 
",
                CreateExpectedError(invalidToken: ")", TextResource.FromFieldNameExpected));
        }

        [TestMethod]
        public void SpanExtractionErrors()
        {
            ParseAndCompareErrors(
                patterns: "Html() = '<html>' .. UndeclaredField .. '</html>';",
                CreateExpectedError("UndeclaredField", TextResource.UndeclaredField));
            ParseAndCompareErrors(
                patterns: "Html(X) = X: Word + ('<html>' .. X .. '</html>');",
                CreateExpectedError("X", TextResource.FieldAlreadyUsedForTextExtraction));
        }

        [TestMethod]
        public void NumericRangeErrors()
        {
            ParseAndCompareErrors(
                patterns: "Pattern = [10-5 AlphaNum];",
                CreateExpectedError(invalidToken: "10", TextResource.NumericRangeLowBoundCannotBeGreaterThanHighBound));
            ParseAndCompareErrors(
                patterns: "Pattern = [10- AlphaNum];",
                CreateExpectedError(invalidToken: "AlphaNum", TextResource.HighBoundOfNumericRangeExpected));
            ParseAndCompareErrors(
                patterns: "Pattern = [-5 AlphaNum];",
                new List<ExpectedError>
                {
                    CreateExpectedError(invalidToken: "-", TextResource.NumericRangeExpected),
                    CreateExpectedError(invalidToken: "5", TextResource.ExpressionExpected),
                });
            ParseAndCompareErrors(
                patterns: $"Pattern = [0-{(long) int.MaxValue + 1} AlphaNum];",
                CreateExpectedError(invalidToken: ((long) int.MaxValue + 1).ToString(), TextResource.StringLiteralCannotBeConvertedToIntegerValue));
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
        public void IdentifierErrors()
        {
            ParseAndCompareErrors(
                patterns: "@search Basic.;",
                CreateExpectedError(";", TextResource.IdentifierOrAsteriskExpected));
            ParseAndCompareErrors(
                patterns: "Pattern = Namespace.;",
                CreateExpectedError(";", TextResource.IdentifierExpected));
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

        [TestMethod]
        public void UnknownTokenErrors()
        {
            ParseAndCompareErrors(
                patterns: "@p Pattern = Word;",
                CreateExpectedError(invalidToken: "@p", TextResource.UnknownKeyword));
            ParseAndCompareErrors(
                patterns: "% = 'Percent';",
                new List<ExpectedError>
                {
                    CreateExpectedError("%", TextResource.InvalidCharacter),
                    CreateExpectedError("=", TextResource.PatternNameExpected)
                });
        }

        [TestMethod]
        public void TextAttributesErrors()
        {
            ParseAndCompareErrors(
                patterns: "P1 = 'Text'*(WordNum);",
                CreateExpectedError("WordNum", TextResource.UnknownAttribute));
            ParseAndCompareErrors(
                patterns: "P2 = Word(Num, Uppercase);",
                CreateExpectedError("Num", TextResource.WordClassAttributeIsAllowedOnlyForTextPrefixLiterals));
            ParseAndCompareErrors(
                patterns: "P3 = 'Text'*(Alpha, 3-6, CamelCase);",
                CreateExpectedError("CamelCase", TextResource.UnknownAttribute));
            ParseAndCompareErrors(
                patterns: "P4 = 'Text'*(Uppercase, 3-6);",
                new List<ExpectedError>
                {
                    CreateExpectedError(",", TextResource.CloseParenthesisExpected),
                    CreateExpectedError("3-6", TextResource.NumericRangeIsInWrongPlace)
                });
            ParseAndCompareErrors(
                patterns: "P5 = 'Text'*(Uppercase, Num);",
                new List<ExpectedError>
                {
                    CreateExpectedError(",", TextResource.CloseParenthesisExpected),
                    CreateExpectedError("Num", TextResource.AttributeIsInWrongPlace)
                });
        }

        [TestMethod]
        public void StandardPatternWithNotAllowedAttributes()
        {
            ParseAndCompareErrors(
                patterns: "Pattern = Any(1-2);",
                CreateExpectedErrorWithArgs("(1-2)", 
                    TextResource.AttributesAreNotAllowedForStandardPattern, "Any"));
        }

        [TestMethod]
        public void UnexpectedTokensInVariation()
        {
            ParseAndCompareErrors(
                patterns: "Pattern = {Word 3-5, Num};",
                new List<ExpectedError>
                {
                    CreateExpectedError("3", TextResource.CloseCurlyBraceOrCommaExpected),
                    CreateExpectedError("-", TextResource.UnexpectedToken),
                    CreateExpectedError("5", TextResource.UnexpectedToken)
                });
        }

        [TestMethod]
        public void SearchTargetWithoutIdentifier()
        {
            ParseAndCompareErrors(
                patterns: "@search ;",
                CreateExpectedError(";", TextResource.IdentifierExpected),
                additionalChecks: package =>
                {
                    Assert.AreEqual(expected: 1, package.SearchTargets.Count, message: "Actual number of search targets is not equal to the expected one.");
                });
        }

        [TestMethod]
        public void NamespaceInNestedPatterns()
        {
            ParseAndCompareErrors(
                patterns: @"
Pattern = Word @where {
    @namespace Namespace {
        Inner = Num;
    }
};
",
                new List<ExpectedError>
                {
                    CreateExpectedError("@namespace", TextResource.NamespacesAreNotAllowedInNestedPatterns),
                    CreateExpectedError("{", TextResource.EqualSignExpectedInPatternDefinition),
                    CreateExpectedError("Inner", TextResource.ExpressionExpected),
                    // Space after first close curly brace
                    CreateExpectedError(errorStart: 78, errorLength: 1, TextResource.PatternShouldEndWithSemicolon),
                    CreateExpectedError("}", TextResource.PatternNameExpected),
                    CreateExpectedError(";", TextResource.PatternNameExpected)
                },
                additionalChecks: package =>
                {
                    ReadOnlyCollection<PatternSyntax> nestedPatterns = ((PatternSyntax)package.Patterns[0]).NestedPatterns;
                    Assert.AreEqual(expected: 2, nestedPatterns.Count, "'Namespace' should be parsed as pattern with no body.");
                    PatternSyntax innerPattern = nestedPatterns[0];
                    Assert.AreEqual(String.Empty, innerPattern.Namespace, message: "Inner pattern should have no namespace.");
                });
        }

        [TestMethod]
        public void ParseExpressionTextErrors()
        {
            ParseExpressionAndCompareErrors(
                expression: "Pattern = Word;",
                new List<ExpectedError>
                {
                    CreateExpectedError("Pattern", TextResource.ExpressionExpected),
                    CreateExpectedError("Pattern", TextResource.PatternDefinitionsAreNotAllowedInExpressionMode)
                });
            ParseExpressionAndCompareErrors(
                expression: "@namespace Namespace { Pattern = Word; }",
                new List<ExpectedError>
                {
                    CreateExpectedError("@namespace", TextResource.ExpressionExpected),
                    CreateExpectedError("@namespace", TextResource.NamespacesAreNotAllowedInExpressionMode)
                });
            ParseExpressionAndCompareErrors(
                expression: @"
@require 'Basic.np';
@search Basic.Url;
",
                new List<ExpectedError>
                {
                    CreateExpectedError("@require", TextResource.ExpressionExpected),
                    CreateExpectedError("@require", TextResource.RequireKeywordsAreNotAllowedInExpressionMode)
                });
            ParseExpressionAndCompareErrors(
                expression: @"Word + Num 3-5",
                CreateExpectedError("3", TextResource.EndOfExpressionExpectedInExpressionMode));
        }

        [TestMethod]
        public void EmptySpan()
        {
            ParseAndCompareErrors(
                patterns: @"Pattern = [];",
                CreateExpectedError("]", TextResource.NumericRangeExpected),
                additionalChecks: package =>
                {
                    var span = (SpanSyntax)((PatternSyntax)package.Patterns[0]).Body;
                    Assert.AreEqual(expected: 0, span.Elements.Count, message: "Actual number of span elements is not equal to the expected one.");
                });
        }
    }
}
