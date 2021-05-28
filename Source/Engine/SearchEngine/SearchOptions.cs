//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System;
using System.IO;

namespace Nezaboodka.Nevod
{
    public enum LevelOfDetails
    {
        MatchedTagsOnly = 0,
        MatchedTagsWithTokens = 1,
        MatchedTagsWithSyntaxDetails = 2
    }

    public class SearchOptions
    {
        public int CandidateLimit { get; set; }
        public int PatternCandidateLimit { get; set; }
        public LevelOfDetails LevelOfDetails { get; set; }
        public bool NonTargetTagsInResults { get; set; }
        public bool SelfOverlappingTagsInResults { get; set; }
        public bool FirstMatchOnly { get; set; }
        public bool IsDebugMode { get; set; }
        public bool CollectTelemetry { get; set; }

        internal const int DefaultCandidateLimit = 250_000;
        internal const int DefaultPatternCandidateLimit = 5_000;

        internal int MaxCountOfMatchedTagsWaitingForCleanup { get; set; }
        internal int TokenCountToWaitToPerformGarbageCollection { get; set; }
        internal int NewWaitingTokenCountToPerformGarbageCollection { get; set; }

        internal bool SuppressCandidateLimitExceeded { get; set; }

        internal const int DefaultMaxCountOfMatchedTagsWaitingForCleanup = 50;
        internal const int DefaultTokenCountToWaitToPerformGarbageCollection = 1_000;
        internal const int DefaultNewWaitingTokenCountToPerformGarbageCollection = 100_000;

        public SearchOptions()
        {
            InitDefaultFields();
        }

        public SearchOptions(SearchOptions source)
        {
            if (source != null)
            {
                CandidateLimit = source.CandidateLimit;
                PatternCandidateLimit = source.PatternCandidateLimit;
                LevelOfDetails = source.LevelOfDetails;
                NonTargetTagsInResults = source.NonTargetTagsInResults;
                SelfOverlappingTagsInResults = source.SelfOverlappingTagsInResults;
                FirstMatchOnly = source.FirstMatchOnly;
                IsDebugMode = source.IsDebugMode;
                CollectTelemetry = source.CollectTelemetry;

                MaxCountOfMatchedTagsWaitingForCleanup = source.MaxCountOfMatchedTagsWaitingForCleanup;
                TokenCountToWaitToPerformGarbageCollection = source.TokenCountToWaitToPerformGarbageCollection;
                NewWaitingTokenCountToPerformGarbageCollection = source.NewWaitingTokenCountToPerformGarbageCollection;
                SuppressCandidateLimitExceeded = source.SuppressCandidateLimitExceeded;
            }
            else
            {
                InitDefaultFields();
            }
        }

        // Internal
        private void InitDefaultFields()
        {
            CandidateLimit = DefaultCandidateLimit;
            PatternCandidateLimit = DefaultPatternCandidateLimit;

            MaxCountOfMatchedTagsWaitingForCleanup = DefaultMaxCountOfMatchedTagsWaitingForCleanup;
            TokenCountToWaitToPerformGarbageCollection = DefaultTokenCountToWaitToPerformGarbageCollection;
            NewWaitingTokenCountToPerformGarbageCollection = DefaultNewWaitingTokenCountToPerformGarbageCollection;
        }
    }
}
