//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Nezaboodka.Nevod
{
    internal sealed class OutsideCandidate : CompoundCandidate
    {
        private List<RejectionTargetCandidate> fRelatedTargetCopies;

        public OutsideCandidate(OutsideExpression expression)
            : base(expression)
        {
        }

        public override void OnElementMatch(Candidate element, MatchingEvent matchingEvent)
        {
            matchingEvent.UpdateEventObserver(this);
            End = matchingEvent.Location;
            SearchContext.AddToPendingOutsideCandidates(this);
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
            RejectTarget();
            if (fRelatedTargetCopies != null)
            {
                for (int i = fRelatedTargetCopies.Count - 1; i >= 0; i--)
                    fRelatedTargetCopies[i].Reject();
            }
        }

        public void OnOuterPatternReject()
        {
            RejectionTargetCandidate target = GetRejectionTargetCandidate();
            if (!target.IsRejected)
                target.RemovePendingOutsideCandidate(this);
            if (fRelatedTargetCopies != null)
            {
                for (int i = fRelatedTargetCopies.Count - 1; i >= 0; i--)
                {
                    target = fRelatedTargetCopies[i];
                    if (!target.IsRejected)
                        target.RemovePendingOutsideCandidate(this);
                }
            }
        }
    }
}
