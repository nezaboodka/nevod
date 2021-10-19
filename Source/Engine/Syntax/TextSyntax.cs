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
    public class TextSyntax : Syntax
    {
        public new string Text { get; }
        public bool IsCaseSensitive { get; }
        public bool TextIsPrefix { get; }
        public WordAttributes SuffixAttributes { get; }

        internal static readonly PlainTextParserOptions TextParserOptions = new PlainTextParserOptions()
        {
            ProduceStartAndEndTokens = false,
            DetectParagraphs = false
        };

        internal override bool CanReduce => true;

        internal TextSyntax(string text, bool isCaseSensitive, bool textIsPrefix, WordAttributes suffixAttributes, bool allowEmptyText = false)
        {
            if (!allowEmptyText && string.IsNullOrEmpty(text))
                throw new ArgumentException(nameof(text));
            Text = text;
            IsCaseSensitive = isCaseSensitive;
            TextIsPrefix = textIsPrefix;
            SuffixAttributes = suffixAttributes;
        }

        internal override Syntax Reduce()
        {
            // "Email: contact@nezaboodka.com" = "Email" + ":" + Space + "contact" + "@" + "nezaboodka" + "." + "com"
            Syntax result;
            ParsedText parsedText = PlainTextParser.Parse(Text, TextParserOptions);
            var textSource = new ParsedTextSource(parsedText);
            var elements = new List<Syntax>();
            for (int i = 0, n = textSource.TokenCount; i < n; i++)
            {
                var token = textSource.GetToken(i);
                bool isCaseSensitive = token.Kind != TokenKind.Word || IsCaseSensitive;
                WordAttributes tokenAttributes = null;
                if (i == n - 1)
                {   // последний элемент
                    if (token.Kind != TokenKind.Word && TextIsPrefix)
                    {
                        elements.Add(new TokenSyntax(token.Kind, token.Text, isCaseSensitive: true,
                            textIsPrefix: false, tokenAttributes: null));
                        token = new Token(SuffixAttributes.WordClass);
                    }
                    if (token.Kind == TokenKind.Word)
                        tokenAttributes = SuffixAttributes;
                }
                string tokenText = token.Text;
                if (token.Kind == TokenKind.Space || token.Kind == TokenKind.LineBreak)
                    tokenText = null;
                var element = new TokenSyntax(token.Kind, tokenText, isCaseSensitive, TextIsPrefix, tokenAttributes);
                elements.Add(element);
            }
            if (elements.Count >= 2)
                result = Sequence(elements);
            else if (elements.Count == 1)
                result = elements[0];
            else // if (elements.Count == 0)
                result = Empty();
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitText(this);
        }
    }

    public partial class Syntax
    {
        public static TextSyntax Text(string text)
        {
            var result = new TextSyntax(text, isCaseSensitive: false, textIsPrefix: false, null);
            return result;
        }

        public static TextSyntax Text(string text, bool isCaseSensitive)
        {
            var result = new TextSyntax(text, isCaseSensitive, textIsPrefix: false, null);
            return result;
        }

        public static TextSyntax Text(string text, bool isCaseSensitive, bool textIsPrefix)
        {
            var result = new TextSyntax(text, isCaseSensitive, textIsPrefix, null);
            return result;
        }

        public static TextSyntax Text(string prefix, bool isCaseSensitive, WordAttributes suffixAttributes)
        {
            var result = new TextSyntax(prefix, isCaseSensitive, textIsPrefix: true, suffixAttributes);
            return result;
        }

        public static TextSyntax Text(string prefix, bool isCaseSensitive, WordClass suffixWordClass,
            Range suffixLengthRange)
        {
            var result = new TextSyntax(prefix, isCaseSensitive, textIsPrefix: true,
                new WordAttributes(suffixWordClass, suffixLengthRange, CharCase.Undefined));
            return result;
        }

        public static TextSyntax Text(string prefix, bool isCaseSensitive, WordClass suffixWordClass,
            CharCase suffixCharCase)
        {
            var result = new TextSyntax(prefix, isCaseSensitive, textIsPrefix: true,
                new WordAttributes(suffixWordClass, Range.ZeroPlus(), suffixCharCase));
            return result;
        }

        public static TextSyntax Text(string prefix, bool isCaseSensitive, WordClass suffixWordClass,
            Range suffixLengthRange, CharCase suffixCharCase)
        {
            var result = new TextSyntax(prefix, isCaseSensitive, textIsPrefix: true,
                new WordAttributes(suffixWordClass, suffixLengthRange, suffixCharCase));
            return result;
        }

        internal static TextSyntax EmptyText(bool isCaseSensitive, WordAttributes suffixAttributes)
        { 
            var result = new TextSyntax(text: null, isCaseSensitive, textIsPrefix: true, 
                suffixAttributes, allowEmptyText: true);
            return result;
        }

        internal static TextSyntax EmptyText(bool isCaseSensitive, bool textIsPrefix)
        {
            var result = new TextSyntax(text: null, isCaseSensitive, textIsPrefix, 
                null, allowEmptyText: true);
            return result;
        }
    }
}
