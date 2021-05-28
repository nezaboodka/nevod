//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Nezaboodka.Nevod
{
    internal class AnySpanCandidate : RejectionTargetCandidate
    {
        private TextLocation fEndOfLeft;    // конец левой части промежутка - начало пропуска
        private TextLocation fStartOfRight; // начало правой части промежутка - конец пропуска
        private List<AnySpanCandidate> fPendingCandidates;
        private ExceptionStubCandidate fRelatedExceptionStub;
        private AnySpanCandidate fMasterCandidate; // исходный кандидат, который находится в ожидании и
                                                   // пропускает лексемы для более длинного совпадения
        private bool fIsBeingRemoved;
        private bool fIsMarkedToCheckFinalMatchOnRemoval;
        private List<RootCandidate> fRelatedRootCandidates;

        public int SavedWordCount;  // количество слов, обработанных на момент совпадения левой части;
                                    // необходимо для определения количества пропущенных слов

        public bool IsPending => (fMasterCandidate != null);

        public AnySpanCandidate(AnySpanExpression expression)
            : base(expression)
        {
        }

        public override CompoundCandidate Clone()
        {
            var result = (AnySpanCandidate)MemberwiseClone();
            result.fRootCandidate = null;
            result.fRejectionTargetCandidate = null;
            if (fMasterCandidate != null)
                fMasterCandidate.AddPendingCandidate(result);
            return result;
        }

        public override CompoundCandidate CloneStateToContinueMatching(TokenExpression tokenExpression,
            TokenEvent tokenEvent, out RootCandidate rootCopy, in RootCandidate replacingRoot = null)
        {
            AnySpanCandidate thisCopy = (AnySpanCandidate)base.CloneStateToContinueMatching(tokenExpression, tokenEvent,
                out rootCopy, in replacingRoot);
            thisCopy.fPendingCandidates = null;
            thisCopy.fRelatedExceptionStub = null;
            thisCopy.fStartOfRight = tokenEvent.Location;
            thisCopy.fMasterCandidate = this;
            AddPendingCandidate(thisCopy);
            return thisCopy;
        }

        public void AddRelatedRootCandidate(RootCandidate root)
        {
            if (fRelatedRootCandidates == null)
                fRelatedRootCandidates = new List<RootCandidate>();
            fRelatedRootCandidates.Add(root);
        }

        public void RemoveRelatedRootCandidate(RootCandidate root)
        {
            fRelatedRootCandidates.Remove(root);
            if (fRelatedRootCandidates.Count == 0)
            {
                fRelatedRootCandidates = null;
                RemoveFromPendingCandidates();
            }
        }

        public void StartWaiting()
        {
            GetRootCandidate().StartWaiting();
        }

        public ExceptionStubCandidate CreateWaitingExceptionStub(out AnySpanCandidate exceptionSpan)
        {
            if (fRelatedExceptionStub != null)
                throw new InvalidOperationException($"{nameof(fRelatedExceptionStub)} is already created.");
            RootCandidate root = GetRootCandidate();
            fRelatedExceptionStub = SearchContext.CandidateFactory.CreateExceptionStubCandidate(root);
            fRelatedExceptionStub.StartWaiting();
            exceptionSpan = (AnySpanCandidate)Expression.CreateCandidate(SearchContext, fRelatedExceptionStub);
            return fRelatedExceptionStub;
        }

        public void AddPendingCandidate(AnySpanCandidate candidate)
        {
            if (fPendingCandidates == null)
                fPendingCandidates = new List<AnySpanCandidate>();
            fPendingCandidates.Add(candidate);
        }

        public void RemovePendingCandidate(AnySpanCandidate candidate)
        {
            AnySpanCandidate candidateToFinalMatch = RemovePendingCandidateAndGetCandidateToFinalMatch(candidate);
            if (candidateToFinalMatch != null)
                candidateToFinalMatch.TryFinalMatch();
        }

        public void RemoveFromPendingCandidates()
        {
            if (!fIsBeingRemoved)
                fMasterCandidate.RemovePendingCandidate(this);
            fMasterCandidate = null; // IsPending = false
        }

        public override void RemoveException(ExceptionCandidate candidate)
        {
            base.RemoveException(candidate);
            if (IsCompleted && IsPending && !MayBeRejected)
                TryFinalMatch();
        }

        public override void RemovePendingHavingCandidate(HavingCandidate candidate)
        {
            base.RemovePendingHavingCandidate(candidate);
            if (IsCompleted && IsPending && !MayBeRejected)
                TryFinalMatch();
        }

        public override void RemovePendingInsideCandidate(InsideCandidate candidate)
        {
            base.RemovePendingInsideCandidate(candidate);
            if (IsCompleted && IsPending && !MayBeRejected)
                TryFinalMatch();
        }

        public override void RemovePendingOutsideCandidate(OutsideCandidate candidate)
        {
            base.RemovePendingOutsideCandidate(candidate);
            if (IsCompleted && IsPending && !MayBeRejected)
                TryFinalMatch();
        }

        public override void OnElementMatch(Candidate element, MatchingEvent matchingEvent)
        {
            matchingEvent.UpdateEventObserver(this);
            End = matchingEvent.Location;
            Expression elementExpression = element.Expression;
            int positionInParent = elementExpression.PositionInParentExpression;
            switch (positionInParent)
            {
                case AnySpanExpression.LeftPosition:
                    SaveEndOfLeft(left: element);
                    bool success = SearchContext.TryAddToWaitingCandidates(this);
                    if (!success)
                        Reject();
                    else if (TargetParentCandidate is PatternCandidate targetPattern && !MayBeRejected
                        && !targetPattern.MayBeRejected)
                    {
                        // TODO: check expression.SpanRangeInWords when translated from A .. [n-m] .. B
                        // ***
                        // TODO: reconsider AnySpan inside context expressions + references
                        bool mayBeRejectedInFuture = false;
                        var parentExpression = Expression.ParentExpression;
                        while (parentExpression != null && !mayBeRejectedInFuture)
                        {
                            mayBeRejectedInFuture =  parentExpression is InsideExpression
                                || parentExpression is OutsideExpression;
                            parentExpression = parentExpression.ParentExpression;
                        }
                        if (!mayBeRejectedInFuture)
                            SearchContext.DisableAnySpanPattern(targetPattern.PatternId);
                    }
                    break;
                case AnySpanExpression.RightPosition:
                    SaveExtractionOfSpanIfSpecified();
                    TryFinalMatchOrLeavePending();
                    if (TargetParentCandidate is PatternCandidate targetPattern1)
                        SearchContext.EnableAnySpanPattern(targetPattern1.PatternId);
                    CompleteMatch(matchingEvent);
                    break;
                default:
                    throw new IndexOutOfRangeException($"Invalid position in parent.");
            }
        }

        public override void Reject()
        {
            if (!IsRejected)
            {
                base.Reject();
                RejectRoot();
            }
        }

        // Internal

        protected void SaveEndOfLeft(Candidate left)
        {
            fEndOfLeft = left.End;
        }

        protected void SaveExtractionOfSpanIfSpecified()
        {
            Expression extractionOfSpanExpression = ((AnySpanExpression)Expression).ExtractionOfSpan;
            if (extractionOfSpanExpression != null)
            {
                long spanStartPosition = fEndOfLeft.Position + fEndOfLeft.Length;
                long spanEndPosition = fStartOfRight.Position;
                long spanLength = spanEndPosition - spanStartPosition;
                if (spanLength > 0)
                {
                    var extractionOfSpan = (ExtractionCandidate)extractionOfSpanExpression.CreateCandidate(
                        SearchContext, targetParentCandidate: null);
                    long spanStartTokenNumber = ~(fEndOfLeft.TokenNumber + 1);
                    long spanEndTokenNumber = ~(fStartOfRight.TokenNumber - 1);
                    extractionOfSpan.Start = new TextLocation(spanStartTokenNumber, spanStartPosition, spanLength);
                    extractionOfSpan.End = new TextLocation(spanEndTokenNumber, spanEndPosition, length: 0);
                    RootCandidate fieldContainer = GetRootCandidate();
                    fieldContainer.AddExtraction(extractionOfSpan);
                    extractionOfSpan.OnCompleted();
                }
            }
        }

        protected void TryFinalMatchOrLeavePending()
        {
            if (IsPending)
            {
                bool isFinalMatch = false;
                if (!MayBeRejected)
                    isFinalMatch = TryFinalMatch();
                if (!isFinalMatch)
                {
                    RootCandidate root = GetRootCandidate();
                    root.AddCompletedPendingAnySpanCandidate(this);
                }
            }
        }

        private bool TryFinalMatch()
        {
            long matchedSpanTokenNumber = fStartOfRight.TokenNumber;
            bool isFinalMatch = true;
            List<AnySpanCandidate> pendingCandidates = fMasterCandidate.fPendingCandidates;
            int count = pendingCandidates.Count;
            int j = 0;
            for (int i = 0; i < count; i++)
            {
                AnySpanCandidate candidate = pendingCandidates[i];
                long spanTokenNumber = candidate.fStartOfRight.TokenNumber;
                if (spanTokenNumber > matchedSpanTokenNumber)
                {
                    // Отменить более длинных ожидающих кандидатов.
                    candidate.fIsBeingRemoved = true;
                    candidate.Reject();
                }
                else
                {
                    if (i != j)
                        pendingCandidates[j] = candidate;
                    j++;
                    // FinalMatch, только если у совпавшего кандидата промежуток наименьшей длины.
                    if (spanTokenNumber < matchedSpanTokenNumber)
                        isFinalMatch = false;
                }
            }
            pendingCandidates.RemoveRange(j, count - j);
            if (isFinalMatch)
                fMasterCandidate.MatchPendingCandidatesAndReject();
            else
                fIsMarkedToCheckFinalMatchOnRemoval = true;
            return isFinalMatch;
        }

        private AnySpanCandidate RemovePendingCandidateAndGetCandidateToFinalMatch(AnySpanCandidate candidateToRemove)
        {
            long minSpanTokenNumber = long.MaxValue;
            long markedToCheckMinSpanTokenNumber = long.MaxValue;
            AnySpanCandidate markedToCheckCandidate = null;
            // Кандидат изымается из списка,
            // поэтому обход производится с конца.
            for (int i = fPendingCandidates.Count - 1; i >= 0; i--)
            {
                AnySpanCandidate candidate = fPendingCandidates[i];
                if (candidate == candidateToRemove)
                    fPendingCandidates.RemoveAt(i);
                else
                {
                    long spanTokenNumber = candidate.fStartOfRight.TokenNumber;
                    if (spanTokenNumber < minSpanTokenNumber)
                        minSpanTokenNumber = spanTokenNumber;
                    if (candidate.fIsMarkedToCheckFinalMatchOnRemoval)
                    {
                        if (spanTokenNumber < markedToCheckMinSpanTokenNumber)
                        {
                            markedToCheckMinSpanTokenNumber = spanTokenNumber;
                            markedToCheckCandidate = candidate;
                        }
                    }
                }
            }
            AnySpanCandidate candidateToFinalMatch;
            if (markedToCheckCandidate != null && markedToCheckMinSpanTokenNumber == minSpanTokenNumber)
                candidateToFinalMatch = markedToCheckCandidate;
            else
                candidateToFinalMatch = null;
            return candidateToFinalMatch;
        }

        private void MatchPendingCandidatesAndReject()
        {
            for (int i = 0, n = fPendingCandidates.Count; i < n; i++)
            {
                AnySpanCandidate candidate = fPendingCandidates[i];
                if (candidate.IsCompleted)
                {
                    foreach (RootCandidate relatedRootCandidate in candidate.fRelatedRootCandidates)
                    {
                        if (!relatedRootCandidate.IsRejected)
                            relatedRootCandidate.RemoveCompletedPendingAnySpanCandidate(candidate);
                    }
                }
                candidate.fMasterCandidate = null; // IsPending = false
            }
            if (fRelatedExceptionStub != null)
                fRelatedExceptionStub.Reject();
            Reject();
        }
    }
}
