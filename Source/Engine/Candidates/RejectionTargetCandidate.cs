//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal abstract class RejectionTargetCandidate : CompoundCandidate
    {
        private List<ExceptionCandidate> fRelatedExceptions;
        private List<HavingCandidate> fPendingHavingCandidates;
        private List<InsideCandidate> fPendingInsideCandidates;
        private List<OutsideCandidate> fPendingOutsideCandidates;

        public bool IsRejected { get; private set; }

        public bool HasExceptions => (fRelatedExceptions != null);
        public bool HasPendingHavingCandidates => (fPendingHavingCandidates != null);
        public bool HasPendingInsideCandidates => (fPendingInsideCandidates != null);
        public bool HasPendingOutsideCandidates => (fPendingOutsideCandidates != null);
        public bool MayBeRejected => (HasExceptions || HasPendingHavingCandidates || HasPendingInsideCandidates
            || HasPendingOutsideCandidates);

        public RejectionTargetCandidate(CompoundExpression expression)
            : base(expression)
        {
        }

        public override CompoundCandidate Clone()
        {
            var result = (RejectionTargetCandidate)MemberwiseClone();
            result.fRootCandidate = null;
            result.fRejectionTargetCandidate = null;
            if (fRelatedExceptions != null)
            {
                fRelatedExceptions = new List<ExceptionCandidate>(fRelatedExceptions);
                fRelatedExceptions.ForEach(x => x.AddTargetCandidate(result));
            }
            if (fPendingHavingCandidates != null)
            {
                fPendingHavingCandidates = new List<HavingCandidate>(fPendingHavingCandidates);
                fPendingHavingCandidates.ForEach(x => x.AddTargetCandidate(result));
            }
            if (fPendingInsideCandidates != null)
            {
                fPendingInsideCandidates = new List<InsideCandidate>(fPendingInsideCandidates);
                fPendingInsideCandidates.ForEach(x => x.AddTargetCandidate(result));
            }
            if (fPendingOutsideCandidates != null)
            {
                fPendingOutsideCandidates = new List<OutsideCandidate>(fPendingOutsideCandidates);
                fPendingOutsideCandidates.ForEach(x => x.AddTargetCandidate(result));
            }
            return result;
        }

        public void AddException(ExceptionCandidate exceptionCandidate)
        {
            if (fRelatedExceptions == null)
                fRelatedExceptions = new List<ExceptionCandidate>();
            fRelatedExceptions.Add(exceptionCandidate);
        }

        public virtual void RemoveException(ExceptionCandidate candidate)
        {
            fRelatedExceptions.Remove(candidate);
            if (fRelatedExceptions.Count == 0)
                fRelatedExceptions = null;
        }

        public void AddPendingHavingCandidate(HavingCandidate candidate)
        {
            if (fPendingHavingCandidates == null)
                fPendingHavingCandidates = new List<HavingCandidate>();
            fPendingHavingCandidates.Add(candidate);
        }

        public virtual void RemovePendingHavingCandidate(HavingCandidate candidate)
        {
            fPendingHavingCandidates.Remove(candidate);
            if (fPendingHavingCandidates.Count == 0)
                fPendingHavingCandidates = null;
        }

        public void AddPendingInsideCandidate(InsideCandidate candidate)
        {
            if (fPendingInsideCandidates == null)
                fPendingInsideCandidates = new List<InsideCandidate>();
            fPendingInsideCandidates.Add(candidate);
        }

        public virtual void RemovePendingInsideCandidate(InsideCandidate candidate)
        {
            fPendingInsideCandidates.Remove(candidate);
            if (fPendingInsideCandidates.Count == 0)
                fPendingInsideCandidates = null;
        }

        public void AddPendingOutsideCandidate(OutsideCandidate candidate)
        {
            if (fPendingOutsideCandidates == null)
                fPendingOutsideCandidates = new List<OutsideCandidate>();
            fPendingOutsideCandidates.Add(candidate);
        }

        public virtual void RemovePendingOutsideCandidate(OutsideCandidate candidate)
        {
            fPendingOutsideCandidates.Remove(candidate);
            if (fPendingOutsideCandidates.Count == 0)
                fPendingOutsideCandidates = null;
        }

        public virtual void Reject()
        {
            if (!IsRejected)
            {
                RemoveFromRelatedExceptions();
                RemoveFromRelatedPendingCandidates();
                IsRejected = true;
            }
        }

        // Internal

        private void RemoveFromRelatedExceptions()
        {
            if (fRelatedExceptions != null)
            {
                fRelatedExceptions.ForEach(x => x.RemoveTargetCandidate(this));
                fRelatedExceptions = null;
            }
        }

        private void RemoveFromRelatedPendingCandidates()
        {
            if (fPendingHavingCandidates != null)
            {
                fPendingHavingCandidates.ForEach(x => x.RemoveTargetCandidate(this));
                fPendingHavingCandidates = null;
            }
            if (fPendingInsideCandidates != null)
            {
                fPendingInsideCandidates.ForEach(x => x.RemoveTargetCandidate(this));
                fPendingInsideCandidates = null;
            }
            if (fPendingOutsideCandidates != null)
            {
                fPendingOutsideCandidates.ForEach(x => x.RemoveTargetCandidate(this));
                fPendingOutsideCandidates = null;
            }
        }
    }
}
