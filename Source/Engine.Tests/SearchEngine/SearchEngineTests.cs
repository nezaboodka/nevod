//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using static Nezaboodka.Nevod.Engine.Tests.TestHelper;

namespace Nezaboodka.Nevod.Engine.Tests
{
    [TestClass]
    [TestCategory("SearchEngine")]
    public class SearchEngineTests
    {
        [TestMethod]
        public void SingleTextToken()
        {
            string patterns = "#Nezaboodka = 'nezaboodka';";
            string text = "Nezaboodka Software LLC";
            SearchPatternsAndCheckMatches(patterns, text,
                "Nezaboodka");
        }

        [TestMethod]
        public void SingleTextTokenOptionalPatternSyntax()
        {
            string patterns = "@pattern #Nezaboodka = 'nezaboodka';";
            string text = "Nezaboodka Software LLC";
            SearchPatternsAndCheckMatches(patterns, text,
                "Nezaboodka");
        }

        [TestMethod]
        public void SingleTextTokenOptionalSearchPatternSyntax()
        {
            string patterns = "@search @pattern Nezaboodka = 'nezaboodka';";
            string text = "Nezaboodka Software LLC";
            SearchPatternsAndCheckMatches(patterns, text,
                "Nezaboodka");
        }

        [TestMethod]
        public void SingleCaseSensitiveTextToken()
        {
            string patterns = "#Nezaboodka = 'Nezaboodka'!;";
            string text = "Nezaboodka Software LLC";
            SearchPatternsAndCheckMatches(patterns, text,
                "Nezaboodka");
        }

