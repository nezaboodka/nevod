//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;

namespace Nezaboodka.Nevod
{
    internal sealed class PatternReferenceCandidate : CompoundCandidate
    {
        public PatternReferenceCandidate(PatternReferenceExpression expression)
            : base(expression)
        {
        }

        public override CompoundCandidate Clone()
        {
            var result = (PatternReferenceCandidate)MemberwiseClone();
            result.fRootCandidate = null;
            result.fRejectionTargetCandidate = null;
            return result;
        }

        public override void OnNext(MatchingEvent matchingEvent)
        {
            if (matchingEvent is PatternEvent patternEvent)
            {
                PatternCandidate patternCandidate = patternEvent.Pattern;
                PatternReferenceExpression referenceExpression = (PatternReferenceExpression)Expression;
                if (referenceExpression.ReferencedPattern == patternCandidate.Expression)
                    CompleteMatch(matchingEvent);
            }
        }

        public override void OnElementMatch(Candidate element, MatchingEvent matchingEvent)
        {
            throw new NotImplementedException();
        }
    }
}
