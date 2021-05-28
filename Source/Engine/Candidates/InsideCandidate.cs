//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Nezaboodka.Nevod
{
    internal sealed class InsideCandidate : CompoundCandidate
    {
        private List<RejectionTargetCandidate> fRelatedTargetCopies;

        public InsideCandidate(InsideExpression expression)
            : base(expression)
        {
        }

        public override void OnElementMatch(Candidate element, MatchingEvent matchingEvent)
        {
            matchingEvent.UpdateEventObserver(this);
            End = matchingEvent.Location;
            SearchContext.AddToPendingInsideCandidates(this);
            CompleteMatch(matchingEvent);
        }

        public void AddTargetCandidate(RejectionTargetCandidate target)
        {
            if (fRelatedTargetCopies == null)
                fRelatedTargetCopies = new List<RejectionTargetCandidate>();
            fRelatedTargetCopies.Add(target);
        }

        public void RemoveTargetCandidate(RejectionTargetCandidate target)
        {
            if (fRelatedTargetCopies != null)
                fRelatedTargetCopies.Remove(target);
        }

        public void OnOuterPatternMatch()
        {
            RejectionTargetCandidate target = GetRejectionTargetCandidate();
            if (!target.IsRejected)
                target.RemovePendingInsideCandidate(this);
            if (fRelatedTargetCopies != null)
            {
                for (int i = fRelatedTargetCopies.Count - 1; i >= 0; i--)
                {
                    target = fRelatedTargetCopies[i];
                    if (!target.IsRejected)
                        target.RemovePendingInsideCandidate(this);
                }
            }
        }

        public void OnOuterPatternReject()
        {
            RejectTarget();
            if (fRelatedTargetCopies != null)
            {
                for (int i = fRelatedTargetCopies.Count - 1; i >= 0; i--)
                    fRelatedTargetCopies[i].Reject();
            }
        }
    }
}
