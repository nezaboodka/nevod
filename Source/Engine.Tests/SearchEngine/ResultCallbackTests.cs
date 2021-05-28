//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using static Nezaboodka.Nevod.Engine.Tests.TestHelper;
using System.Text;
using Nezaboodka.Text.Parsing;
using FluentAssertions;
using FluentAssertions.Equivalency;

namespace Nezaboodka.Nevod.Engine.Tests
{
    [TestClass]
    [TestCategory("ResultCallback")]
    public class ResultCallbackTests
    {
        [TestMethod]
        public void CompareResultCallbackWithRegularMode()
        {
            string patterns = "#A = 'android'; #B = 'iphone';";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            var options = new SearchOptions
            {
                FirstMatchOnly = false,
                SelfOverlappingTagsInResults = false,
                TokenCountToWaitToPerformGarbageCollection = 5
            };
            CheckResultCallbackWithRegularMode(patterns, text, options);
        }

        [TestMethod]
        public void ResultCallbackSelfOverlappingTags()
        {
            string patterns = "#T = [1-2 Word + ? WordBreak];";
            string text = "First Second Third.";
            var options = new SearchOptions()
            {
                FirstMatchOnly = false,
                SelfOverlappingTagsInResults = true,
                TokenCountToWaitToPerformGarbageCollection = 5
            };
            CheckResultCallbackWithRegularMode(patterns, text, options);
        }

        [TestMethod]
        public void ResultCallbackFirstMatchOnlySentence()
        {
            string patterns = @"
                SentenceSeparator = {End, '.', '!', '?', [2 LineBreak], ~'.'+Word};
                #Sentence = Word ... SentenceSeparator;
            ";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            var options = new SearchOptions()
            {
                FirstMatchOnly = true,
                TokenCountToWaitToPerformGarbageCollection = 5
            };
            CheckResultCallbackWithRegularMode(patterns, text, options);
        }

        [TestMethod]
        public void ResultCallbackFirstMatchOnlySequence()
        {
            string patterns = "#T = {'ANDROID OR IPHONE THE BETTER SMARTPHONE', 'IPHONE THE BETTER'};";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE";
            var options = new SearchOptions()
            {
                FirstMatchOnly = true,
                TokenCountToWaitToPerformGarbageCollection = 5
            };
            CheckResultCallbackWithRegularMode(patterns, text, options);
        }

        [TestMethod]
        public void ResultCallbackFirstMatchOnlySequenceOverlap()
        {
            string patterns = "#T = {'ANDROID OR IPHONE', 'IPHONE THE BETTER'};";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE";
            var options = new SearchOptions()
            {
                FirstMatchOnly = true
            };
            CheckResultCallbackWithRegularMode(patterns, text, options);
        }

        [TestMethod]
        public void ResultCallbackFirstMatchOnlySequenceWithLaterMatchOfShorter()
        {
            string patterns = "#T = 'android or' + Space + {{'iphone', ~'iphone the worst'}, 'iphone' + Space};";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE";
            var options = new SearchOptions()
            {
                FirstMatchOnly = true
            };
            CheckResultCallbackWithRegularMode(patterns, text, options);
        }

        [TestMethod]
        public void ResultCallbackFirstMatchOnlyVariationOfSequencesWithLaterMatchOfShorter()
        {
            string patterns = "#T = {'1 2'+Space+{'3', '3 5'}, '2 3 4'};";
            string text = "1 2 3 4 5";
            var options = new SearchOptions()
            {
                FirstMatchOnly = true
            };
            // SearchPatternsAndCheckMatches(patterns, text,
            //     "1 2 3");
            CheckResultCallbackWithRegularMode(patterns, text, options);
        }

        [TestMethod]
        public void ResultCallbackFirstMatchOnlyAnySpanWithExceptionInRightPart()
        {
            string patterns = "#T = {'android'...{'better', ~'better smartphone ever'}, 'iphone'...'smartphone'};";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE";
            var options = new SearchOptions()
            {
                FirstMatchOnly = true
            };
            SearchPatternsAndCheckMatches(patterns, text,
                "ANDROID OR IPHONE THE BETTER");
            CheckResultCallbackWithRegularMode(patterns, text, options);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void ResultCallbackWithLineContextSingleMatchAtStart(bool withReader)
        {
            string patterns = "#P = 'First';";
            string text = "First\nSecond\nThird";
            // Tokens:
            //    0      1       2         3         4        5     6
            // [Start] First [LineBreak] Second [LineBreak] Third [End]

            // Tokens by line:
            // 0: [Start] First [LineBreak]
            // 1: Second [LineBreak]
            // 2: Third [End]

            int bufferSizeInChars = 7;
            SearchOptions options = new SearchOptions()
            {
                FirstMatchOnly = false,
                SelfOverlappingTagsInResults = false,
                MaxCountOfMatchedTagsWaitingForCleanup = 0
            };

            ITextSource lineParsedTextSource = LineParsedTextSource.FromString(text);
            SearchResult expectedResult = SearchPatternsWithOptions(patterns, lineParsedTextSource, options);
            List<MatchedTag> expectedTags = expectedResult.GetTagsSortedByLocationInText();
            IEnumerable<string> expectedStrings = expectedTags.Select(t => t.GetText());

            ITextSource lineStreamTextSource = LineStreamTextSource.FromString(text, withReader, bufferSizeInChars);
            List<MatchedTag> actualTags = new List<MatchedTag>();
            List<string> actualStrings = new List<string>();
            SearchPatternsWithOptionsAndResultCallback(patterns, lineStreamTextSource, (SearchEngine _, MatchedTag tag) =>
            {
                actualTags.Add(tag);
                actualStrings.Add(tag.GetText());
            }, options);

            actualTags.Should().BeEquivalentTo(expectedTags, opt => opt
                .Excluding(t => t.TextSource)
                .Excluding(t => t.WasPassedToCallback)
                .Excluding(t => t.Start.Context.PreviousLineBreak.Context)
                .Excluding(t => t.Start.Context.CurrentLineBreak.Context)
                .Excluding(t => t.End.Context.PreviousLineBreak.Context)
                .Excluding(t => t.End.Context.CurrentLineBreak.Context)
                .IgnoringCyclicReferences()
                .WithStrictOrdering());

            actualStrings.Should().BeEquivalentTo(expectedStrings);
        }

        // Internal

        private static void CheckResultCallbackWithRegularMode(string patterns, string text, SearchOptions options)
        {
            List<MatchedTag> tagsList = SearchPatternsWithOptionsAndCallback(patterns, text, options);

            SearchResult expectedResult = SearchPatternsWithOptions(patterns, text, options);
            List<MatchedTag> expectedTagsList = expectedResult.GetTagsSortedByLocationInText();

            tagsList.Should().BeEquivalentTo(expectedTagsList, opt => opt.Excluding(t => t.TextSource)
                .Excluding(t => t.WasPassedToCallback));
        }

        private static void CheckResultCallbackWithRegularMode(string patterns, ITextSource textSource, SearchOptions options)
        {
            List<MatchedTag> tagsList = SearchPatternsWithOptionsAndGetMatchedTags(patterns, textSource, options);

            SearchResult expectedResult = SearchPatternsWithOptions(patterns, textSource, options);
            List<MatchedTag> expectedTagsList = expectedResult.GetTagsSortedByLocationInText();

            tagsList.Should().BeEquivalentTo(expectedTagsList, opt => opt.Excluding(t => t.TextSource)
                .Excluding(t => t.WasPassedToCallback));
        }
    }
}
