//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;

namespace Nezaboodka.Nevod
{
    internal sealed class VariationCandidate : CompoundCandidate
    {
        public VariationCandidate(VariationExpression expression)
            : base(expression)
        {
        }

        public override void OnElementMatch(Candidate element, MatchingEvent matchingEvent)
        {
            matchingEvent.UpdateEventObserver(this);
            End = matchingEvent.Location;
            CompleteMatch(matchingEvent);
        }
    }
}
