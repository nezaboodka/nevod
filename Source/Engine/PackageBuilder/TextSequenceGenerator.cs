//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Nezaboodka.Text.Parsing;

namespace Nezaboodka.Nevod
{
    internal static class TextSequenceGenerator
    {
        public static SequenceExpression Generate(string text, bool isCaseSensitive)
        {
            SequenceExpression sequenceExpression = BuildTextSequenceExpression(text, isCaseSensitive);
            BuildIndex(sequenceExpression);
            return sequenceExpression;
        }

        // Internal

        private static SequenceExpression BuildTextSequenceExpression(string text, bool isCaseSensitive)
        {
            ParsedText parsedText = PlainTextParser.Parse(text, TextSyntax.TextParserOptions);
            var textSource = new ParsedTextSource(parsedText);
            var elements = new TokenExpression[textSource.TokenCount];
            for (int i = 0, n = textSource.TokenCount; i < n; i++)
            {
                var token = textSource.GetToken(i);
                bool isCaseSensitiveToken = token.Kind != TokenKind.Word || isCaseSensitive;
                var tokenExpression = new TokenExpression(syntax: null, token.Kind, token.Text, isCaseSensitiveToken, 
                    textIsPrefix: false, tokenAttributes: null);
                elements[i] = tokenExpression;
            }
            var result = new SequenceExpression(syntax: null, elements);
            return result;
        }

        private static void BuildIndex(SequenceExpression sequence)
        {
            for (int i = 0, n = sequence.Elements.Length; i < n; i++)
            {
                var token = (TokenExpression)sequence.Elements[i];
                sequence.OwnIndex = token.OwnIndex = ExpressionIndex.CreateFromToken(token);
            }
        }
    }
}
