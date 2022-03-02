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
    public class TokenSyntax : Syntax
    {
        public TokenKind TokenKind { get; }
        public new string Text { get; }
        public bool IsCaseSensitive { get; }
        public bool TextIsPrefix { get; }
        public TokenAttributes TokenAttributes { get; }

        public override void CreateChildren(string text)
        {
            if (Children == null)
            {
                var childrenBuilder = new ChildrenBuilder(text);
                childrenBuilder.AddInsideRange(TextRange);
                Children = childrenBuilder.GetChildren();
            }
        }

        internal TokenSyntax(TokenKind tokenKind, string text, bool isCaseSensitive, bool textIsPrefix,
            TokenAttributes tokenAttributes)
        {
            TokenKind = tokenKind;
            Text = text;
            IsCaseSensitive = isCaseSensitive;
            TextIsPrefix = textIsPrefix;
            TokenAttributes = tokenAttributes;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitToken(this);
        }
    }

    public partial class Syntax
    {
        public static TokenSyntax Token(TokenKind tokenKind)
        {
            var result = new TokenSyntax(tokenKind, text: null, isCaseSensitive: true, textIsPrefix: false, null);
            return result;
        }

        public static TokenSyntax Token(TokenKind tokenKind, Range lengthRange)
        {
            var result = new TokenSyntax(tokenKind, text: null, isCaseSensitive: true, textIsPrefix: false, 
                new TokenAttributes(lengthRange));
            return result;
        }

        public static TokenSyntax Token(WordClass wordClass)
        {
            WordAttributes attributes = null;
            if (wordClass != WordClass.Any)
                attributes = new WordAttributes(wordClass, Range.ZeroPlus(), CharCase.Undefined);
            var result = new TokenSyntax(TokenKind.Word, string.Empty, isCaseSensitive: true, textIsPrefix: false,
                attributes);
            return result;
        }

        public static TokenSyntax Token(WordClass wordClass, Range lengthRange)
        {
            WordAttributes attributes = null;
            if (wordClass != WordClass.Any || !lengthRange.IsZeroPlus())
                attributes = new WordAttributes(wordClass, lengthRange, CharCase.Undefined);
            var result = new TokenSyntax(TokenKind.Word, string.Empty, isCaseSensitive: true, textIsPrefix: false,
                attributes);
            return result;
        }

        public static TokenSyntax Token(WordClass wordClass, CharCase charCase)
        {
            WordAttributes attributes = null;
            if (wordClass != WordClass.Any || charCase != CharCase.Undefined)
                attributes = new WordAttributes(wordClass, Range.ZeroPlus(), charCase);
            var result = new TokenSyntax(TokenKind.Word, string.Empty, isCaseSensitive: true, textIsPrefix: false,
                attributes);
            return result;
        }

        internal static TokenSyntax Token(WordClass wordClass, Range lengthRange, CharCase charCase)
        {
            WordAttributes attributes = null;
            if (wordClass != WordClass.Any || !lengthRange.IsZeroPlus() || charCase != CharCase.Undefined)
                attributes = new WordAttributes(wordClass, lengthRange, charCase);
            var result = new TokenSyntax(TokenKind.Word, string.Empty, isCaseSensitive: true, textIsPrefix: false,
                attributes);
            return result;
        }
    }
}
