//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Nezaboodka.Text.Parsing;

namespace Nezaboodka.Nevod
{
    public class ParsedTextSource : TextSource
    {
        public ParsedText Text { get; }

        public int TokenCount => Text.PlainTextTokens.Count;

        public ParsedTextSource(ParsedText text)
        {
            Text = text;
        }

        public override  IEnumerator<Token> GetEnumerator()
        {
            for (int i = 0; i < Text.PlainTextTokens.Count; i++)
                yield return GetToken(i);
        }

        public override string GetText(TextLocation start, TextLocation end)
        {
            string result;
            long startTokenNumber = start.TokenNumber;
            if (startTokenNumber < 0)
                startTokenNumber = ~startTokenNumber;
            long endTokenNumber;
            if (start != end)
            {
                endTokenNumber = end.TokenNumber;
                if (endTokenNumber < 0)
                    endTokenNumber = ~endTokenNumber;
            }
            else
                endTokenNumber = startTokenNumber;
            int internalStartTokenNumber = (int)startTokenNumber;
            if (startTokenNumber != endTokenNumber)
            {
                var sb = new StringBuilder();
                for (int i = internalStartTokenNumber; i <= endTokenNumber; i++)
                    sb.Append(Text.GetTokenText(Text.PlainTextTokens[i]));
                result = sb.ToString();
            }
            else
                result = Text.GetTokenText(Text.PlainTextTokens[internalStartTokenNumber]);
            return result;
        }

        public Token GetToken(int tokenNumber)
        {
            TokenReference tokenReference = Text.PlainTextTokens[tokenNumber];
            Token result = new Token(TokenKindByTokenReferenceKind[(int)tokenReference.TokenKind],
                WordClassByTokenReferenceKind[(int)tokenReference.TokenKind],
                Text.GetTokenText(tokenReference),
                new TextLocation(tokenNumber, tokenReference.StringPosition, tokenReference.StringLength));
            return result;
        }
    }
}
