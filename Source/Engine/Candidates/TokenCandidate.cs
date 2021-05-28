//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;

namespace Nezaboodka.Nevod
{
    internal sealed class TokenCandidate : Candidate
    {
        public TokenCandidate(TokenExpression expression)
            : base(expression)
        {
        }

        public override void OnNext(MatchingEvent matchingEvent)
        {
            Start = matchingEvent.Location;
            End = matchingEvent.Location;
            CompleteMatch(matchingEvent);
        }
    }
}
