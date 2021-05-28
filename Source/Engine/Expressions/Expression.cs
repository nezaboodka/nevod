//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Nezaboodka.Nevod
{
    internal abstract class Expression
    {
        private ExpressionIndex fOwnIndex;
        private ExpressionIndex fNestedIndex;

        public Syntax Syntax { get; }
        public CompoundExpression ParentExpression { get; private set; }
        public int PositionInParentExpression { get; private set; }
        public bool IsOptional { get; protected set; }
        public bool IsOwnIndexCreated { get; private set; }
        public bool IsNestedIndexCreated { get; private set; }

        public ExpressionIndex OwnIndex
        {
            get
            {
                if (!IsOwnIndexCreated)
                    throw new InvalidOperationException($"{nameof(OwnIndex)} not initialized");
                return fOwnIndex;
            }
            set
            {
                fOwnIndex = value;
                IsOwnIndexCreated = true;
            }
        }

        public ExpressionIndex NestedIndex
        {
            get
            {
                if (!IsNestedIndexCreated)
                    throw new InvalidOperationException($"{nameof(NestedIndex)} not initialized");
                return fNestedIndex;
            }
            set
            {
                fNestedIndex = value;
                IsNestedIndexCreated = true;
            }
        }

        public void SetParentExpression(CompoundExpression parentExpression, int positionInParentExpression)
        {
            ParentExpression = parentExpression;
            PositionInParentExpression = positionInParentExpression;
        }

        public RootExpression GetRootExpression()
        {
            Expression current = ParentExpression;
            if (current != null)
            {
                while (!(current is RootExpression))
                    current = ParentExpression;
            }
            return (RootExpression)current;
        }

        public Candidate CreateCandidate(SearchContext searchContext, CompoundCandidate targetParentCandidate)
        {
            Candidate result = CreateCandidate(searchContext.CandidateFactory);
            result.SearchContext = searchContext;
            result.TargetParentCandidate = targetParentCandidate;
            return result;
        }

        public override string ToString()
        {
            return Syntax?.ToString();
        }

        // Internal

        protected Expression(Syntax syntax)
        {
            Syntax = syntax;
        }

        protected abstract Candidate CreateCandidate(CandidateFactory candidateFactory);

        protected internal abstract void Accept(ExpressionVisitor visitor);
    }
}
