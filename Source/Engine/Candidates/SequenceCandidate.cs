//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal sealed class SequenceCandidate : CompoundCandidate
    {
        public int MatchedPositions;
        public int MatchedElementCount;

        public SequenceCandidate(SequenceExpression expression)
            : base(expression)
        {
        }

        public override CompoundCandidate Clone()
        {
            var result = (SequenceCandidate)MemberwiseClone();
            result.fRootCandidate = null;
            result.fRejectionTargetCandidate = null;
            return result;
        }

        public override void OnNext(MatchingEvent matchingEvent)
        {
            SequenceExpression sequenceExpression = (SequenceExpression)Expression;
            Expression nextElementExpression = sequenceExpression.Elements[MatchedPositions];
            if (matchingEvent is TokenEvent tokenEvent)
            {
                bool processed = OnNextToken(tokenEvent, nextElementExpression, includeOptional: true,
                    alwaysCloneCandidateToContinueMatching: false);
                if (!processed)
                    matchingEvent.Reject();
            }
        }

        public override void OnElementMatch(Candidate element, MatchingEvent matchingEvent)
        {
            matchingEvent.UpdateEventObserver(this);
            End = matchingEvent.Location;
            MatchedElementCount++;
            Expression elementExpression = element.Expression;
            int elementPosition = elementExpression.PositionInParentExpression;
            MatchedPositions = elementPosition + 1;
            SequenceExpression sequenceExpression = (SequenceExpression)Expression;
            if (MatchedPositions > sequenceExpression.LastNonOptionalPosition)
            {
                if (MatchedPositions < sequenceExpression.Elements.Length)
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
