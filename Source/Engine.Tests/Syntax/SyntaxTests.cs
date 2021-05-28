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
    [TestClass]
    [TestCategory("Syntax"), TestCategory("SyntaxStringBuilder")]
    public class SyntaxTests
    {
        [TestMethod]
        public void StandardPatternReferenceToString()
        {
            string pattern = "Symbol = Symbol;";
            PatternSyntax syntax = Syntax.StandardPattern.SymbolPattern;
            string text = syntax.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void StandardPattern()
        {
            string pattern = "AnyToken = Any;";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "AnyToken",
                Syntax.StandardPattern.Any));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void SimplestPattern()
        {
            string pattern = "TheWord = Word;";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "TheWord",
                Syntax.StandardPattern.Word));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void TargetPattern()
        {
            string pattern = "#TheWord = Word;";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: true, "TheWord",
                Syntax.StandardPattern.Word));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void SequenceSyntax()
        {
            string pattern = "Name = Word + Space + Word;";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "Name",
                Syntax.Sequence(
                    Syntax.StandardPattern.Word,
                    Syntax.StandardPattern.Space,
                    Syntax.StandardPattern.Word)));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void SequenceSyntaxPriority()
        {
            string pattern = "Name = Word + Space + Word;"; // Name = Alpha + (Space + Alpha);
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "Name",
                Syntax.Sequence(
                    Syntax.StandardPattern.Word,
                    Syntax.Sequence(
                        Syntax.StandardPattern.Space,
                        Syntax.StandardPattern.Word))));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void WordsAtZeroDistanceSyntax()
        {
            string pattern = "Name = Word ... Symbol ... Word;";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "Name",
                Syntax.AnySpan(
                    Syntax.AnySpan(
                        Syntax.StandardPattern.Word,
                        Syntax.StandardPattern.Symbol),
                    Syntax.StandardPattern.Word)));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void WordsAtZeroDistanceSyntaxPriority()
        {
            string pattern = "Name = Word ... (Symbol .. [0+] .. Word);";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "Name",
                Syntax.AnySpan(
                    Syntax.StandardPattern.Word,
                        Syntax.WordSpan(
                            Syntax.StandardPattern.Symbol,
                            Syntax.StandardPattern.Word))));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void WordsAtVariableDistanceSyntax()
        {
            string pattern = "Name = Word .. [0-5] .. Symbol .. [1+] .. Word;";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "Name",
                Syntax.WordSpan(
                    Syntax.WordSpan(
                        Syntax.StandardPattern.Word, 0, 5,
                        Syntax.StandardPattern.Symbol), 1, int.MaxValue,
                    Syntax.StandardPattern.Word)));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void ConjunctionSyntax()
        {
            string pattern = "YouAndMeAndThey = You & Me & They;";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "YouAndMeAndThey",
                Syntax.Conjunction(
                    Syntax.PatternReference("You"),
                    Syntax.PatternReference("Me"),
                    Syntax.PatternReference("They"))));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void ConjunctionSyntaxPriority()
        {
            string pattern = "YouAndMeAndThey = You & Me & They;"; // You & (Me & They);
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "YouAndMeAndThey",
                Syntax.Conjunction(
                    Syntax.PatternReference("You"),
                        Syntax.Conjunction(
                            Syntax.PatternReference("Me"),
                            Syntax.PatternReference("They")))));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void VariationSyntax()
        {
            string pattern = "Cipher = {Word, Punct, ~Symbol};";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "Cipher",
                Syntax.Variation(
                    Syntax.StandardPattern.Word,
                    Syntax.StandardPattern.Punct,
                    Syntax.Exception(Syntax.StandardPattern.Symbol))));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void EndlessFromZeroRepetitionSyntax()
        {
            string pattern = "TheBlanks = [0+ {Space, LineBreak}];";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "TheBlanks",
                Syntax.Span(
                    Syntax.Repetition(0, int.MaxValue,
                        Syntax.Variation(
                            Syntax.StandardPattern.Space,
                            Syntax.StandardPattern.LineBreak)))));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void OptionalRepetitionSyntax()
        {
            string pattern = "TheBlanks = ? {Space, LineBreak};";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "TheBlanks",
                Syntax.Optionality(
                    Syntax.Variation(
                        Syntax.StandardPattern.Space,
                        Syntax.StandardPattern.LineBreak))));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void ExactRepetitionSyntax()
        {
            string pattern = "TheBlanks = [3 {Space, LineBreak}];";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "TheBlanks",
                Syntax.Span(
                    Syntax.Repetition(3, 3,
                        Syntax.Variation(
                            Syntax.StandardPattern.Space,
                            Syntax.StandardPattern.LineBreak)))));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void EndlessFromOneRepetitionSyntax()
        {
            string pattern = "TheBlanks = [1+ {Space, LineBreak}];";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "TheBlanks",
                Syntax.Span(
                    Syntax.Repetition(1, int.MaxValue,
                        Syntax.Variation(
                            Syntax.StandardPattern.Space,
                            Syntax.StandardPattern.LineBreak)))));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void TokenSyntax()
        {
            string pattern = "Nezaboodka = 'Nezaboodka'! ... 'Software';";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "Nezaboodka",
                Syntax.AnySpan(
                    Syntax.Text("Nezaboodka", isCaseSensitive: true),
                    Syntax.Text("Software", isCaseSensitive: false))));
            string text = package.ToString().Replace('"', '\'');
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void PatternWithFieldsSyntax()
        {
            string pattern = "Relation(Subj, Verb, Obj) = Subj ... Verb ... Obj;";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "Relation",
                Syntax.AnySpan(
                    Syntax.AnySpan(
                        Syntax.PatternReference("Subj"),
                        Syntax.PatternReference("Verb")),
                    Syntax.PatternReference("Obj")),
                Syntax.Field("Subj"),
                Syntax.Field("Verb"),
                Syntax.Field("Obj")));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void PatternReferenceSyntax()
        {
            string pattern = "Acquisition(Who, Whom) = Relation(Who, 'acquire', Whom);";
            var who = Syntax.Field("Who");
            var whom = Syntax.Field("Whom");
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "Acquisition",
                Syntax.PatternReference("Relation", who, Syntax.Text("acquire", isCaseSensitive: false), whom),
                who, whom));
            string text = package.ToString().Replace('"', '\'');
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void InsideSyntax()
        {
            string pattern = "PrimaryKeyword = Keyword @inside Sentence;";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "PrimaryKeyword",
                Syntax.Inside(
                    Syntax.PatternReference("Keyword"),
                    Syntax.PatternReference("Sentence"))));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void ParenthesisSyntax()
        {
            string pattern = "Name = [1-5 Word ... Word] + Space + (Word ... Word);";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "Name",
                Syntax.Sequence(
                    Syntax.Span(
                        Syntax.Repetition(1, 5,
                            Syntax.AnySpan(
                                Syntax.PatternReference("Word"),
                                Syntax.PatternReference("Word")))),
                    Syntax.PatternReference("Space"),
                    Syntax.AnySpan(
                        Syntax.PatternReference("Word"),
                        Syntax.PatternReference("Word")))));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void OperationPriority()
        {
            string pattern = "#Pattern = P1 ... P2 + P3 & ? {P4, P5} @inside P6;";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: true, "Pattern",
                Syntax.Inside(
                    Syntax.Conjunction(
                        Syntax.AnySpan(
                            Syntax.PatternReference("P1"),
                            Syntax.Sequence(
                                Syntax.PatternReference("P2"),
                                Syntax.PatternReference("P3"))),
                        Syntax.Optionality(
                            Syntax.Variation(
                                Syntax.PatternReference("P4"),
                                Syntax.PatternReference("P5")))),
                    Syntax.PatternReference("P6"))));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void IdentifierPattern()
        {
            string pattern = "Identifier = {Word, Word, '_'} + [0+ {Word, '_'}];";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "Identifier",
                Syntax.Sequence(
                    Syntax.Variation(
                        Syntax.PatternReference("Word"),
                        Syntax.PatternReference("Word"),
                        Syntax.Text("_", isCaseSensitive: false)),
                    Syntax.Span(
                        Syntax.Repetition(0, int.MaxValue,
                            Syntax.Variation(
                                Syntax.PatternReference("Word"),
                                Syntax.Text("_", isCaseSensitive: false)))))));
            string text = package.ToString().Replace('"', '\'');
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void EmailPattern()
        {
            string pattern = "#Email = Word + [0+ {Word, '.', '_', '+'}] + '@' + Word + [0+ {Word, '.', '_'}];";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: true, "Email",
                Syntax.Sequence(
                    Syntax.PatternReference("Word"),
                    Syntax.Span(
                        Syntax.Repetition(0, int.MaxValue,
                            Syntax.Variation(
                                Syntax.PatternReference("Word"),
                                Syntax.Text(".", isCaseSensitive: false),
                                Syntax.Text("_", isCaseSensitive: false),
                                Syntax.Text("+", isCaseSensitive: false)))),
                    Syntax.Text("@", isCaseSensitive: false),
                    Syntax.PatternReference("Word"),
                    Syntax.Span(
                        Syntax.Repetition(0, int.MaxValue,
                            Syntax.Variation(
                                Syntax.PatternReference("Word"),
                                Syntax.Text(".", isCaseSensitive: false),
                                Syntax.Text("_", isCaseSensitive: false)))))));
            string text = package.ToString().Replace('"', '\'');
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void QualifiedPatternName()
        {
            string pattern = "My.Word = Word;";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: false, "My.Word",
                Syntax.StandardPattern.Word));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }

        [TestMethod]
        public void QualifiedPatternReference()
        {
            string pattern = "#My.Word = My.Word;";
            PackageSyntax package = Syntax.Package(Syntax.Pattern(isSearchTarget: true, "My.Word",
                Syntax.PatternReference("My.Word")));
            string text = package.ToString();
            TestPackageIs(pattern, text);
        }
    }
}
