//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Nezaboodka.Text.Formatting
{
    public delegate bool MoveNextDelegate<TKind>(CustomTokenizer<TKind> tokenizer);

    public class TokenKind
    {
        public TokenKind(string literal) { Literal = literal; }
        public override string ToString() { return Literal; }
        public readonly string Literal;
    }

    public class Syntax : CustomSyntax<TokenKind>
    {
        public static TokenKind Tk(string literal)
        {
            return new TokenKind(literal);
        }

        public static TokenKind Tk(string literal, ref Dictionary<string, TokenKind> vocabulary)
        {
            if (vocabulary == null)
                vocabulary = new Dictionary<string, TokenKind>();
            var tk = new TokenKind(literal);
            vocabulary.Add(tk.Literal, tk);
            return tk;
        }

        public void AddToVocabulary(params TokenKind[] tokenKinds)
        {
            if (tokenKinds != null)
                foreach (var tk in tokenKinds)
                    Vocabulary.Add(tk.Literal, tk);
        }

        public bool TryGetFromVocabulary(string text, ref TokenKind kind)
        {
            var k = (TokenKind)null;
            var result = Vocabulary.TryGetValue(text, out k);
            if (result)
                kind = k;
            return result;
        }

        public bool TryGetFromVocabulary(char nextChar, Slice literal, ref TokenKind kind)
        {
            var t = literal.ToString() + nextChar;
            return TryGetFromVocabulary(t, ref kind);
        }
    }

    // CustomSyntax

    public class CustomSyntax<TKind>
    {
        public CustomSyntax()
        {
            Predicates = new List<CustomTokenizer<TKind>.TokenizingPredicate>();
            Vocabulary = new Dictionary<string, TKind>();
            SkipList = new List<TKind>();
        }

        public List<CustomTokenizer<TKind>.TokenizingPredicate> Predicates { get; set; }
        public Dictionary<string, TKind> Vocabulary { get; set; }
        public List<TKind> SkipList { get; set; }
        public TKind UnknownTokenKind { get; set; }
        public MoveNextDelegate<TKind> DoMoveNext { get; set; }
    }
}
