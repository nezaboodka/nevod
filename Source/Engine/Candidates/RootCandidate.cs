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
    internal abstract class RootCandidate : RejectionTargetCandidate
    {
        private List<WaitingToken> fWaitingTokens;
        private List<AnySpanCandidate> fCompletedPendingAnySpanCandidates;

        public List<ExtractionCandidate> Extractions { get; private set; }

        public int PatternId;
        public bool IsRegistered;

        public bool IsWaiting { get; private set; }

        public bool IsCompletedOrWaiting => (IsCompleted || IsWaiting);
        public bool HasPendingAnySpanCandidates => (fCompletedPendingAnySpanCandidates != null);
        public bool IsFinalMatch => (IsCompleted && !IsRejected && !MayBeRejected && !HasPendingAnySpanCandidates);

        public RootCandidate(CompoundExpression expression)
            : base(expression)
        {
        }

        public override CompoundCandidate Clone()
        {
            var result = (RootCandidate)base.Clone();
            result.IsRegistered = false;
            if (Extractions != null)
                Extractions = new List<ExtractionCandidate>(Extractions);
            if (fCompletedPendingAnySpanCandidates != null)
            {
                fCompletedPendingAnySpanCandidates = new List<AnySpanCandidate>(fCompletedPendingAnySpanCandidates);
                for (int i = 0, n = fCompletedPendingAnySpanCandidates.Count; i < n; i++)
                    fCompletedPendingAnySpanCandidates[i].AddRelatedRootCandidate(result);
            }
            return result;
        }

        public void StartWaiting()
        {
            fWaitingTokens = new List<WaitingToken>();
            IsWaiting = true;
        }

        public void AddWaitingToken(WaitingToken waitingToken)
        {
            fWaitingTokens.Add(waitingToken);
        }

        public void FinishWaiting()
        {
            IsWaiting = false;
            fWaitingTokens = null;
        }

        public override void RemoveException(ExceptionCandidate candidate)
        {
            base.RemoveException(candidate);
            if (IsFinalMatch)
                OnFinalMatch();
        }

        public override void RemovePendingHavingCandidate(HavingCandidate candidate)
        {
            base.RemovePendingHavingCandidate(candidate);
            if (IsFinalMatch)
                OnFinalMatch();
        }

        public override void RemovePendingInsideCandidate(InsideCandidate candidate)
        {
            base.RemovePendingInsideCandidate(candidate);
            if (IsFinalMatch)
                OnFinalMatch();
        }

        public override void RemovePendingOutsideCandidate(OutsideCandidate candidate)
        {
            base.RemovePendingOutsideCandidate(candidate);
            if (IsFinalMatch)
                OnFinalMatch();
        }

        public void AddCompletedPendingAnySpanCandidate(AnySpanCandidate candidate)
        {
            if (fCompletedPendingAnySpanCandidates == null)
                fCompletedPendingAnySpanCandidates = new List<AnySpanCandidate>();
            fCompletedPendingAnySpanCandidates.Add(candidate);
            candidate.AddRelatedRootCandidate(this);
        }

        public void RemoveCompletedPendingAnySpanCandidate(AnySpanCandidate candidate)
        {
            fCompletedPendingAnySpanCandidates.Remove(candidate);
            if (fCompletedPendingAnySpanCandidates.Count == 0)
                fCompletedPendingAnySpanCandidates = null;
            if (IsFinalMatch)
                OnFinalMatch();
        }

        public void AddExtraction(ExtractionCandidate candidate)
        {
            if (Extractions == null)
                Extractions = new List<ExtractionCandidate>();
            Extractions.Add(candidate);
        }

        public virtual ExtractionCandidate GetFieldLatestValue(int fieldNumber)
        {
            bool found = false;
            ExtractionCandidate value = null;
            if (Extractions != null)
            {
                int i = Extractions.Count - 1;
                while (!found && i >= 0)
                {
                    value = Extractions[i];
                    var extractionExpression = (ExtractionExpression)value.Expression;
                    found = (extractionExpression.FieldNumber == fieldNumber);
                    i--;
                }
            }
            ExtractionCandidate result = found ? value : null;
            return result;
        }

        public override void Reject()
        {
            if (!IsRejected)
            {
                base.Reject();
                DisposeWaitingTokens();
                RemoveCompletedPendingAnySpanCandidates();
                // Отмена цепочки кандидатов начинается с поиска кандидата, который сейчас совпадает.
                // Вызов OnCompleted производится на подъёме от совпадающего кандидата к корню.
                Candidate current = this;
                while (!current.IsCompleted && current.CurrentEventObserver != null)
                    current = current.CurrentEventObserver;
                while (current != null)
                {
                    current.OnCompleted();
                    if (current is AnySpanCandidate currentAnySpan)
                    {
                        if (currentAnySpan.IsPending)
                            currentAnySpan.RemoveFromPendingCandidates();
                    }
                    if (current.ParentCandidate != null)
                        current = current.ParentCandidate;
                    else
                        current = current.TargetParentCandidate;
                }
            }
        }

        public sealed override void OnCompleted()
        {
            if (!IsCompleted)
            {
                SearchContext.CandidateFactory.UnregisterRootCandidate(this);
                SearchContext.Telemetry.TrackEnd(this);
                base.OnCompleted();
            }
        }

        // Internal

        protected abstract void OnFinalMatch();

        private void DisposeWaitingTokens()
        {
            if (fWaitingTokens != null)
            {
                fWaitingTokens.ForEach(x => x.Dispose());
                fWaitingTokens = null;
            }
        }

        private void RemoveCompletedPendingAnySpanCandidates()
        {
            if (fCompletedPendingAnySpanCandidates != null)
            {
                fCompletedPendingAnySpanCandidates.ForEach(x => x.RemoveRelatedRootCandidate(this));
                fCompletedPendingAnySpanCandidates = null;
            }
        }
    }
}
