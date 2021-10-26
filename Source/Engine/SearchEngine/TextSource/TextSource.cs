//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Nezaboodka.Nevod
{
    public interface ITextSource : IEnumerable<Token>
    {
        string GetText(TextLocation start, TextLocation end);
    }

    public abstract class TextSource : ITextSource
    {
        public abstract string GetText(TextLocation start, TextLocation end);
        public abstract IEnumerator<Token> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static readonly TokenKind[] TokenKindByTokenReferenceKind =
        {
            TokenKind.Undefined,    // Undefined = 0,
            TokenKind.Start,        // Start = 1,
            TokenKind.End,          // End = 2,
            TokenKind.Word,         // Alphabetic = 3,
            TokenKind.Word,         // AlphaNumeric = 4,
            TokenKind.Word,         // NumericAlpha = 5,
            TokenKind.LineBreak,    // LineFeed = 6,
            TokenKind.Word,         // Numeric = 7,
            TokenKind.Punctuation,  // Punctuation = 8,
            TokenKind.Symbol,       // Symbol = 9,
            TokenKind.Space         // WhiteSpace = 10
        };

        public static readonly WordClass[] WordClassByTokenReferenceKind =
        {
            WordClass.Any,           // Undefined = 0,
            WordClass.Any,           // Start = 1,
            WordClass.Any,           // End = 2,
            WordClass.Alpha,         // Alphabetic = 3,
            WordClass.AlphaNum,      // AlphaNumeric = 4,
            WordClass.NumAlpha,      // NumericAlpha = 5,
            WordClass.Any,           // LineFeed = 6,
            WordClass.Num,           // Numeric = 7,
            WordClass.Any,           // Punctuation = 8,
            WordClass.Any,           // Symbol = 9,
            WordClass.Any            // WhiteSpace = 10
        };
    }
}
