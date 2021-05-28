//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Nezaboodka.Nevod
{
    internal sealed class HavingExpression : CompoundExpression
    {
        public Expression Body { get; }
        public PatternReferenceExpression InnerContent { get; }
        public HavingExpression ParentConditionalHaving { get; set; }

        public HavingExpression(Syntax syntax, Expression body, PatternReferenceExpression innerContent)
            : base(syntax)
        {
            Body = body;
            Body.SetParentExpression(this, BodyPosition);
            InnerContent = innerContent;
            InnerContent.SetParentExpression(this, InnerContentPosition);
            RefreshIsOptional();
        }

        public override void RefreshIsOptional()
        {
            IsOptional = Body.IsOptional;
            if (IsOptional && ParentExpression != null)
                ParentExpression.RefreshIsOptional();
        }

        public const int BodyPosition = 0;
        public const int InnerContentPosition = 1;

        // Internal

        protected override Candidate CreateCandidate(CandidateFactory candidateFactory)
        {
            HavingCandidate result = candidateFactory.CreateHavingCandidate(this);
            return result;
        }

        protected internal override void Accept(ExpressionVisitor visitor)
        {
            visitor.VisitHaving(this);
        }
    }
}
