//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

namespace Nezaboodka.Text.Formatting
{
    public class SourceCodeSyntax : CustomSourceCodeSyntax
    {
        public bool VocabularySymbolsOnly { get; set; }

        // Override

        public override bool Identifier(char nextChar, Slice literal, ref TokenKind kind)
        {
            var result = base.Identifier(nextChar, literal, ref kind);
            if (!result && literal.Length > 0) // if token text is fully read
                TryGetFromVocabulary(literal.ToString(), ref kind); // e.g. lookup for keywords
            return result;
        }

        public override bool Symbol(char nextChar, Slice literal, ref TokenKind kind)
        {
            var result = TryGetFromVocabulary(nextChar, literal, ref kind); // lookup for multi-char symbols
            if (!result && !VocabularySymbolsOnly)
                result = base.Symbol(nextChar, literal, ref kind);
            return result;
        }
    }

    // CustomSourceCodeSyntax

    public class CustomSourceCodeSyntax : Syntax
    {
        public static readonly TokenKind
            TkSpace = Tk("a space"),
            TkComment = Tk("a comment"),
            TkIdentifier = Tk("an identifier"),
            TkNumber = Tk("a number"),
            TkString = Tk("a string"),
            TkSymbol = Tk("a symbol"),
            TkUnknown = Tk("an unknown symbol");

        public CustomSourceCodeSyntax()
        {
            UnknownTokenKind = TkUnknown;
            Predicates.ClearAndAdd(Space, Identifier, Number, String, Comment, Symbol);
            SkipList.Add(TkSpace, TkComment);
        }

        public virtual bool Space(char nextChar, Slice literal, ref TokenKind kind)
        {
            var result = char.IsWhiteSpace(nextChar);
            if (result)
                kind = TkSpace;
            return result;
        }

        public virtual bool Identifier(char nextChar, Slice literal, ref TokenKind kind)
        {
            var result = false;
            if (literal.Length == 0)
                result = char.IsLetter(nextChar) || nextChar == '_';
            else
                result = char.IsLetterOrDigit(nextChar) || nextChar == '_';
            if (result)
                kind = TkIdentifier;
            return result;
        }

        public virtual bool Number(char nextChar, Slice literal, ref TokenKind kind)
        {
            var result = false;
            if (literal.Length == 0)
                result = char.IsDigit(nextChar);
            else
                result = char.IsLetterOrDigit(nextChar);
            if (result)
                kind = TkNumber;
            return result;
        }

        public virtual bool String(char nextChar, Slice literal, ref TokenKind kind)
        {
            var result = nextChar == '"' || nextChar == '\'';
            if (!result && literal.Length > 0) // is inside a string already
            {
                result = literal.Length < 2;
                result = result || literal[literal.Length - 1] != literal[0];
                result = result || (literal.Length > 2 && literal[literal.Length - 2] == literal[0]);
            }
            if (result)
                kind = TkString;
            return result;
        }

        public virtual bool Comment(char nextChar, Slice literal, ref TokenKind kind)
        {
            return false; // it's up to a particular language to define syntax of comments
        }

        public virtual bool Symbol(char nextChar, Slice literal, ref TokenKind kind)
        {
            var result = literal.Length == 0 && char.IsSymbol(nextChar);
            if (result)
                kind = TkSymbol;
            return result;
        }

        public virtual bool Unknown(char nextChar, Slice literal, ref TokenKind kind)
        {
            return false;
        }
    }
}
