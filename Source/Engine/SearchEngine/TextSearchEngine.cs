//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Nezaboodka.Text.Parsing;

namespace Nezaboodka.Nevod
{
    public class TextSearchEngine : SearchEngine
    {
        internal IEnumerator<Token> fTokenEnumerator;

        public ITextSource TextSource { get; private set; }
        public SearchResultCallback ResultCallback { get; private set; }

        public TextSearchEngine(PatternPackage package)
            : this(package, null)
        {
        }

        public TextSearchEngine(PatternPackage package, SearchOptions options)
            : base(package, options)
        {
        }

        public SearchResult Search(string text)
        {
            var parsedText = GetParsedText(text);
            SearchResult result = Search(parsedText);
            return result;
        }

        public SearchResult Search(ParsedText parsedText)
        {
            var textSource = new ParsedTextSource(parsedText);
            SearchResult result = Search(textSource);
            return result;
        }

        public SearchResult Search(ITextSource textSource)
        {
            Search(textSource, resultCallback: null);
            return GetResult();
        }

        public void Search(ITextSource textSource, SearchResultCallback resultCallback)
        {
            Restart(textSource, resultCallback);
            while (MoveNext())
            {
                // Do nothing
            }
        }

        public void Restart()
        {
            Restart(TextSource, ResultCallback);
        }

        public void Restart(string text)
        {
            var parsedText = GetParsedText(text);
            Restart(parsedText);
        }

        public void Restart(ParsedText parsedText)
        {
            var textSource = new ParsedTextSource(parsedText);
            Restart(textSource);
        }

        public void Restart(ITextSource textSource)
        {
            Restart(textSource, resultCallback: null);
        }

        public new void Restart(ITextSource textSource, SearchResultCallback resultCallback)
        {
            fTokenEnumerator = textSource.GetEnumerator();
            TextSource = textSource;
            ResultCallback = resultCallback;
            base.Restart(textSource, resultCallback);
        }

        public bool MoveNext()
        {
            bool result = false;
            if (fTokenEnumerator != null)
            {
                result = fTokenEnumerator.MoveNext();
                if (result)
                    NextToken(fTokenEnumerator.Current);
                else
                    Complete();
            }
            return result;
        }

        public bool MoveUntil(int targetTextTokenNumber)
        {
            bool result = false;
            if (fTokenEnumerator != null)
            {
                if (targetTextTokenNumber < fTokenEnumerator.Current.Location.TokenNumber)
                    fTokenEnumerator.Reset();
                result = fTokenEnumerator.Current.Location.TokenNumber < targetTextTokenNumber;
                while (fTokenEnumerator.Current.Location.TokenNumber < targetTextTokenNumber && result)
                    result = MoveNext();
            }
            return result;
        }

        // Internal

        internal ParsedText GetParsedText(string text)
        {
            var options = new PlainTextParserOptions()
            {
                ProduceStartAndEndTokens = true,
                DetectParagraphs = false
            };
            ParsedText result = PlainTextParser.Parse(text, options);
            return result;
        }
    }
}
