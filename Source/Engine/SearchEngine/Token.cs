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
    public enum TokenKind
    {
        Word = 0,
        Space = 1,
        Punctuation = 2,
        Symbol = 3,
        LineBreak = 4,
        Start = 5,
        End = 6,
        Undefined = 7
    }

    public enum WordClass
    {
        Any = 0,
        Alpha = 1,
        Num = 2,
        AlphaNum = 3,
        NumAlpha = 4
    }

    public class Token
    {
        public TokenKind Kind { get; set; }
        public WordClass WordClass { get; set; }
        public string Text { get; set; }
        public TextLocation Location { get; set; }

        public Token(TokenKind tokenKind, WordClass wordClass, string text, TextLocation location)
        {
            Kind = tokenKind;
            WordClass = wordClass;
            Text = text;
            Location = location;
        }

        public Token(TokenKind tokenKind)
        {
            Kind = tokenKind;
            WordClass = WordClass.Any;
            Text = null;
        }

        public Token(WordClass wordClass)
        {
            Kind = TokenKind.Word;
            WordClass = wordClass;
            Text = string.Empty;
        }

        public Token(WordClass wordClass, string word)
        {
            Kind = TokenKind.Word;
            WordClass = wordClass;
            Text = word;
        }

        public override string ToString()
        {
            string result;
            switch (Kind)
            {
                case TokenKind.Space:
                case TokenKind.LineBreak:
                case TokenKind.Start:
                case TokenKind.End:
                case TokenKind.Undefined:
                    result = "Token." + Kind.ToString();
                    break;
                default:
                    result = $"\"{Text}\"";
                    break;
            }
            if (Location != null)
                result += $" [{Location.TokenNumber}, {Location.Position}->{Location.Length}]";
            return result;
        }
    }

    public class TextLocation
    {
        public long TokenNumber { get; set; }
        public long Position { get; set; }
        public long Length { get; set; }
        public TextLocationContext Context { get; set; }

        public TextLocation(long tokenNumber, long position, long length,
            TextLocationContext context = null)
        {
            TokenNumber = tokenNumber;
            Position = position;
            Length = length;
            Context = context;
        }

        public TextLocation(TextLocation source)
            : this(source.TokenNumber, source.Position, source.Length, context: null)
        {
        }
    }

    public class TextLocationContext
    {
        public TextLocation PreviousLineBreak;
        public TextLocation CurrentLineBreak;

        public TextLocationContext(TextLocation previousLineBreak, TextLocation currentLineBreak)
        {
            PreviousLineBreak = previousLineBreak;
            CurrentLineBreak = currentLineBreak;
        }
    }
}
