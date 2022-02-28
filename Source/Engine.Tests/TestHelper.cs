//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Nezaboodka.Text.Parsing;
using System.IO;

namespace Nezaboodka.Nevod.Engine.Tests
{
    internal static class TestHelper
    {
        public static void TestParseAndToString(string patterns)
        {
            var parser = new SyntaxParser();
            PackageSyntax package = parser.ParsePackageText(patterns);
            string text = package.ToString();
            TestPackageIs(expected: patterns, actual: text);
        }

        public static void TestPackageIs(string expected, string actual)
        {
            string optionalPatternPrefix = "@pattern ";
            actual = actual.Replace('"', '\'').Replace(optionalPatternPrefix, string.Empty).Trim('\n', ' ');
            expected = expected.Replace("\r\n", "\n").Replace(optionalPatternPrefix, string.Empty).Trim('\n', ' ');
            Assert.AreEqual(expected, actual);
        }

        public static void TestExceptionMessage<TException>(Func<string, object> func, string arg,
            string expectedMessage) where TException : Exception
        {
            func.Invoking(x => x(arg)).Should().Throw<TException>().Which.Message.Should().StartWith(expectedMessage);
        }

        public static void AssertNoMatches(string patterns, string text, SearchOptions options = null)
        {
            SearchPatternsAndCheckMatches(patterns, text, options: options,
                expectedMatches: Array.Empty<string>());
        }

        public static void AssertNoExtractions(string patterns, string text)
        {
            SearchPatternsAndCheckExtractions(patterns, text,
                expectedExtractions: Array.Empty<(string, string[])>());
        }

        public static void SearchPatternsAndCheckMatchesAndMeasureResourceConsumption(string patterns, string text,
            ResourceConsumption linkingConsumption, ResourceConsumption generationConsumption, ResourceConsumption searchConsumption,
            params string[] expectedMatches)
        {
            SearchPatternsAndCheckMatchesAndMeasureResourceConsumption(patterns, text, options: null,
                linkingConsumption, generationConsumption, searchConsumption, expectedMatches);
        }

        public static void SearchPatternsAndCheckMatches(string patterns, string text,
            params (string, string[])[] expectedMatches)
        {
            SearchPatternsAndCheckMatches(patterns, text, options: null,
                expectedMatches);
        }

        public static void SearchPatternsAndCheckMatches(string patterns, string text,
            params string[] expectedMatches)
        {
            SearchPatternsAndCheckMatches(patterns, text, options: null,
                expectedMatches);
        }

        public static void SearchPatternsAndCheckMatches(
            Func<string, string> fileContentProvider, string patterns, string text,
            params string[] expectedMatches)
        {
            SearchPatternsAndCheckMatches(fileContentProvider, patterns, text, options: null,
                expectedMatches);
        }

        public static void SearchPatternsAndCheckMatches(string patterns, string text, SearchOptions options,
            params (string, string[])[] expectedMatches)
        {
            var actualMatches = SearchPatternsWithOptions(patterns, text, options).TagsByName
                .Select(x => (x.Key, x.Value.Select(t => t.GetText()).ToArray())).ToArray();
            actualMatches.Should().BeEquivalentTo(expectedMatches);
        }

        public static void SearchPatternsAndCheckMatches(
            Func<string, string> fileContentProvider, string patterns, string text,
            params (string, string[])[] expectedMatches)
        {
            var actualMatches = SearchPatternsWithOptions(fileContentProvider, patterns, text).TagsByName
                .Select(x => (x.Key, x.Value.Select(t => t.GetText()).ToArray())).ToArray();
            actualMatches.Should().BeEquivalentTo(expectedMatches);
        }

        public static void SearchPatternsAndCheckMatchesAndMeasureResourceConsumption(string patterns, string text, SearchOptions options,
            ResourceConsumption linkingConsumption, ResourceConsumption generationConsumption, ResourceConsumption searchConsumption,
            params string[] expectedMatches)
        {
            var actualMatches = SearchPatternsWithOptionsAndMeasureResourceConsumption(patterns, text, options,
                linkingConsumption, generationConsumption, searchConsumption)
                .GetTagsSortedByLocationInText().Select(x => x.GetText()).ToArray();
            actualMatches.Should().BeEquivalentTo(expectedMatches);
        }

        public static void SearchPatternsAndCheckMatches(string patterns, string text, SearchOptions options,
            params string[] expectedMatches)
        {
            var actualMatches = SearchPatternsWithOptions(patterns, text, options)
                .GetTagsSortedByLocationInText().Select(x => x.GetText()).ToArray();
            actualMatches.Should().BeEquivalentTo(expectedMatches);
        }

        public static void SearchPatternsAndCheckMatches(
            Func<string, string> fileContentProvider, string patterns, string text, SearchOptions options,
            params string[] expectedMatches)
        {
            var actualMatches = SearchPatternsWithOptions(fileContentProvider, patterns, text, options)
                .GetTagsSortedByLocationInText().Select(x => x.GetText()).ToArray();
            actualMatches.Should().BeEquivalentTo(expectedMatches);
        }

