//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

namespace Nezaboodka.Text.Parsing
{
   internal enum WordBreak
    {
        AlphabeticLetter,
        Any,
        CarriageReturn,
        DoubleQuote,
        Empty,
        Extender,
        ExtenderForNumbersAndLetters,
        Format,
        HebrewLetter,
        Katakana,
        LineFeed,
        MidLetter,
        MidNumber,
        MidNumberAndLetter,
        Newline,
        Numeric,
        SingleQuote,
        Whitespace,
    }

    internal static class WordBreakTable
    {
        private const WordBreak AL = WordBreak.AlphabeticLetter;
        private const WordBreak W = WordBreak.Whitespace;
        private const WordBreak K = WordBreak.Katakana;
        private const WordBreak F = WordBreak.Format;
        private const WordBreak ML = WordBreak.MidLetter;
        private const WordBreak DQ = WordBreak.DoubleQuote;
        private const WordBreak MN = WordBreak.MidNumber;
        private const WordBreak CR = WordBreak.CarriageReturn;
        private const WordBreak SQ = WordBreak.SingleQuote;
        private const WordBreak ENL = WordBreak.ExtenderForNumbersAndLetters;
        private const WordBreak HL = WordBreak.HebrewLetter;
        private const WordBreak NM = WordBreak.Numeric;
        private const WordBreak E = WordBreak.Extender;
        private const WordBreak NL = WordBreak.Newline;
        private const WordBreak A = WordBreak.Any;
        private const WordBreak LF = WordBreak.LineFeed;
        private const WordBreak MNL = WordBreak.MidNumberAndLetter;
        private static readonly WordBreak[] AlphabeticLetterArray = {};
        private static readonly WordBreak[] AnyArray = {};
        
