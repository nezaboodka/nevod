//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal sealed class ConjunctionCandidate : CompoundCandidate
    {
        public bool[] MatchPerPosition;
        public int MatchedElementCount;
        public int MatchedNonOptionalElementCount;

        public ConjunctionCandidate(ConjunctionExpression expression)
            : base(expression)
        {
            MatchPerPosition = new bool[expression.Elements.Length];
        }

        public override CompoundCandidate Clone()
        {
            var result = (ConjunctionCandidate)MemberwiseClone();
            result.MatchPerPosition = (bool[])MatchPerPosition.Clone();
            result.fRootCandidate = null;
            result.fRejectionTargetCandidate = null;
            return result;
        }

        public override void OnNext(MatchingEvent matchingEvent)
        {
            if (CurrentEventObserver != null)
            {
                CurrentEventObserver.OnNext(matchingEvent);
            }
            else
            {
                ConjunctionExpression conjunctionExpression = (ConjunctionExpression)Expression;
                if (matchingEvent is TokenEvent tokenEvent)
                    OnNextToken(tokenEvent, conjunctionExpression.Elements, excludeFlagPerPosition: MatchPerPosition,
                        includeOptional: true, alwaysCloneCandidateToContinueMatching: false);
                // TODO: process references
            }
            if (!IsCompleted)
            {
                switch (matchingEvent.ResultStatus)
                {
                    case MatchingEventResultStatus.Ignore:
                        // Do nothing
                        break;
                    case MatchingEventResultStatus.UpdateEventObserver:
                        matchingEvent.Ignore();
                        if (matchingEvent.ResultEventObserver != this)
                            CurrentEventObserver = matchingEvent.ResultEventObserver;
                        else
                            CurrentEventObserver = null;
                        break;
                    case MatchingEventResultStatus.Reject:
                        matchingEvent.Ignore();
                        CurrentEventObserver = null;
                        break;
                    case MatchingEventResultStatus.Complete:
                        // Do nothing
                        break;
                }
            }
        }

        public override void OnElementMatch(Candidate element, MatchingEvent matchingEvent)
        {
            matchingEvent.UpdateEventObserver(this);
            End = matchingEvent.Location;
            CurrentEventObserver = null;
            Expression elementExpression = element.Expression;
            int elementPosition = elementExpression.PositionInParentExpression;
            MatchPerPosition[elementPosition] = true;
            MatchedElementCount++;
            if (!elementExpression.IsOptional)
                MatchedNonOptionalElementCount++;
            ConjunctionExpression conjunctionExpression = (ConjunctionExpression)Expression;
            if (MatchedNonOptionalElementCount == conjunctionExpression.NonOptionalElementCount)
            {
                if (MatchedElementCount < conjunctionExpression.Elements.Length)
                {
                    this.CloneState(out RootCandidate rootCopy);
                    bool success = SearchContext.TryAddToActiveCandidates(rootCopy);
                    if (!success)
                        rootCopy.Reject();
                }
                if (MatchedElementCount > 1)
                    matchingEvent.InnerRepetitionCandidate = null;
                CompleteMatch(matchingEvent);
            }
        }
    }
}
