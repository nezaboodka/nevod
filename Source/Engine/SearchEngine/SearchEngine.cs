//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using Nezaboodka.Text.Parsing;

namespace Nezaboodka.Nevod
{
    public delegate void SearchResultCallback(SearchEngine searchEngine, MatchedTag matchedTag);

    public class SearchEngine
    {
        internal CandidateFactory fCandidateFactory;
        internal SearchContext fSearchContext;
        internal TokenEvent fTokenEvent;
        internal SearchEngineTelemetry fTelemetry;

        public PatternPackage Package { get; }
        public SearchOptions Options { get; set; }

        public SearchEngine(PatternPackage package, SearchOptions options)
        {
            if (package != null)
            {
                Package = package;
                Options = new SearchOptions(options);
                fCandidateFactory = new CandidateFactory(package.SearchQuery.PatternIndexLength);
            }
            else
                throw new ArgumentNullException(nameof(package));
        }

        public SearchEngine(PatternPackage package)
            : this(package, null)
        {
        }

        public void Restart(ITextSource textSource, SearchResultCallback resultCallback)
        {
            if (fSearchContext == null)
            {
                fTelemetry = new SearchEngineTelemetry(Options.CollectTelemetry);
                fSearchContext = new SearchContext(searchEngine: this, fCandidateFactory, Package.SearchQuery, Options,
                    textSource, resultCallback, fTelemetry);
                fTokenEvent = new TokenEvent();
            }
            else
                fSearchContext.Reset(textSource, resultCallback);
        }

        public void NextToken(Token token)
        {
            fTelemetry.SetToken(token);
            fTokenEvent.ClearResults();
            fTokenEvent.Token = token;
            fSearchContext.OnNext(fTokenEvent);
        }

        public void Complete()
        {
            fSearchContext.OnCompleted();
        }

        public SearchResult GetResult()
        {
            return fSearchContext.GetResult();
        }

        public SearchEngineTelemetry GetTelemetry()
        {
            return fTelemetry;
        }
    }
}