        private static readonly WordBreak[][] WordBreaks =
        {
            /* 00xx */ new[]
            {
                /* 0x */ A, A, A, A, A, A, A, A, A, W, LF, NL, NL, CR, A, A,
                /* 1x */ A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                /* 2x */ W, A, DQ, A, A, A, A, SQ, A, A, A, A, MN, A, MN, A,
                /* 3x */ NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, A, MN, A, A, A, A,
                /* 4x */ A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                /* 5x */ AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, A,
                /* 6x */ A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                /* 7x */ AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, A,
                /* 8x */ A, A, A, A, A, NL, A, A, A, A, A, A, A, A, A, A,
                /* 9x */ A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                /* Ax */ W, A, A, A, A, A, A, A, A, A, AL, A, A, F, A, A,
                /* Bx */ A, A, A, A, A, AL, A, ML, A, A, AL, A, A, A, A, A,
                /* Cx */ AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                /* Dx */ AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, AL,
                /* Ex */ AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                /* Fx */ AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, AL
            },
            /* 01xx */ AlphabeticLetterArray,
            /* 02xx */ new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, A, A, A, A, A, ML, A, A, A, A, A, A, A, A, AL, AL, AL, AL, AL, A, A, A, A, A, A, A, AL, A, AL,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            /* 03xx */ new[]
            {
                E, ENL, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
                E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
                E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
                E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, AL, AL, AL, AL, AL, A, AL,
                AL, A, A, AL, AL, AL, AL, MN, A, A, A, A, A, A, A, AL, ML, AL, AL, AL, A, AL, A, AL, AL, AL, AL, AL, AL,
                AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, AL, AL
            },
            /* 04xx */ new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, E, E, E, E, E, E, E, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL
            },
            /* 05xx */ new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, A, A,
                AL, A, A, A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, MN, A, A, A, A, A, A, A, E, E, E,
                E,
                E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
                E, E, E, E, E, E, E, E, E, E, E, A, E, A, E, E, A, E, E, A, E, A, A, A, A, A, A, A, A, HL,
                HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL,
                A, A, A, A,
                A, HL, HL, HL, AL, ML, A, A, A, A, A, A, A, A, A, A, A
            },
            /* 06xx */ new[]
            {
                F, F, F, F, F, A, A, A, A, A, A, A, MN, MN, A, A, E, E, E, E, E, E, E, E, E, E, E, A, F,
                A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
                E, E, E, E, E, E, E, NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, A, NM, MN, A, AL, AL, E, AL, AL, AL, AL, AL,
                AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, A, AL, E, E, E, E, E, E, E, F, A, E, E, E, E, E, E, AL, AL, E, E, A, E, E, E, E, AL,
                AL, NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, AL, AL, AL, A, A, AL
            },
            /* 07xx */ new[]
            {
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, F, AL, E, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, E, E, E, E, E, E, E, E,
                E,
                E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, E, E, E, E, E, E, E, E, E, AL,
                A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, AL, AL, AL, AL, AL, AL,
                AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                E, E, E, E,
                E, E, E, E, E, AL, AL, A, A, MN, A, AL, A, A, A, A, A
            },
            /* 08xx */ new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, E, E, AL,
                E, E,
                E, E, E, E, E, E, E, AL, E, E, E, AL, E, E, E, E, E, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                E, E, E, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, AL, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, E, E, E, E, E, E, E, E, E, E, E,
                E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, A
            },
            /* 09xx */ new[]
            {
                E, E, E, E, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, E,
                E, E, AL, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, AL, E, E, E, E, E, E, E, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, A, A, NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, A, AL, AL, AL,
                AL, AL, AL,
                AL, A, AL, AL, AL, AL, AL, AL, AL, A, E, E, E, A, AL, AL, AL, AL, AL, AL, AL, AL, A, A, AL, AL, A, A, AL,
                AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL,
                AL, AL, A, AL,
                A, A, A, AL, AL, AL, AL, A, A, E, AL, E, E, E, E, E, E, E, A, A, E, E, A, A, E, E, E, AL, A, A,
                A, A, A, A, A, A, E, A, A, A, A, AL, AL, A, AL, AL, AL, E, E, A, A, NM, NM, NM, NM, NM, NM, NM, NM, NM,
                NM, AL, AL, A, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            /* 0Axx */ new[]
            {
                A, E, E, E, A, AL, AL, AL, AL, AL, AL, A, A, A, A, AL, AL, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, A, AL, AL, A,
                AL, AL, A,
                A, E, A, E, E, E, E, E, A, A, A, A, E, E, A, A, E, E, E, A, A, A, E, A, A, A, A, A, A, A,
                AL, AL, AL, AL, A, AL, A, A, A, A, A, A, A, NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, E, E, AL, AL, AL, E,
                A,
                A, A, A, A, A, A, A, A, A, A, E, E, E, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, A, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL,
                AL, AL, A, AL,
                AL, A, AL, AL, AL, AL, AL, A, A, E, AL, E, E, E, E, E, E, E, E, A, E, E, E, A, E, E, E, A, A, AL,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, AL, AL, E, E, A, A, NM, NM, NM, NM, NM, NM, NM, NM, NM,
                NM, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            /* 0Bxx */ new[]
            {
                A, E, E, E, A, AL, AL, AL, AL, AL, AL, AL, AL, A, A, AL, AL, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, A, AL, AL, AL,
                AL, AL, A,
                A, E, AL, E, E, E, E, E, E, E, A, A, E, E, A, A, E, E, E, A, A, A, A, A, A, A, A, E, E, A,
                A, A, A, AL, AL, A, AL, AL, AL, E, E, A, A, NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, A, AL, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, E, AL, A, AL, AL, AL, AL, AL, AL, A, A, A, AL, AL, AL, A, AL, AL, AL,
                AL, A, A, A, AL, AL, A, AL, A, AL, AL, A, A, A, AL, AL, A, A, A, AL, AL, AL, A, A, A, AL, AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, A, A, A, A, E, E, E, E, E, A, A, A, E, E, E, A, E, E, E, E, A, A, AL,
                A, A, A, A, A, A, E, A, A, A, A, A, A, A, A, A, A, A, A, A, A, NM, NM, NM, NM, NM, NM, NM, NM, NM,
                NM, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            /* 0Cxx */ new[]
            {
                A, E, E, E, A, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL,
                AL, AL, A,
                A, A, AL, E, E, E, E, E, E, E, A, E, E, E, A, E, E, E, E, A, A, A, A, A, A, A, E, E, A, AL,
                AL, A, A, A, A, A, A, AL, AL, E, E, A, A, NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, E, E, A, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, A, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, A, AL, AL, AL, AL, AL, A, A, E, AL, E, E, E, E, E, E, E, A, E, E, E, A, E, E, E, E, A, A, A,
                A, A, A, A, E, E, A, A, A, A, A, A, A, AL, A, AL, AL, E, E, A, A, NM, NM, NM, NM, NM, NM, NM, NM, NM,
                NM, A, AL, AL, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            /* 0Dxx */ new[]
            {
                A, A, E, E, A, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                A, A, AL, E, E, E, E, E, E, E, A, E, E, E, A, E, E, E, E, AL, A, A, A, A, A, A, A, A, E, A,
                A, A, A, A, A, A, A, AL, AL, E, E, A, A, NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, A, A, A, A, A, A, A,
                A, A, A, AL, AL, AL, AL, AL, AL, A, A, E, E, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, A,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, A, A, AL, AL, AL, AL, AL, AL, AL, A, A, A, E, A, A, A, A, E,
                E,
                E, E, E, E, A, E, A, E, E, E, E, E, E, E, E, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, E, E, A, A, A, A, A, A, A, A, A, A, A, A
            },
            /* 0Exx */ new[]
            {
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, E, A, A, E, E, E, E, E, E, E,
                A, A, A, A, A, A, A, A, A, A, A, A, E, E, E, E, E, E, E, E, A, NM, NM, NM, NM, NM, NM, NM, NM, NM,
                NM, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, E, A,
                A, E, E, E, E, E, E, A, E, E, A, A, A, A, A, A, A, A, A, A, A, E, E, E, E, E, E, A, A, NM,
                NM, NM, NM, NM, NM, NM, NM, NM, NM, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            /* 0Fxx */ new[]
            {
                AL, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, E, E, A, A, A,
                A, A, A, NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, A, A, A, A, A, A, A, A, A, A, A, E, A, E, A, E, A,
                A, A, A, E, E, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, E, E, E, E,
                E, E,
                E, E, E, E, E, E, E, E, E, E, E, E, E, E, A, E, E, AL, AL, AL, AL, AL, E, E, E, E, E, E, E, E,
                E, E, E, A, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
                E, E, E, E, E, E, E, E, E, E, A, A, A, A, A, A, A, A, A, E, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            /* 10xx */ new[]
            {
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
                E, E, E, E, A, NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, A, A, A, A, A, A, A, A, A, A, A, A, E, E, E,
                E, A, A, A, A, E, E, E, A, E, E, E, A, A, E, E, E, E, E, E, E, A, A, A, E, E, E, E, A, A,
                A, A, A, A, A, A, A, A, A, A, A, E, E, E, E, E, E, E, E, E, E, E, E, A, E, NM, NM, NM, NM, NM,
                NM, NM, NM, NM, NM, E, E, E, E, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, A, A, A, A, A, AL, A,
                A, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL
            },
            /* 11xx */ AlphabeticLetterArray,
            /* 12xx */ new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, A, A, AL, AL, AL, AL, AL, AL,
                AL, A, AL,
                A, AL, AL, AL, AL, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, A, A, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, A, AL,
                AL, AL, AL, A, A, AL, AL, AL, AL, AL, AL, AL, A, AL, A, AL, AL, AL, AL, A, A, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL
            },
            /* 13xx */ new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, A, A, AL, AL, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, A, A, E, E, E, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, A, A, A, A, A, A, A, A, A, A, A
            },
            /* 14xx */ new[]
            {
                A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL
            },
            /* 15xx */ AlphabeticLetterArray,
            /* 16xx */ new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                A, A, A, AL,
                AL, AL, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            /* 17xx */ new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, E, E, E, A, A, A, A, A, A, A, A,
                A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, E, A, A, A, A, A,
                A,
                A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, A, A, A, A,
                A,
                A, A, A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, A, E, E, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
                E, E, E, A, A, A, A, A, A, A, A, A, E, A, A, NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            /* 18xx */ new[]
            {
                A, A, A, A, A, A, A, A, A, A, A, E, E, E, F, A, NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, A, A, A,
                A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, A, A, A, A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, E, AL, A, A, A, A, A, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, A, A, A, A, A, A, A, A, A, A
            },
            /* 19xx */ new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                A, A, A, E, E, E, E, E, E, E, E, E, E, E, E, A, A, A, A, E, E, E, E, E, E, E, E, E, E, E,
                E, A, A, A, A, A, A, A, A, A, A, NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, E, E, E,
                E, E, E, E, E, E, E, E, E, E, E, E, E, E, A, A, A, A, A, A, A, E, E, A, A, A, A, A, A, NM,
                NM, NM, NM, NM, NM, NM, NM, NM, NM, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            /* 1Axx */ new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, E, E,
                E, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, E, E, E, E,
                E, E, E, E, E, E, A, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
                E, E, E, E, E, E, A, A, E, NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, A, A, A, A, A, A, NM, NM, NM, NM, NM,
                NM, NM, NM, NM, NM, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            /* 1Bxx */ new[]
            {
                E, E, E, E, E, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, E, E,
                E, E, E,
                E, E, E, E, E, E, E, E, E, E, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, NM, NM, NM, NM, NM, NM, NM, NM, NM,
                NM, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, E, E, E, E, E, E, E, E, E, A, A, A,
                A, A, A, A, A, A, A, A, A, E, E, E, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, E, E, E, E, E, E, E, E, E, E, E, AL, AL, NM, NM,
                NM,
                NM, NM, NM, NM, NM, NM, NM, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, E, E, E, E, E,
                E, E,
                E, E, E, E, E, A, A, A, A, A, A, A, A, A, A, A, A
            },
            /* 1Cxx */ new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, A, A, A,
                A, A, A, A, A, NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, A, A, A, AL, AL, AL, NM, NM, NM, NM, NM, NM, NM,
                NM, NM,
                NM, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, E,
                E, E, A, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, AL, AL, AL, AL, E, AL,
                AL, AL, AL, E, E, E, AL, AL, A, A, A, A, A, A, A, A, A
            },
            /* 1Dxx */ new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
                E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, E, E, E, E
            },
            /* 1Exx */ AlphabeticLetterArray,
            /* 1Fxx */ new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, AL, AL, AL,
                AL, AL,
                AL, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, AL, AL, AL, AL, AL, AL, A, A, AL, AL, AL, AL, AL, AL,
                AL, AL, A,
                AL, A, AL, A, AL, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, A, AL, AL, AL, AL, AL, AL, AL, A, AL, A, A, A, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, A, A,
                A, AL,
                AL, AL, AL, A, A, AL, AL, AL, AL, AL, AL, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                A, A,
                A, A, A, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, A, A, A
            },
            /* 20xx */ new[]
            {
                /* 0x */ A, A, A, A, A, A, A, A, A, A, A, A, E, E, A, F,
                /* 1x */ A, A, A, A, A, A, A, A, MNL, A, A, A, A, A, A, A,
                /* 2x */ A, A, A, A, MNL, A, A, ML, NL, NL, F, F, A, F, F, A,
                /* 3x */ A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, ENL,
                /* 4x */ ENL, A, A, A, MN, A, A, A, A, A, A, A, A, A, A, A,
                /* 5x */ A, A, A, A, ENL, A, A, A, A, A, A, A, A, A, A, A,
                /* 6x */ F, F, F, F, F, A, F, F, F, F, F, F, F, F, F, F,
                /* 7x */ A, AL, A, A, A, A, A, A, A, A, A, A, A, A, A, AL,
                /* 8x */ A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                /* 9x */ AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A,
                /* Ax */ A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                /* Bx */ A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                /* Cx */ A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                /* Dx */ E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
                /* Ex */ E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
                /* Fx */ E, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            /* 21xx */ new[]
            {
                A, A, AL, A, A, A, A, AL, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, A, A, A, AL, AL, AL, AL,
                AL, A, A, A, A, A, A, AL, A, AL, A, AL, A, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                A,
                A, AL, AL, AL, AL, A, A, A, A, A, AL, AL, AL, AL, AL, A, A, A, A, AL, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, A, A, A, A, A, A, A,
                A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            /* 22xx */ AnyArray,
            /* 23xx */ AnyArray,
            /* 24xx */ new[]
            {
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A,
                A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray,
            new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, A, A, AL, AL,
                AL, AL,
                E, E, E, AL, AL, A, A, A, A, A, A, A, A, A, A, A, A
            },
            new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, A, AL, A, A, A, A, A, AL, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, A, A, A, AL, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, E, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, A, A, A, A, A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, A, AL, AL,
                AL,
                AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL,
                AL, A, AL,
                AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, A, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
                E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E
            },
            new[]
            {
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, AL, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            AnyArray,
            new[]
            {
                A, A, A, A, A, AL, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, E, E, E, E, E, E, A, K, K, K, K, K, A, A, A, A, A,
                AL, AL, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, E, E, K, K, A, A, A, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K,
                K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K,
                K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K,
                K, K, K, K, K, K, K, K, K, K, K, K, A, K, K, K, K
            },
            new[]
            {
                A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A,
                A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL,
                AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K
            },
            new[]
            {
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, K,
                K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K,
                K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, A
            },
            new[]
            {
                K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K,
                K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K,
                K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray,
            AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray,
            AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray,
            AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray,
            AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray,
            AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray,
            AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray,
            AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray,
            AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray,
            AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray,
            AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AlphabeticLetterArray,
            AlphabeticLetterArray,
            AlphabeticLetterArray, AlphabeticLetterArray,
            new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, A, A,
                A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A
            },
            AlphabeticLetterArray,
            new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, AL, AL, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, E, E, A, E,
                E, E,
                E, E, E, E, E, E, E, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, A, A, A, A, A, A, A, E, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, E, E, A, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            new[]
            {
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, AL, AL, AL, AL, A, AL, AL,
                AL, AL, A,
                A, A, A, A, A, A, A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL
            },
            new[]
            {
                AL, AL, E, AL, AL, AL, E, AL, AL, AL, AL, E, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, E, E, E, E, E, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, A, A, A,
                A, A, A, A, A, A, A, A, A, E, E, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, A, A, A, A, A, A, A, A, A, A, A, NM,
                NM, NM, NM, NM, NM, NM, NM, NM, NM, A, A, A, A, A, A, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E,
                E, E, E, AL, AL, AL, AL, AL, AL, A, A, A, AL, A, A, A, A
            },
            new[]
            {
                NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, E, E, E, E, E, E, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, E, E, E, E, E, E, E, E, E, E, E, A, A, A, A, A,
                A, A, A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, A, A, A, E, E, E, E, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                E, E, E, E, E, E, E, E, E, E, E, E, E, E, A, A, A, A, A, A, A, A, A, A, A, A, A, A, AL, NM,
                NM, NM, NM, NM, NM, NM, NM, NM, NM, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A
            },
            new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, E, E, E, E, E, E, E, E, E, E, E, E, A, A, A, A,
                A, A, A, A, A, AL, AL, AL, E, AL, AL, AL, AL, AL, AL, AL, AL, E, E, A, A, NM, NM, NM, NM, NM, NM, NM, NM,
                NM,
                NM, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, E, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, E, A, E,
                E, E, A, A, E, E, A, A, A, A, A, E, E, A, E, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, E, E,
                E, A, A, AL, AL, AL, E, E, A, A, A, A, A, A, A, A, A
            },
            new[]
            {
                A, AL, AL, AL, AL, AL, AL, A, A, AL, AL, AL, AL, AL, AL, A, A, AL, AL, AL, AL, AL, AL, A, A, A, A, A, A,
                A, A, A, AL, AL, AL, AL, AL, AL, AL, A, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, E, E, E, E, E, E, E, E, A, E, E,
                A,
                A, NM, NM, NM, NM, NM, NM, NM, NM, NM, NM, A, A, A, A, A, A
            },
            AlphabeticLetterArray, AlphabeticLetterArray, AlphabeticLetterArray, AlphabeticLetterArray,
            AlphabeticLetterArray, AlphabeticLetterArray, AlphabeticLetterArray, AlphabeticLetterArray,
            AlphabeticLetterArray, AlphabeticLetterArray,
            AlphabeticLetterArray, AlphabeticLetterArray, AlphabeticLetterArray, AlphabeticLetterArray,
            AlphabeticLetterArray, AlphabeticLetterArray, AlphabeticLetterArray, AlphabeticLetterArray,
            AlphabeticLetterArray, AlphabeticLetterArray,
            AlphabeticLetterArray, AlphabeticLetterArray, AlphabeticLetterArray, AlphabeticLetterArray,
            AlphabeticLetterArray, AlphabeticLetterArray, AlphabeticLetterArray, AlphabeticLetterArray,
            AlphabeticLetterArray, AlphabeticLetterArray,
            AlphabeticLetterArray, AlphabeticLetterArray, AlphabeticLetterArray, AlphabeticLetterArray,
            AlphabeticLetterArray, AlphabeticLetterArray, AlphabeticLetterArray, AlphabeticLetterArray,
            AlphabeticLetterArray, AlphabeticLetterArray,
            AlphabeticLetterArray, AlphabeticLetterArray, AlphabeticLetterArray,
            new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, A, A, A, A, A, A, A, A, AL, AL,
                AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A
            },
            AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray,
            AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray,
            AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray, AnyArray,
            AnyArray, AnyArray, AnyArray, AnyArray, AnyArray,
            new[]
            {
                AL, AL, AL, AL, AL, AL, AL, A, A, A, A, A, A, A, A, A, A, A, A, AL, AL, AL, AL, AL, A, A, A, A, A,
                HL, E, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, A, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, A,
                HL, HL, HL,
                HL, HL, A, HL, A, HL, HL, A, HL, HL, A, HL, HL, HL, HL, HL, HL, HL, HL, HL, HL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL
            },
            AlphabeticLetterArray,
            new[]
            {
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A,
                AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A, A, A, A,
                A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, A
            },
            new[]
            {
                E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, E, MN, A, A, ML, MN, A, A, A, A, A, A, A, A,
                A, A, A, E, E, E, E, E, E, E, A, A, A, A, A, A, A, A, A, A, A, A, ENL, ENL, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, ENL, ENL, ENL, MN, A, MNL, A, MN, ML, A, A, A,
                A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, AL, AL, AL, AL, AL, A, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL, AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, F
            },
            new[]
            {
                A, A, A, A, A, A, A, MNL, A, A, A, A, MN, A, MNL, A, A, A, A, A, A, A, A, A, A, A, ML, MN, A,
                A, A, A, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                A, A, A, A, ENL, A, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL, AL, AL,
                AL, AL, A, A, A, A, A, A, A, A, A, A, A, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K,
                K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K, K,
                K, K, K, K, K, K, K, K, K, E, E, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL,
                AL,
                AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, AL, A, A, A, AL, AL, AL, AL, AL, AL, A, A, AL, AL, AL, AL,
                AL, AL, A,
                A, AL, AL, AL, AL, AL, AL, A, A, AL, AL, AL, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A, A,
                A, A, A, A, A, A, A, A, A, A, F, F, F, A, A, A, A
            }
        };

        // Public

        public static WordBreak GetCharacterWordBreak(char c)
        {
            WordBreak result;
            int high = c >> 8;
            WordBreak[] lowArray = WordBreaks[high];
            if (lowArray == AlphabeticLetterArray)
            {
                result = WordBreak.AlphabeticLetter;
            } else if (lowArray == AnyArray)
            {
                result = WordBreak.Any;
            }
            else
            {
                result = lowArray[c & 0x00FF];
            }
            return result;
        }
    }
}
