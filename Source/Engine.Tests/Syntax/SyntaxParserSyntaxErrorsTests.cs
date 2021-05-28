//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Nezaboodka.Nevod.Engine.Tests
{
    [TestClass]
    [TestCategory("Syntax"), TestCategory("SyntaxParser"), TestCategory("Syntax errors")]
    public class SyntaxParserSyntaxErrorsTests
    {
        [TestMethod]
        public void PatternShouldEndWithSemicolon()
        {
            string pattern = "TheWord = Word";
            TryParseAndTestExceptionMessage(
                pattern,
                expectedMessage: TextResource.PatternShouldEndWithSemicolon);
        }

        [TestMethod]
        public void PatternIdentifierExpected()
        {
            string pattern = "#$NotIdentifier = Word;";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.PatternDefinitionExpected,
                invalidToken: "$");
        }

        [TestMethod]
        public void PatternDefinitionExpected()
        {
            string pattern = "NotDefinition;";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.EqualSignExpectedInPatternDefinition,
                invalidToken: ";");
        }

        [TestMethod]
        public void IdentifierOrStringLiteralExpected()
        {
            string pattern = "#Identifier = ;";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.IdentifierOrStringLiteralExpected,
                invalidToken: ";");

        }

        [TestMethod]
        public void EqualSignExpectedInPatternDefinition()
        {
            string pattern = "Pattern != Word;";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.EqualSignExpectedInPatternDefinition,
                invalidToken: "!=");
        }

        [TestMethod]
        public void DoublePeriodExpected()
        {
            string pattern = "Pattern = Word .. [0-5] Alpha";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.DoublePeriodExpected,
                invalidToken: "Alpha");
        }

        [TestMethod]
        public void CloseParenthesisOrOperatorExpected()
        {
            string pattern = "Pattern = AlphaNum + (AlphaNum + Num;";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.CloseParenthesisOrOperatorExpected,
                invalidToken: ";");
        }

        [TestMethod]
        public void CloseSquareBracketExpected()
        {
            string pattern = "Pattern = [0-5 AlphaNum;";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.CloseSquareBracketExpected,
                invalidToken: ";");
        }

        [TestMethod]
        public void NumericRangeLowBoundCannotBeGreaterThanHighBound()
        {
            string pattern = "Pattern = [10-5 AlphaNum];";
            TryParseAndTestExceptionMessage(
                pattern,
                expectedMessage: TextResource.NumericRangeLowBoundCannotBeGreaterThanHighBound);
        }

        [TestMethod]
        public void IntegerLiteralExpectedInNumericRange()
        {
            string pattern = "Pattern = [1.0-5 AlphaNum];";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.IntegerLiteralExpected,
                invalidToken: "1.0");
        }

        [TestMethod]
        public void InvalidValueOfNumericRangeBound()
        {
            string pattern = $"Pattern = [0-{int.MaxValue} AlphaNum];";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.InvalidValueOfNumericRangeBound,
                invalidToken: int.MaxValue.ToString());
        }

        [TestMethod]
        public void EmptyStringLiteral()
        {
            string pattern = "Pattern = '';";
            var parser = new SyntaxParser();
            TryParseAndTestExceptionMessage(
                pattern,
                expectedMessage: TextResource.NonEmptyStringLiteralExpected);
        }

        [TestMethod]
        public void UnterminatedStringLiteral()
        {
            string pattern = $"Pattern = 'string;";
            TryParseAndTestExceptionMessage(
                pattern,
                expectedMessage: TextResource.UnterminatedStringLiteral);
        }

        [TestMethod]
        public void CloseCurlyBraceOrCommaExpected()
        {
            string pattern = "Pattern = {Alpha, AlphaNum;";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.CloseCurlyBraceOrCommaExpected,
                invalidToken: ";");
        }

        [TestMethod]
        public void CloseParenthesisOrCommaExpectedInPatternFieldsDeclaration()
        {
            string pattern = "Acquisition(Who, Whom = Relation(Who, 'acquire', Whom);";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.CloseParenthesisOrCommaExpected,
                invalidToken: "=");
        }

        [TestMethod]
        public void FieldExpectedInPatternFieldsDeclaration()
        {
            string pattern = "Acquisition(Who, Whom, = Relation(Who, 'acquire', Whom);";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.FieldNameExpected,
                invalidToken: "=");
        }

        [TestMethod]
        public void CloseParenthesisOrCommaExpectedInPatternReference()
        {
            string pattern = "Acquisition(Who, Whom = Who ... 'acquire' ... Whom;";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.CloseParenthesisOrCommaExpected,
                invalidToken: "=");
        }

        [TestMethod]
        public void DuplicatedPatternName()
        {
            string patterns = "TheWord = Word;\n" +
                "TheWord = Alpha;";
            TryParseAndTestExceptionMessage(
                patterns,
                messageTemplate: TextResource.DuplicatedPatternName,
                invalidToken: "TheWord");
        }

        [TestMethod]
        public void PatternNameWithUnderscoreParsing()
        {
            string pattern = "#Some_Pattern = Alpha _ Alpha;";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.EqualSignExpectedInPatternDefinition,
                invalidToken: "_");
        }

        [TestMethod]
        public void FieldReferenceToRepetitionInvalidParsing()
        {
            string pattern = "#Pattern(X) = [0+ X: Word] _ X;";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.ValueOfFieldShouldBeExtractedFromTextBeforeUse,
                invalidToken: "X");
        }

        [TestMethod]
        public void FieldReferenceToVariationInvalidParsing()
        {
            string pattern = "#Pattern(X) = {X: Word, Alpha, Num} _ X;";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.ValueOfFieldShouldBeExtractedFromTextBeforeUse,
                invalidToken: "X");
        }

        [TestMethod]
        public void FieldReferenceToVariationFromAnotherAlternativeInvalidParsing()
        {
            string pattern = "#Pattern(X) = {X: Word, Alpha _ X, Num};";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.ValueOfFieldShouldBeExtractedFromTextBeforeUse,
                invalidToken: "X");
        }

        [TestMethod]
        public void DoubleExtractionOfFieldParsing()
        {
            string pattern = "#Pattern(X) = [0+ X: Word] _ X: Word;";
            TryParseAndTestExceptionMessage(
                pattern,
                messageTemplate: TextResource.FieldAlreadyUsedForTextExtraction,
                invalidToken: "X");
        }

        // Internal

        private static void TryParseAndTestExceptionMessage(string patterns,
            string messageTemplate, string invalidToken)
        {
            string expectedMessage = string.Format(messageTemplate, invalidToken);
            TryParseAndTestExceptionMessage(patterns, expectedMessage);
        }

        private static void TryParseAndTestExceptionMessage(string patterns,
            string expectedMessage)
        {
            var parser = new SyntaxParser();
            TestHelper.TestExceptionMessage<SyntaxException>(parser.ParsePackageText, patterns, expectedMessage);
        }
    }
}
