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
    internal class WaitingIndexHandler
    {
        private List<WaitingToken> fWaitingTokensForRootCandidates;
        private List<WaitingToken> fWaitingTokensForExceptionCandidates;

        private List<RootCandidate> fRejectableRootCandidates;
        private Dictionary<int, List<RootCandidate>> fRejectableRootCandidatesByVariation;
        private List<ExceptionCandidate> fExceptionCandidates;

        public SearchContext SearchContext { get; }

        public WaitingIndexHandler(SearchContext searchContext)
        {
            SearchContext = searchContext;

            fWaitingTokensForRootCandidates = new List<WaitingToken>();
            fWaitingTokensForExceptionCandidates = new List<WaitingToken>();

            fRejectableRootCandidates = new List<RootCandidate>();
            fRejectableRootCandidatesByVariation = new Dictionary<int, List<RootCandidate>>();
            fExceptionCandidates = new List<ExceptionCandidate>();
        }

        // Связывание альтернатив и исключений осуществляется через вариации.
        // Во вложенных вариациях исключения верхнего уровня влияют на альтернативы нижнего уровня.
        // Для организации связи между альтернативами нижнего уровня и исключениями верхнего уровня
        // задействуются все вложенные вариации от токена до корня.
        public void ResumeCandidatesWaitingForToken(WaitingCandidatesIndex waitingCandidatesIndex,
            TokenEvent tokenEvent, bool[] excludeFlagPerPattern)
        {
            waitingCandidatesIndex.SelectMatchingTokenCandidates(tokenEvent.Token, excludeFlagPerPattern,
                fWaitingTokensForRootCandidates, fWaitingTokensForExceptionCandidates);
            if (fWaitingTokensForRootCandidates.Count > 0)
            {
                if (fWaitingTokensForExceptionCandidates.Count > 0)
                {
                    foreach (var waitingToken in fWaitingTokensForRootCandidates)
                    {
                        RootCandidate rootCandidate = ResumeWaitingCandidate(waitingToken, tokenEvent);
                        VariationExpression variation = waitingToken.TokenExpression.ParentVariationWithExceptions;
                        if (variation != null)
                        {
                            fRejectableRootCandidates.Add(rootCandidate);
                            // Отложить корневой кандидат для связывания с исключениями
                            // Перебрать все вариации с исключениями в текущем корневом кандидате,
                            // в которые вложен токен
                            do
                            {
                                fRejectableRootCandidatesByVariation.GetOrCreate(variation.VariationWithExceptionsId)
                                    .Add(rootCandidate);
                                variation = variation.ParentVariationWithExceptions;
                            }
                            while (variation != null);
                        }
                        else
                        {
                            ContinueCandidateMatching(tokenEvent, rootCandidate);
                        }
                    }
                    if (fRejectableRootCandidatesByVariation.Count > 0)
                    {
                        FilterAndCreateExceptions(tokenEvent);
                        if (fExceptionCandidates.Count > 0)
                        {
                            LinkExceptionsWithRootCandidates();
                            ContinueCandidatesMatching(tokenEvent, fExceptionCandidates);
                            fExceptionCandidates.Clear();
                        }
                        ContinueCandidatesMatching(tokenEvent, fRejectableRootCandidates);
                        fRejectableRootCandidates.Clear();
                        fRejectableRootCandidatesByVariation.Clear();
                    }
                    fWaitingTokensForExceptionCandidates.Clear();
                }
                else // нет исключений (fWaitingTokensForNewExceptionCandidates.Count == 0)
                {
                    foreach (var waitingToken in fWaitingTokensForRootCandidates)
                    {
                        RootCandidate resumedCandidate = ResumeWaitingCandidate(waitingToken, tokenEvent);
                        ContinueCandidateMatching(tokenEvent, resumedCandidate);
                    }
                }
                fWaitingTokensForRootCandidates.Clear();
            }
        }

        // Internal

        // Создать кандидаты для исключений, которые соответствуют выбранным ранее положительным альтернативам вариаций,
        // чтобы избежать создания лишних кандидатов исключений.
        // Исключения, соответствующие другим исключениям, создаются только в том случае, если
        // другие исключения были выбраны ранее.
        private void FilterAndCreateExceptions(TokenEvent tokenEvent)
        {
            List<WaitingToken> exceptionTokensToFilter = fWaitingTokensForExceptionCandidates;
            int remainedCount = exceptionTokensToFilter.Count;
            bool done = false;
            while (!done)
            {
                done = true;
                int i = 0;
                int j = 0;
                while (i < remainedCount)
                {
                    WaitingToken waitingExceptionCandidate = exceptionTokensToFilter[i];
                    TokenExpression exceptionTokenExpression = waitingExceptionCandidate.TokenExpression;
                    var exceptionVariation = (VariationExpression)exceptionTokenExpression.RootExpression.ParentExpression;
                    int id = exceptionVariation.VariationWithExceptionsId;
                    if (fRejectableRootCandidatesByVariation.ContainsKey(id))
                    {
                        ExceptionCandidate exceptionCandidate = ResumeWaitingExceptionCandidate(
                            waitingExceptionCandidate, tokenEvent);
                        fExceptionCandidates.Add(exceptionCandidate);
                        VariationExpression variation = exceptionTokenExpression.ParentVariationWithExceptions;
                        // Перебрать все вариации с исключениями в текущем корневом кандидате,
                        // в которые вложен токен
                        while (variation != exceptionVariation)
                        {
                            if (!fRejectableRootCandidatesByVariation.ContainsKey(variation.VariationWithExceptionsId))
                                done = false;
                            fRejectableRootCandidatesByVariation.GetOrCreate(variation.VariationWithExceptionsId)
                                .Add(exceptionCandidate);
                            variation = variation.ParentVariationWithExceptions;
                        }
                    }
                    else
                    {
                        if (j != i)
                        {
                            exceptionTokensToFilter[j] = exceptionTokensToFilter[i];
                            exceptionTokensToFilter[i] = null;
                        }
                        j++;
                    }
                    i++;
                }
                if (j > 0 && j < remainedCount)
                    remainedCount = j;
                else
                    done = true;
            }
        }

        private void LinkExceptionsWithRootCandidates()
        {
            for (int i = 0, n = fExceptionCandidates.Count; i < n; i++)
            {
                ExceptionCandidate exceptionCandidate = fExceptionCandidates[i];
                var exceptionVariation = (VariationExpression)exceptionCandidate.Expression.ParentExpression;
                int id = exceptionVariation.VariationWithExceptionsId;
                if (fRejectableRootCandidatesByVariation.TryGetValue(id, out List<RootCandidate> relatedCandidates))
                {
                    bool linked = false;
                    for (int j = 0, m = relatedCandidates.Count; j < m; j++)
                    {
                        RootCandidate candidate = relatedCandidates[j];
                        if (candidate.Start.TokenNumber == exceptionCandidate.Start.TokenNumber
                            && candidate != exceptionCandidate)
                        {
                            RejectionTargetCandidate rejectionTarget =
                                candidate.CurrentEventObserver.GetRejectionTargetCandidate();
                            rejectionTarget.AddException(exceptionCandidate);
                            exceptionCandidate.AddTargetCandidate(rejectionTarget);
                            linked = true;
                        }
                    }
                    if (!linked)
                        exceptionCandidate.Reject();
                }
                else
                {
                    exceptionCandidate.Reject();
                }
            }
        }

        private void ContinueCandidatesMatching<T>(TokenEvent tokenEvent, IList<T> candidates) where T : RootCandidate
        {
            for (int i = 0, n = candidates.Count; i < n; i++)
            {
                T rootCandidate = candidates[i];
                if (!rootCandidate.IsCompleted)     // кандидат не отменен совпавшим исключением
                    ContinueCandidateMatching(tokenEvent, rootCandidate);
            }
        }

        private void ContinueCandidateMatching(TokenEvent tokenEvent, RootCandidate rootCandidate)
        {
            var tokenExpression = (TokenExpression)rootCandidate.CurrentEventObserver.Expression;
            HavingExpression conditionalHavingExpression = tokenExpression.ParentConditionalHaving;
            rootCandidate.OnNext(tokenEvent);
            if (!rootCandidate.IsCompletedOrWaiting)
            {
                bool success = SearchContext.TryAddToActiveCandidates(rootCandidate);
                if (!success)
                {
                    rootCandidate.Reject();
                    conditionalHavingExpression = null;
                }
            }
            while (conditionalHavingExpression != null)
            {
                int patternId = conditionalHavingExpression.InnerContent.ReferencedPattern.Id;
                SearchContext.EnableConditionalHavingPattern(patternId, isRootIndex: false);
                conditionalHavingExpression = conditionalHavingExpression.ParentConditionalHaving;
            }
        }

        private RootCandidate ResumeWaitingCandidate(WaitingToken resumingCandidate, TokenEvent tokenEvent)
        {
            resumingCandidate.Candidate.CloneStateToContinueMatching(resumingCandidate.TokenExpression, tokenEvent,
                out RootCandidate rootCandidateCopy);
            rootCandidateCopy.FinishWaiting();
            return rootCandidateCopy;
        }

        private ExceptionCandidate ResumeWaitingExceptionCandidate(WaitingToken resumingCandidate,
            TokenEvent tokenEvent)
        {
            var exceptionExpression = (ExceptionExpression)resumingCandidate.TokenExpression.RootExpression;
            var exceptionCandidate = (ExceptionCandidate)exceptionExpression.CreateCandidate(SearchContext, null);
            RootCandidate root = resumingCandidate.Candidate.GetRootCandidate();
            exceptionCandidate.PatternId = root.PatternId;
            // Установить позицию начала кандидату исключения для связывания с соответствующими отменяемыми кандидатами
            exceptionCandidate.Start = root.Start;
            resumingCandidate.Candidate.CloneStateToContinueMatching(resumingCandidate.TokenExpression, tokenEvent,
                out RootCandidate _, replacingRoot: exceptionCandidate);
            exceptionCandidate.CurrentEventObserver.TargetParentCandidate = exceptionCandidate;
            return exceptionCandidate;
        }
    }
}
