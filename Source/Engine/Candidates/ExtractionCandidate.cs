//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;

namespace Nezaboodka.Nevod
{
    internal sealed class ExtractionCandidate : CompoundCandidate
    {
        public SequenceExpression TextExpression;   // закэшированное выражение для поиска захваченного текста

        public ExtractionCandidate(ExtractionExpression expression)
            : base(expression)
        {
        }

        public override void OnElementMatch(Candidate element, MatchingEvent matchingEvent)
        {
            matchingEvent.UpdateEventObserver(this);
            End = matchingEvent.Location;
            RootCandidate fieldContainer = GetRootCandidate();
            fieldContainer.AddExtraction(this);
            CompleteMatch(matchingEvent);
        }
    }
}
