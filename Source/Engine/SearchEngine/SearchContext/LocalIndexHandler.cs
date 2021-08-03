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
    internal class LocalIndexHandler
    {
        // Чтобы правильно клонировать исходного кандидата, первое выражение должно обрабатываться последним!
        private TokenExpression fFirst;
        private List<TokenExpression> fExceptionTokensToFilter;

        private List<RootCandidate> fRejectableRootCopies;
        private Dictionary<int, List<RootCandidate>> fRejectableRootCandidatesByVariation;
        private List<ExceptionCandidate> fExceptionCandidates;

        public SearchContext SearchContext { get; }

        public LocalIndexHandler(SearchContext searchContext)
        {
            SearchContext = searchContext;

            fRejectableRootCopies = new List<RootCandidate>();
            fExceptionTokensToFilter = new List<TokenExpression>();

            fRejectableRootCandidatesByVariation = new Dictionary<int, List<RootCandidate>>();
            fExceptionCandidates = new List<ExceptionCandidate>();
        }

        public bool CandidateOnNextToken(CompoundCandidate candidate, TokenEvent tokenEvent,
            Expression[] nextExpressions, bool[] excludeFlagPerPosition, bool includeOptional,
            bool alwaysCloneCandidateToContinueMatching)
        {
            // Чтобы правильно клонировать исходного кандидата, первое выражение должно обрабатываться последним!
            fFirst = null;
            for (int i = 0, n = nextExpressions.Length; i < n; i++)
            {
                if (!excludeFlagPerPosition[i])
                {
                    Expression nextExpression = nextExpressions[i];
                    HandleMatchingTokenExpressionsAndKeepFirst(candidate, tokenEvent, nextExpression, includeOptional,
                        alwaysCloneCandidateToContinueMatching);
                }
            }
            if (fFirst != null)
            {
                if (fRejectableRootCandidatesByVariation.Count > 0)
                {
                    for (int i = 0, n = nextExpressions.Length; i < n; i++)
                    {
                        if (!excludeFlagPerPosition[i])
                        {
                            Expression nextExpression = nextExpressions[i];
                            HandleMatchingExceptionExpressions(tokenEvent, nextExpression, includeOptional);
                        }
                    }
                    FilterAndLinkAndContinueMatchingOfExceptions(tokenEvent);
                    if (fRejectableRootCopies.Count > 0)
                    {
                        ContinueCandidatesMatching(tokenEvent, fRejectableRootCopies);
                        fRejectableRootCopies.Clear();
                    }
                    fRejectableRootCandidatesByVariation.Clear();
                }
                if (!alwaysCloneCandidateToContinueMatching)
                    ContinueInitialCandidateMatching(candidate, tokenEvent, fFirst);
            }
            return (fFirst != null);
        }

        public bool CandidateOnNextToken(CompoundCandidate candidate, TokenEvent tokenEvent,
            Expression nextExpression, bool includeOptional, bool alwaysCloneCandidateToContinueMatching)
        {
            // Чтобы правильно клонировать исходного кандидата, первое выражение должно обрабатываться последним!
            fFirst = null;
            HandleMatchingTokenExpressionsAndKeepFirst(candidate, tokenEvent, nextExpression, includeOptional,
                alwaysCloneCandidateToContinueMatching);
            if (fFirst != null)
            {
                if (fRejectableRootCandidatesByVariation.Count > 0)
                {
                    HandleMatchingExceptionExpressions(tokenEvent, nextExpression, includeOptional);
                    FilterAndLinkAndContinueMatchingOfExceptions(tokenEvent);
                    if (fRejectableRootCopies.Count > 0)
                    {
                        ContinueCandidatesMatching(tokenEvent, fRejectableRootCopies);
                        fRejectableRootCopies.Clear();
                    }
                    fRejectableRootCandidatesByVariation.Clear();
                }
                if (!alwaysCloneCandidateToContinueMatching)
                    ContinueInitialCandidateMatching(candidate, tokenEvent, fFirst);
            }
            return (fFirst != null);
        }

        // Internal

        // Связывание альтернатив и исключений осуществляется через вариации.
        // Во вложенных вариациях исключения верхнего уровня влияют на альтернативы нижнего уровня.
        // Для организации связи между альтернативами нижнего уровня и исключениями верхнего уровня
        // задействуются все вложенные вариации от токена до корня.
        private void HandleMatchingTokenExpressionsAndKeepFirst(CompoundCandidate candidate, TokenEvent tokenEvent,
            Expression nextExpression, bool includeOptional, bool alwaysCloneCandidateToContinueMatching)
        {
            if (nextExpression.OwnIndex.HasExceptionTokens(includeOptional))
            {
                nextExpression.OwnIndex.HandleMatchingTokenExpressions(tokenEvent.Token, includeOptional,
                (TokenExpression tokenExpression) =>
                {
                    bool shouldCopy = (fFirst != null) || alwaysCloneCandidateToContinueMatching;
                    if (fFirst == null)
                        fFirst = tokenExpression;
                    VariationExpression variation = tokenExpression.ParentVariationWithExceptions;
                    RootCandidate rootCandidate;
                    if (shouldCopy)
                    {
                        candidate.CloneStateToContinueMatching(tokenExpression, tokenEvent, out rootCandidate);
                        if (variation != null)
                        {
                            // Отложить корневой кандидат для связывания с исключениями.
                            fRejectableRootCopies.Add(rootCandidate);
                        }
                        else
                        {
                            // Не откладывать копию корневого кандидата - сразу продолжить её совпадение.
                            ContinueCandidateMatching(tokenEvent, rootCandidate);
                        }
                    }
                    else
                    {
                        rootCandidate = candidate.GetRootCandidate();
                    }
                    while (variation != null)
                    {
                        // Перебрать все вариации с исключениями в текущем корневом кандидате,
                        // в которые вложен токен.
                        fRejectableRootCandidatesByVariation.GetOrCreate(variation.VariationWithExceptionsId)
                            .Add(rootCandidate);
                        variation = variation.ParentVariationWithExceptions;
                    }
                });
            }
            else    // (nextExpression.OwnIndex.HasExceptionTokens == false) -- нет исключений
            {
                nextExpression.OwnIndex.HandleMatchingTokenExpressions(tokenEvent.Token, includeOptional,
                (TokenExpression tokenExpression) =>
                {
                    bool shouldCopy = (fFirst != null) || alwaysCloneCandidateToContinueMatching;
                    if (fFirst == null)
                        fFirst = tokenExpression;
                    RootCandidate rootCandidate;
                    if (shouldCopy)
                    {
                        candidate.CloneStateToContinueMatching(tokenExpression, tokenEvent, out rootCandidate);
                        ContinueCandidateMatching(tokenEvent, rootCandidate);
                    }
                });
            }
            // TODO: Обработать выражения из NestedIndex
        }

        private void HandleMatchingExceptionExpressions(TokenEvent tokenEvent, Expression nextExpression, bool includeOptional)
        {
            nextExpression.OwnIndex.HandleMatchingExceptionTokenExpressions(tokenEvent.Token, includeOptional,
            (TokenExpression exceptionTokenExpression) =>
            {
                var exceptionVariation = (VariationExpression)exceptionTokenExpression.RootExpression.ParentExpression;
                int id = exceptionVariation.VariationWithExceptionsId;
                if (fRejectableRootCandidatesByVariation.ContainsKey(id))
                {
                    // Исключение напрямую влияет на положительные альтернативы вариаций или уже созданные исключения
                    // => сразу создать кандидат исключения и добавить в структуру для связывания
                    var exceptionCandidate = (ExceptionCandidate)SearchContext.CreateRootCandidate(
                        exceptionTokenExpression, tokenEvent);
                    fExceptionCandidates.Add(exceptionCandidate);
                    VariationExpression variation = exceptionTokenExpression.ParentVariationWithExceptions;
                    // Перебрать все вариации с исключениями в текущем корневом кандидате,
                    // в которые вложен токен
                    while (variation != exceptionVariation)
                    {
                        fRejectableRootCandidatesByVariation.GetOrCreate(variation.VariationWithExceptionsId)
                            .Add(exceptionCandidate);
                        variation = variation.ParentVariationWithExceptions;
                    }
                }
                else
                {
                    // Исключение может влиять на положительные альтернативы вариаций и другие исключения косвенно
                    // => отложить выражение для фильтрации
                    fExceptionTokensToFilter.Add(exceptionTokenExpression);
                }
            });
            // TODO: Обработать выражения из NestedIndex для исключений
        }

        private void FilterAndLinkAndContinueMatchingOfExceptions(TokenEvent tokenEvent)
        {
            if (fExceptionTokensToFilter.Count > 0)
            {
                FilterAndCreateExceptions(tokenEvent);
                fExceptionTokensToFilter.Clear();
            }
            if (fExceptionCandidates.Count > 0) // кандидаты исключений были созданы
            {
                LinkExceptionsWithRootCandidates();
                ContinueCandidatesMatching(tokenEvent, fExceptionCandidates);
                fExceptionCandidates.Clear();
            }
        }

        // Создать кандидаты для исключений, которые соответствуют выбранным ранее положительным альтернативам вариаций,
        // чтобы избежать создания лишних кандидатов исключений.
        // Исключения, соответствующие другим исключениям, создаются только в том случае,
        // если другие исключения были выбраны ранее.
        private void FilterAndCreateExceptions(TokenEvent tokenEvent)
        {
            int remainedCount = fExceptionTokensToFilter.Count;
            bool done = false;
            while (!done)
            {
                done = true;
                int i = 0;
                int j = 0;
                while (i < remainedCount)
                {
                    TokenExpression exceptionTokenExpression = fExceptionTokensToFilter[i];
                    var exceptionVariation = (VariationExpression)exceptionTokenExpression.RootExpression.ParentExpression;
                    int id = exceptionVariation.VariationWithExceptionsId;
                    if (fRejectableRootCandidatesByVariation.ContainsKey(id))
                    {
                        var exceptionCandidate = (ExceptionCandidate)SearchContext.CreateRootCandidate(
                            exceptionTokenExpression, tokenEvent);
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
                            fExceptionTokensToFilter[j] = fExceptionTokensToFilter[i];
                            fExceptionTokensToFilter[i] = null;
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
                    for (int j = 0, m = relatedCandidates.Count; j < m; j++)
                    {
                        RootCandidate candidate = relatedCandidates[j];
                        RejectionTargetCandidate rejectionTarget =
                            candidate.CurrentEventObserver.GetRejectionTargetCandidate();
                        rejectionTarget.AddException(exceptionCandidate);
                        exceptionCandidate.AddTargetCandidate(rejectionTarget);
                    }
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
            tokenEvent.ClearResults();
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

        private void ContinueInitialCandidateMatching(CompoundCandidate candidate, TokenEvent tokenEvent,
            TokenExpression firstExpression)
        {
            Candidate firstTokenCandidate = firstExpression.CreateCandidate(SearchContext, targetParentCandidate: candidate);
            var tokenExpression = (TokenExpression)firstTokenCandidate.Expression;
            HavingExpression conditionalHavingExpression = tokenExpression.ParentConditionalHaving;
            tokenEvent.ClearResults();
            firstTokenCandidate.OnNext(tokenEvent);
            while (conditionalHavingExpression != null)
            {
                int patternId = conditionalHavingExpression.InnerContent.ReferencedPattern.Id;
                SearchContext.EnableConditionalHavingPattern(patternId, isRootIndex: false);
                conditionalHavingExpression = conditionalHavingExpression.ParentConditionalHaving;
            }
        }
    }
}
