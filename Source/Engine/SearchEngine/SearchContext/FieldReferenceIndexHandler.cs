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
    // Выражение для совпадения ссылки на поле - простая цепочка токенов
    internal class FieldReferenceIndexHandler
    {
        public SearchContext SearchContext { get; }

        public FieldReferenceIndexHandler(SearchContext searchContext)
        {
            SearchContext = searchContext;
        }

        public bool CandidateOnFirstToken(CompoundCandidate candidate, TokenEvent tokenEvent,
            Expression nextExpression)
        {
            TokenExpression firstTokenExpression = null;
            nextExpression.OwnIndex.HandleMatchingTokenExpressions(tokenEvent.Token, includeOptional: false,
            (TokenExpression tokenExpression) =>
            {
                firstTokenExpression = tokenExpression;
            });
            if (firstTokenExpression != null)
                ContinueInitialCandidateMatching(candidate, tokenEvent, firstTokenExpression);
            return (firstTokenExpression != null);
        }

        // Internal

        private void ContinueInitialCandidateMatching(CompoundCandidate candidate, TokenEvent tokenEvent,
            TokenExpression firstExpression)
        {
            Candidate firstTokenCandidate =
                firstExpression.CreateCandidate(SearchContext, targetParentCandidate: candidate);
            firstTokenCandidate.OnNext(tokenEvent);
        }
    }
}
