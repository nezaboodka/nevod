//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Nezaboodka.Text.Parsing;

namespace Nezaboodka.Nevod
{
    internal class WaitingCandidatesIndex
    {
        public WaitingTokenIndex WaitingTokenIndex { get; }

        public WaitingCandidatesIndex(SearchContext searchContext)
        {
            WaitingTokenIndex = new WaitingTokenIndex(searchContext);
        }

        public void Reset()
        {
            WaitingTokenIndex.Reset();
        }

        public void AddWaitingCandidate(AnySpanCandidate candidate)
        {
            candidate.StartWaiting();
            var expressionToWait = ((AnySpanExpression)candidate.Expression).Right;
            WaitingTokenIndex.AddWaitingTokens(expressionToWait.OwnIndex.TokenIndex, candidate,
                isException: false);
            WaitingTokenIndex.AddWaitingTokens(expressionToWait.OwnIndex.OptionalTokenIndex, candidate,
                isException: false);
            if (expressionToWait.OwnIndex.ExceptionTokenIndex != null)
            {
                candidate.CreateWaitingExceptionStub(out AnySpanCandidate exceptionSpanCandidate);
                WaitingTokenIndex.AddWaitingTokens(expressionToWait.OwnIndex.ExceptionTokenIndex,
                    exceptionSpanCandidate, isException: true);
                WaitingTokenIndex.AddWaitingTokens(expressionToWait.OwnIndex.ExceptionOptionalTokenIndex,
                    exceptionSpanCandidate, isException: true);
            }
            // TODO: Добавить в ожидание по ссылке + работа с NestedIndex
        }

        public void SelectMatchingTokenCandidates(Token token, bool[] excludeFlagPerPattern,
            List<WaitingToken> candidates, List<WaitingToken> exceptions)
        {
            WaitingTokenIndex.SelectMatchingTokenCandidates(token, excludeFlagPerPattern, candidates, exceptions);
        }

        public void RejectCandidatesWithWordLimitExceeded()
        {
            WaitingTokenIndex.RejectCandidatesWithWordLimitExceeded();
        }

        public void TryRemoveDisposedCandidates()
        {
            WaitingTokenIndex.TryRemoveDisposedCandidates();
        }

        public void RejectAll()
        {
            WaitingTokenIndex.RejectAll();
        }

        public void ForEach(Action<RootCandidate> action)
        {
            WaitingTokenIndex.ForEach(action);
        }
    }
}
