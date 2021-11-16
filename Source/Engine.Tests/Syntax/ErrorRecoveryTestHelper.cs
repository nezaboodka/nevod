using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Nezaboodka.Nevod.Engine.Tests
{
    internal static class ErrorRecoveryTestHelper
    {
        public delegate void PatternsAndPackageSelector(LinkedPackageSyntax rootPackage, out string patterns,
            out LinkedPackageSyntax package);
        
        public static ExpectedErrorByToken CreateExpectedError(string invalidToken, string messageTemplate) =>
            new ExpectedErrorByToken(string.Format(messageTemplate, invalidToken), invalidToken);
        
        public static ExpectedErrorByToken CreateExpectedErrorWithArgs(string invalidToken, string messageTemplate, params object[] args) =>
            new ExpectedErrorByToken(string.Format(System.Globalization.CultureInfo.CurrentCulture, messageTemplate, args), invalidToken);
        
        public static ExpectedErrorByRange CreateExpectedError(int errorStart, int errorLength, string messageTemplate) =>
            new ExpectedErrorByRange(messageTemplate, new TextRange(errorStart, errorStart + errorLength));

        public static void ParseAndCompareErrors(string patterns, ExpectedError expectedError,
            Action<PackageSyntax> additionalChecks = null) =>
            ParseAndCompareErrors(patterns, new List<ExpectedError> { expectedError }, additionalChecks);

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
        
        public static void LinkAndCompareErrors(string filePath, Func<string, string> fileContentProvider,
            ExpectedError expectedError, PatternsAndPackageSelector patternsAndPackageSelector = null)
        {
            string patterns = fileContentProvider(filePath);
            LinkAndCompareErrors(patterns, expectedError, fileContentProvider, filePath, patternsAndPackageSelector);
        }
        
        public static void LinkAndCompareErrors(string patterns, ExpectedError expectedError, 
            Func<string, string> fileContentProvider = null, string filePath = null,
            PatternsAndPackageSelector patternsAndPackageSelector = null)
        {
            LinkAndCompareErrors(patterns, new List<ExpectedError> { expectedError }, 
                fileContentProvider, filePath, patternsAndPackageSelector);
        }

        public static void LinkAndCompareErrors(string filePath, Func<string, string> fileContentProvider,
            IList<ExpectedError> expectedErrors)
        {
            string patterns = fileContentProvider(filePath);
            LinkAndCompareErrors(patterns, expectedErrors, fileContentProvider, filePath);
        }
        
        public static void LinkAndCompareErrors(string patterns, IList<ExpectedError> expectedErrors, 
            Func<string, string> fileContentProvider = null, string filePath = null,
            PatternsAndPackageSelector patternsAndPackageSelector = null)
        {
            patterns = patterns.Replace("\r\n", "\n");
            fileContentProvider = CreateNormalizingProvider(fileContentProvider);
            PackageSyntax package = new SyntaxParser().ParsePackageText(patterns);
            LinkedPackageSyntax linkedPackage = new PatternLinker(fileContentProvider).Link(package, baseDirectory: "", filePath);
            if (patternsAndPackageSelector != null)
                patternsAndPackageSelector(linkedPackage, out patterns, out linkedPackage);
            List<Error> actualErrors = linkedPackage.Errors;
            Assert.AreEqual(expectedErrors.Count, actualErrors.Count, message: "Actual number of errors is not equal to the expected one.");
            for (var i = 0; i < expectedErrors.Count; i++)
                CompareErrors(expectedErrors[i], actualErrors[i], patterns);
        }

        private static Func<string, string> CreateNormalizingProvider(Func<string, string> provider) =>
            filePath => provider(filePath).Replace("\r\n", "\n");

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
