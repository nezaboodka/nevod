//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;

namespace Nezaboodka.Text.Parsing
{
    public abstract class Parser : IDisposable
    {
        protected static readonly TokenReference StartToken = new TokenReference
        {
            StringPosition = 0,
            StringLength = 0,
            TokenKind = TokenKind.Start,
            XhtmlIndex = 0
        };

        protected TokenReference GetEndToken(int stringPosition)
        {
            return new TokenReference
            {
                StringPosition = stringPosition,
                StringLength = 0,
                TokenKind = TokenKind.End,
                XhtmlIndex = fParsedText.XhtmlElements.Count - 1
            };
        }

        protected int fTokenStartPosition;
        protected readonly ParsedText fParsedText = new ParsedText();
        internal readonly TokenClassifier fTokenClassifier = new TokenClassifier();
        internal readonly WordBreaker fWordBreaker = new WordBreaker();

        protected int CurrentTokenIndex => fParsedText.PlainTextTokens.Count - 1;

        // Public

        public abstract void Dispose();

        public abstract ParsedText Parse();
    }
}
