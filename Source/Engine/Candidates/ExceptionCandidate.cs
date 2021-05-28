//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal sealed class ExceptionCandidate : RootCandidate
    {
        private SharedListOfExceptionTargets fExceptionTargets;

        public ExceptionCandidate(ExceptionExpression expression)
            : base(expression)
        {
        }

        public override CompoundCandidate Clone()
        {
            var result = (ExceptionCandidate)base.Clone();
            fExceptionTargets.RegisterExceptionCopy(result);
            return result;
        }

        public void AddTargetCandidate(RejectionTargetCandidate candidate)
        {
            if (fExceptionTargets == null)
                fExceptionTargets = new SharedListOfExceptionTargets();
            fExceptionTargets.Add(candidate);
        }

        public void RemoveTargetCandidate(RejectionTargetCandidate candidate)
        {
            fExceptionTargets.Remove(candidate);
            if (fExceptionTargets.Count == 0)
                Reject();
        }

        public override ExtractionCandidate GetFieldLatestValue(int fieldNumber)
        {
            ExtractionCandidate result = base.GetFieldLatestValue(fieldNumber);
            if (result == null)
            {
                RejectionTargetCandidate firstTarget = fExceptionTargets.GetFirst();
                RootCandidate root;
                if (firstTarget is RootCandidate rootTarget)
                    root = rootTarget;
                else
                    root = firstTarget.GetRootCandidate();
                result = root.GetFieldLatestValue(fieldNumber);
            }
            return result;
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

        public override void Reject()
        {
            if (!IsRejected)
            {
                if (fExceptionTargets != null)
                    fExceptionTargets.UnregisterExceptionCopy(this);
                base.Reject();
            }
        }

        // Internal

        protected override void OnFinalMatch()
        {
            if (fExceptionTargets != null)
                fExceptionTargets.RejectAll();
        }

        private void Match(MatchingEvent matchingEvent)
        {
            if (!IsCompleted)
            {
                OnCompleted();
                if (IsFinalMatch)
                    OnFinalMatch();
            }
        }
    }
}
