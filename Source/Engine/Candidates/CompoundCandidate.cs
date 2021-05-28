//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal abstract class CompoundCandidate : Candidate
    {
        public CompoundCandidate(CompoundExpression expression)
            : base(expression)
        {
        }

        public bool IsExpectedParentOf(Candidate candidate)
        {
            return ReferenceEquals(Expression, candidate.Expression.ParentExpression);
        }

        public virtual CompoundCandidate Clone()
        {
            throw new InvalidOperationException();
        }

        public CompoundCandidate CloneState(out RootCandidate rootCandidate, in RootCandidate replacingRoot = null)
        {
            CompoundCandidate result;
            if (TargetParentCandidate != null)    // это не корневой кандидат
            {
                CompoundCandidate copy = this.Clone();
                result = copy;
                // Скопировать цепочку кандидатов, исключая корень.
                while (copy.TargetParentCandidate.TargetParentCandidate != null)
                {
                    // Клон кандидата сочетания должен отменяться при невозможности продолжить совпадение,
                    // поэтому он не должен обрабатывать отмену.
                    // Такая отмена обрабатывается только в исходном кандидате сочетания.
                    copy.CurrentEventObserver = null;
                    copy.TargetParentCandidate = copy.TargetParentCandidate.Clone();
                    copy = copy.TargetParentCandidate;
                }
                copy.CurrentEventObserver = null;
                // Скопировать или заменить корень, если необходимо.
                if (replacingRoot == null)
                    rootCandidate = (RootCandidate)copy.TargetParentCandidate.Clone();
                else
                    rootCandidate = replacingRoot;
                copy.TargetParentCandidate = rootCandidate;
                rootCandidate.CurrentEventObserver = result;
            }
            else    // (TargetParentCandidate == null)    // это корневой кандидат
            {
                throw new InvalidOperationException();
            }
            return result;
        }

        public virtual CompoundCandidate CloneStateToContinueMatching(TokenExpression tokenExpression,
            TokenEvent tokenEvent, out RootCandidate rootCopy, in RootCandidate replacingRoot = null)
        {
            CompoundCandidate thisCopy = this.CloneState(out rootCopy, in replacingRoot);
            Candidate tokenCandidate = tokenExpression.CreateCandidate(SearchContext, targetParentCandidate: thisCopy);
            rootCopy.CurrentEventObserver = tokenCandidate;
            return thisCopy;
        }

        public abstract void OnElementMatch(Candidate element, MatchingEvent matchingEvent);

        // Internal

        protected bool OnNextToken(TokenEvent tokenEvent, Expression[] nextExpressions,
            bool[] excludeFlagPerPosition, bool includeOptional,
            bool alwaysCloneCandidateToContinueMatching)
        {
            return SearchContext.CandidateOnNextToken(candidate: this, tokenEvent, nextExpressions,
                excludeFlagPerPosition, includeOptional, alwaysCloneCandidateToContinueMatching);
        }

        protected bool OnNextToken(TokenEvent tokenEvent, Expression nextExpression,
            bool includeOptional, bool alwaysCloneCandidateToContinueMatching)
        {
            return SearchContext.CandidateOnNextToken(candidate: this, tokenEvent, nextExpression,
                includeOptional, alwaysCloneCandidateToContinueMatching);
        }
    }
}
