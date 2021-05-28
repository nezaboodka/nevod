//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System;
using System.IO;
using Nezaboodka.Text.Parsing;

namespace Nezaboodka.Nevod
{
    internal enum MatchingEventResultStatus
    {
        Ignore = 0,
        UpdateEventObserver = 1,
        Reject = 2,
        Complete = 3
    }

    internal abstract class MatchingEvent
    {
        public MatchingEventResultStatus ResultStatus { get; private set; }
        public Candidate ResultEventObserver { get; private set; }
        public RepetitionCandidate InnerRepetitionCandidate { get; set; }
        public abstract TextLocation Location { get; }

        public void ClearResults()
        {
            ResultStatus = MatchingEventResultStatus.Ignore;
            ResultEventObserver = null;
            InnerRepetitionCandidate = null;
        }

        public void Ignore()
        {
            ResultStatus = MatchingEventResultStatus.Ignore;
        }

        public void UpdateEventObserver(Candidate value)
        {
            ResultStatus = MatchingEventResultStatus.UpdateEventObserver;
            ResultEventObserver = value;
        }

        public void Reject()
        {
            ResultStatus = MatchingEventResultStatus.Reject;
        }

        public void Complete()
        {
            ResultStatus = MatchingEventResultStatus.Complete;
        }
    }

    internal class TokenEvent : MatchingEvent
    {
        public Token Token { get; set; }

        public override TextLocation Location => Token.Location;

        public override string ToString()
        {
            return Token.ToString();
        }
    }

    internal class PatternEvent : MatchingEvent
    {
        public PatternCandidate Pattern { get; set; }
        public string Name => ((PatternExpression)Pattern.Expression).Name;
        public TextLocation Start { get; set; }
        public TextLocation End { get; set; }

        public override TextLocation Location => Start;

        public override string ToString()
        {
            return Name;
        }
    }
}
