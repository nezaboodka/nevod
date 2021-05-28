//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Nezaboodka.Nevod
{
    internal sealed class PatternCandidate : RootCandidate
    {
        public PatternCandidate(PatternExpression expression)
            : base(expression)
        {
        }

        public override void OnNext(MatchingEvent matchingEvent)
        {
            CurrentEventObserver.OnNext(matchingEvent);
            switch (matchingEvent.ResultStatus)
            {
                case MatchingEventResultStatus.Ignore:
                    End = matchingEvent.Location;
                    // Do nothing
                    break;
                case MatchingEventResultStatus.UpdateEventObserver:
                    End = matchingEvent.Location;
                    CurrentEventObserver = matchingEvent.ResultEventObserver;
                    break;
                case MatchingEventResultStatus.Reject:
                    Reject();
                    CurrentEventObserver = null;
                    break;
                case MatchingEventResultStatus.Complete:
                    End = matchingEvent.Location;
                    Match(matchingEvent);
                    CurrentEventObserver = null;
                    break;
            }
        }

        public override void OnElementMatch(Candidate element, MatchingEvent matchingEvent)
        {
            matchingEvent.Complete();
        }

        // Internal

        protected override void OnFinalMatch()
        {
            SearchContext.CreatePatternEvent(this);     // добавить кандидата в результаты
        }

        private void Match(MatchingEvent matchingEvent)
        {
            if (!IsCompleted)
            {
                End = matchingEvent.Location;
                OnCompleted();
                if (IsFinalMatch)
                    OnFinalMatch();
                else
                    SearchContext.CreatePatternEvent(this);
            }
        }
    }
}
