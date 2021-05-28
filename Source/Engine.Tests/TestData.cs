//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod.Engine.Tests
{
    public static class TestData
    {
        public const TokenKind Token1Kind = TokenKind.Word;
        public const WordClass Word1Class = WordClass.Alpha;
        public static string Token1Text = "test";

        public const TokenKind Token2Kind = TokenKind.Word;
        public const WordClass Word2Class = WordClass.Alpha;
        public static string Token2Text = "testing";

        public const TokenKind Token3Kind = TokenKind.Word;
        public const WordClass Word3Class = WordClass.Alpha;
        public static string Token3Text = "tested";

        public const TokenKind Token4Kind = TokenKind.Word;
        public const WordClass Word4Class = WordClass.Num;
        public static string Token4Text = "123";

        internal static readonly TokenExpression CaseSensitiveTokenExpression1 = new TokenExpression(
            syntax: Syntax.Text(nameof(CaseSensitiveTokenExpression1)), Token1Kind, Token1Text, 
            isCaseSensitive: true, textIsPrefix: false, tokenAttributes: null);

        internal static readonly TokenExpression CaseSensitiveTokenExpression2 = new TokenExpression(
            syntax: Syntax.Text(nameof(CaseSensitiveTokenExpression2)), Token2Kind, Token2Text, 
            isCaseSensitive: true, textIsPrefix: false, tokenAttributes: null);

        internal static readonly TokenExpression CaseInsensitiveTokenExpression1 = new TokenExpression(
            syntax: Syntax.Text(nameof(CaseInsensitiveTokenExpression1)), Token1Kind, Token1Text, 
            isCaseSensitive: false, textIsPrefix: false, tokenAttributes: null);

        internal static readonly TokenExpression CaseInsensitiveTokenExpression2 = new TokenExpression(
            syntax: Syntax.Text(nameof(CaseInsensitiveTokenExpression2)), Token2Kind, Token2Text, 
            isCaseSensitive: false, textIsPrefix: false, tokenAttributes: null);

        internal static readonly TokenExpression TokenKindExpression1 = new TokenExpression(
            syntax: Syntax.Text(nameof(TokenKindExpression1)), Token1Kind, null, 
            isCaseSensitive: true, textIsPrefix: false, tokenAttributes: null);

        internal static readonly TokenExpression TokenKindExpression2 = new TokenExpression(
            syntax: Syntax.Text(nameof(TokenKindExpression2)), Token4Kind, null, isCaseSensitive: true, 
            textIsPrefix: false, tokenAttributes: null);

        internal static readonly TokenExpression WordPrefixExpression1 = new TokenExpression(
            syntax: Syntax.Text(nameof(WordPrefixExpression1)), Token1Kind, Token1Text, 
            isCaseSensitive: false, textIsPrefix: true, tokenAttributes: null);

        internal static readonly TokenExpression WordPrefixExpression2 = new TokenExpression(
            syntax: Syntax.Text(nameof(WordPrefixExpression2)), Token4Kind, Token4Text, isCaseSensitive: false, 
            textIsPrefix: true, tokenAttributes: null);

        internal static readonly TokenExpression WordPrefixAttributesExpression1 = new TokenExpression(
            syntax: Syntax.Text(nameof(WordPrefixAttributesExpression1)), Token1Kind, Token1Text, isCaseSensitive: false, 
            textIsPrefix: true, new WordAttributes(WordClass.Alpha, Range.ZeroPlus(), CharCase.Lowercase));

        internal static readonly TokenExpression TokenKindAttributesExpression1 = new TokenExpression(
            syntax: Syntax.Text(nameof(TokenKindAttributesExpression1)), TokenKind.Word, 
            string.Empty, isCaseSensitive: false, textIsPrefix: true, 
            new WordAttributes(WordClass.Alpha, Range.OnePlus(), CharCase.Lowercase));
    }
}