        public static void SearchPatternsAndCheckExtractions(string patterns, string text,
            params (string, string[])[] expectedExtractions)
        {
            List<MatchedTag> matches = TestHelper.SearchPatternsWithOptions(patterns, text, options: null)
                .GetTagsSortedByLocationInText();
            (string, string[])[] actualExtractions = matches
                .Where(x => x.Extractions != null)
                .SelectMany(x => x.Extractions
                    .Select(e => ($"{x.PatternFullName}.{e.Key}", e.Value.Select(t => t.GetText()).ToArray())))
                .ToArray();
            actualExtractions.Should().BeEquivalentTo(expectedExtractions);
        }

        public static void SearchPatternsAndCheckExtractions(Func<string, string> fileContentProvider,
            string patterns, string text, params (string, string[])[] expectedExtractions)
        {
            List<MatchedTag> matches = TestHelper.SearchPatternsWithOptions(fileContentProvider, patterns, text,
                options: null).GetTagsSortedByLocationInText();
            (string, string[])[] actualExtractions = matches
                .Where(x => x.Extractions != null)
                .SelectMany(x => x.Extractions
                    .Select(e => ($"{x.PatternFullName}.{e.Key}", e.Value.Select(t => t.GetText()).ToArray())))
                .ToArray();
            actualExtractions.Should().BeEquivalentTo(expectedExtractions);
        }

        public static SearchResult SearchPatternsWithOptions(string patterns, string text,
            SearchOptions options = null)
        {
            if (options == null)
                options = DefaultSearchOptionsForTest;
            var package = PatternPackage.FromText(patterns, DefaultPackageBuilderOptionsForTest);
            var engine = new TextSearchEngine(package, options);
            SearchResult searchResult = engine.Search(text);
            if (!options.SuppressCandidateLimitExceeded && searchResult.WasCandidateLimitExceeded)
                Assert.Fail("Candidate limit exceeded");
            return searchResult;
        }

        public static SearchResult SearchPatternsWithOptions(string patterns, ITextSource textSource,
            SearchOptions options = null)
        {
            if (options == null)
                options = DefaultSearchOptionsForTest;
            var package = PatternPackage.FromText(patterns, DefaultPackageBuilderOptionsForTest);
            var engine = new TextSearchEngine(package, options);
            SearchResult searchResult = engine.Search(textSource);
            if (!options.SuppressCandidateLimitExceeded && searchResult.WasCandidateLimitExceeded)
                Assert.Fail("Candidate limit exceeded");
            return searchResult;
        }

        public class ResourceConsumption
        {
            public long ElapsedMilliseconds;
            public long TotalAllocatedBytes;
            public long ConsumedBytes;
        }

        public static SearchResult SearchPatternsWithOptionsAndMeasureResourceConsumption(string patterns, string text, SearchOptions options,
            ResourceConsumption linkingConsumption, ResourceConsumption generationConsumption, ResourceConsumption searchConsumption)
        {
            if (options == null)
                options = DefaultSearchOptionsForTest;

            Console.WriteLine();
            
            var parser = new SyntaxParser();
            PackageSyntax parsedTree = parser.ParsePackageText(patterns);
            
            var startMemory = GC.GetTotalMemory(true);
            var startAllocatedBytes = GC.GetTotalAllocatedBytes();
            var watch = Stopwatch.StartNew();

            var linker = new NormalizingPatternLinker(
                fileContentProvider: File.ReadAllText,
                PackageCache.Global
            );
            LinkedPackageSyntax linkedTree = linker.Link(parsedTree, Environment.CurrentDirectory, null);
            
            parsedTree = null;

            var elapsed = watch.ElapsedMilliseconds;
            var endMemory = GC.GetTotalMemory(true);
            var consumedBytes = endMemory - startMemory;
            var endAllocatedBytes = GC.GetTotalAllocatedBytes();
            var deltaAllocatedBytes = endAllocatedBytes - startAllocatedBytes;

            linkingConsumption.ElapsedMilliseconds = elapsed;
            linkingConsumption.TotalAllocatedBytes = deltaAllocatedBytes;
            linkingConsumption.ConsumedBytes = consumedBytes;

            watch.Restart();
            startMemory = endMemory;
            startAllocatedBytes = endAllocatedBytes;
            parsedTree = linkedTree;
            var builder = new PackageBuilder(DefaultPackageBuilderOptionsForTest,
                PackageCache.Global);
            PatternPackage package = builder.BuildPackageFromSyntax(parsedTree);

            elapsed = watch.ElapsedMilliseconds;
            endMemory = GC.GetTotalMemory(true);
            consumedBytes = endMemory - startMemory;
            endAllocatedBytes = GC.GetTotalAllocatedBytes();
            deltaAllocatedBytes = endAllocatedBytes - startAllocatedBytes;

            generationConsumption.ElapsedMilliseconds = elapsed;
            generationConsumption.TotalAllocatedBytes = deltaAllocatedBytes;
            generationConsumption.ConsumedBytes = consumedBytes;

            watch.Restart();
            startMemory = endMemory;
            startAllocatedBytes = endAllocatedBytes;
            var engine = new TextSearchEngine(package, options);
            SearchResult searchResult = engine.Search(text);

            elapsed = watch.ElapsedMilliseconds;
            endMemory = GC.GetTotalMemory(true);
            consumedBytes = endMemory - startMemory;
            endAllocatedBytes = GC.GetTotalAllocatedBytes();
            deltaAllocatedBytes = endAllocatedBytes - startAllocatedBytes;

            searchConsumption.ElapsedMilliseconds = elapsed;
            searchConsumption.TotalAllocatedBytes = deltaAllocatedBytes;
            searchConsumption.ConsumedBytes = consumedBytes;

            if (!options.SuppressCandidateLimitExceeded && searchResult.WasCandidateLimitExceeded)
                Assert.Fail("Candidate limit exceeded");
            return searchResult;
        }

