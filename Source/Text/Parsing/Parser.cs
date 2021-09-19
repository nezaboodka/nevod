//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;

namespace Nezaboodka.Text.Parsing
{
    public abstract class Parser : IDisposable
    {
        protected int fTokenStartPosition;
        protected readonly ParsedText fParsedText = new ParsedText();
        internal TokenClassifier fTokenClassifier = TokenClassifier.Create();
        internal WordBreaker fWordBreaker = WordBreaker.Create();

        protected int CurrentTokenIndex => fParsedText.PlainTextTokens.Count - 1;

        // Public

        public abstract void Dispose();

        public abstract ParsedText Parse();

        // Internal

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
    }
}
