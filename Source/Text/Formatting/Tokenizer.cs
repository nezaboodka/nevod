//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Nezaboodka.Text.Formatting
{
    public class Tokenizer : CustomTokenizer<TokenKind>
    {
        public Tokenizer()
        {
        }

        public Tokenizer(CustomSyntax<TokenKind> syntax)
        {
            Syntax = syntax;
        }

        public static TokenKind Tk(string literal)
        {
            return new TokenKind(literal);
        }
    }

    // TokenOccurrence

    public struct TokenOccurrence<TKind>
    {
        public TKind Kind;
        public Slice Literal;
    }

    // CustomTokenizer

    public class CustomTokenizer<TKind>
    {
        public delegate bool TokenizingPredicate(char nextChar, Slice literal, ref TKind kind);

        public string SourceText
        {
            get { return fSourceText; }
            set
            {
                if (fPosition >= 0)
                    throw new Exception("cannot change SourceText property while tokenizing is in progress");
                SourceReader = ReaderFactory.GetReader(value);
                fSourceText = value;
            }
        }

        public Reader<char> SourceReader
        {
            get { return fSourceReader; }
            set
            {
                if (fPosition >= 0)
                    throw new Exception( "cannot change Reader property while tokenizing is in progress");
                fSourceText = null;
                fSourceReader = value;
            }
        }

        public CustomSyntax<TKind> Syntax
        {
            get { return fSyntax; }
            set { fSyntax = value; }
        }

        public bool MoveNext()
        {
            var result = Syntax.DoMoveNext == null ? DoMoveNext() : Syntax.DoMoveNext(this);
            while (result && CurrentIn(Syntax.SkipList))
                result = Syntax.DoMoveNext == null ? DoMoveNext() : Syntax.DoMoveNext(this);
            return result;
        }

        public TokenOccurrence<TKind> Current
        {
            get
            {
                return new TokenOccurrence<TKind>()
                {
                    Kind = fTokenKind,
                    Literal = fTokenLiteral
                };
            }
        }

        public bool Eof
        {
            get { return fEof && fTokenLiteral.Length == 0; }
        }

        public IEnumerable<TokenOccurrence<TKind>> ReadAllTokens()
        {
            while (MoveNext())
                yield return Current;
        }

        public Slice Take(TKind kind)
        {
            return DoTake(kind);
        }

        public string TakeAsString(TKind kind)
        {
            return Take(kind).ToString();
        }

        public bool Skip(TKind kind)
        {
            return DoSkip(kind, null);
        }

        public bool SkipAndSetSyntax(TKind kind, CustomSyntax<TKind> syntax)
        {
            return DoSkip(kind, syntax);
        }

        public bool CurrentIn(params TKind[] kinds)
        {
            return CurrentIn((IEnumerable<TKind>)kinds);
        }

        public bool CurrentIn(IEnumerable<TKind> kinds)
        {
            var kind = Current.Kind;
            foreach (var x in kinds)
                if (x.Equals(kind))
                    return true;
            return false;
        }

        // Implementation

        protected virtual bool DoMoveNext()
        {
            fTokenKind = Syntax.UnknownTokenKind;
            fTokenLiteral = "".Slice(0, 0);
            if (fPosition < 0 && !fEof)
                fEof = !SourceReader(out fNextChar);
            if (!fEof)
            {
                var predicates = Syntax.Predicates; // (!) must be saved in local variable
                foreach (var p in predicates)
                    if (TryMoveNext(p, ref fTokenLiteral, ref fTokenKind))
                        break;
                if (fTokenLiteral.Length == 0)
                {
                    if (predicates == Syntax.Predicates) // if syntax was not changed
                        fTokenLiteral = JoinChar(fTokenLiteral, fNextChar);
                    else // if syntax was changed
                        predicates = Syntax.Predicates;
                }
                else
                    predicates = null;
                fPosition += fTokenLiteral.Length;
            }
            return fTokenLiteral.Length > 0;
        }

        protected virtual bool TryMoveNext(TokenizingPredicate predicate,
            ref Slice literal, ref TKind kind)
        {
            if (predicate != null)
            {
                while (!fEof && predicate(fNextChar, literal, ref kind))
                {
                    literal = JoinChar(literal, fNextChar);
                    fEof = !SourceReader(out fNextChar);
                }
            }
            return literal.Length != 0;
        }

        protected Slice DoTake(TKind kind)
        {
            var result = default(Slice);
            var t = Current.Kind;
            if (kind.Equals(t))
            {
                result = Current.Literal;
                MoveNext();
            }
            else
                throw new Exception(string.Format("{0} expected instead of {1}", kind, t));
            return result;
        }

        protected bool DoSkip(TKind kind, CustomSyntax<TKind> syntax)
        {
            var result = kind.Equals(Current.Kind);
            if (result)
            {
                if (syntax != null)
                    Syntax = syntax;
                MoveNext();
            }
            return result;
        }

        protected Slice JoinChar(Slice literal, char nextChar)
        {
            if (SourceText == null)
                literal = (literal + fNextChar.ToString()).Slice(0);
            else
                literal = SourceText.Slice((int)fPosition + 1, (int)literal.Length + 1);
            return literal;
        }

        // Fields
        private string fSourceText;
        private Reader<char> fSourceReader;
        private CustomSyntax<TKind> fSyntax;
        private TKind fTokenKind = default(TKind);
        private Slice fTokenLiteral = default(Slice);
        private char fNextChar = default(char);
        private long fPosition = -1;
        private bool fEof = false;
    }
}
