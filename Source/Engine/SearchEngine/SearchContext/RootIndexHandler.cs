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
    internal class RootIndexHandler
    {
        private HashSet<TokenExpression> fTokenExpressionsForPatternCandidates;
        private HashSet<TokenExpression> fTokenExpressionsForExceptionCandidates;

        private List<RootCandidate> fRejectablePatternCandidates;
        private Dictionary<int, List<RootCandidate>> fRejectableRootCandidatesByVariation;
        private List<ExceptionCandidate> fExceptionCandidates;

        public SearchContext SearchContext { get; }

        public RootIndexHandler(SearchContext searchContext)
        {
            SearchContext = searchContext;

            fTokenExpressionsForPatternCandidates = new HashSet<TokenExpression>();
            fTokenExpressionsForExceptionCandidates = new HashSet<TokenExpression>();

            fRejectablePatternCandidates = new List<RootCandidate>();
            fRejectableRootCandidatesByVariation = new Dictionary<int, List<RootCandidate>>();
            fExceptionCandidates = new List<ExceptionCandidate>();
        }

        // Связывание альтернатив и исключений осуществляется через вариации.
        // Во вложенных вариациях исключения верхнего уровня влияют на альтернативы нижнего уровня.
        // Для организации связи между альтернативами нижнего уровня и исключениями верхнего уровня
        // задействуются все вложенные вариации от токена до корня.
        public void CreateNewRootCandidatesOnToken(ExpressionIndex rootIndex, TokenEvent tokenEvent,
            bool[] excludeFlagPerPattern)
        {
            rootIndex.SelectMatchingTokenExpressions(tokenEvent.Token, includeOptional: true,
                excludeFlagPerPattern, fTokenExpressionsForPatternCandidates);
            if (fTokenExpressionsForPatternCandidates.Count > 0)
            {
                rootIndex.SelectMatchingExceptionTokenExpressions(tokenEvent.Token, includeOptional: true,
                    excludeFlagPerPattern, fTokenExpressionsForExceptionCandidates);
                if (fTokenExpressionsForExceptionCandidates.Count > 0)
                {
                    foreach (var tokenExpression in fTokenExpressionsForPatternCandidates)
                    {
                        RootCandidate rootCandidate = SearchContext.CreateRootCandidate(tokenExpression, tokenEvent);
                        VariationExpression variation = tokenExpression.ParentVariationWithExceptions;
                        if (variation != null)
                        {
                            fRejectablePatternCandidates.Add(rootCandidate);
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
                        ContinueCandidatesMatching(tokenEvent, fRejectablePatternCandidates);
                        fRejectablePatternCandidates.Clear();
                        fRejectableRootCandidatesByVariation.Clear();
                    }
                    fTokenExpressionsForExceptionCandidates.Clear();
                }
                else    // нет исключений (fTokenExpressionsForNewPatternCandidates.Count == 0)
                {
                    foreach (var tokenExpression in fTokenExpressionsForPatternCandidates)
                    {
                        RootCandidate rootCandidate = SearchContext.CreateRootCandidate(tokenExpression, tokenEvent);
                        ContinueCandidateMatching(tokenEvent, rootCandidate);
                    }
                }
                fTokenExpressionsForPatternCandidates.Clear();
            }
        }

        // Internal

        // Создать кандидаты для исключений, которые соответствуют выбранным ранее положительным альтернативам вариаций,
        // чтобы избежать создания лишних кандидатов исключений.
        // Исключения, соответствующие другим исключениям, создаются только в том случае, если
        // другие исключения были выбраны ранее.
        private void FilterAndCreateExceptions(TokenEvent tokenEvent)
        {
            TokenExpression[] exceptionTokensToFilter = fTokenExpressionsForExceptionCandidates.ToArray();
            int remainedCount = exceptionTokensToFilter.Length;
            bool done = false;
            while (!done)
            {
                done = true;
                int i = 0;
                int j = 0;
                while (i < remainedCount)
                {
                    TokenExpression exceptionTokenExpression = exceptionTokensToFilter[i];
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
                    for (int j = 0, m = relatedCandidates.Count; j < m; j++)
                    {
                        RootCandidate candidate = relatedCandidates[j];
                        candidate.AddException(exceptionCandidate);
                        exceptionCandidate.AddTargetCandidate(candidate);
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
                SearchContext.EnableConditionalHavingPattern(patternId, isRootIndex: true);
                conditionalHavingExpression = conditionalHavingExpression.ParentConditionalHaving;
            }
        }
    }
}
