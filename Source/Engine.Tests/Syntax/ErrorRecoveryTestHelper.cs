using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nezaboodka.Nevod.Engine.Tests
{
    internal static class ErrorRecoveryTestHelper
    {
        public static ExpectedErrorByToken CreateExpectedError(string invalidToken, string messageTemplate) =>
            new ExpectedErrorByToken(string.Format(messageTemplate, invalidToken), invalidToken);
        
        public static ExpectedErrorByRange CreateExpectedError(int errorStart, int errorLength, string messageTemplate) =>
            new ExpectedErrorByRange(messageTemplate, new TextRange(errorStart, errorStart + errorLength));

        public static void ParseAndCompareErrors(string patterns, ExpectedError expectedError,
            Action<PackageSyntax> additionalChecks = null)
        {
            patterns = patterns.Replace("\r\n", "\n");
            PackageSyntax package = new SyntaxParser().ParsePackageText(patterns);
            Assert.AreEqual(expected: 1, package.Errors.Count, message: "Actual number of errors is not equal to the expected one.");
            CompareErrors(expectedError, actualError: package.Errors[0], patterns);
            additionalChecks?.Invoke(package);
        }

        public static void ParseAndCompareErrors(string patterns, IList<ExpectedError> expectedErrors, 
            Action<PackageSyntax> additionalChecks = null)
        {
            patterns = patterns.Replace("\r\n", "\n");
            PackageSyntax package = new SyntaxParser().ParsePackageText(patterns);
            List<Error> actualErrors = package.Errors;
            Assert.AreEqual(expectedErrors.Count, actualErrors.Count, message: "Actual number of errors is not equal to the expected one.");
            for (var i = 0; i < expectedErrors.Count; i++)
                CompareErrors(expectedErrors[i], actualErrors[i], patterns);
            additionalChecks?.Invoke(package);
        }

        private static void CompareErrors(ExpectedError expectedError, Error actualError, string patterns)
        {
            Assert.AreEqual(expectedError.ErrorMessage, actualError.ErrorMessage);
            switch (expectedError)
            {
                case ExpectedErrorByToken expectedErrorByToken:
                {
                    string actualInvalidToken = TextAtRange(patterns, actualError.ErrorRange);
                    Assert.AreEqual(expectedErrorByToken.InvalidToken, actualInvalidToken);
                    break;
                }
                case ExpectedErrorByRange expectedErrorByRange:
                {
                    Assert.AreEqual(expectedErrorByRange.Range, actualError.ErrorRange);
                    break;
                }
            }
        }

        private static string TextAtRange(string text, TextRange range) => text.Substring(range.Start, range.End - range.Start);
    }
    
    internal abstract class ExpectedError
    {
        internal string ErrorMessage { get; }
            
        public ExpectedError(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }

    internal class ExpectedErrorByToken : ExpectedError
    {
        internal string InvalidToken { get; }
            
        public ExpectedErrorByToken(string errorMessage, string invalidToken) : base(errorMessage)
        {
            InvalidToken = invalidToken;
        }
    }

    internal class ExpectedErrorByRange : ExpectedError
    {
        internal TextRange Range { get; }
            
        public ExpectedErrorByRange(string errorMessage, TextRange range) : base(errorMessage)
        {
            Range = range;
        }
    }
}
