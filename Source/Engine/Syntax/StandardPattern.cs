//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Nezaboodka.Text.Parsing;

namespace Nezaboodka.Nevod
{
    public partial class Syntax
    {
        public static TokenSyntax Start;
        public static TokenSyntax End;
        public static TokenSyntax Word;
        public static TokenSyntax Alpha;
        public static TokenSyntax Num;
        public static TokenSyntax AlphaNum;
        public static TokenSyntax NumAlpha;
        public static TokenSyntax Punct;
        public static TokenSyntax Symbol;
        public static TokenSyntax Space;
        public static TokenSyntax LineBreak;

        public static class StandardPattern
        {
            public static PatternReferenceSyntax Start;
            public static PatternReferenceSyntax End;
            public static PatternReferenceSyntax Word;
            public static PatternReferenceSyntax Alpha;
            public static PatternReferenceSyntax Num;
            public static PatternReferenceSyntax AlphaNum;
            public static PatternReferenceSyntax NumAlpha;
            public static PatternReferenceSyntax Punct;
            public static PatternReferenceSyntax Symbol;
            public static PatternReferenceSyntax Space;
            public static PatternReferenceSyntax LineBreak;
            public static PatternReferenceSyntax Any;
            public static PatternReferenceSyntax Blank;
            public static PatternReferenceSyntax WordBreak;

            public static PatternReferenceSyntax Начало;
            public static PatternReferenceSyntax Конец;
            public static PatternReferenceSyntax Слово;
            public static PatternReferenceSyntax Буквы;
            public static PatternReferenceSyntax Цифры;
            public static PatternReferenceSyntax БуквыЦифры;
            public static PatternReferenceSyntax ЦифрыБуквы;
            public static PatternReferenceSyntax ЗнакПрепинания;
            public static PatternReferenceSyntax Символ;
            public static PatternReferenceSyntax Пробел;
            public static PatternReferenceSyntax РазделительСтрок;
            public static PatternReferenceSyntax Любое;
            public static PatternReferenceSyntax Пропуск;
            public static PatternReferenceSyntax РазделительСлов;

            internal static TokenSyntax StartToken;
            internal static TokenSyntax EndToken;
            internal static TokenSyntax WordToken;
            public static TokenSyntax WordAlphaToken;
            public static TokenSyntax WordNumToken;
            public static TokenSyntax WordAlphaNumToken;
            public static TokenSyntax WordNumAlphaToken;
            internal static TokenSyntax PunctToken;
            internal static TokenSyntax SymbolToken;
            internal static TokenSyntax SpaceToken;
            internal static TokenSyntax LineBreakToken;
            internal static FundamentalPatternSyntax StartPattern;
            internal static FundamentalPatternSyntax EndPattern;
            internal static FundamentalPatternSyntax WordPattern;
            internal static FundamentalPatternSyntax AlphaPattern;
            internal static FundamentalPatternSyntax NumPattern;
            internal static FundamentalPatternSyntax AlphaNumPattern;
            internal static FundamentalPatternSyntax NumAlphaPattern;
            internal static FundamentalPatternSyntax PunctPattern;
            internal static FundamentalPatternSyntax SymbolPattern;
            internal static FundamentalPatternSyntax SpacePattern;
            internal static FundamentalPatternSyntax LineBreakPattern;
            internal static SystemPatternSyntax AnyPattern;
            internal static SystemPatternSyntax BlankPattern;
            internal static SystemPatternSyntax WordBreakPattern;

            internal static FundamentalPatternSyntax ШаблонНачало;
            internal static FundamentalPatternSyntax ШаблонКонец;
            internal static FundamentalPatternSyntax ШаблонСлово;
            internal static FundamentalPatternSyntax ШаблонБуквы;
            internal static FundamentalPatternSyntax ШаблонЦифры;
            internal static FundamentalPatternSyntax ШаблонБуквыЦифры;
            internal static FundamentalPatternSyntax ШаблонЦифрыБуквы;
            internal static FundamentalPatternSyntax ШаблонЗнакПрепинания;
            internal static FundamentalPatternSyntax ШаблонСимвол;
            internal static FundamentalPatternSyntax ШаблонПробел;
            internal static FundamentalPatternSyntax ШаблонРазделительСтрок;
            internal static SystemPatternSyntax ШаблонЛюбое;
            internal static SystemPatternSyntax ШаблонПропуск;
            internal static SystemPatternSyntax ШаблонРазделительСлов;