        public static List<MatchedTag> SearchPatternsWithOptionsAndCallback(string patterns, string text,
            SearchOptions options)
        {
            ITextSource parsedTextSource = new ParsedTextSource(PlainTextParser.Parse(text));
            List<MatchedTag> result = SearchPatternsWithOptionsAndGetMatchedTags(patterns, parsedTextSource, options);
            return result;
        }

        public static List<MatchedTag> SearchPatternsWithOptionsAndGetMatchedTags(string patterns, ITextSource textSource,
            SearchOptions options)
        {
            List<MatchedTag> result = new List<MatchedTag>();
            SearchPatternsWithOptionsAndResultCallback(patterns, textSource, (engine, matchedTag) =>
            {
                result.Add(matchedTag);
            },
            options);
            result.Sort();
            return result;
        }

        public static void SearchPatternsWithOptionsAndResultCallback(string patterns, ITextSource textSource,
            SearchResultCallback resultCallback, SearchOptions options = null)
        {
            if (options == null)
                options = DefaultSearchOptionsForTest;
            if (resultCallback == null)
                throw new ArgumentNullException($"{nameof(resultCallback)}");
            var package = PatternPackage.FromText(patterns, DefaultPackageBuilderOptionsForTest);
            var engine = new TextSearchEngine(package, options);
            engine.Search(textSource, resultCallback);
        }

        public static SearchResult SearchPatternsWithOptions(Func<string, string> fileContentProvider,
            string patterns, string text, SearchOptions options = null)
        {
            if (options == null)
                options = DefaultSearchOptionsForTest;
            var packageBuilder = new PackageBuilder(DefaultPackageBuilderOptionsForTest,
                new PackageCache(), fileContentProvider);
            var package = packageBuilder.BuildPackageFromText(patterns, baseDirectory: "/");
            var engine = new TextSearchEngine(package, options);
            SearchResult searchResult = engine.Search(text);
            if (!options.SuppressCandidateLimitExceeded && searchResult.WasCandidateLimitExceeded)
                Assert.Fail("Candidate limit exceeded");
            return searchResult;
        }

        public static readonly PackageBuilderOptions DefaultPackageBuilderOptionsForTest = new PackageBuilderOptions()
        {
            PatternReferencesInlined = true,
            SyntaxInformationBinding = true
        };

        public static readonly SearchOptions DefaultSearchOptionsForTest = new SearchOptions()
        {
            LevelOfDetails = LevelOfDetails.MatchedTagsWithSyntaxDetails,
            NonTargetTagsInResults = false,
            IsDebugMode = true
        };

        public static string GetBasicPackageDirectory()
        {
            string currDir = Directory.GetCurrentDirectory();
            string dataParentDir = Path.GetFullPath(currDir + "/../../..");
            string dataDir = Path.Combine(dataParentDir, "Patterns");
            if (!Directory.Exists(dataDir))
            {
                dataParentDir = Path.GetFullPath(dataParentDir + "/..");
                dataDir = Path.Combine(dataParentDir, "Patterns");
            }
            return dataDir;
        }
        
        public static void TestSourceTextInformation(string patterns, Syntax syntax, string expectedText)
        {
            string actualText = patterns.Substring(syntax.TextRange.Start, syntax.TextRange.Length);
            Assert.AreEqual(expectedText, actualText);
        }

        public static PackageSyntax ParsePatterns(string patterns)
        {
            var syntaxParser = new SyntaxParser();
            return syntaxParser.ParsePackageText(patterns);
        }

        public static void TestTokenIdAndTextRange(string patterns, Syntax syntax, TokenId tokenId, string expectedText)
        {
            Assert.IsInstanceOfType(syntax, typeof(Lexeme));
            var lexeme = (Lexeme)syntax;
            Assert.AreEqual(tokenId, lexeme.TokenId);
            TestSourceTextInformation(patterns, lexeme, expectedText);
        }

        public static void TestToken(LexicalToken token, TokenId tokenId, string expectedText)
        {
            Assert.AreEqual(tokenId, token.Id);
            Assert.AreEqual(expectedText, token.TextSlice.ToString());
        }
    }
}