        [TestMethod]
        public void SingleCaseSensitiveTextTokenNoMatch()
        {
            string patterns = "#Nezaboodka = 'nezaboodka'!;";
            string text = "Nezaboodka Software LLC";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void SingleWordToken()
        {
            string patterns = "#SingleWord = Word;";
            string text = "Nezaboodka Software LLC";
            SearchPatternsAndCheckMatches(patterns, text,
                "Nezaboodka",
                "Software",
                "LLC");
        }

        [TestMethod]
        public void SingleWordTokenAttributes()
        {
            string patterns = "#SingleWord = Alpha(9-12, TitleCase);";
            string text = "Nezaboodka Software LLC";
            SearchPatternsAndCheckMatches(patterns, text,
                "Nezaboodka");
        }

        [TestMethod]
        public void SingleWordPrefixWithAttributes()
        {
            string patterns = "#SingleWord = 'Ne'!*(Alpha, 8, Lowercase);";
            string text = "Nezaboodka Software LLC";
            SearchPatternsAndCheckMatches(patterns, text,
                "Nezaboodka");
        }

        [TestMethod]
        public void SingleWordPrefixWithAttributesNoMatch()
        {
            string patterns = "#SingleWord = 'ne'!*(Alpha, 9-12, Lowercase);";
            string text = "Nezaboodka Software LLC";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void SingleWordPrefixWithAttributesCaseSensivityNoMatch()
        {
            string patterns = "#SingleWord = 'Ne'!*(Alpha, 7-10, Uppercase);";
            string text = "Nezaboodka Software LLC";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        [Ignore]
        public void SingleWordPrefixWithAttributesNumSuffix()
        {
            string patterns = "#SingleWord = 'N'!*(Num);";
            string text = "N12";
            SearchPatternsAndCheckMatches(patterns, text,
                "N12");
        }

        [TestMethod]
        public void SpaceFromTwoToFour()
        {
            string patterns = "#Space4 = Space(2-4) + Word;";
            string text = "This is Nezaboodka  Software    LLC";
            SearchPatternsAndCheckMatches(patterns, text,
                "  Software",
                "    LLC");
        }

        [TestMethod]
        public void SingleTextTokenEscapedSingleQuote()
        {
            string patterns = "#Q = '''';";
            string text = "Test single quote: '.";
            SearchPatternsAndCheckMatches(patterns, text,
                "'");
        }

        [TestMethod]
        public void SingleTextTokenEscapedDoubleQuote()
        {
            string patterns = "#Q = \"\"\"\";";
            string text = "Test double quote: \".";
            SearchPatternsAndCheckMatches(patterns, text,
                "\"");
        }

        [TestMethod]
        public void SequenceWithEscapedSingleQuote()
        {
            string patterns = "#Q = 'test''test';";
            string text = "Test single quote: test'test.";
            SearchPatternsAndCheckMatches(patterns, text,
                "test'test");
        }

        [TestMethod]
        public void SequenceWithEscapedDoubleQuote()
        {
            string patterns = "#Q = \"test\"\"test\";";
            string text = "Test single quote: test\"test.";
            SearchPatternsAndCheckMatches(patterns, text,
                "test\"test");
        }

        [TestMethod]
        public void RepetitionOfToken()
        {
            string patterns = "#Pattern = [2 '.'];";
            string text = "..";
            SearchPatternsAndCheckMatches(patterns, text,
                "..");
        }

        [TestMethod]
        public void RepetitionOfTokenNoMatch()
        {
            string patterns = "#Pattern = [3+ '.'];";
            string text = "..";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void RepetitionOfTokenOverlapped()
        {
            string patterns = "#Pattern = [2-3 '.'];";
            string text = "...";
            SearchPatternsAndCheckMatches(patterns, text,
                "...");
        }

        [TestMethod]
        public void RepetitionOfTokenSecondOverlapped()
        {
            string patterns = "#Pattern = [2-3 '.'];";
            string text = "....";
            SearchPatternsAndCheckMatches(patterns, text,
                "...");
        }

        [TestMethod]
        public void SequenceForEmptyText()
        {
            string patterns = "#Empty = Start + End;";
            string text = string.Empty;
            SearchPatternsAndCheckMatches(patterns, text,
                string.Empty);
        }

        [TestMethod]
        public void SequenceForEmptyTextNoMatch()
        {
            string patterns = "#Empty = Start + End;";
            string text = "Nezaboodka Software LLC";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void SequenceOfTokens()
        {
            string patterns = "#Pattern = 'Nezaboodka' + Space + 'Software' + Space + 'LLC';";
            string text = "Nezaboodka Software LLC";
            SearchPatternsAndCheckMatches(patterns, text,
                "Nezaboodka Software LLC");
        }

        [TestMethod]
        public void SequenceOfTokensNoMatch()
        {
            string patterns = "#Pattern = 'Nezaboodka' + Space + 'LLC';";
            string text = "Nezaboodka Software LLC";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void SequenceOfTokensStartingWithOptionalElementsThirdOnly()
        {
            string patterns = "#Pattern = ?'blog.' + ?'nezaboodka.' + 'com';";
            string comText = "Try to find us at google.com";
            SearchPatternsAndCheckMatches(patterns, comText,
                "com");
        }

        [TestMethod]
        public void SequenceOfTokensStartingWithOptionalElementsSecondAndThird()
        {
            string patterns = "#Pattern = ?'blog.' + ?'nezaboodka.' + 'com';";
            string nezaboodkaText = "Visit our website: nezaboodka.com";
            SearchPatternsAndCheckMatches(patterns, nezaboodkaText,
                "nezaboodka.com");
        }

        [TestMethod]
        public void SequenceOfTokensStartingWithOptionalElementsFull()
        {
            string patterns = "#Pattern = ?'blog.' + ?'nezaboodka.' + 'com';";
            string blogNezaboodkaComText = "Check out our blog for latest news: blog.nezaboodka.com";
            SearchPatternsAndCheckMatches(patterns, blogNezaboodkaComText,
                "blog.nezaboodka.com");
        }

        [TestMethod]
        public void SequenceOfTokensStartingWithOptionalElement()
        {
            string patterns = "#Pattern = ?'nezaboodka' + ?'.' + 'com';";
            string nezaboodkaText = "Website .com";
            SearchPatternsAndCheckMatches(patterns, nezaboodkaText,
                ".com");
        }

        [TestMethod]
        public void SequenceOfTokensStartingWithOptionalElementsFirstAndLastElements()
        {
            string patterns = "#Pattern = ?'blog.' + ?'nezaboodka.' + 'com';";
            string blogComText = "blog.com is not our address";
            SearchPatternsAndCheckMatches(patterns, blogComText,
                "blog.com");
        }

        [TestMethod]
        public void SequenceOfTokensEndingWithOptionalElements()
        {
            string patterns = "#Pattern = 'Nezaboodka' + ?(Space + 'Software') + ?(Space + 'LLC');";
            string blogComText = "Welcome to Nezaboodka!";
            SearchPatternsAndCheckMatches(patterns, blogComText,
                "Nezaboodka");
        }

        [TestMethod]
        public void SequenceOfTokensWithInnerOptionalElementsFirstRejectAndSecond()
        {
            string patterns = "#Pattern = 'blog.' + ?('nezaboodka' + '-') + ?('nezaboodka' + '.') + 'com';";
            string blogComText = "blog.nezaboodka.com";
            SearchPatternsAndCheckMatches(patterns, blogComText,
                "blog.nezaboodka.com");
        }

        [TestMethod]
        public void SequenceOfTokensWithEqualInnerOptionalElementsBoth()
        {
            string patterns = "#Pattern = 'blog.' + ?('nezaboodka' + '.') + ?('nezaboodka' + '.') + 'com';";
            string blogComText = "blog.nezaboodka.com";
            SearchPatternsAndCheckMatches(patterns, blogComText,
                "blog.nezaboodka.com");
        }

        [TestMethod]
        public void SequenceWithOptionalRepetitionWithException()
        {
            string patterns = "#Pattern = '<' + [0+ {Any, ~'>'}] + '>';";
            string nezaboodkaText = "<>";
            SearchPatternsAndCheckMatches(patterns, nezaboodkaText,
                "<>");
        }

        [TestMethod]
        public void SequenceWithOptionalElementWithException()
        {
            string patterns = "#Pattern = '<' + ?{Any, ~'>'} + '>';";
            string nezaboodkaText = "<>";
            SearchPatternsAndCheckMatches(patterns, nezaboodkaText,
                "<>");
        }

        [TestMethod]
        public void SequenceWithFirstOptionalElementWithException()
        {
            string patterns = "#Pattern = ?{Any, ~'<'} + '<';";
            string nezaboodkaText = "<";
            SearchPatternsAndCheckMatches(patterns, nezaboodkaText,
                "<");
        }

        [TestMethod]
        public void SequenceWithFirstOptionalElementWithExceptionAndSecondElementWithException_NoMatch()
        {
            string patterns = "#Pattern = ?{Any, ~'!'} + {Symbol, ~'<'};";
            string nezaboodkaText = "<";
            AssertNoMatches(patterns, nezaboodkaText);
        }

        [TestMethod]
        public void SequenceWithFirstOptionalElementWithExceptionAndSecondElementWithException()
        {
            string patterns = "#Pattern = ?{Any, ~'<'} + {'<', ~'<-'};";
            string nezaboodkaText = "<";
            SearchPatternsAndCheckMatches(patterns, nezaboodkaText,
                "<");
        }

        [TestMethod]
        public void SequenceWithSecondOptionalElementAndThirdElementWithException_NoMatch()
        {
            string patterns = "#Pattern = '/' + ?Space + {Num, ~(Num + '%')};";
            string nezaboodkaText = "1/4% done";
            AssertNoMatches(patterns, nezaboodkaText);
        }

        [TestMethod]
        public void SequenceWithSecondOptionalElementAndThirdElementWithException_NoMatch_Debug()
        {
            string patterns = @"#Pattern1 = '/' + ?Space + {Num, ~(Num + '%')};
                                #Pattern2 = '/' + {Num, ~(Num + '%')};";
            string nezaboodkaText = "/4%";
            AssertNoMatches(patterns, nezaboodkaText);
        }

        [TestMethod]
        public void AnySpanWithOptionalElementWithExceptionInRightPosition()
        {
            string patterns = "#Pattern = '<' ... ?{Any, ~'>'} + '>';";
            string nezaboodkaText = "<>";
            SearchPatternsAndCheckMatches(patterns, nezaboodkaText,
                "<>");
        }

        [TestMethod]
        public void WordSequence()
        {
            string patterns = "#Pattern = 'Nezaboodka' _ 'Software' _ 'LLC';";
            string text = "Nezaboodka Software LLC";
            SearchPatternsAndCheckMatches(patterns, text,
                "Nezaboodka Software LLC");
        }

        [TestMethod]
        public void WordSequenceWithSequenceOfTokens()
        {
            string patterns = "#Pattern = 'Nezaboodka' _ 'Software' + Space + 'LLC';";
            string text = "Nezaboodka - Software LLC";
            SearchPatternsAndCheckMatches(patterns, text,
                "Nezaboodka - Software LLC");
        }

        [TestMethod]
        public void RepetitionOfSequence()
        {
            string patterns = "#Pattern = [2(Word + Space)];";
            string text = "Nezaboodka Software LLC";
            SearchPatternsAndCheckMatches(patterns, text,
                "Nezaboodka Software ");
        }

        [TestMethod]
        public void RepetitionOfSequenceSelfOverlapped()
        {
            string patterns = "#Pattern = [1-2(Word + Space)];";
            string text = "Nezaboodka Software LLC";
            SearchPatternsAndCheckMatches(patterns, text,
                "Nezaboodka Software ");
        }

        [TestMethod]
        public void RepetitionOfSequenceNoMatch()
        {
            string patterns = "#Pattern = [3+(Word + Space)];";
            string text = "Nezaboodka Software LLC";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void Variation()
        {
            string patterns = "#Pattern = {'one', 'two', 'three'};";
            string text = "One,two,three.";
            SearchPatternsAndCheckMatches(patterns, text,
                "One",
                "two",
                "three");
        }

        [TestMethod]
        public void SequenceWithOptionalVariationInLastPosition()
        {
            string patterns = "#Pattern = Word + [0+ {Word, ',', '.'}];";
            string text = "One,two,three.";
            SearchPatternsAndCheckMatches(patterns, text,
                "One,two,three.");
        }

        [TestMethod]
        public void SequenceWithImplicitOptionalVariationInLastPosition()
        {
            string patterns = "#Pattern = Word + {Symbol, Space, ?'.'};";
            string text = "One?";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void SequenceWithImplicitOptionalVariation()
        {
            string patterns = "#Pattern = Word + {?Symbol, ?Space, ?'.'};";
            string text = "One?";
            SearchPatternsAndCheckMatches(patterns, text,
                "One");
        }

        [TestMethod]
        public void SequenceWithRepetitionOfOptionalToken()
        {
            string patterns = "#Pattern = Word + [1 ?Symbol];";
            string text = "One?";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void SequenceWithRepetitionAndImplicitOptionalVariation()
        {
            string patterns = "#Pattern = Word + [2 {?Symbol, ?Space, ?'.'}];";
            string text = "One?";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void SequenceWithSingleRepetitionAndImplicitOptionalVariation()
        {
            string patterns = "#Pattern = Word + [1 {?Symbol, ?Space, ?'.'}];";
            string text = "One?";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void VariationOfSequences()
        {
            string patterns = "#Pattern = {Word, Word + ',', Word + '.'};";
            string text = "One,two,three.";
            SearchPatternsAndCheckMatches(patterns, text,
                "One,",
                "two,",
                "three.");
        }

        [TestMethod]
        public void RepetitionOfRepetitionWithCandidateLimit()
        {
            string patterns = "#Test = Start + [1+ [1+ Punct]] + End;";
            const string text = ".,;";
            var options = new SearchOptions()
            {
                CandidateLimit = 14
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                ".,;");
        }

        [TestMethod]
        public void RepetitionOfVariationWithRepetitionWithCandidateLimit()
        {
            string patterns = "#Test = Start + [1+ {[1+ Punct], Symbol}] + End;";
            const string text = ".,;";
            var options = new SearchOptions()
            {
                CandidateLimit = 14
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                ".,;");
        }

        [TestMethod]
        public void RepetitionOfNestedVariationsWithRepetitionWithCandidateLimit()
        {
            string patterns = "#Test = Start + [1+ {[1+ {[1+'@'], '$'}], '#'}] + End;";
            const string text = "@@@@@@";
            var options = new SearchOptions()
            {
                CandidateLimit = 114
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "@@@@@@");
        }

        [TestMethod]
        public void RepetitionOfSequenceWithRepetitionInLastPosition()
        {
            string patterns = "#Test = Start + [1+ (Word + [1+ {Space, Punct}])];";
            string text = "Will you do it?";
            SearchPatternsAndCheckMatches(patterns, text,
                text);
        }

        [TestMethod]
        public void RepetitionOfVariationWithVariation()
        {
            string patterns = "#Test = Start + [1+ {Word, {Space, Punct}}];";
            string text = "You , ;will do it";
            SearchPatternsAndCheckMatches(patterns, text,
                text);
        }

        [TestMethod]
        public void RepetitionOfVariationWithRepetition()
        {
            string patterns = "#Test = Start + [1+ {Word, [1+ {Space, Punct}]}];";
            string text = "You , ;will do it";
            SearchPatternsAndCheckMatches(patterns, text,
                text);
        }

        [Ignore]
        [TestMethod]
        public void RepetitionOfVariationWithSequenseWithFirstOptionalElementAndRepetitionOfVariation()
        {
            string patterns = "#Test = Start + [1+ {Word, (?Symbol + [1+ {Space, Punct}])}];";
            string text = "You , ;will do it";
            SearchPatternsAndCheckMatches(patterns, text,
                text);
        }

        [TestMethod]
        public void RepetitionOfVariationWithRepetitionOfVariationWithSequence()
        {
            string patterns = "#Test = Start + [1+ {Word, [1+ {Space + Punct, Punct}]}];";
            string text = "You , ;will,do.it";
            SearchPatternsAndCheckMatches(patterns, text,
                text);
        }

        [TestMethod]
        public void RepetitionOfVariationOfRepetitions()
        {
            string patterns = "#Test = Start + [1+ {[1+ Word], [1+ Space], [1+ Punct]}];";
            string text = "You...will do it";
            SearchPatternsAndCheckMatches(patterns, text,
                text);
        }

        [TestMethod]
        public void RepetitionOfVariationWithRepetitionEquivalentPattern()
        {
            string patterns = "#Test = Start + [1+ {Word, Space, Punct}];";
            string text = "You will do it";
            SearchPatternsAndCheckMatches(patterns, text,
                text);
        }

        [TestMethod]
        public void RepetitionOfRepetitionOfRepetitionOfVariationWithCandidateLimit()
        {
            string patterns = "#Test = Start + [1+ [1+ [1+ {Word, Space, Punct}]]];";
            string text = "You will do it";
            var options = new SearchOptions()
            {
                CandidateLimit = 500,
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                text);
        }

        [TestMethod]
        public void RepetitionOfVariationWithRepetitionDifferentLowBound()
        {
            string patterns = "#Test = Start + [2+ {Word, [1+ {Space, Punct}]}];";
            string text = "You will do it";
            SearchPatternsAndCheckMatches(patterns, text,
                text);
        }

        [TestMethod]
        public void Conjunction()
        {
            string patterns = "#IPhoneAndAndroid = 'iphone' & 'android';";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE?";
            SearchPatternsAndCheckMatches(patterns, text,
                "ANDROID OR IPHONE");
        }

        [TestMethod]
        public void ConjunctionWithThreeElements()
        {
            string patterns = "#IPhoneAndAndroid = ('iphone' & 'android' & 'better');";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE?";
            SearchPatternsAndCheckMatches(patterns, text,
                "ANDROID OR IPHONE THE BETTER");
        }

        [TestMethod]
        public void ConjunctionWithTwoObligatoryAndOneOptionalElements()
        {
            string patterns = "#IPhoneAndAndroid = ('iphone' & 'android' & ?'better');";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE?";
            SearchPatternsAndCheckMatches(patterns, text,
                "ANDROID OR IPHONE THE BETTER");
        }

        [TestMethod]
        public void ConjunctionOfVariations()
        {
            string patterns = "#IPhoneAndAndroid = {'iphone', 'android'} & {'iphone', 'android'};";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE?";
            SearchPatternsAndCheckMatches(patterns, text,
                "ANDROID OR IPHONE");
        }

        [TestMethod]
        public void ConjunctionOfVariationsOverRepeatedKeyword()
        {
            string patterns = "#IPhoneAndAndroid = {'iphone', 'android'} & {'iphone', 'android'};";
            string text = "Android is the Google Android platform";
            SearchPatternsAndCheckMatches(patterns, text,
                "Android is the Google Android");
        }

        [TestMethod]
        public void ConjunctionOfRepetitionsSimple()
        {
            string patterns = "#Test = '.' & [2'*'];";
            string text = ". * ** ***";
            SearchPatternsAndCheckMatches(patterns, text,
                ". * **");
        }

        [TestMethod]
        public void ConjunctionOfRepetitions()
        {
            string patterns = "#Test = [2'*'] & [1-2'markdown'];";
            string text = "In Markdown format * means italic, ** means bold, and *** means bold italic";
            SearchPatternsAndCheckMatches(patterns, text,
                "Markdown format * means italic, **");
        }

        [TestMethod]
        public void ConjunctionOfRepetitionWithOverlappingLongerMatchAndShortReject()
        {
            string patterns = "#Test = 'X' & ([3 Punct + Symbol]);";
            string text = "X,,,,&";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void WordSpanZeroToOneDistance()
        {
            string patterns = "#How = 'how' .. [0-1] .. 'you';";
            string text = "How are you?";
            SearchPatternsAndCheckMatches(patterns, text,
                "How are you");
        }

        [TestMethod]
        public void WordSpan_NoMatch()
        {
            string patterns = "#How = 'how' .. [3-5] .. 'you';";
            string text = "How are you?";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void WordSpanZeroPlusDistanceShortResult()
        {
            string patterns = "#How = 'how' .. [0-15] .. 'you';";
            string text = "How are you today, not bad, are you?";
            SearchPatternsAndCheckMatches(patterns, text,
                "How are you");
        }

        [TestMethod]
        public void WordSpanZeroPlusDistanceToSequence()
        {
            string patterns = "#How = 'how' .. [0-15] .. ('you' + '?');";
            string text = "How are you today, not bad, are you?";
            SearchPatternsAndCheckMatches(patterns, text,
                "How are you today, not bad, are you?");
        }

        [TestMethod]
        public void RepetitionInRightPartOfAnySpanExpression()
        {
            string patterns = "#Test = 'you' ... [2+ {Word, [1+ Blank]}];";
            string text = "You will do it";
            SearchPatternsAndCheckMatches(patterns, text,
                text);
        }

        [TestMethod]
        public void WordSpanUntilPunctuation()
        {
            string patterns = "#How = 'how' .. [0-5] .. '?';";
            string text = "How are you? Fine?";
            SearchPatternsAndCheckMatches(patterns, text,
                "How are you?");
        }

        [TestMethod]
        public void WordSpanUntilSequenceWithOptionalElements()
        {
            string patterns = "#How = 'how' .. [0-5] .. ?'you' + ?'?';";
            string text = "How are you? Fine?";
            SearchPatternsAndCheckMatches(patterns, text,
                "How are you?");
        }

        [TestMethod]
        public void WordSpanUntilSequenceWithOptionalElementsShortResult()
        {
            string patterns = "#How = 'how' .. [0-5] .. ?'you' + ?'?';";
            string text = "How are you, are you?";
            SearchPatternsAndCheckMatches(patterns, text,
                "How are you");
        }

        [TestMethod]
        [Timeout(2 * 1000)] // 2 seconds
        public void WordSpanWordBreak()
        {
            string patterns = "#P5 = Start .. [1-50] .. LineBreak;";
            string text =
                "When it comes to buying one of the best\n" +
                "\n" +
                "smartphones, the first choice can be the hardest: iPhone or Android. \n";
            SearchPatternsAndCheckMatches(patterns, text,
                "When it comes to buying one of the best\n");
        }

        [TestMethod]
        public void WordSpanInRealText()
        {
            string patterns = "#India = {'India' .. [0-5] .. 'IN', 'IN' .. [0-5] .. 'India'};";
            string text = "Seeking to diversify its foreign supplies, India first imported U.S. crude in October...";
            SearchPatternsAndCheckMatches(patterns, text,
                "India first imported U.S. crude in");
        }

        [TestMethod]
        public void NestedVariationsOptimization()
        {
            string pattern =
                "#NameFromEmail = [1+ {Word, {Word, '.', '_', '+', '-'}}] + '@';";
            string text = "Email: dmitry.surkov@nezaboodka.com.";
            var packageBuilderOptions = new PackageBuilderOptions()
            {
                PatternReferencesInlined = true,
                SyntaxInformationBinding = true
            };
            var package = PatternPackage.FromText(pattern, packageBuilderOptions);
            string optimizedPattern =
                "#NameFromEmail = [1+ {Word, Word, '.', '_', '+', '-'}] + '@';";
            string patternsFormatted = package.Syntax.ToString().Replace('"', '\'');
            TestPackageIs(optimizedPattern, patternsFormatted);
            SearchPatternsAndCheckMatches(pattern, text,
                "dmitry.surkov@");
        }

        [TestMethod]
        public void RepetitionOptimizationAfterPatternReferenceSubstitution()
        {
            string patterns =
                "#NameFromEmail = [1+ {Word, LoginName}] + '@';\n" +
                "LoginName = [1+ {Word, '.', '_', '+', '-'}];";
            string text = "Email: dmitry.surkov@nezaboodka.com.";
            var packageBuilderOptions = new PackageBuilderOptions()
            {
                PatternReferencesInlined = true,
                SyntaxInformationBinding = true
            };
            var package = PatternPackage.FromText(patterns, packageBuilderOptions);
            string substitutedPatterns =
                "#NameFromEmail = [1+ {Word, {Word, '.', '_', '+', '-'}}] + '@';\n" +
                "LoginName = [1+ {Word, '.', '_', '+', '-'}];";
            string patternsFormatted = package.Syntax.ToString().Replace('"', '\'');
            TestPackageIs(substitutedPatterns, patternsFormatted);
            SearchPatternsAndCheckMatches(patterns, text,
                "dmitry.surkov@");
        }

        [TestMethod]
        public void SimpleEmail()
        {
            string patterns = @"
                #Email = Word + [0+ {Word, '.', '_', '+', '-'}] +
                    '@' + Word + [1+ ('.' + Word + [0+ {Word, '_', '-'}])];
            ";
            string text = "Email: ivan_shimko-2018@nezaboodka.com.";
            SearchPatternsAndCheckMatches(patterns, text,
                "ivan_shimko-2018@nezaboodka.com");
        }

        [TestMethod]
        public void HashTag()
        {
            string patterns = "#HashTag = '#' + {AlphaNum, Alpha, '_'} + [0+ {Word, '_'}];";
            string text = "#Nezaboodka #NLP #TextAnalysis";
            SearchPatternsAndCheckMatches(patterns, text,
                "#Nezaboodka",
                "#NLP",
                "#TextAnalysis");
        }

        [TestMethod]
        public void HashTagOverIdentifier()
        {
            string patterns = @"
                Identifier = {AlphaNum, Alpha, '_'} + [0+ {Word, '_'}];
                #HashTag = '#' + Identifier;
            ";
            string text = "#Nezaboodka #NLP #TextAnalysis";
            SearchPatternsAndCheckMatches(patterns, text,
                "#Nezaboodka",
                "#NLP",
                "#TextAnalysis");
        }

        [TestMethod]
        public void FourStandardPatternsInlined()
        {
            string patterns = @"
                #PhoneNumber = ?'+' + {Num, '(' + Num + ')'} + [2+ ({'-', Space} + Num)];
                #Email = Word + [0+ {Word, '.', '_', '+', '-'}] + '@' + Word + [1+ ('.' + Word + [0+ {Word, '_', '-'}])];
                #Url = {'http', 'https'} + '://' + (Word + [1+ ('.' + Word + [0+ {Word, '_', '-'}])]) + ?('/' + [0+ {Word, '/', '_', '+', '-', '%'}]) + ?('?' + ?((({AlphaNum, Alpha, '_'} + [0+ {Word, '_'}]) + '=' + ({AlphaNum, Alpha, '_'} + [0+ {Word, '_'}])) + [0+ ('&' + (({AlphaNum, Alpha, '_'} + [0+ {Word, '_'}]) + '=' + ({AlphaNum, Alpha, '_'} + [0+ {Word, '_'}])))]));
                #HashTag = '#' + {AlphaNum, Alpha, '_'} + [0+ {Word, '_'}];
            ";
            string text = "Nezaboodka http://nezaboodka.com +375 33 333-77-78 #NLP #TextAnalysis";
            SearchPatternsAndCheckMatches(patterns, text,
                ("Url", new[] { "http://nezaboodka.com" }),
                ("PhoneNumber", new[] { "+375 33 333-77-78" }),
                ("HashTag", new[] { "#NLP", "#TextAnalysis" })
            );
        }

        [TestMethod]
        public void StandardPatternsWithReferences()
        {
            string patterns = @"
                #PhoneNumber = ?'+' + {Num, '(' + Num + ')'} + [2+ ({'-', Space} + Num)];
                #Email = Word + [0+ {Word, '.', '_', '+', '-'}] + '@' + Domain;
                #Url = {'http', 'https'} + '://' + Domain + ?Path + ?Query;
                Domain = Word + [1+ ('.' + Word + [0+ {Word, '_', '-'}])];
                Path = '/' + [0+ {Word, '/', '_', '+', '-', '%'}];
                Query = '?' + ?(QueryParam + [0+ ('&' + QueryParam)]);
                QueryParam = Identifier + '=' + Identifier;
                Identifier = {AlphaNum, Alpha, '_'} + [0+ {Word, '_'}];
                #HashTag = '#' + Identifier;
            ";
            string text = "Nezaboodka http://nezaboodka.com +375 33 333-77-78 #NLP #TextAnalysis";
            SearchPatternsAndCheckMatches(patterns, text,
                ("Url", new[] { "http://nezaboodka.com" }),
                ("PhoneNumber", new[] { "+375 33 333-77-78" }),
                ("HashTag", new[] { "#NLP", "#TextAnalysis" })
            );
        }

        [TestMethod]
        public void UrlOverDomainPathQueryOverQueryParamIdentifier()
        {
            string patterns = @"
                #Url = {'http', 'https'} + '://' + Domain + ?Path + ?Query;
                Domain = Word + [1+ ('.' + Word + [0+ {Word, '_', '-'}])];
                Path = '/' + [0+ {Word, '/', '_', '+', '-', '%'}];
                Query = '?' + ?(QueryParam + [0+ ('&' + QueryParam)]);
                QueryParam = Identifier + '=' + Identifier;
                Identifier = {AlphaNum, Alpha, '_'} + [0+ {Word, '_'}];
            ";
            string text = "Nezaboodka http://nezaboodka.com +375 33 333-77-78 #NLP #TextAnalysis";
            SearchPatternsAndCheckMatches(patterns, text,
                "http://nezaboodka.com");
        }

        [TestMethod]
        public void HtmlComment()
        {
            string patterns = "#HtmlComment = '<!--' ... '-->';";
            string text = "Hi, this is a <!-- comment -- real comment in HTML --> and this as --> text";
            SearchPatternsAndCheckMatches(patterns, text,
                "<!-- comment -- real comment in HTML -->");
        }

        [TestMethod]
        public void AnySpanOnSymbols()
        {
            string patterns = "#AnySpan = '*' ... '#';";
            string text = "*# *A# *B#";
            SearchPatternsAndCheckMatches(patterns, text,
                "*#",
                "*A#",
                "*B#"
            );
        }

        [TestMethod]
        public void AnySpanWithExtractionOnSymbols()
        {
            string patterns = "#AnySpan(X) = '*' .. X .. '#';";
            string text = "*# *A# *B#";
            SearchPatternsAndCheckMatches(patterns, text,
                "*#",
                "*A#",
                "*B#"
            );
        }

        [TestMethod]
        public void AnySpanArrows()
        {
            string patterns = "#HtmlComment = '<-' ... '->';";
            string text = "<-arrow1->arrow2->arrow3";
            SearchPatternsAndCheckMatches(patterns, text,
                "<-arrow1->");
        }

        [TestMethod]
        public void AnySpanOfSingleTokens()
        {
            string patterns = "#HtmlBrTag = '<' ... '>';";
            string text = "<br><br>\ntext";
            SearchPatternsAndCheckMatches(patterns, text,
                "<br>",
                "<br>");
        }

        [TestMethod]
        public void AnySpanChainOfSingleTokens()
        {
            string patterns = "#HtmlBrTag = '<' ... '>' ... '>';";
            string text = "<br><br>\ntext";
            SearchPatternsAndCheckMatches(patterns, text,
                "<br><br>");
        }

        [TestMethod]
        public void AnySpanToCaseSensitiveToken()
        {
            string patterns = "#P = 'android' ... 'SMARTPHONE'!;";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE";
            SearchPatternsAndCheckMatches(patterns, text,
                "ANDROID OR IPHONE THE BETTER SMARTPHONE");
        }

        [TestMethod]
        public void AnySpanToSingleWordPrefix()
        {
            string patterns = "#P = 'android' ... 'smart'*;";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE";
            SearchPatternsAndCheckMatches(patterns, text,
                "ANDROID OR IPHONE THE BETTER SMARTPHONE");
        }


        [TestMethod]
        public void AnySpanToVariationOfWordPrefix()
        {
            string patterns = "#P = 'android' ... {'smart'*, 'tabl'*, 'comput'*};";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE";
            SearchPatternsAndCheckMatches(patterns, text,
                "ANDROID OR IPHONE THE BETTER SMARTPHONE");
        }

        [TestMethod]
        public void AnySpanToNumber()
        {
            string patterns = "#P = 'android' ... Num;";
            string text = "IS ANDROID OR IPHONE 5 THE BETTER SMARTPHONE";
            SearchPatternsAndCheckMatches(patterns, text,
                "ANDROID OR IPHONE 5");
        }

        [TestMethod]
        public void AnySpanWithSequence()
        {
            string patterns = "#HtmlBrTag = '<br' ... '>' + LineBreak;";
            string text = "<br><br>\ntext";
            SearchPatternsAndCheckMatches(patterns, text,
                "<br><br>\n");
        }

        [TestMethod]
        public void SequenceWithAnySpan()
        {
            string patterns = "#HtmlBrTag = ('<br' ... '>') + LineBreak;";
            string text = "<br><br>\ntext";
            SearchPatternsAndCheckMatches(patterns, text,
                "<br>\n");
        }

        [TestMethod]
        public void AnySpanWithExceptionInRightPart()
        {
            string patterns = "#T = '<' ... {'>', ~'>@'};";
            string text = "<first>@<second>";
            SearchPatternsAndCheckMatches(patterns, text,
                "<first>@<second>");
        }

        [TestMethod]
        public void AnySpanWithOptimizationInOutside()
        {
            string patterns = @"
                SentenceSeparator = {End, '.', '!', '?', [2 LineBreak], ~'.'+Word};
                Sentence = Word ... SentenceSeparator;
                Test = 'test';
                #SentenceWithoutTest = Sentence @outside Test;
            ";
            string text = "First sentence. Second with test. Third without.";
            SearchPatternsAndCheckMatches(patterns, text,
                "First sentence.",
                "Third without.");
        }

        [TestMethod]
        public void AnySpanWithOptimizationAndCandidateLimit()
        {
            string patterns = "#W = Word ... End;";
            string text = "One Two Three Four Five";
            var options = new SearchOptions()
            {
                CandidateLimit = 1
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "One Two Three Four Five");
        }

        [TestMethod]
        public void AnySpanWithOptimizationInSequenceAndCandidateLimit()
        {
            string patterns = "#W = (Word ... '.')+'@';";
            string text = "One Two.@ Three Four Five.@";
            var options = new SearchOptions()
            {
                CandidateLimit = 1
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "One Two.@",
                "Three Four Five.@");
        }

        [TestMethod]
        public void AnySpanToLastQuoteInRowOdd()
        {
            string patterns = "#T = \"'\" ... {\"'\", ~\"''\"};";
            string text = "'It''s ok'''";
            SearchPatternsAndCheckMatches(patterns, text,
                "'It''",
                "'''");
        }

        [TestMethod]
        public void AnySpanToLastQuoteInRowEven()
        {
            string patterns = "#T = \"'\" ... {\"'\", ~\"''\"};";
            string text = "'It''s ok''''";
            SearchPatternsAndCheckMatches(patterns, text,
                "'It''",
                "''''");
        }

        [TestMethod]
        public void AnySpanToSingleQuoteOnly()
        {
            string patterns = "#T = \"'\" ... (\"'\" @outside \"''\");";
            string text = "'It''s ok'''maybe'";
            SearchPatternsAndCheckMatches(patterns, text,
                "'It''s ok'''maybe'");
        }

        [TestMethod]
        public void AnySpanFromSingleQuoteOnlyToSingleQuoteOnly()
        {
            string patterns = "#T = (\"'\" @outside \"''\") ... (\"'\" @outside \"''\");";
            string text = "'It''s ok'''maybe'";
            SearchPatternsAndCheckMatches(patterns, text,
                "'It''s ok'''maybe'");
        }

        [TestMethod]
        public void AnySpanFromQuoteInsideStartEndToSingleQuoteOnly()
        {
            string patterns = "#T = (\"'\" @inside Start...End) ... (\"'\" @outside \"''\");";
            string text = "'It''s ok'''maybe'";
            SearchPatternsAndCheckMatches(patterns, text,
                "'It''s ok'''maybe'");
        }

        [TestMethod]
        public void HtmlImgSrc()
        {
            string patterns = "#ImgSrc = '<img src=\"' ... '\"';";
            string text = "<img src=\"https://nezaboodka.com/\">";
            SearchPatternsAndCheckMatches(patterns, text,
                "<img src=\"https://nezaboodka.com/\"");
        }

        [TestMethod]
        public void SimpleAnySpanFromQuoteToPairingQuote()
        {
            string patterns = "#ImgSrc(Q) = '<img src=' + Q:{'\"', \"'\"} ... Q;";
            string text = "<img src=\"https://nezaboodka.com/\">\n<img src='./assets/favicon.png'>";
            SearchPatternsAndCheckMatches(patterns, text,
                "<img src=\"https://nezaboodka.com/\"",
                "<img src='./assets/favicon.png'");
        }

        [TestMethod]
        public void SimpleAnySpanWithSameEndOfLeftAndStartOfRight()
        {
            string patterns = "#T = '_'+'@' ... '@';";
            string text = "First._@Second@ Third._@Fourth@";
            SearchPatternsAndCheckMatches(patterns, text,
                "_@Second@",
                "_@Fourth@");
        }

        [TestMethod]
        public void SingleQuoteOnly()
        {
            string patterns = "#T = (\"'\" @outside \"''\");";
            string text = "'It''s ok'''maybe'";
            SearchPatternsAndCheckMatches(patterns, text,
                "'",
                "'");
        }

        [TestMethod]
        public void AnySpanEscapedQuotesByDuplicationOutside_NoMatch()
        {
            string patterns = "#T = \"'\" ... (\"'\" @outside \"''\");";
            string text = "'Hello, that''s ok''''";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void HouseInsideWordToDot()
        {
            string patterns = "#A = 'house' @inside B;\n" +
                "B = Word ... '.';";
            string text = "This house is too small.";
            SearchPatternsAndCheckMatches(patterns, text,
                "house");
        }

        [TestMethod]
        public void StartToHouseIsInsideWordToDot()
        {
            string patterns = "#A = Start ... ('house is' @inside B);\n" +
                "B = Word ... '.';";
            string text = "This house is too small.";
            SearchPatternsAndCheckMatches(patterns, text,
                "This house is");
        }

        [TestMethod]
        public void HouseInsideWordToDotBothTarget()
        {
            string patterns = "#A = 'house' @inside B;\n" +
                "#B = Word ... '.';";
            string text = "This house is too small.";
            SearchPatternsAndCheckMatches(patterns, text,
                ("A", new[] { "house" }),
                ("B", new[] { "This house is too small." })
            );
        }

        [TestMethod]
        public void HouseInsideWordToDotInlined()
        {
            string patterns = "#T = 'house' @inside (Word ... '.');";
            string text = "This house is too small.";
            SearchPatternsAndCheckMatches(patterns, text,
                "house");
        }

        [TestMethod]
        public void HouseWithExceptionInsideWordToDot()
        {
            string patterns = "#T = {'house', ~'house.com.org'} @inside (Word ... '.');";
            string text = "The house.com";
            SearchPatternsAndCheckMatches(patterns, text,
                "house");
        }

        [TestMethod]
        public void ExceptionInVariationAtFirstPositionInPattern()
        {
            string patterns = "#Microsoft = {'microsoft',  ~'microsoft must die'};";
            string text = "He said: 'Microsoft must die!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void ExceptionInVariationAtFirstPositionInPatternPositive()
        {
            string patterns = "#Microsoft = {'microsoft',  ~'microsoft must die'};";
            string text = "He said: 'Microsoft is good!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Microsoft");
        }

        [TestMethod]
        public void ExceptionInVariationAtNotFirstPositionInPattern()
        {
            string patterns = "#Microsoft = 'damn' + Space + {'microsoft',  ~'microsoft must die'};";
            string text = "He said: 'Damn Microsoft must die!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void ExceptionInVariationAtNotFirstPositionInPatternPositive()
        {
            string patterns = "#Microsoft = 'damn' + Space + {'microsoft',  ~'microsoft must die'};";
            string text = "He said: 'Damn Microsoft is good!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Damn Microsoft");
        }

        [TestMethod]
        public void ExceptionInVariationAtAnySpanAndSimpleAnySpan()
        {
            string patterns = @"
                #T1 = Start ... 'hello';
                #T2 = Start ... {'hello', ~'hello world'};
            ";
            string text = "Damn hello world!'";
            SearchPatternsAndCheckMatches(patterns, text,
                ("T1", new[] { "Damn hello" }));
        }

        [TestMethod]
        public void ExceptionInVariationAtAnySpan()
        {
            string patterns = "#Microsoft = 'damn' ... {'microsoft',  ~'microsoft must die'};";
            string text = "He said: 'Damn right, Microsoft must die!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void ExceptionInVariationAtAnySpanSkip()
        {
            string patterns = "#Microsoft = 'damn' ... {'microsoft',  ~'microsoft must die'};";
            string text = "He said: 'Damn right, Microsoft must die! Microsoft is evil!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Damn right, Microsoft must die! Microsoft");
        }

        [TestMethod]
        public void ExceptionInVariationAtAnySpanPositive()
        {
            string patterns = "#Microsoft = 'damn' ... {'microsoft',  ~'microsoft must die'};";
            string text = "He said: 'Damn right, Microsoft is good!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Damn right, Microsoft");
        }

        [TestMethod]
        public void ExceptionInVariationAtFirstPositionInPatternLongerNestedPositive()
        {
            string patterns = "#Microsoft = {'microsoft',  ~{'microsoft must', ~'microsoft must die'}};";
            string text = "He said: 'Damn Microsoft must die!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Microsoft");
        }

        [TestMethod]
        public void ExceptionInVariationAtNotFirstPositionInPatternLongerNestedPositive()
        {
            string patterns = "#Microsoft = 'damn' + Space + {'microsoft',  ~{'microsoft must', ~'microsoft must die'}};";
            string text = "He said: 'Damn Microsoft must die!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Damn Microsoft");
        }

        [TestMethod]
        public void ExceptionInVariationAtAnySpanLongerNestedPositive()
        {
            string patterns = "#Microsoft = 'damn' ... {'microsoft',  ~{'microsoft must', ~'microsoft must die'}};";
            string text = "He said: 'Damn right, Microsoft is good!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Damn right, Microsoft");
        }

        [TestMethod]
        public void ExceptionInVariationAtFirstPositionInPatternShorterNestedPositive()
        {
            string patterns = "#Microsoft = {'microsoft',  ~{'microsoft must die', ~'microsoft must'}};";
            string text = "He said: 'Damn Microsoft must die!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Microsoft");
        }

        [TestMethod]
        public void ExceptionInVariationAtNotFirstPositionInPatternShorterNestedPositive()
        {
            string patterns = "#Microsoft = 'damn' + Space + {'microsoft',  ~{'microsoft must die', ~'microsoft must'}};";
            string text = "He said: 'Damn Microsoft must die!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Damn Microsoft");
        }

        [TestMethod]
        public void ExceptionInVariationAtAnySpanShorterNestedPositive()
        {
            string patterns = "#Microsoft = 'damn' ... {'microsoft',  ~{'microsoft must die', ~'microsoft must'}};";
            string text = "He said: 'Damn right, Microsoft must die!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Damn right, Microsoft");
        }

        [TestMethod]
        public void ExceptionInVariationAtFirstPositionInPatternNestedCascade()
        {
            string patterns = "#Microsoft = {'microsoft',  ~{'microsoft must', ~{'microsoft must die', ~'microsoft must die!'}}};";
            string text = "He said: 'Damn Microsoft must die!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void ExceptionInVariationAtNotFirstPositionInPatternNestedCascade()
        {
            string patterns = "#Microsoft = 'damn' + Space + {'microsoft',  ~{'microsoft must', ~{'microsoft must die', ~'microsoft must die!'}}};";
            string text = "He said: 'Damn Microsoft must die!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void ExceptionInVariationAtAnySpanNestedCascade()
        {
            string patterns = "#Microsoft = 'damn' ... {'microsoft',  ~{'microsoft must', ~{'microsoft must die', ~'microsoft must die!'}}};";
            string text = "He said: 'Damn right, Microsoft must die!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void ExceptionInVariationAtNotFirstPositionInPatternTokenKind()
        {
            string patterns = "#Microsoft = 'damn' + Space + {'microsoft',  ~Word + '.'};";
            string text = "He said: 'Damn Microsoft.'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void ExceptionInVariationAtAnySpanTokenKind()
        {
            string patterns = "#Microsoft = 'damn' ... {'microsoft',  ~Word + '.'};";
            string text = "He said: 'Damn right, Microsoft.'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void ExceptionInVariationAtFirstPositionInPatternWithUnmatchingException()
        {
            string patterns = "#Microsoft = {'microsoft', {'google', ~'microsoft'},  ~'microsoft must die'};";
            string text = "He said: 'Damn Microsoft must live!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Microsoft");
        }

        [TestMethod]
        public void ExceptionInVariationAtNotFirstPositionInPatternWithUnmatchingException()
        {
            string patterns = "#Microsoft = 'damn' + Space + {'microsoft', {'google', ~'microsoft'},  ~'microsoft must die'};";
            string text = "He said: 'Damn Microsoft must live!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Damn Microsoft");
        }

        [TestMethod]
        public void ExceptionInVariationAtAnySpanWithUnmatchingException()
        {
            string patterns = "#Microsoft = 'damn' ... {'microsoft', {'google', ~'microsoft'},  ~'microsoft must die'};";
            string text = "He said: 'Damn right, Microsoft must live!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Damn right, Microsoft");
        }

        [TestMethod]
        public void ExceptionInTopLevelVariationAtFirstPositionInPattern()
        {
            string patterns = "#Microsoft = {'google', {'microsoft', ~'drive'},  ~'microsoft must die'};";
            string text = "He said: 'Damn Microsoft must die!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void ExceptionInTopLevelVariationAtNotFirstPositionInPattern()
        {
            string patterns = "#Microsoft = 'damn' + Space + {'google', {'microsoft', ~'drive'},  ~'microsoft must die'};";
            string text = "He said: 'Damn Microsoft must die!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void ExceptionInTopLevelVariationAtAnySpan()
        {
            string patterns = "#Microsoft = 'damn' ... {'google', {'microsoft', ~'drive'},  ~'microsoft must die'};";
            string text = "He said: 'Damn Microsoft must die!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void ExceptionInConjunction()
        {
            string patterns = "#Pattern = {'google', ~'google drive'} & {'microsoft', ~'microsoft office'};";
            string text = "Microsoft acquires Google Drive";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void ExceptionInConjunctionPositive()
        {
            string patterns = "#Pattern = {'google', ~'google drive'} & {'microsoft', ~'microsoft office'};";
            string text = "Google acquires Microsoft";
            SearchPatternsAndCheckMatches(patterns, text,
                "Google acquires Microsoft");
        }

        [TestMethod]
        public void ExceptionWithAnySpan()
        {
            string patterns = "#Microsoft = {'microsoft', ~('microsoft' ... '@')};";
            string text = "The Microsoft hater said:@ 'I hate MICROSOFT!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "MICROSOFT"
            );
        }

        [TestMethod]
        public void ExceptionWithAnySpanPositive()
        {
            string patterns = "#Microsoft = {'microsoft', ~('microsoft' ... '@')};";
            string text = "The Microsoft hater said: 'I hate MICROSOFT!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Microsoft",
                "MICROSOFT"
            );
        }

        [TestMethod]
        public void ExceptionWithConjunctionWihException()
        {
            string patterns = "#MicrosoftOverGoogle = {'microsoft', ~('microsoft' & {'google', ~'google drive'} @inside Start ... '.')};";
            string text = "Microsoft tries its best to make OneDrive better than Google Drive and beat Google in cloud storage technologies field.";
            SearchPatternsAndCheckMatches(patterns, text,
                "Microsoft"
            );
        }

        [TestMethod]
        public void OverlapsInResultsEffectedByException()
        {
            string patterns = "#Test = {'a b', 'b a', ~'a b a b a c'};";
            string text = "a b a b a b";
            SearchPatternsAndCheckMatches(patterns, text,
                "a b",
                "a b",
                "a b");
        }

        [TestMethod]
        public void SentenceOptimalPattern()
        {
            string patterns = @"
                SentenceSeparator = {End, '.', '!', '?', [2 LineBreak], ~'.'+Word};
                #Sentence = Word ... SentenceSeparator;
            ";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android. Last.";
            SearchPatternsAndCheckMatches(patterns, text,
                "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n",
                "When it comes to buying one of the best smartphones,\nthe first choice can be the hardest (www.google.com): iPhone or Android.",
                "Last.");
        }

        [TestMethod]
        public void SentenceOptimalPatternSimpler()
        {
            string patterns = @"
                SentenceSeparator = {End, '.', '!', '?', [2 LineBreak], ~'.'+Word};
                #Sentence = Word ... SentenceSeparator;
            ";
            string text =
                "When\n. Last.";
            SearchPatternsAndCheckMatches(patterns, text,
                "When\n.",
                "Last.");
        }

        [TestMethod]
        public void SentenceOptimalPatternSimple()
        {
            string patterns = "#Sentence = Word ... {'.', ~'.'+Word};";
            string text = "First. Last.";
            SearchPatternsAndCheckMatches(patterns, text,
                "First.",
                "Last.");
        }

        [TestMethod]
        public void SentenceOptimalPatternSimpleWithException()
        {
            string patterns = "#Sentence = Word ... {'.', ~'.'+Word};";
            string text = "First .com. Last.";
            SearchPatternsAndCheckMatches(patterns, text,
                "First .com.",
                "Last.");
        }

        [TestMethod]
        public void SentenceOptimalPatternSimplest()
        {
            string patterns = "#Sentence = Word ... {'.', ~'.'+Word};";
            string text = "First. .";
            SearchPatternsAndCheckMatches(patterns, text,
                "First.");
        }

        [TestMethod]
        public void GoogleInsideBrackets()
        {
            string patterns = @"
                Brackets = '(' ... ')';
                #T = 'Google' @inside Brackets;
            ";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            SearchPatternsAndCheckMatches(patterns, text,
                "google");
        }

        [TestMethod]
        public void GoogleInsideGoogle()
        {
            string patterns = "#T = 'Google' @inside 'Google';";
            string text = "www.google.com";
            SearchPatternsAndCheckMatches(patterns, text,
                "google");
        }

        [TestMethod]
        public void GoogleInsideSquareBracketsNoMatch()
        {
            string patterns = @"
                Brackets = '[' ... ']';
                #T = 'Google' @inside Brackets;
            ";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void GoogleSequenceInsideGoogleNoMatch()
        {
            string patterns = "#T = 'www.google.com' @inside 'Google';";
            string text = "www.google.com";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void GoogleComInsideBrackets()
        {
            string patterns = @"
                Brackets = '(' ... ')';
                #T = 'google.com' @inside Brackets;
            ";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            SearchPatternsAndCheckMatches(patterns, text,
                "google.com");
        }

        [TestMethod]
        public void GoogleInsideBracketsInsideSentenceWithReferences()
        {
            string patterns = @"
                SentenceSeparator = {End, '.', '!', '?', [2 LineBreak], ~'.'+Word};
                Sentence = Word ... SentenceSeparator;
                OpenBracket = '(';
                ClosingBracket = ')';
                BracketsInSentence = (OpenBracket ... ClosingBracket) @inside Sentence;
                #T = 'Google' @inside BracketsInSentence;
            ";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            SearchPatternsAndCheckMatches(patterns, text,
                "google");
        }

        [TestMethod]
        public void GoogleInsideBracketsInsideSentenceInlined()
        {
            string patterns = @"
                SentenceSeparator = {End, '.', '!', '?', [2 LineBreak], ~'.'+Word};
                Sentence = Word ... SentenceSeparator;
                #T = 'Google' @inside '('...')' @inside Sentence;
            ";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            SearchPatternsAndCheckMatches(patterns, text,
                "google");
        }

        [TestMethod]
        public void GoogleInsideBracketsSyntaxInlined()
        {
            string patterns = "#T = 'Google' @inside '(' ... ')';";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            SearchPatternsAndCheckMatches(patterns, text,
                "google");
        }

        [TestMethod]
        public void GoogleAtFirstPositionInInside()
        {
            string patterns = @"
                GoogleCom = 'Google' ... 'com';
                #T1 = 'Google' @inside GoogleCom;
                #T2 = 'G'* @inside GoogleCom;
                #T3 = 'com' @inside GoogleCom;
                #T4 = '.' @inside GoogleCom;
                #T5 = ('Google' ... 'Android') @inside GoogleCom;
            ";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            SearchPatternsAndCheckMatches(patterns, text,
                ("T1", new[] { "google" }),
                ("T2", new[] { "google" }),
                ("T3", new[] { "com" }),
                ("T4", new[] { "." })
            );
        }

        [TestMethod]
        public void GoogleAtFirstPositionInInsideWithException()
        {
            string patterns = @"
                Brackets = '(' ... {')', ~'): iPhone or XXX'};
                #T1 = 'Google' @inside Brackets;
                #T2 = ('Google' ... ':') @inside Brackets;
                #T3 = ('Google' ... 'iPhone') @inside Brackets;
            ";
            string text = "the first choice can be the hardest (www.google.com): iPhone or Android.";
            SearchPatternsAndCheckMatches(patterns, text,
                ("T1", new[] { "google" })
            );
        }

        [TestMethod]
        public void GoogleInsideOpenBracketsToEnd()
        {
            string patterns = @"
                OpenBracketToEnd = '(' ... End;
                #T1 = 'Google' @inside OpenBracketToEnd;
                #T2 = ('(' ... ':') @inside OpenBracketToEnd;
                #T3 = ('Google' ... 'iPhone') @inside OpenBracketToEnd;
            ";
            string text = "the first choice can be the hardest (www.google.com): iPhone or Android.";
            SearchPatternsAndCheckMatches(patterns, text,
                ("T1", new[] { "google" }),
                ("T2", new[] { "(www.google.com):" }),
                ("T3", new[] { "google.com): iPhone" })
            );
        }

        [TestMethod]
        public void GoogleWithExceptionInsideBracketsNoMatch()
        {
            string patterns = @"
                Brackets = '(' ... ')';
                #T = {'Google', ~'google.com'} @inside Brackets;
            ";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsideSimple()
        {
            string patterns = "#Microsoft = 'microsoft' @outside 'I hate microsoft';";
            string text = "He said: 'I hate Microsoft!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsideAnySpan()
        {
            string patterns = "#MsHater = 'I hate microsoft' @outside (':' ... '!');";
            string text = "He said: 'I hate Microsoft!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsideQuotes()
        {
            string patterns = "#MsHater = 'I hate microsoft' @outside ('''' ... '''');";
            string text = "He said: 'I hate Microsoft!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsideCascadePositive()
        {
            string patterns = "#Microsoft = 'microsoft' @outside ('hate microsoft' @outside 'I hate microsoft');";
            string text = "He said: 'I hate Microsoft!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Microsoft");
        }

        [TestMethod]
        public void OutsideCascadeNoMatch()
        {
            string patterns = "#Microsoft = 'microsoft' @outside ('hate microsoft' @outside ('I hate microsoft' @outside ':'...'!'));";
            string text = "He said: 'I hate Microsoft!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsideSimplePositive()
        {
            string patterns = "#Microsoft = 'microsoft' @outside 'I hate microsoft';";
            string text = "He said: 'I love Microsoft!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Microsoft");
        }

        [TestMethod]
        public void OutsideWithOuterVariationWithExceptionPositive()
        {
            string patterns = "#Microsoft = 'microsoft' @outside {'I hate microsoft', ~('I' ... '@')};";
            string text = "The Microsoft (1) hater said: 'I hate Microsoft (2)! MICROSOFT (3) is bad!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Microsoft",
                "MICROSOFT"
            );
        }

        [TestMethod]
        public void OutsideSequenceWithOuterVariationWithExceptionPositive()
        {
            string patterns =
                "#Microsoft = 'microsoft' + Space + '(' + Num + ')' @outside {'I hate microsoft', ~('I' ... '@')};";
            string text = "The Microsoft (1) hater said: 'I hate Microsoft (2)! MICROSOFT (3) is bad!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Microsoft (1)",
                "MICROSOFT (3)"
            );
        }

        [TestMethod]
        public void OutsideWithBodyLongerThanOuterPattern()
        {
            string patterns = "#Ms = 'microsoft office' @outside 'I hate microsoft';";
            string text = "He said: 'I hate Microsoft Office!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsideWithBodyWithExceptionInReferenceCascade()
        {
            string patterns = @"
                Ms = {'microsoft', ~'microsoft office word'};
                MsOffice = Ms + Space + 'office';
                #Microsoft = MsOffice @outside 'I hate microsoft';
                #I = 'I' @outside 'I hate microsoft';
            ";
            string text = "He said: 'I hate Microsoft Office!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsideWithShorterOuterPatternToken()
        {
            string patterns = "#MsHater = 'I hate microsoft office' @outside 'microsoft';";
            string text = "He said: 'I hate microsoft office!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsideWithShorterOuterPatternTokenPositive()
        {
            string patterns = "#MsHater = 'I hate microsoft office' @outside 'google';";
            string text = "He said: 'I hate microsoft office!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "I hate microsoft office");
        }

        [TestMethod]
        public void OutsideWithShorterOuterPatternSequence()
        {
            string patterns = "#MsHater = 'I hate microsoft office' @outside 'hate microsoft';";
            string text = "He said: 'I hate microsoft office!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsideWithShorterOuterPatternSequencePositive()
        {
            string patterns = "#MsHater = 'I hate microsoft office' @outside 'hate google';";
            string text = "He said: 'I hate microsoft office!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "I hate microsoft office");
        }

        [TestMethod]
        public void OutsideWithSameTokenAsBodyAndOuterPattern()
        {
            string patterns = "#MsHater = 'microsoft' @outside 'microsoft';";
            string text = "He said: 'I hate microsoft office!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsideWithSameBodyAndOuterPattern()
        {
            string patterns = @"
                HateMs = 'I hate microsoft office';
                #MsHater = HateMs @outside HateMs;
            ";
            string text = "He said: 'I hate microsoft office!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsideWithSameBodyAndOuterPatternInlined()
        {
            string patterns = "#MsHater = 'I hate microsoft office' @outside 'I hate microsoft office';";
            string text = "He said: 'I hate microsoft office!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsidesInSequencePositive()
        {
            string patterns =
                "#Microsoft = ('microsoft' @outside 'I hate microsoft') + Space + ('office' @outside 'office word');";
            string text = "He said: 'I love Microsoft Office very much!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Microsoft Office");
        }

        [TestMethod]
        public void OutsidesWithCommonOuterPatternInSequencePositive()
        {
            string patterns = @"
                Outer = 'microsoft office word';
                #Microsoft = ('microsoft' @outside Outer) + Space + ('office' @outside Outer);
            ";
            string text = "He said: 'I love Microsoft Office very much!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Microsoft Office");
        }

        [TestMethod]
        public void OutsidesWithCommonOuterPatternInSequence()
        {
            string patterns = @"
                Outer = 'microsoft office word';
                #Microsoft = ('microsoft' @outside Outer) + Space + ('office' @outside Outer);
            ";
            string text = "He said: 'I love Microsoft Office Word very much!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsidesWithCommonOuterPatternAndExceptionsInSequencePositive()
        {
            string patterns = @"
                Outer = 'microsoft office word';
                #Microsoft = ('microsoft' @outside Outer) + Space + ('office' @outside Outer);
            ";
            string text = "He said: 'I love Microsoft Office very much!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Microsoft Office");
        }

        [TestMethod]
        public void OutsideWithRightOverlap()
        {
            string patterns = "#Microsoft = 'microsoft office' @outside 'office word';";
            string text = "He said: 'I love Microsoft Office Word very much!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsidesWithSameEndAndCommonOuterPattern()
        {
            string patterns = @"
                Outer = {'word', ~'word' + Space + 'is'};
                #MsOfficeWord = 'microsoft office word' @outside Outer;
                #OfficeWord = 'office word' @outside Outer;
                #MyWord = 'word' @outside Outer;
            ";
            string text = "He said: 'I love Microsoft Office Word very much!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsideWord()
        {
            string patterns = "#MsOfficeWord = 'microsoft office' @outside Word;";
            string text = "He said: 'I love Microsoft Office Word very much!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsideAndInsideCombinationNoMatch()
        {
            string patterns = "#Microsoft = ('microsoft' @inside 'I hate microsoft') @outside 'microsoft office';";
            string text = "He said: 'I hate Microsoft Office very much!'";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsideAndInsideCombinationPositive()
        {
            string patterns = "#Microsoft = ('microsoft' @inside 'I hate microsoft') @outside 'microsoft office word';";
            string text = "He said: 'I hate Microsoft Office very much!'";
            SearchPatternsAndCheckMatches(patterns, text,
                "Microsoft");
        }

        [TestMethod]
        public void HtmlTagAttributesExtraction()
        {
            string patterns = @"
                #TagStart(Name, AttrList) = '<' + Name:Word + AttrList:TagAttrList + ?'/' + '>';
                TagAttrList(Attr) = [0+ (?Space + Attr:TagAttr + ?Space)];
                TagAttr(Name, Value1, Value2) = Name:Word + ?Space + '='
                    + {TagAttrValue(Value1:Value), TagAttrValue2(Value2:Value)};
                TagAttrValue(Value) = '""' + Value:[0+ {Any, ~'""'}] + '""';
                TagAttrValue2(Value) = ""'"" + Value:[0+ {Any, ~""'""}] + ""'"";
            ";
            string text = @"<h1 class=""Hello""><h2>Hello World!</h2></h1>";
            SearchPatternsAndCheckMatches(patterns, text,
                @"<h1 class=""Hello"">",
                "<h2>");
            SearchPatternsAndCheckExtractions(patterns, text,
                ("TagStart.Name", new[] { "h1" }),
                ("TagStart.AttrList", new[] { @" class=""Hello""" }),
                ("TagStart.Name", new[] { "h2" })
            );
        }

        [TestMethod]
        public void HtmlTagAttributesExtractionWithReferences()
        {
            string patterns = @"
                #TagStart(Name, AttrList) = '<' + Name:Word + AttrList:TagAttrList + ?'/' + '>';
                TagAttrList(Attr) = [0+ (?Space + Attr:TagAttr + ?Space)];
                TagAttr(Name, Value) = Name:Word + ?Space + '=' + TagAttrValue(Value:Value);
                TagAttrValue(Value, ~Q) = Q:{'""', ""'""} + Value:[0+ {Any, ~Q}] + Q;
            ";
            string text = @"<h1 class=""Hello""><h2>Hello World!</h2></h1><h3 class='World'></h3>";
            SearchPatternsAndCheckMatches(patterns, text,
                @"<h1 class=""Hello"">",
                "<h2>",
                "<h3 class='World'>");
            SearchPatternsAndCheckExtractions(patterns, text,
                ("TagStart.Name", new[] { "h1" }),
                ("TagStart.AttrList", new[] { @" class=""Hello""" }),

                ("TagStart.Name", new[] { "h2" }),

                ("TagStart.Name", new[] { "h3" }),
                ("TagStart.AttrList", new[] { " class='World'" })
            );
        }

        [TestMethod]
        public void ExtractionReferenceSimple()
        {
            string patterns = @"
                Heading = {'h1', 'h2', 'h3', 'h4', 'h5' ,'h6', 'h7'};
                #HtmlHeaderTags(H) = '<' + H:Heading ... '>' ... ('</' + H + '>');
            ";
            string text = @"
                <h1>One</h1>
                <h2>Two<h3>Three</h3></h2>
                <h4></h4>
            ";
            // Самопересечения устранены:
            SearchPatternsAndCheckMatches(patterns, text,
                "<h1>One</h1>",
                "<h2>Two<h3>Three</h3></h2>",
                "<h4></h4>");
        }

        [TestMethod]
        public void ExtractionReferenceInAnySpan()
        {
            string patterns = @"
                Heading = {'h1', 'h2', 'h3', 'h4', 'h5' ,'h6', 'h7'};
                #HtmlHeaderTags(H, Content) = '<' + H:Heading ... '>' .. Content .. ('</' + H + '>');
            ";
            string text = @"
                <h1>One</h1>
                <h2>Two<h3>Three</h3></h2>
                <h4></h4>
            ";
            // Самопересечения устранены:
            SearchPatternsAndCheckMatches(patterns, text,
                "<h1>One</h1>",
                "<h2>Two<h3>Three</h3></h2>",
                "<h4></h4>");
            SearchPatternsAndCheckExtractions(patterns, text,
                ("HtmlHeaderTags.H", new[] { "h1" }),
                ("HtmlHeaderTags.Content", new[] { "One" }),

                ("HtmlHeaderTags.H", new[] { "h2" }),
                ("HtmlHeaderTags.Content", new[] { "Two<h3>Three</h3>" }),

                ("HtmlHeaderTags.H", new[] { "h4" })
            );
        }

        [TestMethod]
        public void HtmlHeadingTagExtraction()
        {
            string patterns = @"
                Heading = {'h1', 'h2', 'h3', 'h4', 'h5' ,'h6', 'h7'};
                #HtmlHeaderTags(H, AttrList, Content) = '<' + H:Heading + AttrList:[0+ {Any, ~'>'}] + '>' +
                    Content:[0+ {Any, ~('</' + H + '>')}] + '</' + H + '>';
            ";
            string text =
                @"<h1 class=""Hello""><h2>World!<h3 class=""test""></h3></h2></h1><h4 class=""Hello 2"">Content</h4>";
            // Самопересечения устранены:
            SearchPatternsAndCheckMatches(patterns, text,
                @"<h1 class=""Hello""><h2>World!<h3 class=""test""></h3></h2></h1>",
                @"<h4 class=""Hello 2"">Content</h4>");
            SearchPatternsAndCheckExtractions(patterns, text,
                ("HtmlHeaderTags.H", new[] { "h1" }),
                ("HtmlHeaderTags.AttrList", new[] { @" class=""Hello""" }),
                ("HtmlHeaderTags.Content", new[] { @"<h2>World!<h3 class=""test""></h3></h2>" }),

                ("HtmlHeaderTags.H", new[] { "h4" }),
                ("HtmlHeaderTags.AttrList", new[] { @" class=""Hello 2""" }),
                ("HtmlHeaderTags.Content", new[] { @"Content" })
            );
        }

        [Ignore]
        [TestMethod]
        public void HtmlHeadingTagContentExternalPatternFieldReference()
        {
            string patterns = @"
                HtmlHeaderTags(H, AttrList, Content) = '<' + H:{'h1', 'h2', 'h3', 'h4', 'h5' ,'h6', 'h7'} +
                    AttrList:[0+ {Any, ~'>'}] + '>' + Content:[0+ {Any, ~('</' + H + '>')}] + '</' + H + '>';
                #HtmlHeaderTagsContent = HtmlHeaderTags.Content;
            ";
            string text = @"<h1 class=""Hello""></h1><h2 class=""Hello"">Hello World!</h2>";
            SearchPatternsAndCheckMatches(patterns, text,
                @"<h1 class=""Hello""></h1>",
                @"<h2 class=""Hello"">Hello World!</h2>");
        }

        [TestMethod]
        public void HtmlATagHRefAttributeExtraction()
        {
            string patterns = @"
                #HtmlHref2(AttrListA, HRef, AttrListB, Content) = '<a' + AttrListA:[0+ {Any, ~'href'}] +
                    'href=""' + HRef:[0+ {Any, ~'""'}] + '""' + AttrListB:[0+ {Any, ~'>'}] + '>' +
                    Content:[0+ {Any, ~'</a>'}] + '</a>';
            ";
            string text = @"<a class=""Hello"" href=""./link.html"" hidden>Link Text</a>";
            SearchPatternsAndCheckExtractions(patterns, text,
                ("HtmlHref2.AttrListA", new[] { @" class=""Hello"" " }),
                ("HtmlHref2.HRef", new[] { @"./link.html" }),
                ("HtmlHref2.AttrListB", new[] { @" hidden" }),
                ("HtmlHref2.Content", new[] { @"Link Text" })
            );
        }

        [TestMethod]
        public void ExtractionInRepetition()
        {
            string patterns = "#P(x) = [1+ x:{Symbol, Punct}];";
            string text = "One, Two!? Three, &^)";
            SearchPatternsAndCheckExtractions(patterns, text,
                ("P.x", new[] { "," }),
                ("P.x", new[] { "!", "?" }),
                ("P.x", new[] { "," }),
                ("P.x", new[] { "&", "^", ")" })
            );
        }

        [TestMethod]
        public void ExtractionInVariationInRepetition()
        {
            string patterns = "#P(x) = [1+ {Symbol, x:Punct}];";
            string text = "One, Two!? Three, &^)";
            SearchPatternsAndCheckExtractions(patterns, text,
                ("P.x", new[] { "," }),
                ("P.x", new[] { "!", "?" }),
                ("P.x", new[] { "," }),
                ("P.x", new[] { ")" })
            );
        }

        [TestMethod]
        public void ExtractionInSequenceOptionalElement()
        {
            string patterns = "#P(x) = '*' + x:?'100' + '#';";
            string text = "*# *100# *200#";
            SearchPatternsAndCheckExtractions(patterns, text,
                ("P.x", new[] { "100" })
            );
        }

        [TestMethod]
        public void ExtractionReferenceInExceptionInAnySpan()
        {
            string patterns = "#P(~X) = '*' + X:Num ... {'#', ~'#' + X};";
            string text = "*100# *200#200# *300#";
            SearchPatternsAndCheckMatches(patterns, text,
                "*100#",
                "*200#200#",
                "*300#");
        }

        [TestMethod]
        public void ExtractionOfExtraction()
        {
            string patterns = "#P(x, y) = '*' + x:y:Num + '#';";
            string text = "*# *100#";
            SearchPatternsAndCheckExtractions(patterns, text,
                ("P.x", new[] { "100" }),
                ("P.y", new[] { "100" })
            );
        }

        [TestMethod]
        public void FromExpressionText()
        {
            string patterns = "'nezaboodka'";
            string text = "Nezaboodka Software LLC";
            string[] expectedMatches = new[] { "Nezaboodka" };

            var package = PatternPackage.FromExpressionText(patterns);
            var engine = new TextSearchEngine(package);
            SearchResult searchResult = engine.Search(text);
            if (searchResult.WasCandidateLimitExceeded)
                Assert.Fail("Candidate limit exceeded");
            List<string> actualMatches = searchResult.GetTagsSortedByLocationInText()
                .Select(x => x.ToString()).ToList();

            actualMatches.Should().BeEquivalentTo(expectedMatches);
        }

        [TestMethod]
        public void RepetitionOfVariationWithAnyButDot()
        {
            string patterns = "#TestPattern = [0+ {Any, ~'.'}];";
            string text = ".";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void RepetitionOfVariationWithAnyButDoubleDot()
        {
            string patterns = "#TestPattern = [0+ {Any, ~'..'}];";
            string text = "..";
            SearchPatternsAndCheckMatches(patterns, text,
                ".");
        }

        [TestMethod]
        public void RepetitionOfVariationWithAnyButDoubleDotLong()
        {
            string patterns = "#TestPattern = [0+ {Any, ~'..'}];";
            string text = "..test";
            SearchPatternsAndCheckMatches(patterns, text,
                ".test");
        }

        [TestMethod]
        public void RepetitionOfVariationWithAnyButDoubleDotInRepetition()
        {
            string patterns = "#TestPattern = [0+ {Any, ~[0+ '..']}];";
            string text = "..";
            SearchPatternsAndCheckMatches(patterns, text,
                ".");
        }

        [TestMethod]
        public void RepetitionCascadeOfVariationsWithAnyButDot()
        {
            string patterns = "#TestPattern = [0+ {Any, ~[1+ {Any, ~'.'} ] } ];";
            string text = "..";
            SearchPatternsAndCheckMatches(patterns, text,
                ".",
                ".");
        }

        [TestMethod]
        public void VariationWithExceptionInRepetitionSingleMatch()
        {
            string patterns = "#TestPattern = [1-2 {'*', '#', ~'#$'}];";
            string text = "*#$";
            SearchPatternsAndCheckMatches(patterns, text,
                "*");
        }

        [TestMethod]
        public void VariationWithExceptionInRepetitionTwoMatches()
        {
            string patterns = "#TestPattern = [1-3 {'*', '#', ~'*#$'}];";
            string text = "**#$";
            SearchPatternsAndCheckMatches(patterns, text,
                "*",    // first asterisk
                "#");
        }

        [TestMethod]
        public void SequenceOfOptionalRepetitionAndOptionalElements()
        {
            string patterns = "#P = '{' + [0+ {LineBreak, Space}] + ?'Hello' + ?Space + '}';";
            string text = "{   \nHello   }";
            SearchPatternsAndCheckMatches(patterns, text,
                "{   \nHello   }");
        }

        [TestMethod]
        public void SequenceOfOptionalRepetitionAndDifferentOptionalElements()
        {
            string patterns = "#P = '{' + [0+ {LineBreak, Space}] + ?'Hello' + ?Punct + '}';";
            string text = "{   \nHello.}";
            SearchPatternsAndCheckMatches(patterns, text,
                "{   \nHello.}");
        }

        [TestMethod]
        public void CandidateLimitExceededSimple()
        {
            string patterns = "#W = [2 Any];";
            string text = "One Two Three Four Five";
            var options = new SearchOptions()
            {
                CandidateLimit = 0,
                SuppressCandidateLimitExceeded = true
            };
            AssertNoMatches(patterns, text, options);
        }

        [TestMethod]
        public void CandidateLimitExceededRepetitionPositive()
        {
            string patterns = "#Test = Start + [1+ {Word, Space, Punct}];";
            string text = "You will do it You will do it You will do it.";
            var options = new SearchOptions()
            {
                CandidateLimit = 1,
                SuppressCandidateLimitExceeded = true
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "You");
        }

        [TestMethod]
        public void CandidateLimitExceededConjunctionWithOptionalElementsPositive()
        {
            string patterns = "#T1 = 'You' & 'will' & ?'do';";
            string text = "You definitely will do it.";
            var options = new SearchOptions()
            {
                CandidateLimit = 1,
                SuppressCandidateLimitExceeded = true
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "You definitely will");
        }

        [TestMethod]
        public void CandidateLimitExceededWaitingCandidate()
        {
            string patterns = @"
                #T1 = Start ... 'will';
                #T2 = 'you' ... End;
            ";
            string text = "You will do it.";
            var options = new SearchOptions()
            {
                CandidateLimit = 1,
                SuppressCandidateLimitExceeded = true
            };
            AssertNoMatches(patterns, text, options);
        }

        [TestMethod]
        public void CandidateLimitExceededAnySpan()
        {
            string patterns = "#T1 = Start ... (Word + Space);";
            string text = "You will do it.";
            var options = new SearchOptions()
            {
                CandidateLimit = 1,
                SuppressCandidateLimitExceeded = true
            };
            AssertNoMatches(patterns, text, options);
        }

        [TestMethod]
        public void CandidateLimitExceededAnySpanPositive()
        {
            string patterns = "#W = {Word, ~Word+Space} ... End;";
            string text = "One Two Three Four Five";
            var options = new SearchOptions()
            {
                CandidateLimit = 2,
                SuppressCandidateLimitExceeded = true
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "Five");
        }

        [TestMethod]
        public void CandidateLimitExceededWordSpanPositive()
        {
            string patterns = "#W = 'A' ..[2-3].. 'B';";
            string text = "A One Two Three Four B A One Two Three Four B A One Two Three B";
            var options = new SearchOptions()
            {
                PatternCandidateLimit = 2
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "A One Two Three B");
        }

        [TestMethod]
        public void CandidateLimitExceededVariation()
        {
            string patterns = "#Test = Start + {Word + Space, Word + Punct, Word + LineBreak, Word + Symbol};";
            string text = "You will.";
            var options = new SearchOptions()
            {
                CandidateLimit = 2,
                SuppressCandidateLimitExceeded = true
            };
            AssertNoMatches(patterns, text, options);
        }

        [TestMethod]
        public void CandidateLimitExceededOutside()
        {
            string patterns = "#Test = ('will' @outside 'will do it') + WordBreak + {'do', Word};";
            string text = "You will do it.";
            var options = new SearchOptions()
            {
                CandidateLimit = 1,
                SuppressCandidateLimitExceeded = true
            };
            AssertNoMatches(patterns, text, options);
        }

        [TestMethod]
        public void CandidateLimitExceededSequenceEndingWithOptionalElementsPositive()
        {
            string patterns = "#Pattern = 'Nezaboodka' + Space + 'Software' + ?(Space + 'LLC');";
            string blogComText = "Welcome to Nezaboodka Software LLC!";
            var options = new SearchOptions()
            {
                CandidateLimit = 1,
                SuppressCandidateLimitExceeded = true
            };
            SearchPatternsAndCheckMatches(patterns, blogComText, options,
                "Nezaboodka Software");
        }

        [TestMethod]
        public void PatternCandidateLimitExceededVariation()
        {
            string patterns = "#Test = Start + {Word + Space, Word + Punct, Word + LineBreak, Word + Symbol};";
            string text = "You will.";
            var options = new SearchOptions()
            {
                PatternCandidateLimit = 2,
                SuppressCandidateLimitExceeded = true
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "You ");
        }

        [TestMethod]
        public void PatternCandidateLimitExceededAnySpan()
        {
            string patterns = @"
                #P1 = (Any @outside LineBreak) ... End;
                #P2 = [1+ Word+Space];
                #P3 = Word+Space @outside '@'...'Four';
            ";
            string text = "One Two @ Three Four Five";
            var options = new SearchOptions()
            {
                PatternCandidateLimit = 5,
                SuppressCandidateLimitExceeded = true
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                ("P1", new[] { "One Two @ Three Four Five" }),
                ("P2", new[] { "One Two ", "Three Four " }),
                ("P3", new[] { "One ", "Two " })
            );
        }

        [TestMethod]
        public void FirstMatchOnlySentence()
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
                FirstMatchOnly = true
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n");
        }

        [TestMethod]
        public void FirstMatchOnlySequence()
        {
            string patterns = "#T = {'ANDROID OR IPHONE THE BETTER SMARTPHONE', 'IPHONE THE BETTER'};";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE";
            var options = new SearchOptions()
            {
                FirstMatchOnly = true
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "ANDROID OR IPHONE THE BETTER SMARTPHONE");
        }

        [TestMethod]
        public void FirstMatchOnlySequenceWithLaterMatchOfShorter()
        {
            string patterns = "#T = 'android or' + Space + {{'iphone', ~'iphone the worst'}, 'iphone' + Space};";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE";
            var options = new SearchOptions()
            {
                FirstMatchOnly = true
            };
            SearchPatternsAndCheckMatches(patterns, text,
                "ANDROID OR IPHONE ");
        }

        [TestMethod]
        public void FirstMatchOnlyAnySpan()
        {
            string patterns = "#T = {'android'...'smartphone', 'iphone'...'better'};";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE";
            var options = new SearchOptions()
            {
                FirstMatchOnly = true
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "ANDROID OR IPHONE THE BETTER SMARTPHONE");
        }

        [TestMethod]
        public void GarbageCollectionAtSpecificTokenToFindCleaningTokenNumberOfReorderedCandidates()
        {
            string patterns = "#P = '@A#B' ... '#';";
            string text = "@A#B@A#B";
            var options = new SearchOptions()
            {
                TokenCountToWaitToPerformGarbageCollection = 7  // garbage collection at second 'A' token
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "@A#B@A#");
        }

        [TestMethod]
        public void GarbageCollectionSentence()
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
                TokenCountToWaitToPerformGarbageCollection = 5,
                MaxCountOfMatchedTagsWaitingForCleanup = 5,
                NewWaitingTokenCountToPerformGarbageCollection = 2
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n",
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.");
        }

        [TestMethod]
        public void GarbageCollectionSentenceWordSpan()
        {
            string patterns = @"
                SentenceSeparator = {End, '.', '!', '?', [2 LineBreak], ~'.'+Word};
                #Sentence = Word ..[3-50].. SentenceSeparator;
            ";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            var options = new SearchOptions()
            {
                TokenCountToWaitToPerformGarbageCollection = 5,
                MaxCountOfMatchedTagsWaitingForCleanup = 5,
                NewWaitingTokenCountToPerformGarbageCollection = 2
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n",
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.");
        }

        [TestMethod]
        public void GarbageCollectionSequenceWithInsideSimple()
        {
            string patterns = "#P = ('is' @inside 'android or iphone') + {Space, Space + Word};";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE";
            var options = new SearchOptions()
            {
                TokenCountToWaitToPerformGarbageCollection = 10 // garbage collection at token 'THE'
            };
            AssertNoMatches(patterns, text, options);
        }

        [TestMethod]
        public void GarbageCollectionSentenceWithInside()
        {
            string patterns = @"
                SentenceSeparator = {End, '.', '!', '?', [2 LineBreak], ~ '.' @inside '.'+Word};
                #Sentence = Word ... SentenceSeparator;
            ";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            var options = new SearchOptions()
            {
                TokenCountToWaitToPerformGarbageCollection = 5,
                MaxCountOfMatchedTagsWaitingForCleanup = 5
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n",
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.");
        }

        [TestMethod]
        public void GarbageCollectionWithActiveCandidateAfterWaiting()
        {
            string patterns = "#P = ('@' + 'B' @outside '...') ... End;";
            string text = "A @B@B C";
            var options = new SearchOptions()
            {
                TokenCountToWaitToPerformGarbageCollection = 6,
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "@B@B C");
        }

        [TestMethod]
        public void AnySpanWithInsideInExceptionSimplest()
        {
            string patterns = "#Sentence = Start ... {'.', ~ '.' @inside '.'+Word};";
            string text = "When (www.google.com) Android.";
            SearchPatternsAndCheckMatches(patterns, text,
                "When (www.google.com) Android.");
        }

        [TestMethod]
        public void SentenceWithInsideSimple()
        {
            string patterns = "#Sentence = Word ... {'.', ~ '.' @inside '.'+Word};";
            string text = "When (www.google.com) Android.";
            SearchPatternsAndCheckMatches(patterns, text,
                "When (www.google.com) Android.");
        }

        [TestMethod]
        public void GarbageCollectionSentenceWithInsideSimple()
        {
            string patterns = "#Sentence = Word ... {'.', ~ '.' @inside '.'+Word};";
            string text = "When (www.google.com) Android.";
            var options = new SearchOptions()
            {
                MaxCountOfMatchedTagsWaitingForCleanup = 1
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "When (www.google.com) Android.");
        }

        [TestMethod]
        public void GarbageCollectionSentenceWithOutside()
        {
            string patterns = @"
                SentenceSeparator = {End, ('.' @outside '.'+Word), '!', '?', [2 LineBreak]};
                #Sentence = Word ... SentenceSeparator;
            ";
            string text = "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n" +
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.";
            var options = new SearchOptions()
            {
                TokenCountToWaitToPerformGarbageCollection = 5,
                MaxCountOfMatchedTagsWaitingForCleanup = 5
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "IS ANDROID OR IPHONE THE BETTER SMARTPHONE\n\n",
                "When it comes to buying one of the best smartphones,\n" +
                "the first choice can be the hardest (www.google.com): iPhone or Android.");
        }

        [TestMethod]
        public void WordOutsideOfPhraseWithThatWordReference()
        {
            string patterns = @"
                Const = 'nervous';
                #Original = 'nervous' @outside 'excited' .. [0-20] .. Const;
            ";
            string text = "I am excited and a little nervous.";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void WordOutsideOfPhraseWithThatWordReferenceInlined()
        {
            string patterns = "#Original = 'nervous' @outside 'excited' .. [0-20] .. 'nervous';";
            string text = "I am excited and a little nervous.";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void PackageWithNoTargetPatterns()
        {
            string patterns = "Test = 'nervous';";
            string text = "I am excited and a little nervous.";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void ExtractionOfSpan()
        {
            string patterns = "#P(X) = '*' .. X .. '#';";
            string text = "*# *100# *200#";
            SearchPatternsAndCheckExtractions(patterns, text,
                ("P.X", new[] { "100" }),
                ("P.X", new[] { "200" })
            );
        }

        [TestMethod]
        public void ExtractionOfWordSpan()
        {
            string patterns = "#P(X) = '*' .. X:[0-5] .. '#';";
            string text = "*# *100# *200 times#";
            SearchPatternsAndCheckExtractions(patterns, text,
                ("P.X", new[] { "100" }),
                ("P.X", new[] { "200 times" })
            );
        }

        [TestMethod]
        public void FieldReferenceMultiple()
        {
            string patterns = "#P1(X) = X: Symbol + {X + '^', X + '%'};";
            string text = "@@^**-$$%";
            SearchPatternsAndCheckMatches(patterns, text,
                "@@^",
                "$$%");
            SearchPatternsAndCheckExtractions(patterns, text,
                ("P1.X", new[] { "@" }),
                ("P1.X", new[] { "$" })
            );
        }

        [TestMethod]
        public void ExtractionAndFieldReferenceInRepetitionSimple()
        {
            string patterns = "#P1(X) = [2 X:Symbol + '-' + X];";
            string text = "@-@*-*";
            SearchPatternsAndCheckMatches(patterns, text,
                "@-@*-*");
            SearchPatternsAndCheckExtractions(patterns, text,
                ("P1.X", new[] { "@", "*" })
            );
        }

        [TestMethod]
        public void FieldReferenceInExceptionSimple()
        {
            string patterns = "#P1(~X) = X: Symbol + {Symbol, ~X};";
            string text = "@@**";
            SearchPatternsAndCheckMatches(patterns, text,
                "@*");
            AssertNoExtractions(patterns, text);
        }

        [TestMethod]
        public void ExtractionAndFieldReferenceInException()
        {
            string patterns = "#P1(X) = {Symbol, ~X:Symbol + [2 X]};";
            string text = "@@**";
            SearchPatternsAndCheckMatches(patterns, text,
                "@",
                "@",
                "*",
                "*");
            AssertNoExtractions(patterns, text);
        }

        [TestMethod]
        public void WordBreakAtInside()
        {
            string patterns = @"
                #P = ('val2'  + [0+ WordBreak] +  'val1') @inside Context;
                Context = 'val1 val2';
            ";
            string text = "val1 val2 val1 val2";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void RepetitionOfSequenceWithFirstOptionalElement()
        {
            string patterns = "#Pattern = Start + [1+ ?'.'+'*'];";
            string text = "*.**.*";
            SearchPatternsAndCheckMatches(patterns, text,
                "*.**.*");
        }

        [TestMethod]
        public void VariationWithExceptionInSimpleVariationInSequence()
        {
            string patterns = "#DeliveryCustomer = 'Delivery' + Space + {{'Driver', ~ 'Drivers'}, 'customer'};";
            string text = "delivery customer";
            SearchPatternsAndCheckMatches(patterns, text,
                "delivery customer");
        }

        [TestMethod]
        public void RestartSearchEngine()
        {
            string patterns = "#PhoneNumber = ?'+' + {Num, '(' + Num + ')'} + [2+ ({'-', Space} + Num)];";
            string text = "6212 1234 5678 9012";
            string[] expectedMatches = new[] { "6212 1234 5678 9012" };
            const int repeatCount = 5;

            PatternPackage patternPackage = PatternPackage.FromText(patterns);
            TextSearchEngine engine = new TextSearchEngine(patternPackage);
            for (int i = 0; i < repeatCount; i++)
            {
                SearchResult result = engine.Search(text);
                var actualMatches = result.GetTagsSortedByLocationInText().Select(x => x.GetText()).ToArray();
                actualMatches.Should().BeEquivalentTo(expectedMatches);
            }
        }

        [TestMethod]
        public void VariationOfReferencesToPatternsWithExtractionsOptionalSpaceBeforeWord()
        {
            string patterns = @"
                #TestPattern = {A, B};
                A(X1) = X1:'$';
                #B(X2) = [2 ?Space + Word] + X2:Punct;
            ";
            string text = "One Two Three.";
            SearchPatternsAndCheckMatches(patterns, text,
                ("TestPattern", new[] { " Two Three." }),
                ("B", new[] { " Two Three." }));
        }

        [TestMethod]
        public void VariationOfReferencesToPatternsWithExtractionsOptionalSpaceAfterWord()
        {
            string patterns = @"
                #TestPattern = {A, B};
                A(X1) = X1:'$';
                #B(X2) = [2 Word + ?Space] + X2:Punct;
            ";
            string text = "One Two Three.";
            SearchPatternsAndCheckMatches(patterns, text,
                ("TestPattern", new[] { "Two Three." }),
                ("B", new[] { "Two Three." }));
        }

        [TestMethod]
        public void SentenceHavingGoogle()
        {
            string patterns = @"
                #SentenceWithGoogle = Sentence @having 'google';
                Sentence = Word ... SentenceSeparator;
                SentenceSeparator = {End, '.', '!', '?', [2 LineBreak], ~'.'+Word};
            ";
            string text = "First without. Second with Google. Third.";
            SearchPatternsAndCheckMatches(patterns, text,
                "Second with Google.");
        }

        [TestMethod]
        public void SentenceHavingGoogleSimpler()
        {
            string patterns = "#SentenceWithGoogle = 'X' ... {'.', ~'.'+Word} @having 'google';";
            string text = "X First without. X Second with Google. X Third.";
            SearchPatternsAndCheckMatches(patterns, text,
                "X Second with Google.");
        }

        [TestMethod]
        public void DriveOutsideSentenceHavingGoogle()
        {
            string patterns = @"
                #DriveSentence = Sentence @having ('drive' @outside SentenceWithGoogle);
                SentenceWithGoogle = Sentence @having 'google';
                Sentence = Word ... SentenceSeparator;
                SentenceSeparator = {End, '.', '!', '?', [2 LineBreak], ~'.'+Word};
            ";
            string text = "First drive without. Second with Google Drive. Third Drive and Google.";
            SearchPatternsAndCheckMatches(patterns, text,
                "First drive without.");
        }

        [TestMethod]
        public void DriveOutsideTextHavingGooglePositive()
        {
            string patterns = @"
                #DriveSentence = (Start ... End) @having ('drive' @outside (Start ... End @having 'google'));
            ";
            string text = "Drive in text without";
            SearchPatternsAndCheckMatches(patterns, text,
                "Drive in text without");
        }

        [TestMethod]
        public void DriveOutsideTextHavingGoogle_NoMatch()
        {
            string patterns = @"
                #DriveSentence = (Start ... End) @having ('drive' @outside (Start ... End @having 'google'));
            ";
            string text = "Drive in text with Google.";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void HavingAtRightPositionOfAnySpan()
        {
            string patterns = @"
                #P = Start ... (Sentence @having 'google');
                Sentence = Word ... '.';
            ";
            string text = "First without. Second without. Third with Google. Forth without.";
            SearchPatternsAndCheckMatches(patterns, text,
                "First without. Second without. Third with Google.");
        }

        [TestMethod]
        public void HavingAtRightPositionOfAnySpanSimple()
        {
            string patterns = "#P = Start ... (('google' ... '.') @having 'drive');";
            string text = "First Google Spreadsheet. Second Google Drive. Third Google Mail.";
            SearchPatternsAndCheckMatches(patterns, text,
                "First Google Spreadsheet. Second Google Drive.");
        }

        [TestMethod]
        public void RightPositionOfHavingAtRightPositionOfAnySpanSimple()
        {
            string patterns = "#P = ('google' ... '.') @having 'drive';";
            string text = "First Google Spreadsheet. Second Google Drive. Third Google Mail.";
            SearchPatternsAndCheckMatches(patterns, text,
                "Google Drive.");
        }

        [TestMethod]
        public void OutsideAtRightPositionOfAnySpanPositive()
        {
            string patterns = "#P = Start ... ('google' @outside 'google cloud');";
            string text = "First with Google Cloud. Second without Google Drive.";
            SearchPatternsAndCheckMatches(patterns, text,
                "First with Google Cloud. Second without Google");
        }

        [TestMethod]
        public void OutsideAtRightPositionOfAnySpan_NoMatch()
        {
            string patterns = "#P = Start ... ('google' @outside 'google cloud');";
            string text = "First with Google Drive. Second without Google Cloud.";
            SearchPatternsAndCheckMatches(patterns, text,
                "First with Google");
        }

        [TestMethod]
        public void NestedHavingInnerContent()
        {
            string patterns = "#T = (Start ... End) @having (('There' ... '!') @having 'you');";
            string text = "There you are!";
            SearchPatternsAndCheckMatches(patterns, text,
                text);
        }

        [TestMethod]
        public void NestedHavingFromOneTokenInnerContent()
        {
            string patterns = "#T = ('There' ... End) @having (('There' ... '!') @having 'you');";
            string text = "There you are!";
            SearchPatternsAndCheckMatches(patterns, text,
                text);
        }

        [TestMethod]
        public void NestedHaving()
        {
            string patterns =
                "#T = Start + (('There' + ([2 WordBreak + Word] @having 'you') + '!') @having 'you are') ... End ;";
            string text = "There you are!";
            SearchPatternsAndCheckMatches(patterns, text,
                text);
        }

        [TestMethod]
        public void SentenceHavingThreeCompanies()
        {
            string patterns = @"
                #SentenceWithCompanies = Sentence @having ('microsoft' & 'google' & 'amazon');
                Sentence = Word ... SentenceSeparator;
                SentenceSeparator = {End, '.', '!', '?', [2 LineBreak], ~'.'+Word};
            ";
            string text = @"
                First with Microsoft only. Second with Google only. Third with Amazon only.
                Last with all: Microsoft, Google, Amazon.
            ";
            SearchPatternsAndCheckMatches(patterns, text,
                "Last with all: Microsoft, Google, Amazon.");
        }

        [TestMethod]
        public void SentenceHavingTargetPattern()
        {
            string patterns = @"
                #SentenceWithCompanies = Sentence @having ThreeCompanies;
                Sentence = Word ... SentenceSeparator;
                SentenceSeparator = {End, '.', '!', '?', [2 LineBreak], ~'.'+Word};
                #ThreeCompanies = 'microsoft' & 'google' & 'amazon';
            ";
            string text = @"
                First with Microsoft only. Second with Google only. Third with Amazon only.
                Last with all: Microsoft, Google, Amazon.
            ";
            SearchPatternsAndCheckMatches(patterns, text,
                ("ThreeCompanies",
                    new[] {
                        "Microsoft only. Second with Google only. Third with Amazon",
                        "Microsoft, Google, Amazon"
                    }),
                ("SentenceWithCompanies", new[] { "Last with all: Microsoft, Google, Amazon." })
            );
        }

        [TestMethod]
        public void HavingInSequenceWithRepetitionPositive()
        {
            string patterns = "#P = ((Start ... ',') @having 'word') + [1+ WordBreak + Word];";
            string text = "First word, second word third.";
            SearchPatternsAndCheckMatches(patterns, text,
                "First word, second word third");
        }

        [TestMethod]
        public void HavingInSequenceWithRepetition_NoMatch()
        {
            string patterns = "#P = ((Start ... ',') @having 'word') + [1+ WordBreak + Word];";
            string text = "First, second word third.";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void SelfOverlappingTags()
        {
            string patterns = "#T = [1-2 Word + ? WordBreak];";
            string text = "First Second Third.";
            var options = new SearchOptions()
            {
                SelfOverlappingTagsInResults = true
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "First",
                "First ",
                "First Second",
                "First Second ",
                "Second",
                "Second ",
                "Second Third",
                "Second Third.",
                "Third",
                "Third.");
        }

        [DataTestMethod]
        [DataRow("Project Manager Assistant (Senior Project Coordinator) / Project Manager")]
        [DataRow("Internet Manager / Internet Sales Director /Internet Director")]
        public void VariationWithAnySpanOutsideAnySpanFromStartToEnd(string text)
        {
            string patterns =
                "#Target = { 'nurse' } @outside { Start ... { 'manager' @outside { 'services manager' }, 'coordinator' } ... End };";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void VariationOfJustException()
        {
            string patterns = "#P = {~Symbol};";
            string text = @"hello\";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void VariationOfJustExceptionShouldNotMatch()
        {
            string patterns = "#P = {~Word};";
            string text = @"hello";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void SingleWhitespaceInConstantStringShouldMatchMultipleWhitespacesInText()
        {
            string patterns = "#Target = 'word word';";
            string text = "word    word";
            SearchPatternsAndCheckMatches(patterns, text,
                expectedMatches: text);
        }

        [TestMethod]
        public void MultipleWhitespacesInConstantStringShouldMatchSingleWhitespaceInText()
        {
            string patterns = "#Target = 'word   word';";
            string text = "word word";
            SearchPatternsAndCheckMatches(patterns, text,
                expectedMatches: text);
        }

        [TestMethod]
        [DataRow("word\nword")]
        [DataRow("word\rword")]
        [DataRow("word\r\nword")]
        public void LineBreakInConstantStringShouldAnyLineBreakTokenInText(string text)
        {
            string patterns = "#Target = 'word\nword';";
            SearchPatternsAndCheckMatches(patterns, text,
                expectedMatches: text);
        }

        [TestMethod]
        public void NestedOutsideFromStartToEnd()
        {
            string patterns =
                "#Target = 'nurse' @outside (Start ... ('manager' @outside 'services manager') ... End);";
            string text = "Assistant Nurse Manager, Surgery Center - NW 50th";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void OutsideFromStartToEnd()
        {
            string patterns =
                "#Target = Start ... ('manager' @outside 'services manager') ... End;";
            string text = "Assistant Nurse Manager, Surgery Center - NW 50th";
            SearchPatternsAndCheckMatches(patterns, text,
                text);
        }

        [TestMethod]
        public void SimpleOutsideFromStartToEndPositive()
        {
            string patterns =
                "#Target = Start ... ('test' @outside 'local test') ... End;";
            string text = "test";
            SearchPatternsAndCheckMatches(patterns, text,
                text);
        }

        [TestMethod]
        public void SimpleOutsideWithinAnySpansPositive()
        {
            string patterns =
                "#Target = 'first' ... ('test' @outside 'local test') ... 'last';";
            string text = "The first test is not last one.";
            SearchPatternsAndCheckMatches(patterns, text,
                "first test is not last");
        }

        [TestMethod]
        public void SimpleOutsideWithinAnySpans_NoMatch()
        {
            string patterns =
                "#Target = 'first' ... ('test' @outside 'local test') ... 'last';";
            string text = "The first local test is not last one.";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void AnySpanWithOutsideInRightPosition()
        {
            string patterns =
                "#Target = Start ... ('test' @outside 'local test');";
            string text = "test";
            SearchPatternsAndCheckMatches(patterns, text,
                text);
        }

        [TestMethod]
        public void AnySpanWithOutsideInLeftPosition()
        {
            string patterns =
                "#Target = ('test' @outside 'local test') ... End;";
            string text = "test";
            SearchPatternsAndCheckMatches(patterns, text,
                text);
        }

        [TestMethod]
        public void AnySpanInsideSentence()
        {
            string patterns = @"
                #Test = ('test' ... 'span') @inside Sentence;
                Sentence = (Word ... '.');
            ";
            string text = "First sentence with test word. Second sentence with test word span.";
            var options = new SearchOptions()
            {
                FirstMatchOnly = true,
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "test word span");
        }

        [TestMethod]
        public void SentenceHavingAnySpan()
        {
            string patterns = @"
                X = ('test' ... 'span') @inside Sentence;
                Sentence = (Word ... '.');
                #Test = Sentence @having X;
            ";
            string text = "First sentence with test word. Second sentence with test word span.";
            var options = new SearchOptions()
            {
                FirstMatchOnly = true,
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                "Second sentence with test word span.");
        }

        [TestMethod]
        public void SimpleOutsideFromStartToEnd_NoMatch()
        {
            string patterns =
                "#Target = Start ... ('test' @outside 'local test') ... End;";
            string text = "local test";
            AssertNoMatches(patterns, text);
        }

        [TestMethod]
        public void SimpleOutside()
        {
            string patterns =
                "#Target = 'manager' @outside 'services manager';";
            string text = "Assistant Nurse Manager, Surgery Center - NW 50th";
            SearchPatternsAndCheckMatches(patterns, text,
                "Manager");
        }

        [TestMethod]
        public void ConditionalHavingMatchAndAnySpanMatch()
        {
            string patterns = @"
                #Another = 'word' @having 'word';
                #Line = Word ... End;
            ";
            string text = "Word word.";
            SearchPatternsAndCheckMatches(patterns, text,
                ("Another", new[] { "Word", "word" }),
                ("Line", new[] { "Word word." }));
        }

        [TestMethod]
        public void ConditionalHavingNoMatchAndAnySpanMatch()
        {
            string patterns = @"
                #Another = 'another' @having 'another';
                #Line = Word ... End;
            ";
            string text = "Word word.";
            SearchPatternsAndCheckMatches(patterns, text,
                ("Line", new[] { "Word word." }));
        }

        [TestMethod]
        public void ConditionalHavingNoMatchAndSimplePatternMatch()
        {
            string patterns = @"
                #Another = 'another' @having 'another';
                #Test = '.';
            ";
            string text = "Word word.";
            SearchPatternsAndCheckMatches(patterns, text,
                ("Test", new[] { "." }));
        }

        [TestMethod]
        public void ConditionalHavingAndAnySpanWithFirstMatchOnly()
        {
            string patterns = @"
                Test = 'test';
                #SentenceWithTest = (Word ... '.') @having Test;
            ";
            string text = "First sentence. First sentence with first test word. Second sentence with test word.";
            var options = new SearchOptions()
            {
                FirstMatchOnly = true,
            };
            SearchPatternsAndCheckMatches(patterns, text, options,
                ("SentenceWithTest", new[] { "First sentence with first test word." }));
        }

        [TestMethod]
        public void ConditionalHavingCandidateLimitExceeded()
        {
            string patterns = @"
                #Test = Sentence @having FirstLastWithDay;
                Sentence = Word ... '.';
                FirstLastWithDay = FirstLast @having 'day';
                FirstLast = 'first' ... 'last';
            ";
            var options = new SearchOptions()
            {
                CandidateLimit = 1,
                SuppressCandidateLimitExceeded = true,
            };
            string text = "First day in last sentence.";
            AssertNoMatches(patterns, text, options);
        }

        [TestMethod]
        public void RestartSearchEngineWithConditionalHaving()
        {
            string patterns = @"
                #MyWord = 'word' @having 'another';
                #Test = '.';
            ";
            string text = "Word word.";
            string[] expectedMatches = new[] { "." };
            const int repeatCount = 5;

            PatternPackage patternPackage = PatternPackage.FromText(patterns);
            TextSearchEngine engine = new TextSearchEngine(patternPackage);
            for (int i = 0; i < repeatCount; i++)
            {
                SearchResult result = engine.Search(text);
                var actualMatches = result.GetTagsSortedByLocationInText().Select(x => x.GetText()).ToArray();
                actualMatches.Should().BeEquivalentTo(expectedMatches);
            }
        }

        [TestMethod]
        public void OverlappingIssueWithOutside()
        {
            string patterns = @"
                #P = {'oh hello', 'hello' @outside 'hello world'};
            ";
            string text = "oh hello, oh hello, oh hello, oh hello";
            string[] expectedMatches = new[] { "oh hello", "oh hello", "oh hello", "oh hello" };

            PatternPackage patternPackage = PatternPackage.FromText(patterns);
            TextSearchEngine engine = new TextSearchEngine(patternPackage, new SearchOptions
            {
                MaxCountOfMatchedTagsWaitingForCleanup = 1,
            });

            SearchResult result = engine.Search(text);
            var actualMatches = result.GetTagsSortedByLocationInText().Select(x => x.GetText()).ToArray();
            actualMatches.Should().BeEquivalentTo(expectedMatches);
        }

        [TestMethod]
        public void ReferenceSimple()
        {
            string patterns = @"
                @search @pattern Number = SomeNumber;
                @pattern SomeNumber = Num;";
            string text = "10 20";
            SearchPatternsAndCheckMatches(patterns, text,
                "10", "20");
        }

        [TestMethod]
        public void ReferenceWithinWhereBlock()
        {
            string patterns = @"
                @search @pattern Number = {Integer, Float}
                @where
                {
                    Integer = ?'-' + Num @outside Float;
                    Float = ?'-' + Num + {'.', ','} + {Num, NumAlpha + ?(?{'-', '+'} + Num)};
                };";
            string text = "10 20 30.5";
            SearchPatternsAndCheckMatches(patterns, text,
                "10", "20", "30.5");
        }

        [TestMethod]
        public void ReferenceWithinDeepWhereBlock()
        {
            string patterns = @"
                @search @pattern Number = {Integer, Float}
                @where
                {
                    Integer = ?'-' + Num @outside Float;
                    Float = ?'-' + Num + Delim + {Num, NumAlpha + ?(?{'-', '+'} + Num)}
                    @where
                    {
                        Delim = {Dot, Comma};
                        Dot = '.';
                        Comma = ',';
                    };
                };";
            string text = "10 20 30.5";
            SearchPatternsAndCheckMatches(patterns, text,
                "10", "20", "30.5");
        }

        [TestMethod]
        public void ReferenceWithinWhereBlockInNamespace()
        {
            string patterns = @"
                @namespace Common
                {
                    @search @pattern Number = {Integer, Float}
                    @where
                    {
                        Integer = ?'-' + Num @outside Float;
                        Float = ?'-' + Num + {'.', ','} + {Num, NumAlpha + ?(?{'-', '+'} + Num)};
                    };
                }";
            string text = "10 20 30.5";
            SearchPatternsAndCheckMatches(patterns, text,
                "10", "20", "30.5");
        }

        [TestMethod]
        public void ReferenceWithinDeepWhereBlockInNamespace()
        {
            string patterns = @"
                @namespace Common
                {
                    @search @pattern Number = {Integer, Float}
                    @where
                    {
                        Integer = ?'-' + Num @outside Float;
                        Float = ?'-' + Num + Delim + {Num, NumAlpha + ?(?{'-', '+'} + Num)}
                        @where
                        {
                            Delim = {Dot, Comma};
                            Dot = '.';
                            Comma = ',';
                        };
                    };
                }";
            string text = "10 20 30.5";
            SearchPatternsAndCheckMatches(patterns, text,
                "10", "20", "30.5");
        }
    }
}