            internal static PatternSyntax[] StandardPatterns;

            static StandardPattern()
            {
                StartToken = Token(TokenKind.Start);
                EndToken = Token(TokenKind.End);
                WordToken = Token(TokenKind.Word);
                WordAlphaToken = Token(WordClass.Alpha);
                WordNumToken = Token(WordClass.Num);
                WordAlphaNumToken = Token(WordClass.AlphaNum);
                WordNumAlphaToken = Token(WordClass.NumAlpha);
                PunctToken = Token(TokenKind.Punctuation);
                SymbolToken = Token(TokenKind.Symbol);
                SpaceToken = Token(TokenKind.Space);
                LineBreakToken = Token(TokenKind.LineBreak);

                StartPattern = new FundamentalPatternSyntax("Start", StartToken);
                EndPattern = new FundamentalPatternSyntax("End", EndToken);
                WordPattern = new FundamentalPatternSyntax("Word", WordToken);
                AlphaPattern = new FundamentalPatternSyntax("Alpha", WordAlphaToken);
                NumPattern = new FundamentalPatternSyntax("Num", WordNumToken);
                AlphaNumPattern = new FundamentalPatternSyntax("AlphaNum", WordAlphaNumToken);
                NumAlphaPattern = new FundamentalPatternSyntax("NumAlpha", WordNumAlphaToken);
                PunctPattern = new FundamentalPatternSyntax("Punct", PunctToken);
                SymbolPattern = new FundamentalPatternSyntax("Symbol", SymbolToken);
                SpacePattern = new FundamentalPatternSyntax("Space", SpaceToken);
                LineBreakPattern = new FundamentalPatternSyntax("LineBreak", LineBreakToken);

                ШаблонНачало = new FundamentalPatternSyntax("Начало", StartToken);
                ШаблонКонец = new FundamentalPatternSyntax("Конец", EndToken);
                ШаблонСлово = new FundamentalPatternSyntax("Слово", WordToken);
                ШаблонБуквы = new FundamentalPatternSyntax("Буквы", WordAlphaToken);
                ШаблонЦифры = new FundamentalPatternSyntax("Цифры", WordNumToken);
                ШаблонБуквыЦифры = new FundamentalPatternSyntax("БуквыЦифры", WordAlphaNumToken);
                ШаблонЦифрыБуквы = new FundamentalPatternSyntax("ЦифрыБуквы", WordNumAlphaToken);
                ШаблонЗнакПрепинания = new FundamentalPatternSyntax("ЗнакПрепинания", PunctToken);
                ШаблонСимвол = new FundamentalPatternSyntax("Символ", SymbolToken);
                ШаблонПробел = new FundamentalPatternSyntax("Пробел", SpaceToken);
                ШаблонРазделительСтрок = new FundamentalPatternSyntax("РазделительСтрок", LineBreakToken);

                // The following variables must be initialized prior to WordBreakPattern!!!
                Start = new PatternReferenceSyntax(StartPattern, EmptyFieldList());
                End = new PatternReferenceSyntax(EndPattern, EmptyFieldList());
                Word = new PatternReferenceSyntax(WordPattern, EmptyFieldList());
                Punct = new PatternReferenceSyntax(PunctPattern, EmptyFieldList());
                Symbol = new PatternReferenceSyntax(SymbolPattern, EmptyFieldList());
                Space = new PatternReferenceSyntax(SpacePattern, EmptyFieldList());
                LineBreak = new PatternReferenceSyntax(LineBreakPattern, EmptyFieldList());
                // The following variables must be initialized after Start, End, Word, Punct, Symbol, Space, LineBreak!!!
                AnyPattern = new SystemPatternSyntax("Any",
                    new VariationSyntax(new[]
                    {
                        Word,
                        Space,
                        Punct,
                        Symbol,
                        LineBreak
                    }, checkCanReduce: false));
                BlankPattern = new SystemPatternSyntax("Blank",
                    new VariationSyntax(new[]
                    {
                        Space,
                        LineBreak
                    }, checkCanReduce: false));
                WordBreakPattern = new SystemPatternSyntax("WordBreak",
                    new VariationSyntax(new[]
                    {
                        Space,
                        Punct,
                        Symbol,
                        LineBreak
                    }, checkCanReduce: false));
                Any = new PatternReferenceSyntax(AnyPattern, EmptyFieldList());
                Blank = new PatternReferenceSyntax(BlankPattern, EmptyFieldList());
                WordBreak = new PatternReferenceSyntax(WordBreakPattern, EmptyFieldList());

                // The following variables must be initialized prior to ШаблонРазделительСлов!!!
                Начало = new PatternReferenceSyntax(ШаблонНачало, EmptyFieldList());
                Конец = new PatternReferenceSyntax(ШаблонКонец, EmptyFieldList());
                Слово = new PatternReferenceSyntax(ШаблонСлово, EmptyFieldList());
                ЗнакПрепинания = new PatternReferenceSyntax(ШаблонЗнакПрепинания, EmptyFieldList());
                Символ = new PatternReferenceSyntax(ШаблонСимвол, EmptyFieldList());
                Пробел = new PatternReferenceSyntax(ШаблонПробел, EmptyFieldList());
                РазделительСтрок = new PatternReferenceSyntax(ШаблонРазделительСтрок, EmptyFieldList());
                // The following variables must be initialized after Начало, Конец, Слово, ЗнакПрепинания, Символ, Пробел, РазделительСтрок!!!
                ШаблонЛюбое = new SystemPatternSyntax("Любое",
                    new VariationSyntax(new[]
                    {
                        Слово,
                        Пробел,
                        ЗнакПрепинания,
                        Символ,
                        РазделительСтрок
                    }, checkCanReduce: false));
                ШаблонПропуск = new SystemPatternSyntax("Пропуск",
                    new VariationSyntax(new[]
                    {
                        Пробел,
                        РазделительСтрок
                    }, checkCanReduce: false));
                ШаблонРазделительСлов = new SystemPatternSyntax("РазделительСлов",
                    new VariationSyntax(new[]
                    {
                        Пробел,
                        ЗнакПрепинания,
                        Символ,
                        РазделительСтрок
                    }, checkCanReduce: false));
                Любое = new PatternReferenceSyntax(ШаблонЛюбое, EmptyFieldList());
                Пробел = new PatternReferenceSyntax(ШаблонПробел, EmptyFieldList());
                РазделительСлов = new PatternReferenceSyntax(ШаблонРазделительСлов, EmptyFieldList());

                StandardPatterns = new PatternSyntax[]
                {
                    StartPattern, EndPattern, WordPattern, AlphaPattern, NumPattern, AlphaNumPattern,
                    NumAlphaPattern, PunctPattern, SymbolPattern, SpacePattern, LineBreakPattern,
                    AnyPattern, BlankPattern, WordBreakPattern,

                    ШаблонНачало, ШаблонКонец, ШаблонСлово, ШаблонБуквы, ШаблонЦифры, ШаблонБуквыЦифры,
                    ШаблонЦифрыБуквы, ШаблонЗнакПрепинания, ШаблонСимвол, ШаблонПробел, ШаблонРазделительСтрок,
                    ШаблонЛюбое, ШаблонПропуск, ШаблонРазделительСлов
                };
            }
        }

        static Syntax()
        {
            Start = StandardPattern.StartToken;
            End = StandardPattern.EndToken;
            Word = StandardPattern.WordToken;
            Alpha = StandardPattern.WordAlphaToken;
            Num = StandardPattern.WordNumToken;
            AlphaNum = StandardPattern.WordAlphaNumToken;
            NumAlpha = StandardPattern.WordNumAlphaToken;
            Punct = StandardPattern.PunctToken;
            Symbol = StandardPattern.SymbolToken;
            Space = StandardPattern.SpaceToken;
            LineBreak = StandardPattern.LineBreakToken;
        }
    }
}
